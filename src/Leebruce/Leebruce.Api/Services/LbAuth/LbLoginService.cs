using Leebruce.Api.Models;
using System.Net;

namespace Leebruce.Api.Services.LbAuth;

public interface ILbLoginService
{
	Task<string> LoginAsync( string username, string password );
}

public class LbLoginService : ILbLoginService, IDisposable
{

	private readonly HttpClientHandler _httpHandler;
	private readonly HttpClient _http;
	private readonly JsonService _json;

	public LbLoginService( JsonService json )
	{
		_json = json;
		_httpHandler = new() { AllowAutoRedirect = false, CookieContainer = new() };
		_http = new( _httpHandler );
	}

	public async Task<string> LoginAsync( string username, string password )
	{
		await PreCall();
		await SendCredentials( username, password );
		var uri = await PostGrant();
		await PostToken( uri );

		var cookies = _httpHandler.CookieContainer.GetCookies( LbConstants.lbCookiesDomain );

		return PackCookiesData( cookies );
	}

	private string PackCookiesData( CookieCollection cookies )
	{
		var dsid = cookies[LbConstants.dsidName]?.Value;
		var sdsid = cookies[LbConstants.sdsidName]?.Value;
		var oatoken = cookies[LbConstants.oatokenName]?.Value;

		if ( string.IsNullOrEmpty( dsid )
			|| string.IsNullOrEmpty( sdsid )
			|| string.IsNullOrEmpty( oatoken ) )
		{
			throw new LbLoginException( "Server did not send all required cookies." );
		}

		var escapedOa = Uri.UnescapeDataString( oatoken );
		LbAuthData data = new( dsid, sdsid, escapedOa );
		return _json.ToBase64Json( data );
	}

	// I recommend not reading this, this is an example of how login process hould NOT be structured
	#region login stages
	/// <summary>
	/// precedes login action and sets cookies up
	/// </summary>
	/// <returns></returns>
	private async Task PreCall()
	{
		using var resp = await _http.GetAsync( "https://api.librus.pl/OAuth/Authorization?client_id=46&response_type=code&scope=mydata" );
	}

	/// <summary>
	/// sends actual credentials
	/// </summary>
	/// <param name="username"></param>
	/// <param name="password"></param>
	/// <returns>wether authentication has succeeded</returns>
	private async Task SendCredentials( string username, string password )
	{
		Dictionary<string, string> data = new()
		{
			["login"] = username,
			["pass"] = password,
			["action"] = "login",
		};
		using FormUrlEncodedContent ctnt = new( data );
		ctnt.Headers.ContentType = new( "application/x-www-form-urlencoded" );
		ctnt.Headers.ContentLength = data.Sum( x => x.Key.Length + x.Value.Length + 2 ) - 1;

		using var resp = await _http.PostAsync( "https://api.librus.pl/OAuth/Authorization?client_id=46", ctnt );
		var respData = await resp.Content.ReadFromJsonAsync<CredentialsResponse>();

		if ( respData is { Errors.Length: > 0 } )
		{
			throw new NotAuthorizedException( $"Error(s) occured: {_json.ToJson( respData.Errors )}" );
		}
	}
	record CredentialsResponse(
		string Status,
		CredentialsResponse.Error[] Errors )
	{
		public record Error(
			int Code,
			string Message );
	}

	/// <summary>
	/// gets tokent for verification
	/// </summary>
	/// <returns>uri containing token to verification</returns>
	private async Task<Uri> PostGrant()
	{
		using var resp = await _http.PostAsync( "https://api.librus.pl/OAuth/Authorization/Grant?client_id=46", null );
		return resp.Headers.Location ?? throw new LbLoginException( "Server did not return verification token." );
	}

	/// <summary>
	/// finishes login action by submiting token (at least I think that's how it works)
	/// </summary>
	/// <param name="uri">Location header from <see cref="SendCredentials(string, string)"/> response containing token</param>
	/// <returns></returns>
	private async Task PostToken( Uri uri )
	{
		using var resp = await _http.PostAsync( uri, null );
	}

	#endregion

	private bool disposedValue;
	protected virtual void Dispose( bool disposing )
	{
		if ( !disposedValue )
		{
			if ( disposing )
			{
				( (IDisposable)_http ).Dispose();
				( (IDisposable)_httpHandler ).Dispose();
			}
			disposedValue = true;
		}
	}
	public void Dispose()
	{
		Dispose( disposing: true );
		GC.SuppressFinalize( this );
	}
}
