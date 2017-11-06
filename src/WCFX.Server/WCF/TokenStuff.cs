using Microsoft.IdentityModel.Protocols;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Configuration;
using System.IdentityModel.Selectors;
using System.IdentityModel.Tokens;
using System.Linq;
using System.Security.Claims;
using System.Threading;

namespace WCFX.Server.WCF
{
	class RequireAuthenticationAuthorization : ClaimsAuthorizationManager
	{
		public override bool CheckAccess(AuthorizationContext context)
		{
			return context.Principal.Identity.IsAuthenticated;
		}
	}

	class JwtValidator : Saml2SecurityTokenHandler
	{
		static string _tenant = ConfigurationManager.AppSettings["ida:Tenant"];
		static string _audience = ConfigurationManager.AppSettings["ida:Audience"];
		static string _authority = $"https://login.microsoftonline.com/{_tenant}";
		static string _issuer = string.Empty;
		static List<SecurityToken> _signingTokens = null;
		static DateTime _stsMetadataRetrievalTime = DateTime.MinValue;
		
		public override ReadOnlyCollection<ClaimsIdentity> ValidateToken(SecurityToken token)
		{
			var saml = token as Saml2SecurityToken;
			var samlAttributeStatement = saml.Assertion.Statements.OfType<Saml2AttributeStatement>().FirstOrDefault();
			var jwt = samlAttributeStatement.Attributes.Where(sa => sa.Name.Equals("jwt", StringComparison.OrdinalIgnoreCase)).SingleOrDefault().Values.Single();

			var principal = ValidateJwt(jwt);
			Thread.CurrentPrincipal = principal;
			
			return new ReadOnlyCollection<ClaimsIdentity>(new List<ClaimsIdentity> { principal.Identities.First() });
		}

		ClaimsPrincipal ValidateJwt(string jwt)
		{
			try
			{
				if (DateTime.UtcNow.Subtract(_stsMetadataRetrievalTime).TotalHours > 24// The issuer and signingTokens are cached for 24 hours.
					|| string.IsNullOrEmpty(_issuer)
					|| _signingTokens == null)
				{
					var configManager = new ConfigurationManager<OpenIdConnectConfiguration>($"{_authority}/.well-known/openid-configuration");
					var config = configManager.GetConfigurationAsync().Result;
					_issuer = config.Issuer;
					_signingTokens = config.SigningTokens.ToList();

					_stsMetadataRetrievalTime = DateTime.UtcNow;
				}
				
				var claimsPrincipal = new JwtSecurityTokenHandler().ValidateToken(jwt, new TokenValidationParameters
				{
					ValidAudience = _audience,
					ValidIssuer = _issuer,
					IssuerSigningTokens = _signingTokens,
					CertificateValidator = X509CertificateValidator.None
				}, out SecurityToken validatedToken);

				if (!claimsPrincipal.HasClaim("http://schemas.microsoft.com/identity/claims/scope", "User.Read"))
				{
					throw new SecurityTokenValidationException("Insufficient Scope");
				}

				return claimsPrincipal;
			}
			catch (Exception)
			{
				throw;
			}
		}
	}
}
