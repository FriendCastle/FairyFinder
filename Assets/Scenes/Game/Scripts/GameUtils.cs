using System;

public static class GameUtils
{
	public static void TransitionTo<T1, T2>(this T1 yourClassInstance, T2 argToIndex) where T1 : ITransitioner where T2 : Enum
	{
		yourClassInstance.TransitionTo(Convert.ToInt32(argToIndex));
	}
}

