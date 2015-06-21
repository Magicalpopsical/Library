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

/// Version:1.1.3.0
namespace Com.Magicalpopsical {
    namespace TwoD {

        #region Interfaces
        public interface IDimensions {
            float MinDisplayX { get; }
            float MaxDisplayX { get; }
            float MinDisplayY { get; }
            float MaxDisplayY { get; }
        }

        public interface ISpriteBasedGame {
            void UpdateLives(int update);
            void UpdateScore(int update);
            int GetScore();
            //GamePadState GetGamePad();
            MouseState GetMouseState();
            bool KeyPressed(Keys curKey);
            bool KeyReleased(Keys curKey);
            bool KeyHeld(Keys curKey);
            void StartGame();
            void EndGame();
            void ExitProgram();
        }

        /// <summary>
        /// Defines a focusable object that the camera can focus on
        /// </summary>
        public interface IFocusable {
            Vector2 GetPosition();
        }

        /// <summary>
        /// Interface that allows entities to check if they can move
        /// </summary>
        public interface IMapControls {
            bool CheckPlayerMove(Rectangle target);
            bool CheckAIMove(Rectangle original, Rectangle target);
            bool CheckAttack(Rectangle target);
            void AddAttack(int xMod, int yMod);
        } 
        #endregion

        #region FPS
        public class FpsCounter {
            static GameTime _gameTime;
            float Fps = 0f;
            private const int NumberSamples = 50;
            int[] Samples = new int[NumberSamples];
            int CurrentSample = 0, TicksAggregate = 0, SecondSinceStart = 0;

            public static GameTime GameTime { set { _gameTime = value; } get { return _gameTime; } }

            private float Sum(int[] Values) {
                float RetVal = 0f;
                for (int i = 0; i < Values.Length; i++) {
                    RetVal += (float)Values[i];
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
        #endregion

        #region Camera
        public class Camera2D {
            #region Fields
            float zoom;
            Matrix transform;
            Vector2 pos;
            float rotation;
            IFocusable focus;
            #endregion

            #region Properties
            public float Zoom {
                get { return zoom; }
                set { zoom = value; if (zoom < 0.1f) zoom = 0.1f; }
            }
            public Vector2 Position { get { return pos; } set { pos = value; } }
            public float Rotation { get { return rotation; } set { rotation = value; } }
            #endregion

            public Camera2D(IFocusable inFocus) {
                zoom = 1.0f;
                rotation = 0.0f;
                pos = Vector2.Zero;
                focus = inFocus;
            }

            public void SetFocus(IFocusable inFocus) {
                focus = inFocus;
            }

            public void Update() {
                pos = focus.GetPosition();
            }

            public Matrix GetTransformation(GraphicsDevice graphicsDevice) {
                transform = Matrix.CreateTranslation(new Vector3(-pos.X, -pos.Y, 0)) *
                Matrix.CreateRotationZ(rotation) *
                Matrix.CreateScale(new Vector3(zoom, zoom, 1)) *
                Matrix.CreateTranslation(new Vector3(graphicsDevice.Viewport.Width * 0.5f, graphicsDevice.Viewport.Height * 0.5f, 0));

                return transform;
            }
        }  
        #endregion

        #region TileMapping
        public class Tile {
            #region Fields
            protected Texture2D spriteTexture;
            private Rectangle spriteRectangle;
            private static ContentManager content;
            #endregion

            #region Properties
            public Rectangle SpriteRectangle {
                get { return spriteRectangle; }
                protected set { spriteRectangle = value; }
            }
            public static ContentManager Content {
                protected get { return content; }
                set { content = value; }
            }
            #endregion

            public void Draw(SpriteBatch spriteBatch) {
                spriteBatch.Draw(spriteTexture, spriteRectangle, Color.White);
            }
        }

        public class CollisionTile : Tile {
            public CollisionTile(string assetName, Rectangle inRectangle) {
                spriteTexture = Content.Load<Texture2D>(assetName);
                this.SpriteRectangle = inRectangle;
            }
        }

        public class TileMap : IMapControls {
            #region Fields
            protected Color[,] colorData;
            protected SpriteFont font;
            protected int width, height, tileSize;
            private static ContentManager content;
            int levelCounter = 0;
            #endregion

            #region Properties
            public int Width { get { return width; } }
            public int Height { get { return height; } }
            public int TileSize { get { return tileSize; } }
            public static ContentManager Content {
                protected get { return content; }
                set { content = value; }
            }
            public virtual IFocusable GetFocus { get { return FocusObject; } }
            #endregion

            #region Interfaces
            IDimensions Dimensions;
            ISpriteBasedGame game;
            IFocusable FocusObject;
            #endregion

            #region Interface Methods
            public virtual bool CheckPlayerMove(Rectangle target) {
                return false;
            }

            public virtual bool CheckAIMove(Rectangle original, Rectangle target) {
                return false;
            }

            public virtual bool CheckAttack(Rectangle target) {
                return false;
            }

            public virtual void AddAttack(int xMod, int yMod) {

            }
            #endregion

            public TileMap(IDimensions inDim, int inTileSize, ISpriteBasedGame inGame, SpriteFont inFont) {
                tileSize = inTileSize;
                Dimensions = inDim;
                game = inGame;
                font = inFont;
            }

            /// <summary>
            /// Must override
            /// </summary>
            /// <returns></returns>
            protected virtual bool LoadLevel() {
                return false;
                #region EXAMPLE
                //try {
                //    Texture2D levelTexture = Content.Load<Texture2D>("Levels\\level" + levelCounter + "\\map");
                //    width = levelTexture.Width;
                //    height = levelTexture.Height;

                //    Color[] rawData = new Color[width * height];
                //    levelTexture.GetData<Color>(rawData);

                //    colorData = new Color[width, height];
                //    for (int y = 0; y < height; y++) {
                //        for (int x = 0; x < width; x++) {
                //            colorData[x, y] = rawData[y * width + x];
                //        }
                //    }
                //}
                //catch (Exception) {
                //    EndGame();
                //    levelCounter = 1;
                //    return false;
                //    throw;
                //}

                //ReadLevel();
                //return true; 
                #endregion
            }

            /// <summary>
            /// Must override
            /// </summary>
            protected virtual void ReadLevel() {
                #region EXAMPLE
                //mapSprites = new List<BaseSprite>();
                //mapTiles = new List<CollisionTile>();
                //bool playerFound = false;

                //for (int x = 0; x < width; x++) {
                //    for (int y = 0; y < height; y++) {
                //        Color curColor = colorData[x, y];
                //        Rectangle tileRectangle = GetTileRectangle(x, y);
                //        string assetDirectory = "Levels\\level" + levelCounter + "\\";

                //        if (curColor == Color.Green && !playerFound) {
                //            // PLAYER
                //            tileRectangle.Width /= 2;
                //            tileRectangle.Height /= 2;
                //            if (levelCounter == 1) {
                //                player = new PlayerSprite(
                //                    Content.Load<Texture2D>("player"),
                //                    .05f, 130, Dimensions, tileRectangle,
                //                    32, 32, this, game);
                //            }
                //            else {
                //                player.NextLevel(tileRectangle.X, tileRectangle.Y);
                //            }
                //            playerFound = true;
                //            continue;
                //        }

                //        if (curColor == Color.White) {
                //            // WHITE SPACE
                //            continue;
                //        }

                //        if (curColor == Color.Black) {
                //            // WALL
                //            mapTiles.Add(new CollisionTile(assetDirectory + "wall", tileRectangle));
                //            continue;
                //        }

                //        if (curColor == Color.Yellow) {
                //            exit = new ExitTile("Exit", tileRectangle);
                //        }
                //    }
                //} 
                #endregion
            }

            /// <summary>
            /// Gets the current block in the map. Each block is 16x16.
            /// </summary>
            /// <param name="x">Provided row</param>
            /// <param name="y">Provide column</param>
            /// <returns>The rectangle for the sprite to load in</returns>
            protected Rectangle GetTileRectangle(int x, int y) {
                return new Rectangle(x * tileSize, y * tileSize, tileSize, tileSize);
            }

            public virtual void NextLevel() {
                levelCounter++;
                if (!LoadLevel()) {
                    game.EndGame();
                }
            }

            public virtual void Update() {
                // UPDATE SPRITES
            }

            public virtual void Draw() {
                // DRAW SPRITES
            }

            public virtual void StartGame() {
                levelCounter = 1;
                LoadLevel();
                // END SPRITES
            }

            public virtual void EndGame() {
                // START SPRITES
            }
        }
        #endregion

        #region Sprites
        public class BaseSprite {
            #region Fields
            protected Texture2D _spriteTexture;
            protected Rectangle _spriteRectangle;
            #endregion

            #region Properties
            public Rectangle SpriteRectangle { get { return _spriteRectangle; } }
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
                _spriteTexture = inSpriteTexture;
            }

            /// <summary>
            /// Loads the rectangle to be used as the destination for 
            /// draw operations for this background sprite.
            /// </summary>
            /// <param name="inSpriteRectangle">The rectangle to be used.</param>
            public void SetRectangle(Rectangle inSpriteRectangle) {
                _spriteRectangle = inSpriteRectangle;
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
                spriteBatch.Draw(_spriteTexture, _spriteRectangle, Color.White);
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
                    new Rectangle((int)inDimensions.MinDisplayX, (int)inDimensions.MinDisplayY, (int)(inDimensions.MaxDisplayX - inDimensions.MinDisplayX), (int)(inDimensions.MaxDisplayY - inDimensions.MinDisplayY))) {

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
            public bool IsClickable { get { return Clickable; } }
            public Color HoverColor { get { return _HoverColor; } }
            public Color DefaultColor { get { return _DefaultColor; } }
            protected MouseState MouseState { get { return game.GetMouseState(); } }
            #endregion

            #region Interfaces
            protected ISpriteBasedGame game;
            #endregion

            public ButtonSprite(Texture2D inSpriteTexture, Rectangle inRectangle, Color inHoverColor, Color inDefaultColor, ISpriteBasedGame inGame, bool inClickable = true)
                : base(inSpriteTexture, inRectangle) {
                Clickable = inClickable;
                _HoverColor = inHoverColor;
                _DefaultColor = inDefaultColor;
                game = inGame;
            }

            public bool IsHovering(Vector2 mousePosition) {
                return (mousePosition.X >= _spriteRectangle.X &&
                    mousePosition.Y >= _spriteRectangle.Y &&
                    mousePosition.X <= _spriteRectangle.X + _spriteRectangle.Width &&
                    mousePosition.Y <= _spriteRectangle.Y + _spriteRectangle.Height);
            }

            public bool IsHovering(MouseState inMouseState) {
                return (IsHovering(new Vector2(inMouseState.X, inMouseState.Y)));
            }

            public bool IsHovering() {
                return (IsHovering(MouseState));
            }

            public override void Draw(SpriteBatch spriteBatch) {
                if (Clickable) {
                    if (IsHovering()) {
                        spriteBatch.Draw(_spriteTexture, _spriteRectangle, HoverColor);
                    }
                    else {
                        spriteBatch.Draw(_spriteTexture, _spriteRectangle, DefaultColor);
                    }
                }
                else {
                    spriteBatch.Draw(_spriteTexture, _spriteRectangle, DefaultColor);
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
                if (IsHovering() && MouseState.LeftButton == ButtonState.Pressed && IsClickable) {
                    game.StartGame();
                }
            }
        }

        public class ExitButton : ButtonSprite {
            public ExitButton(Texture2D inSpriteTexture, Rectangle inRectangle, Color inHoverColor, Color inDefaultColor, ISpriteBasedGame inGame, bool inClickable = true)
                : base(inSpriteTexture, inRectangle, inHoverColor, inDefaultColor, inGame, inClickable) {

            }

            public override void Update() {
                if (IsHovering() && MouseState.LeftButton == ButtonState.Pressed && IsClickable) {
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
            public MovingSprite(Texture2D inSpriteTexture, float ticksToCrossScreen, float inInitialX, float inInitialY)
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
                minDisplayX = inDimensions.MinDisplayX;
                minDisplayY = inDimensions.MinDisplayY;
                maxDisplayX = inDimensions.MaxDisplayX;
                maxDisplayY = inDimensions.MaxDisplayY;

                initialX = inInitialX;
                initialY = inInitialY;

                float displayWidth = maxDisplayX - minDisplayX;

                _spriteRectangle.Width = (int)((displayWidth * widthFactor) + 0.5f);
                float aspectRatio =
                    (float)_spriteTexture.Width / _spriteTexture.Height;
                _spriteRectangle.Height =
                    (int)((_spriteRectangle.Width / aspectRatio) + 0.5f);
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
                return _spriteRectangle.Intersects(target);
            }

            public override void Draw(SpriteBatch spriteBatch) {
                base.Draw(spriteBatch);
            }

            public override void Update(ISpriteBasedGame game) {
                _spriteRectangle.X = (int)(x + 0.5f);
                _spriteRectangle.Y = (int)(y + 0.5f);
                base.Update(game);
            }

            public override void StartGame() {
                x = initialX;
                y = initialY;
                _spriteRectangle.X = (int)x;
                _spriteRectangle.Y = (int)y;
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
                _spriteRectangle.Height = (int)((_spriteRectangle.Width / aspectRatio) + 0.5f);
            }

            public AnimatedSprite(Texture2D inSpriteTexture, float ticksToCrossScreen, float initialX, float initialY, int inFrameWidth, int inFrameHeight, int inUpdateClock = 5)
                : base(inSpriteTexture, ticksToCrossScreen, initialX, initialY) {
                frameWidth = inFrameWidth;
                frameHeight = inFrameHeight;
                sourceRect = new Rectangle(0, 0, inFrameWidth, inFrameHeight);
                updateClock = inUpdateClock;
            }

            public override void Update(ISpriteBasedGame game) {
                updateTickCounter++;

                if (updateTickCounter == updateClock) {
                    updateTickCounter = 0;
                    if (sourceRect.X + frameWidth >= _spriteTexture.Width) {
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

                if (rowY + frameHeight > _spriteTexture.Height) {
                    // This row does not exist
                    return false;
                }

                sourceRect.Y = rowY;
                rowNumber = newRowNumber;
                return true;
            }

            public override void Draw(SpriteBatch spriteBatch) {
                Draw(spriteBatch, _spriteRectangle);
            }

            public void Draw(SpriteBatch spriteBatch, Rectangle inSpriteRectangle) {
                spriteBatch.Draw(_spriteTexture, inSpriteRectangle, sourceRect, Color.White);
            }

            public void Draw(SpriteBatch spriteBatch, float inRotation, SpriteEffects spriteEffects) {
                spriteBatch.Draw(_spriteTexture, _spriteRectangle, sourceRect, Color.White, inRotation, GetOrigin(), spriteEffects, 1);
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
                    spriteBatch.Draw(explodeTexture, _spriteRectangle,
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
        #endregion
    }
}
