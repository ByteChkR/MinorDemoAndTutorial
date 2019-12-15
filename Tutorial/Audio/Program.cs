using System;
using System.Drawing;
using Engine.Audio;
using Engine.Core;
using Engine.DataTypes;
using Engine.Debug;
using Engine.IO;
using Engine.Rendering;
using OpenTK;

namespace Audio
{
    class AudioScene : AbstractScene
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
            bc.Rotate(Vector3.UnitX, MathHelper.DegreesToRadians(-90));
            Add(bc); //Adding the BasicCamera(That is a gameobject under the hood) to the scene to receive events
            SetCamera(bc); //Sets the Camera as the "active" camera that the scene will be rendered from.
            bc.AddComponent(new AudioListener());
            GameObject boxContainer = new GameObject("Container"); //Empty Container at origin
            GameObject box = new GameObject(-Vector3.UnitZ * 6, "Box"); //Creating a new Empty GameObject
            LitMeshRendererComponent lmr = new LitMeshRendererComponent( //Creating a Renderer Component
                DefaultFilepaths.DefaultLitShader, //The OpenGL Shader used(Unlit and Lit shaders are provided)
                Prefabs.Cube, //The Mesh that is going to be used by the MeshRenderer
                TextureLoader.ColorToTexture(Color.Red), //Diffuse Texture to put on the mesh
                1); //Render Mask (UI = 1 << 30)
            box.AddComponent(lmr); //Attaching the Renderer to the GameObject

            AudioSourceComponent asc = new AudioSourceComponent();
            AudioLoader.TryLoad("assets/sound.wav", out AudioFile file);
            asc.Clip = file;
            asc.Looping = true;
            asc.Play();
            asc.UpdatePosition = true; //Enable 3D Tracking the Gameobjects movements and apply it to the audio source
            asc.Gain = 0.6f;

            box.AddComponent(asc);
            boxContainer.AddComponent(new RotatingComponent());
            boxContainer.Add(box); //Adding the Object to the Scene.
            Add(boxContainer);
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

            ManifestReader.PrepareManifestFiles(true);
            ManifestReader.PrepareManifestFiles(false);
            ge.Initialize();
            ge.InitializeScene<AudioScene>();
            ge.Run();
        }
    }
}
