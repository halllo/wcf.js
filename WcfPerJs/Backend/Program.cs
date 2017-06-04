using Microsoft.Owin.Cors;
using Microsoft.Owin.Hosting;
using Owin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Description;
using System.ServiceModel.Dispatcher;
using System.Text;
using System.Threading.Tasks;

namespace Backend
{
	class Program
	{
		static void Main(string[] args)
		{
			var baseAddress = new Uri("http://localhost:8082");

			//using (WebApp.Start(baseAddress.ToString()))
			using (var host = Wcf.Start(baseAddress))
			{
				Console.WriteLine("The WCF service is ready at {0}hello", baseAddress);

				Console.WriteLine("\n" + "Press <Enter> to stop the service.");
				Console.ReadLine();
				host.Close();
			}
		}
	}


	public class Startup
	{
		public void Configuration(IAppBuilder app)
		{
			app.UseCors(CorsOptions.AllowAll);
			app.Use((c, next) => next()); //ServiceHost requests dont go through OWIN middlewares :(
		}
	}



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
			endpoint.EndpointBehaviors.Add(new MyBehavior());

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
