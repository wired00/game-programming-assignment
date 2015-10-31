using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Content;

/// <summary>
/// Enemy model type
/// </summary>
namespace BatteryDerby {
    class Enemy : BasicModel {

        Game1 game;

        public static int MAX_HEALTH = 100;

        Player playerModel;

        public int health { get; set; }

        Matrix rotation = Matrix.Identity;

        state currentState = state.Resting;
        seekState currentSeekState = seekState.Player;

        float moveSpeed = 50f;
        float jumpVelocity = 0f;
        static float JUMP_HEIGHT = 35f;
        private BasicModel knockbackModelPosition;
        Random rng;
        Vector3 randomPoint;

        /// <summary>
        /// List of a* found paths updated from the ModelManager
        /// </summary>
        public List<Vector2> aStarPaths { get; set; }

        public Vector3? seekLocation { get; set; }


        bool bClearedPlayerSeekPaths = false;


        enum state {
            Moving,
            Resting,
            Rotating,
            Jumping,
            Falling
        }

        enum seekState {
            EnergyItem,
            Player,
            Flee
        }

        public Enemy(Model model, GraphicsDevice device, Camera camera, Vector3 position, Player playerModel, Game1 game)
            : base(model) {

            this.game = game;

			base.translation.Translation = position;

			this.playerModel = playerModel;
            health = MAX_HEALTH;
            rng = new Random();
            randomPoint = new Vector3(rng.Next(96, 1440), 30, rng.Next(96, 1056));

            aStarPaths = new List<Vector2>();
            seekLocation = null;
        }

        public override void Update(GameTime gameTime) {
            float elapsedTime = (float) gameTime.ElapsedGameTime.TotalSeconds;

            Vector3 currentPosition = translation.Translation;
            Vector3? targetItem = GetNearestEnergyItem();

            if (!targetItem.HasValue)
            {
                targetItem = randomPoint;
            }

            // if damaged, then seek health box
            // however, if Game1.EnemyDamagedBehaviour is read from XML Behaviour config, and set to agressive then don't hunt health, 
            // just attack player mercilessly!
            if (health < MAX_HEALTH && game.enemyDamagedBehaviour == Game1.EnemyDamagedBehaviour.normal) {
                
                // first clear previous player seek path.
                if (!bClearedPlayerSeekPaths) {
                    this.aStarPaths.Clear();
                    bClearedPlayerSeekPaths = true;

                }

                if (targetItem.HasValue) {

                    if (targetItem.HasValue) {
                        HandleAStarSeek(targetItem.Value, currentPosition, gameTime);
                    }

                    if (seekLocation.HasValue) {
                        HandleRotation(seekLocation.Value, currentPosition);
                    }
                    
                    currentSeekState = seekState.EnergyItem;
                } else {
                    currentSeekState = seekState.Flee;
                }

            } else if (game.enemyDamagedBehaviour == Game1.EnemyDamagedBehaviour.thief) {
                // if game1 behaviour is set to thief then regardless of what happens just steal the energy items! 
                if (targetItem.HasValue) {
                    HandleAStarSeek(targetItem.Value, currentPosition, gameTime);
                }

                if (seekLocation.HasValue) {
                    HandleRotation(seekLocation.Value, currentPosition);
                }
                currentSeekState = seekState.EnergyItem;
            } else {

                // if not damaged then seek player

                Vector3? targetPlayer = GetNearestPlayer();
                bClearedPlayerSeekPaths = false;

                if (targetPlayer.HasValue) {
                    HandleAStarSeek(targetPlayer.Value, currentPosition, gameTime);
                }

                if (seekLocation.HasValue) {
                    HandleRotation(seekLocation.Value, currentPosition);
                    currentSeekState = seekState.Player;
                }

            }              

            if (this.knockbackModelPosition != null) {
                HandleJump(false);
            }

            MovementClamp();

            // change enemy model to signify damage
            if (health < MAX_HEALTH) {
                base.tintColour = BasicModel.TINT_RED;
            } else {
                base.tintColour = BasicModel.TINT_TRANSPARENT;
            }

            base.Update(gameTime);
        }

        private Vector3? GetNearestPlayer() {
            Vector3? foundModel = null;

            if (models != null) {
                foreach (BasicModel model in models) {
                    
                    // if player
                    if (model.GetType() == typeof(Player)) {
                        foundModel = ((Player)model).translation.Translation;
                    }
                    
                }

            }

            return foundModel;
        }

        public Vector3? GetNearestEnergyItem() {
            float? shortestPathDistance = null;
            Vector3? closestModel = null;

            if (models != null) {
                foreach (BasicModel model in models) {

                    // if energy pickup item
                    if (model.GetType() == typeof(Pickup)) {
                        if (!shortestPathDistance.HasValue) {
                            shortestPathDistance = Vector3.Distance(((Pickup)model).translation.Translation, this.translation.Translation);
                            closestModel = ((Pickup)model).translation.Translation;
                        } else if (Vector3.Distance(((Pickup)model).translation.Translation, this.translation.Translation) < shortestPathDistance) {
                            shortestPathDistance = Vector3.Distance(((Pickup)model).translation.Translation, this.translation.Translation);
                            closestModel = ((Pickup)model).translation.Translation;
                        }
                    }

                }

            }

            return closestModel;

        }
        
        private void HandleRotation(Vector3 targetPosition, Vector3 currentModelPosition) {
            rotation = RotateToFace((Vector3)targetPosition, currentModelPosition, Vector3.Up);
        }

        Matrix RotateToFace(Vector3 targetPosition, Vector3 currentPosition, Vector3 up) {
            Vector3 newDirection = (targetPosition - currentPosition);
            Vector3 Right = Vector3.Cross(up, newDirection);
            Vector3.Normalize(ref Right, out Right);
            Vector3 Backwards = Vector3.Cross(Right, up);
            Vector3.Normalize(ref Backwards, out Backwards);
            Vector3 Up = Vector3.Cross(Backwards, Right);
            Up = new Vector3(Up.X, Up.Y, Up.Z);
            Matrix newRotation = new Matrix(Right.X, Right.Y, Right.Z, 0, Up.X, Up.Y, Up.Z, 0, Backwards.X, Backwards.Y, Backwards.Z, 0, 0, 0, 0, 1);
            return newRotation;
        }

        // TODO: move to model manager.
        private void MovementClamp() {
            float currentX = translation.Translation.X;
            float currentZ = translation.Translation.Z;
            float currentY = translation.Translation.Y;

            // clamp player within game area
            //currentX = MathHelper.Clamp(currentX, -650f, 650f);
            //currentZ = MathHelper.Clamp(currentZ, -650f, 450f);

            translation.Translation = new Vector3(currentX, currentY, currentZ);
        }

        private void HandleSeek(Vector3 targetPosition, Vector3 currentModelPosition, GameTime gameTime) {
            float elapsedTime = (float)gameTime.ElapsedGameTime.TotalMilliseconds / 80f;

            if ((isResting() || isMoving())) {
                if (Vector3.Distance(targetPosition, currentModelPosition) > 10f) {
                    translation.Translation += Vector3.Normalize(targetPosition - currentModelPosition) * moveSpeed * elapsedTime;
                    currentState = state.Moving;
                } else {
                    currentState = state.Resting;
                }
            } 

        }

        private void HandleAStarSeek(Vector3 targetPlayer, Vector3 currentModelPosition, GameTime gameTime) {
            // Implement a quasi queue. Issue a move command to enemy, wait for the enemy to become resting again, then issue another command.
            // this way we process through the path list

            if (this.isResting() && aStarPaths.Count() > 0) {

                seekLocation = new Vector3(aStarPaths.First().X, this.translation.Translation.Y, aStarPaths.First().Y);
                aStarPaths.RemoveAt(0);

                HandleSeek(seekLocation.Value, currentModelPosition, gameTime);

            } else if (this.isMoving() || this.isJumping() && seekLocation.HasValue) {
            
                HandleSeek(seekLocation.Value, currentModelPosition, gameTime);
                
            }

        }

        public override void Draw(GraphicsDevice device, Camera camera) {
            base.Draw(device, camera);
        }

        public override Matrix GetWorld() {
            return Matrix.CreateScale(6f) * rotation * translation;
        }

        /// <summary>
        /// Knockback the enemy when it hits a target
        /// </summary>
        /// <param name=""></param>
        internal void KnockBackFrom(BasicModel model) {
            
            // if knockback from player, then throw into the air, if another car, then small knockback
            if (model.GetType() == typeof(Player)) {
                this.knockbackModelPosition = model;

                if (this.knockbackModelPosition != null) {
                    HandleJump(true);
                }

                // make sure Y always at identity Y
                if (translation.Translation.Y < Matrix.Identity.Translation.Y) {
                    translation.Translation = new Vector3(translation.Translation.X, Matrix.Identity.Translation.Y, translation.Translation.Z);
                }

            } else {
                translation.Translation += Vector3.Normalize(translation.Translation - model.translation.Translation) * 50f;
            }

        }


        /// 
        /// jump model on collision
        /// 
        private void HandleJump(bool startJump) {

            // store the current jump Y position and modify each frame/tick with the current velocity
            float jumpPosition = translation.Translation.Y + jumpVelocity;
            
            
            // jump model into air with an initial velocity
            if (startJump && (isMoving() || isResting())) {
                jumpPosition += 5f;
                jumpVelocity += 5f;
                currentState = state.Jumping;
            }

            // gravity emulation
            // if jumping or falling, reduce the velocity 
            if (isJumping() || isFalling()) {
                jumpVelocity -= 0.15f;
                translation.Translation += Vector3.Normalize(translation.Translation - this.knockbackModelPosition.translation.Translation) * 3f;
            }
            // if reached highest expected jump height then start falling
            if (jumpPosition >= JUMP_HEIGHT) {
                currentState = state.Falling;
            }

            // clamp above ground, falling through ground!
            if (jumpPosition < Matrix.Identity.Translation.Y) {
                jumpVelocity = 0f;
                jumpPosition = Matrix.Identity.Translation.Y;
                currentState = state.Moving;
            }

            // set the camera position based on Y jump position which was altered
            translation.Translation = new Vector3(translation.Translation.X, jumpPosition, translation.Translation.Z);

        }

        internal void FullHealth() {
            health = MAX_HEALTH;
        }

        private bool isMoving() {
            return currentState == state.Moving;
        }
        private bool isResting() {
            return currentState == state.Resting;
        }
        private bool isRotating() {
            return currentState == state.Rotating;
        }
        public bool isSeekingPlayer() {
            return currentSeekState == seekState.Player;
        }
        public bool isSeekingEnergyItem() {
            return currentSeekState == seekState.EnergyItem;
        }
        public bool isSeekingFlee() {
            return currentSeekState == seekState.Flee;
        }

        private bool isFalling() {
            return currentState == state.Falling;
        }

        private bool isJumping() {
            return currentState == state.Jumping;
        }
               
    }
}
