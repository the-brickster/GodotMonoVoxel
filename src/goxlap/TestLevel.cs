using Godot;
using Goxlap.src.Goxlap.utils;
using System;

namespace Goxlap.src.Goxlap
{
    using global::Goxlap.src.Goxlap.rasterizer;
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

        MeshInstance cubeBBOX;

        PND3d pndCreator;
        public override void _Ready()
        {

            cam = GetNode("FreeCam") as Camera;
            cubeBBOX = GetNode("cubeBBOXTest") as MeshInstance;
            // var colorImage = (Texture)GD.Load(@"res://assets/textures/test_map_texture/C2W.png");
            colorImage.SetFlags(16);
            // var heightImage = (Texture)GD.Load(@"res://assets/textures/test_map_texture/D2.png");
            Console.WriteLine("----------------------------------This is a test {0} ---", heightImage.GetData().GetData().Length);
            volumeBase = new VoxelVolume(64, 64, 64, 32, 0.125f, @"res://assets/shaders/splatvoxel.shader", new HeightMapPopulator(colorImage.GetData(), heightImage.GetData()), cam);
            // this.AddChild(volumeBase);

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
            Node2D canvas = GetNode("FreeCam/RasterCanvas") as Node2D;
            pndCreator = new PND3d(cam,canvas);
            pndCreator.testBBOX = cubeBBOX;
            Polygon2D poly = GetNode("FreeCam/RasterCanvas/Center") as Polygon2D;
            int i = (0xFF << (1*8))^0xFF;
            poly.Color = new Color(i);

            Vector2 tmpVec = -Vector2.Inf;
            Console.WriteLine($"Vector before: {tmpVec}");
            tmpVec=tmpVec.maxLocal(new Vector2(1,3));
            Console.WriteLine($"Vector after: {tmpVec}");

        }
        
        //  // Called every frame. 'delta' is the elapsed time since the previous frame.
        public override void _Process(float delta)
        {
            // pndCreator.drawZenithBars();
            pndCreator.fastBoundSquare();
        }
    }

}