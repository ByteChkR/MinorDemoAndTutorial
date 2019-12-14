using System;
using System.Collections.Generic;
using System.Reflection;
using Engine.Core;
using Engine.DataTypes;
using Engine.Debug;
using Engine.IO;
using Engine.OpenCL;
using Engine.OpenCL.DotNetCore.Memory;
using Engine.OpenCL.TypeEnums;
using Engine.OpenFL;
using Engine.OpenFL.Runner;
using Engine.Rendering;
using OpenTK;
using OpenTK.Input;

namespace OpenFL_Runner
{
    class Scene : AbstractScene
    {

        protected override void InitializeScene()
        {
            Texture tex = TextureLoader.ParameterToTexture(128, 128); //Generating 2 Textures for our 2 boxes
            Texture tex2 = TextureLoader.ParameterToTexture(128, 128);

            //Adding Component that will do the FLRunner tasks
            AddComponent(new KeyTriggerComponent(tex, tex2));

            Matrix4 proj = Matrix4.CreatePerspectiveFieldOfView(
                MathHelper.DegreesToRadians(75f),  //Field of View Vertical
                16f / 9f, //Aspect Ratio
                0.1f, //Near Plane
                1000f); //Far Plane

            BasicCamera bc = new BasicCamera(proj, Vector3.Zero);
            Add(bc); //Adding the BasicCamera(That is a gameobject under the hood) to the scene to receive events
            SetCamera(bc); //Sets the Camera as the "active" camera that the scene will be rendered from.

            GameObject box = new GameObject(-Vector3.UnitZ * 4 + Vector3.UnitX, "Box"); //Creating a new Empty GameObject
            LitMeshRendererComponent lmr = new LitMeshRendererComponent( //Creating a Renderer Component
                DefaultFilepaths.DefaultLitShader, //The OpenGL Shader used(Unlit and Lit shaders are provided)
                Prefabs.Cube, //The Mesh that is going to be used by the MeshRenderer
                tex, //Diffuse Texture to put on the mesh
                1); //Render Mask (UI = 1 << 30)
            box.AddComponent(lmr); //Attaching the Renderer to the GameObject
            box.AddComponent(new RotatingComponent()); //Adding a component that rotates the Object on the Y-Axis
            Add(box); //Adding the Object to the Scene.

            GameObject box2 = new GameObject(-Vector3.UnitZ * 4 - Vector3.UnitX, "Box2"); //Creating a new Empty GameObject
            LitMeshRendererComponent lmr2 = new LitMeshRendererComponent( //Creating a Renderer Component
                DefaultFilepaths.DefaultLitShader, //The OpenGL Shader used(Unlit and Lit shaders are provided)
                Prefabs.Cube, //The Mesh that is going to be used by the MeshRenderer
                tex2, //Diffuse Texture to put on the mesh
                1); //Render Mask (UI = 1 << 30)
            box2.AddComponent(lmr2); //Attaching the Renderer to the GameObject
            box2.AddComponent(new RotatingComponent()); //Adding a component that rotates the Object on the Y-Axis
            Add(box2); //Adding the Object to the Scene.
        }
    }

    public class KeyTriggerComponent : AbstractComponent
    {
        //Our 2 Textures
        private Texture _tex;
        private Texture _tex2;

        private bool red = true;

        FlRunner flRunner = new FlRunner(Clapi.MainThread);
        public KeyTriggerComponent(Texture tex, Texture tex2)
        {
            _tex = tex;
            _tex2 = tex2;
        }

        protected override void OnKeyDown(object sender, KeyboardKeyEventArgs e)
        {
            if (e.Key == Key.Space) // Space enqueues a FLExecutionContext for each texture
            {
                //Creating a Texture Map that maps the buffers of the Interpreter to the Textures
                //"result" is always the result buffer of the interpreter
                //but if we wanted, we can define a buffer in the FL Script and map this buffer to any texture by its name.
                //Thats for example a cheap way to do specular textures alongside the actual textures.
                Dictionary<string, Texture> texMap = new Dictionary<string, Texture>
                {
                    //In This example we could also use "in" as a key,
                    //but this can be wrong at times when the fl execution context starts with a different input texture
                    //than output texture
                    {"result", _tex }
                };
                Dictionary<string, Texture> texMap2 = new Dictionary<string, Texture>
                {
                    {"result", _tex2 }
                };

                //We change the color every enqueue, to be able to see the change
                string path = red ? "assets/filter/red.fl" : "assets/filter/blue.fl";
                red = !red;

                //Creating the Execution Context
                FlExecutionContext fle = new FlExecutionContext(path, _tex, texMap, null);
                FlExecutionContext fle2 = new FlExecutionContext(path, _tex2, texMap2, null);

                //Enqueuing the Contexts
                flRunner.Enqueue(fle);
                flRunner.Enqueue(fle2);
                

                Logger.Log("Enqueued 2 Items. Items In Queue: " + flRunner.ItemsInQueue, DebugChannel.Log | DebugChannel.GameOpenFL, 10);
            }

            if (e.Key == Key.Enter && flRunner.ItemsInQueue != 0) //When we press enter we will process our queue.
            {
                flRunner.Process(
                    () => Logger.Log("Finished Processing", DebugChannel.Log | DebugChannel.GameOpenFL, 10));
            }
            base.OnKeyDown(sender, e);
        }
    }

    public class RotatingComponent : AbstractComponent
    {
        protected override void Update(float deltaTime)
        {
            Owner.Rotate(Vector3.UnitY, MathHelper.DegreesToRadians(45f) * deltaTime);
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            GameEngine ge = new GameEngine(EngineSettings.DefaultSettings);

            ManifestReader.RegisterAssembly(Assembly.GetExecutingAssembly()); //Register this assembly(where the files will be embedded in)
            ManifestReader.PrepareManifestFiles(false); //First Read Assembly files
            ManifestReader.PrepareManifestFiles(true); //Replace Any Loaded assembly files with files on the file system.

            ge.Initialize();
            ge.InitializeScene<Scene>();
            ge.Run();
        }
    }
}
