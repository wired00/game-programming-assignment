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

        public CollisionHandler(AudioManager audioManager) {
            this.audioManager = audioManager;
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

            } else if (modelA.GetType() == typeof(Enemy) && modelB.GetType() == typeof(Enemy)) {
                ///
                /// COLLISION - ENEMY AI WITH ENEMY AI
                ///

                Enemy enemyModelA = (Enemy)modelA;
                Enemy enemyModelB = (Enemy)modelB;

                // knockback each enemy
                enemyModelA.KnockBackFrom(enemyModelB);
                enemyModelB.KnockBackFrom(enemyModelA);

            } else if (modelA.GetType() == typeof(Enemy) && modelB.GetType() == typeof(Player)) {
                ///
                /// COLLISION - ENEMY AND PLAYER
                ///

                Enemy enemyModel = (Enemy)modelA;
                Player playerModel = (Player)modelB;

                enemyModel.KnockBackFrom(playerModel); // knockback player from enemy
                playerModel.KnockBackFrom(enemyModel); // knockback enemy from player

                if (playerModel.isBoosting())
                {
                    enemyModel.health = enemyModel.health - Enemy.MAX_HEALTH;
                }
                else
                {
                    enemyModel.health = enemyModel.health - 25;
                    playerModel.health = playerModel.health - 10;
                    if (audioManager.crash.State != SoundState.Playing)
                    {
                        audioManager.crash.Play();
                    }
                }

                if (enemyModel.health <= 0) {
                    audioManager.enemyDeath.Play();
                    enemyModel.currentDrawState = BasicModel.drawState.remove;
                }

                if (playerModel.health <= 0) {
                    audioManager.enemyDeath.Play();
                    playerModel.currentDrawState = BasicModel.drawState.remove;
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
