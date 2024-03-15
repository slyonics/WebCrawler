﻿using WebCrawler.Models;
using WebCrawler.SceneObjects.Maps;
using WebCrawler.SceneObjects.Widgets;
using WebCrawler.Scenes.ConversationScene;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebCrawler.Scenes.TitleScene
{
    public class ContinueViewModel : ViewModel
    {

        public ContinueViewModel(Scene scene)
            : base(scene, PriorityLevel.GameLevel)
        {
            var saves = GameProfile.GetAllSaveData();
            for (int i = 0; i < 3; i++)
            {
                if (saves.ContainsKey(i))
                {
                    var save = saves[i];
                    GameSprite portrait1 = (GameSprite)Enum.Parse(typeof(GameSprite), (string)save["EnviPortrait"]);
                    GameSprite portrait2 = (GameSprite)Enum.Parse(typeof(GameSprite), (string)save["SparrPortrait"]);
                    AvailableSaves.Add(new SaveModel()
                    {
                        Location = new ModelProperty<string>((string)save["PlayerLocation"]),
                        SaveSlot = new ModelProperty<int>(i),
                        Portrait1 = new ModelProperty<Texture2D>(AssetCache.SPRITES[portrait1]),
                        Portrait2 = new ModelProperty<Texture2D>(AssetCache.SPRITES[portrait2])
                    });
                }
                else
                {
                    AvailableSaves.Add(new SaveModel()
                    {
                        Location = new ModelProperty<string>("- Empty Save -"),
                        SaveSlot = new ModelProperty<int>(i),
                        Portrait1 = new ModelProperty<Texture2D>(AssetCache.SPRITES[GameSprite.Actors_Blank]),
                        Portrait2 = new ModelProperty<Texture2D>(AssetCache.SPRITES[GameSprite.Actors_Blank])
                    });
                }
            }

            LoadView(GameView.TitleScene_ContinueView);
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            if (Input.CurrentInput.CommandPressed(Command.Cancel))
            {
                Audio.PlaySound(GameSound.Back);
                Close();
            }
        }

        public void Save(object parameter)
        {
            int saveSlot;
            if (parameter is IModelProperty)
            {
                saveSlot = (int)((IModelProperty)parameter).GetValue();
            }
            else saveSlot = (int)parameter;

            if (AvailableSaves[saveSlot].Location.Value == "- Empty Save -") Audio.PlaySound(GameSound.Error);
            else
            {
                Audio.PlaySound(GameSound.Confirm);

                GameProfile.SaveSlot = saveSlot;
                GameProfile.LoadState("Save" + saveSlot + ".sav");

                string map = GameProfile.GetSaveData<string>("LastMapName");
                string entrance = GameProfile.GetSaveData<string>("LastEntrance");
                WebCrawlerGame.Transition(typeof(MapScene.MapScene), map, entrance);
            }
        }

        public ModelCollection<SaveModel> AvailableSaves { get; set; } = new ModelCollection<SaveModel>();
    }
}
