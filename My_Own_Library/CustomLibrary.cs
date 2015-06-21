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

/// Version:1.1.2.0
namespace com.Magicalpopsical {
    namespace TwoD {
        
        public interface IDimensions {
            float minDisplayX { get; }
            float maxDisplayX { get; }
            float minDisplayY { get; }
            float maxDisplayY { get; }
        }

        public interface ISpriteBasedGame {
            void UpdateLives(int update);
            void UpdateScore(int update);
            int GetScore();
            //GamePadState GetGamePad();
            MouseState GetMouseState();
            bool keyPressed(Keys curKey);
            bool keyReleased(Keys curKey);
            bool keyHeld(Keys curKey);
            void StartGame();
            void EndGame();
            void ExitProgram();
        }
        
        /// <summary>
        /// Defines a focusable object that the camera can focus on
        /// </summary>
        public interface IFocusable {
            Vector2 getPosition();
        }
    }
}
