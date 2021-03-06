﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

/// <summary>
/// Basic model is inherited by all other models
/// </summary>
namespace BatteryDerby {
    public class BasicModel {

        public static Vector3 TINT_RED = Color.Firebrick.ToVector3();
        public static Vector3 TINT_TRANSPARENT = Color.Transparent.ToVector3();
        public static Vector3 TINT_BLUE = Color.DarkBlue.ToVector3();

        public Matrix translation = Matrix.Identity; // model translation / position matrix

        public Model model { get; protected set; }

        public List<BasicModel> models { get; set; }

        public int uniqueId { get; protected set; }

        protected Matrix world = Matrix.Identity;

        public drawState currentDrawState { get; set; }

        public Vector3 tintColour { get; set; }

        public enum drawState {
            show,
            remove
        }

        public BasicModel(Model model) {

            uniqueId = Guid.NewGuid().GetHashCode();

            this.model = model;
        }

        public virtual void Update() {

        }

        public virtual void Update(GameTime gametime) {

        }

        public virtual void Update(GameTime gametime, List<BasicModel> models) {

        }

        public virtual void Draw(GraphicsDevice device, Camera camera) {

            Matrix[] transformation = new Matrix[model.Bones.Count];

            model.CopyAbsoluteBoneTransformsTo(transformation);

            foreach (ModelMesh mesh in model.Meshes) {
                //effects and matricies go here
                foreach (BasicEffect effect in mesh.Effects) {

                    Matrix matrix = GetWorld();
                    effect.World = mesh.ParentBone.Transform * matrix;
                    effect.View = camera.view;
                    effect.Projection = camera.projection;
                    effect.TextureEnabled = true;
                    effect.EnableDefaultLighting();
                    effect.World = transformation[mesh.ParentBone.Index] * GetWorld();

                    if (this.tintColour != null) {
                        effect.AmbientLightColor = this.tintColour;
                    }

                }
                mesh.Draw();
            }

        }

        public virtual Matrix GetWorld() {
            return world;
        }

    }
}
