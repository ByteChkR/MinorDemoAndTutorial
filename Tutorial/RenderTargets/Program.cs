using System;
using System.Drawing;
using Engine.Core;
using Engine.DataTypes;
using Engine.Debug;
using Engine.IO;
using Engine.Physics;
using Engine.Physics.BEPUphysics.Entities;
using Engine.Physics.BEPUphysics.Entities.Prefabs;
using Engine.Physics.BEPUphysics.Materials;
using Engine.Rendering;
using OpenTK;

namespace RenderTargets
{
    
    class RenderTargetsScene : AbstractScene
    {
        private RenderTarget splitCam;
        protected override void InitializeScene()
        {

            Add(DebugConsoleComponent.CreateConsole());
            BasicCamera inPicCam =
                new BasicCamera(
                    Matrix4.CreatePerspectiveFieldOfView(MathHelper.DegreesToRadians(75f),
                        GameEngine.Instance.Width / (float)GameEngine.Instance.Height, 0.01f, 1000f), Vector3.Zero);
            
            inPicCam.Rotate(Vector3.UnitX, MathHelper.DegreesToRadians(-90f));
            inPicCam.Translate(Vector3.UnitY*25);

            Add(inPicCam);

            splitCam = new RenderTarget(inPicCam, 1, Color.FromArgb(0, 0, 0, 0))
            {
                MergeType = RenderTargetMergeType.Additive,
                ViewPort = new Rectangle(0, 0, (int)(GameEngine.Instance.Width * 0.3f),
                    (int)(GameEngine.Instance.Height * 0.3f))
            };

            GameEngine.Instance.AddRenderTarget(splitCam);

            BasicCamera bc =
                new BasicCamera(Matrix4.CreatePerspectiveFieldOfView(MathHelper.DegreesToRadians(75), 16 / 9f, 0.1f, 1000f),
                    new Vector3(0, 5, 30)); //Creating a Basic Camera
            SetCamera(bc);
            Add(bc);


            //Creating a Box that is meant to fall down on the kinetic box
            GameObject box = new GameObject(OpenTK.Vector3.UnitZ * -3 + Vector3.UnitY * 2, "Box");
            LitMeshRendererComponent lmr = new LitMeshRendererComponent(DefaultFilepaths.DefaultLitShader, Prefabs.Cube,
                TextureLoader.ColorToTexture(Color.Red), 1);
            box.AddComponent(lmr);
            Add(box);

            //Creating a Kinetic Box that will rotate slowly
            GameObject kinetic = new GameObject(OpenTK.Vector3.UnitZ * -3 + Vector3.UnitY * -2, "Box");
            LitMeshRendererComponent kineticlmr = new LitMeshRendererComponent(DefaultFilepaths.DefaultLitShader, Prefabs.Cube,
                TextureLoader.ColorToTexture(Color.Green), 1);
            kinetic.AddComponent(kineticlmr);
            kinetic.AddComponent(new RotatingComponent());
            Add(kinetic);

            //A Large sphere that will act as a ground
            GameObject ground = new GameObject(OpenTK.Vector3.UnitZ * -3 + Vector3.UnitY * -1, "Box");
            LitMeshRendererComponent groundlmr = new LitMeshRendererComponent(DefaultFilepaths.DefaultLitShader, Prefabs.Sphere,
                TextureLoader.ColorToTexture(Color.Blue), 1);
            ground.AddComponent(groundlmr);
            Add(ground);
            ground.Scale = new Vector3(20, 20, 20);
            ground.LocalPosition = Engine.Physics.BEPUutilities.Vector3.UnitY * -25;

            //Creating the Collider Shapes
            Entity boxShape = new Box(
                Engine.Physics.BEPUutilities.Vector3.Zero,
                2f,
                2f,
                2f,
                1f);

            Entity kineticShape = new Box(
                Engine.Physics.BEPUutilities.Vector3.Zero,
                2f,
                2f,
                2f,
                1f);

            Entity groundShape = new Sphere(
                Vector3.Zero,
                20f);
            //Note: Not specifying the mass when creating makes the shape a static shape that is really cheap computatinally

            //Ground(Sphere) and the falling box is going to have 0 friction and maximum bounciness.
            Material groundPhysicsMaterial = new Material(0, 0, 1f);
            Material boxPhysicsMaterial = new Material(0, 0, 1f);

            //Creating A physics layer to be able to control which objects are meant to collide with each other
            int physicsLayerID = LayerManager.RegisterLayer("physics", new Layer(1, 1));

            //Creating the Components for the Physics Engine
            //Note: There are different ways to get the LayerID than storing it.
            Collider boxCollider = new Collider(boxShape, physicsLayerID);
            Collider groundCollider = new Collider(groundShape, "physics");
            Collider kineticCollider = new Collider(kineticShape, LayerManager.LayerToName(physicsLayerID));

            //Final Collider Setup
            //Kinetic becomes Kinetic
            kineticCollider.PhysicsCollider.BecomeKinematic();
            //Adding the Physics Materials
            boxCollider.PhysicsCollider.Material = boxPhysicsMaterial;
            groundCollider.PhysicsCollider.Material = groundPhysicsMaterial;

            //Adding the Components
            box.AddComponent(boxCollider);
            kinetic.AddComponent(kineticCollider);
            ground.AddComponent(groundCollider);

            //Making the Camera LookAt the origin
            bc.LookAt(Vector3.Zero);
        }
    }

    class RotatingComponent : AbstractComponent
    {
        Collider c;
        protected override void Awake()
        {
            c = Owner.GetComponent<Collider>();
        }

        protected override void Update(float deltaTime)
        {
            //Rotating the Gameobject
            //Note: Since the collider is kinematic it has infinite mass
            //      So no force can be applied to move it.
            //      As a workaround we move the two systems manually(EngineRendering and EnginePhysics)
            Engine.Physics.BEPUutilities.Quaternion v = Quaternion.FromAxisAngle(new Vector3(1, 1, 1), deltaTime);
            c.PhysicsCollider.Orientation *= v;
            Owner.Rotate(new OpenTK.Vector3(1, 1, 1), deltaTime);
        }
    }


    class Program
    {
        static void Main(string[] args)
        {
            GameEngine ge = new GameEngine(EngineSettings.DefaultSettings);
            ge.Initialize();
            ge.InitializeScene<RenderTargetsScene>();
            ge.Run();
        }
    }
}
