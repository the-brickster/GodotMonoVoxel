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
            System.Numerics.Matrix4x4 mat1 = new System.Numerics.Matrix4x4(
                11,12,13,14,
                21,22,23,24,
                31,32,33,34,
                41,42,43,44
            );
            System.Numerics.Matrix4x4 mat2 = new System.Numerics.Matrix4x4(
                11,12,13,14,
                21,22,23,24,
                31,32,33,34,
                41,42,43,44
            );
            
            Console.WriteLine("Val: {0}",mat1.multiplyColMaj(mat2));
            volumeBase = new VoxelVolume(256, 256, 256, 32, 0.125f, @"res://assets/shaders/splatvoxel.shader", new HeightMapPopulator(colorImage.GetData(), heightImage.GetData()), cam);
            this.AddChild(volumeBase);
            
            // TestMorton3D test = new TestMorton3D();
            // test.setup();
            // test.testEncode();
            // test.testDecode();
            // Use implicit operator Span<char>(char[]).


            foreach (var item in cam.GetFrustum())
            {
                Plane p = (Plane)item;
                Console.WriteLine("Cam: {0}", p);
            }
            GetViewport().DebugDraw = Viewport.DebugDrawEnum.Wireframe;
            VisualServer.SetDebugGenerateWireframes(true);
            
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