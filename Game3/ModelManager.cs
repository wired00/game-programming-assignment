using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace Game3 {
    class ModelManager : DrawableGameComponent {
        List<BasicModel> models = new List<BasicModel>();

        CollisionHandler collisionHandler = new CollisionHandler();

        float secondsSinceLastItem = 0;

        public ModelManager(Game game) : base(game) {
           
        }

        public override void Initialize() {
            base.Initialize();
        }

        protected override void LoadContent() {

            models.Add(new Pickup(
                Game.Content.Load<Model > (@"Models/Crate/crate"),
                new Vector3(300,30,100))
			);
            models.Add(new Pickup(
                Game.Content.Load<Model > (@"Models/Crate/crate"),
                new Vector3(-100, 30, 100))
            );

            //models.Add(new Pickup(
            //    Game.Content.Load<Model>(@"Models/Car/Cartest3"),
            //    new Vector3(-0, 100, 0))
            //);

            models.Add(new Pickup(
                Game.Content.Load<Model > (@"Models/Crate/crate"),
                new Vector3(300, 30, 200))
            );


            models.Add(new Pickup(
                Game.Content.Load<Model>(@"Models/Crate/crate"),
                new Vector3(400, 30, 50))
            );

            models.Add(new Pickup(
                Game.Content.Load<Model>(@"Models/Crate/crate"),
                new Vector3(200, 30, -200))
            );

            models.Add(new Pickup(
                Game.Content.Load<Model>(@"Models/Crate/crate"),
                new Vector3(50, 30, -100))
            );

            models.Add(new Ground(
                Game.Content.Load<Model>(@"Models/Ground/Ground"))
            );

            // need to keep hold of the players tank
            Tank playerTank = new Tank (
	              Game.Content.Load<Model> (@"Models/Tank/tank"),
	              ((Game1)Game).GraphicsDevice,
	              ((Game1)Game).camera,
                  ((Game1)Game).graphics,
                  ((Game1)Game).uiManager);

			models.Add(playerTank);

            
			models.Add(new Enemy(
				Game.Content.Load<Model>(@"Models/Tank/tank"),
				((Game1)Game).GraphicsDevice,
				((Game1)Game).camera,
				new Vector3 (500, 0, -400),
				playerTank
				));

            models.Add(new Enemy(
                Game.Content.Load<Model>(@"Models/Tank/tank"),
                ((Game1)Game).GraphicsDevice,
                ((Game1)Game).camera,
                new Vector3(-300, 0, 150),
                playerTank
                ));

            models.Add(new Enemy(
                Game.Content.Load<Model>(@"Models/Tank/tank"),
                ((Game1)Game).GraphicsDevice,
                ((Game1)Game).camera,
                new Vector3(-00, 0, -200),
                playerTank
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
                Console.WriteLine("inside");
                models.Add(new Pickup(
                    Game.Content.Load<Model>(@"Models/Crate/crate"),
                    new Vector3(rnd.Next(-300, 300), 30, rnd.Next(-300, 300))));
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

    }
}
