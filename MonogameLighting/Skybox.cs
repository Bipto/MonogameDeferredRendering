using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Text;

namespace MonogameLighting
{
    class Skybox
    {
        private Model skyboxModel;
        private TextureCube skyboxTexture;
        private Effect skyboxEffect;
        private float size = 50f;

        public Skybox(ContentManager content)
        {
            skyboxModel = content.Load<Model>("Skybox/cube");
            skyboxTexture = content.Load<TextureCube>("Skybox/SkyBox");
            skyboxEffect = content.Load<Effect>("Skybox");
        }

        public void Draw(Matrix view, Matrix projection, Vector3 cameraPosition)
        {
            foreach (EffectPass pass in skyboxEffect.CurrentTechnique.Passes)
            {
                foreach (ModelMesh mesh in skyboxModel.Meshes)
                {
                    foreach (ModelMeshPart part in mesh.MeshParts)
                    {
                        part.Effect = skyboxEffect;
                        part.Effect.Parameters["World"].SetValue(Matrix.CreateScale(size) * Matrix.CreateTranslation(cameraPosition));
                        part.Effect.Parameters["View"].SetValue(view);
                        part.Effect.Parameters["Projection"].SetValue(projection);
                        part.Effect.Parameters["SkyBoxTexture"].SetValue(skyboxTexture);
                        part.Effect.Parameters["CameraPosition"].SetValue(cameraPosition);
                    }

                    mesh.Draw();
                }
            }
        }
    }
}
