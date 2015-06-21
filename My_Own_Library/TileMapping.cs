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

namespace com.Magicalpopsical.TwoD.TileMapping {

    /// <summary>
    /// Interface that allows entities to check if they can move
    /// </summary>
    public interface IMapControls {
        bool CheckPlayerMove(Rectangle target);
        bool CheckAIMove(Rectangle original, Rectangle target);
        bool CheckAttack(Rectangle target);
        void AddAttack(int xMod, int yMod);
    }

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
        Color[,] colorData;
        SpriteFont font;
        private int width, height, tileSize;
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

}
