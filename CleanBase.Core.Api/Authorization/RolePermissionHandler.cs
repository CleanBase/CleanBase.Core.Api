using Microsoft.AspNetCore.Authorization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;

namespace CleanBase.Core.Api.Authorization
{
	public class RolePermissionHandler : IAuthorizationHandler
	{
		public Task HandleAsync(AuthorizationHandlerContext context)
		{
			var requirements = context.PendingRequirements.ToList();
			bool isAuthorized = true;

			foreach (var requirement in requirements)
			{
				if (requirement is RoleRequirement roleRequirement)
				{
					if (!CheckRoles(context.User.Identity, roleRequirement.Roles))
					{
						isAuthorized = false;
						break;
					}
					context.Succeed(requirement);
				}
				else if (requirement is PermissionRequirement permissionRequirement)
				{
					if (!CheckPermissions(context.User.Identity, permissionRequirement.Permissions))
					{
						isAuthorized = false;
						break;
					}
					context.Succeed(requirement);
				}
			}

			if (!isAuthorized)
			{
				context.Fail();
			}

			return Task.CompletedTask;
		}

		protected bool CheckPermissions(IIdentity identity, IEnumerable<string> requiredPermissions)
		{
			if (!identity.IsAuthenticated)
			{
				return false;
			}

			if (HasClaim(identity, RoleConstants.Type, RoleConstants.SuperAdmin))
			{
				return true;
			}

			return requiredPermissions.Any(permission => HasClaim(identity, PermissionConstants.Type, permission));
		}

		protected virtual bool HasClaim(IIdentity identity, string claimType, string claimValue)
		{
			return identity is ClaimsIdentity claimsIdentity &&
				   claimsIdentity.HasClaim(c => c.Type == claimType && c.Value == claimValue);
		}

		protected virtual bool CheckRoles(IIdentity identity, IEnumerable<string> requiredRoles)
		{
			if (!identity.IsAuthenticated)
			{
				return false;
			}

			return requiredRoles.Any(role => HasClaim(identity, RoleConstants.Type, role));
		}
	}
}
