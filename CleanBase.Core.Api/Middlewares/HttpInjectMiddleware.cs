using CleanBase.Core.Services.Core.Base;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CleanBase.Core.Api.Middlewares
{
	public class HttpInjectMiddleware
	{
		private readonly RequestDelegate _next;
		private readonly ILogger<HttpInjectMiddleware> _logger;

		public HttpInjectMiddleware(RequestDelegate next, ILogger<HttpInjectMiddleware> logger)
		{
			_next = next;
			_logger = logger;
		}

		public async Task Invoke(HttpContext httpContext, IIdentityProvider identityProvider)
		{
			try
			{
				if (httpContext.Request.Headers.TryGetValue("TenantId", out var tenantId))
				{
					identityProvider.Identity.TenantId = tenantId.FirstOrDefault();
				}

				if (httpContext.Request.Headers.TryGetValue("AppId", out var appId))
				{
					identityProvider.Identity.AppId = appId.FirstOrDefault();
				}

				if (httpContext.Request.Headers.TryGetValue("Culture", out var culture))
				{
					var cultureInfo = new CultureInfo(culture.ToString());
					CultureInfo.CurrentUICulture = CultureInfo.CurrentCulture = cultureInfo;
				}

				await _next(httpContext);
			}
			catch (CultureNotFoundException ex)
			{
				_logger.LogWarning(ex, "Invalid culture specified.");
				// Handle invalid culture here
				throw;
			}
		}
	}
}
