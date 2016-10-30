using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Configuration;
using System.ServiceModel.Description;
using System.ServiceModel.Dispatcher;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Owin;
using Microsoft.Owin.FileSystems;
using Microsoft.Owin.Hosting;
using Microsoft.Owin.StaticFiles;
using Owin;

namespace WcfPerJs
{
	class Program
	{
		/*
		 * Request via Postman:
		 * 
		 * POST http://localhost:8080/hello
		 * Content-Type: application/soap+xml; charset=utf-8
		 * 
		 * <soap:Envelope xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance" xmlns:xsd="http://www.w3.org/2001/XMLSchema" xmlns:soap="http://www.w3.org/2003/05/soap-envelope">
		 * <soap:Header xmlns:wsa="http://www.w3.org/2005/08/addressing">
		 * <wsa:To>http://localhost:8080/hello</wsa:To>
		 * <wsa:Action>http://tempuri.org/IHelloWorldService/SayHello</wsa:Action>
		 * </soap:Header>
		 * <soap:Body>
		 * <SayHello xmlns="http://tempuri.org/">
		 * </SayHello>
		 * </soap:Body>
		 * </soap:Envelope>
		 * 
		 */
		static void Main(string[] args)
		{
			var baseAddress = new Uri("http://localhost:8080");

			using (var signalR = WebApp.Start(baseAddress.ToString()))
			using (ServiceHost host = new ServiceHost(typeof(HelloWorldService), new Uri(baseAddress, "hello")))
			{
				var smb = new ServiceMetadataBehavior();
				smb.HttpGetEnabled = true;
				smb.MetadataExporter.PolicyVersion = PolicyVersion.Policy15;
				host.Description.Behaviors.Add(smb);


				var wSHttpBinding = new WSHttpBinding();
				wSHttpBinding.Security.Mode = SecurityMode.None;
				var endpoint = host.AddServiceEndpoint(typeof(IHelloWorldService), wSHttpBinding, new Uri("", UriKind.Relative));

				host.Open();


				Console.WriteLine("The wcf service is ready at {0}hello", baseAddress);
				Console.WriteLine("The html is ready at {0}web/jsclient.html", baseAddress);
				Console.WriteLine("\nPress <Enter> to stop the service.");
				Console.ReadLine();

				host.Close();
			}
		}
	}

	public class Startup
	{
		public void Configuration(IAppBuilder app)
		{
			var staticFileOptions = new StaticFileOptions
			{
				RequestPath = new PathString("/web"),
				FileSystem = new PhysicalFileSystem(Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "webdir"))
			};
			app.UseDefaultFiles(new DefaultFilesOptions
			{
				RequestPath = staticFileOptions.RequestPath,
				FileSystem = staticFileOptions.FileSystem,
			});
			app.UseStaticFiles(staticFileOptions);
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





















	// CORS?

	//public class CustomHeaderMessageInspector : IDispatchMessageInspector
	//{
	//	Dictionary<string, string> requiredHeaders;
	//	public CustomHeaderMessageInspector(Dictionary<string, string> headers)
	//	{
	//		requiredHeaders = headers ?? new Dictionary<string, string>();
	//	}

	//	public object AfterReceiveRequest(ref System.ServiceModel.Channels.Message request, System.ServiceModel.IClientChannel channel, System.ServiceModel.InstanceContext instanceContext)
	//	{
	//		return null;
	//	}

	//	public void BeforeSendReply(ref System.ServiceModel.Channels.Message reply, object correlationState)
	//	{
	//		var httpHeader = reply.Properties["httpResponse"] as HttpResponseMessageProperty;
	//		foreach (var item in requiredHeaders)
	//		{
	//			httpHeader.Headers.Add(item.Key, item.Value);
	//		}
	//	}
	//}

	//public class EnableCrossOriginResourceSharingBehavior : BehaviorExtensionElement, IEndpointBehavior
	//{
	//	public void AddBindingParameters(ServiceEndpoint endpoint, System.ServiceModel.Channels.BindingParameterCollection bindingParameters)
	//	{

	//	}

	//	public void ApplyClientBehavior(ServiceEndpoint endpoint, System.ServiceModel.Dispatcher.ClientRuntime clientRuntime)
	//	{

	//	}

	//	public void ApplyDispatchBehavior(ServiceEndpoint endpoint, System.ServiceModel.Dispatcher.EndpointDispatcher endpointDispatcher)
	//	{
	//		var requiredHeaders = new Dictionary<string, string>();

	//		requiredHeaders.Add("Access-Control-Allow-Origin", "*");
	//		requiredHeaders.Add("Access-Control-Request-Method", "POST,GET,PUT,DELETE,OPTIONS");
	//		requiredHeaders.Add("Access-Control-Allow-Headers", "X-Requested-With,Content-Type");

	//		endpointDispatcher.DispatchRuntime.MessageInspectors.Add(new CustomHeaderMessageInspector(requiredHeaders));
	//	}

	//	public void Validate(ServiceEndpoint endpoint)
	//	{

	//	}

	//	public override Type BehaviorType
	//	{
	//		get { return typeof(EnableCrossOriginResourceSharingBehavior); }
	//	}

	//	protected override object CreateBehavior()
	//	{
	//		return new EnableCrossOriginResourceSharingBehavior();
	//	}
	//}

}
