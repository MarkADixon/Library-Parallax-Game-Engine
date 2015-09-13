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

namespace ParallaxEngine
{

    //class that contains a list of sourcetextures in use by the parallaxengine in the current level and/or editor project
    //the class maintains the indexing of tiled textures and can return a tile texture with a call contiaining the texture ID and index number  
    //Sprite draw calls functions in this class

    public static class LevelDataManager
    {
        #region DECLARATIONS
        public static List<TextureData> levelTextures;
        public static List<TextureData> effectTextures;
        static ContentManager content;
        public const int texturePadding = 2;

        #endregion

        #region CONSTRUCTOR
        public static void Initialize( Game game )
        {
            levelTextures = new List<TextureData>();
            effectTextures = new List<TextureData>();
            if (content != null) content.Unload();
            if (content == null) content = new ContentManager(game.Services, "Content");
        }
        public static void Initialize(ContentManager _content)
        {
            levelTextures = new List<TextureData>();
            effectTextures = new List<TextureData>();
            content = _content;
        }

        #endregion



        #region TEXTURE METHODS

        //sprites or tiles in given row on the sprite sheet texture
        public static int SpritesInRow (int id)
        {
           return levelTextures[id].SpritesInRow; 
        }

        public static int SpritesInColumn(int id)
        {
            return levelTextures[id].SpritesInColumn;
        }

        public static int SpriteWidth(int id)
        {
            return levelTextures[id].TileWidth;
        }

        public static int SpriteHeight(int id)
        {
            return levelTextures[id].TileHeight;
        }
        //returns a rectangle to draw tile from given the index number for a particular tile
        public static Rectangle GetSourceRect(int id, int index)
        {
            return new Rectangle(
                texturePadding+(int)(index % SpritesInRow(id)) * (SpriteWidth(id)+(2*texturePadding)),
                texturePadding+(int)(index / SpritesInRow(id)) * (SpriteHeight(id)+(2*texturePadding)),
                SpriteWidth(id),
                SpriteHeight(id));
        }

        public static Texture2D GetSourceTexture(int id)
        {
            return levelTextures[id].Texture;
        }

        public static int GetIDbyName(string texturetype)
        {
            int ID = -1;
            foreach (TextureData texture in levelTextures)
            {
                if (texture.SpriteType == texturetype)
                    return texture.TextureID;
            }
            return ID;
        }

        public static string GetFileByID(int id)
        {
            foreach (TextureData texture in levelTextures)
            {
                if (texture.TextureID == id)
                    return texture.FileName;
            }

            return "";
        }


        public static bool[,] GetCollisionData(int _id, int _index)
        { 
            if (!levelTextures[_id].IsTiled) return levelTextures[_id].GetCollisionData;
            else
            {
                bool[,] tileCollisionData = new bool[SpriteWidth(_id),SpriteHeight(_id)];
                Rectangle tileRectangle = GetSourceRect(_id,_index);
                for (int y = 0; y < tileRectangle.Height; y++)
                {
                    for (int x = 0; x < tileRectangle.Width; x++)
                    {
                        tileCollisionData [x,y] = levelTextures[_id].GetCollisionData[x+tileRectangle.X,y+tileRectangle.Y];
                    }
                }
                return tileCollisionData;
            }
        }

        #region LOAD TEXTURES 
        //asks the level manager to load a texture, returns Texture ID or -1 if the ID
        public static int Load (string filepath)
        {
            int loadedTextureID = -1;
            TextureData newTexture = new TextureData();
            newTexture.FilePath = filepath;
            string[] seperators = new string[] { @"\" };
            string[] result2 = filepath.Split(seperators, StringSplitOptions.RemoveEmptyEntries);
            newTexture.FileName = result2[result2.Length - 1];

            //if the texture in the filepath is already loaded, return the ID
            loadedTextureID = IsTextureLoaded(newTexture.FileName); 
            if (loadedTextureID != -1) return loadedTextureID;
            
            //if it wasnt already loaded, load it and generate an ID, and return the new ID
            newTexture.IsTiled = false; 
            newTexture.Texture = content.Load<Texture2D>(newTexture.FilePath);
            if (newTexture.TextureID < 0) CreateID(newTexture); 
            LevelDataManager.levelTextures.Add(newTexture);
            return newTexture.TextureID;
        }

        public static int Load(string filepath, bool isTiled, int tileWidth, int tileHeight)
        {
            int loadedTextureID = -1;
            TextureData newTexture = new TextureData();
            newTexture.FilePath = filepath;
            string[] seperators = new string[] { @"\" };
            string[] result2 = filepath.Split(seperators, StringSplitOptions.RemoveEmptyEntries);
            newTexture.FileName = result2[result2.Length - 1];

            //if the texture in the filepath is already loaded, return the ID
            loadedTextureID = IsTextureLoaded(newTexture.FileName);

            if (loadedTextureID != -1)
            {
                int loadedIndex = GetIndexByID(loadedTextureID);
                levelTextures[loadedIndex].IsTiled = isTiled;
                levelTextures[loadedIndex].TileWidth = tileWidth;
                levelTextures[loadedIndex].TileHeight = tileHeight;
                return loadedTextureID;
            }

            //if it wasnt already loaded, load it and generate an ID, and return the new ID
            else
            {
                newTexture.IsTiled = isTiled;
                newTexture.TileWidth = tileWidth;
                newTexture.TileHeight = tileHeight;
                newTexture.Texture = content.Load<Texture2D>(newTexture.FilePath);
                if (newTexture.TextureID < 0) CreateID(newTexture);
                LevelDataManager.levelTextures.Add(newTexture);
                return newTexture.TextureID;
            }
        }

        //pass initialized TextureData, for loading of levels in mapeditor and game
        public static void Load(TextureData newTexture)
        {
            newTexture.Texture = content.Load<Texture2D>(newTexture.FilePath);
            LevelDataManager.levelTextures.Add(newTexture);
        }

        

        private static void CreateID(TextureData newTexture)
        {
            newTexture.TextureID += 1;         
                foreach (TextureData loadedTextures in levelTextures)
                {
                    if (loadedTextures.TextureID == newTexture.TextureID) CreateID(newTexture);
                }
            return;
        }
        //check by filename to see if a texture has already been loaded
        public static int IsTextureLoaded(string filename)
        {
            int loadedID = -1;
            foreach (TextureData texture in levelTextures)
            {
                if (texture.FileName == filename) return texture.TextureID;
            }
            return loadedID;
        }
        #endregion

        #region UNLOAD 
        //call when changing levels
        public static void Unload()
        {
            levelTextures = new List<TextureData>();
            content.Unload();
        }

        #endregion


        public static string GetFileNameByTextureID(int ID)
        {
            foreach (TextureData texture in levelTextures)
            {
                if (texture.TextureID == ID) return texture.FileName;
            }
            return "no texture by that ID";
        }

       private static int GetIndexByID(int ID)
       {
           for( int i = 0; i < levelTextures.Count; i++)
           {
               if (levelTextures[i].TextureID == ID) return i;
           }
           return -1;
       }

       public static void SetSpriteType(int ID, string type)
       {
           levelTextures[ID].SpriteType = type;
       }

       public static void SetAnimatedFlag(int ID, bool flag)
       {
           levelTextures[ID].IsAnimated = flag;
       }

       public static void SetTiledFlag(int ID, bool flag)
       {
           levelTextures[ID].IsTiled = flag;
       }


        #endregion

    }
}
