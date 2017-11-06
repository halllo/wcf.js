using System;
using System.IdentityModel.Tokens;
using System.IO;
using System.Security.Claims;
using System.Text;
using System.Xml;
using System.Xml.Linq;

namespace WCFX.DesktopClient
{
	public static class TokenStuff
	{
		public static GenericXmlSecurityToken WrapJwt(string jwt)
		{
			var subject = new ClaimsIdentity("saml");
			subject.AddClaim(new Claim("jwt", jwt));

			var descriptor = new SecurityTokenDescriptor
			{
				TokenType = TokenTypes.Saml2TokenProfile11,
				TokenIssuerName = "urn:wrappedjwt",
				Subject = subject
			};

			var handler = new Saml2SecurityTokenHandler();
			var token = handler.CreateToken(descriptor);

			var xmlToken = new GenericXmlSecurityToken(
				XElement.Parse(token.ToTokenXmlString()).ToXmlElement(),
				null,
				DateTime.Now,
				DateTime.Now.AddHours(1),
				null,
				null,
				null);

			return xmlToken;
		}
	}

	public static class TokenTypes
	{
		public const string Kerberos = "http://schemas.microsoft.com/ws/2006/05/identitymodel/tokens/Kerberos";
		public const string OasisWssSaml11TokenProfile11 = "http://docs.oasis-open.org/wss/oasis-wss-saml-token-profile-1.1#SAMLV1.1";
		public const string OasisWssSaml2TokenProfile11 = "http://docs.oasis-open.org/wss/oasis-wss-saml-token-profile-1.1#SAMLV2.0";
		public const string Rsa = "http://schemas.microsoft.com/ws/2006/05/identitymodel/tokens/Rsa";
		public const string Saml11TokenProfile11 = "urn:oasis:names:tc:SAML:1.0:assertion";
		public const string Saml2TokenProfile11 = "urn:oasis:names:tc:SAML:2.0:assertion";
		public const string UserName = "http://schemas.microsoft.com/ws/2006/05/identitymodel/tokens/UserName";
		public const string X509Certificate = "http://schemas.microsoft.com/ws/2006/05/identitymodel/tokens/X509Certificate";
		public const string SimpleWebToken = "http://schemas.xmlsoap.org/ws/2009/11/swt-token-profile-1.0";
		public const string JsonWebToken = "urn:ietf:params:oauth:token-type:jwt";
	}

	public static class TokenExtensions
	{
		/// <summary>
		/// Retrieves the XML from a GenericXmlSecurityToken
		/// </summary>
		/// <param name="token">The token.</param>
		/// <returns>The token XML string.</returns>
		public static string ToTokenXmlString(this GenericXmlSecurityToken token)
		{
			return token.TokenXml.OuterXml;
		}
		
		/// <summary>
		/// Converts a supported token to an XML string.
		/// </summary>
		/// <param name="token">The token.</param>
		/// <returns>The token XML string.</returns>
		public static string ToTokenXmlString(this SecurityToken token)
		{
			var genericToken = token as GenericXmlSecurityToken;
			if (genericToken != null)
			{
				return genericToken.ToTokenXmlString();
			}

			var handler = SecurityTokenHandlerCollection.CreateDefaultSecurityTokenHandlerCollection();
			return token.ToTokenXmlString(handler);
		}

		/// <summary>
		/// Converts a supported token to an XML string.
		/// </summary>
		/// <param name="token">The token.</param>
		/// <param name="handler">The token handler.</param>
		/// <returns>The token XML string.</returns>
		public static string ToTokenXmlString(this SecurityToken token, SecurityTokenHandlerCollection handler)
		{
			if (handler.CanWriteToken(token))
			{
				var sb = new StringBuilder(128);
				handler.WriteToken(new XmlTextWriter(new StringWriter(sb)), token);
				return sb.ToString();
			}
			else
			{
				throw new InvalidOperationException("Token type not suppoted");
			}
		}

		/// <summary>
		/// Converts a XElement to a XmlElement.
		/// </summary>
		/// <param name="element">The XElement.</param>
		/// <returns>A XmlElement</returns>
		public static XmlElement ToXmlElement(this XElement element)
		{
			return new XmlConverter(element).CreateXmlElement();
		}
	}

	public class XmlConverter
	{
		private readonly StringBuilder _xmlTextBuilder;
		private readonly XmlWriter _writer;

		private XmlConverter()
		{
			_xmlTextBuilder = new StringBuilder();

			_writer = new XmlTextWriter(new StringWriter(_xmlTextBuilder))
			{
				Formatting = Formatting.Indented,
				Indentation = 2
			};
		}

		public XmlConverter(XNode e)
			: this()
		{
			e.WriteTo(_writer);
		}

		public XmlConverter(System.Xml.XmlNode e)
			: this()
		{
			e.WriteTo(_writer);
		}

		public XElement CreateXElement()
		{
			return XElement.Load(new StringReader(_xmlTextBuilder.ToString()));
		}

		public XDocument CreateXDocument()
		{
			return XDocument.Load(new StringReader(_xmlTextBuilder.ToString()));
		}

		public XmlElement CreateXmlElement()
		{
			return CreateXmlDocument().DocumentElement;
		}

		public XmlDocument CreateXmlDocument()
		{
			var doc = new XmlDocument();
			doc.Load(new XmlTextReader(new StringReader(_xmlTextBuilder.ToString())));
			return doc;
		}
	}
}
