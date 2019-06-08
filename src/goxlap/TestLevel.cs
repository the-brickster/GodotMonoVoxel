using Godot;
using System;

namespace Goxlap.src.Goxlap
{
    public class TestLevel : Spatial
    {
        // Declare member variables here. Examples:
        // private int a = 2;
        // private string b = "text";

        // Called when the node enters the scene tree for the first time.
        VoxelVolume volumeBase;
        public override void _Ready()
        {
            GD.Print("This is a test");
            volumeBase = new VoxelVolume(512, 512, 512, 32, 0.125f, @"res://assets/shaders/splatvoxel.shader", new BasicPopulator());
            this.AddChild(volumeBase);
        }

        //  // Called every frame. 'delta' is the elapsed time since the previous frame.
        //  public override void _Process(float delta)
        //  {
        //      
        //  }
    }
}