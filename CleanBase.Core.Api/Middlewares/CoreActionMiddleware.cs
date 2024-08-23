using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CleanBase.Core.Api.Middlewares
{
	public class CoreActionMiddleware
	{
		private readonly RequestDelegate _next;

		public CoreActionMiddleware(RequestDelegate next) => this._next = next;

		public async Task Invoke(HttpContext httpContext, IServiceProvider service)
		{
			await this._next(httpContext);
		}
	}
}
