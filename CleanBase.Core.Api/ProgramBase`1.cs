using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Serilog;
using System.Threading.Tasks;

namespace CleanBase.Core.Api
{
	public class ProgramBase<TStartup> where TStartup : class
	{
		public static void Main(string[] args)
		{
			try
			{
				var builder = WebApplication.CreateBuilder(args);

				builder.Host.UseSerilog((context, services, configuration) =>
				{
					configuration.ReadFrom.Configuration(context.Configuration)
								  .ReadFrom.Services(services)
								  .Enrich.FromLogContext()
								  .WriteTo.Console();
				});

				builder.Configuration.AddJsonFile($"appsettings.{Environment.MachineName}.json", optional: true);

				var app = builder.Build();
				app.UseRouting();
				app.UseAuthorization();
				app.MapControllers();

				app.Run();
			}
			catch (Exception ex)
			{
				Log.Fatal(ex, "Host terminated unexpectedly");
				throw;
			}
			finally
			{
				Log.CloseAndFlush();
			}
		}
	}
}

