using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.ComponentModel.DataAnnotations;

namespace Leebruce.Api;

public class RecordValidationSchemaFilter : ISchemaFilter
{
	public void Apply( OpenApiSchema schema, SchemaFilterContext context )
	{
		if ( context is { Type: not null, MemberInfo: null, ParameterInfo: null } )
		{
			ApplyType( schema, context );
			return;
		}

		if ( context is { MemberInfo.DeclaringType: not null, ParameterInfo: null } )
		{
			ApplyMember( schema, context );
			return;
		}

	}

	private static void ApplyMember( OpenApiSchema schema, SchemaFilterContext context )
	{
		if ( context is not { MemberInfo.DeclaringType: not null, ParameterInfo: null } )
		{
			return;
		}

		var ctor = context.MemberInfo.DeclaringType.GetConstructors( System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance ).SingleOrDefault();
		if ( ctor is null )
		{
			return;
		}

		var param = ctor.GetParameters()
			.Where( p => p.Name == context.MemberInfo.Name )
			.FirstOrDefault();
		if ( param is null )
		{
			return;
		}

		foreach ( var attr in param.GetCustomAttributes( true ) )
		{
			switch ( attr )
			{
				case RequiredAttribute:
					schema.Nullable = false;
					if ( context.SchemaRepository.TryLookupByType( context.MemberInfo.DeclaringType, out var parentSchema ) )
					{
						_ = parentSchema.Required.Add( schema.Type );
					}
					break;
				case StringLengthAttribute a:
					schema.MaxLength = a.MaximumLength;
					schema.MinLength = a.MinimumLength;
					break;

				case MaxLengthAttribute a:
					schema.MaxLength = a.Length;
					break;

				case MinLengthAttribute a:
					schema.MinLength = a.Length;
					break;

				case RangeAttribute a:
					if ( a.Minimum is IConvertible )
					{
						schema.Minimum = Convert.ToDecimal( a.Minimum );
					}
					if ( a.Maximum is IConvertible )
					{
						schema.Maximum = Convert.ToDecimal( a.Minimum );
					}
					break;

				case DisplayFormatAttribute a:
					break;

				case RegularExpressionAttribute a:
					schema.Pattern = a.Pattern;
					break;
			}
		}
	}

	private static void ApplyType( OpenApiSchema schema, SchemaFilterContext context )
	{
		var ctors = context.Type.GetConstructors( System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance );
		if ( ctors.Length > 1 )
		{
			return;
		}
		var ctor = ctors.SingleOrDefault();
		if ( ctor is null )
		{
			return;
		}

		foreach ( var param in ctor.GetParameters()
			.Where( p => p.Name is not null )
			.Where( p => p
				.GetCustomAttributes( true )
				.OfType<RequiredAttribute>()
				.Any() ) )
		{
			if ( schema.Properties.ContainsKey( param.Name ) )
			{
				_ = schema.Required.Add( param.Name );
				continue;
			}

			var firstLower = param.Name!.ToLower()[0] + param.Name[1..];
			if ( schema.Properties.ContainsKey( firstLower ) )
			{
				_ = schema.Required.Add( firstLower );
			}

		}
	}

}
