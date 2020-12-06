using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace OpenBN.Helpers
{
    /// <summary>
    /// Handles inputs with downpress duration and
    /// multiple simultaneous keypresses
    /// </summary>
    public class Inputs
    {
        public Dictionary<Keys, ExtraKeyState> KbStream = new();

        public Keys[] MonitKeys;
        KeyboardState oldKeyboardState;

        public Inputs(Keys[] MonitoredKeys)
        {
            MonitKeys = MonitoredKeys;
            foreach (var x in MonitoredKeys)
            {
                KbStream.Add(x, new ExtraKeyState(KeyState.Up));
            }
        }
        
        public KeyValuePair<Keys, ExtraKeyState>[] GetActiveKeys()
        {
          return KbStream.Select(i => i)
                                    .Where(p => p.Value.KeyState == KeyState.Down)
                                    .ToArray();
        }
        
        public bool Halt {get; set;}

        public void Update(KeyboardState keyTrigger, GameTime gmt)
        {
            
            if (Halt) return;


            foreach (var u in oldKeyboardState.GetPressedKeys())
            {
                if (!KbStream.ContainsKey(u)) continue; 
                if (keyTrigger.IsKeyUp(u)) KbStream[u].KeyState = KeyState.Up;
                if (keyTrigger.IsKeyDown(u)) KbStream[u].KeyState = KeyState.Down;
            }

            foreach (var x in keyTrigger.GetPressedKeys())
            {
                if (KbStream.ContainsKey(x)) //Check if it is in the monitored keys list
                {
                    var y = KbStream[x];
                    if (y.KeyState == KeyState.Up)
                    {
                        y.KeyState = KeyState.Down;
                        y.RegisterDuration = DateTime.Now.TimeOfDay;
                        y.OldDelta = y.DurDelta;
                        y.DurDelta = 0;
                        continue;
                    }
                    else if (y.KeyState == KeyState.Down)
                    {
                        y.DurDelta = DateTime.Now.TimeOfDay.TotalMilliseconds - y.RegisterDuration.TotalMilliseconds;
                        continue;
                    }
                }
            }

            oldKeyboardState = keyTrigger;
        }

        public void InputHandled(Keys[] x)
        {
            foreach(var y in x)
            {
                if (KbStream.ContainsKey(y)) //Check if it is in the monitored keys list
                {
                    KbStream[y].KeyState = KeyState.Down;
                    KbStream[y].RegisterDuration = DateTime.Now.TimeOfDay;
                    KbStream[y].OldDelta = 0;
                    KbStream[y].DurDelta = 0;
                }
            }
        }

        public bool KeyStateToBool(KeyState x)
        {
            switch (x)
            {
                case KeyState.Down: return true;
                case KeyState.Up: return false;
                default: throw new InvalidCastException();
            }
        }

    }
}
