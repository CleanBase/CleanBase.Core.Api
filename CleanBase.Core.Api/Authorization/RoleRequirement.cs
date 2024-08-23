using Microsoft.AspNetCore.Authorization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CleanBase.Core.Api.Authorization
{
	public class RoleRequirement : IAuthorizationRequirement
	{
		public string[] Roles { get; set; }

		public RoleRequirement(params string[] roles) => this.Roles = roles;
	}
}
