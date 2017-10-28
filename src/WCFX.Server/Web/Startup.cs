using System;
using System.IO;
using System.Reflection;
using Microsoft.Owin;
using Microsoft.Owin.Cors;
using Microsoft.Owin.FileSystems;
using Microsoft.Owin.StaticFiles;
using Owin;
using WCFX.Server.Web;

[assembly: OwinStartup(typeof(Startup))]
namespace WCFX.Server.Web
{
	public class Startup
	{
		public void Configuration(IAppBuilder app)
		{
			//app.Use((c, next) => next());
			//app.UseCors(CorsOptions.AllowAll);

			var staticFileOptions = new StaticFileOptions
			{
				RequestPath = new PathString("/wb"),
				FileSystem = new PhysicalFileSystem(Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? String.Empty, "Web"))
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
