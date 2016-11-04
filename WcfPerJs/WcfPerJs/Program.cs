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

namespace WcfPerJs
{
	class Program
	{
		static void Main(string[] args)
		{
			var baseAddress = new Uri("http://localhost:8080");

			using (WebApp.Start(baseAddress.ToString()))
			{
				Console.WriteLine("The html is ready at {0}web/jsclient.html", baseAddress); Process.Start(baseAddress + "web/jsclient.html");

				using (var host = Wcf.Start(baseAddress))
				{
					Console.WriteLine("The wcf service is ready at {0}hello", baseAddress);
					Console.WriteLine("\n" + "Press <Enter> to stop the service.");
					Console.ReadLine();
					host.Close();
				}
			}
		}
	}





	public class Startup
	{
		public void Configuration(IAppBuilder app)
		{
			app.UseCors(CorsOptions.AllowAll);


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
