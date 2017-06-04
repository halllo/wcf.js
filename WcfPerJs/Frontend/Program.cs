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

namespace Frontend
{
	class Program
	{
		static void Main(string[] args)
		{
			var baseAddress = new Uri("http://localhost:8082"); //selbe origin wie das backend; geht :)

			using (WebApp.Start(baseAddress.ToString()))
			{
				Console.WriteLine("The HTML client is ready at {0}web/jsclient.html", baseAddress);
				Process.Start(baseAddress + "web/jsclient.html");

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
}
