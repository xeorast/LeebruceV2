namespace Leebruce.Api.Models;

public sealed record class FileDto(
	string FileName,
	string MediaType,
	Stream Content )
	: IDisposable
{
	public void Dispose()
	{
		( (IDisposable)Content ).Dispose();
		GC.SuppressFinalize( this );
	}
}
