using System;
using System.Collections.Generic;
using System.Drawing;
using Engine.Core;
using Engine.DataTypes;
using Engine.IO;
using Engine.Physics;
using Engine.Physics.BEPUphysics.Entities;
using Engine.Physics.BEPUphysics.Entities.Prefabs;
using Engine.Physics.BEPUphysics.Materials;
using Engine.Physics.BEPUutilities;
using Engine.Rendering;
using OpenTK;
using MathHelper = OpenTK.MathHelper;
using Vector2 = OpenTK.Vector2;
using Vector3 = OpenTK.Vector3;

namespace Raycasting
{

    class Scene : AbstractScene
    {
        protected override void InitializeScene()
        {
            BasicCamera bc =
                new BasicCamera(Matrix4.CreatePerspectiveFieldOfView(MathHelper.DegreesToRadians(75), 16 / 9f, 0.1f, 1000f),
                    new Vector3(0, 5, 7)); //Creating a Basic Camera
            SetCamera(bc);
            Add(bc);
            bc.AddComponent(new MouseTracker());
            
            GameObject box = new GameObject(OpenTK.Vector3.UnitX * 3, "Box");
            LitMeshRendererComponent boxlmr = new LitMeshRendererComponent(DefaultFilepaths.DefaultLitShader, Prefabs.Cube,
                TextureLoader.ColorToTexture(Color.Red), 1);
            box.AddComponent(boxlmr);
            Add(box);
            
            GameObject box2 = new GameObject(OpenTK.Vector3.UnitX * -3, "Box");
            LitMeshRendererComponent box2lmr = new LitMeshRendererComponent(DefaultFilepaths.DefaultLitShader, Prefabs.Cube,
                TextureLoader.ColorToTexture(Color.Red), 1);
            box2.AddComponent(box2lmr);
            Add(box2);

            //Creating the Collider Shapes
            Entity boxShape = new Box(
                Engine.Physics.BEPUutilities.Vector3.Zero,
                2f,
                2f,
                2f);

            Entity box2Shape = new Box(
                Engine.Physics.BEPUutilities.Vector3.Zero,
                2f,
                2f,
                2f);


            //Creating A physics layer to be able to control which objects are meant to collide with each other
            int raycastLayerID = LayerManager.RegisterLayer("raycast", new Layer(1, 1));

            //Creating the Components for the Physics Engine
            //Note: There are different ways to get the LayerID than storing it.
            Collider boxCollider = new Collider(boxShape, raycastLayerID);
            Collider box2Collider = new Collider(box2Shape, LayerManager.LayerToName(raycastLayerID));



            //Adding the Components
            box.AddComponent(boxCollider);
            box2.AddComponent(box2Collider);
            //Making the Camera LookAt the origin
            bc.LookAt(Vector3.Zero);
        }
    }


    class MouseTracker : AbstractComponent
    {
        private LitMeshRendererComponent Last = null;
        private Texture LastTex = null;
        private Texture HitTex = TextureLoader.ColorToTexture(Color.Green);
        protected override void Awake()
        {

        }

        protected override void Update(float deltaTime)
        {
            if (ObjectUnderMouse(Owner.LocalPosition, out var hit))
            {
                LitMeshRendererComponent lmr = hit.Key.Owner.GetComponent<LitMeshRendererComponent>();
                if (Last == null)
                {
                    Last = lmr;
                    LastTex = GetTexture(lmr);

                    ApplyTexture(lmr, HitTex);
                }
                else if (lmr != Last)
                {
                    ApplyTexture(Last, LastTex);

                    Last = lmr;
                    LastTex = GetTexture(lmr);

                    ApplyTexture(lmr, HitTex);

                }
            }
            else if (Last != null)
            {
                ApplyTexture(Last, LastTex);
                Last = null;
                LastTex = null;
            }
        }
        private Texture GetTexture(LitMeshRendererComponent lmr)
        {
            for (int i = 0; i < lmr.Textures.Length; i++)
            {
                if (lmr.Textures[i].TexType == TextureType.Diffuse || lmr.Textures[i].TexType == TextureType.None)
                {
                    return lmr.Textures[i];
                }
            }
            return null;
        }

        private void ApplyTexture(LitMeshRendererComponent lmr, Texture tex)
        {
            for (int i = 0; i < lmr.Textures.Length; i++)
            {
                if (lmr.Textures[i].TexType == TextureType.Diffuse || lmr.Textures[i].TexType == TextureType.None)
                {
                    lmr.Textures[i] = tex;
                    return;
                }
            }
        }

        public static bool ObjectUnderMouse(Vector3 cameraPosition, out KeyValuePair<Collider, RayHit> hit)
        {
            Ray r = ConstructRayFromMousePosition(cameraPosition);
            bool ret = PhysicsEngine.RayCastFirst(r, 1000, LayerManager.NameToLayer("raycast"), out hit); //Here we are doing the raycast.

            return ret;
        }

        private static Ray ConstructRayFromMousePosition(Vector3 localPosition)
        {
            Vector2 mpos = GameEngine.Instance.MousePosition;
            Vector3 mousepos = GameEngine.Instance.ConvertScreenToWorldCoords((int)mpos.X, (int)mpos.Y);
            return new Ray(localPosition, (mousepos - localPosition).Normalized());
        }
    }


    class Program
    {
        static void Main(string[] args)
        {
            GameEngine ge = new GameEngine(EngineSettings.DefaultSettings);
            ge.Initialize();
            ge.InitializeScene<Scene>();
            ge.Run();
        }
    }
}
