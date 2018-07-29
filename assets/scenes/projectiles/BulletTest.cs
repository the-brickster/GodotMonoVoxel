using Godot;
using System;

public class BulletTest : Spatial
{
    private static Vector3 up = new Vector3(0,1,0);
    public float speed  = 400f;
    public Vector3 projectileDirection ;
    public Vector3 origin ;

    private Spatial projectileHitEffect;
    private float dx = 0;
    private float deltaChanged  = 0;
    private RayCast hitRay;
    public async override void _Ready()                
    {
        this.SetPhysicsProcess(true);
        this.hitRay = this.GetNode("HitRay") as RayCast;
        // GD.print("Raymond: "+hitRay);
        // GD.print("origin: "+origin+" dir: "+projectileDirection);
        this.LookAtFromPosition(origin,projectileDirection,up);
        // GD.print("origin: "+origin+" dir: "+projectileDirection);
        projectileDirection = this.projectileDirection.Normalized();
        // GD.print("origin: "+origin+" dir: "+projectileDirection);
        
    }

    public override async void _PhysicsProcess(float delta){
        
        if(deltaChanged != delta){
            deltaChanged = delta;
            dx = (this.speed*deltaChanged)/60;
            GD.Print("curr frame speed: "+dx);
        }
        if(this.hitRay.IsColliding()){
            GD.Print("Projectile Hit");
            var point = this.hitRay.GetCollisionPoint();
            var pointNormal = this.hitRay.GetCollisionNormal();
            var root = this.GetTree().GetRoot();
            root.AddChild(this.projectileHitEffect);
            this.projectileHitEffect.LookAtFromPosition(point,up,pointNormal);
            this.QueueFree();
            return;
        }
        this.SetTranslation(this.GetTranslation()+projectileDirection*dx);
        
    }
//    public override void _Process(float delta)
//    {
//        this.hitRay.ForceRaycastUpdate();
       
//    }
}
