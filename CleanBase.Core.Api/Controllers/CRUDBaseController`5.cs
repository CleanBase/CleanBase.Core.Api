using CleanBase.Core.Domain.Generic;
using CleanBase.Core.Entities;
using CleanBase.Core.Extensions;
using CleanBase.Core.Services.Core.Base;
using CleanBase.Core.Services.Core.Generic;
using CleanBase.Core.ViewModels.Request;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CleanBase.Core.Api.Controllers
{
	[Produces("application/json")]
	public class CRUDBaseController<T, TRequest, TResponse, TGetAllRequest, TService> : ApiControllerBase
		where T : class, IEntityKey, new()
		where TGetAllRequest : GetAllRequest
		where TService : IServiceBaseCore<T, TRequest, TGetAllRequest>
	{
		protected TService Service { get; }

		public CRUDBaseController(ICoreProvider coreProvider, TService service)
			: base(coreProvider)
		{
			Service = service;
		}

		protected virtual async Task<IActionResult> GetByIdInternal(Guid id)
		{
			var entity = await Service.GetByIdAsync(id);
			var response = entity != null ? Mapper.Map<TResponse>(entity) : default;
			return CreateSuccessResult(response);
		}

		protected virtual async Task<IActionResult> GetAllInternal(TGetAllRequest request)
		{
			request.NormalizeData();
			var result = await Service.ListAsync(request);
			var response = Mapper.Map<ListResult<TResponse>>(result);
			return CreateSuccessResult(response);
		}

		protected virtual async Task<IActionResult> CreateOrUpdateInternal(TRequest request)
		{
			request.NormalizeData();
			var entity = await Service.SaveAsync(request);
			var response = Mapper.Map<TResponse>(entity);
			return CreateSuccessResult(response);
		}

		protected virtual async Task<IActionResult> DeActiveInternal(Guid id)
		{
			var success = await Service.SoftDeleteAsync(id);
			var message = success ? "Action Success" : "Object was deleted or could not be found";
			return CreateSuccessResult(message);
		}
	}
}
