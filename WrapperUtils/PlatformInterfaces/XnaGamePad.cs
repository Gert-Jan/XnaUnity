using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System.Collections.Generic;

namespace XnaWrapper.PlatformInterfaces
{
	public abstract class XnaGamePad
	{
		public const int MAX_GAMEPADS = 4;

		public static XnaGamePad Instance;

		public abstract GamePadState GetState(PlayerIndex playerIndex);
	}
}
