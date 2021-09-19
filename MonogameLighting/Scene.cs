using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace MonogameLighting
{
    class Scene
    {
        private Game game;

        private Model model;
        private Texture2D modelDiffuseTexture;
        private Texture2D modelNormalTexture;
        private Texture2D modelSpecularTexture;

        private Effect gBufferEffect;

        private Model platformModel;
        private Texture2D platformDiffuseTexture;
        private Texture2D platformNormalTexture;

        private TextureCube skyboxTexture;

        public Scene(Game game)
        {
            this.game = game;
        }

        public void InitializeScene()
        {
            model = game.Content.Load<Model>("theboss/theboss");
            modelDiffuseTexture = game.Content.Load<Texture2D>("theboss/textures/Boss_diffuse");
            modelNormalTexture = game.Content.Load<Texture2D>("theboss/textures/Boss_normal");
            //modelSpecularTexture = game.Content.Load<Texture2D>("mutant/textures/Mutant_specular");

            gBufferEffect = game.Content.Load<Effect>("RenderGBuffer");

            platformModel = game.Content.Load<Model>("platform/platform");
            platformDiffuseTexture = game.Content.Load<Texture2D>("platform/platformTexture");
            platformNormalTexture = game.Content.Load<Texture2D>("platform/platformNormal");
        }

        public void DrawScene(Camera camera)
        {
            game.GraphicsDevice.DepthStencilState = new DepthStencilState()
            {
                DepthBufferEnable = true,
                DepthBufferFunction = CompareFunction.Less,
            };

            //game.GraphicsDevice.RasterizerState = RasterizerState.CullCounterClockwise;

            game.GraphicsDevice.RasterizerState = new RasterizerState()
            {
                CullMode = CullMode.CullCounterClockwiseFace,
            };

            game.GraphicsDevice.BlendState = BlendState.Opaque;

            //DrawModel(platformModel, Matrix.CreateTranslation(0f, -0.24f, 0f) * Matrix.CreateScale(100f) * camera.World, camera.View, camera.Projection, platformDiffuseTexture, platformNormalTexture, camera.Position);
            DrawModel(platformModel, Matrix.CreateScale(10f, 10f, 10f) * camera.World, camera.View, camera.Projection, platformDiffuseTexture, platformNormalTexture, camera.Position);
            DrawModel(model, camera.World, camera.View, camera.Projection, modelDiffuseTexture, modelNormalTexture, camera.Position);
        }

        private void DrawModel(Model model, Matrix world, Matrix view, Matrix projection, Texture2D texture, Texture2D normalMap, Vector3 cameraPosition)
        {
            gBufferEffect.Parameters["World"].SetValue(world);
            gBufferEffect.Parameters["View"].SetValue(view);
            gBufferEffect.Parameters["Projection"].SetValue(projection);
            gBufferEffect.Parameters["Texture"].SetValue(texture);
            gBufferEffect.Parameters["NormalMap"].SetValue(normalMap);
            gBufferEffect.Parameters["CameraPosition"].SetValue(cameraPosition);

            gBufferEffect.CurrentTechnique.Passes[0].Apply();

            foreach (ModelMesh mesh in model.Meshes)
            {
                foreach (ModelMeshPart meshPart in mesh.MeshParts)
                {
                    game.GraphicsDevice.SetVertexBuffer(meshPart.VertexBuffer);
                    game.GraphicsDevice.Indices = meshPart.IndexBuffer;
                    game.GraphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, meshPart.StartIndex, 0, meshPart.PrimitiveCount);
                }
            }
        }
    }
}
