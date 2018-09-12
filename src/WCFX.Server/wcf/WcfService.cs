using System;
using System.IdentityModel.Configuration;
using System.Security.Cryptography.X509Certificates;
using System.ServiceModel;
using System.ServiceModel.Description;
using System.ServiceModel.Security;

namespace WCFX.Server.wcf
{
	public class WcfService
	{
		//static ServiceHostFactory _ServiceHostFactory = new WebServiceHostFactory();

		public WcfService(string urlInfix) => this.urlInfix = urlInfix;

		public void HostNetTcp<TService, TContract>(string serverAddress, int port, long maxReceivedMessageSize, string certThumbprint)
		{
			//var serviceHost = _ServiceHostFactory.CreateServiceHost(typeof(TService).FullName, new[] { new Uri($"net.tcp://{serverAddress}:{port}/{urlInfix}") });
			var serviceHost = new ServiceHost(typeof(TService), new[] { new Uri($"net.tcp://{serverAddress}:{port}") });
			serviceHost.Credentials.ServiceCertificate.SetCertificate(StoreLocation.CurrentUser, StoreName.My, X509FindType.FindByThumbprint, certThumbprint);
			serviceHost.Credentials.UserNameAuthentication.UserNamePasswordValidationMode = UserNamePasswordValidationMode.Custom;
			serviceHost.Credentials.UserNameAuthentication.CustomUserNamePasswordValidator = new CustomUsernameJwtValidator();

			var serviceEndpoint = serviceHost.AddServiceEndpoint(typeof(TContract).FullName,
				binding: NetTcpBinding(maxReceivedMessageSize),
				address: $"/{typeof(TContract).FullName}");

			SetStuff(serviceHost, serviceEndpoint);
			serviceHost.Open();
		}

		public void HostWS2007FederationHttp<TService, TContract>(string serverAddress, int port, long maxReceivedMessageSize, string certThumbprint)
		{
			var identityConfig = new IdentityConfiguration();
			identityConfig.SecurityTokenHandlers.Clear();
			identityConfig.SecurityTokenHandlers.Add(new SamlJwtValidator());
			identityConfig.ClaimsAuthorizationManager = new RequireAuthenticationAuthorization();

			//var serviceHost = _ServiceHostFactory.CreateServiceHost(typeof(TService).FullName, new[] { new Uri($"https://{serverAddress}:{port}") });
			var serviceHost = new ServiceHost(typeof(TService), new[] { new Uri($"https://{serverAddress}:{port}") });
			serviceHost.Credentials.ServiceCertificate.SetCertificate(StoreLocation.CurrentUser, StoreName.My, X509FindType.FindByThumbprint, certThumbprint);
			serviceHost.Credentials.IdentityConfiguration = identityConfig;
			serviceHost.Credentials.UseIdentityConfiguration = true;

			var serviceEndpoint = serviceHost.AddServiceEndpoint(typeof(TContract).FullName,
				binding: WS2007FederationHttpBinding(maxReceivedMessageSize),
				address: $"/{urlInfix}/{typeof(TContract).FullName}");

			SetStuff(serviceHost, serviceEndpoint);
			serviceHost.Open();
		}

		private void SetStuff(ServiceHostBase serviceHost, ServiceEndpoint serviceEndpoint)
		{
			serviceEndpoint.Binding.OpenTimeout = mTimeoutDuration;
			serviceEndpoint.Binding.CloseTimeout = mTimeoutDuration;
			serviceEndpoint.Binding.ReceiveTimeout = mTimeoutDuration;
			serviceEndpoint.Binding.SendTimeout = mTimeoutDuration;

			GetBehavior<ServiceAuthorizationBehavior>(serviceHost).PrincipalPermissionMode = PrincipalPermissionMode.Always;
			GetBehavior<ServiceBehaviorAttribute>(serviceHost).MaxItemsInObjectGraph = int.MaxValue;
			GetBehavior<ServiceDebugBehavior>(serviceHost).IncludeExceptionDetailInFaults = true;

			serviceEndpoint.EndpointBehaviors.Add(new SoapLoggerBehavior());
		}

		private T GetBehavior<T>(ServiceHostBase serviceHost) where T : class, IServiceBehavior, new()
		{
			var smb = serviceHost.Description.Behaviors.Find<T>();
			if (smb == null)
			{
				smb = new T();
				serviceHost.Description.Behaviors.Add(smb);
			}

			return smb;
		}

		private static NetTcpBinding NetTcpBinding(long maxReceivedMessageSize)
		{
			var binding = new NetTcpBinding();
			binding.MaxReceivedMessageSize = maxReceivedMessageSize;
			binding.MaxBufferSize = (int)Math.Min(Int32.MaxValue, maxReceivedMessageSize);
			binding.ReaderQuotas.MaxStringContentLength = int.MaxValue;
			binding.Security.Mode = SecurityMode.TransportWithMessageCredential;
			binding.Security.Message.ClientCredentialType = MessageCredentialType.UserName;

			return binding;
		}

		private static WS2007FederationHttpBinding WS2007FederationHttpBinding(long maxReceivedMessageSize)
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

		private readonly TimeSpan mTimeoutDuration = TimeSpan.FromMinutes(5);
		private readonly string urlInfix;
	}
}