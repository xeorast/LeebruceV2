using System.Text;
using js = System.Text.Json;

namespace Leebruce.Api.Services;

public class JsonService
{
	private static readonly js.JsonSerializerOptions options = new( js.JsonSerializerDefaults.Web );

	public string ToJson<T>( T obj )
	{
		return js.JsonSerializer.Serialize<T>( obj, options );
	}
	public T? FromJson<T>( string json )
	{
		return js.JsonSerializer.Deserialize<T>( json, options );
	}
	public string ToBase64Json<T>( T obj )
	{
		var json = ToJson( obj );
		return Convert.ToBase64String( Encoding.UTF8.GetBytes( json ) );
	}
	public T? FromBase64Json<T>( string base64 )
	{
		var json = Encoding.UTF8.GetString( Convert.FromBase64String( base64 ) );
		return FromJson<T>( json );
	}


}
