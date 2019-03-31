using Godot;
using System;
using System.Collections.Generic;

public class HUDCanvas : ReferenceRect
{
    // Member variables here, example:
    // private int a = 2;
    // private string b = "textvar";
    private Label fpsTextLabel = null;

    private List<int> times = new List<int>();

    public async override void _Ready()
    {

        GD.Print("Loaded HUD");
        this.fpsTextLabel = this.GetNode("FPSCounter") as Label;
        this.fpsTextLabel.SetText(Engine.GetFramesPerSecond().ToString());
    }
    public async override void _Process(float delta)
    {

        this.fpsTextLabel.SetText(Engine.GetFramesPerSecond().ToString());
    }
    //    public override void _Process(float delta)
    //    {
    //        // Called every frame. Delta is time since last frame.
    //        // Update game logic here.
    //        
    //    }
}
