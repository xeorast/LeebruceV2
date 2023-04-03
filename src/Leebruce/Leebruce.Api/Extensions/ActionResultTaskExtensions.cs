using Microsoft.AspNetCore.Mvc;

namespace Leebruce.Api.Extensions;

public static class ActionResultTaskExtensions
{
	public static async ValueTask<ActionResult<T>> WithMappedMaintenanceBreak<T>( this Task<T> task, Func<string, ObjectResult> serviceUnavailableFactory )
	{
		try
		{
			return await task;
		}
		catch ( MaintenanceBreakException )
		{
			return serviceUnavailableFactory( "Can not contact Lb as it is undergoing maintenance." );
		}
	}

	public static async ValueTask<ActionResult<T>> WithMappedUnauthorized<T>( this ValueTask<ActionResult<T>> task, Func<string, ObjectResult> unauthorizedFactory )
	{
		try
		{
			return await task;
		}
		catch ( NotAuthorizedException )
		{
			return unauthorizedFactory( "Session has expired." );
		}
	}

}
