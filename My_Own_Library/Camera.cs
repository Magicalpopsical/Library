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

namespace com.Magicalpopsical.TwoD.Camera {
    
    public class Camera2d {
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

        public Camera2d(IFocusable inFocus) {
            zoom = 1.0f;
            rotation = 0.0f;
            pos = Vector2.Zero;
            focus = inFocus;
        }

        public void setFocus(IFocusable inFocus) {
            focus = inFocus;
        }

        public void Update() {
            pos = focus.getPosition();
        }

        public Matrix getTransformation(GraphicsDevice gd) {
            transform = Matrix.CreateTranslation(new Vector3(-pos.X, -pos.Y, 0)) *
            Matrix.CreateRotationZ(rotation) *
            Matrix.CreateScale(new Vector3(zoom, zoom, 1)) *
            Matrix.CreateTranslation(new Vector3(gd.Viewport.Width * 0.5f, gd.Viewport.Height * 0.5f, 0));

            return transform;
        }
    }
}
