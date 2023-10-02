using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using VitchinnikMonoCore.GUI;

namespace VitchinnikMonoCore
{
    public static class Core
    {
        public static Game GameInstance { get; private set; }
        //need tryLoad resources
        public static ContentManager ContentManagerInstance { get; private set; }
        public static SpriteBatch SpriteBatchInstance { get; private set; }
        public static RenderTarget2D DefaultRenderTarget { get; internal set; }
        public static void Initialize(Game game, ContentManager contentManager, SpriteBatch spriteBatch, RenderTarget2D defaultTarget) 
        {
            GameInstance = game;
            ContentManagerInstance = contentManager;
            SpriteBatchInstance = spriteBatch;
            DefaultRenderTarget = defaultTarget;
            MouseHandler.Initialize();
        }
        public static void Update(GameTime gameTime)
        {
            MouseHandler.Update(gameTime);
            Tooltip.Actual.Update(gameTime);
        }
    }
}