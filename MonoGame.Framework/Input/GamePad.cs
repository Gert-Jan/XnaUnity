namespace Microsoft.Xna.Framework.Input
{
	public static class GamePad
	{
		public static GamePadState GetState(PlayerIndex playerIndex)
		{
			GamePadState state = XnaWrapper.PlatformInstances.GamePad.GetState(playerIndex);
			string dpad = "";
			if (state.DPad.Up == ButtonState.Pressed)
				dpad += "Up ";
			if (state.DPad.Right == ButtonState.Pressed)
				dpad += "Right ";
			if (state.DPad.Down == ButtonState.Pressed)
				dpad += "Down ";
			if (state.DPad.Left == ButtonState.Pressed)
				dpad += "Left ";
			if (!string.IsNullOrEmpty(dpad))
				XnaWrapper.Log.WriteT(dpad);
			return state;
        }

		//private struct ButtonMapStruct
		//{
		//	public KeyCode UnityKey;
		//	public Buttons MonoKey;
		//
		//	public ButtonMapStruct(KeyCode UnityKey, Buttons MonoKey)
		//	{
		//		this.UnityKey = UnityKey;
		//		this.MonoKey = MonoKey;
		//	}
		//}
		//static ButtonMapStruct[][] buttonMapping;
		//
		//private class GamePadInput
		//{
		//	public List<Buttons> buttons;
		//	public GamePadThumbSticks thumbSticks;
		//	public GamePadTriggers triggers;
		//	public GamePadDPad dPad;
		//
		//	public GamePadInput()
		//	{
		//		buttons = new List<Buttons>();
		//		thumbSticks = new GamePadThumbSticks();
		//		triggers = new GamePadTriggers();
		//		dPad = new GamePadDPad();
		//	}
		//}
		//static GamePadInput[] pressedInput;
		//
		//static GamePad()
		//{
		//	const UInt16 maxGamePads = (UInt16)PlayerIndex.Four;
		//	buttonMapping = new ButtonMapStruct[maxGamePads + 1][];
		//	pressedInput = new GamePadInput[maxGamePads + 1];
		//
		//	for (int i = 0; i <= maxGamePads; i++)
		//	{
		//		buttonMapping[i] = new ButtonMapStruct[10];
		//		pressedInput[i] = new GamePadInput();
		//	}
		//
		//	buttonMapping[0][0] = new ButtonMapStruct(KeyCode.Joystick1Button0, Buttons.A);
		//	buttonMapping[0][1] = new ButtonMapStruct(KeyCode.Joystick1Button1, Buttons.B);
		//	buttonMapping[0][2] = new ButtonMapStruct(KeyCode.Joystick1Button2, Buttons.X);
		//	buttonMapping[0][3] = new ButtonMapStruct(KeyCode.Joystick1Button3, Buttons.Y);
		//	buttonMapping[0][4] = new ButtonMapStruct(KeyCode.Joystick1Button4, Buttons.LeftShoulder);
		//	buttonMapping[0][5] = new ButtonMapStruct(KeyCode.Joystick1Button5, Buttons.RightShoulder);
		//	buttonMapping[0][6] = new ButtonMapStruct(KeyCode.Joystick1Button6, Buttons.Back);
		//	buttonMapping[0][7] = new ButtonMapStruct(KeyCode.Joystick1Button7, Buttons.Start);
		//	buttonMapping[0][8] = new ButtonMapStruct(KeyCode.Joystick1Button8, Buttons.LeftStick);
		//	buttonMapping[0][9] = new ButtonMapStruct(KeyCode.Joystick1Button9, Buttons.RightStick);
		//
		//	buttonMapping[1][0] = new ButtonMapStruct(KeyCode.Joystick2Button0, Buttons.A);
		//	buttonMapping[1][1] = new ButtonMapStruct(KeyCode.Joystick2Button1, Buttons.B);
		//	buttonMapping[1][2] = new ButtonMapStruct(KeyCode.Joystick2Button2, Buttons.X);
		//	buttonMapping[1][3] = new ButtonMapStruct(KeyCode.Joystick2Button3, Buttons.Y);
		//	buttonMapping[1][4] = new ButtonMapStruct(KeyCode.Joystick2Button4, Buttons.LeftShoulder);
		//	buttonMapping[1][5] = new ButtonMapStruct(KeyCode.Joystick2Button5, Buttons.RightShoulder);
		//	buttonMapping[1][6] = new ButtonMapStruct(KeyCode.Joystick2Button6, Buttons.Back);
		//	buttonMapping[1][7] = new ButtonMapStruct(KeyCode.Joystick2Button7, Buttons.Start);
		//	buttonMapping[1][8] = new ButtonMapStruct(KeyCode.Joystick2Button8, Buttons.LeftStick);
		//	buttonMapping[1][9] = new ButtonMapStruct(KeyCode.Joystick2Button9, Buttons.RightStick);
		//
		//	buttonMapping[2][0] = new ButtonMapStruct(KeyCode.Joystick3Button0, Buttons.A);
		//	buttonMapping[2][1] = new ButtonMapStruct(KeyCode.Joystick3Button1, Buttons.B);
		//	buttonMapping[2][2] = new ButtonMapStruct(KeyCode.Joystick3Button2, Buttons.X);
		//	buttonMapping[2][3] = new ButtonMapStruct(KeyCode.Joystick3Button3, Buttons.Y);
		//	buttonMapping[2][4] = new ButtonMapStruct(KeyCode.Joystick3Button4, Buttons.LeftShoulder);
		//	buttonMapping[2][5] = new ButtonMapStruct(KeyCode.Joystick3Button5, Buttons.RightShoulder);
		//	buttonMapping[2][6] = new ButtonMapStruct(KeyCode.Joystick3Button6, Buttons.Back);
		//	buttonMapping[2][7] = new ButtonMapStruct(KeyCode.Joystick3Button7, Buttons.Start);
		//	buttonMapping[2][8] = new ButtonMapStruct(KeyCode.Joystick3Button8, Buttons.LeftStick);
		//	buttonMapping[2][9] = new ButtonMapStruct(KeyCode.Joystick3Button9, Buttons.RightStick);
		//
		//	buttonMapping[3][0] = new ButtonMapStruct(KeyCode.Joystick4Button0, Buttons.A);
		//	buttonMapping[3][1] = new ButtonMapStruct(KeyCode.Joystick4Button1, Buttons.B);
		//	buttonMapping[3][2] = new ButtonMapStruct(KeyCode.Joystick4Button2, Buttons.X);
		//	buttonMapping[3][3] = new ButtonMapStruct(KeyCode.Joystick4Button3, Buttons.Y);
		//	buttonMapping[3][4] = new ButtonMapStruct(KeyCode.Joystick4Button4, Buttons.LeftShoulder);
		//	buttonMapping[3][5] = new ButtonMapStruct(KeyCode.Joystick4Button5, Buttons.RightShoulder);
		//	buttonMapping[3][6] = new ButtonMapStruct(KeyCode.Joystick4Button6, Buttons.Back);
		//	buttonMapping[3][7] = new ButtonMapStruct(KeyCode.Joystick4Button7, Buttons.Start);
		//	buttonMapping[3][8] = new ButtonMapStruct(KeyCode.Joystick4Button8, Buttons.LeftStick);
		//	buttonMapping[3][9] = new ButtonMapStruct(KeyCode.Joystick4Button9, Buttons.RightStick);
		//}
		//
		//public static GamePadState GetState(PlayerIndex playerIndex)
		//{
		//	int index = (int)playerIndex;
		//	string playerGamePad = GAMEPAD + (index + 1); //inputmanager names from 1-4 because joystick0
		//	pressedInput[index].buttons.Clear();
		//	
		//	//Button
		//	for (int i = 0; i < 10; i++)
		//	{
		//		KeyCode key = buttonMapping[index][i].UnityKey;
		//		if (key != KeyCode.None && UnityEngine.Input.GetKey(key))
		//		{
		//			pressedInput[index].buttons.Add(buttonMapping[index][i].MonoKey);
		//		}
		//	}
		//	
		//	//Axis Thumsticks
		//	pressedInput[index].thumbSticks = new GamePadThumbSticks(new Vector2(GetAxis(playerGamePad + "LeftAX"), -GetAxis(playerGamePad + "LeftAY")),
		//		new Vector2(GetAxis(playerGamePad + "RightAX"), -GetAxis(playerGamePad + "RightAY")));
		//	
		//	//Axis Triggers
		//	pressedInput[index].triggers = new GamePadTriggers(GetAxis(playerGamePad + "TriggerLeft"), GetAxis(playerGamePad + "TriggerRight"));
		//	
		//	//Axis DPad
		//	ButtonState[] states = GetDPadButtonsState(playerGamePad + "DpadX", playerGamePad + "DpadY");
		//	pressedInput[index].dPad = new GamePadDPad(states[0], states[1], states[2], states[3]);
		//	
		//	if (states[0] == ButtonState.Pressed)
		//		pressedInput[index].buttons.Add(Buttons.DPadUp);
		//	if (states[1] == ButtonState.Pressed)
		//		pressedInput[index].buttons.Add(Buttons.DPadDown);
		//	if (states[2] == ButtonState.Pressed)
		//		pressedInput[index].buttons.Add(Buttons.DPadLeft);
		//	if (states[3] == ButtonState.Pressed)
		//		pressedInput[index].buttons.Add(Buttons.DPadRight);
		//	
		//	return new GamePadState(pressedInput[index].thumbSticks,
		//		pressedInput[index].triggers,
		//		new GamePadButtons(pressedInput[index].buttons.ToArray()),
		//		pressedInput[index].dPad);
		//}
		//
		//private static float GetAxis(string axisName)
		//{
		//	return UnityEngine.Input.GetAxis(axisName);
		//}
		//
		//private static ButtonState[] GetDPadButtonsState(string xAxisName, string yAxisName)
		//{
		//	ButtonState[] state = new ButtonState[4];
		//
		//	float pressedValue = 0.8f;
		//	state[0] = GetAxisPressedState(yAxisName, pressedValue);  //up
		//	state[1] = GetAxisPressedState(yAxisName, -pressedValue);   //down
		//	state[2] = GetAxisPressedState(xAxisName, -pressedValue);  //left
		//	state[3] = GetAxisPressedState(xAxisName, pressedValue);   //right
		//
		//	return state;
		//}
		//
		//private static ButtonState GetAxisPressedState(string axisName, float pressedValue)
		//{
		//	float axis = GetAxis(axisName);
		//	if (pressedValue < 0)
		//	{
		//		if (axis <= pressedValue)
		//			return ButtonState.Pressed;
		//	}
		//	else
		//	{
		//		if (axis >= pressedValue)
		//			return ButtonState.Pressed;
		//	}
		//
		//	return ButtonState.Released;
		//}
	}
}
