using Microsoft.AspNetCore.Authorization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CleanBase.Core.Api.Authorization
{
	public class RoleAttribute : AuthorizeAttribute
	{
		public string[] RoleNames { get; set; }

		public RoleAttribute(params string[] roles)
		{
			this.RoleNames = roles;
			this.Policy = RoleConstants.Prefix + string.Join(",", roles);
		}
	}
}
