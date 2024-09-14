using CleanBase.Core.Domain.Exceptions;
using CleanBase.Core.Services.Core.Base;
using CleanBase.Core.ViewModels.Response;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CleanBase.Core.Api.Extensions
{
	public static class GlobalExceptionHandler
	{
		private static readonly DefaultContractResolver ContractResolver = new DefaultContractResolver
		{
			NamingStrategy = new CamelCaseNamingStrategy()
		};

		private static JsonSerializerSettings JsonSettings => new JsonSerializerSettings
		{
			ContractResolver = ContractResolver,
			Formatting = Formatting.None,
			MaxDepth = 1,
			ReferenceLoopHandling = ReferenceLoopHandling.Ignore
		};

		private static async Task BuildActionResponse(
			IIdentityProvider identityProvider,
			ISmartLogger logger,
			HttpContext context,
			DomainException domainException)
		{
			context.Response.StatusCode = domainException.HttpCode;
			var response = new FailActionResponse(domainException.Message, domainException.ErrorCode, domainException.ErrorDetails);
			var jsonResponse = JsonConvert.SerializeObject(response, JsonSettings);

			LogError(logger, identityProvider, jsonResponse);

			await context.Response.WriteAsync(jsonResponse);
		}

		private static string FormatError(Exception error)
		{
			var errorDetails = new List<ErrorCodeDetail>();

			if (error.InnerException != null)
			{
				errorDetails.Add(new ErrorCodeDetail { Message = error.InnerException.Message });
			}

			var responseMessage = error.InnerException == null
				? "An unexpected error occurred. Please try again later."
				: error.Message;

			var response = new FailActionResponse(responseMessage, errorDetails);
			return JsonConvert.SerializeObject(response, JsonSettings);
		}

		private static void LogError(ISmartLogger logger, IIdentityProvider identityProvider, string message)
		{
			var logMessage = $"{identityProvider.RequestId} - {identityProvider.Identity?.UserName}: {message}";
			logger.Error(logMessage);
		}

		public static void UseExceptionConfiguration(this IApplicationBuilder builder)
		{
			builder.UseExceptionHandler(app => app.Run(async context =>
			{
				var logger = context.RequestServices?.GetService(typeof(ISmartLogger)) as ISmartLogger;
				var identityProvider = context.RequestServices?.GetService(typeof(IIdentityProvider)) as IIdentityProvider;

				if (context.Response.StatusCode != 500) return;

				context.Response.ContentType = "application/json";
				var exceptionFeature = context.Features.Get<IExceptionHandlerFeature>();
				var error = exceptionFeature?.Error;

				if (error is DomainException domainException)
				{
					await BuildActionResponse(identityProvider, logger, context, domainException);
				}
				else
				{
					var errorId = Guid.NewGuid().ToString();
					var logMessage = $"{identityProvider.RequestId} - {identityProvider.Identity?.UserName} - {errorId}: {error}";
					logger.Error(logMessage);

					var errorResponse = FormatError(error);
					await context.Response.WriteAsync(errorResponse);
				}
			}));
		}
	}

}
