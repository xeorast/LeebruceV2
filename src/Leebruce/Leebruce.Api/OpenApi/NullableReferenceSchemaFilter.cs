using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Reflection;

namespace Leebruce.Api.OpenApi;

public class NullableReferenceSchemaFilter : ISchemaFilter
{
	static readonly NullabilityInfoContext nullCtx = new();
	public void Apply( OpenApiSchema schema, SchemaFilterContext context )
	{
		if ( context.MemberInfo is not PropertyInfo prop )
			return;

		var nullInfo = nullCtx.Create( prop );

		if ( nullInfo.WriteState is NullabilityState.NotNull )
			schema.Nullable = false;

	}

}
