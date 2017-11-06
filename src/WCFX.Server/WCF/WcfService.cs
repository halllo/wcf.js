using System;
using System.IdentityModel.Configuration;
using System.Security.Cryptography.X509Certificates;
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

		public static WcfService Host<TService>(string serverCertSubjectName)
		{
			var identityConfig = new IdentityConfiguration();
			identityConfig.SecurityTokenHandlers.Clear();
			identityConfig.SecurityTokenHandlers.Add(new JwtValidator());
			identityConfig.ClaimsAuthorizationManager = new RequireAuthenticationAuthorization();

			var serviceHost = new ServiceHost(typeof(TService));
			serviceHost.Credentials.IdentityConfiguration = identityConfig;
			serviceHost.Credentials.UseIdentityConfiguration = true;
			serviceHost.Credentials.ServiceCertificate.SetCertificate(StoreLocation.LocalMachine, StoreName.Root, X509FindType.FindBySubjectName, serverCertSubjectName);

			var authz = serviceHost.Description.Behaviors.Find<ServiceAuthorizationBehavior>();
			authz.PrincipalPermissionMode = PrincipalPermissionMode.Always;

			return new WcfService { mServiceHost = serviceHost };
		}

		public ServiceHost Start()
		{
			SetMaxItemsInObjectGraph(int.MaxValue);
			EnableExceptionDetailsInFaults(true);

			mServiceHost.Open();

			return mServiceHost;
		}

		public ServiceEndpoint AddEndpoint<TContract>(string address, Binding binding)
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