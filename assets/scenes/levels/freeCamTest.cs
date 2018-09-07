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
        GodotTaskScheduler gdTask = manager.TaskScheduler;
        
        
        Console.WriteLine("GD Task Scheduler concurrency level {0}, num processors {1}, task scheduler {2}",
        gdTask.MaximumConcurrencyLevel,System.Environment.ProcessorCount, gdTask);
        VoxelVolume basicVoxel = new VoxelVolume(new NoisePopulator(),64,512,512,64,0.0625f);
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
