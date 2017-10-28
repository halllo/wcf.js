using System;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;

namespace WCFX.Server.WCF
{
	public class WcfService
	{
		private WcfService()
		{
		}

		public static WcfService Host<TService>()
		{
			var wcf = new WcfService();

			wcf.mServiceHost = new ServiceHost(typeof(TService));

			return wcf;
		}

		public static WcfService Host(object serviceImplementation)
		{
			var wcf = new WcfService();

			wcf.mServiceHost = new ServiceHost(serviceImplementation);

			return wcf;
		}

		public WcfService AddNetTcpEndpoint<TContract>(string address, long maxReceivedMessageSize)
		{
			var binding = WcfBindingProvider.GetNetTcpBinding(maxReceivedMessageSize);
			AddEndpoint<TContract>(string.Concat("net.tcp://", address), binding);

			return this;
		}

		public WcfService AddHttpEndpoint<TContract>(string address, bool isMtomEnabled, long maxReceivedMessageSize, bool useWindowsAuthentication = false)
		{
			var wsHttpBinding = WcfBindingProvider.GetWsHttpBinding(maxReceivedMessageSize, isMtomEnabled, useWindowsAuthentication);
			AddEndpoint<TContract>(string.Concat("http://", address), wsHttpBinding);

			return this;
		}

		public WcfService AddHttpsEndpoint<TContract>(string address, bool isMtomEnabled, long maxReceivedMessageSize)
		{
			var customBinding = WcfBindingProvider.GetSecuredWsHttpBindingWithWindowsAuthentication(maxReceivedMessageSize, isMtomEnabled);
			AddEndpoint<TContract>(string.Concat("https://", address), customBinding);

			return this;
		}

		public ServiceHost Start()
		{
			SetMaxItemsInObjectGraph(int.MaxValue);
			EnableExceptionDetailsInFaults(true);

			mServiceHost.Open();

			return mServiceHost;
		}

		private ServiceEndpoint AddEndpoint<TContract>(string address, Binding binding)
		{
			var serviceEndpoint = mServiceHost.AddServiceEndpoint(typeof(TContract), binding, address);
			serviceEndpoint.Binding.OpenTimeout = mTimeoutDuration;
			serviceEndpoint.Binding.CloseTimeout = mTimeoutDuration;
			serviceEndpoint.Binding.ReceiveTimeout = mTimeoutDuration;
			serviceEndpoint.Binding.SendTimeout = mTimeoutDuration;

			return serviceEndpoint;
		}

		private void SetMaxItemsInObjectGraph(int size)
		{
			var sba = GetBehavior<ServiceBehaviorAttribute>();
			sba.MaxItemsInObjectGraph = size;
		}

		private void EnableExceptionDetailsInFaults(bool isEnabled)
		{
			var sdb = GetBehavior<ServiceDebugBehavior>();
			sdb.IncludeExceptionDetailInFaults = isEnabled;
		}

		private T GetBehavior<T>() where T : class, IServiceBehavior, new()
		{
			var smb = mServiceHost.Description.Behaviors.Find<T>();
			if (smb == null)
			{
				smb = new T();
				mServiceHost.Description.Behaviors.Add(smb);
			}

			return smb;
		}

		private ServiceHost mServiceHost;
		private readonly TimeSpan mTimeoutDuration = TimeSpan.FromMinutes(5);
	}
}