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
    public class ParallaxManager   
        //engine creates and manages the group of layers that make up the level 
        //draw and update calls passed to each layer which manages the infomation and passes the draw/update calls to thier component sprites
        //compnent sprites ultimately execute thier own draw and update call in the batch

    {
#region DECLARATIONS

        public List<Layer> worldLayers = new List<Layer>();

#endregion

        #region CONSTRUCTOR

        public ParallaxManager() { }

        public ParallaxManager(Game game)
        {
        
        }

        #endregion

 


        #region UPDATE

        public void Update(GameTime gameTime)
        {

            for (int i = 0; i < worldLayers.Count; i++) 
            {
                if (!worldLayers[i].IsExpired) worldLayers[i].Update(gameTime);
            }

        }
        #endregion

        #region DRAW
        public void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            for (int i = 0; i < worldLayers.Count; i++) 
            {
                if (!worldLayers[i].IsExpired) worldLayers[i].Draw(gameTime, spriteBatch);
            }
        }
        #endregion

        #region ADD/DELETE/COPY LAYER IN WORLD

        public void DeleteLayerFromWorld(Layer layer)
        {
            this.worldLayers.Remove(layer);
            layer = null;
            return;
        }

        public void AddLayerToWorld(Layer layer)
        {
            this.worldLayers.Add(layer);
        }

        public void CopyLayerToWorld (Layer copyThisLayer)
        {
            this.worldLayers.Add(copyThisLayer.Clone());
            return;
        }
        #endregion

        #region SPRITE AND LAYER CLEAN UP METHODS

        public bool DeleteOffWorldSprites (Layer layer)
        {
            bool wereSpritesDeletedFlag = false;
            float layerWidth = 1280.0f + (layer.LayerParallax.X * ((float)Camera.WorldRectangle.Width - 1280.0f));
            float layerHeight = 720.0f + (layer.LayerParallax.Y * ((float)Camera.WorldRectangle.Height - 720.0f));

            for (int i=layer.layerSprites.Count-1; i > -1; i--)
                {

                    if 
                   (layer.layerSprites[i].SpriteRectangle.Bottom <  -100 ||
                    layer.layerSprites[i].SpriteRectangle.Right <  -100 ||
                    layer.layerSprites[i].SpriteRectangle.Left > (layerWidth + 100) ||
                    layer.layerSprites[i].SpriteRectangle.Top > (layerHeight + 100)) 
                         {
                                 layer.DeleteSpriteFromLayer(layer.layerSprites[i]);
                                 wereSpritesDeletedFlag = true;
                         }
                 }
            return wereSpritesDeletedFlag;
        }





        #endregion

        #region PROPERTIES

        #endregion

        #region METHODS

     


        #endregion

    }
}
