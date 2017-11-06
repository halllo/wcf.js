using System;
using System.Configuration;
using System.ServiceModel;
using System.ServiceModel.Description;
using WCFX.Common;

namespace WCFX.DesktopClient
{
	public class WcfServiceProvider
	{
		public TResult Execute<TService, TResult>(string token, Func<TService, TResult> func, int operationTimeout = 90) where TService : class, IWcfService
		{
			var service = CreateService<TService>(token, operationTimeout);
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

		public void Execute<TService>(string token, Action<TService> action, int operationTimeout = 90) where TService : class, IWcfService
		{
			var service = CreateService<TService>(token, operationTimeout);
			try
			{
				action(service);
			}
			catch (Exception)
			{
				throw;
			}
		}




























		public TService CreateService<TService>(string token, int operationTimeout = 90) where TService : class, IWcfService
		{
			var channelFactory = GetChannelFactory<TService>();
			channelFactory.Credentials.SupportInteractive = false;
			var service = channelFactory.CreateChannelWithIssuedToken(TokenStuff.WrapJwt(token));
			
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




		private ChannelFactory<T> GetChannelFactory<T>()
		{
			var serverAddress = ConfigurationManager.AppSettings["ServerAddress"];
			var serverPort = ConfigurationManager.AppSettings["ServerPort"];
			var maxReceivedMessageSize = long.Parse(ConfigurationManager.AppSettings["MaxReceivedMessageSize"]);

			return new ChannelFactory<T>(WcfBindingProvider.WS2007FederationHttpBinding(maxReceivedMessageSize), $"https://{serverAddress}:{serverPort}/WCFX/{typeof(T).FullName}");
			//return new ChannelFactory<T>(WcfBindingProvider.NetTcpBinding(maxReceivedMessageSize), $"net.tcp://{serverAddress}:{serverPort}/WCFX/{typeof(T).FullName}");
		}
	}
}
