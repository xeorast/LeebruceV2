namespace Leebruce.Api.Extensions;

public static class HttpClientExtensions
{
	public static async Task<HttpResponseMessage> GetWithCookiesAsync( this HttpClient http, string url, string cookies )
	{
		HttpRequestMessage request = new( HttpMethod.Get, url );
		request.Headers.Add( "Cookie", cookies );
		return await http.SendAsync( request );
	}

	public static async Task<HttpResponseMessage> PostWithCookiesAsync( this HttpClient http, string url, HttpContent content, string cookies )
	{
		HttpRequestMessage request = new( HttpMethod.Post, url );
		request.Headers.Add( "Cookie", cookies );
		request.Content = content;
		return await http.SendAsync( request );
	}

}
