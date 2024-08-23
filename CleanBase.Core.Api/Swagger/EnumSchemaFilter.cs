using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CleanBase.Core.Api.Swagger
{
	public class EnumSchemaFilter : ISchemaFilter
	{
		public void Apply(OpenApiSchema schema, SchemaFilterContext context)
		{
			if (!context.Type.IsEnum)
				return;

			schema.Type = "string"; 
			schema.Enum.Clear();

			foreach (var name in Enum.GetNames(context.Type))
			{
				schema.Enum.Add(new OpenApiString(name));
			}
		}
	}
}
