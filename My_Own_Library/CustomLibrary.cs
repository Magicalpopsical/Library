using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

/// Version:1.1.5.1
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
            int GetScore { get; }
            //GamePadState GetGamePad();
            MouseState GetMouseState { get; }
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
            Vector2 GetPosition { get; }
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
            int[] SampleS = new int[NumberSamples];
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
                SampleS[CurrentSample++] = (int)_gameTime.ElapsedGameTime.Ticks;
                TicksAggregate += (int)_gameTime.ElapsedGameTime.Ticks;
                if (TicksAggregate > TimeSpan.TicksPerSecond) {
                    TicksAggregate -= (int)TimeSpan.TicksPerSecond;
                    SecondSinceStart += 1;
                }
                if (CurrentSample == NumberSamples) {
                    float AverageFrameTime = Sum(SampleS) / NumberSamples;
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
                pos = focus.GetPosition;
            }

            public Matrix GetTransformation(GraphicsDevice graphicsDevice) {
                if (graphicsDevice != null) {
                    transform = Matrix.CreateTranslation(new Vector3(-pos.X, -pos.Y, 0))
                                * Matrix.CreateRotationZ(rotation)
                                * Matrix.CreateScale(new Vector3(zoom, zoom, 1))
                                * Matrix.CreateTranslation(new Vector3(graphicsDevice.Viewport.Width * 0.5f, graphicsDevice.Viewport.Height * 0.5f, 0));

                    return transform;
                }
                return Matrix.Identity;
            }
        }
        #endregion

        #region TileMapping
        public class Tile {
            #region Fields
            private Texture2D spriteTexture;
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
            protected Texture2D SpriteTexture { get { return spriteTexture; } set { spriteTexture = value; } }
            #endregion

            public void Draw(SpriteBatch spriteBatch) {
                if (spriteBatch != null) {
                    spriteBatch.Draw(spriteTexture, spriteRectangle, Color.White);
                }
            }
        }

        public class CollisionTile : Tile {
            public CollisionTile(string assetName, Rectangle inRectangle) {
                SpriteTexture = Content.Load<Texture2D>(assetName);
                this.SpriteRectangle = inRectangle;
            }
        }

        public class TileMap : IMapControls {
            #region Fields
            private Color[,] colorData;
            private SpriteFont font;
            private int width, height, tileSize;
            private static ContentManager content;
            int levelCounter = 0;
            #endregion

            #region Properties
            public int Width { get { return width; } protected set { width = value; } }
            public int Height { get { return height; } protected set { height = value; } }
            public int TileSize { get { return tileSize; } protected set { tileSize = value; } }
            public static ContentManager Content {
                protected get { return content; }
                set { content = value; }
            }
            public virtual IFocusable GetFocus { get { return FocusObject; } }
            protected Color[,] ColorData { get { return colorData; } set { colorData = value; } }
            protected SpriteFont Font { get { return font; } set { font = value; } }

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
            private Texture2D _spriteTexture;
            private Rectangle _spriteRectangle;
            #endregion

            #region Properties
            public Rectangle SpriteRectangle { get { return _spriteRectangle; } protected set { _spriteRectangle = value; } }
            public Texture2D SpriteTexture { get { return _spriteTexture; } protected set { _spriteTexture = value; } }
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
                if (spriteBatch != null) {
                    spriteBatch.Draw(_spriteTexture, _spriteRectangle, Color.White);
                }
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
            public TitleSprite(Texture2D inSpriteTexture, Rectangle inRectangle)
                : base(inSpriteTexture, inRectangle) {
            }

            public TitleSprite(Texture2D inSpriteTexture, IDimensions inDimensions)
                : base(inSpriteTexture,
                    new Rectangle((int)inDimensions.MinDisplayX, (int)inDimensions.MinDisplayY, (int)(inDimensions.MaxDisplayX - inDimensions.MinDisplayX), (int)(inDimensions.MaxDisplayY - inDimensions.MinDisplayY))) {

            }

            public override void Draw(SpriteBatch spriteBatch) {
                base.Draw(spriteBatch);
            }

        }

        public class MovingSprite : BaseSprite {
            #region Fields
            private float x, y, xSpeed, ySpeed;
            private float initialX, initialY;
            private float minDisplayX, maxDisplayX, minDisplayY, maxDisplayY;
            #endregion

            #region Properties
            public float XPos { get { return x; } protected set { x = value; } }
            public float YPos { get { return y; } protected set { y = value; } }
            protected float XSpeed { get { return xSpeed; } }
            protected float YSpeed { get { return ySpeed; } }
            protected float InitialX { get { return initialX; } }
            protected float InitialY { get { return initialY; } }
            protected float MinDisplayX { get { return minDisplayX; } set { minDisplayX = value; } }
            protected float MaxDisplayX { get { return maxDisplayX; } set { maxDisplayX = value; } }
            protected float MinDisplayY { get { return minDisplayY; } set { minDisplayY = value; } }
            protected float MaxDisplayY { get { return maxDisplayY; } set { maxDisplayY = value; } }
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
                if (inDimensions != null) {
                    minDisplayX = inDimensions.MinDisplayX;
                    minDisplayY = inDimensions.MinDisplayY;
                    maxDisplayX = inDimensions.MaxDisplayX;
                    maxDisplayY = inDimensions.MaxDisplayY;

                    initialX = inInitialX;
                    initialY = inInitialY;

                    float displayWidth = maxDisplayX - minDisplayX;
                    Rectangle TempRectangle = SpriteRectangle;
                    TempRectangle.Width = (int)((displayWidth * widthFactor) + 0.5f);
                    float aspectRatio =
                        (float)SpriteTexture.Width / SpriteTexture.Height;
                    TempRectangle.Height =
                        (int)((TempRectangle.Width / aspectRatio) + 0.5f);
                    SpriteRectangle = TempRectangle;
                    x = initialX;
                    y = initialY;
                    xSpeed = displayWidth / ticksToCrossScreen;
                    ySpeed = xSpeed;
                }
            }

            /// <summary>
            /// Check for collisions between the sprite and other objects.
            /// </summary>
            /// <param name="target">Rectangle giving the position of the other object.</param>
            /// <returns>true if the target has collided with this object</returns>
            public virtual bool CheckCollision(Rectangle target) {
                return SpriteRectangle.Intersects(target);
            }

            public override void Draw(SpriteBatch spriteBatch) {
                base.Draw(spriteBatch);
            }

            public override void Update(ISpriteBasedGame game) {
                Rectangle TempRectangle = SpriteRectangle;
                TempRectangle.X = (int)(x + 0.5f);
                TempRectangle.Y = (int)(y + 0.5f);
                SpriteRectangle = TempRectangle;
                base.Update(game);
            }

            public override void StartGame() {
                x = initialX;
                y = initialY;
                Rectangle TempRectangle = SpriteRectangle;
                TempRectangle.X = (int)x;
                TempRectangle.Y = (int)y;
                SpriteRectangle = TempRectangle;
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
            /// When it reaches the updateClock it is time to update the frames
            /// </summary>
            private int updateTickCounter = 0;
            private int updateClock;
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
                Rectangle TempRectangle = SpriteRectangle;
                TempRectangle.Height = (int)((SpriteRectangle.Width / aspectRatio) + 0.5f);
                SpriteRectangle = TempRectangle;
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
                    if (sourceRect.X + frameWidth >= SpriteTexture.Width) {
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

                if (rowY + frameHeight > SpriteTexture.Height) {
                    // This row does not exist
                    return false;
                }

                sourceRect.Y = rowY;
                rowNumber = newRowNumber;
                return true;
            }

            public override void Draw(SpriteBatch spriteBatch) {
                Draw(spriteBatch, SpriteRectangle);
            }

            public void Draw(SpriteBatch spriteBatch, Rectangle inSpriteRectangle) {
                if (spriteBatch != null) {
                    spriteBatch.Draw(SpriteTexture, inSpriteRectangle, sourceRect, Color.White);
                }
            }

            public void Draw(SpriteBatch spriteBatch, float inRotation, SpriteEffects spriteEffects) {
                if (spriteBatch != null) {
                    spriteBatch.Draw(SpriteTexture, SpriteRectangle, sourceRect, Color.White, inRotation, GetOrigin(), spriteEffects, 1);
                }
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
                if (inExplodeTexture != null) {
                    explodeTexture = inExplodeTexture;
                    explodeFrameWidth = inExplodeFrameWidth;
                    explodeSourceRect = new Rectangle(
                        0, 0,
                        explodeFrameWidth,
                        explodeTexture.Height);
                    explodeSound = inExplodeSound;
                }
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
                if (exploding && spriteBatch != null) {
                    spriteBatch.Draw(explodeTexture, SpriteRectangle,
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

namespace Menu {
    public class MainMenu {
        public delegate void ClickActions();
        public delegate void Click_MenuChoose(string p);

        #region Other Classes
        class cMenu {
            List<cMenuObject> MenuObjects = new List<cMenuObject>();

            public void Update(MouseState inMouse) {
                foreach (var val in MenuObjects) {
                    val.Update(inMouse);
                }
            }

            public void Draw(SpriteBatch spriteBatch) {
                foreach (cButton val in MenuObjects) {
                    val.Draw(spriteBatch);
                }
            }

            public void AddObject(cMenuObject val) {
                MenuObjects.Add(val);
            }
        }

        public class cMenuObject {
            public virtual void Update(MouseState inMouse) {

            }

            public virtual void Draw(SpriteBatch spriteBatch) {

            }
        }

        public class cText : cMenuObject {
            string text;
            Vector2 location;
            SpriteFont font;
            Color c_text;

            public cText(string inText, Vector2 inLocation, SpriteFont inFont) {
                text = inText;
                location = inLocation;
                font = inFont;
                c_text = Color.White;
            }

            public cText(string inText, Vector2 inLocation, SpriteFont inFont, Color inColor) {
                text = inText;
                location = inLocation;
                font = inFont;
                c_text = inColor;
            }

            public override void Draw(SpriteBatch spriteBatch) {
                spriteBatch.DrawString(font, text, location, c_text);
            }
        }

        public class cButton : cMenuObject {
            bool Clickable, Hover;
            Color HoverColor, DefaultColor;
            ClickActions ClickActions;
            Click_MenuChoose ChooseMenu;
            string p_Menu;
            Texture2D SpriteTexture;
            Rectangle SpriteRectangle;

            public bool IsClickable { get { return Clickable; } }
            public bool IsHovering { get { return Hover; } }

            public cButton(Texture2D inSpriteTexture, Rectangle inSpriteRectangle, Color inHover, Color inDefault, ClickActions inClickActions, Click_MenuChoose inChooseMenu = null, string inMenu = null, bool inClickable = true) {
                SpriteTexture = inSpriteTexture;
                SpriteRectangle = inSpriteRectangle;
                HoverColor = inHover;
                DefaultColor = inDefault;
                Clickable = inClickable;
                ClickActions = inClickActions;
                ChooseMenu = inChooseMenu;
                p_Menu = inMenu;
            }

            public override void Update(MouseState inMouse) {
                Rectangle Mouse = new Rectangle(inMouse.X, inMouse.Y, 1, 1);
                if (Mouse.Intersects(SpriteRectangle)) {
                    Hover = true;
                    if (Clickable && inMouse.LeftButton == ButtonState.Pressed) {
                        if (ClickActions != null) {
                            ClickActions();
                        }
                        if (ChooseMenu != null && p_Menu != null) {
                            ChooseMenu(p_Menu);
                        }
                    }
                }
                else {
                    Hover = false;
                }
            }

            public override void Draw(SpriteBatch spriteBatch) {
                if (spriteBatch != null) {
                    if (IsClickable && IsHovering) {
                        spriteBatch.Draw(SpriteTexture, SpriteRectangle, HoverColor);
                    }
                    else {
                        spriteBatch.Draw(SpriteTexture, SpriteRectangle, DefaultColor);
                    }
                }
            }
        }

        public class cPicture : cMenuObject {
            Texture2D SpriteTexture;
            Rectangle SpriteRectangle;
            Color c_picture;

            public cPicture(Texture2D inSpriteTexture, Rectangle inSpriteRectangle) {
                SpriteTexture = inSpriteTexture;
                SpriteRectangle = inSpriteRectangle;
                c_picture = Color.White;
            }

            public cPicture(Texture2D inSpriteTexture, Rectangle inSpriteRectangle, Color inColor) {
                SpriteTexture = inSpriteTexture;
                SpriteRectangle = inSpriteRectangle;
                c_picture = inColor;
            }

            public override void Draw(SpriteBatch spriteBatch) {
                spriteBatch.Draw(SpriteTexture, SpriteRectangle, c_picture);
            }
        }
        #endregion

        Dictionary<string, cMenu> AllMenus = new Dictionary<string, cMenu>();
        cMenu CurrentMenu;

        public MainMenu() {
            AllMenus.Add("Main", new cMenu());
            CurrentMenu = AllMenus["Main"];
        }

        public void AddMenu(string p) {
            AllMenus.Add(p, new cMenu());
        }

        public void AddMenuObject(string p, cMenuObject val) {
            AllMenus[p].AddObject(val);
        }

        public void Update(MouseState inMouse) {
            CurrentMenu.Update(inMouse);
        }

        public void Draw(SpriteBatch spriteBatch) {
            CurrentMenu.Draw(spriteBatch);
        }

        public void ChooseMenu(string p) {
            CurrentMenu = AllMenus[p];
        }
    }
}

namespace Ini {
    /// <summary>
    /// Creates a New INI file to store or load data
    /// </summary>
    public class IniFile {
        public string path;

        [DllImport("kernel32")]
        private static extern long WritePrivateProfileString(string section, string key, string val, string filePath);
        [DllImport("kernel32")]
        private static extern int GetPrivateProfileString(string section, string key, string def, StringBuilder retVal, int size, string filePath);

        /// <summary>
        /// INIFile Constructor
        /// </summary>
        /// <param name="INIPath"></param>
        public IniFile(string INIPath) {
            path = INIPath;
        }

        /// <summary>
        /// Write Data to the INI File
        /// </summary>
        /// <param name="Section">Section Name</param>
        /// <param name="Key">Key Name</param>
        /// <param name="Value">Value Name</param>
        public void IniWriteValue(string Section, string Key, string Value) {
            WritePrivateProfileString(Section, Key, Value, this.path);
        }

        /// <summary>
        /// Read Data Value from the INI File
        /// </summary>
        /// <param name="Section"></param>
        /// <param name="Key"></param>
        /// <returns></returns>
        public string IniReadValue(string Section, string Key) {
            StringBuilder temp = new StringBuilder(255);
            int i = GetPrivateProfileString(Section, Key, "", temp, 255, this.path);
            return temp.ToString();
        }
    }

    public static class GameSettings {
        public delegate object ValidatorDelegate(string readValue);
        public delegate string SaveFormatDelegate(object readValue);

        struct setting {
            public object fileName;
            public string name;

            public object actualValue;
            public object defaultValue;

            public ValidatorDelegate validate;
            public SaveFormatDelegate saveFormat;
        }

        const string settingsDirectory = "Settings";
        const string configSuffix = ".ini";

        static Dictionary<string, setting> settings =
            new Dictionary<string, setting>();
        public static string SettingsDirectory() { return Environment.CurrentDirectory + Path.DirectorySeparatorChar + settingsDirectory + Path.DirectorySeparatorChar; }

        public static void registerSetting(string file, string name, object defaultValue, ValidatorDelegate validator, SaveFormatDelegate saveFormat) {
            IniFile f;
            setting v = new setting();

            if (!Directory.Exists(SettingsDirectory()))
                Directory.CreateDirectory(SettingsDirectory());

            string[] taxonomy = name.Split(".".ToCharArray(), 2);

            Debug.Assert(taxonomy.Length == 2, "Taxonomy must include at least one dot character [.]");

            v.name = name;
            v.fileName = file;

            v.validate = validator;
            v.saveFormat = saveFormat;

            v.defaultValue = defaultValue;

            f = new IniFile(SettingsDirectory() + v.fileName + configSuffix);
            v.actualValue = validator(f.IniReadValue(taxonomy[0], taxonomy[1]));

            settings.Add(name, v);
        }

        public static void saveSettings() {
            foreach (setting v in settings.Values) {
                Save(v);
            }
        }

        private static void Save(setting v) {
            IniFile f = new IniFile(SettingsDirectory() + v.fileName + configSuffix);

            string[] taxonomy = v.name.Split(".".ToCharArray(), 2);

            f.IniWriteValue(taxonomy[0], taxonomy[1], v.saveFormat(v.actualValue));
        }

        public static string getString(string p) {
            return (string)settings[p].actualValue;
        }

        public static string getStringDef(string p) {
            return (string)settings[p].defaultValue;
        }

        public static bool getBool(string p) {
            return (bool)settings[p].actualValue;
        }

        public static bool getBoolDef(string p) {
            return (bool)settings[p].defaultValue;
        }

        public static int getInt(string p) {
            return (int)settings[p].actualValue;
        }

        public static int getIntDef(string p) {
            return (int)settings[p].defaultValue;
        }

        public static float getFloat(string p) {
            return (float)settings[p].actualValue;
        }

        public static float getFloatDef(string p) {
            return (float)settings[p].defaultValue;
        }

        public static string SaveFormatStd(object var) {
            return var.ToString();
        }

        public static object ValidateString(string var) {
            return var;
        }

        public static object ValidateFloat(string var) {
            float result = 0;

            if (!float.TryParse(var, out result))
                return 0;

            return result;
        }

        public static object ValidateBool(string var) {
            bool result = true;

            if (!bool.TryParse(var, out result))
                return 0;

            return result;
        }

        public static object ValidateInt(string var) {
            int result = 0;

            if (!int.TryParse(var, out result))
                return 0;

            return result;
        }

        public static void UpdateSettingValue(string p, object val) {
            setting v = settings[p];
            v.actualValue = v.validate(val.ToString());
            settings.Remove(p);
            settings.Add(p, v);
        }
    }
}
