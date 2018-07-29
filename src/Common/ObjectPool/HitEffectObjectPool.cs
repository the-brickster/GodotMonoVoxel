using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Godot;

namespace FPSGame.src.Common.ObjectPool
{
    class HitEffectObjectPool : AbstractSpatialObjectPool
    {
        private float cleanUpTimer = 10.0f;

        public HitEffectObjectPool(int maxNumObjects, float cleanUpTime) : base(maxNumObjects)
        {
            GD.Print("Testing hit effect pool");
            this.cleanUpTimer = cleanUpTime;
        }

        public override Spatial AcquireObject(string objectType) {
            Spatial spatial = base.AcquireObject(objectType);

            return spatial;
        }
        
    }
}
