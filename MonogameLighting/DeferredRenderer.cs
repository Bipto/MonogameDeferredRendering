using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace MonogameLighting
{
    class DeferredRenderer
    {
        private Effect clearBufferEffect;

        RenderTarget2D colourRT;
        RenderTarget2D normalRT;
        RenderTarget2D depthRT;
        RenderTarget2D specularRT;

        RenderTarget2D combineRT;
        RenderTarget2D skyboxRT;
        RenderTarget2D outputRT;
        RenderTarget2D positionRT;

        public RenderTarget2D ColourRT { get { return colourRT; } }
        public RenderTarget2D NormalRT { get { return normalRT; } }
        public RenderTarget2D DepthRT { get { return depthRT; } }
        public RenderTarget2D SpecularRT { get { return specularRT; } }
        public RenderTarget2D CombineRT { get { return combineRT; } }        
        public RenderTarget2D SkyboxRT { get { return skyboxRT; } }
        public RenderTarget2D OutputRT {  get { return outputRT; } }
        public RenderTarget2D PositionRT { get { return positionRT; } }

        RenderTargetBinding[] renderTargetBinding = new RenderTargetBinding[4];

        QuadRenderer quadRenderer;

        Scene scene;
        Camera camera;

        Effect combineEffect;
        Effect pointLightEffect;
        Effect directionaLightEffect;

        Vector2 halfPixel;
        Skybox skybox;

        Color ambientColor = Color.Black;

        public DeferredRenderer(GraphicsDevice device, ContentManager content, Game game)
        {
            int width = device.PresentationParameters.BackBufferWidth;
            int height = device.PresentationParameters.BackBufferHeight;

            colourRT = new RenderTarget2D(device, width, height, false, SurfaceFormat.Color, DepthFormat.Depth24);
            normalRT = new RenderTarget2D(device, width, height, false, SurfaceFormat.Color, DepthFormat.Depth24);
            depthRT = new RenderTarget2D(device, width, height, false, SurfaceFormat.Color, DepthFormat.Depth24);
            combineRT = new RenderTarget2D(device, width, height, false, SurfaceFormat.Color, DepthFormat.Depth24);
            skyboxRT = new RenderTarget2D(device, width, height);
            outputRT = new RenderTarget2D(device, width, height, false, SurfaceFormat.Color, DepthFormat.None);
            positionRT = new RenderTarget2D(device, width, height);
            specularRT = new RenderTarget2D(device, width, height);

            renderTargetBinding[0] = colourRT;
            renderTargetBinding[1] = normalRT;
            renderTargetBinding[2] = depthRT;
            renderTargetBinding[3] = positionRT;

            clearBufferEffect = content.Load<Effect>("ClearGBuffer");
            combineEffect = content.Load<Effect>("Combine");
            pointLightEffect = content.Load<Effect>("PointLight");
            directionaLightEffect = content.Load<Effect>("DirectionalLight");

            quadRenderer = new QuadRenderer();

            scene = new Scene(game);
            scene.InitializeScene();

            camera = new Camera();

            skybox = new Skybox(content);

            halfPixel.X = 0.5f / (float)device.PresentationParameters.BackBufferWidth;
            halfPixel.Y = 0.5f / (float)device.PresentationParameters.BackBufferHeight;
        }

        public void Resize(GraphicsDevice device, int width, int height)
        {
            colourRT = new RenderTarget2D(device, width, height, false, SurfaceFormat.Color, DepthFormat.Depth24);
            normalRT = new RenderTarget2D(device, width, height, false, SurfaceFormat.Color, DepthFormat.Depth24);
            depthRT = new RenderTarget2D(device, width, height, false, SurfaceFormat.Color, DepthFormat.Depth24);
            combineRT = new RenderTarget2D(device, width, height, false, SurfaceFormat.Color, DepthFormat.Depth24);
            skyboxRT = new RenderTarget2D(device, width, height);
            outputRT = new RenderTarget2D(device, width, height, false, SurfaceFormat.Color, DepthFormat.None);
            positionRT = new RenderTarget2D(device, width, height);
            specularRT = new RenderTarget2D(device, width, height);

            renderTargetBinding = new RenderTargetBinding[4];

            renderTargetBinding[0] = colourRT;
            renderTargetBinding[1] = normalRT;
            renderTargetBinding[2] = depthRT;
            renderTargetBinding[3] = positionRT;

            camera.Resize(width, height);

            halfPixel.X = 0.5f / (float)device.PresentationParameters.BackBufferWidth;
            halfPixel.Y = 0.5f / (float)device.PresentationParameters.BackBufferHeight;
        }

        public void Draw(GraphicsDevice device)
        {
            device.SetRenderTarget(skyboxRT);
            device.RasterizerState = new RasterizerState() { CullMode = CullMode.CullClockwiseFace };
            device.BlendState = BlendState.AlphaBlend;
            device.DepthStencilState = new DepthStencilState { DepthBufferEnable = false };
            skybox.Draw(camera.View, camera.Projection, camera.Position);
            device.RasterizerState = new RasterizerState() { CullMode = CullMode.None };
            device.SetRenderTarget(null);

            //device.SetRenderTarget(null);

            device.SetRenderTargets(renderTargetBinding);
            clearBufferEffect.CurrentTechnique.Passes[0].Apply();
            quadRenderer.RenderQuad(device, -Vector2.One, Vector2.One);
            scene.DrawScene(camera);


            DrawLights(device);

            device.SetRenderTarget(CombineRT);

            device.DepthStencilState = new DepthStencilState() { DepthBufferEnable = true };
            device.Clear(Color.Transparent);
            device.BlendState = BlendState.AlphaBlend;

            combineEffect.Parameters["colorMap"].SetValue(outputRT);
            //lightingEffect.Parameters["normalMap"].SetValue(normalRT);
            combineEffect.Parameters["depthMap"].SetValue(depthRT);
            combineEffect.Parameters["halfPixel"].SetValue(halfPixel);
            combineEffect.Parameters["AmbientColor"].SetValue(Color.Red.ToVector4());
            combineEffect.Parameters["AmbientIntensity"].SetValue(.4f);
            combineEffect.Parameters["WorldInverseTranspose"].SetValue(Matrix.Transpose(Matrix.Invert(camera.World)));
            combineEffect.Parameters["DiffuseColor"].SetValue(Color.Red.ToVector4());
            combineEffect.Parameters["DiffuseIntensity"].SetValue(1f);
            combineEffect.Parameters["skyboxTexture"].SetValue(skyboxRT);

            combineEffect.Techniques["Combine"].Passes[0].Apply();
            quadRenderer.RenderQuad(device, -Vector2.One, Vector2.One);

            device.SetRenderTarget(null);
        }

        private void DrawLights(GraphicsDevice device)
        {
            //device.BlendState = BlendState.Additive;

            device.BlendState = new BlendState()
            {
                ColorBlendFunction = BlendFunction.Max,
                AlphaSourceBlend = Blend.InverseSourceColor,
                ColorSourceBlend = Blend.InverseSourceColor,
                AlphaDestinationBlend = Blend.One,
                ColorDestinationBlend = Blend.One,
            };

            device.SetRenderTarget(outputRT);
            device.Clear(ambientColor);

            Vector3 lightPos = new Vector3(10f, 2f, 10f);
            float lightPower = 1f;

            Matrix lightsView = Matrix.CreateLookAt(lightPos, Vector3.Zero, Vector3.Up);
            Matrix lightsProjection = Matrix.CreatePerspectiveFieldOfView(MathHelper.PiOver2, 1f, 5f, 100f);
            Matrix lightsViewProjectionMatrix = lightsView * lightsProjection;
            DrawPointLight(device, lightPos, lightPower, lightsViewProjectionMatrix, Color.Red);

            lightPos = new Vector3(0, 5f, 10f);
            lightPower = 5f;

            lightsView = Matrix.CreateLookAt(lightPos, new Vector3(0f, 5f, 0f), Vector3.Up);
            lightsProjection = Matrix.CreatePerspectiveFieldOfView(MathHelper.PiOver2, 1f, 5f, 100f);
            lightsViewProjectionMatrix = lightsView * lightsProjection;
            //DrawLight(device, lightPos, lightPower, lightsViewProjectionMatrix, Color.White);

            Vector3 lightDirection = new Vector3(-1f, 0, 0);
            float lightIntensity = 2f;
            Color lightCol = Color.Blue;
            DrawDirectionalLight(device, lightDirection, lightIntensity, lightCol);

            device.SetRenderTarget(null);
        }

        public void DrawPointLight(GraphicsDevice device, Vector3 lightPos, float lightPower, Matrix lightsViewProjectionMatrix, Color lightCol)
        {          

            pointLightEffect.Parameters["CameraViewProjection"].SetValue(camera.View * camera.Projection);
            pointLightEffect.Parameters["World"].SetValue(camera.World);

            pointLightEffect.Parameters["LightViewProjection"].SetValue(lightsViewProjectionMatrix);
            pointLightEffect.Parameters["LightColor"].SetValue(lightCol.ToVector4());
            pointLightEffect.Parameters["LightPosition"].SetValue(lightPos);
            pointLightEffect.Parameters["LightPower"].SetValue(lightPower);            

            pointLightEffect.Parameters["CameraPosition"].SetValue(camera.Position);         
                        
            pointLightEffect.Parameters["ColorMap"].SetValue(colourRT);
            pointLightEffect.Parameters["NormalMap"].SetValue(normalRT);
            pointLightEffect.Parameters["PositionMap"].SetValue(positionRT);

            pointLightEffect.Techniques["PointLight"].Passes[0].Apply();
            quadRenderer.RenderQuad(device, -Vector2.One, Vector2.One);
        }

        public void DrawDirectionalLight(GraphicsDevice device, Vector3 lightDirection, float lightPower, Color lightCol)
        {
            directionaLightEffect.Parameters["LightColor"].SetValue(lightCol.ToVector4());
            directionaLightEffect.Parameters["LightDirection"].SetValue(lightDirection);
            directionaLightEffect.Parameters["LightPower"].SetValue(lightPower);

            directionaLightEffect.Parameters["CameraPosition"].SetValue(camera.Position);

            directionaLightEffect.Parameters["ColorMap"].SetValue(colourRT);
            directionaLightEffect.Parameters["NormalMap"].SetValue(normalRT);
            directionaLightEffect.Parameters["PositionMap"].SetValue(positionRT);

            directionaLightEffect.Techniques["DirectionalLight"].Passes[0].Apply();
            quadRenderer.RenderQuad(device, -Vector2.One, Vector2.One);
        }

        public void Update()
        {
            camera.Update();
        }
    }
}
