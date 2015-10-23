using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Content;

namespace Game3 {
    class Tank : BasicModel {
        public string name;
        public Matrix translation = Matrix.Identity;
        Matrix rotation = Matrix.Identity;

        GraphicsDeviceManager graphicsDeviceManager;

        Vector3 newPosition;

        int maxHealth = 100;
        public int health { get; set; }

        MousePick mousePick;
        Vector3? pickPosition;
        Vector3? mousePosition;

        // bones which will be animated
        ModelBone turretBone;
        ModelBone leftBackWheelBone;
        ModelBone rightBackWheelBone;
        ModelBone leftFrontWheelBone;
        ModelBone rightFrontWheelBone;
        ModelBone cannonBone;

        public static explicit operator Tank(Model v) {
            throw new NotImplementedException();
        }

        // the original animating bone transform matrix must be stored
        Matrix turretTransform;
        Matrix leftBackWheelTransform;
        Matrix rightBackWheelTransform;
        Matrix leftFrontWheelTransform;
        Matrix rightFrontWheelTransform;
        Matrix cannonTransform;

        Matrix[] boneTransforms;

        state currentState = state.Resting;
        direction currentDirection;

        float maxMoveSpeed = 9f;
        float turboSpeed = 1f;
        float moveSpeed = 3f;
        float wheelRotateSpeed = 10f;
        float velocity = 0f; // need to track velocity so the car naturally slows down
        float velocityIncrementor = 0.3f;
        float velocityDecrementor = 0.95f;
        
        enum state {
            Moving,
            Resting,
            Slowing
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

        public Tank(Model model, GraphicsDevice device, Camera camera, GraphicsDeviceManager graphics, String name)
            : base(model) {

            this.name = name;
            graphicsDeviceManager = graphics;

            mousePick = new MousePick(device, camera);

            boneTransforms = new Matrix[model.Bones.Count];

            // references to bones to animate
            turretBone = model.Bones["turret_geo"];
            leftBackWheelBone = model.Bones["l_back_wheel_geo"];
            rightBackWheelBone = model.Bones["r_back_wheel_geo"];
            leftFrontWheelBone = model.Bones["l_front_wheel_geo"];
            rightFrontWheelBone = model.Bones["r_front_wheel_geo"];
            cannonBone = model.Bones["canon_geo"];

            // store the original transform matrix, otherwise animations on rotations will be all wonky
            turretTransform = turretBone.Transform;
            leftBackWheelTransform = leftBackWheelBone.Transform;
            rightBackWheelTransform = rightBackWheelBone.Transform;
            leftFrontWheelTransform = leftFrontWheelBone.Transform;
            rightFrontWheelTransform = rightFrontWheelBone.Transform;
            cannonTransform = cannonBone.Transform;

            health = maxHealth;
        }

        public override void Update(GameTime gameTime) {

            float elapsedTime = (float)gameTime.ElapsedGameTime.TotalSeconds;

            Vector3 currentTankPosition = translation.Translation;


            HandleMovement(elapsedTime);

            MovementClamp();

            HandleDecceleration(elapsedTime);

            translation.Translation = newPosition;

            // if the model has moved then rotate to face new position
            if (newPosition != currentTankPosition) {
                rotation = RotateToFace(newPosition, currentTankPosition, Vector3.Up); // rotate to the future position
            }

			base.Update(gameTime);
        }

        /// wheel rotation
        private void HandleRotateWheels(float elapsedTime) {
            float wheelRotationValue = elapsedTime * wheelRotateSpeed;

            Matrix wheelRotation = Matrix.CreateRotationX(wheelRotationValue);

            leftBackWheelBone.Transform = wheelRotation * leftBackWheelTransform;
            rightBackWheelBone.Transform = wheelRotation * rightBackWheelTransform;
            leftFrontWheelBone.Transform = wheelRotation * leftFrontWheelTransform;
            rightFrontWheelBone.Transform = wheelRotation * rightFrontWheelTransform;
        }
        
        /// rotate turret
        private void HandleRotateTurret(float elapsedTime) {
            if (mousePosition.HasValue) {
                Matrix newTurretFace = RotateToFace((Vector3)mousePosition, turretBone.Transform.Translation, Vector3.Up);
                newTurretFace.Translation = new Vector3(newTurretFace.Translation.X, newTurretFace.Translation.Y, newTurretFace.Translation.Z);
                turretBone.Transform = newTurretFace * turretTransform;
            }
        }

        /// move cannon up and down
        private void HandleRotateCannon(float elapsedTime) {
            float cannonRotationValue = (float)Math.Sin(elapsedTime * 0.5f) * 0.333f - 0.333f;
            Matrix cannonRotation = Matrix.CreateRotationX(cannonRotationValue);
            cannonBone.Transform = cannonRotation * cannonTransform;
        }

        /// whole tank rotation
        private void HandleTankRotation(Vector3? pickPosition, Vector3 currentTankPosition) {

            if (pickPosition.HasValue == true) {

                // cross product method
                rotation = RotateToFace((Vector3)pickPosition, currentTankPosition, Vector3.Up);

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

            //Console.WriteLine("currentDirectionL " + currentDirection + ", " + velocity);

            if (Keyboard.GetState().IsKeyDown(Keys.A) && Keyboard.GetState().IsKeyDown(Keys.W)) {
                currentState = state.Moving;
                HandleAcceleration();

                newPosition.X -= velocity * turboSpeed;
                newPosition.Z -= velocity * turboSpeed;

                HandleRotateWheels(elapsedTime);
                currentDirection = direction.left_up;
            } else if (Keyboard.GetState().IsKeyDown(Keys.A) && Keyboard.GetState().IsKeyDown(Keys.S)) {
                currentState = state.Moving;
                HandleAcceleration();

                newPosition.X -= velocity * turboSpeed;
                newPosition.Z += velocity * turboSpeed;


                HandleRotateWheels(elapsedTime);
                currentDirection = direction.left_down;

            } else if (Keyboard.GetState().IsKeyDown(Keys.D) && Keyboard.GetState().IsKeyDown(Keys.W)) {
                currentState = state.Moving;
                HandleAcceleration();

                newPosition.X += velocity * turboSpeed;
                newPosition.Z -= velocity * turboSpeed;

                HandleRotateWheels(elapsedTime);
                currentDirection = direction.right_up;

            } else if (Keyboard.GetState().IsKeyDown(Keys.D) && Keyboard.GetState().IsKeyDown(Keys.S)) {
                currentState = state.Moving;
                HandleAcceleration();

                newPosition.X += velocity * turboSpeed;
                newPosition.Z += velocity * turboSpeed;

                HandleRotateWheels(elapsedTime);
                currentDirection = direction.right_down;

            } else if (Keyboard.GetState().IsKeyDown(Keys.A)) {
                currentState = state.Moving;
                HandleAcceleration();

                newPosition.X -= velocity * turboSpeed;
                
                HandleRotateWheels(elapsedTime);
                currentDirection = direction.left;

            } else if (Keyboard.GetState().IsKeyDown(Keys.D)) {
                currentState = state.Moving;
                HandleAcceleration();

                newPosition.X += velocity * turboSpeed;
                HandleRotateWheels(elapsedTime);
                currentDirection = direction.right;

            }  else if (Keyboard.GetState().IsKeyDown(Keys.W)) {
                currentState = state.Moving;
                HandleAcceleration();

                newPosition.Z -= velocity * turboSpeed;
                HandleRotateWheels(elapsedTime);
                currentDirection = direction.up;

            } else if (Keyboard.GetState().IsKeyDown(Keys.S)) {
                currentState = state.Moving;
                HandleAcceleration();

                newPosition.Z += velocity * turboSpeed;
                HandleRotateWheels(elapsedTime);
                currentDirection = direction.back;

            } else {
                currentState = state.Slowing;
            }

            // turbo boost if space bar held down
            if (Keyboard.GetState().IsKeyDown(Keys.Space)) {
                turboSpeed = 1.5f;
            } else {
                turboSpeed = 1f;
            }
        }

        private void MovementClamp() {
            float currentX = newPosition.X;
            float currentZ = newPosition.Z;
            

            // clamp player within game area
            currentX = MathHelper.Clamp(currentX, -550f, 550f);
            currentZ = MathHelper.Clamp(currentZ, -550f, 250f);

            newPosition = new Vector3(currentX, 0, currentZ);
        }

        private void HandleAcceleration() {

            if (isMoving()) {
                currentDirection = direction.unset;
                // set initial velocity of car
                if (this.velocity == 0) {
                    this.velocity = moveSpeed;
                }

                // increase acceleration & clamp under max speed
                if (this.velocity < this.maxMoveSpeed) {
                    this.velocity += this.velocityIncrementor;
                }
            }
       
        }

        private void HandleDecceleration(float elapsedTime) {

            if (isSlowing()) {
                if (this.velocity > 0.01f) {
                    this.velocity *= this.velocityDecrementor;

                    if (this.velocity > 0.1f) {
                        HandleRotateWheels(elapsedTime);
                    }

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

                }  else if (this.velocity <= 0.01f ) {
                    this.velocity = 0;

                    currentState = state.Resting;
                }


            }

        }

        public override void Draw(GraphicsDevice device, Camera camera) {
            base.Draw(device, camera);
        }

        public override Matrix GetWorld() {
            return Matrix.CreateScale(0.1f) * rotation * translation;
        }

        /// <summary>
        /// Knockback the player when it hits a target. Use similar as flee mechanic
        /// TODO: Convert this to do a jump mechanic at same time.
        /// </summary>
        /// <param name=""></param>
        internal void KnockBackFrom(Enemy model) {
            translation.Translation += Vector3.Normalize(translation.Translation - model.GetWorld().Translation) * 50f;

            // some reason Y can go below 0, make sure Y always at identity Y
            if (translation.Translation.Y < Matrix.Identity.Translation.Y) {
                translation.Translation = new Vector3(translation.Translation.X, Matrix.Identity.Translation.Y, translation.Translation.Z);
            }

        }

        private bool isMoving() {
            return currentState == state.Moving;
        }
        private bool isSlowing() {
            return currentState == state.Slowing;
        }
        private bool isResting() {
            return currentState == state.Resting;
        }

    }
}
