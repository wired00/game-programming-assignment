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
    class MonsterTruck : BasicModel {

        public int MAX_HEALTH = 200;

        Player playerModel;

        public int health { get; set; }

        Matrix rotation = Matrix.Identity;

        state currentState = state.Resting;
        seekState currentSeekState = seekState.Player;

        float moveSpeed;

        /// <summary>
        /// List of a* found paths updated from the ModelManager
        /// </summary>
        public List<Vector2> aStarPaths { get; set; }

        public Vector3? seekLocation { get; set; }
        
        enum state {
            Moving,
            Resting,
            Rotating
        }

        enum seekState {
            EnergyItem,
            Player,
            Flee
        }

        public MonsterTruck(Model model, GraphicsDevice device, Camera camera, Vector3 position, Player playerModel, Game1 game)
            : base(model) {

            this.MAX_HEALTH = (int) game.truckHealth;
            this.moveSpeed = game.truckMoveSpeed;

            base.translation.Translation = position;

            this.playerModel = playerModel;
            health = MAX_HEALTH;
            Random rng = new Random();

            aStarPaths = new List<Vector2>();
            seekLocation = null;
        }

        public override void Update(GameTime gameTime) {
            float elapsedTime = (float)gameTime.ElapsedGameTime.TotalSeconds;

            Vector3 currentPosition = translation.Translation;
            
            // seek player, Unlike standard enemies (buggy) this big guy always seeks player regardless of dmg.

            Vector3? targetPlayer = GetNearestPlayer();

            if (targetPlayer.HasValue) {
                HandleAStarSeek(targetPlayer.Value, currentPosition, gameTime);
            }

            if (seekLocation.HasValue) {
                HandleRotation(seekLocation.Value, currentPosition);
                currentSeekState = seekState.Player;

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

        private void HandleRotation(Vector3 targetPosition, Vector3 currentModelPosition) {
            rotation = RotateToFace(targetPosition, currentModelPosition, Vector3.Up);
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
            } else if (this.isMoving()) {
                HandleSeek(seekLocation.Value, currentModelPosition, gameTime);
            }

        }

        public override void Draw(GraphicsDevice device, Camera camera) {
            base.Draw(device, camera);
        }

        public override Matrix GetWorld() {
            return Matrix.CreateScale(2.5f) * rotation * translation;
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
    }
}
