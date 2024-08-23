using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;

namespace CleanBase.Core.Api.Authorization
{
	public class AuthorizationPolicyProvider : DefaultAuthorizationPolicyProvider
	{
		public AuthorizationPolicyProvider(IOptions<AuthorizationOptions> options) : base(options) { }

		public override Task<AuthorizationPolicy> GetPolicyAsync(string policyName)
		{
			if (policyName.StartsWith(PermissionConstants.Prefix, StringComparison.OrdinalIgnoreCase))
			{
				return Task.FromResult(CreatePolicy(policyName, PermissionConstants.Prefix, typeof(PermissionRequirement)));
			}

			if (policyName.StartsWith(RoleConstants.Prefix, StringComparison.OrdinalIgnoreCase))
			{
				return Task.FromResult(CreatePolicy(policyName, RoleConstants.Prefix, typeof(RoleRequirement)));
			}

			return base.GetPolicyAsync(policyName);
		}

		private AuthorizationPolicy CreatePolicy(string policyName, string prefix, Type requirementType)
		{
			string[] requirements = policyName.Substring(prefix.Length).Split(',');
			var policyBuilder = new AuthorizationPolicyBuilder(Array.Empty<string>());

			var requirement = (IAuthorizationRequirement)Activator.CreateInstance(requirementType, new object[] { requirements });
			policyBuilder.AddRequirements(requirement);

			return policyBuilder.Build();
		}
	}
}
