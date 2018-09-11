//#define NETTCP
#define WS2007HTTP

using System;
using System.Collections.Generic;
using System.Configuration;
using System.ServiceModel;
using System.ServiceModel.Description;
using WCFX.Common;

namespace WCFX.DesktopClient
{
	public class WcfClient
	{
		public TResult Execute<TService, TResult>(Func<TService, TResult> func, int operationTimeout = 90) where TService : class, IWcfService
		{
			var service = CreateService<TService>(operationTimeout);
			try
			{
				var result = func(service);
				return result;
			}
			catch (Exception)
			{
				throw;
			}
		}

		public void Execute<TService>(Action<TService> action, int operationTimeout = 90) where TService : class, IWcfService
		{
			var service = CreateService<TService>(operationTimeout);
			try
			{
				action(service);
			}
			catch (Exception)
			{
				throw;
			}
		}

























		public string Token = null;

		private TService CreateService<TService>(int operationTimeout = 90) where TService : class, IWcfService
		{
			if (string.IsNullOrWhiteSpace(Token)) throw new ArgumentNullException(nameof(Token));

#if NETTCP
			var channelFactory = GetChannelFactory<TService>(Token);
			var service = channelFactory.CreateChannel();
#endif
#if WS2007HTTP
			var channelFactory = GetChannelFactory<TService>();
			var service = channelFactory.CreateChannelWithIssuedToken(TokenStuff.WrapJwt(Token));
#endif
			{//SetMaxItemsInObjectGraph
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

			if (operationTimeout > 0)
			{
				((IClientChannel)service).OperationTimeout = TimeSpan.FromSeconds(operationTimeout);
			}

			return service;
		}














		Dictionary<Type, object> channelFactoryCache = new Dictionary<Type, object>();
		private ChannelFactory<T> GetChannelFactory<T>
			(
#if NETTCP
			string token
#endif
			)
		{
			if (!channelFactoryCache.ContainsKey(typeof(T)))
			{
				var serverAddress = ConfigurationManager.AppSettings["ServerAddress"];
				var serverPort = int.Parse(ConfigurationManager.AppSettings["ServerPort"]);
				var maxReceivedMessageSize = long.Parse(ConfigurationManager.AppSettings["MaxReceivedMessageSize"]);
#if NETTCP
				var channelFactory = new ChannelFactory<T>(NetTcpBinding(maxReceivedMessageSize), $"net.tcp://{serverAddress}:{serverPort}/WCFX/{typeof(T).FullName}");
				channelFactory.Credentials.SupportInteractive = false;
				channelFactory.Credentials.UserName.UserName = token;
				channelFactory.Credentials.UserName.Password = "jwt";
				channelFactory.Credentials.ServiceCertificate.Authentication.CertificateValidationMode = System.ServiceModel.Security.X509CertificateValidationMode.None;//beacause of self signed cert, not a good idea for production
#endif
#if WS2007HTTP
				var channelFactory = new ChannelFactory<T>(WS2007FederationHttpBinding(maxReceivedMessageSize), $"https://{serverAddress}:{serverPort + 1}/WCFX/{typeof(T).FullName}");
#endif
				channelFactoryCache.Add(typeof(T), channelFactory);
			}
			return (ChannelFactory<T>)channelFactoryCache[typeof(T)];
		}






		public static NetTcpBinding NetTcpBinding(long maxReceivedMessageSize)
		{
			var binding = new NetTcpBinding();
			binding.MaxReceivedMessageSize = maxReceivedMessageSize;
			binding.MaxBufferSize = (int)Math.Min(Int32.MaxValue, maxReceivedMessageSize);
			binding.ReaderQuotas.MaxStringContentLength = int.MaxValue;
			binding.Security.Mode = SecurityMode.TransportWithMessageCredential;
			binding.Security.Message.ClientCredentialType = MessageCredentialType.UserName;

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
	}
}
