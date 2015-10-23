using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;

namespace BatteryDerby {

    public class MapBuilder : DrawableGameComponent {

        public const int TILE_SIZE = 96;
        private List<String> textures = new List<String>();
        private List<String> obstacleModels = new List<String>();

        private int[,] layout = new int[,] {
            { 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 0, 1, },
            { 1, 2, 0, 0, 1, 0, 0, 1, 0, 0, 0, 0, 1, },
            { 1, 2, 0, 0, 1, 0, 0, 1, 0, 0, 0, 0, 1, },
            { 1, 2, 0, 0, 1, 0, 0, 1, 0, 0, 0, 0, 1, },
            { 1, 1, 0, 0, 1, 0, 0, 1, 0, 0, 0, 0, 1, },
            { 1, 1, 0, 0, 1, 0, 0, 1, 0, 0, 0, 0, 1, },
            { 1, 1, 0, 0, 1, 0, 0, 1, 0, 0, 0, 0, 1, },
            { 1, 1, 0, 0, 1, 0, 0, 1, 0, 0, 0, 0, 1, },
            { 1, 1, 0, 0, 1, 0, 0, 1, 0, 0, 0, 0, 1, },
            { 1, 1, 0, 0, 1, 0, 0, 1, 0, 0, 0, 0, 1, },
            { 1, 1, 0, 0, 1, 0, 0, 1, 0, 0, 0, 0, 1, },
            { 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 0, 1, },
        };

        private int[,] layoutObstacleModels = new int[,] {
            { 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, },
            { 1, 0, 0, 0, 1, 0, 0, 1, 0, 0, 0, 0, 1, },
            { 1, 0, 0, 0, 1, 0, 0, 1, 0, 0, 0, 0, 1, },
            { 1, 0, 0, 0, 1, 0, 0, 1, 0, 0, 0, 0, 1, },
            { 1, 1, 0, 0, 1, 0, 0, 1, 0, 0, 0, 0, 1, },
            { 1, 1, 0, 0, 1, 0, 0, 1, 0, 0, 0, 0, 1, },
            { 1, 1, 0, 0, 1, 0, 0, 1, 0, 0, 0, 0, 1, },
            { 1, 1, 0, 0, 1, 0, 0, 1, 0, 0, 0, 0, 1, },
            { 1, 1, 0, 0, 1, 0, 0, 1, 0, 0, 0, 0, 1, },
            { 1, 1, 0, 0, 1, 0, 0, 1, 0, 0, 0, 0, 1, },
            { 1, 1, 0, 0, 1, 0, 0, 1, 0, 0, 0, 0, 1, },
            { 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 0, 1, },
        };

        public int Width {
            get { return layout.GetLength(1); }
        }

        public int Height {
            get { return layout.GetLength(0); }
        }
        public MapBuilder(Game game)
            : base(game) {

            /// keep list of all loaded tile textures.
            textures.Add(@"Models/Tiles/TileDirt");
            textures.Add(@"Models/Tiles/TileOil");
            textures.Add(@"Models/Tiles/TileSkid");

            /// keep list of all possible obstacle models
            obstacleModels.Add(@"Models/Obstacles/WallSectionL");
            obstacleModels.Add(@"Models/Obstacles/WallSectionS");

        }

        public int GetIndex(int cellX, int cellY) {
            if (cellX < 0 || cellX > Width - 1 || cellY < 0 || cellY > Height - 1)
                return 0;

            return layout[cellY, cellX];
        }

        public Point GetQuantisation(Vector3? vector) {
            double tileX = Math.Abs(Math.Floor(Math.Abs(vector.Value.X + 2) / MapBuilder.TILE_SIZE));
            double tileY = Math.Abs(Math.Floor(Math.Abs(vector.Value.Z + 2) / MapBuilder.TILE_SIZE));

            if (tileX > Width-1) {
                tileX = Width - 1;
            }
            if (tileY > Height-1) {
                tileY = Height - 1;
            }

            return new Point((int) tileX, (int) tileY);
        }

        public List<BasicModel> Render() {

            List<BasicModel> mapModels = new List<BasicModel>();

            /// add tile models
            mapModels.AddRange(RenderTiles());

            /// add map obstacle models
            mapModels.AddRange(RenderObstacles());

            return mapModels;
        }

        private List<MapTile> RenderTiles() {
            List<MapTile> tiles = new List<MapTile>();

            for (int i = 0; i < Width; i++) {
                for (int j = 0; j < Height; j++) {

                    int tileIndex = layout[j, i];

                    // dynamically load tile texture based on map index
                    MapTile tile = new MapTile(
                            Game.Content.Load<Model>(textures[tileIndex]),
                            new Vector3((TILE_SIZE * i) - 576, 0, (TILE_SIZE * j) - 720));

                    // TODO: ran out of time building unmovable tile/model, instead if 1 tile index, then just tint red, to indicate cannot move.
                    //if (tileIndex == 1) {
                    //    tile.tintColour = BasicModel.TINT_RED;
                    //}  

                    tiles.Add(tile);

                }
            }

            return tiles;
        }

        private List<Pickup> RenderObstacles() {
            List<Pickup> obstacleModels = new List<Pickup>();

            for (int i = 0; i < Width; i++) {
                for (int j = 0; j < Height; j++) {

                    int tileIndex = layoutObstacleModels[j, i];

                    // dynamically load map obstacle models
                    Pickup pickup = new Pickup(
                            Game.Content.Load<Model>(@"Models/Obstacles/WallSectionL"),
                            new Vector3((TILE_SIZE * i) - 576, 0, (TILE_SIZE * j) - 720));

                    obstacleModels.Add(pickup);

                }
            }

            return obstacleModels;
        }
    }
}
