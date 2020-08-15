using Godot;
using Goxlap.src.Goxlap.utils;
using System;
using System.Runtime.CompilerServices;

namespace Goxlap.src.Goxlap.rasterizer{
    public class PND3d{
        public Camera camera;
        public Node2D canvas;
        public Vector2 screenSize;
        public float hx, hy, hz;
        public Transform transform;

        public float[] m = new float[9];
        public VanishLines[] axisLines = new VanishLines[6];
        public Polygon2D[] intersectPoints = {new Polygon2D(),new Polygon2D(),new Polygon2D()};
        public static Vector2[] points = {
            new Vector2(0f,0f),
            new Vector2(10f,0f),
            new Vector2(10f,10f),
            new Vector2(0f,10f)
        };

        public MeshInstance testBBOX;
        private Polygon2D boundSq = new Polygon2D();
        private System.Numerics.Matrix4x4 projMatrix = new System.Numerics.Matrix4x4();

        public PND3d(Camera camera, Node2D canvas){
            Console.WriteLine($"Size of AABB struct: {System.Runtime.InteropServices.Marshal.SizeOf(typeof(utils.AABB))}");
            Console.WriteLine($"Size of Bounding Rect struct: {System.Runtime.InteropServices.Marshal.SizeOf(typeof(utils.BoundingRect))}");
            Console.WriteLine($"VECTOR TEST: {System.Numerics.Vector.IsHardwareAccelerated}, int count: {System.Numerics.Vector<byte>.Count}");
            this.camera = camera;
            this.canvas = canvas;
            this.screenSize = camera.GetViewport().Size;
            float xres = screenSize.x;
            float yres = screenSize.y;

            hx = xres/2.0f; hy = yres/2.0f; hz = hx;
            transform = camera.GetCameraTransform();
            //------------------
            // Set matrix values
            //------------------
            m[0] = transform.basis.Column0.x;
            m[1] = transform.basis.Column0.y;
            m[2] = transform.basis.Column0.z;

            m[3] = transform.basis.Column1.x;
            m[4] = transform.basis.Column1.y;
            m[5] = transform.basis.Column1.z;

            m[6] = transform.basis.Column2.x;
            m[7] = transform.basis.Column2.y;
            m[8] = transform.basis.Column2.z;
            
            for(int i =0; i < 3; i++){
                intersectPoints[i].Polygon = points;
            }
            boundSq.SetColor(new Color(0, 1, 1, 1 ));
            boundSq.InvertBorder = 3f;
            boundSq.InvertEnable = true;

            var width = (int)camera.GetViewport().Size.x;
            var height = (int)camera.GetViewport().Size.y;

            float fovInDegrees = 80f;
            var near = camera.Near;
            var far = camera.Far;
            var imageAspectRatio = width / (float)height;
            Console.WriteLine($"{camera.Fov} FOV in degrees: {fovInDegrees}");
            GDExtension.gluPerspective(ref projMatrix,ref fovInDegrees,imageAspectRatio,near,far,true);
        }
        
        public void drawZenithBars(){
            transform =camera.GetCameraTransform() * testBBOX.GlobalTransform;
            //------------------
            // Set matrix values
            //------------------
            m[0] = transform.basis.Column0.x;
            m[1] = transform.basis.Column0.y;
            m[2] = transform.basis.Column0.z;

            m[3] = transform.basis.Column1.x;
            m[4] = transform.basis.Column1.y;
            m[5] = transform.basis.Column1.z;

            m[6] = transform.basis.Column2.x;
            m[7] = transform.basis.Column2.y;
            m[8] = transform.basis.Column2.z;
            
            for(int i=0;i<3;i++){
                if(m[i*3+2] ==0) continue;
                Godot.Collections.Array arrLines = canvas.GetChildren();
                int ofs = 3-i;
                Color col = new Color((0xFF << (ofs*8))^0xFF);
                float f = hz/m[i*3+2];
                float sx = m[i*3+0]*f + hx;
                axisLines[i*2].updateLine(sx,0,sx,screenSize.y-1,col);
                if(arrLines.Contains(axisLines[i*2].axisLine)){
                    axisLines[i*2].axisLine.Update();
                }
                else{
                    canvas.AddChild(axisLines[i*2].axisLine);
                }
                float sy = m[i*3+1]*f + hy;
                axisLines[i*2+1].updateLine(0,sy,screenSize.x-1,sy,col);
                if(arrLines.Contains(axisLines[i*2+1].axisLine)){
                    axisLines[i*2+1].axisLine.Update();
                }
                else{
                    canvas.AddChild(axisLines[i*2+1].axisLine);
                }

                
                intersectPoints[i].Color = col;
                intersectPoints[i].Position=new Vector2(sx-5f,sy-5f);
                if(intersectPoints[i].IsInsideTree()){
                    intersectPoints[i].Update();
                }
                else{
                    canvas.AddChild(intersectPoints[i]);
                }
                
            }
        }
        public BoundingRect boundSquare(utils.AABB AABB){
            var watch = System.Diagnostics.Stopwatch.StartNew();
            Vector3 cen = AABB.center.toGDVector3();
            Vector3 ext = new Vector3(0.5f,0.5f,0.5f);

            Camera cam = this.camera;

            Vector2 min = cam.UnprojectPosition(new Vector3(cen.x - ext.x, cen.y - ext.y, cen.z - ext.z));
         Vector2 max = min;
 
 
         //0
         Vector2 point = min;
         min = new Vector2(min.x >= point.x ? point.x : min.x, min.y >= point.y ? point.y : min.y);
         max = new Vector2(max.x <= point.x ? point.x : max.x, max.y <= point.y ? point.y : max.y);
 
         //1
         point = cam.UnprojectPosition(new Vector3(cen.x + ext.x, cen.y - ext.y, cen.z - ext.z));
         min = new Vector2(min.x >= point.x ? point.x : min.x, min.y >= point.y ? point.y : min.y);
         max = new Vector2(max.x <= point.x ? point.x : max.x, max.y <= point.y ? point.y : max.y);
 
 
         //2
         point = cam.UnprojectPosition(new Vector3(cen.x - ext.x, cen.y - ext.y, cen.z + ext.z));
         min = new Vector2(min.x >= point.x ? point.x : min.x, min.y >= point.y ? point.y : min.y);
         max = new Vector2(max.x <= point.x ? point.x : max.x, max.y <= point.y ? point.y : max.y);
 
         //3
         point = cam.UnprojectPosition(new Vector3(cen.x + ext.x, cen.y - ext.y, cen.z + ext.z));
         min = new Vector2(min.x >= point.x ? point.x : min.x, min.y >= point.y ? point.y : min.y);
         max = new Vector2(max.x <= point.x ? point.x : max.x, max.y <= point.y ? point.y : max.y);
 
         //4
         point = cam.UnprojectPosition(new Vector3(cen.x - ext.x, cen.y + ext.y, cen.z - ext.z));
         min = new Vector2(min.x >= point.x ? point.x : min.x, min.y >= point.y ? point.y : min.y);
         max = new Vector2(max.x <= point.x ? point.x : max.x, max.y <= point.y ? point.y : max.y);
 
         //5
         point = cam.UnprojectPosition(new Vector3(cen.x + ext.x, cen.y + ext.y, cen.z - ext.z));
         min = new Vector2(min.x >= point.x ? point.x : min.x, min.y >= point.y ? point.y : min.y);
         max = new Vector2(max.x <= point.x ? point.x : max.x, max.y <= point.y ? point.y : max.y);
 
         //6
         point = cam.UnprojectPosition(new Vector3(cen.x - ext.x, cen.y + ext.y, cen.z + ext.z));
         min = new Vector2(min.x >= point.x ? point.x : min.x, min.y >= point.y ? point.y : min.y);
         max = new Vector2(max.x <= point.x ? point.x : max.x, max.y <= point.y ? point.y : max.y);
 
         //7
         point = cam.UnprojectPosition(new Vector3(cen.x + ext.x, cen.y + ext.y, cen.z + ext.z));
         min = new Vector2(min.x >= point.x ? point.x : min.x, min.y >= point.y ? point.y : min.y);
         max = new Vector2(max.x <= point.x ? point.x : max.x, max.y <= point.y ? point.y : max.y);
         watch.Stop();
         
        var elapsedMs = watch.Elapsed.TotalMilliseconds;
         Console.WriteLine($"Regular BBox, min: {min}, max: {max}, elapsed ms: {elapsedMs}");
         BoundingRect bounding = new BoundingRect(min.toNumericVector2(),max.toNumericVector2());
         return bounding;
        //  Vector2 diff = min - max;
        //     float diffX = Mathf.Abs(diff.x);
        //     float diffy = Mathf.Abs(diff.y);
        //     Vector2[] points = {
        //         new Vector2(0f,0f),
        //         new Vector2(diffX,0f),
        //         new Vector2(diffX,diffy),
        //         new Vector2(0f,diffy)
        //     };
            
        //     boundSq.Polygon = points;
        //     boundSq.Position = min;
            
        //     if(boundSq.IsInsideTree()){
        //         boundSq.Update();
        //     }
        //     else{
        //         canvas.AddChild(boundSq);
        //     }
        }
        public unsafe void fastBoundSquare(Transform camTrans){
            transform =camTrans;
            // var watch = System.Diagnostics.Stopwatch.StartNew();
            
            
            // Console.WriteLine($"{camera.Fov} FOV in degrees: {80}");
            
            //------------------
            // Set matrix values
            //------------------
            m[0] = transform.basis.Column0.x;
            m[1] = transform.basis.Column0.y;
            m[2] = transform.basis.Column0.z;

            m[3] = transform.basis.Column1.x;
            m[4] = transform.basis.Column1.y;
            m[5] = transform.basis.Column1.z;

            m[6] = transform.basis.Column2.x;
            m[7] = transform.basis.Column2.y;
            m[8] = transform.basis.Column2.z;

            Vector3 pos = transform.Xform(testBBOX.GetGlobalTransform().origin);

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

//             Vector2 tmp1 = camera.UnprojectPosition(new Vector3(tmpX0,tmpY0,tmpZ0));
// Vector2 tmp2 = camera.UnprojectPosition(new Vector3(tmpX1,tmpY1,tmpZ1));
// Vector2 tmp3 = camera.UnprojectPosition(new Vector3(tmpX2,tmpY2,tmpZ2));
// Vector2 tmp4 = camera.UnprojectPosition(new Vector3(tmpX3,tmpY3,tmpZ3));
            Vector2 tmp1 = viewToScreenSpace(tmpX0,tmpY0,tmpZ0);
            Vector2 tmp2 = viewToScreenSpace(tmpX1,tmpY1,tmpZ1);
            Vector2 tmp3 = viewToScreenSpace(tmpX2,tmpY2,tmpZ2);
            Vector2 tmp4 = viewToScreenSpace(tmpX3,tmpY3,tmpZ3);

            // Console.WriteLine($"ox0: {ox0}, oy0: {oy0}, oz0: {oz0}, ox1: {ox1}, oy1: {oy1}, oz1: {oz1}");

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
            
            // Vector2 scrnMin = Vector2.Inf;
            // scrnMin = scrnMin.minLocal(tmp1);
            // scrnMin = scrnMin.minLocal(tmp2);
            // scrnMin = scrnMin.minLocal(tmp3);
            // scrnMin = scrnMin.minLocal(tmp4);

            // Vector2 scrnMax = -Vector2.Inf;
            // scrnMax = scrnMax.maxLocal(tmp1);
            // scrnMax = scrnMax.maxLocal(tmp2);
            // scrnMax = scrnMax.maxLocal(tmp3);
            // scrnMax = scrnMax.maxLocal(tmp4);
            // watch.Stop();
            // var elapsedMs = watch.Elapsed.TotalMilliseconds;
            // Console.WriteLine($"Fast BBox method, min: {scrnMin}, max: {scrnMax}, elapsed ms: {elapsedMs}");
            // Console.WriteLine($"ScreenMax {scrnMax}, ScreenMin {scrnMin}");
            Vector2 diff = scrnMax - scrnMin;
            float diffX = Mathf.Abs(diff.x);
            float diffy = Mathf.Abs(diff.y);
            Vector2[] points = {
                new Vector2(0f,0f),
                new Vector2(diffX,0f),
                new Vector2(diffX,diffy),
                new Vector2(0f,diffy)
            };
            
            boundSq.Polygon = points;
            boundSq.Position = scrnMin;
            
            if(boundSq.IsInsideTree()){
                boundSq.Update();
            }
            else{
                canvas.AddChild(boundSq);
            }

        }

        // [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Vector2 viewToScreenSpace(float x, float y, float z){
            
            Vector3 viewPos = new Vector3(x,y,z);
            Vector3 scrnPos = new Vector3();

            float w = GDExtension.multProj(projMatrix,viewPos,ref scrnPos);
            scrnPos /= w;
            

            Vector2 tmp = new Vector2();
            tmp.x = (scrnPos.x * 0.5f + 0.5f) * screenSize.x;
            tmp.y = (-scrnPos.y * 0.5f + 0.5f) * screenSize.y;

            return tmp;
        }
        
    }
}