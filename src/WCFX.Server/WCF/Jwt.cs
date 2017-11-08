using Microsoft.IdentityModel.Protocols;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Configuration;
using System.IdentityModel.Selectors;
using System.IdentityModel.Tokens;
using System.Linq;
using System.Security.Claims;
using System.ServiceModel;

namespace WCFX.Server.WCF
{
	public static class Jwt
	{
		static string _tenant = ConfigurationManager.AppSettings["ida:Tenant"];
		static string _audience = ConfigurationManager.AppSettings["ida:Audience"];
		static string _authority = $"https://login.microsoftonline.com/{_tenant}";
		static string _issuer = string.Empty;
		static List<SecurityToken> _signingTokens = null;
		static DateTime _stsMetadataRetrievalTime = DateTime.MinValue;
		
		public static ClaimsPrincipal Validate(string jwt)
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

				Username = claimsPrincipal.Identity.Name;

				return claimsPrincipal;
			}
			catch (Exception)
			{
				throw;
			}
		}


		[ThreadStatic]
		private static string Username = null;
		public static string CurrentUser
		{
			get
			{
				var jwtInUsername = OperationContext.Current.ServiceSecurityContext.PrimaryIdentity.Name;
				if (!string.IsNullOrWhiteSpace(jwtInUsername))
				{
					var token = new JwtSecurityToken(jwtInUsername);
					return token.Claims.FirstOrDefault(c => c.Type == "unique_name")?.Value;
				}
				else if (!string.IsNullOrWhiteSpace(Username))
				{
					return Username;
				}
				else
				{
					return null;
				}
			}
			set
			{
				Username = value;
			}
		}
	}





	public class SamlJwtValidator : Saml2SecurityTokenHandler
	{
		public override ReadOnlyCollection<ClaimsIdentity> ValidateToken(SecurityToken token)
		{
			var saml = token as Saml2SecurityToken;
			var samlAttributeStatement = saml.Assertion.Statements.OfType<Saml2AttributeStatement>().FirstOrDefault();
			var jwt = samlAttributeStatement.Attributes.Where(sa => sa.Name.Equals("jwt", StringComparison.OrdinalIgnoreCase)).SingleOrDefault().Values.Single();

			var principal = Jwt.Validate(jwt);

			return new ReadOnlyCollection<ClaimsIdentity>(new List<ClaimsIdentity> { principal.Identities.First() });
		}
	}
		
	public class CustomUsernameJwtValidator : UserNamePasswordValidator
	{
		public override void Validate(string userName, string password)
		{
			Jwt.Validate(userName);
		}
	}



	




	public class RequireAuthenticationAuthorization : ClaimsAuthorizationManager
	{
		public override bool CheckAccess(AuthorizationContext context)
		{
			return context.Principal.Identity.IsAuthenticated;
		}
	}
}
