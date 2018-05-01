using UnityEngine;

namespace Microsoft.Xna.Framework.Input
{
	/// <summary>
	/// A mouse implementation based on touch input
	/// </summary>
	public static class Mouse
	{

#if U_SWITCH
		private const int SWITCH_HANDHELD_WIDTH = 1280;
		private const int SWITCH_HANDHELD_HEIGHT = 720;
#endif

		public static MouseState GetState()
		{
			MouseState state = new MouseState();

			if (UnityEngine.Input.touchCount > 0)
			{
				UnityEngine.Touch touchState = UnityEngine.Input.GetTouch(0);

				if (touchState.phase == TouchPhase.Began || touchState.phase == TouchPhase.Stationary || touchState.phase == TouchPhase.Moved)
					state.LeftButton = ButtonState.Pressed;
				else
					state.LeftButton = ButtonState.Released;

#if U_SWITCH
				float xScale = (float)UnityEngine.Screen.width / (float)SWITCH_HANDHELD_WIDTH;
				float yScale = (float)UnityEngine.Screen.height / (float)SWITCH_HANDHELD_HEIGHT;

				state.X = (int)(touchState.position.x * xScale);
				state.Y = UnityEngine.Screen.height - (int)(touchState.position.y * yScale); // Switch has inverted the y-axis on the screen(720 is top, 0 is bottom)
#else
				// This might be wrong, depending on touch device and native resolution
				state.X = (int)touchState.position.x;
				state.Y = (int)touchState.position.y;
#endif
			}
			else
			{
				state.LeftButton = ButtonState.Released;
				state.X = 0;
				state.Y = 0;
			}

			state.RightButton = ButtonState.Released;
			state.MiddleButton = ButtonState.Released;
			state.ScrollWheelValue = 0;
			state.XButton1 = ButtonState.Released;
			state.XButton2 = ButtonState.Released;

			return state;
		}
	}
}
