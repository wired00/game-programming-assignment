using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace Game3 {
    class ModelManager : DrawableGameComponent {
        List<BasicModel> models = new List<BasicModel>();

        Player playerModel;

        CollisionHandler collisionHandler;// = new CollisionHandler(((Game1)Game).audioManager);

        float secondsSinceLastItem = 0;

        public ModelManager(Game game) : base(game) {
           
        }

        public override void Initialize() {
            collisionHandler = new CollisionHandler(((Game1)Game).audioManager);
            base.Initialize();
        }

        protected override void LoadContent() {

            models.Add(new Pickup(
                Game.Content.Load<Model > (@"Models/Battery/BatteryModel"),
                new Vector3(300,30,100))
			);
            models.Add(new Pickup(
                Game.Content.Load<Model > (@"Models/Battery/BatteryModel"),
                new Vector3(-100, 30, 100))
            );

            //models.Add(new Pickup(
            //    Game.Content.Load<Model>(@"Models/Car/Cartest3"),
            //    new Vector3(-0, 100, 0))
            //);

            models.Add(new Pickup(
                Game.Content.Load<Model > (@"Models/Battery/BatteryModel"),
                new Vector3(300, 30, 200))
            );


            models.Add(new Pickup(
                Game.Content.Load<Model>(@"Models/Battery/BatteryModel"),
                new Vector3(400, 30, 50))
            );

            models.Add(new Pickup(
                Game.Content.Load<Model>(@"Models/Battery/BatteryModel"),
                new Vector3(200, 30, -200))
            );

            models.Add(new Pickup(
                Game.Content.Load<Model>(@"Models/Battery/BatteryModel"),
                new Vector3(50, 30, -100))
            );

            models.Add(new Ground(
                Game.Content.Load<Model>(@"Models/Ground/Ground"))
            );

            // need to keep hold of the players tank
            playerModel = new Player (
	              Game.Content.Load<Model> (@"Models/Car/CarModel"),
	              ((Game1)Game).GraphicsDevice,
	              ((Game1)Game).camera,
                  ((Game1)Game).graphics,
                  ((Game1)Game).uiManager,
                  ((Game1)Game).audioManager);

			models.Add(playerModel);

            
			models.Add(new Enemy(
				Game.Content.Load<Model>(@"Models/Tank/tank"),
				((Game1)Game).GraphicsDevice,
				((Game1)Game).camera,
				new Vector3 (500, 0, -400),
				playerModel
				));

            models.Add(new Enemy(
                Game.Content.Load<Model>(@"Models/Tank/tank"),
                ((Game1)Game).GraphicsDevice,
                ((Game1)Game).camera,
                new Vector3(-300, 0, 150),
                playerModel
                ));

            models.Add(new Enemy(
                Game.Content.Load<Model>(@"Models/Tank/tank"),
                ((Game1)Game).GraphicsDevice,
                ((Game1)Game).camera,
                new Vector3(-00, 0, -200),
                playerModel
                ));

            base.LoadContent();
        }

        public override void Update(GameTime gameTime) {

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
                        
            for (int i = 0; i < models.Count; i++) {
                BasicModel model = models[i];
                
                model.Update(gameTime);

                model.models = models;
            }

            SpawnEnergyItem(gameTime);

            base.Update(gameTime);
        }

        private void SpawnEnergyItem(GameTime gameTime) {
            float elapsedTime = (float) gameTime.ElapsedGameTime.TotalSeconds;

            if (secondsSinceLastItem >= 2) {
                Random rnd = new Random();
                models.Add(new Pickup(
                    Game.Content.Load<Model>(@"Models/Battery/BatteryModel"),
                    new Vector3(rnd.Next(-300, 300), 30, rnd.Next(-250, 250))));
                secondsSinceLastItem = 0;
            } else {
                secondsSinceLastItem += elapsedTime;
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

    }
}
