using Godot;
using System;
using FPSGame.src.Common.goxlap;
using System.Threading;
using System.Threading.Tasks;
using FPSGame.src.Common;

public class freeCamTest : Spatial
{
    // Member variables here, example:
    // private int a = 2;
    // private string b = "textvar";
    private ShaderMaterial material;
    public override async void _Ready()
    {
        var voxelSize = 1.0f;
        float r = 0.02f;
        float g = 0.25f;
        float b = 0.9f;

        var manager = this.GetNode("/root/GlobalGameSystemsManager") as GlobalGameSystemsManager;
        var mat = new ShaderMaterial();
        mat.SetShader(ResourceLoader.Load(@"res://assets/shaders/point_shader_test.shader") as Shader);

        mat.SetShaderParam("albedo", new Color(r, g, b, 1));
        mat.SetShaderParam("voxelSize", voxelSize);

        var cam = GetViewport().GetCamera();
        var position = cam.GetGlobalTransform().origin;
        mat.SetShaderParam("camera_pos", position);

        var screenRes = GetViewport().Size;
        var screenPos = GetViewport().GetVisibleRect().Position;
        GD.Print($"Screen Res: {screenRes}");
        mat.SetShaderParam("screen_size", screenRes);
        mat.SetShaderParam("viewport_pos", screenPos);
        MeshInstance m = this.GetChild(3) as MeshInstance;



        Console.WriteLine("LAYER: {0}", Convert.ToString(m.Layers, 2));
        VoxelVolume basicVoxel = new VoxelVolume(new NoisePopulator(), 256, 256, 256, 64, voxelSize * 2.0f, mat);
        // VoxelTypes s = basicVoxel[1,2,3];
        // Vector3 a = new Vector3(1,0,1);
        // Console.WriteLine("Vector before {0}",a);
        // vecTest(a);
        // Console.WriteLine("Vector after {0}",a);
        this.AddChild(basicVoxel);

        material = mat;
        GetViewport().Connect("size_changed", this, nameof(ScreenResChanged));
    }

    public override void _Process(float delta)
    {
        var cam = GetViewport().GetCamera();
        var position = cam.GetGlobalTransform().origin;
        material.SetShaderParam("camera_pos", position);
    }


    public void ScreenResChanged()
    {

        var screenRes = GetViewport().Size;
        var screenPos = GetViewport().GetVisibleRect().Position;
        GD.Print($"Screen Resolution Changed {screenRes}, screen position {screenPos}");
        material.SetShaderParam("screen_size", screenRes);
        material.SetShaderParam("viewport_pos", screenPos);
    }

    static Random random = new Random();
    public double GetRandomNumber(double minimum, double maximum)
    {
        return random.NextDouble() * (maximum - minimum) + minimum;
    }
    public void vecTest(Vector3 a)
    {
        a.x = 10;
        a.y = 200;
        a.z = 300;
        Console.WriteLine("Vector during {0}", a);
    }
    //    public override void _Process(float delta)
    //    {
    //        // Called every frame. Delta is time since last frame.
    //        // Update game logic here.
    //        
    //    }
}
