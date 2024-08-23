using CleanBase.Core.Entities;
using CleanBase.Core.Entities.Base;
using CleanBase.Core.Services.Core.Base;
using CleanBase.Core.Services.Core.Generic;
using CleanBase.Core.ViewModels.Request;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CleanBase.Core.Api.Controllers
{
	public class CRUDSummaryController<T, TRequest, TResponse, TGetAllRequest, TService> :
		CRUDSummaryController<T, TRequest, TResponse, TGetAllRequest, EntityBaseName, TService>
		where T : class, IEntityKey, new()
		where TRequest : IKeyObject
		where TGetAllRequest : GetAllRequest
		where TService : IServiceBase<T, TRequest, TResponse, TGetAllRequest>
	{
		public CRUDSummaryController(ICoreProvider coreProvider, TService service)
		  : base(coreProvider, service)
		{
		}
	}
}