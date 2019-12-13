
using System.Diagnostics;
using System.Reflection;
using System.Threading;
using Engine.Core;
using Engine.DataTypes;
using Engine.Debug;
using Engine.IO;
using Engine.OpenCL;
using Engine.OpenCL.DotNetCore.Memory;
using Engine.OpenCL.TypeEnums;
using Engine.OpenFL;
using Engine.Rendering;
using OpenTK;

namespace CL_Begin
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


            int imageSize = 512 * 512 * 4;
            //Creating a Kernel Database that will load all the Kernels contained in the asset directory
            KernelDatabase db = new KernelDatabase(Clapi.MainThread, "assets/test_kernel/", DataTypes.Uchar1);
            db.TryGetClKernel("kernel_red", out CLKernel redKernel);

            MemoryBuffer imageBuffer = Clapi.CreateEmpty<byte>(Clapi.MainThread, imageSize, MemoryFlag.ReadWrite);
            
            redKernel.SetBuffer("imageData", imageBuffer);
            redKernel.SetArg("strength", 0.5f);
            redKernel.SetArg("channelCount", 4);

            Clapi.Run(Clapi.MainThread, redKernel, imageSize);

            Texture tex = TextureLoader.BytesToTexture(Clapi.ReadBuffer<byte>(Clapi.MainThread, imageBuffer, imageSize), 512, 512);



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

