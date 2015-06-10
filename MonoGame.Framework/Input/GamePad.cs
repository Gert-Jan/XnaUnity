using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Microsoft.Xna.Framework.Input;

//Joystick xbox 360 layout: www.visualstudiogallery.msdn.microsoft.com

namespace Microsoft.Xna.Framework.Input
{
    public static class GamePad
    {
        private struct ButtonMapStruct
        {
            public KeyCode UnityKey;
            public Buttons MonoKey;

            public ButtonMapStruct(KeyCode UnityKey, Buttons MonoKey)
            {
                this.UnityKey = UnityKey;
                this.MonoKey = MonoKey;
            }
        }
        static ButtonMapStruct[][] buttonMapping;

        private class GamePadInput
        {
            public List<Buttons> buttons;
            public GamePadThumbSticks thumbSticks;
            public GamePadTriggers triggers;
            public GamePadDPad dPad;
           
            public GamePadInput()
            {
                buttons = new List<Buttons>();
                thumbSticks = new GamePadThumbSticks();
                triggers = new GamePadTriggers();
                dPad = new GamePadDPad();
            }
        }
        static GamePadInput[] pressedInput;
        
        static GamePad()
		{
            const UInt16 maxGamePads = (UInt16)PlayerIndex.Four;
            buttonMapping = new ButtonMapStruct[maxGamePads + 1][];
            pressedInput = new GamePadInput[maxGamePads + 1];

            for (int i = 0; i <= maxGamePads; i++)
            {
                buttonMapping[i] = new ButtonMapStruct[10];
                pressedInput[i] = new GamePadInput();
            }

            for (int i = 0; i <= maxGamePads; i++)
            {
                buttonMapping[i][0] = new ButtonMapStruct(KeyCode.Joystick1Button0, Buttons.A);
                buttonMapping[i][1] = new ButtonMapStruct(KeyCode.Joystick1Button1, Buttons.B);
                buttonMapping[i][2] = new ButtonMapStruct(KeyCode.Joystick1Button2, Buttons.X);
                buttonMapping[i][3] = new ButtonMapStruct(KeyCode.Joystick1Button3, Buttons.Y);
                buttonMapping[i][4] = new ButtonMapStruct(KeyCode.Joystick1Button4, Buttons.LeftShoulder);
                buttonMapping[i][5] = new ButtonMapStruct(KeyCode.Joystick1Button5, Buttons.RightShoulder);
                buttonMapping[i][6] = new ButtonMapStruct(KeyCode.Joystick1Button6, Buttons.Back);
                buttonMapping[i][7] = new ButtonMapStruct(KeyCode.Joystick1Button7, Buttons.Start);
                buttonMapping[i][8] = new ButtonMapStruct(KeyCode.Joystick1Button8, Buttons.LeftStick);
                buttonMapping[i][9] = new ButtonMapStruct(KeyCode.Joystick1Button9, Buttons.RightStick);
            }
        }

        public static GamePadState GetState(PlayerIndex playerIndex)
        {
            int index = (int)playerIndex;
            pressedInput[index].buttons.Clear();
            
            //Button
            for (int i = 0; i < 10; i++)
            {
                KeyCode key = buttonMapping[index][i].UnityKey;
                if (key != KeyCode.None && UnityEngine.Input.GetKey(key))
                {
                    pressedInput[index].buttons.Add(buttonMapping[index][i].MonoKey);
                }
            }

            //Axis Thumsticks
            pressedInput[index].thumbSticks = new GamePadThumbSticks(new Vector2(UnityEngine.Input.GetAxis("Joystick1LeftAX"), -UnityEngine.Input.GetAxis("Joystick1LeftAY")),
                new Vector2(UnityEngine.Input.GetAxis("Joystick1RightAX"), -UnityEngine.Input.GetAxis("Joystick1RightAY")));

            //Axis Triggers
            pressedInput[index].triggers = new GamePadTriggers(UnityEngine.Input.GetAxis("Joystick1TriggerLeft"), UnityEngine.Input.GetAxis("Joystick1TriggerRight"));
            
            //Axis DPad
            ButtonState[] states = GetDPadButtonsState("Joystick1DpadX", "Joystick1DpadY");
            pressedInput[index].dPad = new GamePadDPad(states[0], states[1], states[2], states[3]);
            
            if (states[0] == ButtonState.Pressed)
                pressedInput[index].buttons.Add(Buttons.DPadUp);
            if (states[1] == ButtonState.Pressed)
                pressedInput[index].buttons.Add(Buttons.DPadDown);
            if (states[2] == ButtonState.Pressed)
                pressedInput[index].buttons.Add(Buttons.DPadLeft);
            if (states[3] == ButtonState.Pressed)
                pressedInput[index].buttons.Add(Buttons.DPadRight);

            return new GamePadState(pressedInput[index].thumbSticks,
                pressedInput[index].triggers,
                new GamePadButtons(pressedInput[index].buttons.ToArray()),
                pressedInput[index].dPad);
        }    
        
        private static float GetAxis(string axisName)
        {
            return UnityEngine.Input.GetAxis(axisName);
        }

        private static ButtonState[] GetDPadButtonsState(string xAxisName, string yAxisName)
        {
            ButtonState[] state = new ButtonState[4];

            state[0] = GetAxisPressedState(yAxisName, 1);  //up
            state[1] = GetAxisPressedState(yAxisName, -1);   //down
            state[2] = GetAxisPressedState(xAxisName, -1);  //left
            state[3] = GetAxisPressedState(xAxisName, 1);   //right

            return state;
        }

        private static ButtonState GetAxisPressedState(string axisName, float pressedValue)
        {
             if (GetAxis(axisName) == pressedValue)
                return ButtonState.Pressed;

            return ButtonState.Released;
        }
    }
}
