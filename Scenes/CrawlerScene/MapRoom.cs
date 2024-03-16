﻿using WebCrawler.Models;
using WebCrawler.Scenes.MapScene;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebCrawler.Scenes.CrawlerScene
{
    public class MapRoom
    {
        public class RoomWall
        {
            public Direction Orientation { get; set; }
            public VertexPositionTexture[] Quad { get; set; }
            public Vector4[] Lighting { get; set; } = new Vector4[] { new Vector4(1.0f), new Vector4(1.0f), new Vector4(1.0f), new Vector4(1.0f) };
            public Texture2D Texture { get; set; }
            public WallShader Shader { get; set; }

        }

        private const int ATLAS_ROWS = 16;
        private const int WALL_SPRITE_LENGTH = 128;
        private const int ATLAS_LENGTH = 2048;
        private const int WALL_HALF_LENGTH = 5;
        private const int CAM_HEIGHT = -1;
        private static readonly short[] INDICES = new short[] { 0, 2, 1, 2, 0, 3 };
        private static readonly Dictionary<Direction, Vector3[]> VERTICES = new Dictionary<Direction, Vector3[]>()
        {   {
                Direction.North, new Vector3[] {
                    new Vector3(-WALL_HALF_LENGTH, -WALL_HALF_LENGTH + CAM_HEIGHT, WALL_HALF_LENGTH),
                    new Vector3(-WALL_HALF_LENGTH, WALL_HALF_LENGTH + CAM_HEIGHT, WALL_HALF_LENGTH),
                    new Vector3(WALL_HALF_LENGTH, WALL_HALF_LENGTH + CAM_HEIGHT, WALL_HALF_LENGTH),
                    new Vector3(WALL_HALF_LENGTH, -WALL_HALF_LENGTH + CAM_HEIGHT, WALL_HALF_LENGTH) }
            }, {
                Direction.West, new Vector3[] {
                    new Vector3(-WALL_HALF_LENGTH, -WALL_HALF_LENGTH + CAM_HEIGHT, -WALL_HALF_LENGTH),
                    new Vector3(-WALL_HALF_LENGTH, WALL_HALF_LENGTH + CAM_HEIGHT, -WALL_HALF_LENGTH),
                    new Vector3(-WALL_HALF_LENGTH, WALL_HALF_LENGTH + CAM_HEIGHT, WALL_HALF_LENGTH),
                    new Vector3(-WALL_HALF_LENGTH, -WALL_HALF_LENGTH + CAM_HEIGHT, WALL_HALF_LENGTH) }
            }, {
                Direction.East, new Vector3[] {
                    new Vector3(WALL_HALF_LENGTH, -WALL_HALF_LENGTH + CAM_HEIGHT, WALL_HALF_LENGTH),
                    new Vector3(WALL_HALF_LENGTH, WALL_HALF_LENGTH + CAM_HEIGHT, WALL_HALF_LENGTH),
                    new Vector3(WALL_HALF_LENGTH, WALL_HALF_LENGTH + CAM_HEIGHT, -WALL_HALF_LENGTH),
                    new Vector3(WALL_HALF_LENGTH, -WALL_HALF_LENGTH + CAM_HEIGHT, -WALL_HALF_LENGTH) }
            }, {
                Direction.South, new Vector3[] {
                    new Vector3(WALL_HALF_LENGTH, -WALL_HALF_LENGTH + CAM_HEIGHT, -WALL_HALF_LENGTH),
                    new Vector3(WALL_HALF_LENGTH, WALL_HALF_LENGTH + CAM_HEIGHT, -WALL_HALF_LENGTH),
                    new Vector3(-WALL_HALF_LENGTH, WALL_HALF_LENGTH + CAM_HEIGHT, -WALL_HALF_LENGTH),
                    new Vector3(-WALL_HALF_LENGTH, -WALL_HALF_LENGTH + CAM_HEIGHT, -WALL_HALF_LENGTH) }
            }, {
                Direction.Up, new Vector3[] {
                    new Vector3(WALL_HALF_LENGTH, -WALL_HALF_LENGTH + CAM_HEIGHT, WALL_HALF_LENGTH),
                    new Vector3(WALL_HALF_LENGTH, -WALL_HALF_LENGTH + CAM_HEIGHT, -WALL_HALF_LENGTH),
                    new Vector3(-WALL_HALF_LENGTH, -WALL_HALF_LENGTH + CAM_HEIGHT, -WALL_HALF_LENGTH),
                    new Vector3(-WALL_HALF_LENGTH, -WALL_HALF_LENGTH + CAM_HEIGHT, WALL_HALF_LENGTH) }
            }, {
                Direction.Down, new Vector3[] {
                    new Vector3(WALL_HALF_LENGTH, WALL_HALF_LENGTH + CAM_HEIGHT, -WALL_HALF_LENGTH),
                    new Vector3(WALL_HALF_LENGTH, WALL_HALF_LENGTH + CAM_HEIGHT, WALL_HALF_LENGTH),
                    new Vector3(-WALL_HALF_LENGTH, WALL_HALF_LENGTH + CAM_HEIGHT, WALL_HALF_LENGTH),
                    new Vector3(-WALL_HALF_LENGTH, WALL_HALF_LENGTH + CAM_HEIGHT, -WALL_HALF_LENGTH) }
        } };

        private Texture2D minimapSprite = AssetCache.SPRITES[GameSprite.MiniMap];
        private static readonly Rectangle[] minimapSource = new Rectangle[] { new Rectangle(0, 0, 8, 8), new Rectangle(8, 0, 8, 8), new Rectangle(16, 0, 8, 8), new Rectangle(24, 0, 8, 8) };

        private MapRoom[,] mapRooms;
        public int RoomX { get; set; }
        public int RoomY { get; set; }
        public bool Blocked { get; set; }
        public string[] Script { get; set; }
        public string[] PreEnterScript { get; set; }
        public Dictionary<Direction, string[]> ActivateScript { get; set; } = new Dictionary<Direction, string[]>();

        //public WallShader WallEffect { get; private set; }
        private GraphicsDevice graphicsDevice = CrossPlatformCrawlerGame.GameInstance.GraphicsDevice;

        private CrawlerScene parentScene;
        private Matrix translationMatrix;

        private GameSprite defaultWall;

        private bool door = false;
        int waypointTile;

        private Dictionary<Direction, RoomWall> wallList = new Dictionary<Direction, RoomWall>();

        public int brightnessLevel = 0;
        private float[] lightVertices;

        public MapRoom(CrawlerScene mapScene, MapRoom[,] iMapRooms, int x, int y, string wallSprite)
        {
            parentScene = mapScene;
            mapRooms = iMapRooms;
            RoomX = x;
            RoomY = y;
            defaultWall = (GameSprite)Enum.Parse(typeof(GameSprite), "Walls_" + wallSprite);
            waypointTile = 1;
        }

        public void ApplyTile()
        {
            /*
            switch (layerName)
            {
                case "Walls":
                    Blocked = true;
                    if (RoomX > 0 && mapRooms[RoomX - 1, RoomY] != null && !mapRooms[RoomX - 1, RoomY].Blocked)
                        mapRooms[RoomX - 1, RoomY].ApplyWall(Direction.East, AssetCache.SPRITES[(GameSprite)Enum.Parse(typeof(GameSprite), "Walls_" + Path.GetFileNameWithoutExtension(tiledTileset.Tiles[gid].image.source))]);

                    if (RoomX < mapRooms.GetLength(0) - 1 && mapRooms[RoomX + 1, RoomY] != null && !mapRooms[RoomX + 1, RoomY].Blocked)
                        mapRooms[RoomX + 1, RoomY].ApplyWall(Direction.West, AssetCache.SPRITES[(GameSprite)Enum.Parse(typeof(GameSprite), "Walls_" + Path.GetFileNameWithoutExtension(tiledTileset.Tiles[gid].image.source))]);

                    if (RoomY > 0 && mapRooms[RoomX, RoomY - 1] != null && !mapRooms[RoomX, RoomY - 1].Blocked)
                        mapRooms[RoomX, RoomY - 1].ApplyWall(Direction.South, AssetCache.SPRITES[(GameSprite)Enum.Parse(typeof(GameSprite), "Walls_" + Path.GetFileNameWithoutExtension(tiledTileset.Tiles[gid].image.source))]);

                    if (RoomY < mapRooms.GetLength(1) - 1 && mapRooms[RoomX, RoomY + 1] != null && !mapRooms[RoomX, RoomY + 1].Blocked)
                        mapRooms[RoomX, RoomY + 1].ApplyWall(Direction.North, AssetCache.SPRITES[(GameSprite)Enum.Parse(typeof(GameSprite), "Walls_" + Path.GetFileNameWithoutExtension(tiledTileset.Tiles[gid].image.source))]);
                    break;

                case "Ceiling":
                    wallList.Add(Direction.Up, new RoomWall() { Orientation = Direction.Up, Texture = AssetCache.SPRITES[(GameSprite)Enum.Parse(typeof(GameSprite), "Walls_" + Path.GetFileNameWithoutExtension(tiledTileset.Tiles[gid].image.source))] });
                    break;

                case "Floor":
                    wallList.Add(Direction.Down, new RoomWall() { Orientation = Direction.Down, Texture = AssetCache.SPRITES[(GameSprite)Enum.Parse(typeof(GameSprite), "Walls_" + Path.GetFileNameWithoutExtension(tiledTileset.Tiles[gid].image.source))] });
                    break;
            }
            */

            ResetMinimapIcon();
        }

        public void ApplyWall(Direction direction, Texture2D texture2D)
        {
            if (wallList.TryGetValue(direction, out var wall))
            {
                wall.Texture = texture2D;
            }
            else
            {
                wallList.Add(direction, new RoomWall()
                {
                    Orientation = direction,
                    Texture = texture2D
                });
            }
        }

        public void SetVertices(int x, int y)
        {
            translationMatrix = Matrix.CreateTranslation(new Vector3(10 * (x), 0, 10 * (mapRooms.GetLength(1) - y)));

            foreach (KeyValuePair<Direction, RoomWall> wall in wallList)
            {
                VertexPositionTexture[] quad = new VertexPositionTexture[4];
                quad[0] = new VertexPositionTexture(VERTICES[wall.Value.Orientation][0], new Vector2(0.0f, 0.0f));
                quad[1] = new VertexPositionTexture(VERTICES[wall.Value.Orientation][1], new Vector2(0.0f, 1.0f));
                quad[2] = new VertexPositionTexture(VERTICES[wall.Value.Orientation][2], new Vector2(1.0f, 1.0f));
                quad[3] = new VertexPositionTexture(VERTICES[wall.Value.Orientation][3], new Vector2(1.0f, 0.0f));
                wall.Value.Quad = quad;
            }

            BuildShader();
        }

        private void BuildShader()
        {
            foreach (KeyValuePair<Direction, RoomWall> wall in wallList)
            {
                wall.Value.Shader = new WallShader(Matrix.CreatePerspectiveFieldOfView((float)Math.PI / 2f, 472 / 332.0f, 0.7f, 10000.0f));
                wall.Value.Shader.World = translationMatrix;
                wall.Value.Shader.WallTexture = wall.Value.Texture;

                Vector4 brightness;

                switch (wall.Value.Orientation)
                {
                    case Direction.Up: brightness = new Vector4(Brightness(lightVertices[2]), Brightness(lightVertices[3]), Brightness(lightVertices[0]), Brightness(lightVertices[1])); break;
                    case Direction.North: brightness = new Vector4(Brightness(lightVertices[1]), Brightness(lightVertices[0]), Brightness(lightVertices[3]), Brightness(lightVertices[2])); break;
                    case Direction.East: brightness = new Vector4(Brightness(lightVertices[3]), Brightness(lightVertices[1]), Brightness(lightVertices[2]), Brightness(lightVertices[0])); break;
                    case Direction.West: brightness = new Vector4(Brightness(lightVertices[0]), Brightness(lightVertices[2]), Brightness(lightVertices[1]), Brightness(lightVertices[3])); break;
                    default: brightness = new Vector4(Brightness(lightVertices[0]), Brightness(lightVertices[1]), Brightness(lightVertices[2]), Brightness(lightVertices[3])); break;
                }

                wall.Value.Shader.Brightness = brightness;
            }
        }

        public float Brightness(float x) { return Math.Min(1.0f, Math.Max(x / 4.0f, parentScene.AmbientLight)); }

        public void BlendLighting()
        {
            lightVertices = new float[] { 0.25f, 0.25f, 0.25f, 0.25f };

            int[] neighborBrightness = new int[9];
            int i = 0;
            for (int x = -1; x <= 1; x++)
            {
                for (int y = -1; y <= 1; y++)
                {
                    if (RoomX + x < 0 || RoomY + y < 0 || RoomX + x >= mapRooms.GetLength(0) || RoomY + y >= mapRooms.GetLength(1)) neighborBrightness[i] = brightnessLevel;
                    else
                    {
                        MapRoom mapRoom = mapRooms[RoomX + x, RoomY + y];
                        neighborBrightness[i] = mapRoom == null || mapRoom.Blocked ? brightnessLevel : mapRoom.brightnessLevel;
                    }
                    i++;
                }
            }

            List<MapRoom> nw = new List<MapRoom>() { this };
            if (Neighbors.Contains(this[Direction.North])) { nw.Add(this[Direction.North]); if (this[Direction.North].Neighbors.Contains(this[Direction.North][Direction.West])) nw.Add(this[Direction.North][Direction.West]); }
            if (Neighbors.Contains(this[Direction.West])) { nw.Add(this[Direction.West]); nw.Add(this[Direction.West]); if (this[Direction.West].Neighbors.Contains(this[Direction.West][Direction.North])) nw.Add(this[Direction.West][Direction.North]); }
            lightVertices[0] = (float)nw.Distinct().Average(x => x.brightnessLevel);

            List<MapRoom> ne = new List<MapRoom>() { this };
            if (Neighbors.Contains(this[Direction.North])) { ne.Add(this[Direction.North]); if (this[Direction.North].Neighbors.Contains(this[Direction.North][Direction.East])) ne.Add(this[Direction.North][Direction.East]); }
            if (Neighbors.Contains(this[Direction.East])) { ne.Add(this[Direction.East]); ne.Add(this[Direction.East]); if (this[Direction.East].Neighbors.Contains(this[Direction.East][Direction.North])) ne.Add(this[Direction.East][Direction.North]); }
            lightVertices[1] = (float)ne.Distinct().Average(x => x.brightnessLevel);

            List<MapRoom> sw = new List<MapRoom>() { this };
            if (Neighbors.Contains(this[Direction.South])) { sw.Add(this[Direction.South]); if (this[Direction.South].Neighbors.Contains(this[Direction.South][Direction.West])) sw.Add(this[Direction.South][Direction.West]); }
            if (Neighbors.Contains(this[Direction.West])) { sw.Add(this[Direction.West]); sw.Add(this[Direction.West]); if (this[Direction.West].Neighbors.Contains(this[Direction.West][Direction.South])) sw.Add(this[Direction.West][Direction.South]); }
            lightVertices[2] = (float)sw.Distinct().Average(x => x.brightnessLevel);

            List<MapRoom> se = new List<MapRoom>() { this };
            if (Neighbors.Contains(this[Direction.South])) { se.Add(this[Direction.South]); if (this[Direction.South].Neighbors.Contains(this[Direction.South][Direction.East])) se.Add(this[Direction.South][Direction.East]); }
            if (Neighbors.Contains(this[Direction.East])) { se.Add(this[Direction.East]); se.Add(this[Direction.East]); if (this[Direction.East].Neighbors.Contains(this[Direction.East][Direction.South])) se.Add(this[Direction.East][Direction.South]); }
            lightVertices[3] = (float)se.Distinct().Average(x => x.brightnessLevel);
        }

        public void Draw(Matrix viewMatrix)
        {
            if (Blocked) return;

            foreach (KeyValuePair<Direction, RoomWall> wall in wallList)
            {
                DrawWall(wall.Value, viewMatrix);
            }
        }

        public void DrawWall(RoomWall wall, Matrix viewMatrix)
        {
            wall.Shader.View = viewMatrix;
            foreach (EffectPass pass in wall.Shader.Effect.CurrentTechnique.Passes)
            {
                pass.Apply();
                graphicsDevice.DrawUserIndexedPrimitives(PrimitiveType.TriangleList, wall.Quad, 0, 4, INDICES, 0, 2);
            }
        }

        public void DrawMinimap(SpriteBatch spriteBatch, Vector2 offset, float depth)
        {
            spriteBatch.Draw(minimapSprite, offset, minimapSource[waypointTile], Color.White, 0.0f, Vector2.Zero, 1.0f, SpriteEffects.None, depth);
        }

        public void ResetMinimapIcon()
        {
            waypointTile = Blocked ? 0 : 1;
            if (door) waypointTile = 2;
        }

        public void BuildNeighbors()
        {
            if (!wallList.ContainsKey(Direction.West))
            {
                if (RoomX > 0 && mapRooms[RoomX - 1, RoomY] != null && !mapRooms[RoomX - 1, RoomY].Blocked) Neighbors.Add(mapRooms[RoomX - 1, RoomY]);
                else wallList.Add(Direction.West, new RoomWall() { Orientation = Direction.West, Texture = AssetCache.SPRITES[defaultWall] });
            }

            if (!wallList.ContainsKey(Direction.East))
            {
                if (RoomX < mapRooms.GetLength(0) - 1 && mapRooms[RoomX + 1, RoomY] != null && !mapRooms[RoomX + 1, RoomY].Blocked) Neighbors.Add(mapRooms[RoomX + 1, RoomY]);
                else wallList.Add(Direction.East, new RoomWall() { Orientation = Direction.East, Texture = AssetCache.SPRITES[defaultWall] });
            }

            if (!wallList.ContainsKey(Direction.North))
            {
                if (RoomY > 0 && mapRooms[RoomX, RoomY - 1] != null && !mapRooms[RoomX, RoomY - 1].Blocked) Neighbors.Add(mapRooms[RoomX, RoomY - 1]);
                else wallList.Add(Direction.North, new RoomWall() { Orientation = Direction.North, Texture = AssetCache.SPRITES[defaultWall] });
            }

            if (!wallList.ContainsKey(Direction.South))
            {
                if (RoomY < mapRooms.GetLength(1) - 1 && mapRooms[RoomX, RoomY + 1] != null && !mapRooms[RoomX, RoomY + 1].Blocked) Neighbors.Add(mapRooms[RoomX, RoomY + 1]);
                else if (!wallList.ContainsKey(Direction.South)) wallList.Add(Direction.South, new RoomWall() { Orientation = Direction.South, Texture = AssetCache.SPRITES[defaultWall] });
            }
        }

        public bool Activate(Direction direction)
        {
            string[] script;
            if (ActivateScript.TryGetValue(direction, out script))
            {
                EventController eventController = new EventController(parentScene, script, this);
                parentScene.AddController(eventController);
                parentScene.ResetPathfinding();

                return true;
            }
            else return false;
        }

        public void ActivatePreScript()
        {
            if (PreEnterScript != null)
            {
                EventController eventController = new EventController(parentScene, PreEnterScript, this);
                parentScene.AddController(eventController);
                parentScene.ResetPathfinding();
            }
        }

        public void EnterRoom()
        {

            if (Script != null)
            {
                EventController eventController = new EventController(parentScene, Script, this);
                parentScene.AddController(eventController);
                parentScene.ResetPathfinding();
            }

        }



        public void SetAsWaypoint()
        {
            waypointTile = 3;
        }


        public List<MapRoom> Neighbors { get; private set; } = new List<MapRoom>();

        public MapRoom this[Direction key]
        {
            get
            {
                switch (key)
                {
                    case Direction.North: return parentScene.GetRoom(RoomX, RoomY - 1);
                    case Direction.East: return parentScene.GetRoom(RoomX + 1, RoomY);
                    case Direction.South: return parentScene.GetRoom(RoomX, RoomY + 1);
                    case Direction.West: return parentScene.GetRoom(RoomX - 1, RoomY);
                    default: return null;
                }
            }
        }
    }
}
