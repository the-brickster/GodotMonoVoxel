using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Godot;

namespace FPSGame.src.Common.ObjectPool
{
    abstract class AbstractSpatialObjectPool
    {
        public static Vector3 MAX_VALUE = new Vector3(float.MaxValue, float.MaxValue, float.MaxValue);

        private ConcurrentBag<Spatial> activeGameObjects;
        private ConcurrentBag<Spatial> inactiveGameObjects;
        private int maxNumObjects = 1000;
        private System.Collections.Generic.Dictionary<String, Spatial> objectTypes;
        public AbstractSpatialObjectPool(int maxNumObjects) {
            this.activeGameObjects = new ConcurrentBag<Spatial>();
            this.inactiveGameObjects = new ConcurrentBag<Spatial>();
            this.objectTypes = new System.Collections.Generic.Dictionary<string, Spatial>();

        }
        

        public virtual Spatial AcquireObject(string objectType) {
            if (this.objectTypes.ContainsKey(objectType)) {
                Type gameObjectType = this.objectTypes[objectType].GetType();
                foreach (var standbyObj in inactiveGameObjects)
                {
                    if (standbyObj.GetType() == gameObjectType)
                    {
                        Spatial obj = standbyObj;
                        inactiveGameObjects.TryTake(out obj);
                        activeGameObjects.Add(obj);
                        obj.Show();
                        GD.Print("Reusing object");
                        return obj;
                    }
                }
                var gameObject = (Spatial)this.objectTypes[objectType].Duplicate();
                activeGameObjects.Add(gameObject);
                GD.Print("Making new object");
                return gameObject;
            }
            
            return null;
        }

        public virtual void RegisterType(Spatial node) {
            string name = node.GetType().Name;
            GD.Print(name+" "+node.GetType());
            if (this.objectTypes.ContainsKey(name)) {
                GD.Print("Duplicate Key");
                return;
            }
            this.objectTypes.Add(name, (Spatial)node.Duplicate());
        }
        

        public virtual Boolean CleanUpNode(Spatial node) {
            bool result = false;
            if (this.activeGameObjects.Contains(node)) {
                node.Hide();
                result = this.activeGameObjects.TryTake(out node);
                GD.Print("Test Clean up "+ node.IsVisible()+" "+activeGameObjects);
                this.inactiveGameObjects.Add(node);
                
            }

            return result;
        }
    }


}
