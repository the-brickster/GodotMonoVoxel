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

    public override async void _Ready()
    {
        var manager = this.GetNode("/root/GlobalGameSystemsManager") as GlobalGameSystemsManager;
        var Freecam = this.GetChild(1) as Camera;
        var top = Freecam.Near * Mathf.Tan(Mathf.Deg2Rad(Freecam.Fov)/2);
        var height = OS.GetWindowSize().y;
        var mat = new ShaderMaterial();
                mat.SetShader(ResourceLoader.Load(@"res://assets/shaders/point_shader_test.shader") as Shader);
                mat.SetShaderParam("albedo",new Color(0.5f,0f,0f,1));
                mat.SetShaderParam("point_size",18.0f);
                mat.SetShaderParam("near",Freecam.Near);
                mat.SetShaderParam("far",Freecam.Far);
                mat.SetShaderParam("bottom",-top);
                mat.SetShaderParam("height",height);
        
        Console.WriteLine("CAM FOV: {0}, TOP: {1}, {2}, {3}, {4}",Freecam.Fov,top,Freecam.Near, Freecam.Far,height);
        VoxelVolume basicVoxel = new VoxelVolume(new NoisePopulator(),128,256,256,128,0.0625f,mat);
        // VoxelTypes s = basicVoxel[1,2,3];
        // Vector3 a = new Vector3(1,0,1);
        // Console.WriteLine("Vector before {0}",a);
        // vecTest(a);
        // Console.WriteLine("Vector after {0}",a);
        this.AddChild(basicVoxel);

    }

    public void vecTest(Vector3 a){
        a.x = 10;
        a.y=200;
        a.z=300;
        Console.WriteLine("Vector during {0}",a);
    }
//    public override void _Process(float delta)
//    {
//        // Called every frame. Delta is time since last frame.
//        // Update game logic here.
//        
//    }
}
