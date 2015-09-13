using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace ParallaxEngine
{
    public static class Camera
    {
        #region Camera Declarations

        private static Vector2 position = Vector2.Zero;
        private static Vector2 viewportSize = Vector2.Zero;
        private static Rectangle worldRectangle = new Rectangle(0, 0, 0, 0);
        private static Rectangle screenRectangle;
        private static Rectangle? cameraPositionLimits;
        private static float zoom = 1.0f;
        private static float rotation = 0.0f;
        


        #endregion

        #region Camera Properties

        public static Vector2 Position
        {
            get { return position; }
            set
            {
                if (cameraPositionLimits == null && Zoom == 1.0f && Rotation == 0.0f)
                {
                    position = new Vector2(
                        MathHelper.Clamp(value.X, worldRectangle.X, worldRectangle.Width - ViewportWidth),
                        MathHelper.Clamp(value.Y, worldRectangle.Y, worldRectangle.Height - ViewportHeight));
                }
                
                if (cameraPositionLimits != null && Zoom == 1.0f && Rotation == 0.0f)
                {
                    position = new Vector2(
                        MathHelper.Clamp(value.X, cameraPositionLimits.Value.X, cameraPositionLimits.Value.X + cameraPositionLimits.Value.Width - Viewport.Width),
                        MathHelper.Clamp(value.Y, cameraPositionLimits.Value.Y, cameraPositionLimits.Value.Y + cameraPositionLimits.Value.Height - Viewport.Height));
                }
            }
        }

        //property gets and sets the rectangle representing the limits of the camera position, allows binding of the camera to only portions of the map at a time if needed
        public static Rectangle? CameraPositionLimits
        {
            get
            {
                return cameraPositionLimits;
            }
            set
            {
                if (value != null)
                {
                    // Assign limit but make sure it's always bigger than the viewport
                    cameraPositionLimits = new Rectangle
                    {
                        X = value.Value.X,
                        Y = value.Value.Y,
                        Width = System.Math.Max(Viewport.Width, value.Value.Width),
                        Height = System.Math.Max(Viewport.Height, value.Value.Height)
                    };

                    // Validate camera position with new limit
                    Position = Position;
                }
                else
                {
                    cameraPositionLimits = null;
                }
            }
        }

        public static Rectangle WorldRectangle
        {
            get { return worldRectangle; }
            set { worldRectangle = value; }
        }

        public static int ViewportWidth
        {
            get { return (int)viewportSize.X; }
            set { viewportSize.X = (int)value; }
        }

        public static int ViewportHeight
        {
            get { return (int)viewportSize.Y; }
            set { viewportSize.Y = (int)value; }
        }

        public static Rectangle Viewport
        {
            get { return new Rectangle((int)Position.X, (int)Position.Y, ViewportWidth, ViewportHeight); }
        }

        public static void SetScreenRectangle()
        {
            screenRectangle = new Rectangle(0, 0, ViewportWidth, ViewportHeight);
        }

        public static Vector2 Origin 
        {
            get { return new Vector2((Viewport.Width / 2.0f), (Viewport.Height / 2.0f)); }    
        }

        public static float Zoom
        {
            get { return zoom; }
            set { if (value > 0.1f) zoom = value; }
        }

        public static float Rotation 
        {
            get { return rotation; }
            set { rotation = value; }
        }

        #endregion

        #region Public Camera Methods

        public static Matrix GetViewMatrix(Vector2 parallax)
        {
            return Matrix.CreateTranslation(new Vector3(-Position * parallax, 0.0f)) *
                   Matrix.CreateTranslation(new Vector3(-Origin, 0.0f)) *
                   Matrix.CreateRotationZ(Rotation) *
                   Matrix.CreateScale(zoom, zoom, 1.0f) *
                  Matrix.CreateTranslation(new Vector3(Origin, 0.0f));
        }


        public static void Move(Vector2 displacement)
        {
            Position += displacement;
        }

        public static void Move(Vector2 displacement, bool respectRotation)
        {
            if (respectRotation)
            {
                displacement = Vector2.Transform(displacement, Matrix.CreateRotationZ(-Rotation));
            }

            Position += displacement;
        }

        public static void LookAt(Vector2 position)
        {
            Position = position - new Vector2(Viewport.Width / 2.0f, Viewport.Height / 2.0f);
        }

        
        public static bool IsObjectVisible(Rectangle bounds, Vector2 parallax)
        {
            return (screenRectangle.Intersects(WorldToScreen(bounds,parallax)));
        }

        public static Vector2 WorldToScreen(Vector2 worldPosition)
        {
            return Vector2.Transform(worldPosition, GetViewMatrix(new Vector2 (1.0f,1.0f)));
        }
        
        public static Vector2 WorldToScreen(Vector2 worldPosition, Vector2 parallax)
        {
            return Vector2.Transform(worldPosition, GetViewMatrix(parallax));
        }

        public static Rectangle WorldToScreen(Rectangle worldPosition, Vector2 parallax)
        {
            Vector2 transformedVector = Vector2.Transform(new Vector2 (worldPosition.X,worldPosition.Y), GetViewMatrix(parallax));
            return new Rectangle((int)transformedVector.X, (int)transformedVector.Y, worldPosition.Width, worldPosition.Height);
        }

        public static Rectangle WorldToScreen(Rectangle worldPosition)
        {
            Vector2 transformedVector = Vector2.Transform(new Vector2(worldPosition.X, worldPosition.Y), GetViewMatrix(new Vector2 (1.0f,1.0f)));
            return new Rectangle((int)transformedVector.X, (int)transformedVector.Y, worldPosition.Width, worldPosition.Height);
        }

        public static Vector2 ScreenToWorld(Vector2 screenPosition, Vector2 parallax)
        {
            return Vector2.Transform(screenPosition, Matrix.Invert(GetViewMatrix(parallax)));
        }

        #endregion







    }
}
