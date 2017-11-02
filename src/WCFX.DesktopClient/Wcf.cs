using System;
using System.Configuration;
using System.ServiceModel;
using System.ServiceModel.Description;
using WCFX.Common;

namespace WCFX.DesktopClient
{
	public class WcfServiceProvider
	{
		public WcfServiceProvider()
		{
			mServiceCache.OnRemove += OnRemoveService;
		}

		public TResult Execute<TService, TResult>(Func<TService, TResult> func, int operationTimeout = 90) where TService : class, IWcfService
		{
			var service = GetService<TService>(operationTimeout);
			try
			{
				var result = func(service);
				return result;
			}
			catch (Exception)
			{
				Abort(service);
				throw;
			}
		}

		public void Execute<TService>(Action<TService> action, int operationTimeout = 90) where TService : class, IWcfService
		{
			var service = GetService<TService>(operationTimeout);
			try
			{
				action(service);
			}
			catch (Exception)
			{
				Abort(service);
				throw;
			}
		}

		public TService CreateService<TService>(int operationTimeout = 90) where TService : class, IWcfService
		{
			var channelFactory = GetChannelFactory<TService>();
			//var windowsClientCredential = channelFactory.Credentials.Windows;

			//windowsClientCredential.AllowedImpersonationLevel = System.Security.Principal.TokenImpersonationLevel.Impersonation;
			//windowsClientCredential.ClientCredential = System.Net.CredentialCache.DefaultNetworkCredentials;

			var service = channelFactory.CreateChannel();

			SetMaxItemsInObjectGraph(channelFactory);
			if (operationTimeout > 0)
			{
				((IClientChannel)service).OperationTimeout = TimeSpan.FromSeconds(operationTimeout);
			}

			return service;
		}

		private void OnRemoveService(object obj)
		{
			var channel = obj as IClientChannel;
			if (channel == null)
			{
				return;
			}
			channel.Abort();
			channel.Close();
			channel.Faulted -= WcfServiceProvider_Faulted;
		}

		private TService GetService<TService>(int operationTimeout = 90) where TService : class, IWcfService
		{
			lock (mLock)
			{
				var serviceTypeName = typeof(TService).FullName;
				var service = (mServiceCache.ContainsKey(serviceTypeName))
					? mServiceCache.Get<TService>(serviceTypeName) as TService
					: null;
				if (service != null)
				{
					var channel = service as IClientChannel;
					if (channel.State == CommunicationState.Faulted || channel.State == CommunicationState.Closed
						|| channel.State == CommunicationState.Closing)
					{
						mServiceCache.Remove(serviceTypeName);
					}
				}

				if (service == null)
				{
					service = CreateService<TService>(operationTimeout);
					((IClientChannel)service).Faulted += WcfServiceProvider_Faulted;
					mServiceCache.Add(serviceTypeName, service);
				}

				return service;
			}
		}

		private void WcfServiceProvider_Faulted(object sender, EventArgs e)
		{
			var service = (ICommunicationObject)sender;
			lock (mLock)
			{
				mServiceCache.Remove(service.GetType().FullName);
			}
		}

		private void Abort(IWcfService service)
		{
			var channel = service as IClientChannel;
			if (channel == null)
			{
				return;
			}

			lock (mLock)
			{
				mServiceCache.Remove(service.GetType().FullName);
			}
		}

		private ChannelFactory<T> GetChannelFactory<T>()
		{
			var serverAddress = ConfigurationManager.AppSettings["ServerAddress"];
			var serverPort = ConfigurationManager.AppSettings["ServerPort"];
			var maxReceivedMessageSize = long.Parse(ConfigurationManager.AppSettings["MaxReceivedMessageSize"]);

			return new ChannelFactory<T>(
				binding: WcfBindingProvider.GetNetTcpBinding(maxReceivedMessageSize),
				remoteAddress: $"net.tcp://{serverAddress}:{serverPort}/WCFX/{typeof(T).FullName}");
		}

		private void SetMaxItemsInObjectGraph(ChannelFactory channelFactory)
		{
			foreach (var operation in channelFactory.Endpoint.Contract.Operations)
			{
				var dataContractBehavior = operation.Behaviors[typeof(DataContractSerializerOperationBehavior)] as DataContractSerializerOperationBehavior;
				if (dataContractBehavior == null)
				{
					continue;
				}
				dataContractBehavior.MaxItemsInObjectGraph = int.MaxValue;
			}
		}

		private static readonly WeakCache mServiceCache = new WeakCache(new FixedLifetimeAfterAccessStrategy(240));
		private static readonly object mLock = new object();
	}







































	public static class WcfBindingProvider
	{
		public static NetTcpBinding GetNetTcpBinding(long maxReceivedMessageSize)
		{
			var binding = new NetTcpBinding();
			binding.MaxReceivedMessageSize = maxReceivedMessageSize;
			binding.MaxBufferSize = (int)Math.Min(Int32.MaxValue, maxReceivedMessageSize);
			binding.ReaderQuotas.MaxStringContentLength = int.MaxValue;
			binding.Security.Mode = SecurityMode.Transport;
			binding.Security.Transport.ClientCredentialType = TcpClientCredentialType.Windows;

			return binding;
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
