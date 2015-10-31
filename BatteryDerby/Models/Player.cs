using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Audio;

/// <summary>
/// Players model
/// </summary>
namespace BatteryDerby {
    public class Player : BasicModel {

        int MAX_HEALTH;
        public static float MAX_ENERGY = 100;
        static float MAX_MOVE_SPEED = 9f;
        float MOVE_SPEED = 1f;
        static float VELOCITY_INCREMENTOR = 0.1f;
        static float VELOCITY_DECREMENTOR = 0.95f;
        public const int MINX = 96;
        public const int MAXX = 1440;
        public const int MINY = 96;
        public const int MAXY = 1056;

        Matrix playerRotation = Matrix.Identity;

        GraphicsDeviceManager graphicsDeviceManager;

        Vector3 newPosition;

        public int health { get; set; }

        public float energy { get; set; }

        state currentState = state.Resting;
        direction currentDirection;
        UIManager uiManager;
        AudioManager audioManager;

        float turboSpeed = 1f;
        float velocity = 0f; // need to track velocity so the car naturally slows down

        enum state {
            Moving,
            Resting,
            Slowing,
            Boosting
        }

        enum direction {
            left,
            right,
            up,
            back,
            left_up,
            right_up,
            left_down,
            right_down,
            unset
        }

        public Player(Model model, GraphicsDevice device, Camera camera, Game1 game)
            : base(model) {

            this.MAX_HEALTH = (int)game.playerHealth;
            this.MOVE_SPEED = game.playerMoveSpeed;

            graphicsDeviceManager = game.graphics;
            this.uiManager = game.uiManager;
            this.audioManager = game.audioManager;

            health = MAX_HEALTH;
            energy = MAX_ENERGY;

            base.translation.Translation = new Vector3(500f, 0f, 500f);
        }

        public override void Update(GameTime gameTime) {
            float elapsedTime = (float)gameTime.ElapsedGameTime.TotalSeconds;

            Vector3 currentPlayerPosition = translation.Translation;

            HandleMovement(elapsedTime);

            MovementClamp();

            HandleDecceleration(elapsedTime);

            translation.Translation = newPosition;

            // if the model has moved then rotate to face new position
            if (newPosition != currentPlayerPosition) {
                playerRotation = RotateToFace(newPosition, currentPlayerPosition, Vector3.Up); // rotate to the future position
            }

            uiManager.playerHealth = this.health;
            uiManager.playerEnergy = this.energy;

            if (!isMoving() && audioManager.idleLoop.State != SoundState.Playing && energy > 0) {
                audioManager.revving.Stop();
                audioManager.accelerate.Pause();
                audioManager.idleLoop.Play();
            }
            if (isBoosting() && audioManager.boost.State != SoundState.Playing) {
                audioManager.boost.Play();
            } else if (!isBoosting()) {
                audioManager.boost.Pause();
            }

            base.Update(gameTime);
        }

        /// whole Player rotation
        private void HandlePlayerRotation(Vector3? pickPosition, Vector3 currentPlayerPosition) {

            if (pickPosition.HasValue == true) {

                // cross product method
                playerRotation = RotateToFace((Vector3)pickPosition, currentPlayerPosition, Vector3.Up);

            }

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

        ///
        /// move left / right
        /// 
        private void HandleMovement(float elapsedTime) {
            newPosition = translation.Translation;
            if (energy > 0) {
                if (Keyboard.GetState().IsKeyDown(Keys.A) && Keyboard.GetState().IsKeyDown(Keys.W)) {
                    currentState = state.Moving;
                    HandleAcceleration();

                    newPosition.X -= velocity * turboSpeed;
                    newPosition.Z -= velocity * turboSpeed;

                    currentDirection = direction.left_up;
                } else if (Keyboard.GetState().IsKeyDown(Keys.A) && Keyboard.GetState().IsKeyDown(Keys.S)) {
                    currentState = state.Moving;
                    HandleAcceleration();

                    newPosition.X -= velocity * turboSpeed;
                    newPosition.Z += velocity * turboSpeed;

                    currentDirection = direction.left_down;

                } else if (Keyboard.GetState().IsKeyDown(Keys.D) && Keyboard.GetState().IsKeyDown(Keys.W)) {
                    currentState = state.Moving;
                    HandleAcceleration();

                    newPosition.X += velocity * turboSpeed;
                    newPosition.Z -= velocity * turboSpeed;

                    currentDirection = direction.right_up;

                } else if (Keyboard.GetState().IsKeyDown(Keys.D) && Keyboard.GetState().IsKeyDown(Keys.S)) {
                    currentState = state.Moving;
                    HandleAcceleration();

                    newPosition.X += velocity * turboSpeed;
                    newPosition.Z += velocity * turboSpeed;

                    currentDirection = direction.right_down;

                } else if (Keyboard.GetState().IsKeyDown(Keys.A)) {
                    currentState = state.Moving;
                    HandleAcceleration();

                    newPosition.X -= velocity * turboSpeed;

                    currentDirection = direction.left;

                } else if (Keyboard.GetState().IsKeyDown(Keys.D)) {
                    currentState = state.Moving;
                    HandleAcceleration();

                    newPosition.X += velocity * turboSpeed;
                    currentDirection = direction.right;

                } else if (Keyboard.GetState().IsKeyDown(Keys.W)) {
                    currentState = state.Moving;
                    HandleAcceleration();

                    newPosition.Z -= velocity * turboSpeed;
                    currentDirection = direction.up;

                } else if (Keyboard.GetState().IsKeyDown(Keys.S)) {
                    currentState = state.Moving;
                    HandleAcceleration();

                    newPosition.Z += velocity * turboSpeed;
                    currentDirection = direction.back;

                } else {
                    currentState = state.Slowing;
                }

                // turbo boost if space bar held down
                if (Keyboard.GetState().IsKeyDown(Keys.Space)) {
                    turboSpeed = 2f;
                    currentState = state.Boosting;
                } else {
                    turboSpeed = 1f;
                    //currentState = state.Moving;
                }
            } else {
                currentState = state.Slowing;
                energy = 0f;
                audioManager.boost.Pause();
                audioManager.accelerate.Pause();
                audioManager.idleLoop.Pause();
            }
        }

        private void MovementClamp() {
            float currentX = newPosition.X;
            float currentZ = newPosition.Z;

            // clamp player within game area
            currentX = MathHelper.Clamp(currentX, MINX, MAXX);
            currentZ = MathHelper.Clamp(currentZ, MINY, MAXY);

            newPosition = new Vector3(currentX, 0, currentZ);
        }

        private void HandleAcceleration() {

            if (isMoving()) {
                currentDirection = direction.unset;
                // set initial velocity of car
                if (this.velocity <= 0) {
                    this.velocity = MOVE_SPEED;
                }

                // increase acceleration & clamp under max speed
                if (this.velocity < MAX_MOVE_SPEED) {
                    this.velocity += VELOCITY_INCREMENTOR;
                }
                energy -= .11f;
                if (audioManager.accelerate.State != SoundState.Playing) {
                    audioManager.accelerate.Play();
                    audioManager.idleLoop.Pause();
                }
            }
        }

        private void HandleDecceleration(float elapsedTime) {

            if (isSlowing()) {
                if (this.velocity > 0.01f) {
                    this.velocity *= VELOCITY_DECREMENTOR;

                    if (currentDirection != direction.unset) {
                        switch (this.currentDirection) {
                            case direction.right_up:
                                newPosition.X += velocity;
                                newPosition.Z -= velocity;
                                break;
                            case direction.left_up:
                                newPosition.Z -= velocity;
                                newPosition.X -= velocity;
                                break;
                            case direction.right_down:
                                newPosition.Z += velocity;
                                newPosition.X += velocity;
                                break;
                            case direction.left_down:
                                newPosition.Z += velocity;
                                newPosition.X -= velocity;
                                break;
                            case direction.left:
                                newPosition.X -= velocity;
                                break;
                            case direction.right:
                                newPosition.X += velocity;
                                break;
                            case direction.up:
                                newPosition.Z -= velocity;
                                break;
                            case direction.back:
                                newPosition.Z += velocity;
                                break;
                        }
                    }

                } else if (this.velocity <= 0.01f) {
                    this.velocity = 0;

                    currentState = state.Resting;
                }
            }

        }

        public override void Draw(GraphicsDevice device, Camera camera) {
            base.Draw(device, camera);
        }

        public override Matrix GetWorld() {
            return Matrix.CreateScale(4f) * playerRotation * translation;
        }

        /// <summary>
        /// Knockback the player when it hits a target. Use similar as flee mechanic
        /// TODO: Convert this to do a jump mechanic at same time.
        /// </summary>
        /// <param name=""></param>
        public void KnockBackFrom(BasicModel model) {
            float knockbackDistance = 5f;

            if (model.GetType() == typeof(Enemy)) {
                knockbackDistance = 50f;
            } else if (model.GetType() == typeof(MonsterTruck)) {
                knockbackDistance = 100f;
            } else {
                // knockback further if boosting
                if (isBoosting()) {
                    knockbackDistance = 12f;
                }
            }

            translation.Translation += Vector3.Normalize(translation.Translation - model.translation.Translation) * knockbackDistance;

            // some reason Y can go below 0, make sure Y always at identity Y
            if (translation.Translation.Y < Matrix.Identity.Translation.Y) {
                translation.Translation = new Vector3(translation.Translation.X, Matrix.Identity.Translation.Y, translation.Translation.Z);
            }

        }

        private bool isMoving() {
            return currentState == state.Moving;
        }
        public bool isBoosting() {
            return currentState == state.Boosting;
        }
        private bool isSlowing() {
            return currentState == state.Slowing;
        }
        private bool isResting() {
            return currentState == state.Resting;
        }

    }
}
