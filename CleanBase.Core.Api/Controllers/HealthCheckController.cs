using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CleanBase.Core.Api.Controllers
{
	[Route("api/[controller]")]
	[Produces("application/json", new string[] { })]
	[ApiController]
	[AllowAnonymous]
	public class HealthCheckController : ControllerBase
	{
		[HttpGet]
		public string HealthCheck() => "ok";
	}
}
