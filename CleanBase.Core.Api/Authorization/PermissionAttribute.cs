using Microsoft.AspNetCore.Authorization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CleanBase.Core.Api.Authorization
{
	public class PermissionAttribute : AuthorizeAttribute
	{
		public string[] Claims { get; set; }
		public PermissionAttribute(params string[] claims) 
		{
			this.Claims = claims;
			this.Policy = PermissionConstants.Prefix + string.Join(",", claims);
		}
	}
}
