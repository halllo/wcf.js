using System;
using System.IO;
using Microsoft.Owin;
using Microsoft.Owin.FileSystems;
using Microsoft.Owin.StaticFiles;
using Owin;

namespace WCFX.Server
{
	public static class StartupWeb
	{
		public static void UseMyStaticFiles(this IAppBuilder app)
		{
			var root = AppDomain.CurrentDomain.BaseDirectory;
			var physicalFileSystem = new PhysicalFileSystem(Path.Combine(root, "wwwroot"));
			var fileServerOptions = new FileServerOptions
			{
				RequestPath = PathString.Empty,
				EnableDefaultFiles = true,
				FileSystem = physicalFileSystem,
				EnableDirectoryBrowsing = false
			};

			fileServerOptions.StaticFileOptions.ServeUnknownFileTypes = false;
			app.UseFileServer(fileServerOptions);
		}

	}
}
