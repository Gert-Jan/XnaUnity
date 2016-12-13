//#define GAMEPAD_TESTING

using XnaWrapper;

namespace Microsoft.Xna.Framework.Input
{
	public static class GamePad
	{
#if !GAMEPAD_TESTING

#if U_XBOXONE
		private const int MAX_GAMEPAD_COUNT = 8;
#else
		private const int MAX_GAMEPAD_COUNT = 4;
#endif
		

		private static GamePadState[] prevStates;

		public static GamePadState GetState(PlayerIndex playerIndex)
		{
			if (!PlatformInstances.GamePad.GetState(playerIndex).IsConnected)
				return GamePadState.Default;

			// code for controlling overlays
			if (UnityEngine.Debug.isDebugBuild)
			{
				if (prevStates == null)
					prevStates = new GamePadState[MAX_GAMEPAD_COUNT];

				int index = (int)playerIndex;
				GamePadState prevState = prevStates[index];
				GamePadState newState = PlatformInstances.GamePad.GetState(playerIndex);
				if (newState.Triggers.Left > 0.5f && prevState.Triggers.Left <= 0.5f)
					PlatformInstances.LogOverlay = !PlatformInstances.LogOverlay;
				if (newState.Buttons.LeftShoulder == ButtonState.Pressed && prevState.Buttons.LeftShoulder == ButtonState.Released)
					PlatformInstances.InfoOverlay = !PlatformInstances.InfoOverlay;

				if (PlatformInstances.LogOverlay)
				{
					if (playerIndex == PlayerIndex.One)
					{
						PlatformInstances.LogToBottom = false;
						PlatformInstances.LogUp = false;
						PlatformInstances.LogDown = false;
					}

					PlatformInstances.LogToBottom = newState.Buttons.RightStick == ButtonState.Pressed | PlatformInstances.LogToBottom;
					PlatformInstances.LogUp = newState.ThumbSticks.Right.Y > 0.5f | PlatformInstances.LogUp;
					PlatformInstances.LogDown = newState.ThumbSticks.Right.Y < -0.5f | PlatformInstances.LogDown;

					// override the right stick 
					newState.ThumbSticks = new GamePadThumbSticks(newState.ThumbSticks.Left, new Vector2());
				}

				prevStates[index] = newState;
				return newState;
			}

			return PlatformInstances.GamePad.GetState(playerIndex);
		}
#else
		public static GamePadState GetState(PlayerIndex playerIndex)
		{
			GamePadState state = XnaWrapper.PlatformInstances.GamePad.GetState(playerIndex);
			XnaWrapper.Log.Buffer("Player" + ((int)playerIndex));
			print(ref state);
			if (playerIndex == PlayerIndex.Four)
				XnaWrapper.Log.Flush();
			else if (playerIndex == PlayerIndex.Two)
				XnaWrapper.Log.Buffer("\n");
			else
				XnaWrapper.Log.Buffer("         ");
			return state;
		}

		private static void print(ref GamePadState state)
		{
			XnaWrapper.Log.Buffer(" Btn:");
			printbutton(ref state, state.Buttons.A, Buttons.A, "A");
			printbutton(ref state, state.Buttons.B, Buttons.B, "B");
			printbutton(ref state, state.Buttons.X, Buttons.X, "X");
			printbutton(ref state, state.Buttons.Y, Buttons.Y, "Y");
			printbutton(ref state, state.Buttons.Start, Buttons.Start, "s");
			printbutton(ref state, state.Buttons.Back, Buttons.Back, "b");
			printbutton(ref state, state.Buttons.LeftShoulder, Buttons.LeftShoulder, "[");
			printbutton(ref state, state.Buttons.RightShoulder, Buttons.RightShoulder, "]");
			printbutton(ref state, state.Buttons.LeftStick, Buttons.LeftStick, "L");
			printbutton(ref state, state.Buttons.RightStick, Buttons.RightStick, "R");
			printbutton(ref state, state.DPad.Up, Buttons.DPadUp, "^");
			printbutton(ref state, state.DPad.Down, Buttons.DPadDown, "v");
			printbutton(ref state, state.DPad.Left, Buttons.DPadLeft, "<");
			printbutton(ref state, state.DPad.Right, Buttons.DPadRight, ">");
			XnaWrapper.Log.Buffer("   LT:" + getfloat(state.Triggers.Left));
			XnaWrapper.Log.Buffer(" RT:" + getfloat(state.Triggers.Right));
			XnaWrapper.Log.Buffer("   LX:" + getfloat(state.ThumbSticks.Left.X));
			XnaWrapper.Log.Buffer(" LY:" + getfloat(state.ThumbSticks.Left.Y));
			XnaWrapper.Log.Buffer(" RX:" + getfloat(state.ThumbSticks.Right.X));
			XnaWrapper.Log.Buffer(" RY:" + getfloat(state.ThumbSticks.Right.Y));
		}
		private static void printbutton(ref GamePadState state, ButtonState boolean, Buttons btn, string btnStr)
		{
			bool pressed = boolean == ButtonState.Pressed;
			if (state.IsButtonDown(btn) != pressed)
				XnaWrapper.Log.Buffer("?");
			else
				XnaWrapper.Log.Buffer(pressed ? btnStr : "_");
		}
		private static string getfloat(float value)
		{
			int intvalue = (int)(value * 50) + 50;
			if (intvalue == 100) intvalue = 99;
			else if (intvalue == 0) intvalue = 1;
			string str = intvalue.ToString();
			if (str.Length == 1)
				str = 0 + str;
			return str;
		}
#endif
	}
}
