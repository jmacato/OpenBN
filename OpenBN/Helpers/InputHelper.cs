using System;
using System.Threading;
using System.Linq;
using System.IO;
using System.Diagnostics;
using System.ComponentModel;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Media;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Audio;

namespace OpenBN
{
    /// <summary>
    /// Handles inputs with downpress duration and
    /// multiple simultaneous keypresses
    /// </summary>
    public class Inputs
    {
        public Dictionary<Keys, ExtraKeyState> KbStream = new Dictionary<Keys, ExtraKeyState>();

        public Keys[] MonitKeys;
        KeyboardState oldKeyboardState;

        public Inputs(Keys[] MonitoredKeys)
        {
            MonitKeys = MonitoredKeys;
            foreach (Keys x in MonitoredKeys)
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


            foreach (Keys u in oldKeyboardState.GetPressedKeys())
            {
                if (!KbStream.ContainsKey(u)) continue; 
                if (keyTrigger.IsKeyUp(u)) KbStream[u].KeyState = KeyState.Up;
                if (keyTrigger.IsKeyDown(u)) KbStream[u].KeyState = KeyState.Down;
            }

            foreach (Keys x in keyTrigger.GetPressedKeys())
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
            foreach(Keys y in x)
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

    public class ExtraKeyState
    {
        public KeyState KeyState { get; set; }
        public TimeSpan RegisterDuration { get; set; }
        public double DurDelta { get; set; }
        public double OldDelta { get; set; }

        public ExtraKeyState(KeyState k)
        {
            KeyState = k;
        }

    }
}
