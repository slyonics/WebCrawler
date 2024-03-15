using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Input.Touch;
using Microsoft.Xna.Framework.Media;
using System;
using System.Collections.Generic;

namespace WebCrawler
{
    /// <summary>
    /// This is the main type for your game.
    /// </summary>
    public class WebCrawlerGame : Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;

        Texture2D splash;
        public WebCrawlerGame()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";

        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            // TODO: Add your initialization logic here

            base.Initialize();

        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);

            // TODO: Use this.Content to load your game content here
            splash = Content.Load<Texture2D>("Graphics\\Background\\Splash");

            effect = Content.Load<Effect>("Shaders\\ColorFade");
            effect.Parameters["filterRed"].SetValue(1.0f);
            effect.Parameters["filterGreen"].SetValue(1.0f);
            effect.Parameters["filterBlue"].SetValue(1.0f);
            effect.Parameters["amount"].SetValue(0.5f);


        }
        Effect effect;
        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// game-specific content.
        /// </summary>
        protected override void UnloadContent()
        {
            // TODO: Unload any non ContentManager content here
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            MouseState mouseState = Mouse.GetState();
            KeyboardState keyboardState = Keyboard.GetState();
            GamePadState gamePadState = default;
            try { gamePadState = GamePad.GetState(PlayerIndex.One); }
            catch (NotImplementedException) { /* ignore gamePadState */ }

            if (keyboardState.IsKeyDown(Keys.Escape) ||
                keyboardState.IsKeyDown(Keys.Back) ||
                gamePadState.Buttons.Back == ButtonState.Pressed)
            {
                try { Exit(); }
                catch (PlatformNotSupportedException) { /* ignore */ }
            }

            // TODO: Add your update logic here

            base.Update(gameTime);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            // TODO: Add your drawing code here
            
            int width = graphics.PreferredBackBufferWidth;
            int height = graphics.PreferredBackBufferHeight;

            spriteBatch.Begin(SpriteSortMode.FrontToBack, BlendState.AlphaBlend, SamplerState.PointWrap, DepthStencilState.Default, RasterizerState.CullCounterClockwise, effect, null);
            spriteBatch.Draw(splash, new Rectangle(0, 0, width, height), new Rectangle(0, 0, 1, 1), Color.White, 0.0f, Vector2.Zero, SpriteEffects.None, 1.0f);
            spriteBatch.Draw(splash, new Rectangle((width - splash.Width) / 2, (height - splash.Height) / 2, splash.Width, splash.Height), null, Color.White, 0.0f, Vector2.Zero, SpriteEffects.None, 0.0f);
            spriteBatch.End();
            

            base.Draw(gameTime);
        }
    }
}
