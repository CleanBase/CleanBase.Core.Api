using CleanBase.Core.Api.Extensions;
using CleanBase.Core.Services.Core.Base;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CleanBase.Core.Api.Middlewares
{
	public class RequestLoggingMiddleware
	{
		private readonly RequestDelegate _next;
		private static long _concurrencyUser;

		public RequestLoggingMiddleware(RequestDelegate next) => _next = next;

		public async Task Invoke(HttpContext context, ISmartLogger logger, IIdentityProvider provider)
		{
			DateTime requestStartTime = DateTime.Now;
			StringBuilder requestInfo = new StringBuilder();
			bool hasError = false;
			var requestIP = "";
			Interlocked.Increment(ref _concurrencyUser);

			try
			{
				logger.Information($"[RequestLog]: method: {context.Request.Method}, path: {context.Request.Path}");

				if (context.Request.Query.Any())
				{
					var queryParams = context.Request.Query
						.Select(p => $"{p.Key}: {p.Value}")
						.ToList();
					requestInfo.Append($" Query {JsonConvert.SerializeObject(queryParams)}");
				}

				if (context.Request.ContentType?.Contains("json") == true &&
					context.Request.ContentLength.HasValue &&
					context.Request.ContentLength.Value < 20000000)
				{
					context.Request.EnableBuffering();
					using var reader = new StreamReader(context.Request.Body, Encoding.UTF8, leaveOpen: true);
					var body = await reader.ReadToEndAsync();
					context.Request.Body.Position = 0; // Reset position for further middleware
					requestInfo.Append(body);
				}

				requestIP = context.GetRemoteIPAddress()?.ToString() ?? string.Empty;

				await _next(context);
			}
			catch (Exception ex)
			{
				logger.Error(ex, "An error occurred while processing the request.");
				hasError = true;
			}
			finally
			{
				Interlocked.Decrement(ref _concurrencyUser);

				var userName = provider.Identity?.UserName ??
					context.User?.FindFirst("name")?.Value ??
					context.User?.FindFirst("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/upn")?.Value ??
					context.User?.FindFirst("sub")?.Value ??
					context.User?.FindFirst("preferred_username")?.Value;

				DateTime now = DateTime.Now;
				int statusCode = hasError ? 500 : 200;
				string errorMessage = hasError ? " See error details above." : string.Empty;
				double totalMilliseconds = (now - requestStartTime).TotalMilliseconds;

				logger.Information($"[RequestLog]: IP: {requestIP}, User: {userName}, method: {context.Request.Method}, path: {context.Request.Path}, status: {statusCode}.{errorMessage} requestInfo {requestInfo}, start at {requestStartTime}, end at {now}, time {totalMilliseconds} ms, concurrency user {_concurrencyUser}");
			}
		}
	}
}
