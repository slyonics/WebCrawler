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

namespace WebCrawler.Scenes.IntroScene
{
    public class IntroScene : Scene
    {
        public IntroScene()
            : base()
        {
            AddView(new IntroViewModel(this, GameView.IntroScene_IntroView));
        }

        public void Notify(SkippableWaitController sender)
        {
            WebCrawlerGame.Transition(typeof(TitleScene.TitleScene));

            /*
            if (GameProfile.SaveList.Count > 0)
            {
                WebCrawlerGame.Transition(typeof(TitleScene.TitleScene));
            }
            else
            {
                NewGame();
            }
            */
        }

        public override void DrawBackground(SpriteBatch spriteBatch)
        {
            base.DrawBackground(spriteBatch);
        }

        public static void NewGame()
        {
            WebCrawlerGame.Transition(typeof(ConversationScene.ConversationScene), "Prologue", new Rectangle(-140, 25, 280, 62), true);

            // WebCrawlerGame.Transition(typeof(MapScene.MapScene), GameMap.TechWorldIntro, 19, 33, Orientation.Down);
        }

        public bool Terminated { get => false; }
    }
}
