using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace XnaWrapper.PlatformInterfaces
{
	public interface IGamePad
	{	
		GamePadState GetState(PlayerIndex playerIndex);
	}
}
