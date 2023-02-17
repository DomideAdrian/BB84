using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace CLI_SignalR_Messages.ClassicComunication;
internal class ClassicComunicationService
{
	public void startService(string[] args)
	{
		CreateWebHostBuilder(args).Build().Run();
	}
	private static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
		WebHost.CreateDefaultBuilder(args)
			.UseStartup<Startup>();

	public class Startup
	{
		public Startup(IConfiguration configuration)
		{
			configuration = configuration;
		}
		public IConfiguration configuration { get; }

		public void ConfigureServices(IServiceCollection services)
		{
			services.AddSignalR();
		}

		public void Configure(IApplicationBuilder app)
		{
			app.UseRouting();
			app.UseEndpoints(endpoints =>
			{
				endpoints.MapHub<ChatHub>("/chatHub");
			});
		}

	}
}

