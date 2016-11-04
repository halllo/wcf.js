using System;
using System.ServiceModel;
using System.ServiceModel.Description;

namespace WcfPerJs
{
	public class Wcf
	{
		public static ServiceHost Start(Uri baseAddress)
		{
			var host = new ServiceHost(typeof(HelloWorldService), new Uri(baseAddress, "hello"));
			var smb = new ServiceMetadataBehavior();
			smb.HttpGetEnabled = true;
			smb.MetadataExporter.PolicyVersion = PolicyVersion.Policy15;
			host.Description.Behaviors.Add(smb);

			var wSHttpBinding = new WSHttpBinding();
			wSHttpBinding.Security.Mode = SecurityMode.None;
			var endpoint = host.AddServiceEndpoint(typeof(IHelloWorldService), wSHttpBinding, new Uri("", UriKind.Relative));
			host.Open();

			return host;
		}
	}





	[ServiceContract]
	public interface IHelloWorldService
	{
		[OperationContract]
		string SayHello();
	}
	
	public class HelloWorldService : IHelloWorldService
	{
		public string SayHello()
		{
			return "Hello, 0, name";
		}
	}
}
