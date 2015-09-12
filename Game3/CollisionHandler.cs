using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Audio;

namespace Game3 {
    class CollisionHandler {

        AudioManager audioManager;

        SplashScreen splashScreen;

        Game1 game;

        Score score;
     
        
    
        public CollisionHandler(Game1 game) {
            this.audioManager = game.audioManager;
            this.splashScreen = game.splashScreen;
            this.game = game;
            this.score = game.score;
        }

        public void detectCollisions(List<BasicModel> models) {

            foreach (BasicModel modelA in models) {
                foreach (BasicModel modelB in models) {
                    if (modelA.uniqueId != modelB.uniqueId && (validModelType(modelA)) && (validModelType(modelB))) {
                        if (collidesWith(modelA, modelB)) {
                            HandleCollision(modelA, modelB);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Depending on the pair of models colliding, handle appropriately
        /// </summary>
        /// <param name="modelA"></param>
        /// <param name="modelB"></param>
        private void HandleCollision(BasicModel modelA, BasicModel modelB) {
            if (modelA.GetType() == typeof(Enemy) && modelB.GetType() == typeof(Pickup)) {
                ///
                /// COLLISION - ENEMY AND PICKUP ITEM
                ///
                score.enemyBatteryCount++;
                ((Enemy)modelA).FullHealth();
                modelB.currentDrawState = BasicModel.drawState.remove;


            } else if (modelA.GetType() == typeof(Player) && modelB.GetType() == typeof(Pickup)) {
                ///
                /// COLLISION - PLAYER AND PICKUP ITEM
                ///
                audioManager.charge.Play();
                if (((Player)modelA).energy + 25 >= Player.MAX_ENERGY)
                {
                    ((Player)modelA).energy = Player.MAX_ENERGY;
                }
                else
                {
                    ((Player)modelA).energy += 25;
                }
                modelB.currentDrawState = BasicModel.drawState.remove;
                score.playerBatteryCount++;

            } else if (modelA.GetType() == typeof(Enemy) && modelB.GetType() == typeof(Enemy)) {
                ///
                /// COLLISION - ENEMY AI WITH ENEMY AI
                ///

                Enemy enemyModelA = (Enemy)modelA;
                Enemy enemyModelB = (Enemy)modelB;

                // knockback each enemy
                enemyModelA.KnockBackFrom(enemyModelB);
                enemyModelB.KnockBackFrom(enemyModelA);

                if (audioManager.crash.State != SoundState.Playing) {
                    audioManager.crash.Play();
                }

            } else if (modelA.GetType() == typeof(Enemy) && modelB.GetType() == typeof(Player)) {
                ///
                /// COLLISION - ENEMY AND PLAYER
                ///

                Enemy enemyModel = (Enemy)modelA;
                Player playerModel = (Player)modelB;

                enemyModel.KnockBackFrom(playerModel); // knockback player from enemy
                
                if (playerModel.isBoosting())
                {
                    playerModel.energy -= enemyModel.health/3;
                    enemyModel.health = enemyModel.health - enemyModel.health;
                    playerModel.health = playerModel.health - 5;
                }
                else
                {
                    playerModel.KnockBackFrom(enemyModel); // knockback enemy from player
                    enemyModel.health = enemyModel.health - 25;
                    playerModel.health = playerModel.health - 5;
                    if (audioManager.crash.State != SoundState.Playing)
                    {
                        audioManager.crash.Play();
                    }
                }

                if (enemyModel.health <= 0) {
                    audioManager.enemyDeath.Play();
                    enemyModel.currentDrawState = BasicModel.drawState.remove;
                    score.enemiesDefeatedCount++;
                }

                if (playerModel.health <= 0) {
                    audioManager.enemyDeath.Play();
                    playerModel.health = 0;
                    playerModel.currentDrawState = BasicModel.drawState.remove;
                    splashScreen.SetData("TODO - enemy wins", Game1.GameState.END); // change splash state
                    this.game.ChangeGameState(Game1.GameState.END, 1); // change game state
                    //score.survivalTime = game.gameTime.TotalGameTime.Seconds;
                }
            }
        }

        public bool collidesWith (BasicModel modelA, BasicModel modelB) {
            // get the position of each model
            Matrix modelATranslation = modelA.GetWorld();
            Matrix modelBTranslation = modelB.GetWorld();

            // check each bounding sphere of each model
            foreach (ModelMesh modelMeshesA in modelA.model.Meshes) {
                foreach (ModelMesh modelMeshesB in modelB.model.Meshes) {
                    // update bounding spheres of each models mesh
                    BoundingSphere BSA = modelMeshesA.BoundingSphere.Transform(modelATranslation);
                    BoundingSphere BSB = modelMeshesB.BoundingSphere.Transform(modelBTranslation);

                    // check if any the mesh bounding sphere intersects with another
                    if (BSA.Intersects(BSB)) {
                        return true;
                    }
                }
            }
            return false;
        }

        private bool validModelType(BasicModel model) {
            return model.GetType() != typeof(Ground); // ignore collisions with the ground
        }

    }
}
