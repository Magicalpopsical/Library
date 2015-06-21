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

namespace com.Magicalpopsical.TwoD.Sprites {

    public class BaseSprite {
        #region Fields
        protected Texture2D spriteTexture;
        protected Rectangle spriteRectangle;
        #endregion

        #region Properties
        public Rectangle SpriteRectangle { get { return spriteRectangle; } }
        #endregion

        /// <summary>
        /// Creates base sprites
        /// </summary>
        /// <param name="inSpriteTexture">Texture of the sprite</param>
        /// <param name="inRectangle">Rectangle of the sprite</param>
        public BaseSprite(Texture2D inSpriteTexture, Rectangle inRectangle) {
            LoadTexture(inSpriteTexture);
            SetRectangle(inRectangle);
        }

        /// <summary>
        /// Loads the texture to be used in the sprite.
        /// </summary>
        /// <param name="spriteTexture">The texture to be used.</param>
        public void LoadTexture(Texture2D inSpriteTexture) {
            spriteTexture = inSpriteTexture;
        }

        /// <summary>
        /// Loads the rectangle to be used as the destination for 
        /// draw operations for this background sprite.
        /// </summary>
        /// <param name="inSpriteRectangle">The rectangle to be used.</param>
        public void SetRectangle(Rectangle inSpriteRectangle) {
            spriteRectangle = inSpriteRectangle;
        }

        /// <summary>
        /// Called when the game starts running
        /// </summary>
        public virtual void StartGame() {
        }

        /// <summary>
        /// Called when the game stops running
        /// </summary>
        public virtual void EndGame() {
        }

        /// <summary>
        /// Draws the texture using the rectangle that dimensions it.
        /// </summary>
        /// <param name="spriteBatch">The SpriteBatch to be used
        /// for the drawing operation.</param>
        public virtual void Draw(SpriteBatch spriteBatch) {
            spriteBatch.Draw(spriteTexture, spriteRectangle, Color.White);
        }

        /// <summary>
        /// Update behavior for the title. The base sprite does not have
        /// any update behavior, but other sprites might.
        /// </summary>
        /// <param name="game">Game to be controlled.</param>
        public virtual void Update(ISpriteBasedGame game) {
        }
    }

    public class TitleSprite : BaseSprite {
        #region Fields
        List<ButtonSprite> ButtonList = new List<ButtonSprite>();
        #endregion

        #region Properties
        public List<ButtonSprite> GetButtonList { get { return ButtonList; } }
        #endregion

        public TitleSprite(Texture2D inSpriteTexture, Rectangle inRectangle)
            : base(inSpriteTexture, inRectangle) {
        }

        public TitleSprite(Texture2D inSpriteTexture, IDimensions inDimensions)
            : base(inSpriteTexture,
                new Rectangle((int)inDimensions.minDisplayX, (int)inDimensions.minDisplayY, (int)(inDimensions.maxDisplayX - inDimensions.minDisplayX), (int)(inDimensions.maxDisplayY - inDimensions.minDisplayY))) {

        }

        public void Update() {
            foreach (var button in ButtonList) {
                button.Update();
            }
        }

        public override void Draw(SpriteBatch spriteBatch) {
            base.Draw(spriteBatch);
            foreach (var button in ButtonList) {
                button.Draw(spriteBatch);
            }
        }

        public void AddButton(ButtonSprite newButton) {
            ButtonList.Add(newButton);
        }
    }

    public class ButtonSprite : TitleSprite {
        #region Fields
        bool Clickable;
        Color _HoverColor, _DefaultColor;
        #endregion

        #region Properties
        public bool isClickable { get { return Clickable; } }
        public Color HoverColor { get { return _HoverColor; } }
        public Color DefaultColor { get { return _DefaultColor; } }
        protected MouseState _MouseState { get { return game.GetMouseState(); } }
        #endregion

        #region Interfaces
        protected ISpriteBasedGame game;
        #endregion

        public ButtonSprite(Texture2D inSpriteTexture, Rectangle inRectangle, Color inHoverColor, Color inDefaultColor, ISpriteBasedGame inGame, bool inClickable = true)
            : base(inSpriteTexture, inRectangle) {
            Clickable = inClickable;
            _HoverColor = inHoverColor;
            _DefaultColor = inDefaultColor;
            inGame = game;
        }

        public bool isHovering(Vector2 MousePosition) {
            return (MousePosition.X >= spriteRectangle.X &&
                MousePosition.Y >= spriteRectangle.Y &&
                MousePosition.X <= spriteRectangle.X + spriteRectangle.Width &&
                MousePosition.Y <= spriteRectangle.Y + spriteRectangle.Height);
        }

        public bool isHovering(MouseState inMouseState) {
            return (isHovering(new Vector2(inMouseState.X, inMouseState.Y)));
        }

        public bool isHovering() {
            return (isHovering(_MouseState));
        }

        public override void Draw(SpriteBatch spriteBatch) {
            if (Clickable) {
                if (isHovering()) {
                    spriteBatch.Draw(spriteTexture, spriteRectangle, HoverColor);
                }
                else {
                    spriteBatch.Draw(spriteTexture, spriteRectangle, DefaultColor);
                }
            }
            else {
                spriteBatch.Draw(spriteTexture, spriteRectangle, DefaultColor);
            }
        }

        public virtual void Update() {

        }
    }

    public class StartButton : ButtonSprite {
        public StartButton(Texture2D inSpriteTexture, Rectangle inRectangle, Color inHoverColor, Color inDefaultColor, ISpriteBasedGame inGame, bool inClickable = true)
            : base(inSpriteTexture, inRectangle, inHoverColor, inDefaultColor, inGame, inClickable) {

        }

        public override void Update() {
            if (isHovering() && _MouseState.LeftButton == ButtonState.Pressed && isClickable) {
                game.StartGame();
            }
        }
    }

    public class ExitButton : ButtonSprite {
        public ExitButton(Texture2D inSpriteTexture, Rectangle inRectangle, Color inHoverColor, Color inDefaultColor, ISpriteBasedGame inGame, bool inClickable = true)
            : base(inSpriteTexture, inRectangle, inHoverColor, inDefaultColor, inGame, inClickable) {

        }

        public override void Update() {
            if (isHovering() && _MouseState.LeftButton == ButtonState.Pressed && isClickable) {
                game.ExitProgram();
            }
        }
    }

    public class MovingSprite : BaseSprite {
        #region Fields
        protected float x, y, xSpeed, ySpeed;
        protected float initialX, initialY;
        protected float minDisplayX, maxDisplayX, minDisplayY, maxDisplayY;
        #endregion

        #region Properties
        public float XPos { get { return x; } }
        public float YPos { get { return y; } }
        #endregion


        /// <summary>
        /// Creates a moving sprite
        /// </summary>
        /// <param name="spriteTexture">Texture to use for the sprite</param>
        /// <param name="ticksToCrossScreen">Speed of movement, in number of 60th of a second ticks to 
        /// cross the entire screen.</param>
        /// <param name="minDisplayX">minimum X value</param>
        /// <param name="maxDisplayX">maximum X value</param>
        /// <param name="minDisplayY">minimum Y value</param>
        /// <param name="maxDisplayY">maximum Y value</param>
        /// <param name="initialX">start X position for the sprite</param>
        /// <param name="initialY">start Y position for the sprite</param>
        public MovingSprite(Texture2D inSpriteTexture, float ticksToCrossScreen, float inInitialX, float inInitialY, IDimensions inDimensions)
            : base(inSpriteTexture, Rectangle.Empty) {
            initialX = inInitialX;
            initialY = inInitialY;
            x = initialX;
            y = initialY;
            float displayWidth = maxDisplayX - minDisplayX;
            xSpeed = displayWidth / ticksToCrossScreen;
            ySpeed = xSpeed;
        }


        /// <summary>
        /// Creates a moving sprite
        /// </summary>
        /// <param name="spriteTexture">Texture to use for the sprite</param>
        /// <param name="widthFactor">Size of the game object, as a fraction of the width
        /// of the screen.</param>
        /// <param name="ticksToCrossScreen">Speed of movement, in number of 60th of a second ticks to 
        /// cross the entire screen.</param>
        /// <param name="minDisplayX">minimum X value</param>
        /// <param name="maxDisplayX">maximum X value</param>
        /// <param name="minDisplayY">minimum Y value</param>
        /// <param name="maxDisplayY">maximum Y value</param>
        /// <param name="initialX">start X position for the sprite</param>
        /// <param name="initialY">start Y position for the sprite</param>
        public MovingSprite(Texture2D inSpriteTexture, float widthFactor, float ticksToCrossScreen, float inInitialX, float inInitialY, IDimensions inDimensions)
            : base(inSpriteTexture, Rectangle.Empty) {
            minDisplayX = inDimensions.minDisplayX;
            minDisplayY = inDimensions.minDisplayY;
            maxDisplayX = inDimensions.maxDisplayX;
            maxDisplayY = inDimensions.maxDisplayY;

            initialX = inInitialX;
            initialY = inInitialY;

            float displayWidth = maxDisplayX - minDisplayX;

            spriteRectangle.Width = (int)((displayWidth * widthFactor) + 0.5f);
            float aspectRatio =
                (float)spriteTexture.Width / spriteTexture.Height;
            spriteRectangle.Height =
                (int)((spriteRectangle.Width / aspectRatio) + 0.5f);
            x = initialX;
            y = initialY;
            xSpeed = displayWidth / ticksToCrossScreen;
            ySpeed = xSpeed;
        }

        /// <summary>
        /// Check for collisions between the sprite and other objects.
        /// </summary>
        /// <param name="target">Rectangle giving the position of the other object.</param>
        /// <returns>true if the target has collided with this object</returns>
        public virtual bool CheckCollision(Rectangle target) {
            return spriteRectangle.Intersects(target);
        }

        public override void Draw(SpriteBatch spriteBatch) {
            base.Draw(spriteBatch);
        }

        public override void Update(ISpriteBasedGame game) {
            spriteRectangle.X = (int)(x + 0.5f);
            spriteRectangle.Y = (int)(y + 0.5f);
            base.Update(game);
        }

        public override void StartGame() {
            x = initialX;
            y = initialY;
            spriteRectangle.X = (int)x;
            spriteRectangle.Y = (int)y;
            base.StartGame();
        }
    }

    public class AnimatedSprite : MovingSprite {
        #region Fields
        /// <summary>
        /// Source rectangle to take out of the sprite texture
        /// </summary>
        private Rectangle sourceRect;
        /// <summary>
        /// Width of one frame in the animation
        /// </summary>
        private int frameWidth;
        /// <summary>
        /// Height of one frame in the animation
        /// </summary>
        private int frameHeight;
        /// <summary>
        /// Counts the updates
        /// When it reaches 5 it is time to update the frames
        /// </summary>
        protected int updateTickCounter = 0;
        protected int updateClock;
        /// <summary>
        /// Number of the row currently selected for the display
        /// </summary>
        private int rowNumber;
        #endregion

        #region Properties
        public int RowNumber { get { return rowNumber; } }
        #endregion

        /// <summary>
        /// Creates a new animated sprite.
        /// </summary>
        /// <param name="inSpriteTexture">Source texture containing multiple sprite images</param>
        /// <param name="widthFactor">Fraction of the width of the screen the star will fill</param>
        /// <param name="ticksToCrossScreen">Number of ticks to travel down the screen</param>
        /// <param name="minDisplayX">Minimum X value of display area</param>
        /// <param name="maxDisplayX">Maximum X value of display area</param>
        /// <param name="minDisplayY">Minimum Y value of display area</param>
        /// <param name="maxDisplayY">Maximum Y value of display area</param>
        /// <param name="initialX">Initial X position of sprite</param>
        /// <param name="initialY">Initial Y position of sprite</param>
        /// <param name="inFrameWidth">width of each frame of the animation</param>
        /// <param name="inFrameHeight">height of each frame of the animation</param>
        public AnimatedSprite(Texture2D inSpriteTexture, float widthFactor, float ticksToCrossScreen, float initialX, float initialY, int inFrameWidth, int inFrameHeight, IDimensions inDimensions, int inUpdateClock = 5)
            : base(inSpriteTexture, widthFactor, ticksToCrossScreen, initialX, initialY, inDimensions) {
            frameWidth = inFrameWidth;
            frameHeight = inFrameHeight;
            sourceRect = new Rectangle(0, 0, inFrameWidth, inFrameHeight);
            updateClock = inUpdateClock;

            // We must use the aspect ratio of a frame in the animation
            // to scale the sprite, not the size of all the frames
            float aspectRatio = (float)frameWidth / frameHeight;
            spriteRectangle.Height = (int)((spriteRectangle.Width / aspectRatio) + 0.5f);
        }

        public AnimatedSprite(Texture2D inSpriteTexture, float ticksToCrossScreen, float initialX, float initialY, int inFrameWidth, int inFrameHeight, IDimensions inDimensions, int inUpdateClock = 5)
            : base(inSpriteTexture, ticksToCrossScreen, initialX, initialY, inDimensions) {
            frameWidth = inFrameWidth;
            frameHeight = inFrameHeight;
            sourceRect = new Rectangle(0, 0, inFrameWidth, inFrameHeight);
            updateClock = inUpdateClock;
        }

        public override void Update(ISpriteBasedGame game) {
            updateTickCounter++;

            if (updateTickCounter == updateClock) {
                updateTickCounter = 0;
                if (sourceRect.X + frameWidth >= spriteTexture.Width) {
                    // reset the animation to the start frame
                    sourceRect.X = 0;
                }
                else {
                    // Move on to the next frame
                    sourceRect.X += frameWidth;
                }
            }
            base.Update(game);
        }

        /// <summary>
        /// Selects a particular display row on the texture.
        /// </summary>
        /// <param name="newRowNumber">Number of the new row. The row at 
        /// the top is number 0</param>
        /// <returns>true if the row was set correctly, 
        /// false if the source texture does not contain that row</returns>
        public bool SetRow(int newRowNumber) {
            int rowY = newRowNumber * frameHeight;

            if (rowY + frameHeight > spriteTexture.Height) {
                // This row does not exist
                return false;
            }

            sourceRect.Y = rowY;
            rowNumber = newRowNumber;
            return true;
        }

        public override void Draw(SpriteBatch spriteBatch) {
            Draw(spriteBatch, spriteRectangle);
        }

        public void Draw(SpriteBatch spriteBatch, Rectangle inSpriteRectangle) {
            spriteBatch.Draw(spriteTexture, inSpriteRectangle, sourceRect, Color.White);
        }

        public void Draw(SpriteBatch spriteBatch, float inRotation, SpriteEffects spriteEffects) {
            spriteBatch.Draw(spriteTexture, spriteRectangle, sourceRect, Color.White, inRotation, GetOrigin(), spriteEffects, 1);
        }

        private Vector2 GetOrigin(Rectangle inRectangle) {
            return new Vector2(inRectangle.Width / 2, inRectangle.Height / 2);
        }

        private Vector2 GetOrigin() {
            return new Vector2(frameWidth / 2, frameHeight / 2);
        }
    }

    public class ExplodingSprite : AnimatedSprite {
        #region Fields
        private Texture2D explodeTexture;
        private Rectangle explodeSourceRect;
        private int explodeFrameWidth;
        private int explodeTickCounter;
        private bool exploding;
        private SoundEffect explodeSound;
        #endregion

        #region Properties
        public bool IsExploding { get { return exploding; } }
        #endregion

        /// <summary>
        /// Creates a new animated sprite.
        /// </summary>
        /// <param name="inSpriteTexture">Source texture containing multiple sprite images</param>
        /// <param name="widthFactor">Fraction of the width of the screen the star will fill</param>
        /// <param name="ticksToCrossScreen">Number of ticks to travel down the screen</param>
        /// <param name="minDisplayX">Minimum X value of display area</param>
        /// <param name="maxDisplayX">Maximum X value of display area</param>
        /// <param name="minDisplayY">Minimum Y value of display area</param>
        /// <param name="maxDisplayY">Maximum Y value of display area</param>
        /// <param name="initialX">Initial X position of sprite</param>
        /// <param name="initialY">Initial Y position of sprite</param>
        /// <param name="inFrameWidth">width of each frame of the animation</param>
        /// <param name="inFrameHeight">height of each frame of the animation</param>
        /// <param name="inExplodeTexture">texture for the explosion</param>
        /// <param name="inExplodeFrameWidth">width of frame in explosion animation</param>
        /// <param name="inExplodeSound">sound to play when the sprite explodes</param>
        public ExplodingSprite(Texture2D inSpriteTexture, float widthFactor, float ticksToCrossScreen, float initialX, float initialY, int inFrameWidth, int inFrameHeight, Texture2D inExplodeTexture, int inExplodeFrameWidth, SoundEffect inExplodeSound, IDimensions inDimensions, int inUpdateClock = 5)
            : base(inSpriteTexture, widthFactor, ticksToCrossScreen, initialX, initialY, inFrameWidth, inFrameHeight, inDimensions, inUpdateClock) {
            explodeTexture = inExplodeTexture;
            explodeFrameWidth = inExplodeFrameWidth;
            explodeSourceRect = new Rectangle(
                0, 0,
                explodeFrameWidth,
                explodeTexture.Height);
            explodeSound = inExplodeSound;
        }

        public override void Update(ISpriteBasedGame game) {
            if (exploding) {
                explodeTickCounter++;
                if (explodeTickCounter == 10) {
                    explodeTickCounter = 0;
                    if (explodeSourceRect.X + explodeFrameWidth >= explodeTexture.Width) {
                        // reached the end of the sequence
                        // not exploding any more
                        exploding = false;
                    }
                    else {
                        // Move on to the next frame
                        explodeSourceRect.X += explodeFrameWidth;
                    }
                }
            }
            base.Update(game);
        }

        public override void StartGame() {
            exploding = false;
            base.StartGame();
        }

        public override void Draw(SpriteBatch spriteBatch) {
            base.Draw(spriteBatch);
            if (exploding) {
                spriteBatch.Draw(explodeTexture, spriteRectangle,
                                 explodeSourceRect, Color.White);
            }
        }

        public void Explode() {
            if (exploding) {
                return;
            }

            try {
                explodeSound.Play();
            }
            catch (Exception) {

            }

            explodeSourceRect.X = 0;
            explodeTickCounter = 0;
            exploding = true;
        }
    }
}
