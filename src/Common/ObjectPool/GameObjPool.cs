using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Godot;

namespace FPSGame.src.Common.ObjectPool
{
    class GameObjPool : Node
    {
        System.Collections.Generic.Dictionary<String, gamePoolObj> gameObjRegistry;

        public override void _PhysicsProcess(float delta)
        {
            
            base._PhysicsProcess(delta);
        }

        public override void _Process(float delta)
        {
            base._Process(delta);
        }

        public override void _Ready()
        {
            base._Ready();
            GD.Print("Testing Game Pool Object");
            Console.WriteLine("Testing Console output");
            gameObjRegistry = new System.Collections.Generic.Dictionary<string, gamePoolObj>();
        }
        public Spatial acquireObject(string objectKey){
            // GD.Print("Testing acquisition "+objectKey);
            if(!this.gameObjRegistry.ContainsKey(objectKey)){
                return null;
            }
            var spatialObj = this.gameObjRegistry[objectKey].acquireObject();
            spatialObj.SetIdentity();
            return spatialObj;
        }

        public void releaseObject(Spatial s){
            var key = s.GetType().Name;
            if(!this.gameObjRegistry.ContainsKey(key)){
                return;
            }
            this.gameObjRegistry[key].releaseObject(s);
        }
        public void RegisterGameObj(PackedScene scene, int maxNumObj = 100){
            var name = scene.Instance().GetType().Name;
            // GD.Print("Register Object: "+name);
            this.gameObjRegistry[name] = new gamePoolObj(scene,maxNumObj);
        }

    }
    class gamePoolObj {
        private Stack<Spatial> inactiveQueue { get; set; } = new Stack<Spatial>();
        private List<Spatial> activeList { get; set; } = new List<Spatial>();
        private int maxNumObj { get; }
        private PackedScene scene;
        public gamePoolObj(PackedScene scene, int maxNumObj = 100) {
            this.maxNumObj = maxNumObj;
            this.scene = scene;
            for (int i = 0; i < maxNumObj; i++) {
                var obj = scene.Instance() as Spatial;
                obj.Hide();
                inactiveQueue.Push(obj);
            }
            // GD.Print("Length of inactive queue "+inactiveQueue.Count);
        }

        public Spatial acquireObject() {
            if (inactiveQueue.Count > 0) {
                var obj = inactiveQueue.Pop();
                activeList.Add(obj);
                obj.Show();
                return obj;
            }
            return null;
        }
        public void releaseObject(Spatial s) {

            string contents = string.Join<Spatial>(",",activeList.ToArray());
            Spatial tmp = activeList.Find(t => t.GetName().Equals(s.GetName()));
            if(tmp != null){
                activeList.Remove(tmp);
                inactiveQueue.Push(tmp);
                tmp.Hide();
                s.Hide();
                contents = string.Join<Spatial>(",",activeList.ToArray());
                // GD.Print("Is Visible: "+tmp.IsVisible()+"_"+tmp.GetName()+"_"+contents);
            }
            else{
                GD.Print("Object not contained within this gamePoolObj "+tmp.GetName()+" "+contents);
            }
            
        }

    }
}
