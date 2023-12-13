using System;

public static class GameUtils
{
	public static void TransitionFromTo<T1, T2>(this T1 yourClassInstance, T2 argFromIndex, T2 argToIndex) where T1 : ITransitioner where T2 : Enum
	{
		yourClassInstance.TransitionFromTo(Convert.ToInt32(argFromIndex), Convert.ToInt32(argToIndex));
	}
}

