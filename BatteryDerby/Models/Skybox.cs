using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

/// <summary>
/// A skybox class
/// </summary>
namespace BatteryDerby {
    class Skybox : BasicModel {

        public Skybox(Model model)
            : base(model) {
        }

        public override void Update() {
            //translate with camera
            base.Update();
        }

        public override void Draw(GraphicsDevice device, Camera camera) {
            device.SamplerStates[0] = SamplerState.LinearClamp;
            base.Draw(device, camera);
        }

        public override Matrix GetWorld() {
            return Matrix.CreateScale(300f);
        }
    }
}
