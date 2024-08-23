using CleanBase.Core.Domain.Generic;
using CleanBase.Core.Entities;
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
	public class CRUDSummaryController<T, TRequest, TResponse, TGetAllRequest, TSummary, TService> :
		CRUDController<T, TRequest, TResponse, TGetAllRequest, TService>
		where T : class, IEntityKey, new()
		where TGetAllRequest : GetAllRequest
		where TSummary : class, IEntityKey, new()
		where TService : IServiceBase<T, TRequest, TResponse, TGetAllRequest, TSummary>
	{
		public CRUDSummaryController(ICoreProvider coreProvider, TService service)
			: base(coreProvider, service)
		{
		}

		protected virtual async Task<ListResult<TSummary>> GetAllSummaryInternal(TGetAllRequest request)
		{
			var summaries = Service.GetAllSummary(request).ToList();
			return new ListResult<TSummary>(summaries);
		}

		[HttpGet("summary")]
		public virtual async Task<IActionResult> GetAllSummary([FromQuery] TGetAllRequest request)
		{
			Logger.Information($"[{GetType().Name}]: Call to GetAllSummary");
			var summaries = await GetAllSummaryInternal(request);
			return CreateSuccessResult(summaries);
		}
	}
}
