using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CleanBase.Core.Api.Swagger
{
	public class BasePathFilter : IDocumentFilter
	{
		private readonly string basePath;

		public BasePathFilter(string basePath) => this.basePath = basePath;

		public void Apply(OpenApiDocument swaggerDoc, DocumentFilterContext context)
		{
			swaggerDoc.Servers = (IList<OpenApiServer>)new List<OpenApiServer>()
			   {
				 new OpenApiServer() { Url = this.basePath }
			   };
		}
	}
}
