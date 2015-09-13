using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;

namespace ParallaxEngine
{
    
    public class Layer
    {
        
        /// <summary>
        /// a layer object is a "collection of sprites" that exists at a certain depth
        /// layer passes the draw and update calls to all sprites that are members of that layer
        /// layer depth is managed in the list in the Parallax manager
        /// the repeating of the entire layer will be handled by the parallax manager if the repeat is on
        /// </summary>
        protected string layerName = ""; //used by the map editor for convienence, no need to load a value during the game
        public List<Sprite> layerSprites = new List<Sprite>();

        protected Vector2 layerParallax = Vector2.One; //1.0f is no parallax, 0.0f for slower rate (farther back), above 1.0f creates faster scroll than camera (near lens)
       
        protected bool isAwake = true;  //skip update for layer if false
        protected bool isVisible = true; //skip draw for layer if false
        protected bool isExpired = false; // if marked true--sets awake and visible to false, allows for disposal or reuse by game logic
       
        //Set of Variables for layers containing Motion (or motion vectors)
        //use 
        protected bool isLayerMotion = false; //set to true if the layer has a velocity vector, translates movement positionally to sprites on layer each update
        protected float layerVelocity = 0.0f; //velocity units per second (pixels if traveling on axis)
        protected Vector2 layerVDirection = Vector2.Zero; //normalized direction vector, does not affect velocity only used for direction determination  
        protected bool isLayerGravity = false;  //set to true of hte layer accelerates object on it, translates velocity onto sprites on layer each update, only sprites flaged as isMoving are affected
        protected float layerAcceleration = 0.0f; //how much velocity to add to sprites each second
        protected Vector2 layerADirection = Vector2.UnitY; //normalized direction vector, does not affect acceleration only used for direction determination
        //not support for rotating layer since that would require translation matrix for all component sprites


        #region CONSTRUCTOR

        public Layer() { layerSprites = new List<Sprite>(); }

        public Layer(String _name)
        { 
            this.layerName = _name;
            layerSprites = new List<Sprite>();
        }

        public Layer(String _name, Vector2 _parallax, bool _awake, bool _visible, bool _motion, float _velocity, Vector2 _Vdir, bool _gravity, float _accel, Vector2 _Adir)
        {
            layerName = _name; 
            layerSprites = new List<Sprite>();
            layerParallax = _parallax; 
            isAwake = _awake; 
            isVisible = _visible; 
            isLayerMotion = _motion; 
            layerVelocity = _velocity; 
            LayerVDirection = _Vdir; //galls the method which normalizes the vector
            isLayerGravity = _gravity; 
            layerAcceleration = _accel; 
            LayerADirection = _Adir; //calls the method which normalizes the vector
        }


        #endregion 

        #region LOAD/UNLOAD

        public void LoadContent()
        {
        }

        public void UnloadContent()
        {
        }

        #endregion



        public void Update(GameTime gameTime)
        {
            if (!isAwake) return;

            if (isLayerMotion) UpdateLayerMotion(gameTime);

            //if (isLayerGravity) UpdateLayerAcceleration(gameTime);

            //calls update on each sprite, 
            for (int i = 0; i < layerSprites.Count; i++) 
            {
                layerSprites[i].Update(gameTime);
            }

            return;
        }

        public void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            if (!isVisible) return;

            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, null, null, null, Camera.GetViewMatrix(layerParallax));
            
            for (int i = 0; i < layerSprites.Count; i++) 
            {
                layerSprites[i].Draw(gameTime, spriteBatch, layerParallax);
            }
            spriteBatch.End();
            return;
        }

        #region METHODS

        private void UpdateLayerMotion(GameTime gameTime)
        {
            if (layerVelocity != 0)
            {
                float distance = layerVelocity * (float)gameTime.ElapsedGameTime.TotalSeconds;
                float xdist = (layerVDirection.X * distance);
                float ydist = (layerVDirection.Y * distance);

                //update positions for all sprites in layer based on motion in this layer
                foreach (Sprite sprite in layerSprites)
                {
                   Vector2 oldLocation = sprite.Location;
                   sprite.Location = oldLocation + new Vector2(xdist, ydist); 
                }
            }

            return;
        }

        //unimplimented if physics engine is implimented
        private void UpdateLayerAcceleration(GameTime gameTime)
        {
            if (layerAcceleration != 0)
            {
                float dvelocity = layerAcceleration * (float)gameTime.ElapsedGameTime.TotalSeconds;
                float dxvel = (layerADirection.X * dvelocity);
                float dyvel = (layerADirection.Y * dvelocity);

                //update positions for all sprites in layer based on motion in this layer
                foreach (Sprite sprite in layerSprites)
                {
                    if (sprite.IsMobile && sprite.IsAwake)
                    {
                        float oldVelX = sprite.Velocity * sprite.Direction.X;
                        float oldVelY = sprite.Velocity * sprite.Direction.Y;
                        float newVelX = (oldVelX + dxvel);
                        float newVelY = (oldVelY + dyvel);

                        sprite.Velocity = ((float)Math.Sqrt((newVelX * newVelX) + (newVelY * newVelY)));
                        sprite.Direction = new Vector2(newVelX, newVelY);
                        
                    }
                }
            }

            return;
        }
        public void DeleteSpriteFromLayer (Sprite sprite)
        {
            this.layerSprites.Remove(sprite);
            sprite = null;
            return;
        }

        public void AddSpriteToLayer(Sprite sprite)
        {
            this.layerSprites.Add(sprite);
        }

        public void CopySpriteToLayer(Sprite copyThisSprite)
        {
            this.layerSprites.Add(copyThisSprite.Clone());
        }

        public void TintLayerColor(Color layerTintColor)
        {
            foreach (Sprite sprite in this.layerSprites)
            {
                sprite.TintColor = layerTintColor;
            }

            return;
        }

        public int SpriteCount
        {
            get { return this.layerSprites.Count; }
        }

        #endregion

        #region PROPERTIES

        public string LayerName { get { return this.layerName; } set { this.layerName = value; } } 

        public Vector2 LayerParallax 
        {
            get {return this.layerParallax;}
            set { this.layerParallax = value; }
        }

        public bool IsLayerMotion
        {
            get { return this.isLayerMotion; }
            set { this.isLayerMotion = value; }
        }

        public float LayerVelocity
        {
            get { return this.layerVelocity; }
            set { this.layerVelocity = value; }
        }

        public Vector2 LayerVDirection
        {
            get { return this.layerVDirection; }
            set
            {
                this.layerVDirection = value;
                if (layerVDirection != Vector2.Zero)
                {
                    layerVDirection.Normalize();
                }
            }
        }

        public bool IsLayerGravity
        {
            get { return this.isLayerGravity; }
            set { this.isLayerGravity = value; }
        }

        public float LayerAcceleration
        {
            get { return this.layerAcceleration; }
            set { this.layerAcceleration = value; }
        }
        public Vector2 LayerADirection
        {
            get { return this.layerADirection; }
            set
            {
                this.layerADirection = value;
                if (layerADirection != Vector2.Zero)
                {
                    layerADirection.Normalize();
                }
            }
        }
        public List<Sprite> LayerSprites
        {
            get { return this.layerSprites; }
        }

        public bool IsAwake { get { return this.isAwake; } set { this.isAwake = value; } }
        public bool IsVisible { get { return this.isVisible; } set { this.isVisible = value; } }
        public bool IsExpired
        {
            get { return this.isExpired; }
            set
            {
                IsAwake = false;
                IsVisible = false;
                this.isExpired = value;
            }
        }

        #endregion

        #region CLONE
        public Layer Clone ()
        {
            Layer data = new Layer ();
                    
            data.layerName = this.layerName;
            data.layerParallax = this.layerParallax;
            data.isAwake = this.isAwake;
            data.isVisible = this.isVisible;
            data.isExpired = this.isExpired;
 
            data.isLayerMotion = this.isLayerMotion;
            data.layerVelocity = this.layerVelocity; 
            data.layerVDirection = this.layerVDirection;
            data.isLayerGravity = this.isLayerGravity;
            data.layerAcceleration = this.layerAcceleration;
            data.LayerADirection = this.LayerADirection;
            foreach (Sprite sprite in this.layerSprites)
            {
                data.layerSprites.Add(sprite.Clone());
            }
            return data;
        }

        #endregion




    }
}
