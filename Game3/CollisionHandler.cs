using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Game3 {
    class CollisionHandler {
        public void detectCollisions(List<BasicModel> models) {

            foreach (BasicModel modelA in models) {
                foreach (BasicModel modelB in models) {
                    if (modelA.uniqueId != modelB.uniqueId && (validModelType(modelA)) && (validModelType(modelB))) {
                        if (collidesWith(modelA, modelB)) {
                            HandleCollision(modelA, modelB);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Depending on the pair of models colliding, handle appropriately
        /// </summary>
        /// <param name="modelA"></param>
        /// <param name="modelB"></param>
        private void HandleCollision(BasicModel modelA, BasicModel modelB) {
            if (modelA.GetType() == typeof(Enemy) && modelB.GetType() == typeof(Pickup)) {

                ((Enemy)modelA).FullHealth();
                modelB.currentDrawState = BasicModel.drawState.remove;

            } else if (modelA.GetType() == typeof(Tank) && modelB.GetType() == typeof(Pickup)) {
                modelB.currentDrawState = BasicModel.drawState.remove;
            } else if (modelA.GetType() == typeof(Enemy) && modelB.GetType() == typeof(Tank)) {

                ((Enemy)modelA).KnockBackFrom((Tank)modelB);

                ((Tank)modelB).health = ((Tank)modelB).health - 25;

                if (((Tank)modelB).health <= 0) {
                    modelB.currentDrawState = BasicModel.drawState.remove;
                }

            } else if (modelA.GetType() == typeof(Tank) && modelB.GetType() == typeof(Enemy)) {
                ((Tank)modelA).KnockBackFrom((Enemy) modelB);

                ((Enemy)modelB).health = ((Enemy)modelB).health - 25;

                if (((Enemy)modelB).health <= 0) {
                    modelB.currentDrawState = BasicModel.drawState.remove;
                }

            }
}

        public bool collidesWith (BasicModel modelA, BasicModel modelB) {
            // get the position of each model
            Matrix modelATranslation = modelA.GetWorld();
            Matrix modelBTranslation = modelB.GetWorld();

            // check each bounding sphere of each model
            foreach (ModelMesh modelMeshesA in modelA.model.Meshes) {
                foreach (ModelMesh modelMeshesB in modelB.model.Meshes) {
                    // update bounding spheres of each models mesh
                    BoundingSphere BSA = modelMeshesA.BoundingSphere.Transform(modelATranslation);
                    BoundingSphere BSB = modelMeshesB.BoundingSphere.Transform(modelBTranslation);

                    // check if any the mesh bounding sphere intersects with another
                    if (BSA.Intersects(BSB)) {
                        return true;
                    }
                }
            }
            return false;
        }

        private bool validModelType(BasicModel model) {
            return model.GetType() != typeof(Ground); // ignore collisions with the ground
        }

    }
}
