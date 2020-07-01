using Godot;
using SIMD = System.Numerics;

namespace Goxlap.src.Goxlap.utils{
    public struct VanishLines{
        public float x0;
        public float y0;
        public float x1;
        public float y1;
        public Line2D axisLine;
        public Polygon2D intersect;
        public static Vector2[] points = {
            new Vector2(0f,0f),
            new Vector2(10f,0f),
            new Vector2(10f,10f),
            new Vector2(0f,10f)
        };

        public void updateLine(float x0, float y0,float x1, float y1,Color col){
            if(axisLine == null){
                axisLine = new Line2D();
                axisLine.SetWidth(1.0f);
            }
            this.x0 = x0;this.x1=x1;this.y0=y0;this.y1=y1;
            Vector2[] linePonts = {new Vector2(x0,y0),new Vector2(x1,y1)};
            axisLine.SetPoints(linePonts);
            axisLine.DefaultColor = col;
        }
        public void updateIntersectPoint(float sx, float sy, Color col){
            if(intersect == null){
                intersect = new Polygon2D();
                intersect.Polygon = points;
                intersect.Color = col;
            }
            intersect.Position=new Vector2(sx-5f,sy-5f);
        }
    }
    
    
}