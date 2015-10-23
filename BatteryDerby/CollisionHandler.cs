﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Audio;

/// <summary>
/// Collision handler is used to handle all collisions between models
/// </summary>
namespace BatteryDerby {
    class CollisionHandler {

        AudioManager audioManager;

        SplashScreen splashScreen;

        Game1 game;

        Score score;

        enum Quadrant {
            A,
            B,
            C,
            D
        };
    
        public CollisionHandler(Game1 game) {
            this.audioManager = game.audioManager;
            this.splashScreen = game.splashScreen;
            this.game = game;
            this.score = game.score;
        }

        public void detectCollisions(List<BasicModel> models) {

            foreach (BasicModel modelA in models) {
                foreach (BasicModel modelB in models) {
                    if (isValidCheck(modelA, modelB)) {
                        if (collidesWith(modelA, modelB)) {
                            HandleCollision(modelA, modelB);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Only perform collision checks on valid models
        /// -
        /// - use spacial partition using quadrant to check if models are in same quadrant before more robust and computationallly expensive collision detection routine is performed
        /// - ignore collisions for ground (MapTile) models, there are many, so this vastly improves performance.
        /// - 
        /// </summary>
        /// <returns></returns>
        private bool isValidCheck(BasicModel modelA, BasicModel modelB) {
            if (modelA.uniqueId != modelB.uniqueId && (validModelType(modelA)) && (validModelType(modelB))) {
                if (isMatchingMapQuadrant(modelA, modelB)) {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Check if 2 models are in matching map quadrants. 
        /// </summary>
        /// <returns></returns>
        private bool isMatchingMapQuadrant(BasicModel modelA, BasicModel modelB) {
            return getMapQuadrant(modelA) == getMapQuadrant(modelB);
        }

        /// <summary>
        /// Get the map quadrant based on the provided models position vector.
        /// This is used for spacial partitioning
        /// 
        /// Map is devided into 4 quadrants:
        /// 
        ///         |
        ///      A  |  B
        ///   ------+------
        ///      D  |  C
        ///         |
        /// 
        /// 
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        private Quadrant getMapQuadrant(BasicModel model) {
            float modelX = model.translation.Translation.X;
            float modelZ = model.translation.Translation.Z;

            // detect quadrant 1
            if (modelX < 0 && modelZ >= 0) {
                return Quadrant.A;
            } else if (modelX >= 0 && modelZ >= 0) {
                return Quadrant.B;
            } else if (modelX >= 0 && modelZ < 0) {
                return Quadrant.C;
            } else if (modelX < 0 && modelZ < 0) {
                return Quadrant.D;
            }

            return 0;
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
                    playerModel.energy -= playerModel.energy/3;
                    enemyModel.health = enemyModel.health - enemyModel.health;
                    playerModel.health -= 5;
                }
                else
                {
                    playerModel.KnockBackFrom(enemyModel); // knockback enemy from player
                    enemyModel.health -= 35;
                    playerModel.health -= 5;
                    if (audioManager.crash.State != SoundState.Playing)
                    {
                        audioManager.crash.Play();
                    }
                    if (playerModel.energy <= 0) {
                        playerModel.health -= 20;
                    }
                }

                if (enemyModel.health <= 0) {
                    audioManager.enemyDeath.Play();
                    enemyModel.currentDrawState = BasicModel.drawState.remove;
                    score.enemiesDefeatedCount++;
                }

                if (playerModel.health <= 0) {
                    audioManager.enemyDeath.Play();
                    audioManager.accelerate.Stop();
                    audioManager.boost.Stop();
                    audioManager.charge.Stop();
                    audioManager.idleLoop.Stop();
                    playerModel.health = 0;
                    playerModel.currentDrawState = BasicModel.drawState.remove;
                    splashScreen.SetData("TODO - enemy wins", Game1.GameState.END); // change splash state
                    this.game.ChangeGameState(Game1.GameState.END, 1); // change game state
                    //score.survivalTime = game.gameTime.TotalGameTime.Seconds;
                }
            }
        }

        /// <summary>
        /// Perform robust collision detection using BoundingSpheres to determine if two models are intersecting.
        /// </summary>
        /// <param name="modelA"></param>
        /// <param name="modelB"></param>
        /// <returns></returns>
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

        /// <summary>
        /// Only perform collision detection on valid models. Ie, no point performing collision detection between ground (mapTile) and car models
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        private bool validModelType(BasicModel model) {
            return model.GetType() != typeof(MapTile); // ignore collisions with the ground
        }

    }
}