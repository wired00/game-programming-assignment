using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

/// <summary>
/// Players camer model
/// </summary>
namespace BatteryDerby {
    public class Camera : GameComponent {
        // matrix object represents the location and rotation of the camera
        public Matrix view { get; protected set; }

        // matrix object represents the camera view including near and far clipping plane
        public Matrix projection { get; protected set; }

        // camera vectors
        public Vector3 cameraPosition { get; protected set; }
        Vector3 cameraDirection;
        Vector3 cameraUp;

        State currentState = State.Walking;

        MouseState prevMouseState;

        /// Camera states enumerator
        enum State {
            Walking
        }

        public Camera(Game game, Vector3 pos, Vector3 target, Vector3 up)
                : base(game) {

            // build camera view matrix
            cameraPosition = pos;
            cameraDirection = target - pos;
            cameraDirection.Normalize();
            cameraUp = up;
            CreateLookAt();
            

            projection = Matrix.CreatePerspectiveFieldOfView(
                MathHelper.PiOver4, 
                (float)Game.Window.ClientBounds.Width / 
                (float)Game.Window.ClientBounds.Height, 
                1, 3000);
        }

        public override void Initialize() {

            Mouse.SetPosition(Game.Window.ClientBounds.Width / 2,
                Game.Window.ClientBounds.Height / 2);

            prevMouseState = Mouse.GetState();

            base.Initialize();
        }

        public override void Update(GameTime gameTime) {

            CreateLookAt();

            base.Update(gameTime);
        }
		

        private void CreateLookAt() {
            view = Matrix.CreateLookAt(
                cameraPosition,
                cameraPosition + cameraDirection,
                cameraUp);
        }

        private bool isWalking() {
            return currentState == State.Walking;
        }

    }

}