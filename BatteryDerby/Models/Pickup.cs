﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

/// <summary>
/// Players power up pickup model
/// </summary>
namespace BatteryDerby {
    class Pickup : BasicModel {

        static float ROTATION_SPEED = 2f;

        Matrix rotation = Matrix.Identity;

        Matrix originalRotation = Matrix.Identity;

        public Pickup(Model model, Vector3 position)
            : base(model) {

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
            return Matrix.CreateScale(3f) * rotation * translation;
        }

        private void HandleRotate(float elapsedTime) {
            float rotationValue = elapsedTime * ROTATION_SPEED;

            Matrix newRotation = Matrix.CreateRotationY(rotationValue);
            this.rotation = newRotation * originalRotation;
        }

    }
}


