using Godot;
using System;
using FPSGame.src.Common;

public class EjectingBrassTest : RigidBody
{
    float lifeTime = 5.0f;
    Timer lifeTimer;

    public override void _Ready()
    {
        // Called every time the node is added to the scene.
        // Initialization here
        lifeTimer = new Timer();
        this.AddChild(lifeTimer);
        lifeTimer.SetWaitTime(lifeTime);
        lifeTimer.Connect("timeout",this,"releaseSelf");
        lifeTimer.Start();
        // GD.Print("On ready called");
        this.Connect("visibility_changed",this,"reset");
    }
    public void releaseSelf(){
        // GD.Print("Clearing self: "+this.GetName());
        lifeTimer.Stop();
        var man =this.GetNode("/root/GlobalGameSystemsManager") as GlobalGameSystemsManager;
        man.ReleasePoolObject(this);
        
        // this.RemoveChild(lifeTimer);
    }
    public void reset(){
        // GD.Print("Visibility Changed: "+this.Visible);
        if(this.Visible == true){
            lifeTimer.Start();
            // GD.Print("---------------------------------------Timer Starting: "+this.GetName()+"| - |"+lifeTimer.GetTimeLeft());
        }
    }
    public override string ToString(){
        return this.GetName();
    }
//    public override void _Process(float delta)
//    {
//        // Called every frame. Delta is time since last frame.
//        // Update game logic here.
//        
//    }
}
