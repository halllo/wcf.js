using System;
using System.ServiceModel;

namespace WCFX.Server.WCF
{
	public static class WcfBindingProvider
	{
		public static NetTcpBinding GetNetTcpBinding(long maxReceivedMessageSize)
		{
			var binding = new NetTcpBinding();
			binding.MaxReceivedMessageSize = maxReceivedMessageSize;
			binding.MaxBufferSize = (int)Math.Min(Int32.MaxValue, maxReceivedMessageSize);
			binding.ReaderQuotas.MaxStringContentLength = int.MaxValue;

			return binding;
		}

		public static WSHttpBinding GetWsHttpBinding(long maxReceivedMessageSize, bool isMtomEnabled, bool useWindowsAuthentication = false)
		{
			var wsHttpBinding = new WSHttpBinding();
			wsHttpBinding.MaxReceivedMessageSize = maxReceivedMessageSize;
			wsHttpBinding.ReaderQuotas.MaxStringContentLength = int.MaxValue;
			wsHttpBinding.ReaderQuotas.MaxArrayLength = int.MaxValue;
			wsHttpBinding.ReaderQuotas.MaxDepth = int.MaxValue;
			wsHttpBinding.ReaderQuotas.MaxBytesPerRead = int.MaxValue;
			wsHttpBinding.ReaderQuotas.MaxNameTableCharCount = int.MaxValue;
			wsHttpBinding.MessageEncoding = isMtomEnabled ? WSMessageEncoding.Mtom : WSMessageEncoding.Text;

			if (useWindowsAuthentication == false)
			{
				wsHttpBinding.Security.Mode = SecurityMode.None;
			}

			return wsHttpBinding;
		}

		public static WSHttpBinding GetSecuredWsHttpBindingWithWindowsAuthentication(long maxReceivedMessageSize, bool isMtomEnabled)
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
