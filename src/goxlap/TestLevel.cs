using Godot;
using Goxlap.src.Goxlap.utils;
using System;
using HWVec = System.Numerics;

namespace Goxlap.src.Goxlap
{
    using System.Diagnostics;
    using System.Linq;
    using global::Goxlap.src.Goxlap.rasterizer;
    using global::Goxlap.src.Goxlap.tests;
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
        RasterizerPipeline rasterizer;
        utils.AABB testAABB;
        
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

            rasterizer = new RasterizerPipeline(cam);
            testAABB = new AABB(cubeBBOX.GetGlobalTransform().origin,new Vector3(1,1,1));
            
            GDScript gdClass = (GDScript)GD.Load(@"res://src/scripts/debug/DrawLine3D.gd");
            var lineDrawer = (Node2D)gdClass.New();
            this.AddChild(lineDrawer);
            rasterizer.lineDrawer = lineDrawer;

            RasterizerTests unitTests = new RasterizerTests(rasterizer,pndCreator);
            unitTests.TestDriver();
            // var from = cam.ProjectRayOrigin(new Vector2(511,198));
            // // var to = cam.ProjectRayNormal(new Vector2(511,198));
            // var to = new Vector3(-0.001638866f,0.3048292f, -1f)*1000f;
            // Console.WriteLine($"Start: {from}, to: {to}");
            // lineDrawer.Call("DrawRay",from,to,new Color(1,0,0));
            // lineDrawer.Call("DrawRay",new Vector3(0,0,10),new Vector3(0,0,-21),new Color(0,1,0));
        }
        
        // Called every frame. 'delta' is the elapsed time since the previous frame.
        public override void _Process(float delta)
        {
            // pndCreator.drawZenithBars();
            testAABB.Update(cubeBBOX.GetGlobalTransform().origin,new Vector3(1,1,1));
            pndCreator.fastBoundSquare(cam.GetCameraTransform().Inverse());
            rasterizer.UpdateViewParameters(80f);
            rasterizer.CreateBoundingRect(testAABB);
            // pndCreator.boundSquare();
        }
        

    }

}