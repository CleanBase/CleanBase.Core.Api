using CleanBase.Core.Entities;
using CleanBase.Core.Services.Core.Base;
using CleanBase.Core.Services.Core.Generic;
using CleanBase.Core.ViewModels.Request;
using CleanBase.Core.ViewModels.Response;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CleanBase.Core.Api.Controllers
{
	public class CRUDController<T, TRequest, TResponse, TGetAllRequest, TService> :
	   CRUDBaseController<T, TRequest, TResponse, TGetAllRequest, TService>
	   where T : class, IEntityKey, new()
	   where TGetAllRequest : GetAllRequest
	   where TService : IServiceBaseCore<T, TRequest, TGetAllRequest>
	{
		public CRUDController(ICoreProvider coreProvider, TService service)
			: base(coreProvider, service)
		{
		}

		[HttpGet("{id}")]
		[ProducesResponseType(typeof(ActionResponse), 200)]
		[ProducesResponseType(typeof(FailActionResponse), 400)]
		public virtual async Task<IActionResult> GetById(Guid id)
		{
			Logger.Information($"[{GetType().Name}]: call to GetById {id}");
			return await GetByIdInternal(id);
		}

		[HttpGet]
		[ProducesResponseType(typeof(ActionResponse), 200)]
		[ProducesResponseType(typeof(FailActionResponse), 400)]
		public virtual async Task<IActionResult> GetAll([FromQuery] TGetAllRequest request)
		{
			Logger.Information($"[{GetType().Name}]: call to GetAll - filter: {request.Filter} - sort field: {request.SortField} - asc: {request.Asc}");
			return await GetAllInternal(request);
		}

		[HttpPost]
		[ProducesResponseType(typeof(ActionResponse), 200)]
		[ProducesResponseType(typeof(FailActionResponse), 400)]
		public virtual async Task<IActionResult> Create([FromBody] TRequest entity)
		{
			Logger.Information($"[{GetType().Name}]: call to Creating");
			return await CreateOrUpdateInternal(entity);
		}

		[HttpPut]
		[ProducesResponseType(typeof(ActionResponse), 200)]
		[ProducesResponseType(typeof(FailActionResponse), 400)]
		public virtual async Task<IActionResult> Update([FromBody] TRequest entity)
		{
			Logger.Information($"[{GetType().Name}]: call to Update");
			return await CreateOrUpdateInternal(entity);
		}

		[HttpDelete("{id}")]
		[ProducesResponseType(typeof(ActionResponse), 200)]
		[ProducesResponseType(typeof(FailActionResponse), 400)]
		public virtual async Task<IActionResult> Delete([FromRoute] Guid id)
		{
			Logger.Information($"[{GetType().Name}]: call to DeActive");
			return await DeActiveInternal(id);
		}
	}
}
