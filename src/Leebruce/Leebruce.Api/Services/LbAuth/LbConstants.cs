namespace Leebruce.Api.Services.LbAuth;

static class LbConstants
{
	public const string dsidName = "DZIENNIKSID";
	public const string sdsidName = "SDZIENNIKSID";
	public const string oatokenName = "oauth_token";
	public static readonly Uri lbCookiesDomain = new( "https://synergia.librus.pl" );
}
