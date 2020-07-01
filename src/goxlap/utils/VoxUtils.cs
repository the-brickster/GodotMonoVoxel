using System;
using System.Numerics;
using System.Runtime.CompilerServices;


namespace Goxlap.src.Goxlap.utils
{
    public struct BoundingRect{
        public Vector2 loc;
        public Vector2 extent;

        public float x{
            get {return loc.X;}
            set {loc.X = x;}
        }
        public float y{
            get {return loc.Y;}
            set {loc.Y = y;}
        }
        public float width{
            get {return extent.X;}
            set {extent.X = width;}
        }
        public float height{
            get {return extent.Y;}
            set {extent.Y = height;}
        }
        public BoundingRect(Vector2 loc,Vector2 extent){
            this.loc = loc;
            this.extent =  Vector2.Abs(loc - extent);
        }
        public BoundingRect(float x, float y, float width, float height){
            this.loc.X = x; this.loc.Y = y;
            this.extent.X = width; this.extent.Y = height;
        }
        public static bool IntersectsRect(BoundingRect rect1, BoundingRect rect2){
            return (rect1.x < rect2.x + rect2.width &&
                    rect1.x + rect1.width > rect2.x &&
                    rect1.y < rect2.y + rect2.height &&
                    rect1.y + rect1.height > rect2.y);
        }
        public override string ToString(){
            return "X: "+x+", Y: "+y+", Width: "+extent.X+", Height: "+extent.Y;
        }
    }
    public struct BoundSphere{
        public Vector3 position;
        public float radius;
        public BoundSphere(Vector3 position, float radius){
            this.position = position;
            this.radius = radius;
        }
        
        public bool intersectsSphere(BoundSphere other){
            float distance = Vector3.Distance(this.position,other.position);
            return distance < (this.radius+other.radius);
        }
        public bool intersectsAABB(AABB box){
            // get box closest point to sphere center by clamping
            Vector3 closPoint = Vector3.Max(box.min,Vector3.Min(this.position,box.max));
            
            // this is the same as isPointInsideSphere
            float distance = Vector3.Distance(closPoint,position);
            return distance < radius;
        }
        
    }
    public struct AABB
    {
        public Vector3 min;
        public Vector3 max;
        public Vector3 size;
        public Vector3 center;

        public AABB(Vector3 min, Vector3 max){
            this.min = min;
            this.max = max;
            this.size = max - min;
            this.size.X = Math.Abs(this.size.X);
            this.size.Y = Math.Abs(this.size.Y);
            this.size.Z = Math.Abs(this.size.Z);
            this.center = min+(size/2.0f);
        }
        public bool isPointInsideAABB(Vector3 point)
        {
            return (point.X >= min.X && point.X <= max.X) &&
            (point.Y >= min.Y && point.Y <= max.Y) &&
            (point.Z >= min.Z && point.Z <= max.Z);
        }
        public bool intersectAABB(AABB a)
        {
            return (min.X <= a.max.X && max.X >= a.min.X) &&
            (min.Y <= a.max.Y && max.Y >= a.min.Y) &&
            (min.Z <= a.max.Z && max.Z >= a.min.Z);
        }
        public static unsafe BoundingRect AABBtoScreenRect(AABB box, Godot.Camera cam){
            Vector2 origin = cam.UnprojectPosition(box.min.toGDVector3()).toNumericVector2();
            Vector2 extent = cam.UnprojectPosition(box.max.toGDVector3()).toNumericVector2();
            
            return new BoundingRect(origin,extent);
        }
        

    }
    public static class GDExtension{
        public const float DEG2RAD = 3.141593f / 180;

        public static void gluPerspective(ref System.Numerics.Matrix4x4 matrix, ref float p_fovy_degrees, float p_aspect,
        float p_z_near, float p_z_far, bool flip_fov){
            if(flip_fov){
                p_fovy_degrees = get_fovy(p_fovy_degrees,1.0f/p_aspect);
            }
            float sine, cotangent, deltaZ;
	        float radians = p_fovy_degrees / 2.0f * Godot.Mathf.Pi / 180.0f;
            deltaZ = p_z_far - p_z_near;
	        sine = Godot.Mathf.Sin(radians);
            if ((deltaZ == 0) || (sine == 0) || (p_aspect == 0)) {
		        return;
	        }
	        cotangent = Godot.Mathf.Cos(radians) / sine;
            matrix = System.Numerics.Matrix4x4.Identity;

            matrix.M11 = cotangent / p_aspect;
            matrix.M22 = cotangent;
            matrix.M33 = -(p_z_far + p_z_near) / deltaZ;
            matrix.M43 = -1;
            matrix.M34 = -2 * p_z_near * p_z_far / deltaZ;
            matrix.M44 = 0;
        }
        static float get_fovy(float fovx,float aspect){
            return Godot.Mathf.Rad2Deg(Godot.Mathf.Atan(aspect * Godot.Mathf.Tan(Godot.Mathf.Deg2Rad(fovx) * 0.5f)) * 2.0f);
        }

        public static Vector2 toNumericVector2(this Godot.Vector2 vec){
            return new Vector2(vec.x,vec.y);
        }
        public static Godot.Vector3 toGDVector3(this Vector3 vec3){
            return new Godot.Vector3(vec3.X,vec3.Y,vec3.Z);
        }
        public static Vector3 toNumericVector3(this Godot.Vector3 vec){
            Vector3 val = new Vector3(vec.x,vec.y,vec.z);
            return val;
        }

        /// <summary>
        /// Takes the max of the Vector2 values and returns the resulting vector
        /// </summary>
        /// <param name="curr">The current Vector2</param>
        /// <param name="other">The Vector2 being compared against</param>
        /// <returns>The vector with the max values of the two vectors</returns>
        public static Godot.Vector2 maxLocal(this Godot.Vector2 curr, Godot.Vector2 other){
            curr.x = other.x > curr.x ? other.x : curr.x;
            curr.y = other.y > curr.y ? other.y : curr.y;
            return curr;
        }

        /// <summary>
        /// Takes the min of the Vector2 values and returns the resulting vector
        /// </summary>
        /// <param name="curr">The current Vector2</param>aram>
        /// <param name="other">The Vector2 being compared against</param>param>
        /// <returns>The vector with the max values of the two vectors</returns>
        public static Godot.Vector2 minLocal(this Godot.Vector2 curr, Godot.Vector2 other){
            curr.x = other.x < curr.x ? other.x : curr.x;
            curr.y = other.y < curr.y ? other.y : curr.y;
            return curr;

        }

        /// <summary>
        /// Takes the min of the Vector3 values and returns the resulting vector
        /// </summary>
        /// <param name="curr">The current Vector3</param>
        /// <param name="other">The Vector3 being compared against</param>
        /// <returns>The vector with the min values of the two vectors</returns>
        public static Godot.Vector3 maxLocal(this Godot.Vector3 curr, Godot.Vector3 other){
            curr.x = other.x > curr.x ? other.x : curr.x;
            curr.y = other.y > curr.y ? other.y : curr.y;
            curr.z = other.z > curr.z ? other.z : curr.z;
            return curr;
        }

        /// <summary>
        /// Takes the max of the Vector3 values and returns the resulting vector
        /// </summary>
        /// <param name="curr">The current Vector3</param></param>
        /// <param name="other">The Vector3 being compared against</param>></param>
        /// <returns>The vector with the max values of the two vectors</returns>
        public static Godot.Vector3 minLocal(this Godot.Vector3 curr, Godot.Vector3 other){
            curr.x = other.x < curr.x ? other.x : curr.x;
            curr.y = other.y < curr.y ? other.y : curr.y;
            curr.z = other.z < curr.z ? other.z : curr.z;
            return curr;

        }

        /// <summary>
        /// Transforms a vector by the given matrix.
        /// </summary>
        /// <param name="position">The source vector.</param>
        /// <param name="matrix">The transformation matrix.</param>
        /// <returns>The transformed vector.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3 TransfromColMaj(this Vector3 position, Matrix4x4 matrix){
            return new Vector3(position.X * matrix.M11 + position.Y * matrix.M12 + position.Z * matrix.M13 + matrix.M14,
                position.X * matrix.M21 + position.Y * matrix.M22 + position.Z * matrix.M23 + matrix.M24,
                position.X * matrix.M31 + position.Y * matrix.M32 + position.Z * matrix.M33 + matrix.M34);
        }
        public static Godot.Plane xFromPlane(this Godot.Transform transform, ref Godot.Plane plane){
            Godot.Vector3 point = plane.Normal * plane.D;
            Godot.Vector3 point_dir = point + plane.Normal;
            point = transform.Xform(point);
            point_dir = transform.Xform(point_dir);

            Godot.Vector3 normal = point_dir - point;
            normal.Normalized();
            float d =normal.Dot(point);
            plane.Normal = normal;
            plane.D = d;
            return new Godot.Plane(normal,d);
        }
        public static Matrix4x4 multiplyColMaj(this Matrix4x4 value1, Matrix4x4 value2){
            Matrix4x4 m;
            // First Column
            m.M11 = value1.M11 * value2.M11 + value1.M12 * value2.M21 + value1.M13 * value2.M31 + value1.M14 * value2.M41;
            m.M21 = value1.M21 * value2.M11 + value1.M22 * value2.M21 + value1.M23 * value2.M31 + value1.M24 * value2.M41;
            m.M31 = value1.M31 * value2.M11 + value1.M32 * value2.M21 + value1.M33 * value2.M31 + value1.M34 * value2.M41;
            m.M41 = value1.M41 * value2.M11 + value1.M42 * value2.M21 + value1.M43 * value2.M31 + value1.M44 * value2.M41;

            // Second Column
            m.M12 = value1.M11 * value2.M12 + value1.M12 * value2.M22 + value1.M13 * value2.M32 + value1.M14 * value2.M42;
            m.M22 = value1.M21 * value2.M12 + value1.M22 * value2.M22 + value1.M23 * value2.M32 + value1.M24 * value2.M42;
            m.M32 = value1.M31 * value2.M12 + value1.M32 * value2.M22 + value1.M33 * value2.M32 + value1.M34 * value2.M42;
            m.M42 = value1.M41 * value2.M12 + value1.M42 * value2.M22 + value1.M43 * value2.M32 + value1.M44 * value2.M42;

            // Third Column
            m.M13 = value1.M11 * value2.M13 + value1.M12 * value2.M23 + value1.M13 * value2.M33 + value1.M14 * value2.M43;
            m.M23 = value1.M21 * value2.M13 + value1.M22 * value2.M23 + value1.M23 * value2.M33 + value1.M24 * value2.M43;
            m.M33 = value1.M31 * value2.M13 + value1.M32 * value2.M23 + value1.M33 * value2.M33 + value1.M34 * value2.M43;
            m.M43 = value1.M41 * value2.M13 + value1.M42 * value2.M23 + value1.M43 * value2.M33 + value1.M44 * value2.M43;

            // Fourth Column
            m.M14 = value1.M11 * value2.M14 + value1.M12 * value2.M24 + value1.M13 * value2.M34 + value1.M14 * value2.M44;
            m.M24 = value1.M21 * value2.M14 + value1.M22 * value2.M24 + value1.M23 * value2.M34 + value1.M24 * value2.M44;
            m.M34 = value1.M31 * value2.M14 + value1.M32 * value2.M24 + value1.M33 * value2.M34 + value1.M34 * value2.M44;
            m.M44 = value1.M41 * value2.M14 + value1.M42 * value2.M24 + value1.M43 * value2.M34 + value1.M44 * value2.M44;

            return m;
        }

   
///    <summary>
///     <code>mult</code> multiplies a vector about a rotation matrix and adds
///     translation. The w value is returned as a result of
///     multiplying the last column of the matrix by 1.0  
///     <param name="vec"> 
///                vec to multiply against.
///     </param>
///     <param name="store">
///                a vector to store the result in.
///     </param> 
///     <returns> the W value <returns>
///     </summary>     
        public static float multProj(this Matrix4x4 m, Godot.Vector3 vec, ref Godot.Vector3 store){
            float vx = vec.x, vy = vec.y, vz = vec.z;
            store.x = m.M11 * vx + m.M12 * vy + m.M13 * vz + m.M14;
            store.y = m.M21 * vx + m.M22 * vy + m.M23 * vz + m.M24;
            store.z = m.M31 * vx + m.M32 * vy + m.M33 * vz + m.M34;
            return m.M41 * vx + m.M42 * vy + m.M43 * vz + m.M44;

        }
    }


}