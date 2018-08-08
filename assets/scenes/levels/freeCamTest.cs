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
        VoxelVolume basicVoxel = new VoxelVolume(new NoisePopulator(), ref gdTask,64,2048,2048,64,0.0625f);
        //VoxelTypes s = basicVoxel[1,2,3];
        
        this.AddChild(basicVoxel);

    }

//    public override void _Process(float delta)
//    {
//        // Called every frame. Delta is time since last frame.
//        // Update game logic here.
//        
//    }
}
