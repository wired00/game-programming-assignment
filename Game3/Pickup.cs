using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Game3 {
    class Pickup : BasicModel {

        public Matrix translation = Matrix.Identity;

        public Pickup(Model model, Vector3 position)
            : base(model) {

            translation.Translation = position;

        }
   
        public override void Draw(GraphicsDevice device, Camera camera) {
            device.SamplerStates[0] = SamplerState.LinearWrap;
            base.Draw(device, camera);
        }

        public override Matrix GetWorld() {
            return Matrix.CreateScale(5f) * translation;
        }

    }
}
