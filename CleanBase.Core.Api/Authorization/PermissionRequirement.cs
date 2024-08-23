using Microsoft.AspNetCore.Authorization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CleanBase.Core.Api.Authorization
{
	public class PermissionRequirement : IAuthorizationRequirement
	{
		public string[] Permissions { get; set; }
		public PermissionRequirement(params string[] permissions) => Permissions = permissions;
	}
}
