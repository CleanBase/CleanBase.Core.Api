using AutoMapper;
using CleanBase.Core.Domain.Exceptions;
using CleanBase.Core.Services.Core.Base;
using CleanBase.Core.ViewModels.Response;
using CleanBase.Core.ViewModels.Response.Generic;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CleanBase.Core.Api.Controllers
{
	[Produces("application/json")]
	[Authorize]
	public abstract class ApiControllerBase : ControllerBase
	{
		protected ISmartLogger Logger { get; }
		protected IMapper Mapper { get; }
		protected IIdentityProvider IdentityProvider { get; }

		protected ApiControllerBase(ICoreProvider coreProvider)
		{
			Logger = coreProvider.Logger;
			Mapper = coreProvider.Mapper;
			IdentityProvider = coreProvider.IdentityProvider;
		}

		protected IActionResult CreateSuccessResult<T>(T result)
		{
			return Ok(new ActionResponse<T>(result));
		}

		protected IActionResult CreateSuccess()
		{
			return Ok(new ActionResponse());
		}

		private static IEnumerable<ErrorCodeDetail> ConvertError(IEnumerable<string> details)
		{
			return details.Select(detail => new ErrorCodeDetail { Message = detail }).ToList();
		}

		protected IActionResult CreateFailResult(string error, string errorCode, IEnumerable<string> details)
		{
			return BadRequest(new FailActionResponse(error, errorCode, ConvertError(details)));
		}

		protected IActionResult CreateFailResult(string error, string errorCode, IEnumerable<ErrorCodeDetail> details)
		{
			return BadRequest(new FailActionResponse(error, errorCode, details));
		}

		protected IActionResult CreateFailResult(string error, IEnumerable<string> details)
		{
			return BadRequest(new FailActionResponse(error, string.Empty, ConvertError(details)));
		}

		protected IActionResult CreateFailResult(string error, IEnumerable<ErrorCodeDetail> details)
		{
			return BadRequest(new FailActionResponse(error, string.Empty, details));
		}

		protected IActionResult CreateFailResult(string error)
		{
			return BadRequest(new FailActionResponse(error, string.Empty, null));
		}
	}
}
