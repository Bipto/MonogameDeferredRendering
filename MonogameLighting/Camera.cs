using Microsoft.Xna.Framework;
using System;

namespace MonogameLighting
{
    class Camera
    {
        Matrix world = Matrix.CreateScale(0.02f) * Matrix.CreateTranslation(0, -2, 0);
        Matrix view = Matrix.CreateLookAt(new Vector3(0, 0, 10), new Vector3(0, 0, 0), new Vector3(0, 1, 0));
        Matrix projection = Matrix.CreatePerspectiveFieldOfView(MathHelper.ToRadians(45f), 800f / 600f, 0.1f, 1000f);
        Vector3 position = new Vector3(0, -2, 0);

        float angle = 0;
        float distance = 10;

        public Matrix World { get { return world; } }
        public Matrix View { get { return view; } }
        public Matrix Projection { get { return projection; } }
        public Vector3 Position { get { return position; } }

        Vector3 viewVector;

        public void Resize(int width, int height)
        {
            projection = Matrix.CreatePerspectiveFieldOfView(MathHelper.ToRadians(45f), width / height, 0.1f, 100f);
        }

        public void Update()
        {
            angle += 0.01f;
            Vector3 cameraLocation = distance * new Vector3((float)Math.Sin(angle), 0, (float)Math.Cos(angle));
            Vector3 cameraTarget = new Vector3(0, 0, 0);
            viewVector = Vector3.Transform(cameraTarget - cameraLocation, Matrix.CreateRotationY(0));
            viewVector.Normalize();
            view = Matrix.CreateLookAt(cameraLocation, cameraTarget, new Vector3(0, 1, 0));
        }

        public void Scale(float scale)
        {
            world = Matrix.CreateScale(scale) * Matrix.CreateTranslation(0, -2, 0);
        }
    }    
}
