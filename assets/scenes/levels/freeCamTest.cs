using Godot;
using System;
using FPSGame.src.Common.goxlap;
using System.Threading;

public class freeCamTest : Spatial
{
    // Member variables here, example:
    // private int a = 2;
    // private string b = "textvar";

    public override void _Ready()
    {
        VoxelVolume basicVoxel = new VoxelVolume(new NoisePopulator(),128,1024,1024,64,0.25f);
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
