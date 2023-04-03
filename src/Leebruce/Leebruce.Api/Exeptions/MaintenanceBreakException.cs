namespace Leebruce.Api.Exeptions;


[Serializable]
public class MaintenanceBreakException : Exception
{
	public MaintenanceBreakException() { }
	public MaintenanceBreakException( string message ) : base( message ) { }
	public MaintenanceBreakException( string message, Exception inner ) : base( message, inner ) { }
	protected MaintenanceBreakException(
	  System.Runtime.Serialization.SerializationInfo info,
	  System.Runtime.Serialization.StreamingContext context ) : base( info, context ) { }
}
