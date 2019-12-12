using Godot;
using Goxlap.src.Goxlap.utils;
using System;

namespace Goxlap.src.Goxlap
{
    using utils;
    public class TestLevel : Spatial
    {
        // Declare member variables here. Examples:
        // private int a = 2;
        // private string b = "text";

        // Called when the node enters the scene tree for the first time.
        [Export]
        Texture colorImage;
        [Export]
        Texture heightImage;
        VoxelVolume volumeBase;
        Camera cam;

        MeshInstance testCube;

        public override void _Ready()
        {
            cam = GetNode("FreeCam") as Camera;
            testCube = GetNode("MeshInstance") as MeshInstance;
            // var colorImage = (Texture)GD.Load(@"res://assets/textures/test_map_texture/C2W.png");
            colorImage.SetFlags(16);
            // var heightImage = (Texture)GD.Load(@"res://assets/textures/test_map_texture/D2.png");
            Console.WriteLine("----------------------------------This is a test {0} ---", heightImage.GetData().GetData().Length);
            volumeBase = new VoxelVolume(64, 64, 64, 32, 0.125f, @"res://assets/shaders/splatvoxel.shader", new HeightMapPopulator(colorImage.GetData(), heightImage.GetData()), cam);
            this.AddChild(volumeBase);

            // TestMorton3D test = new TestMorton3D();
            // test.setup();
            // test.testEncode();
            // test.testDecode();
            // Use implicit operator Span<char>(char[]).

            
            foreach (var item in cam.GetFrustum())
            {
                Plane p = (Plane)item;
                Console.WriteLine("Plane: {0}", p);
            }
            GetViewport().DebugDraw = Viewport.DebugDrawEnum.Wireframe;
            VisualServer.SetDebugGenerateWireframes(true);
            bool enabled = System.Numerics.Vector.IsHardwareAccelerated;
            Console.WriteLine("Is hardware enabled {0}", enabled);

            BoundingRect rect1 = new BoundingRect(5,5,50,50);
            BoundingRect rect2 = new BoundingRect(20,10,10,10);
            Console.WriteLine("Testing 2D AABB collision, Intersects:  {0}",BoundingRect.IntersectsRect(rect1,rect2));
            
        }
        
        //  // Called every frame. 'delta' is the elapsed time since the previous frame.
        // public override void _Process(float delta)
        // {
        //     if (testCube != null)
        //     {
        //         Godot.AABB boundingBox = testCube.GetAabb();
        //         Godot.Collections.Array frustum = cam.GetFrustum();
        //         int value = VoxelVolume.boxInFrustum(frustum, boundingBox);
        //         Console.WriteLine("AABB of cube, end: {0}, position: {1}, size: {2} , in FRUSTUM {3}, is visible {4}", boundingBox.End, boundingBox.Position, boundingBox.Size, value
        //         ,testCube.Visible);
        //     }
        // }
    }

}