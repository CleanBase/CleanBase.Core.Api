using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace CleanBase.Core.Api.Extensions
{
	public static class HttpContextExtensions
	{
		public static IPAddress GetRemoteIPAddress(this HttpContext context, bool allowForwarded = true)
		{
			IPAddress address;
			return allowForwarded && IPAddress.TryParse(context.Request.Headers["CF-Connecting-IP"].FirstOrDefault<string>() ?? context.Request.Headers["X-Forwarded-For"].FirstOrDefault<string>(), out address) ? address : context.Connection.RemoteIpAddress;
		}
	}
}
