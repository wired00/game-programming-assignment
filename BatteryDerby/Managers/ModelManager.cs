using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

/// <summary>
/// A model manager will handle all models
/// </summary>
namespace BatteryDerby {
    class ModelManager : DrawableGameComponent {
        List<BasicModel> models = new List<BasicModel>();

        Player playerModel;

        CollisionHandler collisionHandler;// = new CollisionHandler(((Game1)Game).audioManager);

        public const int MINX = 96;
        public const int MAXX = 1440;
        public const int MINY = 96;
        public const int MAXY = 1056;
        float secondsSinceLastItem = 0;
        float secondsSinceLastEnemy = 0;
        int enemySpawnCount = 0;

        UIManager uiManager;
        Game game;
        SplashScreen splashScreen;
        
        VertexDeclaration vertexDeclaration;
        Matrix View, Projection;
        
        bool bBuiltMap = false;

        PathfindAStar pathfindAStar;
        MapBuilder mapBuilder = null;
        Random rnd;

        public ModelManager(Game game, SplashScreen splashScreen) : base(game) {
            this.game = game;
            this.splashScreen = splashScreen;
            rnd = new Random();
        }

        public override void Initialize() {
            collisionHandler = new CollisionHandler((Game1) game);
            uiManager = new UIManager(game);

            View = Matrix.CreateLookAt(new Vector3(0, 0, 2), Vector3.Zero,
                Vector3.Up);
            Projection = Matrix.CreatePerspectiveFieldOfView(
                MathHelper.PiOver4, 4.0f / 3.0f, 1, 500);

            base.Initialize();
        }

        protected override void LoadContent() {
            Random rnd = new Random();

            vertexDeclaration = new VertexDeclaration(new VertexElement[]
                {
                    new VertexElement(0, VertexElementFormat.Vector3, VertexElementUsage.Position, 0),
                    new VertexElement(12, VertexElementFormat.Vector3, VertexElementUsage.Normal, 0),
                    new VertexElement(24, VertexElementFormat.Vector2, VertexElementUsage.TextureCoordinate, 0)
                }
            );

            models.Add(new Ground(
                Game.Content.Load<Model>(@"Models/Ground/Ground")));

            models.Add(new Pickup(
                Game.Content.Load<Model>(@"Models/Battery/BatteryModel"),
                new Vector3(rnd.Next(MINX, MAXX), 30, rnd.Next(MINY, MAXY))));

            models.Add(new Pickup(
                Game.Content.Load<Model>(@"Models/Battery/BatteryModel"),
                new Vector3(rnd.Next(MINX, MAXX), 30, rnd.Next(MINY, MAXY))));

            ///
            /// Build a map using tiles
            ///
            if (!bBuiltMap) {

                mapBuilder = new MapBuilder(this.game);
                models.AddRange(mapBuilder.Render());

                bBuiltMap = true;
            }

            // need to keep hold of the players model
            playerModel = new Player (
	              Game.Content.Load<Model> (@"Models/Vehicles/PlayerCar2"),
	              ((Game1)Game).GraphicsDevice,
	              ((Game1)Game).camera,
                  ((Game1)Game).graphics,
                  ((Game1)Game).uiManager,
                  ((Game1)Game).audioManager);
			models.Add(playerModel);

            Enemy enemy = new Enemy(
                Game.Content.Load<Model>(@"Models/Vehicles/Buggy"),
                ((Game1)Game).GraphicsDevice,
                ((Game1)Game).camera,
                //new Vector3(500, 0, -400),
                new Vector3(500, 0, 400),
                playerModel,
                uiManager);
            models.Add(enemy);

            MonsterTruck enemyTruck = new MonsterTruck(
                Game.Content.Load<Model>(@"Models/Vehicles/MonsterTruckFull"),
                ((Game1)Game).GraphicsDevice,
                ((Game1)Game).camera,
                //new Vector3(500, 0, -400),
                new Vector3(400, 0, 100),
                playerModel,
                uiManager);
            models.Add(enemyTruck);

            /*
            enemy = new Enemy(
                Game.Content.Load<Model>(@"Models/Car/Enemy/CarModel2"),
                ((Game1)Game).GraphicsDevice,
                ((Game1)Game).camera,
                new Vector3(-300, 0, 150),
                playerModel,
                uiManager);
            models.Add(enemy);

            enemy = new Enemy(
                Game.Content.Load<Model>(@"Models/Car/Enemy/CarModel2"),
                ((Game1)Game).GraphicsDevice,
                ((Game1)Game).camera,
                new Vector3(-200, 0, -200),
                playerModel,
                uiManager);
            models.Add(enemy);
            */

            base.LoadContent();
        }

        public override void Update(GameTime gameTime) {
                        
            SpawnModels(gameTime);

            for (int i = 0; i < models.Count; i++) {
                BasicModel model = models[i];

                model.Update(gameTime);
                model.models = models;

                if (model.GetType() == typeof(Enemy) && ((Enemy)model).aStarPaths.Count == 0) {
                    List<Vector2> pathToTarget = FindPath(mapBuilder.GetQuantisation(model.translation.Translation), mapBuilder.GetQuantisation(playerModel.translation.Translation));
                    ((Enemy)model).aStarPaths.AddRange(pathToTarget);
                }

                if (model.GetType() == typeof(MonsterTruck) && ((MonsterTruck)model).aStarPaths.Count == 0) {
                    List<Vector2> pathToTarget = FindPath(mapBuilder.GetQuantisation(model.translation.Translation), mapBuilder.GetQuantisation(playerModel.translation.Translation));
                    ((MonsterTruck)model).aStarPaths.AddRange(pathToTarget);
                }
            }

            //partition.detectPartition(models);
            if (models.Count > 0) {
                collisionHandler.detectCollisions(models);
            }

            // remove any objects flagged for deletion from the collision handler
            for (int i = 0; i < models.Count; i++) {
                BasicModel model = models[i];

                if (model.currentDrawState == BasicModel.drawState.remove) {
                    models.Remove(model);
                }
            }

            base.Update(gameTime);
        }

        public List<Vector2> FindPath(Point from, Point to) {
            pathfindAStar = new PathfindAStar(mapBuilder);
            return pathfindAStar.FindPath(from, to);
        }

        private int PickupCount() {
            int count = 0;
            foreach (BasicModel i in models) {
                if(i.GetType() == typeof(Pickup)) count++;
            }

            return count;
        }


        private void SpawnModels(GameTime gameTime) {

            float elapsedTime = (float) gameTime.ElapsedGameTime.TotalSeconds;

            float spawnModifier = (float) Math.Ceiling(enemySpawnCount / 2f);
            
            // Spawn 
            //items every 2.5 seconds
            if (secondsSinceLastItem >= 2.5f && PickupCount() < 10) {

                SpawnItems();
                secondsSinceLastItem = 0;

            } else {
                secondsSinceLastItem += elapsedTime;
            }

            // spawn enemy, spawn faster as the game progresses.
            float timeToSpawnEnemy = 7f - spawnModifier;
            if (timeToSpawnEnemy < 2f) {
                timeToSpawnEnemy = 2f; // don't allow faster than every 2 seconds, might be a bit too fast!
            }

            if (secondsSinceLastEnemy >= timeToSpawnEnemy) {
                //SpawnEnemys();
                enemySpawnCount++;
                secondsSinceLastEnemy = 0;
            } else {
                secondsSinceLastEnemy += elapsedTime;
            }

        }

        public override void Draw(GameTime gameTime) {
            
            foreach (BasicModel model in models) {
                model.Draw(((Game1)Game).device, ((Game1)Game).camera);
            }

            ((Game1)Game).uiManager.Draw(gameTime);

            base.Draw(gameTime);

        }

        public Player getPlayerModel() {
            return this.playerModel;
        }

        /// <summary>
        /// Spawn item if the coordinate is walkable. problems with random in C# m
        /// </summary>
        private void SpawnItems() {
            Vector3 itemLocation = new Vector3(rnd.Next(MINX, MAXX), 30, rnd.Next(MINY, MAXY));

            Point tilePoint = mapBuilder.GetQuantisation(itemLocation);
            Console.WriteLine("GetQuantisation X: " + tilePoint.X + " Z: " + tilePoint.Y);
            if (mapBuilder.isWalkable(tilePoint)) {
                //Console.WriteLine("OK NO COLLISION X: " + itemLocation.X + " Z: " + itemLocation.Z);
                models.Add(new Pickup(Game.Content.Load<Model>(@"Models/Battery/BatteryModel"), itemLocation));
            } else {
                Console.WriteLine("OK NO COLLISION X: " + itemLocation.X + " Z: " + itemLocation.Z);
                SpawnItems();
            }

        }

        private void SpawnEnemys() {
            models.Add(new Enemy(
                Game.Content.Load<Model>(@"Models/Car/Enemy/CarModel2"),
                ((Game1)Game).GraphicsDevice,
                ((Game1)Game).camera,
                GetEnemySpawnLocation(),
                playerModel,
                uiManager
                ));
        }

        // get random enemy spawn location. Will spawn at a random y coord to the outside of either the left or right of map
        private Vector3 GetEnemySpawnLocation() {
            Random rnd = new Random();

            // Spawn left or right of map?
            if (rnd.Next(1, 10) > 5) {
                return new Vector3(50, 30, rnd.Next(50, 400));
            } else {
                return new Vector3(700, 30, rnd.Next(50, 400));
            }
        }

    }
}
