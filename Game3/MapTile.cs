using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

/// <summary>
/// Map tile model, used to created tiles on the ground
/// </summary>
namespace Game3 {
    class MapTile : BasicModel {
        public MapTile(Model model, Vector3 position)
            : base(model) {

            translation.Translation = position;

        }
   
        public override void Draw(GraphicsDevice device, Camera camera) {
            device.SamplerStates[0] = SamplerState.LinearWrap;
            base.Draw(device, camera);
        }

        public override Matrix GetWorld() {
            return Matrix.CreateScale(3f) * translation;
        }

    }
}
