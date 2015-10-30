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

        private string[,] layout = new string[13, 17];

        private string[,] layoutObstacleModels = new string[13, 17];

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

            /// keep list of all possible obstacle models
            obstacleModels.Add(@"Models/Obstacles/WallSectionL");
            obstacleModels.Add(@"Models/Obstacles/WallSectionS");

            //load the map
            string[] tileLines = File.ReadAllLines(AppDomain.CurrentDomain.BaseDirectory + @"/Content/Map1.txt");

            for (int j = 0; j < tileLines.Length; j++) 
            {
                string[] tileLine = tileLines[j].Split(',');

                for (int i = 0; i < tileLine.Length; i++)
                {
                    layout[j,i] = tileLine[i];
                }
            }

            
            string[] modelLines = File.ReadAllLines(AppDomain.CurrentDomain.BaseDirectory + @"/Content/Map1Obs.txt");

            for (int j = 0; j < modelLines.Length; j++) 
            {
                string[] modelLine = modelLines[j].Split(',');

                for (int i = 0; i < modelLine.Length; i++)
                {
                    layoutObstacleModels[j,i] = modelLine[i];
                }
            }

        }

        public int GetIndex(int cellX, int cellY)
        {
            return int.Parse(layout[cellX, cellY]);
        }

        public int GetObstacleIndex(int cellX, int cellY)
        {
            return int.Parse(layoutObstacleModels[cellX, cellY]);
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

                    // TODO: ran out of time building unmovable tile/model, instead if 1 tile index, then just tint red, to indicate cannot move.
                    if (tileIndex == 1)
                    {
                        tile.tintColour = BasicModel.TINT_RED;
                    }

                    tiles.Add(tile);

                }
            }

            return tiles;
        }

        private bool isWalkable(int modelIndex) {
            return (modelIndex == 1 || modelIndex == 3) ? false : true;
        }

        private List<Tire> RenderObstacles()
        {
            List<Tire> obstacleModels = new List<Tire>();

            for (int i = 0; i < Width; i++)
            {
                for (int j = 0; j < Height; j++)
                {

                    int modelIndex = GetObstacleIndex(j, i);

                    switch (modelIndex) {
                        case 3:
                            Tire barrierObstacle = new Tire(
                                    Game.Content.Load<Model>(@"Models/Obstacles/WallSectionL"),
                                    new Vector3((TILE_SIZE * i), 0, (TILE_SIZE * j)));

                            obstacleModels.Add(barrierObstacle);

                            break;
                    }

                }
            }

            return obstacleModels;
        }
    }
}
