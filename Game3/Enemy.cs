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
namespace Game3 {
    class Enemy : BasicModel {

        public static int MAX_HEALTH = 100;

		Player playerTank;

        UIManager uiManager;
        
        public int health { get; set; }

        Matrix rotation = Matrix.Identity; // tank rotationss

        state currentState = state.Resting;
        
        float moveSpeed = 20f;
        float jumpVelocity = 0f;
        static float JUMP_HEIGHT = 35f;
        private BasicModel knockbackModelPosition;

        enum state {
            Moving,
            Resting,
            Rotating,
            SeekingEnergyItem,
            SeekingPlayer,
            Jumping,
            Falling
        }

		public Enemy(Model model, GraphicsDevice device, Camera camera, Vector3 position, Player playerTank, UIManager uiManager)
            : base(model) {

			base.translation.Translation = position;

			this.playerTank = playerTank;
            health = MAX_HEALTH;
            Random rng = new Random();

            this.uiManager = uiManager;

            base.tintColour = BasicModel.TINT_BLUE;

        }

        public override void Update(GameTime gameTime) {
            float elapsedTime = (float) gameTime.ElapsedGameTime.TotalSeconds;

            Vector3 currentTankPosition = translation.Translation;

            Vector3? targetPlayer = GetNearestPlayer();

            if (targetPlayer.HasValue) {
                // if damaged, first flee from player then seek health box
                if (health < MAX_HEALTH) {
                    Vector3? targetItem = GetNearestEnergyItem();

                    // if safe distance from player then seek health, otherwise flee
                    if (Vector3.Distance((Vector3)targetPlayer, currentTankPosition) > 200f && targetItem.HasValue) {
                        HandleRotation((Vector3)targetItem, currentTankPosition);
                        HandleSeek((Vector3)targetItem, currentTankPosition, gameTime);
                    } else {
                        HandleFlee((Vector3)targetPlayer, currentTankPosition, gameTime);
                    }
                } else {
                    HandleRotation((Vector3)targetPlayer, currentTankPosition);
                    HandleSeek((Vector3)targetPlayer, currentTankPosition, gameTime);
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
                base.tintColour = BasicModel.TINT_BLUE;
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

        private Vector3? GetNearestEnergyItem() {
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
        
        private void HandleRotation(Vector3 targetPosition, Vector3 currentTankPosition) {
            rotation = RotateToFace((Vector3)targetPosition, currentTankPosition, Vector3.Up);
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

        private void MovementClamp() {
            float currentX = translation.Translation.X;
            float currentZ = translation.Translation.Z;
            float currentY = translation.Translation.Y;

            // clamp player within game area
            currentX = MathHelper.Clamp(currentX, -650f, 650f);
            currentZ = MathHelper.Clamp(currentZ, -550f, 250f);

            translation.Translation = new Vector3(currentX, currentY, currentZ);
        }

        private void HandleSeek(Vector3 targetPosition, Vector3 currentTankPosition, GameTime gameTime) {
            float elapsedTime = (float)gameTime.ElapsedGameTime.TotalMilliseconds / 80f;   
            
            if ((isResting() || isMoving())) {
                if (Vector3.Distance(targetPosition, currentTankPosition) > 10f) {
                    translation.Translation += Vector3.Normalize(targetPosition - currentTankPosition) * moveSpeed * elapsedTime;
                    currentState = state.Moving;
                }
            }

        }

        private void HandleFlee(Vector3 targetPosition, Vector3 currentTankPosition, GameTime gameTime) {
            float elapsedTime = (float)gameTime.ElapsedGameTime.TotalMilliseconds / 80f;

            translation.Translation += Vector3.Normalize(currentTankPosition - targetPosition) * moveSpeed * elapsedTime;
        }

        public override void Draw(GraphicsDevice device, Camera camera) {
            base.Draw(device, camera);
        }

        public override Matrix GetWorld() {
            return Matrix.CreateScale(2f) * rotation * translation;
        }

        /// <summary>
        /// Knockback the enemy when it hits a target. Use similar to flee mechanic
        /// TODO: Convert this to do a jump mechanic at same time.
        /// </summary>
        /// <param name=""></param>
        internal void KnockBackFrom(BasicModel model) {
            //translation.Translation += Vector3.Normalize(translation.Translation - model.translation.Translation) * 50f;

            this.knockbackModelPosition = model;

            if (this.knockbackModelPosition != null) {
                HandleJump(true);
            }

            // make sure Y always at identity Y
            if (translation.Translation.Y < Matrix.Identity.Translation.Y) {
                translation.Translation = new Vector3(translation.Translation.X, Matrix.Identity.Translation.Y, translation.Translation.Z);
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
                //Console.WriteLine("JUMPING INITIALISE");
                jumpPosition += 10f;
                jumpVelocity += 10f;
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
        private bool iSeekingPlayer() {
            return currentState == state.SeekingPlayer;
        }
        private bool isSeekingEnergyItem() {
            return currentState == state.SeekingEnergyItem;
        }

        private bool isFalling() {
            return currentState == state.Falling;
        }

        private bool isJumping() {
            return currentState == state.Jumping;
        }

    }
}
