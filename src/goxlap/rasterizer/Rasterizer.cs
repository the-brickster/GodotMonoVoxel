using Godot;
using HWVec = System.Numerics;
using System;

namespace Goxlap.src.Goxlap.rasterizer
{
    using global::Goxlap.src.Goxlap.utils;
    public class RasterizerPipeline{
        public Camera Cam {get;set;}

        public HWVec.Vector3 CameraPosition;
        public HWVec.Matrix4x4 ProjMatrix;
        public HWVec.Matrix4x4 ViewMatrix;
        public HWVec.Matrix4x4 ViewProjectionMatrix;
        public HWVec.Matrix4x4 ViewProjectionMatrixInverse = new HWVec.Matrix4x4();
        public Vector2 ScreenSize;
        public Transform viewTransform;
        public Node2D lineDrawer;

        public RasterizerPipeline(Camera camera){
            Cam = camera;
            ScreenSize = Cam.GetViewport().GetSize();
            float fovInDegrees = 80f;

            int width = (int)ScreenSize.x;
            int height = (int)ScreenSize.y;
            float near = Cam.Near;
            float far = Cam.Far;
            float imageAspecRatio = width/(float)height;
            ProjMatrix = new HWVec.Matrix4x4();
            GDExtension.gluPerspective(ref ProjMatrix,ref fovInDegrees,imageAspecRatio,near,far,true);
            viewTransform = camera.GetCameraTransform().Inverse();

        }

        //---------------------------------------
        // Raster Pipeline Methods
        //---------------------------------------
        public unsafe BoundingRect CreateBoundingRect(utils.AABB voxelAABB){
            
            Vector3 pos = GDExtension.xFormNumeric(viewTransform,voxelAABB.center);
            float* m = stackalloc float[9];
            //------------------
            // Set matrix values
            //------------------
            m[0] = viewTransform.basis.Column0.x;
            m[1] = viewTransform.basis.Column0.y;
            m[2] = viewTransform.basis.Column0.z;
            m[3] = viewTransform.basis.Column1.x;
            m[4] = viewTransform.basis.Column1.y;
            m[5] = viewTransform.basis.Column1.z;
            m[6] = viewTransform.basis.Column2.x;
            m[7] = viewTransform.basis.Column2.y;
            m[8] = viewTransform.basis.Column2.z;

            //Get the 
            float ox0 = Convert.ToInt32(m[0]*pos.z >= m[2]*pos.x) * 1-0.5f;
            float oy0 = Convert.ToInt32(m[3]*pos.z >= m[5]*pos.x) * 1-0.5f;
            float oz0 = Convert.ToInt32(m[6]*pos.z >= m[8]*pos.x) * 1-0.5f;
            float ox1 = Convert.ToInt32(m[1]*pos.z >= m[2]*pos.y) * 1-0.5f;
            float oy1 = Convert.ToInt32(m[4]*pos.z >= m[5]*pos.y) * 1-0.5f;
            float oz1 = Convert.ToInt32(m[7]*pos.z >= m[8]*pos.y) * 1-0.5f;

            float ox02 = - ox0;
            float oy02 = - oy0;
            float oz02 = - oz0;
            float ox12 = - ox1;
            float oy12 = - oy1;
            float oz12 = - oz1;

            //Get the corners approximate view space position to calculate bounding square
            float tmpX0 = (pos.x - (m[0]*ox0 + m[3]*oy0 + m[6]*oz0));
            float tmpY0 = (pos.y - (m[1]*ox1 + m[4]*oy1 + m[7]*oz1));
            float tmpZ0 = (pos.z - (m[2]*ox0 + m[5]*oy0 + m[8]*oz0));

            float tmpX1 = (pos.x + (m[0]*ox0 + m[3]*oy0 + m[6]*oz0));
            float tmpY1 = (pos.y + (m[1]*ox1 + m[4]*oy1 + m[7]*oz1));
            float tmpZ1 = (pos.z + (m[2]*ox1 + m[5]*oy1 + m[8]*oz1));

            float tmpX2 = (pos.x - (m[0]*ox02 + m[3]*oy02 + m[6]*oz02));
            float tmpY2 = (pos.y - (m[1]*ox12 + m[4]*oy12 + m[7]*oz12));
            float tmpZ2 = (pos.z - (m[2]*ox02 + m[5]*oy02 + m[8]*oz02));

            float tmpX3 = (pos.x + (m[0]*ox02 + m[3]*oy02 + m[6]*oz02));
            float tmpY3 = (pos.y + (m[1]*ox12 + m[4]*oy12 + m[7]*oz12));
            float tmpZ3 = (pos.z + (m[2]*ox12 + m[5]*oy12 + m[8]*oz12));

            //Project the coordinates to screenspace
            Vector2 tmp1 = viewToScreenSpace(tmpX0,tmpY0,tmpZ0);
            Vector2 tmp2 = viewToScreenSpace(tmpX1,tmpY1,tmpZ1);
            Vector2 tmp3 = viewToScreenSpace(tmpX2,tmpY2,tmpZ2);
            Vector2 tmp4 = viewToScreenSpace(tmpX3,tmpY3,tmpZ3);

            //Take the maximum and the minimum
            Vector2 scrnMin = Vector2.Inf;
            scrnMin = scrnMin.minLocal(tmp1);
            scrnMin = scrnMin.minLocal(tmp2);
            scrnMin = scrnMin.minLocal(tmp3);
            scrnMin = scrnMin.minLocal(tmp4);
            Vector2 scrnMax = -Vector2.Inf;
            scrnMax = scrnMax.maxLocal(tmp1);
            scrnMax = scrnMax.maxLocal(tmp2);
            scrnMax = scrnMax.maxLocal(tmp3);
            scrnMax = scrnMax.maxLocal(tmp4);
            // Console.WriteLine($"Rasterizer ScreenMax {scrnMax}, ScreenMin {scrnMin}");
            return new BoundingRect(GDExtension.toNumericVector2(scrnMin), GDExtension.toNumericVector2(scrnMax));

        }
        public BitMap CreateScreenToVoxelMapping(BoundingRect rect, utils.AABB aabb){
            BitMap map = new BitMap();
            map.Create(ScreenSize);
            uint x,y;
            uint xmin = (uint)Mathf.Max(0,Mathf.Min(ScreenSize.x-1,Mathf.Floor(rect.x)));
            uint ymin = (uint)Mathf.Max(0,Mathf.Min(ScreenSize.y-1,Mathf.Floor(rect.y)));
            uint xmax = (uint)Mathf.Max(0,Mathf.Min(ScreenSize.x-1,Mathf.Floor(rect.max.X)));
            uint ymax = (uint)Mathf.Max(0,Mathf.Min(ScreenSize.y-1,Mathf.Floor(rect.max.Y)));
            Console.WriteLine($"xmin: {xmin}, ymin: {ymin}, xmax: {xmax}, ymax: {ymax}");
            utils.Ray raymond = new Ray();
            HWVec.Vector2 p = new HWVec.Vector2();
            HWVec.Vector4 rdh = new HWVec.Vector4(); 
            
            HWVec.Vector2 viewPort = ScreenSize.toNumericVector2();
            HWVec.Vector2 coord = Cam.GetViewport().GetVisibleRect().Position.toNumericVector2();
            Vector2 bitSetVec = new Vector2();
            //Test to see if our calculations work:
            // Vector2 tmp1 = new Vector2(xmin + (xmax-xmin)/2,ymin + (ymax-ymin)/2);
            // x = (uint)(xmin + (xmax-xmin)/2);
            // y = (uint)viewPort.Y - (ymin + (ymax-ymin)/2);
            // p = 2.0f * new HWVec.Vector2(x,y) /  (viewPort - coord) - new HWVec.Vector2(1.0f);
            
            raymond.rayOrigin = CameraPosition;
            // rdh = GDExtension.multProj(ViewProjectionMatrixInverse,new HWVec.Vector4(p.X,p.Y,-1.0f,1.0f));
            // raymond.rayDir = Cam.ProjectRayNormal(tmp1).toNumericVector3() - raymond.rayOrigin;
            // raymond.rayDir = ((new HWVec.Vector3(rdh.X,rdh.Y,rdh.Z)/rdh.W)  - raymond.rayOrigin);
            // raymond.invDirection = GDExtension.safeInverse(raymond.rayDir);
            

            // var res = this._intersectsAABB(raymond,aabb.min,aabb.max);
            // Console.WriteLine($"Visible = {viewPort} ,X: {x}, Y: {y}, NDC P: {p}, ray origin: {raymond.rayOrigin}, rdh: {rdh}, ray dir: {raymond.rayDir}, res: {res}");
            // Console.WriteLine($"Does our ray intersect the box? {res.Y > res.X}");
            // lineDrawer.Call("DrawRay",raymond.rayOrigin.toGDVector3(),raymond.rayDir.toGDVector3()*1000f,new Color(1,0,0));
            lineDrawer.Call("DrawRay",new Vector3(0,0,10),new Vector3(0,0,-21),new Color(0,1,0));
            for(y = ymin; y <= ymax; ++y){
                for(x = xmin; x <= xmax;x++){
                    //Need to flip the y coodinates, since Godot UI uses the top left corner for the origin
                    uint y2 =(uint) viewPort.Y - y;
                    p = 2.0f * new HWVec.Vector2(x,y2) /  (viewPort - coord) - new HWVec.Vector2(1.0f);
                    rdh = GDExtension.multProj(ViewProjectionMatrixInverse,new HWVec.Vector4(p.X,p.Y,-1.0f,1.0f));
                    raymond.rayDir = ((new HWVec.Vector3(rdh.X,rdh.Y,rdh.Z)/rdh.W)  - raymond.rayOrigin);
                    raymond.invDirection = GDExtension.safeInverse(raymond.rayDir);
                    var res = this._intersectsAABB(raymond,aabb.min,aabb.max);
                    if(res.Y > res.X){
                        bitSetVec.x = x; bitSetVec.y = y;
                        map.SetBit(bitSetVec,true);
                        // lineDrawer.Call("DrawRay",raymond.rayOrigin.toGDVector3(),raymond.rayDir.toGDVector3()*10f,new Color(1,0,0));
                    }
                }
            }
            return map;
        }

        //----------------------------------
        // Utility Methods
        //----------------------------------
        public void SetupRasterizerProperties(){
            viewTransform = Cam.GetCameraTransform().Inverse();
            ViewMatrix = viewTransform.viewTransToMat4x4();
            CameraPosition = Cam.GetCameraTransform().origin.toNumericVector3();
            ViewProjectionMatrix = ProjMatrix.multiplyColMaj(ViewMatrix);

            // HWVec.Matrix4x4 tmp = new HWVec.Matrix4x4();
            HWVec.Matrix4x4.Invert(ViewProjectionMatrix,out ViewProjectionMatrixInverse);
            // ViewProjectionMatrixInverse = ViewMatrix.multiplyColMaj(tmp);

        }
        private HWVec.Vector2 _intersectsAABB(utils.Ray ray, HWVec.Vector3 boxMin, HWVec.Vector3 boxMax){
            HWVec.Vector3 tMin = (boxMin - ray.rayOrigin)/ray.rayDir;
            HWVec.Vector3 tMax = (boxMax - ray.rayOrigin)/ray.rayDir;
            HWVec.Vector3 t1 = HWVec.Vector3.Min(tMin,tMax);
            HWVec.Vector3 t2 = HWVec.Vector3.Max(tMin,tMax);

            float tNear = Mathf.Max(Mathf.Max(t1.X,t1.Y),t1.Z);
            float tFar = Mathf.Min(Mathf.Min(t2.X,t2.Y),t2.Z);
            return new HWVec.Vector2(tNear,tFar);
        }

        public Vector2 viewToScreenSpace(float x, float y, float z){
            
            Vector3 viewPos = new Vector3(x,y,z);
            Vector3 scrnPos = new Vector3();

            float w = GDExtension.multProj(ProjMatrix,viewPos,ref scrnPos);
            scrnPos /= w;
            

            Vector2 tmp = new Vector2();
            tmp.x = (scrnPos.x * 0.5f + 0.5f) * ScreenSize.x;
            tmp.y = (-scrnPos.y * 0.5f + 0.5f) * ScreenSize.y;

            return tmp;
        }
        public void UpdateViewParameters(float fovInDegrees){
            int width = (int)ScreenSize.x;
            int height = (int)ScreenSize.y;
            float near = Cam.Near;
            float far = Cam.Far;
            float imageAspecRatio = width/(float)height;
            ProjMatrix = new HWVec.Matrix4x4();
            GDExtension.gluPerspective(ref ProjMatrix,ref fovInDegrees,imageAspecRatio,near,far,true);
        }

    }
}