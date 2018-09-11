using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Security.Claims;
using Microsoft.IdentityModel.Protocols;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;

namespace WCFX.Server.wcf
{
	public static class Jwt
	{
		static string _tenant = ConfigurationManager.AppSettings["ida:Tenant"];
		static string _audience = ConfigurationManager.AppSettings["ida:Audience"];
		static string _authority = $"https://login.microsoftonline.com/{_tenant}";
		static string _issuer = string.Empty;
		static List<SecurityKey> _signingKeys = null;
		static DateTime _stsMetadataRetrievalTime = DateTime.MinValue;

		public static ClaimsPrincipal Validate(string jwt)
		{
			try
			{
				CacheOpenIdConnectConfig();
				var claimsPrincipal = ValidateToken(jwt);

				if (!claimsPrincipal.HasClaim("http://schemas.microsoft.com/identity/claims/scope", "user_impersonation"))
				{
					throw new SecurityTokenValidationException("Insufficient Scope");
				}

				return claimsPrincipal;
			}
			catch (Exception e)
			{
				Logger.Log("JWT validation failure: " + e.Message, ConsoleColor.Red);
				throw;
			}
		}

		private static ClaimsPrincipal ValidateToken(string jwt)
		{
			return new System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler().ValidateToken(jwt, new TokenValidationParameters
			{
				ValidAudience = _audience,
				ValidIssuer = _issuer,
				IssuerSigningKeys = _signingKeys,
			}, out SecurityToken validatedToken);
		}

		private static void CacheOpenIdConnectConfig()
		{
			if (DateTime.UtcNow.Subtract(_stsMetadataRetrievalTime).TotalHours > 24// The issuer and signingTokens are cached for 24 hours.
				|| string.IsNullOrEmpty(_issuer)
				|| _signingKeys == null)
			{
				var configManager = new ConfigurationManager<OpenIdConnectConfiguration>($"{_authority}/.well-known/openid-configuration", new OpenIdConnectConfigurationRetriever());
				var config = configManager.GetConfigurationAsync().Result;

				_issuer = config.Issuer;
				_signingKeys = config.SigningKeys.ToList();
				_stsMetadataRetrievalTime = DateTime.UtcNow;
			}
		}
	}
}