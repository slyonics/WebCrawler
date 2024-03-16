using WebCrawler.Main;
using WebCrawler.SceneObjects.Controllers;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebCrawler.SceneObjects
{
    public class Camera
    {
        public const float MINIMUM_ENTITY_DEPTH = 0.2f;
        public const float MAXIMUM_ENTITY_DEPTH = 0.8f;
        public const int LARGEST_ENTITY_SIZE = 32;

        private Controller controller;

        private protected Vector2 position;
        private protected Matrix matrix;
        private protected Rectangle bounds;
        private protected Rectangle view;
        private bool stationary;

        public Camera(Rectangle iBounds)
        {
            bounds = iBounds;
            ClampBounds();
        }

        public void Center(Vector2 target)
        {
            position = target - new Vector2(CrossPlatformCrawlerGame.ScreenWidth / 2, CrossPlatformCrawlerGame.ScreenHeight / 2);
            ClampBounds();
        }

        private protected void ClampBounds()
        {
            if (position.X < bounds.Left) position.X = bounds.Left;
            if (position.X > bounds.Right - CrossPlatformCrawlerGame.ScreenWidth) position.X = bounds.Right - CrossPlatformCrawlerGame.ScreenWidth;
            if (position.Y < bounds.Top) position.Y = bounds.Top;
            if (position.Y > bounds.Bottom - CrossPlatformCrawlerGame.ScreenHeight) position.Y = bounds.Bottom - CrossPlatformCrawlerGame.ScreenHeight;

            matrix = Matrix.CreateTranslation(new Vector3(-((int)position.X + CenteringOffsetX), -((int)position.Y + CenteringOffsetY), 0.0f));
            view = new Rectangle((int)position.X + CenteringOffsetX, (int)position.Y + CenteringOffsetY, CrossPlatformCrawlerGame.ScreenWidth, CrossPlatformCrawlerGame.ScreenHeight);
        }

        public float GetDepth(float z)
        {
            int bottomZ = view.Bottom + LARGEST_ENTITY_SIZE - view.Top;
            float entityZ = z - view.Top;
            float factorZ = entityZ / bottomZ;
            float depth = MathHelper.Lerp(MAXIMUM_ENTITY_DEPTH, MINIMUM_ENTITY_DEPTH, factorZ);

            return depth;
        }

        public Vector2 Position
        {
            get
            {
                return position;
            }

            set
            {
                position = value;
                ClampBounds();
            }
        }

        public Matrix Matrix { get => matrix; }
        public Rectangle View { get => view; }
        public Controller Controller { set => controller = value; }
        public bool Stationary { get => stationary; set => stationary = value; }
        public int CenteringOffsetX { get => Math.Max(0, (CrossPlatformCrawlerGame.ScreenWidth - bounds.Width) / 2); }
        public int CenteringOffsetY { get => Math.Max(0, (CrossPlatformCrawlerGame.ScreenHeight - bounds.Height) / 2); }
        public int MaxVisibleY { get => view.Bottom + LARGEST_ENTITY_SIZE; }
    }
}
