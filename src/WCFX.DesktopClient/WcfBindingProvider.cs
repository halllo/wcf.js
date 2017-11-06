using System;
using System.ServiceModel;

namespace WCFX.DesktopClient
{
	public static class WcfBindingProvider
	{
		public static NetTcpBinding NetTcpBinding(long maxReceivedMessageSize)
		{
			var binding = new NetTcpBinding();
			binding.MaxReceivedMessageSize = maxReceivedMessageSize;
			binding.MaxBufferSize = (int)Math.Min(Int32.MaxValue, maxReceivedMessageSize);
			binding.ReaderQuotas.MaxStringContentLength = int.MaxValue;
			binding.Security.Mode = SecurityMode.TransportWithMessageCredential;
			binding.Security.Message.ClientCredentialType = MessageCredentialType.IssuedToken;
			
			return binding;
		}

		public static WS2007FederationHttpBinding WS2007FederationHttpBinding(long maxReceivedMessageSize)
		{
			var binding = new WS2007FederationHttpBinding();
			binding.MaxReceivedMessageSize = maxReceivedMessageSize;
			binding.ReaderQuotas.MaxStringContentLength = int.MaxValue;
			binding.HostNameComparisonMode = HostNameComparisonMode.Exact;
			binding.Security.Mode = WSFederationHttpSecurityMode.TransportWithMessageCredential;
			binding.Security.Message.EstablishSecurityContext = false;
			binding.Security.Message.IssuedKeyType = System.IdentityModel.Tokens.SecurityKeyType.BearerKey;

			return binding;
		}

		public static WSHttpBinding WsHttpBinding_WithWindowsAuthentication(long maxReceivedMessageSize, bool isMtomEnabled = false)
		{
			var wsHttpBinding = new WSHttpBinding();
			wsHttpBinding.MaxReceivedMessageSize = maxReceivedMessageSize;
			wsHttpBinding.ReaderQuotas.MaxStringContentLength = int.MaxValue;
			wsHttpBinding.ReaderQuotas.MaxArrayLength = int.MaxValue;
			wsHttpBinding.ReaderQuotas.MaxDepth = int.MaxValue;
			wsHttpBinding.ReaderQuotas.MaxBytesPerRead = int.MaxValue;
			wsHttpBinding.ReaderQuotas.MaxNameTableCharCount = int.MaxValue;
			wsHttpBinding.MessageEncoding = isMtomEnabled ? WSMessageEncoding.Mtom : WSMessageEncoding.Text;
			wsHttpBinding.Security.Mode = SecurityMode.Transport;
			wsHttpBinding.Security.Transport.ClientCredentialType = HttpClientCredentialType.Windows;

			return wsHttpBinding;
		}
	}
}
