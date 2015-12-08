//#define TESTING

namespace Microsoft.Xna.Framework.Input
{
	public static class GamePad
	{
#if !TESTING
		public static GamePadState GetState(PlayerIndex playerIndex)
		{
			return XnaWrapper.PlatformInstances.GamePad.GetState(playerIndex);
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
