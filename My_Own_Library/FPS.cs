using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;

namespace com.Magicalpopsical.TwoD.FPS {
    
    public class FpsCounter {
        static GameTime _gameTime;
        float Fps = 0f;
        private const int NumberSamples = 50;
        int[] Samples = new int[NumberSamples];
        int CurrentSample = 0, TicksAggregate = 0, SecondSinceStart = 0;

        public static GameTime gameTime { set { _gameTime = value; } get { return _gameTime; } }

        private float Sum(int[] Samples) {
            float RetVal = 0f;
            for (int i = 0; i < Samples.Length; i++) {
                RetVal += (float)Samples[i];
            }
            return RetVal;
        }

        public void DrawCall() {
            Samples[CurrentSample++] = (int)_gameTime.ElapsedGameTime.Ticks;
            TicksAggregate += (int)_gameTime.ElapsedGameTime.Ticks;
            if (TicksAggregate > TimeSpan.TicksPerSecond) {
                TicksAggregate -= (int)TimeSpan.TicksPerSecond;
                SecondSinceStart += 1;
            }
            if (CurrentSample == NumberSamples) {
                float AverageFrameTime = Sum(Samples) / NumberSamples;
                Fps = TimeSpan.TicksPerSecond / AverageFrameTime;
                CurrentSample = 0;
            }
        }

        public float GetFps { get { return Fps; } }

        public string GetFpsString {
            get {
                return string.Format("FPS: {0}", Fps.ToString("000"));
            }
        }
    }
}
