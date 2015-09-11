using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Content;

namespace Game3 {
    class Enemy : BasicModel {

        public static int MAX_HEALTH = 100;

		Player playerTank;

        UIManager uiManager;
        
        public int health { get; set; }

        Matrix rotation = Matrix.Identity; // tank rotation
        
        // bones which will be animated
        ModelBone leftBackWheelBone;
        ModelBone rightBackWheelBone;
        ModelBone leftFrontWheelBone;
        ModelBone rightFrontWheelBone;

        // the original animating bone transform matrix must be stored
        Matrix leftBackWheelTransform;
        Matrix rightBackWheelTransform;
        Matrix leftFrontWheelTransform;
        Matrix rightFrontWheelTransform;

        Matrix[] boneTransforms;

        state currentState = state.Resting;

        Vector3? lastPos;
        int moveCount = 0;
        
        float moveSpeed = 20f;
        float wheelRotateSpeed = 10f;
        bool seekingOther;
        
        Vector3 other;

        enum state {
            Moving,
            Resting,
            Rotating,
            SeekingEnergyItem,
            SeekingPlayer, 

        }

		public Enemy(Model model, GraphicsDevice device, Camera camera, Vector3 position, Player playerTank, UIManager uiManager)
            : base(model) {

			translation.Translation = position;

            boneTransforms = new Matrix[model.Bones.Count];

            // references to bones to animate
//            leftBackWheelBone = model.Bones["l_back_wheel_geo"];
//            rightBackWheelBone = model.Bones["r_back_wheel_geo"];
//            leftFrontWheelBone = model.Bones["l_front_wheel_geo"];
//            rightFrontWheelBone = model.Bones["r_front_wheel_geo"];

            // store the original transform matrix, otherwise animations on rotations will be all wonky
//            leftBackWheelTransform = leftBackWheelBone.Transform;
//            rightBackWheelTransform = rightBackWheelBone.Transform;
//            leftFrontWheelTransform = leftFrontWheelBone.Transform;
//            rightFrontWheelTransform = rightFrontWheelBone.Transform;

			this.playerTank = playerTank;
            health = MAX_HEALTH;
            Random rng = new Random();
            other = new Vector3(rng.Next(-300, 300), 0, rng.Next(-250, 250));

            this.uiManager = uiManager;
        }

        public override void Update(GameTime gameTime) {
            float elapsedTime = (float) gameTime.ElapsedGameTime.TotalSeconds;

            Vector3 currentTankPosition = translation.Translation;

            Vector3? targetPlayer = GetNearestPlayer();

            //Console.WriteLine("seekOther: " + seekingOther + ", " + "moveCount: " + moveCount);


            if (seekingOther)
            {
                HandleSeek(other, currentTankPosition, gameTime);
                if (Vector3.Distance(other, currentTankPosition) < 10) {
                    //currentState = state.Resting;
                    seekingOther = false;
                    Random rng = new Random();
                    other = new Vector3(rng.Next(-300, 300), 0, rng.Next(-250, 250));
                }
                HandleTankRotation(other, currentTankPosition);
            }
            else if (targetPlayer.HasValue)
            {
                // determine state
                if (health < MAX_HEALTH)
                {
                    Vector3? targetItem = GetNearestEnergyItem();
                    // if safe distance from player then seek health, otherwise flee
                    if (Vector3.Distance((Vector3)targetPlayer, currentTankPosition) > 200f && targetItem.HasValue)
                    {
                        HandleTankRotation((Vector3)targetItem, currentTankPosition);
                        HandleSeek((Vector3)targetItem, currentTankPosition, gameTime);
                    }
                    else
                    {
                        HandleFlee((Vector3)targetPlayer, currentTankPosition, gameTime);
                    }
                }
                else
                {
                    HandleTankRotation((Vector3)targetPlayer, currentTankPosition);
                    HandleSeek((Vector3)targetPlayer, currentTankPosition, gameTime);
                }
            }

            MovementClamp();
            //CheckMovement();

            // change enemy model red to signify damage
            if (health < MAX_HEALTH) {
                base.tintColour = Color.Red.ToVector3();
            } else {
                base.tintColour = Color.Transparent.ToVector3();
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

        /// wheel rotation
        private void HandleRotateWheels(float elapsedTime) {
            float wheelRotationValue = elapsedTime * wheelRotateSpeed;

            Matrix wheelRotation = Matrix.CreateRotationX(wheelRotationValue);

            leftBackWheelBone.Transform = wheelRotation * leftBackWheelTransform;
            rightBackWheelBone.Transform = wheelRotation * rightBackWheelTransform;
            leftFrontWheelBone.Transform = wheelRotation * leftFrontWheelTransform;
            rightFrontWheelBone.Transform = wheelRotation * rightFrontWheelTransform;
        }
        
        /// whole tank rotation
        private void HandleTankRotation(Vector3 targetPosition, Vector3 currentTankPosition) {
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

            // clamp player within game area
            currentX = MathHelper.Clamp(currentX, -550f, 550f);
            currentZ = MathHelper.Clamp(currentZ, -550f, 250f);

            translation.Translation = new Vector3(currentX, 0, currentZ);
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
            return Matrix.CreateScale(0.1f) * rotation * translation;
        }

        /// <summary>
        /// Knockback the enemy when it hits a target. Use similar as flee mechanic
        /// TODO: Convert this to do a jump mechanic at same time.
        /// </summary>
        /// <param name=""></param>
        internal void KnockBackFrom(BasicModel model) {
            translation.Translation += Vector3.Normalize(translation.Translation - model.translation.Translation) * 50f;

            // some reason Y can go below 0, make sure Y always at identity Y
            if (translation.Translation.Y < Matrix.Identity.Translation.Y) {
                translation.Translation = new Vector3(translation.Translation.X, Matrix.Identity.Translation.Y, translation.Translation.Z);
            }

        }

        internal void CheckMovement() {

            if (lastPos.HasValue) {
                float distanceFromLastPosition = Vector3.Distance((Vector3)lastPos, this.translation.Translation);
                if ((distanceFromLastPosition < 5f && distanceFromLastPosition > 3.5f) && health < MAX_HEALTH) {
                    Console.WriteLine(Vector3.Distance((Vector3)lastPos, this.translation.Translation));
                    moveCount++;
                    if (moveCount >= 30) {
                        moveCount = 0;
                        seekingOther = true;
                    }
                }
            }

            lastPos = this.translation.Translation;
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

    }
}
