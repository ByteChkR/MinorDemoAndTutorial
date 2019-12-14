using System;
using System.Diagnostics;
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

namespace OpenFL_Begin
{
    class Scene : AbstractScene
    {

        protected override void InitializeScene()
        {
            Matrix4 proj = Matrix4.CreatePerspectiveFieldOfView(
                MathHelper.DegreesToRadians(75f),  //Field of View Vertical
                16f / 9f, //Aspect Ratio
                0.1f, //Near Plane
                1000f); //Far Plane

            BasicCamera bc = new BasicCamera(proj, Vector3.Zero);
            Add(bc); //Adding the BasicCamera(That is a gameobject under the hood) to the scene to receive events
            SetCamera(bc); //Sets the Camera as the "active" camera that the scene will be rendered from.



            
            int texWidth = 128; //Width of the input texture
            int texHeight = 128; //Height of the input texture
            MemoryBuffer buffer = //Creating the Input Buffer
                Clapi.CreateEmpty<byte>(
                    Clapi.MainThread, //We use the Main thread instance
                    texWidth * texHeight * 4, //The image size in bytes
                    MemoryFlag.ReadWrite); //We want to read and write to the texture

            Interpreter i = new Interpreter( //Creating an interpreter instance
                Clapi.MainThread, //We use the main thread
                "assets/filter/red.fl", //The file to execute
                DataTypes.Uchar1, //The Data type of our input buffer
                buffer, //The buffer
                texWidth, //Width
                texHeight, //Height
                1, //Depth, for images always 1
                4, //Channel count(BGRA)
                "assets/kernel/", //Directory for all kernels
                true); //the "brk" statement is ignored.

            do
            {
                i.Step(); //Step through the Instructions one by one until the script terminated.
            } while (!i.Terminated);

            
            //Create a texture from the output.
            Texture tex = TextureLoader.BytesToTexture(i.GetResult<byte>(), texWidth, texHeight);


            GameObject box = new GameObject(-Vector3.UnitZ * 4, "Box"); //Creating a new Empty GameObject
            LitMeshRendererComponent lmr = new LitMeshRendererComponent( //Creating a Renderer Component
                DefaultFilepaths.DefaultLitShader, //The OpenGL Shader used(Unlit and Lit shaders are provided)
                Prefabs.Cube, //The Mesh that is going to be used by the MeshRenderer
                tex, //Diffuse Texture to put on the mesh
                1); //Render Mask (UI = 1 << 30)
            box.AddComponent(lmr); //Attaching the Renderer to the GameObject
            box.AddComponent(new RotatingComponent()); //Adding a component that rotates the Object on the Y-Axis
            Add(box); //Adding the Object to the Scene.




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
