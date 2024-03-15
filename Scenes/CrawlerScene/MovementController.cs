﻿using WebCrawler.Main;
using WebCrawler.Models;
using WebCrawler.SceneObjects;
using WebCrawler.SceneObjects.Controllers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebCrawler.Scenes.CrawlerScene
{
    public class MovementController : Controller
    {
        private CrawlerScene mapScene;

        public MovementController(CrawlerScene iScene) : base(PriorityLevel.GameLevel)
        {
            mapScene = iScene;
        }

        public override void PreUpdate(GameTime gameTime)
        {
            InputFrame inputFrame = Input.CurrentInput;
            if (inputFrame.CommandDown(Command.Left)) { Path.Clear(); mapScene.TurnLeft(); }
            else if (inputFrame.CommandDown(Command.Right)) { Path.Clear(); mapScene.TurnRight(); }
            else if (inputFrame.CommandDown(Command.Up)) { Path.Clear(); mapScene.MoveForward(); }
            else if (Input.CurrentInput.CommandPressed(Command.Confirm))
            {
                Path.Clear();
                mapScene.Activate();

                /*
                mapViewModel.ModelProperties["MapActor"].Value = "Actors_Commando";

                Task.Run(() => Activator.CreateInstance(typeof(MatchScene.MatchScene))).ContinueWith(t =>
                {
                    WebCrawlerGame.StackScene((Scene)t.Result);
                });
                */
            }
            else if (Input.CurrentInput.CommandPressed(Command.Menu))
            {
                Path.Clear();

                //mapScene.AddView(new MenuViewModel(mapScene));

                /*

                Controller suspendController = mapScene.AddController(new Controller(PriorityLevel.MenuLevel));

                StatusScene.StatusScene statusScene = new StatusScene.StatusScene();
                statusScene.OnTerminated += new TerminationFollowup(suspendController.Terminate);
                WebCrawlerGame.StackScene(statusScene);

                */

                return;
            }
            else if (Path.Count > 0)
            {
                MapRoom nextRoom = Path.First();
                if (mapScene.roomX == nextRoom.RoomX && mapScene.roomY == nextRoom.RoomY) Path.RemoveAt(0);
                else mapScene.MoveTo(nextRoom);
            }
        }

        public List<MapRoom> Path { get; set; } = new List<MapRoom>();
    }
}
