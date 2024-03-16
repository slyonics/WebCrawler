﻿global using WebCrawler.Main;
global using WebCrawler.SceneObjects;
global using Microsoft.Xna.Framework;
using WebCrawler.Models;
using WebCrawler.SceneObjects.Controllers;
using WebCrawler.SceneObjects.Shaders;
using WebCrawler.Scenes.ConversationScene;
using WebCrawler.Scenes.SplashScene;
using WebCrawler.Scenes.IntroScene;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using WebCrawler.Scenes.CrawlerScene;

namespace WebCrawler.Main
{
    public class CrossPlatformCrawlerGame : Game
    {
        public const string CONTENT_DIRECTORY = "Assets";
        public const string GAME_NAME = "Elenigma";

        private const int TARGET_SCREEN_WIDTH = 580;
        private const int TARGET_SCREEN_HEIGHT = 360;
        private const int MAXIMUM_SCREEN_WIDTH = 1920;
        private const int MAXIMUM_SCREEN_HEIGHT = 1080;

        public static readonly Color CLEAR_COLOR = new Color(100, 98, 93, 255);

        private GraphicsDeviceManager graphicsDeviceManager;
        private SpriteBatch spriteBatch;

        private RenderTarget2D gameRender;
        private RenderTarget2D compositeRender;

        private int originalHeight;

        private static int scaledScreenWidth = TARGET_SCREEN_WIDTH;
        private static int scaledScreenHeight = TARGET_SCREEN_HEIGHT;
        private static int screenScale = 1;
        private bool fullscreen = false;
        private static Scene pendingScene;
        public static Scene CurrentScene { get; private set; }
        private static List<Scene> sceneStack = new List<Scene>();
        public static List<Scene> SceneStack { get => sceneStack; }

        private static CrossPlatformCrawlerGame crossPlatformGame;

        public CrossPlatformCrawlerGame()
        {
            crossPlatformGame = this;
            graphicsDeviceManager = new GraphicsDeviceManager(this);
            graphicsDeviceManager.GraphicsProfile = GraphicsProfile.HiDef;
            Content.RootDirectory = CONTENT_DIRECTORY;

            graphicsDeviceManager.SupportedOrientations = DisplayOrientation.LandscapeLeft | DisplayOrientation.LandscapeRight;
#if (ANDROID || iOS)
            graphicsDeviceManager.IsFullScreen = true;
#endif

            Exiting += CrossPlatformGame_Exiting;
        }

        protected override void UnloadContent()
        {
            Audio.Deinitialize();
        }

        protected override void Initialize()
        {
            originalHeight = 1080; // GraphicsDevice.Adapter.CurrentDisplayMode.TitleSafeArea.Height;

            spriteBatch = new SpriteBatch(GraphicsDevice);
            Settings.LoadSettings();
            //Audio.ApplySettings();
            //Input.ApplySettings();
            //ApplySettings();

            base.Initialize();

            //Debug.Initialize(GraphicsDevice);
            //Audio.Initialize();
            //Text.Initialize(GraphicsDevice);

            AssetCache.LoadContent(GraphicsDevice);

            //ConversationScene.Initialize();
            //ItemRecord.ITEMS = AssetCache.LoadRecords<ItemRecord>("ItemData");

            CurrentScene = new IntroScene();
        }

        public void ApplySettings()
        {
            /*
            // fullscreen = Settings.GetProgramSetting<bool>("Fullscreen");
            string targetResolution = Settings.GetProgramSetting<string>("TargetResolution");

            if (fullscreen)
            {
                DisplayModeCollection displayModes = GraphicsDevice.Adapter.SupportedDisplayModes;
                IEnumerable<DisplayMode> bestModes = displayModes.Where(x => x.Width >= TARGET_SCREEN_WIDTH && x.Width <= MAXIMUM_SCREEN_WIDTH &&
                                                                                x.Height >= TARGET_SCREEN_HEIGHT && x.Height <= MAXIMUM_SCREEN_HEIGHT);

                DisplayMode targetMode = bestModes.OrderByDescending(x => x.Width).FirstOrDefault();
                scaledScreenWidth = targetMode.Width;
                scaledScreenHeight = targetMode.Height;
                int scale = targetMode.Height / TARGET_SCREEN_HEIGHT;
                screenScale = scale;
            }
            else
            {
                if (targetResolution == "Best Fit")
                {
                    int availableHeight = originalHeight;
                    int scale = availableHeight / TARGET_SCREEN_HEIGHT;

                    screenScale = scale;
                    scaledScreenWidth = TARGET_SCREEN_WIDTH * scale;
                    scaledScreenHeight = TARGET_SCREEN_HEIGHT * scale;
                }
                else
                {
                    screenScale = 1;
                    scaledScreenWidth = TARGET_SCREEN_WIDTH;
                    scaledScreenHeight = TARGET_SCREEN_HEIGHT;

                }
            }
            */

            screenScale = 1;
            scaledScreenWidth = TARGET_SCREEN_WIDTH;
            scaledScreenHeight = TARGET_SCREEN_HEIGHT;

            int multiSamples = 0;
            if (Settings.GetProgramSetting<bool>("Antialiasing"))
            {
                multiSamples = 16;
            }

            // IsMouseVisible = true;
            IsMouseVisible = false;

            //graphicsDeviceManager.IsFullScreen = fullscreen;
            //graphicsDeviceManager.PreferredBackBufferWidth = scaledScreenWidth;
            //graphicsDeviceManager.PreferredBackBufferHeight = scaledScreenHeight;
            //graphicsDeviceManager.ApplyChanges();

            gameRender = new RenderTarget2D(graphicsDeviceManager.GraphicsDevice, ScreenWidth, ScreenHeight);
            compositeRender = new RenderTarget2D(graphicsDeviceManager.GraphicsDevice, ScreenWidth, ScreenHeight, false, SurfaceFormat.Color, DepthFormat.Depth16, 0, RenderTargetUsage.PreserveContents);

            CrawlerScene.Initialize(GraphicsDevice, multiSamples);
        }

        private void CrossPlatformGame_Exiting(object sender, EventArgs e)
        {
            Settings.SaveSettings();
        }

        protected override void Update(GameTime gameTime)
        {
            Input.Update(gameTime);

            if (Settings.GetProgramSetting<bool>("DebugMode") && Input.CurrentInput.CommandPressed(Command.Menu))
            {
                Transition(typeof(SplashScene));
            }

            if (TransitionShader != null)
            {
                CurrentScene.Update(gameTime);
                TransitionShader.Update(gameTime, null);
                if (TransitionShader.Terminated) TransitionShader = null;
            }
            else
            {
                int i = 0;
                while (i < sceneStack.Count)
                {
                    sceneStack[i].Update(gameTime);
                    i++;
                }

                CurrentScene.Update(gameTime);
            }

            if (pendingScene != null)
            {
                sceneStack.Clear();
                CrossPlatformCrawlerGame.SetCurrentScene(pendingScene);
                pendingScene = null;
            }

            while (CurrentScene.SceneEnded && sceneStack.Count > 0)
            {
                CurrentScene = sceneStack.Last();
                CurrentScene.ResumeScene();
                sceneStack.Remove(CurrentScene);
                TransitionShader = null;
            }

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            ClearedCompositeRender = false;

            lock (sceneStack)
            {
                foreach (Scene scene in sceneStack)
                {
                    scene.Draw(GraphicsDevice, spriteBatch, gameRender, compositeRender);
                }
            }

            CurrentScene.Draw(GraphicsDevice, spriteBatch, gameRender, compositeRender);

            Effect shader = (TransitionShader == null) ? null : TransitionShader.Effect;
            GraphicsDevice.SetRenderTarget(null);
            spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Opaque, SamplerState.PointClamp, DepthStencilState.Default, RasterizerState.CullCounterClockwise, shader, Matrix.CreateScale((int)Scale, (int)Scale, 1));
            spriteBatch.Draw(compositeRender, Vector2.Zero, Color.White);
            spriteBatch.End();

            base.Draw(gameTime);
        }

        public static void ShowCursor(bool show = true)
        {
            GameInstance.IsMouseVisible = show;
        }

        public static void Transition(Type sceneType, params object[] args)
        {
            Transition(CurrentScene, sceneType, args);
        }

        public static void Transition(Scene parentScene, Type sceneType, params object[] args)
        {
            TransitionController transitionController = new TransitionController(TransitionDirection.Out, 600);
            ColorFade colorFade = new ColorFade(Color.Black, transitionController.TransitionProgress);
            transitionController.UpdateTransition += new Action<float>(t => colorFade.Amount = t);
            parentScene.AddController(transitionController);
            TransitionShader = colorFade;

            Task.Run(() => Activator.CreateInstance(sceneType, args)).ContinueWith(t =>
            {
                while (!transitionController.Terminated) ;
                pendingScene = (Scene)t.Result;
            });
        }

        public static void SetCurrentScene(Scene newScene)
        {
            CurrentScene.EndScene();

            TransitionShader.Terminate();
            CurrentScene = newScene;
            newScene.BeginScene();
        }

        public static void StackScene(Scene newScene, bool suspended = false)
        {
            lock (sceneStack)
            {
                sceneStack.Add(CurrentScene);
            }

            CurrentScene.Suspended = suspended;
            var oldScene = CurrentScene;

            if (newScene != null) newScene.OnTerminated += new TerminationFollowup(() => oldScene.Suspended = false);

            Task.Run(() =>
            {
                while (newScene == null) Thread.Sleep(100);
                CurrentScene = newScene;
                newScene.BeginScene();
            });
        }

        public static T GetScene<T>() where T : Scene
        {
            if (CurrentScene is T) return (T)CurrentScene;
            else
            {
                T result;
                lock (sceneStack)
                {
                    result = (T)sceneStack.FirstOrDefault(x => x is T);
                }

                return result;
            }
        }

        public static Shader TransitionShader { get; set; }

        public static int ScreenWidth { get => scaledScreenWidth / screenScale; }
        public static int ScreenHeight { get => scaledScreenHeight / screenScale; }
        public static int Scale { get => screenScale; }
        public static CrossPlatformCrawlerGame GameInstance { get => crossPlatformGame; }
        public static bool ClearedCompositeRender { get; set; }
    }
}
