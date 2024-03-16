﻿using WebCrawler.Models;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebCrawler.Scenes.MapScene
{
    public class InteractionPrompt : Overlay
    {
        private const string PROMPT_FRAME = "LightFrame";
        private const GameFont PROMPT_FONT = GameFont.Interface;

        private MapScene mapScene;
        private IInteractive target;

        private NinePatch textbox;

        private Color color = new Color(173, 119, 87);

        public InteractionPrompt(MapScene iMapScene)
        {
            mapScene = iMapScene;
            textbox = new NinePatch(PROMPT_FRAME, 0.05f);
        }

        public override void Update(GameTime gameTime)
        {

        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            if (CrossPlatformCrawlerGame.CurrentScene != mapScene) return;

            if (target != null && mapScene.PriorityLevel == PriorityLevel.GameLevel)
            {
                string[] textLines = target.Label.Split('_');
                string longestLine = textLines.MaxBy(x => Text.GetStringLength(PROMPT_FONT, x));
                int width = Text.GetStringLength(PROMPT_FONT, longestLine);
                int height = Text.GetStringHeight(PROMPT_FONT);
                textbox.Bounds = new Rectangle(0, 0, width + 10, height * textLines.Count() + 3);
                Vector2 cameraOffset = new Vector2(mapScene.Camera.CenteringOffsetX, mapScene.Camera.CenteringOffsetY);

                Vector2 drawPosition = target.LabelPosition;
                if (drawPosition.X - (textbox.Bounds.Width / 2) <= mapScene.Camera.Position.X) drawPosition.X = mapScene.Camera.Position.X + (textbox.Bounds.Width / 2);
                if (drawPosition.X + (textbox.Bounds.Width / 2) - mapScene.Camera.Position.X >= CrossPlatformCrawlerGame.ScreenWidth)
                {
                    drawPosition.X = CrossPlatformCrawlerGame.ScreenWidth - (textbox.Bounds.Width / 2) + mapScene.Camera.Position.X;
                }

                textbox.Draw(spriteBatch, drawPosition - mapScene.Camera.Position - new Vector2(textbox.Bounds.Width / 2, 0) - cameraOffset);

                int row = 0;
                foreach (string text in textLines)
                {
                    Text.DrawCenteredText(spriteBatch, drawPosition + new Vector2(1, 7) - mapScene.Camera.Position - cameraOffset, PROMPT_FONT, text, color, 0.03f, row);
                    row++;
                }
            }
        }

        public void Target(IInteractive newTarget)
        {
            target = newTarget;
        }
    }
}
