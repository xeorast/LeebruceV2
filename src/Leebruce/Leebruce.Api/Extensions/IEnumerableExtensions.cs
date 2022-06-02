namespace Leebruce.Api.Extensions;

public static class IEnumerableExtensions
{
	public static T[] TrimStart<T>( this T[] arr, T value )
	{
		var firstNotV = 0;

		while ( Equals( arr[firstNotV], value ) )
		{
			++firstNotV;
		}
		return arr[firstNotV..];
	}
	public static T[] TrimEnd<T>( this T[] arr, T value )
	{
		var lastNotV = arr.Length - 1;

		while ( Equals( arr[lastNotV], value ) )
		{
			--lastNotV;
		}
		return arr[..( lastNotV + 1 )];
	}
	public static T[] Trim<T>( this T[] arr, T value )
	{
		var firstNotV = 0;
		var lastNotV = arr.Length - 1;

		while ( Equals( arr[firstNotV], value ) )
		{
			++firstNotV;
		}
		while ( Equals( arr[lastNotV], value ) )
		{
			--lastNotV;
		}
		return arr[firstNotV..( lastNotV + 1 )];
	}
	public static T[][] Transpose<T>( this T[][] arr )
	{
		var colLen = arr.Length;
		var rowLen = arr[0].Length;

		// validate
		foreach ( var row in arr )
		{
			if ( row.Length != rowLen )
			{
				throw new ArgumentException( "Array rows were not same length", nameof( arr ) );
			}
		}

		// prepare empty
		var ret = new T[rowLen][];
		ret.Fill( () => new T[colLen] );

		// copy
		for ( int col = 0; col < rowLen; col++ )
		{
			for ( int row = 0; row < colLen; row++ )
			{
				ret[col][row] = arr[row][col];
			}
		}
		return ret;
	}
	public static void Fill<T>( this T[] arr, Func<T> factory )
	{
		for ( int i = 0; i < arr.Length; i++ )
		{
			arr[i] = factory();
		}
	}

	public static IEnumerable<T> TrimStart<T>( this IEnumerable<T> s, T value )
	{
		using var e = s.GetEnumerator();
		while ( e.MoveNext() && Equals( e.Current, value ) )
		{
		}
		do
		{
			yield return e.Current;
		} while ( e.MoveNext() );
	}
	public static IEnumerable<T> TrimEnd<T>( this IEnumerable<T> s, T value )
	{
		int cnt = 0;
		foreach ( var item in s )
		{
			if ( Equals( item, value ) )
			{
				++cnt;
			}
			else
			{
				while ( cnt > 0 )
				{
					--cnt;
					yield return value;
				}
				yield return item;
			}

		}
	}
	public static IEnumerable<T> Trim<T>( this IEnumerable<T> s, T value )
	{
		return s.TrimStart( value ).TrimEnd( value );
	}

}
