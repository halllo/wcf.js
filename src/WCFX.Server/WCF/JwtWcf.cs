using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IdentityModel.Selectors;
using System.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.ServiceModel;

namespace WCFX.Server.wcf
{
	public class SamlJwtValidator : Saml2SecurityTokenHandler
	{
		[ThreadStatic]
		public static string Username = null;

		public override ReadOnlyCollection<ClaimsIdentity> ValidateToken(SecurityToken token)
		{
			var saml = token as Saml2SecurityToken;
			var samlAttributeStatement = saml.Assertion.Statements.OfType<Saml2AttributeStatement>().FirstOrDefault();
			var jwt = samlAttributeStatement.Attributes.Where(sa => sa.Name.Equals("jwt", StringComparison.OrdinalIgnoreCase)).SingleOrDefault().Values.Single();

			var principal = Jwt.Validate(jwt);
			Username = principal.Identity.Name;

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

	public static class JwtCurrentUsername
	{
		public static string FromToken
		{
			get
			{
				var jwtInUsername = OperationContext.Current.ServiceSecurityContext.PrimaryIdentity.Name;
				if (!string.IsNullOrWhiteSpace(jwtInUsername))
				{
					var token = new JwtSecurityToken(jwtInUsername);
					return token.Claims.FirstOrDefault(c => c.Type == "unique_name")?.Value;
				}
				else if (!string.IsNullOrWhiteSpace(SamlJwtValidator.Username))
				{
					return SamlJwtValidator.Username;
				}
				else
				{
					return null;
				}
			}
			set
			{
				SamlJwtValidator.Username = value;
			}
		}
	}
}