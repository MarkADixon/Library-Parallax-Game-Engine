using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using FarseerPhysics;
using FarseerPhysics.Factories;
using FarseerPhysics.Dynamics;
using FarseerPhysics.Collision;
using FarseerPhysics.Collision.Shapes;
using FarseerPhysics.Common;



namespace ParallaxEngine
{
    //Sprites must be initialized after the Level Texture Manager has loaded textures
    public class Sprite
    {
        #region DECLARATIONS
        
        //set to subclass name or "Player" or "Enemy" or "Terrain" or "Bullet" etc.
        //used by save/load methods to initialize subclass (if applicable) or put sprite data in the correct Lists etc. 
        //used by game methods to apply logic and behavior 
        protected string spriteType = "NONE";

        public enum SpriteState
        {
            None,
            Fruit,
            Coin
        };

        public SpriteState spriteState = SpriteState.None;

        //Texture Variables 
        //A Sprite uses calls to the Level Texture Manager class using ID and Index to retrieve texture and source rectangle for its draw function
        //Save/load data files also will use the ID and Index, initialized at (-1) means it was not explicitly set
        //to avoid crashes and detect bugs, the Level Texture Manager will return whitespace texture if either value is passed out of bounds
        protected int textureID = 0; //identifies which texture sheet the Level Texture Manager should use 
        protected int textureIndex = 0; //identifies where on a texture sheet the sprite is sourced from
        
        //Location and Draw Variables
        protected Rectangle spriteRectangle = new Rectangle (0,0,1,1);
        protected Vector2 location = Vector2.Zero;
        protected Color tintColor = Color.White;
        protected bool isFlippedHorizontally = false;
        protected bool isFlippedVertically = false;
        protected SpriteEffects spriteEffect = SpriteEffects.None;  //the spritebatch flipping variable


        //sprite physical state variables
        protected bool isAwake = false; //skip update if flagged false
        protected bool isVisible = true; //skip draw if flagged false
        protected bool isExpired = false; //when flagged for true, auto flags isAwake and isVisible to false, allows for disposal or reuse by game logic
        protected bool isCollidable = false;
        protected bool isHit = false;//used by the managers for sprite types
        protected float hitPoints = 0.0f;//used by the managers for sprite types
        protected float collisionRadius = 0.0f; //if set to positive value, cirlce collision implimented instead of rectangle collision
        public Body spriteBody;

        //Motion Variables 
        protected bool isMobile = false; //set to true if the sprite has velocity relative to game world, immovable if set false
        protected float velocity = 0.0f; //velocity units per second (pixels if traveling on axis)
        protected Vector2 direction = Vector2.Zero; //normalized direction vector, does not affect velocity only used for direction determination 

        //Rotation and Scaling Variables
        protected bool isRotating = false;  
        protected float rotationSpeed = 0.0f; // velocity of rotation in degrees per second units, negative is counterclockwise
        protected float totalRotation = 0.0f; //internal variable to track roatation and pass to draw method
        protected float scale = 1.0f; //sprite scale

        //Animation Variables
        protected bool isAnimated = false;
        protected bool isAnimatedWhileStopped = false;
        protected bool isBounceAnimated = false;  //will scroll back and forth on the animation row if true, will loop from end to beginning if false
        protected bool isAnimationDirectionForward = true; //internal flag to make bounce animation work
        protected int currentFrame = 0; //would be added to textureIndex with a call to the Level Texture Manager to retrieve source rect for animated sprites
        protected float animationFPS = 0.0f;
        protected float animationFramePrecise = 0.0f;

#endregion

    #region CONSTRUCTORS

        //Minimum constructor for sprite from a sprite sheet, LevelDataManager class manages the calls for a sprite's current texture
        public Sprite(int _textureID, int _textureIndex, Vector2 _location)
        {
            textureID = _textureID;
            textureIndex = _textureIndex;
            location = _location;
            spriteRectangle = new Rectangle ((int)_location.X,(int)_location.Y, 
                                            LevelDataManager.SpriteWidth(textureID),
                                            LevelDataManager.SpriteHeight(textureID));
        }

        public Sprite(string _type, int _ID, int _index, Rectangle _rect, Color _color, bool _fliph, bool _flipV, bool _awake, bool _visible,
                      bool _collide, float _colradius, bool _mobile, float _vel, Vector2 _dir, bool _rotating, float _rotspd, float _totrot, 
                      float _scale, bool _animated, bool _awhilestop, bool _bounce, float _fps )
        {
            spriteType = _type;
            textureID = _ID;
            textureIndex = _index;
            SpriteRectangle = _rect; //calls property to also set location
            tintColor = _color;
            IsFlippedHorizontally = _fliph;
            IsFlippedVertically = _flipV;
            isAwake = _awake;
            isVisible = _visible;
            isCollidable = _collide;
            collisionRadius = _colradius;
            isMobile = _mobile;
            velocity = _vel;
            direction = _dir;
            isRotating = _rotating;
            rotationSpeed = _rotspd;
            totalRotation = _totrot;
            scale = _scale;
            isAnimated = _animated;
            isAnimatedWhileStopped = _awhilestop;
            isBounceAnimated = _bounce;
            animationFPS = _fps;
        }

    #endregion


       


        #region UPDATE
        public void Update(GameTime gameTime)
        {
            if (!isAwake) return;

            //if (isMobile) UpdateMotion(gameTime);

            //if (isRotating) UpdateRotation(gameTime);

            if ((isAnimated && isAnimatedWhileStopped) || (isAnimated && isMobile && velocity !=0)) UpdateAnimation(gameTime);


        }

        private void UpdateMotion(GameTime gameTime)
        {
            if (velocity != 0)
            {
                float distance = velocity * (float)gameTime.ElapsedGameTime.TotalSeconds;
                location.X += (direction.X * distance);
                location.Y += (direction.Y * distance);
                spriteRectangle.X = (int)location.X;
                spriteRectangle.Y = (int)location.Y;
            }
        }

        private void UpdateRotation(GameTime gameTime)
        {
            if (rotationSpeed != 0)
            {
                totalRotation += (MathHelper.ToRadians(rotationSpeed) * (float)gameTime.ElapsedGameTime.TotalSeconds);
            }
        }

        private void UpdateAnimation(GameTime gameTime)
        {
            animationFramePrecise += (float)gameTime.ElapsedGameTime.TotalSeconds*animationFPS;
            if (animationFramePrecise >= 1.0f)
            {
                animationFramePrecise -= 1.0f;
                if (isAnimationDirectionForward)
                {

                    if (currentFrame+1 == LevelDataManager.SpritesInRow(textureID))
                    {
                        if (!isBounceAnimated) currentFrame = 0; //loop if not bounce animated
                        else
                        {
                            isAnimationDirectionForward = false; //send the other direction
                            currentFrame -= 1; //move to frame before 
                        }
                    }
                    else currentFrame += 1;
                }
                else
                {
                    if (currentFrame == 0)
                    {
                        if (!isBounceAnimated) currentFrame = LevelDataManager.SpritesInRow(textureID); //loop if not bounce animated
                        else
                        {
                            isAnimationDirectionForward = true; //send the other direction
                            currentFrame += 1; //move to frame before 
                        }
                    }
                    else currentFrame -= 1;
                }    
            }
        }

        #endregion



        #region DRAW
        public void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            if (!isVisible) return;

            //for rotation the center of the sprite must be added to the draw location since they rotate about the center
            if (isRotating || rotationSpeed != 0 || totalRotation != 0 || scale != 1.0)
            {
                spriteBatch.Draw(Texture,
                    SpriteCenterInWorld,
                    SourceRectangle, tintColor, totalRotation, SpriteOrigin, scale, spriteEffect, 0.0f);
            }
            else
            {
                spriteBatch.Draw(Texture, spriteRectangle, SourceRectangle,
                                 tintColor, 0.0f, Vector2.Zero, spriteEffect, 0.0f);
            }

        }

        public void Draw(GameTime gameTime, SpriteBatch spriteBatch, Vector2 layerParallax)
        {
            if (!isVisible || !Camera.IsObjectVisible(spriteRectangle,layerParallax)) return;

            //for rotation the center of the sprite must be added to the draw location since they rotate about the center
            if (isRotating || rotationSpeed != 0 || totalRotation != 0 || scale != 1.0)
            {
                spriteBatch.Draw(Texture,
                    SpriteCenterInWorld,
                    SourceRectangle, tintColor, totalRotation, SpriteOrigin, scale, spriteEffect, 0.0f);
            }
            else
            {
                spriteBatch.Draw(Texture, spriteRectangle, SourceRectangle,
                                 tintColor, 0.0f, Vector2.Zero, spriteEffect, 0.0f);
            }

        }



        #endregion

        #region  PROPERTIES
        public string SpriteType
        {
            get { return this.spriteType; }
            set { if (value != null) this.spriteType = value; }
        }
        public int TextureID
        {
            get { return this.textureID; }
            set { this.textureID = value; }
        }
        public int TextureIndex
        {
            get { return this.textureIndex; }
            set { this.textureIndex = value; }
        }
        public Rectangle SpriteRectangle
        {
            get { return this.spriteRectangle; }
            set
            { 
                this.spriteRectangle = value;
                this.location = new Vector2 ( (float)spriteRectangle.X,(float)spriteRectangle.Y );
            }
        }
        public int SpriteRectWidth
        {
            get { return this.spriteRectangle.Width; }
            set { this.spriteRectangle.Width = value; }
        }
        public int SpriteRectHeight
        {
            get { return this.spriteRectangle.Height; }
            set { this.spriteRectangle.Height = value; }
        }
        public int TileWidth
        {
            get { return LevelDataManager.levelTextures[textureID].TileWidth; }
        }
        public int TileHeight
        {
            get { return LevelDataManager.levelTextures[textureID].TileHeight; }
        }
        public Vector2 Location
        { 
            get { return this.location; }
            set 
            { 
                this.location = value;
                this.spriteRectangle.X = (int)location.X;
                this.spriteRectangle.Y = (int)location.Y;
            }
        }
        public Color TintColor
        {
            get { return this.tintColor; }
            set 
            { 
                this.tintColor = value;
            }
        }
        public bool IsFlippedHorizontally 
        { 
            get { return this.isFlippedHorizontally; } 
            set 
            { 
                this.isFlippedHorizontally = value;
                if (this.isFlippedHorizontally)
                {
                    this.isFlippedVertically = false;
                    spriteEffect = SpriteEffects.FlipHorizontally;
                }
                if (!this.isFlippedHorizontally) spriteEffect = SpriteEffects.None;
            } 
        }
        public bool IsFlippedVertically 
        { 
            get { return this.isFlippedVertically; }
            set 
            { 
                this.isFlippedVertically = value;
                if (this.isFlippedVertically)
                {
                    this.isFlippedHorizontally = false;
                    spriteEffect = SpriteEffects.FlipVertically;
                }
                if (!this.isFlippedVertically) spriteEffect = SpriteEffects.None;
            } 
        }
        public SpriteEffects SpriteEffect { get { return this.spriteEffect; } set { this.spriteEffect = value; } }
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
        public bool IsHit { get { return this.isHit; } set { this.isHit = value; } }
        public float HitPoints { get { return this.hitPoints; } set { this.hitPoints = value; } }
        public bool IsCollidable { get { return this.isCollidable; } set { this.isCollidable = value; } }
        public float CollisionRadius { get { return this.collisionRadius; } set { if (value >= 0) this.collisionRadius= value; } }
        public bool IsMobile{ get { return this.isMobile; } set { this.isMobile = value; } }
        public float Velocity { get { return this.velocity; } set { this.velocity = value; } }
        public Vector2 Direction
        {
            get { return this.direction; }
            set
            {
                this.direction = value;
                if (direction != Vector2.Zero)
                {
                    direction.Normalize();
                }
            }
        }
        public bool IsRotating { get { return this.isRotating; } set { this.isRotating = value; } }
        public float RotationSpeed { get { return this.rotationSpeed; } set { this.rotationSpeed = value; } }
        public float TotalRotation { get { return this.totalRotation; } set { this.totalRotation = value; } }
        public float Scale
        {
            get { return this.scale; }
            set { this.scale = value; }
        }
        public bool IsAnimated { get { return this.isAnimated; } set { this.isAnimated = value; } }
        public bool IsAnimatedWhileStopped { get { return this.isAnimatedWhileStopped; } set { this.isAnimatedWhileStopped = value; } }
        public bool IsBounceAnimated { get { return this.isBounceAnimated; } set { this.isBounceAnimated = value; } }
        public int CurrentFrame { get { return this.currentFrame; } set { if (value >= 0) this.currentFrame = value; } }
        public float AnimationFPS { get { return this.animationFPS; } set { this.animationFPS = value; } }
        #endregion

        #region READ ONLY PROPERTIES
        public Texture2D Texture { get { return LevelDataManager.GetSourceTexture(textureID); } }
        public bool[,] GetCollisionData { get { return LevelDataManager.GetCollisionData(textureID, textureIndex); } }

        public Rectangle SourceRectangle { 
            get 
            {
                return LevelDataManager.GetSourceRect(textureID, textureIndex+currentFrame);
            }
        }

        public Vector2 SpriteCenterInWorld
        {
            get 
            { 
                return ( new Vector2 
                    ( location.X + ((spriteRectangle.Width)/2.0f)  , 
                      location.Y + ((spriteRectangle.Height)/2.0f)  ) );
            }
        }

        //Returns a vector represented center of sprite from the top left corner of sprite
        public Vector2 SpriteOrigin
        {
            get
            {
                return (new Vector2( ((spriteRectangle.Width) / 2.0f),((spriteRectangle.Height)/ 2.0f)));
            }
        }


        #endregion

        #region CLONE
        public Sprite Clone()
        {
            Sprite data = new Sprite(this.TextureID,this.textureIndex,this.location);

            data.spriteType = this.spriteType;

            data.textureID = this.textureID;
            data.textureIndex = this.textureIndex;

            data.spriteRectangle = this.spriteRectangle;
            data.location = this.location;
            data.tintColor = this.tintColor;
            data.isFlippedHorizontally = this.isFlippedHorizontally;
            data.isFlippedVertically = this.isFlippedVertically;
            data.SpriteEffect = this.SpriteEffect;

            data.isAwake = this.isAwake;
            data.isVisible = this.isVisible;
            data.isExpired = this.isExpired;
            data.isCollidable = this.isCollidable;
            data.collisionRadius = this.collisionRadius;

            data.isMobile = this.isMobile;
            data.velocity = this.velocity;
            data.direction = this.direction;

            data.isRotating = this.isRotating;
            data.rotationSpeed = this.rotationSpeed;
            data.totalRotation = this.totalRotation;
            data.scale = this.scale;

            data.isAnimated = this.isAnimated;
            data.isAnimatedWhileStopped = this.isAnimatedWhileStopped;
            data.isBounceAnimated = this.isBounceAnimated;
            data.isAnimationDirectionForward = this.isAnimationDirectionForward;
            data.currentFrame = this.currentFrame;
            data.animationFPS = this.animationFPS;
            data.animationFramePrecise = this.animationFramePrecise;

            return data;
        }
#endregion

        #region PHYSICS


        public void UpdateSpriteFromPhysics()
        {
            this.Location = ConvertUnits.ToDisplayUnits(this.spriteBody.Position) - this.SpriteOrigin;
            this.TotalRotation = this.spriteBody.Rotation;
        }

        public void RemoveBody(World world)
        {
            world.RemoveBody(this.spriteBody);
            ActivatePhysics(world);
            return;
        }

        public void ActivatePhysics(World physicsWorld)
        {
            foreach (Body body in physicsWorld.BodyList)
            {
                body.Awake = true;
            }
            return;
        }
        #endregion

    }
}
