﻿namespace Leebruce.Api.Exeptions;


[Serializable]
public class ProcessingException : Exception
{
	public ProcessingException() { }
	public ProcessingException( string message ) : base( message ) { }
	public ProcessingException( string message, Exception inner ) : base( message, inner ) { }
	protected ProcessingException(
	  System.Runtime.Serialization.SerializationInfo info,
	  System.Runtime.Serialization.StreamingContext context ) : base( info, context ) { }
}
