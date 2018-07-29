using Godot;
using System;
using FPSGame.src.Common.ObjectPool;
using FPSGame.src.Common;

public class SparkParticleRound : Spatial
{
    // Member variables here, example:
    // private int a = 2;
    // private string b = "textvar";
    /*
    	TRANSFORM[0].xyz *= base_scale;
	TRANSFORM[1].xyz *= base_scale;
	TRANSFORM[2].xyz *= base_scale;
	TRANSFORM[0].xyz += VELOCITY*base_scale*0.25;
	TRANSFORM[1].xyz += VELOCITY*base_scale*0.25;
	TRANSFORM[2].xyz += VELOCITY*base_scale*0.25;
     */
    private float instanceInterval = 1.1f;
    public override async void _Ready()
    {
        this.init();
    }

    public async void init(){
        var particleBlast = this.GetNode("Particles") as Particles;
        var particleHit = this.GetNode("HitParticle") as Particles;
        particleBlast.Restart();
        particleHit.Restart();
        this.instanceInterval = particleBlast.Lifetime;
        // Called every time the node is added to the scene.
        // Initialization here
        var timer = new Timer();
        timer.SetOneShot(true);
        timer.SetWaitTime(instanceInterval);
        AddChild(timer);
        timer.Start();
        timer.Connect("timeout",this,"clearParticle");

        await ToSignal(timer,"timeout");
    }

    public void clearParticle(){
        var tmp = this.GetTree().GetNodesInGroup("Particles").Count;
        // GD.Print("Attempting to delete particle, particle count: "+tmp);
        this.QueueFree();
        // GlobalGameSystemsManager manager = this.GetNode("/root/GlobalGameSystemsManager") as GlobalGameSystemsManager;
        // var particleBlast = this.GetNode("Particles") as Particles;
        // var particleHit = this.GetNode("HitParticle") as Particles;
        // manager.releaseHitEffectNode(this);
        // GD.Print("After calling queuefree, particle count: "+tmp);
    }

//    public override void _Process(float delta)
//    {
//        // Called every frame. Delta is time since last frame.
//        // Update game logic here.
//        
//    }
}
