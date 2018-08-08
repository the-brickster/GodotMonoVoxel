using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Godot;

using FPSGame.src.Common.ObjectPool;
using FPSGame.src.Common.Utils;

namespace FPSGame.src.Common
{
    class GlobalGameSystemsManager : Node
    {
        private GameObjPool gameObjPool { get; set; }
        private LineRenderer line;

        // private List<LineRenderer> tracers;
        // private List<Tuple<Vector3,Vector3,int>> tracerData;
        // private List<float> tracerTime;

        private TracerManager tracerManager;
        public GodotTaskScheduler TaskScheduler {get; private set;}
        public async override void _PhysicsProcess(float delta)
        {
            base._PhysicsProcess(delta);
        }

        public async override void _Process(float delta)
        {
            base._Process(delta);
            TaskScheduler.Activate();
        }

        public override void _Ready()
        {
            base._Ready();
            this.TaskScheduler = new GodotTaskScheduler();
            
            gameObjPool = new GameObjPool();
            this.AddChild(gameObjPool);
            tracerManager = new TracerManager();
            this.AddChild(tracerManager);

        }

        public void RegisterGamePoolObj(PackedScene peteStacker, int maxNumObj=100){
            gameObjPool.RegisterGameObj(peteStacker,maxNumObj:maxNumObj);
        }
        public Spatial AcquirePoolObject(string objectName){
            var d = gameObjPool.acquireObject(objectName);
            // GD.Print("Value of game object: "+d);
            return d;
        }
        public void ReleasePoolObject(Spatial spatial){
            gameObjPool.releaseObject(spatial);
        }
        public void createTracerLine(Node root, Camera camera, float startThickness, float endThickness, Color startColor, Color endColor,Vector3 startPos, Vector3 endPos, int numSegments, float life){
            tracerManager.createTracerLine(root, camera, startThickness, endThickness, 
            startColor, endColor,startPos, endPos, numSegments,life);
        }
        // public void updateTracerLine(LineRenderer lineRenderer,Vector3 startPos, Vector3 endPos, int numSegments){
        //     Vector3 offset = (endPos-startPos)/numSegments;
        //     Vector3[] arr = new Vector3[numSegments];
        //     for(int i = 0; i < arr.Length;i++){
        //         arr[i]= startPos+(offset*i);
        //     }
        //     lineRenderer.Update(arr);
        // }

        // public async Task updateTime(float delta){
        //     for(int i = 0; i < tracerData.Count; i++){
        //         tracerTime[i]+=delta;
        //         tracers[i].UpdateTime(tracerTime[i]);
        //     }
            
        // }

    }
    public class TracerManager:Node{

        List<LineRenderer> tracers;
        List<LineRenderer> removelist = new List<LineRenderer>();

        public override void _Ready(){
            tracers = new List<LineRenderer>();
        }
        public async override void _Process(float delta){
            base._Process(delta);
            
        }
        public async override void _PhysicsProcess(float delta){
            base._PhysicsProcess(delta);
            await updateTime(delta);
        }

        public void createTracerLine(Node root, Camera camera, float startThickness, float endThickness, Color startColor, 
        Color endColor,Vector3 startPos, Vector3 endPos, int numSegments, float life){
            LineRenderer line = new LineRenderer(root, camera,0,0,null);
            line.SetThickness(startThickness,endThickness);
            line.SetColors(startColor,endColor);
            line.UpdateTime(0.0f);
            line.lifetime = life;
            tracers.Add(line);
            updateTracerLine(line,startPos,endPos,numSegments);
        }
        public void updateTracerLine(LineRenderer lineRenderer,Vector3 startPos, Vector3 endPos, int numSegments){
            Vector3 offset = (endPos-startPos)/numSegments;
            Vector3[] arr = new Vector3[numSegments];
            for(int i = 0; i < arr.Length;i++){
                arr[i]= startPos+(offset*i);
            }
            lineRenderer.Update(arr);
        }
        public async Task updateTime(float delta){
            
             
            // await Task.Run(()=>{
                for(int i = 0; i < tracers.Count; i++){
                float time = tracers[i].getTime();
                time+=delta;
                tracers[i].UpdateTime(time);
                if(time >= tracers[i].lifetime){
                    tracers[i].removeFromParent();
                    removelist.Add(tracers[i]);
                }
            }

                for(int i = 0; i < removelist.Count; i++){
                    tracers.Remove(removelist[i]);
                }
                removelist.Clear();
            // });
            
            
        }
        
    }
}
