using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

/// <summary>
/// Tire model
/// </summary>
namespace BatteryDerby {
    class Tire : BasicModel {

        Matrix originalRotation = Matrix.Identity;

        public bool walkable { get; set; }

        public Tire(Model model, Vector3 position)
            : base(model) {

            translation.Translation = position;
            walkable = false;
        }
   
        public override void Draw(GraphicsDevice device, Camera camera) {
            device.SamplerStates[0] = SamplerState.LinearWrap;
            base.Draw(device, camera);
        }

        public override void Update(GameTime gameTime) {
            
            base.Update(gameTime);
        }

        public override Matrix GetWorld() {
            return Matrix.CreateScale(5f) * translation;
        }
        
    }
}


