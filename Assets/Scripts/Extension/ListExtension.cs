using System.Collections.Generic;
using System.Linq;

public static class ListExtension 
{
	public static bool IsNullOrEmpty<T>(this IEnumerable<T> enumerable)
	{
		if(enumerable == null)
		{
			return true;
		}

		return !enumerable.Any();
	}
}