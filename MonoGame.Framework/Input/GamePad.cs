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
        /*static KeyCode[][] buttonMapping;

        private struct GamePadInput
        {
            public List<Buttons> buttons = new List<Buttons>();
            public GamePadThumbSticks thumbSticks;
            public GamePadTriggers triggers;
            public GamePadDPad dPad;
        }
        static GamePadInput[] pressedInput;
        */
        static GamePad()
		{
            /*const UInt16 maxGamePads = (UInt16)PlayerIndex.Four;

            buttonMapping = new KeyCode[maxGamePads][];
            pressedInput = new GamePadInput[maxGamePads];

            for (int i = 0; i < maxGamePads; i++)
            {
                buttonMapping[i] = new KeyCode[19];
            }

            buttonMapping[0][(int)Buttons.A] = KeyCode.Joystick1Button0;
            buttonMapping[0][(int)Buttons.B] = KeyCode.Joystick1Button1;
            buttonMapping[0][(int)Buttons.X] = KeyCode.Joystick1Button2;
            buttonMapping[0][(int)Buttons.Y] = KeyCode.Joystick1Button3;
            buttonMapping[0][(int)Buttons.LeftShoulder] = KeyCode.Joystick1Button4;
            buttonMapping[0][(int)Buttons.RightShoulder] = KeyCode.Joystick1Button5;
            buttonMapping[0][(int)Buttons.Back] = KeyCode.Joystick1Button6;
            buttonMapping[0][(int)Buttons.Start] = KeyCode.Joystick1Button7;
            buttonMapping[0][(int)Buttons.LeftStick] = KeyCode.Joystick1Button8;
            buttonMapping[0][(int)Buttons.RightStick] = KeyCode.Joystick1Button9;

            buttonMapping[1][(int)Buttons.A] = KeyCode.Joystick1Button0;
            buttonMapping[1][(int)Buttons.B] = KeyCode.Joystick1Button1;
            buttonMapping[1][(int)Buttons.X] = KeyCode.Joystick1Button2;
            buttonMapping[1][(int)Buttons.Y] = KeyCode.Joystick1Button3;
            buttonMapping[1][(int)Buttons.LeftShoulder] = KeyCode.Joystick1Button4;
            buttonMapping[1][(int)Buttons.RightShoulder] = KeyCode.Joystick1Button5;
            buttonMapping[1][(int)Buttons.Back] = KeyCode.Joystick1Button6;
            buttonMapping[1][(int)Buttons.Start] = KeyCode.Joystick1Button7;
            buttonMapping[1][(int)Buttons.LeftStick] = KeyCode.Joystick1Button8;
            buttonMapping[1][(int)Buttons.RightStick] = KeyCode.Joystick1Button9;

            buttonMapping[2][(int)Buttons.A] = KeyCode.Joystick1Button0;
            buttonMapping[2][(int)Buttons.B] = KeyCode.Joystick1Button1;
            buttonMapping[2][(int)Buttons.X] = KeyCode.Joystick1Button2;
            buttonMapping[2][(int)Buttons.Y] = KeyCode.Joystick1Button3;
            buttonMapping[2][(int)Buttons.LeftShoulder] = KeyCode.Joystick1Button4;
            buttonMapping[2][(int)Buttons.RightShoulder] = KeyCode.Joystick1Button5;
            buttonMapping[2][(int)Buttons.Back] = KeyCode.Joystick1Button6;
            buttonMapping[2][(int)Buttons.Start] = KeyCode.Joystick1Button7;
            buttonMapping[2][(int)Buttons.LeftStick] = KeyCode.Joystick1Button8;
            buttonMapping[2][(int)Buttons.RightStick] = KeyCode.Joystick1Button9;

            buttonMapping[3][(int)Buttons.A] = KeyCode.Joystick1Button0;
            buttonMapping[3][(int)Buttons.B] = KeyCode.Joystick1Button1;
            buttonMapping[3][(int)Buttons.X] = KeyCode.Joystick1Button2;
            buttonMapping[3][(int)Buttons.Y] = KeyCode.Joystick1Button3;
            buttonMapping[3][(int)Buttons.LeftShoulder] = KeyCode.Joystick1Button4;
            buttonMapping[3][(int)Buttons.RightShoulder] = KeyCode.Joystick1Button5;
            buttonMapping[3][(int)Buttons.Back] = KeyCode.Joystick1Button6;
            buttonMapping[3][(int)Buttons.Start] = KeyCode.Joystick1Button7;
            buttonMapping[3][(int)Buttons.LeftStick] = KeyCode.Joystick1Button8;
            buttonMapping[3][(int)Buttons.RightStick] = KeyCode.Joystick1Button9;*/
        }

        public static GamePadState GetState(PlayerIndex playerIndex)
        {
            /*int index = (int)playerIndex;
            pressedInput[index].buttons.Clear();
            
            //Button
            for (int i = 0; i < buttonMapping[index].Length; i++)
            {
                if (buttonMapping[index][i] != KeyCode.None && UnityEngine.Input.GetKey(buttonMapping[index][i]))
                {
                    pressedInput[index].buttons.Add((Buttons)i);
                }
            }

            //Axis Thumsticks
            pressedInput[index].thumbSticks = new GamePadThumbSticks(new Vector2(GetAxis("LeftX"), GetAxis("LeftY")), new Vector2(GetAxis("RightX"), GetAxis("RightY")));

            //Axis Triggers
            pressedInput[index].triggers = new GamePadTriggers(UnityEngine.Input.GetAxis("LeftAxis"), UnityEngine.Input.GetAxis("RightAxis"));

            //Axis DPad
            ButtonState[] states = GetDPadButtonsState("DPadX", "DPadY");
            pressedInput[index].dPad = new GamePadDPad(states[0], states[1], states[2], states[3]);

            return new GamePadState(pressedInput[index].thumbSticks,
                pressedInput[index].triggers,
                new GamePadButtons(pressedInput[index].buttons.ToArray()),
                pressedInput[index].dPad);
             */
            return new GamePadState();
        }    
        
        private static float GetAxis(string axisName)
        {
            //return UnityEngine.Input.GetAxis(axisName);
            return 0;
        }

        private static ButtonState[] GetDPadButtonsState(string xAxisName, string yAxisName)
        {
            ButtonState[] state = new ButtonState[4];
            
            state[0] = GetAxisPressedState(xAxisName, -1);  //up
            state[1] = GetAxisPressedState(xAxisName, 1);   //down
            state[2] = GetAxisPressedState(yAxisName, -1);  //left
            state[3] = GetAxisPressedState(yAxisName, 1);   //right

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
