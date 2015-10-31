using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
using System.IO;

namespace BatteryDerby
{

    public class MapBuilder : DrawableGameComponent
    {

        public const int TILE_SIZE = 96;
        private List<String> textures = new List<String>();
        private List<String> obstacleModels = new List<String>();

        private int[,] layout = new int[13, 17];

        private int[,] layoutObstacleModels = new int[13, 17];

        public const int MINX = 96;
        public const int MAXX = 1440;
        public const int MINY = 96;
        public const int MAXY = 1056;

        public int Width
        {
            get { return layout.GetLength(1); }
        }

        public int Height
        {
            get { return layout.GetLength(0); }
        }
        public MapBuilder(Game game)
            : base(game)
        {

            /// keep list of all loaded tile textures.
            textures.Add(@"Models/Tiles/TileDirt"); // index 0
            textures.Add(@"Models/Tiles/TileOil"); // index 1
            textures.Add(@"Models/Tiles/TileSkid"); // index 2
            textures.Add(@"Models/Tiles/TileOil"); // index 3 - TODO: replace with tile representing base of barrier

            //load the map
            string[] tileLines = File.ReadAllLines(AppDomain.CurrentDomain.BaseDirectory + @"/Content/Map1.txt");

            for (int j = 0; j < tileLines.Length; j++) 
            {
                string[] tileLine = tileLines[j].Split(',');

                for (int i = 0; i < tileLine.Length; i++)
                {
                    layout[j,i] = int.Parse(tileLine[i]);
                }
            }
            
            string[] modelLines = File.ReadAllLines(AppDomain.CurrentDomain.BaseDirectory + @"/Content/Map1Obs.txt");

            for (int j = 0; j < modelLines.Length; j++) 
            {
                string[] modelLine = modelLines[j].Split(',');

                for (int i = 0; i < modelLine.Length; i++)
                {
                    layoutObstacleModels[j,i] = int.Parse(modelLine[i]);
                }
            }

        }

        public int GetIndex(int cellX, int cellY)
        {
            return layout[cellX, cellY];
        }

        public int GetObstacleIndex(int cellX, int cellY)
        {
            return layoutObstacleModels[cellX, cellY];
        }

        /// <summary>
        /// Check if a point on the tilemap is walkable.
        ///
        /// </summary>
        /// <param name="tilePoint"></param>
        /// <returns></returns>
        public bool isWalkable(Point tilePoint) {
            int tileMapIndex = GetIndex(tilePoint.Y, tilePoint.X);

            return (tileMapIndex != 1);
        }

        public Point GetQuantisation(Vector3? vector)
        {
            double tileX = Math.Abs(Math.Floor(Math.Abs(vector.Value.X + 2) / MapBuilder.TILE_SIZE));
            double tileY = Math.Abs(Math.Floor(Math.Abs(vector.Value.Z + 2) / MapBuilder.TILE_SIZE));
            
            if (tileX > Width - 1)
            {
                tileX = Width - 1;
            }
            if (tileY > Height - 1)
            {
                tileY = Height - 1;
            }
            
            return new Point((int)tileX, (int)tileY);
        }

        public List<BasicModel> Render()
        {

            List<BasicModel> mapModels = new List<BasicModel>();

            /// add tile models
            mapModels.AddRange(RenderTiles());

            /// add map obstacle models
            mapModels.AddRange(RenderObstacles());

            return mapModels;
        }

        private List<MapTile> RenderTiles()
        {
            List<MapTile> tiles = new List<MapTile>();

            for (int i = 0; i < Width; i++)
            {
                for (int j = 0; j < Height; j++)
                {
                    int tileIndex = GetIndex(j, i);

                    // dynamically load tile from "textures" list based on map index
                    MapTile tile = new MapTile(
                            Game.Content.Load<Model>(textures[tileIndex]),
                            new Vector3((TILE_SIZE * i), 3, (TILE_SIZE * j)),
                            tileIndex);

                    tiles.Add(tile);

                }
            }

            return tiles;
        }

        private List<BasicModel> RenderObstacles()
        {
            List<BasicModel> obstacleModels = new List<BasicModel>();

            for (int i = 0; i < Width; i++)
            {
                for (int j = 0; j < Height; j++)
                {

                    int modelIndex = GetObstacleIndex(j, i);

                    switch (modelIndex) {
                        case 1:
                            Tire tireStack = new Tire(
                                    Game.Content.Load<Model>(@"Models/Obstacles/TireStackModel"),
                                    new Vector3((TILE_SIZE * i), 0, (TILE_SIZE * j)));

                            obstacleModels.Add(tireStack);

                            break;
                        case 2:
                            Barrier barrierGH = new Barrier(
                                    Game.Content.Load<Model>(@"Models/Obstacles/WallGreenHorizontalModel"),
                                    new Vector3((TILE_SIZE * i), 0, (TILE_SIZE * j)));

                            obstacleModels.Add(barrierGH);

                            break;
                        case 3:
                            Barrier barrierOH = new Barrier(
                                    Game.Content.Load<Model>(@"Models/Obstacles/WallOJHorModel"),
                                    new Vector3((TILE_SIZE * i), 0, (TILE_SIZE * j)));

                            obstacleModels.Add(barrierOH);

                            break;
                        case 4:
                            Barrier barrierGV = new Barrier(
                                    Game.Content.Load<Model>(@"Models/Obstacles/WallGreenVerticalModel"),
                                    new Vector3((TILE_SIZE * i), 0, (TILE_SIZE * j)));

                            obstacleModels.Add(barrierGV);

                            break;
                        case 5:
                            Barrier barrierOV = new Barrier(
                                    Game.Content.Load<Model>(@"Models/Obstacles/WallOJVertModel"),
                                    new Vector3((TILE_SIZE * i), 0, (TILE_SIZE * j)));

                            obstacleModels.Add(barrierOV);

                            break;
                    }

                }
            }

            return obstacleModels;
        }
    }
}
