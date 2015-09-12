﻿using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace Game3 {
    class ModelManager : DrawableGameComponent {
        List<BasicModel> models = new List<BasicModel>();

        Player playerModel;

        CollisionHandler collisionHandler;// = new CollisionHandler(((Game1)Game).audioManager);

        float secondsSinceLastItem = 0;
        float secondsSinceLastEnemy = 0;
        int enemySpawnCount = 0;

        UIManager uiManager;
        Game game;
        SplashScreen splashScreen;

        public ModelManager(Game game, SplashScreen splashScreen) : base(game) {
            this.game = game;
            this.splashScreen = splashScreen;
        }

        public override void Initialize() {
            collisionHandler = new CollisionHandler((Game1) game);
            uiManager = new UIManager(game);
            base.Initialize();
        }

        protected override void LoadContent() {
            Random rnd = new Random();
            models.Add(new Pickup(
                Game.Content.Load<Model>(@"Models/Battery/BatteryModel"),
                new Vector3(rnd.Next(-400, 400), 30, rnd.Next(-350, 250))));
            models.Add(new Pickup(
                Game.Content.Load<Model>(@"Models/Battery/BatteryModel"),
                new Vector3(rnd.Next(-400, 400), 30, rnd.Next(-350, 250))));

            models.Add(new Ground(
                Game.Content.Load<Model>(@"Models/Ground/Ground"))
            );

            // need to keep hold of the players tank
            playerModel = new Player (
	              Game.Content.Load<Model> (@"Models/Car/Player/CarModel2"),
	              ((Game1)Game).GraphicsDevice,
	              ((Game1)Game).camera,
                  ((Game1)Game).graphics,
                  ((Game1)Game).uiManager,
                  ((Game1)Game).audioManager);
			models.Add(playerModel);

            Enemy enemy = new Enemy(
                Game.Content.Load<Model>(@"Models/Car/Enemy/CarModel2"),
                ((Game1)Game).GraphicsDevice,
                ((Game1)Game).camera,
                new Vector3(500, 0, -400),
                playerModel,
                uiManager);
            models.Add(enemy);

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
                
            base.LoadContent();
        }

        public override void Update(GameTime gameTime) {
                        
            SpawnModels(gameTime);

            for (int i = 0; i < models.Count; i++) {
                BasicModel model = models[i];

                model.Update(gameTime);

                model.models = models;
            }

            //partition.detectPartition(models);
            if (models.Count > 0) {
                collisionHandler.detectCollisions(models);
            }

            // remove any objects incase collision caused removal to trigger
            for (int i = 0; i < models.Count; i++) {
                BasicModel model = models[i];

                if (model.currentDrawState == BasicModel.drawState.remove) {
                    models.Remove(model);
                }
            }

            base.Update(gameTime);


        }

        private void SpawnModels(GameTime gameTime) {

            float elapsedTime = (float) gameTime.ElapsedGameTime.TotalSeconds;

            float spawnModifier = (float) Math.Ceiling(enemySpawnCount / 2f);

            Console.WriteLine(spawnModifier);

            // Spawn Pickup items every 2 seconds
            if (secondsSinceLastItem >= 1.5f) {
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
            base.Draw(gameTime);
            ((Game1)Game).uiManager.Draw(gameTime);
        }

        public Player getPlayerModel() {
            return this.playerModel;
        }

        private void SpawnItems() {
            Random rnd = new Random();
            models.Add(new Pickup(
                Game.Content.Load<Model>(@"Models/Battery/BatteryModel"),
                new Vector3(rnd.Next(-400, 400), 30, rnd.Next(-350, 250))));

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
                return new Vector3(-700, 30, rnd.Next(-400, 400));
            } else {
                return new Vector3(700, 30, rnd.Next(-400, 400));
            }
        }

    }
}
