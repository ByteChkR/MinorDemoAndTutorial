
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

    class CLBeginScene : AbstractScene
    {
        
        protected override void InitializeScene()
        {
            Add(DebugConsoleComponent.CreateConsole());
            Matrix4 proj = Matrix4.CreatePerspectiveFieldOfView(
                MathHelper.DegreesToRadians(75f),  //Field of View Vertical
                16f / 9f, //Aspect Ratio
                0.1f, //Near Plane
                1000f); //Far Plane

            BasicCamera bc = new BasicCamera(proj, Vector3.Zero);
            Add(bc); //Adding the BasicCamera(That is a gameobject under the hood) to the scene to receive events
            SetCamera(bc); //Sets the Camera as the "active" camera that the scene will be rendered from.


            //Image size in bytes(Width * Height * ChannelCount)
            int imageSize = 512 * 512 * 4;

            //Creating a Kernel Database that will load all the Kernels contained in the asset directory
            KernelDatabase db = new KernelDatabase(Clapi.MainThread, "assets/test_kernel/", DataTypes.Uchar1);

            //We try to get the kernel_red from the file assets/test_kernel/red.cl
            db.TryGetClKernel("kernel_red", out CLKernel redKernel);

            //Creating a MemoryBuffer with size of the image.
            //We are using the CLAPI instance of the main thread and specify that we`d like to read/write from the buffer
            MemoryBuffer imageBuffer = Clapi.CreateEmpty<byte>(Clapi.MainThread, imageSize, MemoryFlag.ReadWrite);

            //With plain OpenCL you would need to Set the Arguments/Buffers by their argument index/types/size/yada yads,
            //thanks to the CL abstraction for the engine, we can just specify the argument name how we do in OpenGL Shaders(But Faster).
            redKernel.SetBuffer("imageData", imageBuffer);

            //Set Arg has the capabilities to automatically cast the value that is passed to the right type,
            //however this is not really fast and can be avoided by specifying the correct type directly.
            redKernel.SetArg("strength", 0.5f); //We directly pass a float, no casting required

            //When We pass something as byte(uchar in cl), we need to cast it.
            //If we dont the Engine OpenCL Wrapper will automatically convert the integer into a byte, but it will apply rescaling
            //  This takes over automatic type conversion from float(opengl) to byte(System.Bitmap/opencl)
            //  Calculation when not passed: (4 / Int32.MaxSize) * byte.MaxValue.
            redKernel.SetArg("channelCount", (byte)4); 

            //This Line runs the kernel.
            Clapi.Run(Clapi.MainThread, redKernel, imageSize);

            //After the kernel ran, we can read the buffer we have passed to the kernel and Convert it into a OpenGL Texture.
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
            ge.InitializeScene<CLBeginScene>();
            ge.Run();
        }
    }

}

