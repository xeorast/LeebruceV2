using System.Security.Claims;

namespace Leebruce.Api.Models;

public record struct LbAuthData(
	string DziennikSid,
	string SdziennikSid,
	string OAuthToken )
{
	public static LbAuthData FromClaims( IEnumerable<Claim> claims )
	{
		string? dziennikSid = claims.FirstOrDefault( c => c.Type == nameof( DziennikSid ) )?.Value
			?? throw new ArgumentException( @$"Incomplete claims, no ""{nameof( DziennikSid )}"" claim found.", nameof( claims ) );

		string? sdziennikSid = claims.FirstOrDefault( c => c.Type == nameof( SdziennikSid ) )?.Value
			?? throw new ArgumentException( @$"Incomplete claims, no ""{nameof( SdziennikSid )}"" claim found.", nameof( claims ) );

		string? oAuthToken = claims.FirstOrDefault( c => c.Type == nameof( OAuthToken ) )?.Value
			?? throw new ArgumentException( @$"Incomplete claims, no ""{nameof( OAuthToken )}"" claim found.", nameof( claims ) );

		return new( dziennikSid, sdziennikSid, oAuthToken );
	}

	public Claim[] ToClaimsArray()
	{
		return new Claim[]
		{
			new( nameof( DziennikSid ), DziennikSid ),
			new( nameof( SdziennikSid ), SdziennikSid ),
			new( nameof( OAuthToken ), OAuthToken ),
		};
	}
}
