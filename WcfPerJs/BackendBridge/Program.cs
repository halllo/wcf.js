using System;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using Microsoft.Owin;
using Microsoft.Owin.Cors;
using Microsoft.Owin.FileSystems;
using Microsoft.Owin.Hosting;
using Microsoft.Owin.StaticFiles;
using Owin;

namespace BackendBridge
{
	class Program
	{
		static void Main(string[] args)
		{
			var baseAddress = new Uri("http://localhost:8081");

			using (WebApp.Start(baseAddress.ToString()))
			{
				Console.WriteLine("The Web API is ready at {0}api/wcf", baseAddress);

				Console.WriteLine("\n" + "Press <Enter> to stop the service.");
				Console.ReadLine();
			}
		}
	}







	public class Startup
	{
		public void Configuration(IAppBuilder app)
		{
			app.UseCors(CorsOptions.AllowAll);


			var config = new HttpConfiguration();
			config.Routes.MapHttpRoute("DefaultApi", "api/{controller}/{id}", new { id = RouteParameter.Optional });
			app.UseWebApi(config);
		}
	}






	public class WcfController : ApiController
	{
		// POST /api/wcf
		public async Task<string> Post([FromBody]Message message)
		{
			try
			{
				var httpClient = new HttpClient();
				var response = await httpClient.PostAsync(message.Url, new StringContent(message.Content, Encoding.UTF8, "application/soap+xml"));
				var responseString = await response.Content.ReadAsStringAsync();
				return responseString;
			}
			catch (Exception e)
			{
				Console.WriteLine("FEHLER: " + e.Message);
				return string.Empty;
			}
		}

		public class Message
		{
			public string Url { get; set; }
			public string ContentType { get; set; }
			public string Content { get; set; }
		}
	}


}
