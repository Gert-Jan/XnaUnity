namespace Microsoft.Xna.Framework.Input
{
	public static class GamePad
	{
		public static GamePadState GetState(PlayerIndex playerIndex)
		{
			return XnaWrapper.PlatformInstances.GamePad.GetState(playerIndex);
		}
	}
}
