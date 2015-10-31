using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

/// <summary>
/// Debug Indicator to show where a NPC is pathing using A*
/// </summary>
namespace BatteryDerby {
    class SeekIndicator : BasicModel {

        static float ROTATION_SPEED = 2f;

        Matrix rotation = Matrix.Identity;

        Matrix originalRotation = Matrix.Identity;

        public SeekIndicator(Model model, Vector3 position)
            : base(model) {

            position = new Vector3(position.X, 20f, position.Z);

            translation.Translation = position;

            originalRotation = rotation;

        }

        public override void Draw(GraphicsDevice device, Camera camera) {
            device.SamplerStates[0] = SamplerState.LinearWrap;
            base.Draw(device, camera);
        }

        public override void Update(GameTime gameTime) {

            float elapsedTime = (float)gameTime.TotalGameTime.TotalSeconds;
            HandleRotate(elapsedTime);

            base.Update(gameTime);
        }

        public override Matrix GetWorld() {
            return Matrix.CreateScale(4f) * rotation * translation;
        }

        private void HandleRotate(float elapsedTime) {
            float rotationValue = elapsedTime * ROTATION_SPEED;

            Matrix newRotation = Matrix.CreateRotationY(rotationValue);
            this.rotation = newRotation * originalRotation;
        }

    }
}


