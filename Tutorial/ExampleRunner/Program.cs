using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Reflection;
using Assimp;
using Engine.Audio;
using Engine.Core;
using Engine.DataTypes;
using Engine.Debug;
using Engine.IO;
using Engine.Rendering;
using OpenTK;

namespace ExampleRunner
{
    class StartScene : AbstractScene
    {
        protected override void InitializeScene()
        {

            Add(DebugConsoleComponent.CreateConsole());
            Matrix4 proj = Matrix4.CreatePerspectiveFieldOfView(
                MathHelper.DegreesToRadians(75f),  //Field of View Vertical
                16f / 9f, //Aspect Ratio
                0.1f, //Near Plane
                1000f); //Far Plane

            BasicCamera bc = new BasicCamera(proj, Vector3.UnitY * 7);
            
            Add(bc); //Adding the BasicCamera(That is a gameobject under the hood) to the scene to receive events
            SetCamera(bc); //Sets the Camera as the "active" camera that the scene will be rendered from.
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            if (!Directory.Exists("./examples")) Directory.CreateDirectory("./examples");
            string[] files = Directory.GetFiles("./examples", "*.dll", SearchOption.AllDirectories);
            List<Type> scenes = new List<Type>();
            Type target = typeof(AbstractScene);
            for (int i = 0; i < files.Length; i++)
            {
                try
                {
                    Assembly asm = Assembly.LoadFile(Path.GetFullPath(files[i]));
                    if(asm==Assembly.GetExecutingAssembly())continue;
                    Type[] types = asm.GetTypes();
                    bool hasScene = false;
                    for (int j = 0; j < types.Length; j++)
                    {
                        if (target != types[j] && target.IsAssignableFrom(types[j]) && !scenes.Contains(types[j]))
                        {
                            hasScene = true;
                            scenes.Add(types[j]);
                        }
                    }

                    if (hasScene)
                    {
                        ManifestReader.RegisterAssembly(asm);
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }


            GameEngine ge = new GameEngine(EngineSettings.DefaultSettings);

            ManifestReader.PrepareManifestFiles(true);
            ManifestReader.PrepareManifestFiles(false);
            ge.Initialize();
            ge.InitializeScene<StartScene>();
            ge.Run();
        }
    }
}
