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
using Microsoft.Xna.Framework.GamerServices;
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
        public Dictionary<Keys, ExtraKeyState> KeyboardStream = new Dictionary<Keys, ExtraKeyState>();

        public Keys[] MonitKeys;

        public Inputs(Keys[] MonitoredKeys)
        {
            MonitKeys = MonitoredKeys;
            foreach (Keys x in MonitoredKeys)
            {
                KeyboardStream.Add(x, new ExtraKeyState(KeyState.Up));
            }
        }

        KeyboardState oldKeyboardState;
        
        public KeyValuePair<Keys, ExtraKeyState>[] GetActiveKeys()
        {
          return KeyboardStream.Select(i => i)
                                    .Where(p => p.Value.KeyState == KeyState.Down)
                                    .ToArray();
        }

        public void Update(KeyboardState keyTrigger, GameTime gmt)
        {

            //Reset Buffer after 500ms
            if (gmt.TotalGameTime.Milliseconds%1000 == 0)
            {
                InputHandled(MonitKeys);
            }

            //Set oldkbdstate to initial value
            if (oldKeyboardState == null) { oldKeyboardState = keyTrigger; return; }

            foreach (Keys u in oldKeyboardState.GetPressedKeys())
            {
                if (!KeyboardStream.ContainsKey(u)) continue; 
                if (keyTrigger.IsKeyUp(u)) KeyboardStream[u].KeyState = KeyState.Up;
                if (keyTrigger.IsKeyDown(u)) KeyboardStream[u].KeyState = KeyState.Down;
            }

            foreach (Keys x in keyTrigger.GetPressedKeys())
            {
                if (KeyboardStream.ContainsKey(x)) //Check if it is in the monitored keys list
                {
                    var y = KeyboardStream[x];
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
                if (KeyboardStream.ContainsKey(y)) //Check if it is in the monitored keys list
                {
                    KeyboardStream[y].KeyState = KeyState.Down;
                    KeyboardStream[y].RegisterDuration = DateTime.Now.TimeOfDay;
                    KeyboardStream[y].OldDelta = 0;
                    KeyboardStream[y].DurDelta = 0;
                }
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
