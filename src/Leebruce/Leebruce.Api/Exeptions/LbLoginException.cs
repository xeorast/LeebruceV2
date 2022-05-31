namespace Leebruce.Api.Exeptions;


[Serializable]
public class LbLoginException : Exception
{
	public LbLoginException() { }
	public LbLoginException( string message ) : base( message ) { }
	public LbLoginException( string message, Exception inner ) : base( message, inner ) { }
	protected LbLoginException(
	  System.Runtime.Serialization.SerializationInfo info,
	  System.Runtime.Serialization.StreamingContext context ) : base( info, context ) { }
}
