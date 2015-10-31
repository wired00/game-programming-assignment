using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

/// <summary>
/// A model manager will handle all models
/// </summary>
namespace BatteryDerby {
    public class ModelManager : DrawableGameComponent {
        List<BasicModel> models = new List<BasicModel>();

        Player playerModel;

        CollisionHandler collisionHandler;// = new CollisionHandler(((Game1)Game).audioManager);

        float secondsSinceLastItem = 0;
        float secondsSinceLastEnemy = 0;
        int enemySpawnCount = 0;
        public int monsterTruckCount { get; set; }

        UIManager uiManager;
        Game game;
        SplashScreen splashScreen;
        Score score;
        
        VertexDeclaration vertexDeclaration;
        Matrix View, Projection;
        
        bool bBuiltMap = false;

        PathfindAStar pathfindAStar;
        MapBuilder mapBuilder = null;
        Random rnd;

        public Vector3? aStarSeekTarget { get; set; }

        int seekIndicatorCount = 0;

        public ModelManager(Game1 game) : base(game) {
            this.game = game;
            this.splashScreen = game.splashScreen;
            this.score = game.score;

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
                new Vector3(rnd.Next(MapBuilder.MINX, MapBuilder.MAXX), 30, rnd.Next(MapBuilder.MINY, MapBuilder.MAXY))));

            models.Add(new Pickup(
                Game.Content.Load<Model>(@"Models/Battery/BatteryModel"),
                new Vector3(rnd.Next(MapBuilder.MINX, MapBuilder.MAXX), 30, rnd.Next(MapBuilder.MINY, MapBuilder.MAXY))));

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
	              Game.Content.Load<Model> (@"Models/Vehicles/PlayerCarModel"),
	              ((Game1)Game).GraphicsDevice,
	              ((Game1)Game).camera,
                  ((Game1)Game).graphics,
                  ((Game1)Game).uiManager,
                  ((Game1)Game).audioManager);
			models.Add(playerModel);
            
            Enemy enemy = new Enemy(
                Game.Content.Load<Model>(@"Models/Vehicles/BuggyFullHP"),
                ((Game1)Game).GraphicsDevice,
                ((Game1)Game).camera,
                new Vector3(rnd.Next(MapBuilder.MINX, MapBuilder.MAXX), 0, MapBuilder.MAXY),
                playerModel,
                uiManager);
            models.Add(enemy);
            
            /*
            MonsterTruck enemyTruck = new MonsterTruck(
                Game.Content.Load<Model>(@"Models/Vehicles/MonsterTruckFull"),
                ((Game1)Game).GraphicsDevice,
                ((Game1)Game).camera,
                new Vector3(rnd.Next(MapBuilder.MINX, MapBuilder.MAXX), 0, MapBuilder.MAXY),
                playerModel,
                uiManager);
            models.Add(enemyTruck);
            */

            base.LoadContent();
        }

        public override void Update(GameTime gameTime) {
                        
            SpawnModels(gameTime);

            /// handle manage A* movement Queues for NPCs
            HandleAStarMovement(gameTime);
            
            //partition.detectPartition(models);
            if (models.Count > 0) {
                collisionHandler.detectCollisions(models);
            }

            // use modulus to calculate if divisible by 10, and such spawn a monster every 10 enemy kills
            if (score.enemiesDefeatedCount > 0 && score.enemiesDefeatedCount % 10 == 0 || score.survivalTime > 30) {

                // only spawn one monster truck / boss at a time.
                if (monsterTruckCount == 0) {
                    MonsterTruck enemyTruck = new MonsterTruck(
                        Game.Content.Load<Model>(@"Models/Vehicles/MonsterTruckFull"),
                        ((Game1)Game).GraphicsDevice,
                        ((Game1)Game).camera,
                        new Vector3(rnd.Next(MapBuilder.MINX, MapBuilder.MAXX), 0, MapBuilder.MAXY),
                        playerModel,
                        uiManager);
                    models.Add(enemyTruck);
                    monsterTruckCount++;

                }

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
                SpawnEnemys();
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

        /// <summary>
        /// Spawn item if the coordinate is walkable. problems with random in C# m
        /// </summary>
        private void SpawnItems() {
            Vector3 itemLocation = new Vector3(rnd.Next(MapBuilder.MINX, MapBuilder.MAXX), 30, rnd.Next(MapBuilder.MINY, MapBuilder.MAXY));

            Point tilePoint = mapBuilder.GetQuantisation(itemLocation);
            if (mapBuilder.isWalkable(tilePoint)) {
                models.Add(new Pickup(Game.Content.Load<Model>(@"Models/Battery/BatteryModel"), itemLocation));
            } else {
                SpawnItems();
            }

        }

        private void SpawnEnemys() {
            if (!((Game1)game).debugMode) {
                models.Add(new Enemy(
                Game.Content.Load<Model>(@"Models/Vehicles/BuggyFullHP"),
                ((Game1)Game).GraphicsDevice,
                ((Game1)Game).camera,
                GetEnemySpawnLocation(),
                playerModel,
                uiManager
                ));
            }
        }

        // get random enemy spawn location. Will spawn at a random y coord at bottom of map
        private Vector3 GetEnemySpawnLocation() {
            return new Vector3(rnd.Next(MapBuilder.MINX, MapBuilder.MAXX), 30, MapBuilder.MAXY);

        }

        /// <summary>
        /// Handle manage A* movement Queues for NPCs
        /// </summary>
        /// <param name="gameTime"></param>
        private void HandleAStarMovement(GameTime gameTime) {
            for (int i = 0; i < models.Count; i++) {
                BasicModel model = models[i];

                model.Update(gameTime);
                model.models = models;

                if (model.GetType() == typeof(Enemy) && ((Enemy)model).aStarPaths.Count == 0) {
                    Vector3? seekTarget = null;
                    if (((Enemy)model).isSeekingPlayer()) {
                        seekTarget = playerModel.translation.Translation;
                    } else {
                        if (((Enemy)model).GetNearestEnergyItem().HasValue) {
                            seekTarget = ((Enemy)model).GetNearestEnergyItem().Value;
                        } else {
                            // flee if damaged but no batteries left!
                            seekTarget = new Vector3(rnd.Next(MapBuilder.MINX, MapBuilder.MAXX), ((Enemy)model).translation.Translation.Y, rnd.Next(MapBuilder.MINY, MapBuilder.MAXY));
                        }
                    }

                    if (seekTarget.HasValue) {
                        List<Vector2> pathToTarget = FindPath(mapBuilder.GetQuantisation(model.translation.Translation), mapBuilder.GetQuantisation(seekTarget));
                        ClearSeekTokens();
                        ((Enemy)model).aStarPaths.Clear();
                        ((Enemy)model).aStarPaths.AddRange(pathToTarget);
                        seekIndicatorCount = 0;
                    }

                } else if (model.GetType() == typeof(Enemy) && ((Enemy)model).aStarPaths.Count > 0) {
                    HandleDebugMode(model);
                }

                if (model.GetType() == typeof(MonsterTruck) && ((MonsterTruck)model).aStarPaths.Count == 0) {
                    List<Vector2> pathToTarget = FindPath(mapBuilder.GetQuantisation(model.translation.Translation), mapBuilder.GetQuantisation(playerModel.translation.Translation));
                    ((MonsterTruck)model).aStarPaths.Clear();
                    ((MonsterTruck)model).aStarPaths.AddRange(pathToTarget);
                    seekIndicatorCount = 0;
                }
            }
        }

        /// <summary>
        /// When debug mode show path from enemy to target of a*
        /// </summary>
        private void HandleDebugMode(BasicModel model) {
            
            if (((Game1)game).debugMode) {
                // Add token indicating where enemy seeking. One indicator arrow for each astar coordinate
                // Also, first verify that the token doesnt already exist at the location

                foreach (Vector2 seekLocation in ((Enemy)model).aStarPaths) {

                    if (!SeekTokenAlreadyExists(new Vector3(seekLocation.X, 0, seekLocation.Y))) {
                        SeekIndicator seekIndicator = new SeekIndicator(
                            Game.Content.Load<Model>(@"Models/ArrowPointer/ArrowPointerModel"),
                            new Vector3(seekLocation.X, 0, seekLocation.Y));
                        models.Add(seekIndicator);
                        seekIndicatorCount++;
                    }

                }
            } else {
                // clear all astar debug tokens if not debug mode
                ClearSeekTokens();
            }
           
        }

        private bool SeekTokenAlreadyExists(Vector3 location) {
            for (int i = 0; i < models.Count; i++) {
                BasicModel model = models[i];

                if (seekIndicatorCount >= 50) {
                    return true;
                }
            }

            return false;
        }

       private void ClearSeekTokens() {
            for (int i = 0; i < models.Count; i++) {
                BasicModel model = models[i];

                if (model.GetType() == typeof(SeekIndicator)) {
                    ((BasicModel)model).currentDrawState = BasicModel.drawState.remove;
                }
            }
        }
    }
}
