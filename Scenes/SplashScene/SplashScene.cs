using WebCrawler.Main;
using WebCrawler.Models;
using WebCrawler.SceneObjects.Controllers;
using WebCrawler.SceneObjects.Maps;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebCrawler.Scenes.SplashScene
{
    public class SplashScene : Scene, ISkippableWait
    {
        private Texture2D splashSprite = AssetCache.SPRITES[GameSprite.Background_Splash];

        public SplashScene()
            : base()
        {
            AddController(new SkippableWaitController(PriorityLevel.MenuLevel, this));
        }

        public override void DrawBackground(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(splashSprite, new Rectangle(0, 0, CrossPlatformCrawlerGame.ScreenWidth, CrossPlatformCrawlerGame.ScreenHeight), new Rectangle(0, 0, 1, 1), Color.White, 0.0f, Vector2.Zero, SpriteEffects.None, 1.0f);
            spriteBatch.Draw(splashSprite, new Rectangle((CrossPlatformCrawlerGame.ScreenWidth - splashSprite.Width) / 2, (CrossPlatformCrawlerGame.ScreenHeight - splashSprite.Height) / 2, splashSprite.Width, splashSprite.Height), null, Color.White, 0.0f, Vector2.Zero, SpriteEffects.None, 0.0f);
        }

        public void Notify(SkippableWaitController sender)
        {
            CrossPlatformCrawlerGame.Transition(typeof(IntroScene.IntroScene));
        }

        public bool Terminated { get => false; }
    }
}
