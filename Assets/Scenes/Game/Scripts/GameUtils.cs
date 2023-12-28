using System;

// Utils script to be used for general shared functions for the Game Scene
public static class GameUtils
{
	// Extension for ITransitioner to make transitioning between different screens/panels more explicit with enums
	public static void TransitionTo<T1, T2>(this T1 yourClassInstance, T2 argToIndex) where T1 : ITransitioner where T2 : Enum
	{
		yourClassInstance.TransitionTo(Convert.ToInt32(argToIndex));
	}
}

