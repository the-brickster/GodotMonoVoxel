using Godot;
using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Threading.Tasks;


namespace Goxlap.src.Goxlap
{
    using utils;
    public class VoxelVolume : Spatial
    {
        private int worldHeight { get; set; } = 64;
        private int worldLength { get; set; } = 64;
        private int worldWidth { get; set; } = 64;
        private int CHUNK_SIZE { get; set; } = 64;

        private float voxelSize { get; set; } = 1.0f;
        private ShaderMaterial voxelMat { get; set; }
        private VoxelChunk[] chunkArr;
        public ConcurrentDictionary<System.Numerics.Vector3, VoxelChunk> chunkMap;

        private int CHUNK_X_COUNT { get; }
        private int CHUNK_Y_COUNT { get; }
        private int CHUNK_Z_COUNT { get; }

        public IVoxelDataPopulate populator;
        public IVoxelMesher mesher;
        public ConcurrentQueue<VoxelChunk> chunkCreationQueue { get; } = new ConcurrentQueue<VoxelChunk>();
        public ConcurrentQueue<MeshInstance> meshingQueue { get; } = new ConcurrentQueue<MeshInstance>();
        public Camera cam;
        // public Godot.Collections.Array frustumPlanes;
        public BlockingCollection<VoxelChunk> collection;

        //----------------------
        //Begin Frustum methods
        int width;
        int height;
        BoundingRect ViewportRect;
        //Combined Projection and View Matrix
        System.Numerics.Matrix4x4 comboMatrix;
        System.Numerics.Matrix4x4 projMatrix;
        System.Numerics.Matrix4x4 viewMatrix;
        System.Numerics.Matrix4x4 inverseMat;
        float fovInDegrees;
        float near;
        float far;
        float imageAspectRatio;
        float b = 0, t = 0, l = 0, r = 0;// l - left, r - right, b - bottom, t - top
        public Godot.Plane[] frustumPlanes = new Godot.Plane[6];

        public BoundSphere camSphere = new BoundSphere(new System.Numerics.Vector3(0f, 0f, 0f), 10f);
        //----------------------

        public VoxelVolume(int height, int length, int width, int CHUNK_SIZE, float voxelSize, string materialLoc, IVoxelDataPopulate populator, Camera cam)
        {
            worldHeight = height;
            worldLength = length;
            worldWidth = width;
            this.CHUNK_SIZE = CHUNK_SIZE;
            this.voxelSize = voxelSize;
            VoxelConstants.CHUNK_SIZE = CHUNK_SIZE;
            VoxelConstants.CHUNK_SIZE_MAX = CHUNK_SIZE * CHUNK_SIZE * CHUNK_SIZE;
            VoxelConstants.VOX_SIZE = voxelSize;

            voxelMat = new ShaderMaterial();
            voxelMat.SetShader(ResourceLoader.Load(@"res://assets/shaders/splatvoxel.shader") as Shader);

            this.CHUNK_X_COUNT = worldWidth / CHUNK_SIZE;
            this.CHUNK_Y_COUNT = worldHeight / CHUNK_SIZE;
            this.CHUNK_Z_COUNT = worldLength / CHUNK_SIZE;

            VoxelConstants.WORLD_SIZE_MAX_X = worldWidth;
            VoxelConstants.WORLD_SIZE_MAX_Y = worldHeight;
            VoxelConstants.WORLD_SIZE_MAX_Z = worldLength;

            chunkArr = new VoxelChunk[CHUNK_X_COUNT * CHUNK_Y_COUNT * CHUNK_Z_COUNT];
            chunkMap = new ConcurrentDictionary<System.Numerics.Vector3, VoxelChunk>();
            collection = new BlockingCollection<VoxelChunk>();

            this.cam = cam;
            //TODO: Change this to allow for paging of voxel data
            for (int i = 0; i < chunkArr.Length; i++)
            {
                var x = i % CHUNK_X_COUNT;
                var y = (i / CHUNK_X_COUNT) % CHUNK_Y_COUNT;
                var z = i / (CHUNK_X_COUNT * CHUNK_Y_COUNT);

                var chunk = new VoxelChunk(x, y, z);
                chunkArr[i] = chunk;
                chunkMap.TryAdd(new System.Numerics.Vector3(x, y, z), chunk);
            }
            this.populator = populator;

            //Set out point voxel mesher shader params.

            mesher = new PointVoxelMesher(voxelMat);
        }
        public void ScreenResChanged()
        {

            var screenRes = GetViewport().Size;
            var screenPos = GetViewport().GetVisibleRect().Position;
            GD.Print($"Screen Resolution Changed {screenRes}, screen position {screenPos}");
            voxelMat.SetShaderParam("screen_size", screenRes);
            voxelMat.SetShaderParam("viewport_pos", screenPos);
        }

        public async override void _Ready()
        {
            width = (int)this.GetViewport().Size.x;
            height = (int)this.GetViewport().Size.y;
            ViewportRect.x = 0;ViewportRect.y=0;
            ViewportRect.extent = this.GetViewport().Size.toNumericVector2();
            var screenRes = GetViewport().Size;
            var screenPos = GetViewport().GetVisibleRect().Position;
            voxelMat.SetShaderParam("albedo", new Color(0, 0, 1, 1));
            voxelMat.SetShaderParam("screen_size", screenRes);
            voxelMat.SetShaderParam("viewport_pos", screenPos);
            voxelMat.SetShaderParam("voxelSize", voxelSize / 2.0f);
            GetViewport().Connect("size_changed", this, nameof(ScreenResChanged));
            await ChunkDataPopulate();
            await ChunkMeshCreation();
            Console.WriteLine(cam.VOffset + " " + cam.HOffset);

            SetupProjData();
        }

        //  // Called every frame. 'delta' is the elapsed time since the previous frame.
        public async override void _Process(float delta)
        {
            this.camSphere.position.X = cam.GlobalTransform.origin.x;
            this.camSphere.position.Y = cam.GlobalTransform.origin.y;
            this.camSphere.position.Z = cam.GlobalTransform.origin.z;
            while (!meshingQueue.IsEmpty)
            {
                // Console.WriteLine("Called");
                MeshInstance m;
                meshingQueue.TryDequeue(out m);
                this.AddChild(m);
                cam.GetFrustum();
                // Console.WriteLine("{0}", this.GetChildCount());
            }

            if (collection.Count > 0)
            {
                // gluPerspective(ref fovInDegrees, ref imageAspectRatio, ref near, ref far, ref b, ref t, ref l, ref r);
                // glFrustum(ref b, ref t, ref l, ref r, ref near, ref far, ref projMatrix);
                // gluPerspective(ref projMatrix,ref fovInDegrees,imageAspectRatio,near,far, cam.KeepAspect == Camera.KeepAspectEnum.Width);
                // createViewMatrix(cam, ref viewMatrix);
                // System.Numerics.Matrix4x4.Invert(viewMatrix,out inverseMat);
                // comboMatrix = projMatrix.multiplyColMaj(viewMatrix);

                // extractGLPlanes(ref frustumPlanes, cam, ref comboMatrix,near,far, false);
                Godot.Collections.Array frustumP = cam.GetFrustum();
                foreach (VoxelChunk chunk in collection)
                {

                    utils.AABB box = chunk.boundingBox;
                    
                    // int res = VoxelVolume.boxInFrustum(cam.GetFrustum(), box);
                    bool res = VoxelVolume.insideFrustum(frustumP, box) || VoxelVolume.intersectsFrustum(frustumP,box) ||this.camSphere.intersectsAABB(box);
                    // Console.WriteLine(chunk.mesh.GetSurfaceMaterial(0));
                    var m = chunk.mesh.MaterialOverride as ShaderMaterial;
                    // Console.WriteLine("-------------------------------");

                    
                    //Blue not shown
                    m.SetShaderParam("albedo", new Color(0, 0, 1, 1));
                    if (res)
                    {
                        //Red shown
                        m.SetShaderParam("albedo", new Color(1, 0, 0, 1));
                        Stopwatch stopwatch = Stopwatch.StartNew();
                        stopwatch.Start();
                        BoundingRect rect = AABB.AABBtoScreenRect(box, cam);
                        
                        stopwatch.Stop();
                        bool isInView = BoundingRect.IntersectsRect(ViewportRect,rect);
                        // Console.WriteLine("Bounding Rect: {0}, completed in {1}ms, intersects viewport {2}", rect, stopwatch.ElapsedMilliseconds,isInView);
                    }



                    // Console.WriteLine("Chunk AABB: {0}, frustum val: {1}",box.min,res);
                }

            }


        }
        public static bool insideFrustum(Godot.Collections.Array planes, utils.AABB box)
        {
            Vector3 half_extents = box.size.toGDVector3() * 0.5f;
            Vector3 ofs = box.center.toGDVector3();

            for (int i = 0; i < planes.Count; i++)
            {
                Plane p = (Plane)planes[i];
                Vector3 point = new Vector3(
                (p.Normal.x <= 0) ? -half_extents.x : half_extents.x,
                (p.Normal.y <= 0) ? -half_extents.y : half_extents.y,
                (p.Normal.z <= 0) ? -half_extents.z : half_extents.z);
                point += ofs;
                if (p.Normal.Dot(point) > p.D)
                {
                    return false;
                }
            }
            return true;
        }
        public static bool intersectsFrustum(Godot.Collections.Array planes, utils.AABB box)
        {
            Vector3 half_extents = box.size.toGDVector3() * 0.5f;
            Vector3 ofs = box.center.toGDVector3();

            for (int i = 0; i < planes.Count; i++)
            {
                Plane p = (Plane)planes[i];
                Vector3 point = new Vector3(
                (p.Normal.x > 0) ? -half_extents.x : half_extents.x,
                (p.Normal.y > 0) ? -half_extents.y : half_extents.y,
                (p.Normal.z > 0) ? -half_extents.z : half_extents.z);
                point += ofs;
                if (p.Normal.Dot(point) > (p.D * 100f))
                {
                    return false;
                }
            }

            return true;
        }
        //We assume all sides of the AABB box are equal and thus only take the side length
        //https://gist.github.com/Kinwailo/d9a07f98d8511206182e50acda4fbc9b
        // Returns: INTERSECT : 0 
        //          INSIDE : 1 
        //          OUTSIDE : 2 
        public static int boxInFrustum(Godot.Collections.Array frustum, utils.AABB box)
        {
            int ret = 1;
            System.Numerics.Vector3 mins = box.min, maxs = box.max;
            System.Numerics.Vector3 radius = box.size / 2.0f;
            System.Numerics.Vector3 center = box.center;
            System.Numerics.Plane plane = new System.Numerics.Plane();
            float distance;
            for (int i = 0; i < 6; ++i)
            {
                var tmp = (Plane)frustum[i];
                plane.D = tmp.D;

                plane.Normal = utils.GDExtension.toNumericVector3(tmp.Normal);
                // plane.Normal = System.Numerics.Vector3.Negate(plane.Normal);


                distance = System.Numerics.Vector3.Dot(plane.Normal, center) + plane.D;
                // Console.WriteLine("Norm: {0}, D:{1}",plane.Normal,plane.D);
                if (distance < -radius.X)
                {
                    return 2;
                }
                if ((float)Math.Abs(distance) < radius.X)
                {
                    return 0;
                }
                // Console.WriteLine("Norm: {0}, D:{1}, Loc:{2}",plane.Normal,plane.D,plane.Center);
                // // X axis 
                // if(plane.Normal.x > 0){
                //     vmin.x = mins.x;
                //     vmax.x = maxs.x;
                // }else{
                //     vmin.x = maxs.x; 
                //     vmax.x = mins.x; 
                // }
                // // Y axis 
                // if(plane.Normal.y > 0) { 
                //     vmin.y = mins.y; 
                //     vmax.y = maxs.y; 
                // } else { 
                //     vmin.y = maxs.y; 
                //     vmax.y = mins.y; 
                // } 
                // // Z axis 
                // if(plane.Normal.z > 0) { 
                //     vmin.z = mins.z; 
                //     vmax.z = maxs.z; 
                // } else { 
                //     vmin.z = maxs.z; 
                //     vmax.z = mins.z; 
                // } 
                // if(plane.Normal.Dot(vmin)+plane.D > 0){
                //     return 2;
                // }
                // if(plane.Normal.Dot(vmax)+plane.D >=0){
                //     ret = 0;
                // }
            }

            return ret;
        }
        public async Task<bool> ChunkDataPopulate()
        {
            return await Task.Run(() =>
            {
                List<Color> colors = new List<Color>();
                for (int i = 0; i < chunkArr.Length; i++)
                {
                    populator.PopulateVoxelData(ref chunkArr[i], colors);
                    SetChunkNeighbors(ref chunkArr[i]);
                    // chunkCreationQueue.Enqueue(chunkArr[i]);
                    // Console.WriteLine("Complete Populating voxel: {0}", chunkArr[i]);
                }
                Console.WriteLine("Beginning color copying: {0}", colors.Count);
                var pointMesher = mesher as PointVoxelMesher;
                Color[] colorArr = colors.ToArray();

                pointMesher.SetVoxelTypes(ref colorArr);
                Console.WriteLine("Complete!");

                return true;
            });
        }
        public void SetChunkNeighbors(ref VoxelChunk chunk)
        {
            System.Numerics.Vector3 top = new System.Numerics.Vector3(chunk.DX, chunk.DY + 1, chunk.DZ);
            System.Numerics.Vector3 bottom = new System.Numerics.Vector3(chunk.DX, chunk.DY - 1, chunk.DZ);
            System.Numerics.Vector3 left = new System.Numerics.Vector3(chunk.DX - 1, chunk.DY, chunk.DZ);
            System.Numerics.Vector3 right = new System.Numerics.Vector3(chunk.DX + 1, chunk.DY, chunk.DZ);
            System.Numerics.Vector3 front = new System.Numerics.Vector3(chunk.DX, chunk.DY, chunk.DZ - 1);
            System.Numerics.Vector3 back = new System.Numerics.Vector3(chunk.DX, chunk.DY, chunk.DZ + 1);
            if (this.chunkMap.ContainsKey(top))
            {
                chunk.neighbors[0] = chunkMap[top];
            }
            if (this.chunkMap.ContainsKey(bottom))
            {
                chunk.neighbors[1] = chunkMap[bottom];
            }
            if (this.chunkMap.ContainsKey(left))
            {
                chunk.neighbors[2] = chunkMap[left];
            }
            if (this.chunkMap.ContainsKey(right))
            {
                chunk.neighbors[3] = chunkMap[right];
            }
            if (this.chunkMap.ContainsKey(front))
            {
                chunk.neighbors[4] = chunkMap[front];
            }
            if (this.chunkMap.ContainsKey(back))
            {
                chunk.neighbors[5] = chunkMap[back];
            }
        }
        public async Task<bool> ChunkMeshCreation()
        {
            return await Task.Run(() =>
            {
                for (int i = 0; i < chunkArr.Length; i++)
                {
                    Stopwatch sw = Stopwatch.StartNew();
                    MeshInstance m = mesher.CreateChunkMesh(ref chunkArr[i]);
                    sw.Stop();
                    if (sw.ElapsedMilliseconds > 0)
                        Console.WriteLine("Time taken: {0}ms, {1}", sw.ElapsedMilliseconds, chunkArr[i]);
                    if (m != null)
                    {
                        CubeMesh c = new CubeMesh();

                        c.Size = chunkArr[i].boundingBox.size.toGDVector3();
                        MeshInstance mesh = new MeshInstance();
                        mesh.Mesh = c;
                        mesh.Translation = chunkArr[i].boundingBox.center.toGDVector3();
                        // Console.WriteLine("Mesh AABB {0}",mesh.GetAabb().Position);
                        // Console.WriteLine("AABB custom {0}", chunkArr[i].boundingBox.center.toGDVector3());
                        meshingQueue.Enqueue(m);
                        meshingQueue.Enqueue(mesh);
                        this.collection.Add(chunkArr[i]);
                    }

                }
                return true;
            });
        }
        public void SetupProjData()
        {

            comboMatrix = new System.Numerics.Matrix4x4();
            fovInDegrees = 80;
            near = cam.Near;
            far = cam.Far;
            imageAspectRatio = width / (float)height;
            Console.WriteLine("FOV: {0}, NEAR: {1}, FAR {2}, Aspect Ratio: {3}, Width {4}, Height {5}", fovInDegrees, near, far, imageAspectRatio, width, height);
            // gluPerspective(ref fovInDegrees, ref imageAspectRatio, ref near, ref far, ref b, ref t, ref l, ref r);
            // glFrustum(ref b, ref t, ref l, ref r, ref near, ref far, ref comboMatrix);
            // glFrustum(ref b, ref t, ref l, ref r, ref near, ref far, ref projMatrix);
            gluPerspective(ref projMatrix,ref fovInDegrees,imageAspectRatio,near,far,cam.KeepAspect == Camera.KeepAspectEnum.Width);
            createViewMatrix(cam, ref viewMatrix);
            // System.Numerics.Matrix4x4.Invert(viewMatrix,out inverseMat);
            comboMatrix = viewMatrix.multiplyColMaj(projMatrix);
            Console.WriteLine("Proj Mat: {0}",projMatrix);
            extractGLPlanes(ref frustumPlanes, cam, ref comboMatrix,near,far, true);
            Console.WriteLine("Custom Planes:");
            foreach (var item in frustumPlanes)
            {
                Plane p = (Plane)item;
                Console.WriteLine("Plane: {0}", p);
            }
            System.Numerics.Matrix4x4 matTest = new System.Numerics.Matrix4x4(1,2,1,1,0,1,0,1,2,3,4,1,0,0,0,1);
            System.Numerics.Vector3 vecTest = new System.Numerics.Vector3(1,2,-11);
            System.Numerics.Vector3 result = GDExtension.TransfromColMaj(vecTest,matTest);
            Console.WriteLine($"Test transform: {result}");
            Vector3 vecTest2 = new Vector3(1,0,-11);
            Vector2 result2 = cam.UnprojectPosition(vecTest2);

            Vector2 result1 = world2Screen(cam,vecTest2,ref viewMatrix,ref projMatrix,width,height);
            Console.WriteLine($"Godot Vector Projection {result2}, My Vector Projection {result1}, Width: {width}, height: {height}");
        }
        public static Vector2 world2Screen(Camera cam,Vector3 pos, ref System.Numerics.Matrix4x4 viewMat, ref System.Numerics.Matrix4x4 projMatrix, int screenWidth, int screenHeight){
            // Vector3 tmp = pos.toGDVector3();
            
            // System.Numerics.Vector3 newPos = cam.GetCameraTransform().XformInv(tmp).toNumericVector3();
            // newPos = GDExtension.TransfromColMaj(newPos,projMatrix);
            System.Numerics.Vector3 newPos = cam.GetCameraTransform().Xform(pos).toNumericVector3();
            newPos = GDExtension.TransfromColMaj(newPos,projMatrix);
            
            newPos.X = (newPos.X * 0.5f + 0.5f) * screenWidth;
            newPos.Y = (-newPos.Y * 0.5f + 0.5f) * screenHeight;
            return new Vector2(newPos.X,newPos.Y);
        }
        public static void createViewMatrix(Camera camera, ref System.Numerics.Matrix4x4 viewMat)
        {
            viewMat = System.Numerics.Matrix4x4.Identity;
            Basis camBasis = camera.GetCameraTransform().basis;
            Vector3 translation = camera.GetCameraTransform().origin;
            Vector3 col0 = camBasis.Column0;
            Vector3 col1 = camBasis.Column1;
            Vector3 col2 = camBasis.Column2;

            viewMat.M11 = col0.x; viewMat.M21 = col0.y; viewMat.M31 = col0.z; viewMat.M41 = 0;
            viewMat.M12 = col1.x; viewMat.M22 = col1.y; viewMat.M32 = col1.z; viewMat.M42 = 0;
            viewMat.M13 = col2.x; viewMat.M23 = col2.y; viewMat.M33 = col2.z; viewMat.M43 = 0;
            viewMat.M14 = translation.x; viewMat.M24 = translation.y; viewMat.M34 = translation.z; viewMat.M44 = 1.0f;

        }
        public static void gluPerspective(ref float angleOfView,
            ref float imageAspectRatio,
            ref float n, ref float f,
            ref float b, ref float t, ref float l, ref float r)
        {
            // float scale = Mathf.Tan(angleOfView * Mathf.Pi / 360.0f) * n;
            // r = imageAspectRatio * scale;
            // l = -r;
            // t = scale;
            // b = -t;
            float tangent = Mathf.Tan(angleOfView/2.0f * GDExtension.DEG2RAD);
            float nearHeight = n * tangent;
            float nearWidth = nearHeight * imageAspectRatio;

            l = -nearWidth; r = nearWidth;
            b = -nearHeight; t = nearHeight;
        }
        public static void gluPerspective(ref System.Numerics.Matrix4x4 matrix, ref float p_fovy_degrees, float p_aspect,
        float p_z_near, float p_z_far, bool flip_fov){
            if(flip_fov){
                p_fovy_degrees = get_fovy(p_fovy_degrees,1.0f/p_aspect);
            }
            float sine, cotangent, deltaZ;
	        float radians = p_fovy_degrees / 2.0f * Mathf.Pi / 180.0f;
            deltaZ = p_z_far - p_z_near;
	        sine = Mathf.Sin(radians);
            if ((deltaZ == 0) || (sine == 0) || (p_aspect == 0)) {
		        return;
	        }
	        cotangent = Mathf.Cos(radians) / sine;
            matrix = System.Numerics.Matrix4x4.Identity;

            matrix.M11 = cotangent / p_aspect;
            matrix.M22 = cotangent;
            matrix.M33 = -(p_z_far + p_z_near) / deltaZ;
            matrix.M43 = -1;
            matrix.M34 = -2 * p_z_near * p_z_far / deltaZ;
            matrix.M44 = 0;
        }
        static float get_fovy(float fovx,float aspect){
            return Mathf.Rad2Deg(Mathf.Atan(aspect * Mathf.Tan(Mathf.Deg2Rad(fovx) * 0.5f)) * 2.0f);
        }
        public static void gluFrustum(ref System.Numerics.Matrix4x4 M, float left, float right, float bottom, float top,
        float znear, float zfar){
            float temp, temp2, temp3, temp4;
            temp = 2.0f * znear;
            temp2 = right - left;
            temp3 = top - bottom;
            temp4 = zfar - znear;

            M.M11 = temp / temp2;
            M.M21 = 0;
            M.M31 = 0;
            M.M41 = 0;

            M.M12 = 0;
            M.M22 = temp / temp3;
            M.M32 = 0;
            M.M42 = 0;

            M.M13 = (right + left) / temp2;
            M.M23 = (top + bottom) / temp3;
            M.M33 = (-zfar - znear) / temp4;
            M.M43 = -1;

            M.M14 = 0;
            M.M24 = 0;
            M.M34 = (-temp * zfar) / temp4;
            M.M44 = 0;
        }
        public static void glFrustum(
            ref float b, ref float t, ref float l, ref float r,
            ref float n, ref float f,
            ref System.Numerics.Matrix4x4 M
        )
        {
            Console.WriteLine("---------------Bottom: {0}, Top {1}, Left: {2}, right: {3}, near {4}, far {5}",b,t,l,r,n,f);
            M.M11 = (2 * n) / (r - l);
            M.M21 = 0;
            M.M31 = 0;
            M.M41 = 0;

            M.M12 = 0;
            M.M22 = (2 * n) / (t - b);
            M.M32 = 0;
            M.M42 = 0;

            M.M13 = (r + l) / (r - l);
            M.M23 = (t + b) / (t - b);
            M.M33 = -(f + n) / (f - n);
            M.M43 = -1;

            M.M14 = 0;
            M.M24 = 0;
            M.M34 = (-2 * f * n) / (f - n);
            M.M44 = 0;

        }
        public static void extractGLPlanes(ref Plane[] planes, Camera cam, ref System.Numerics.Matrix4x4 projMatrix,float near,float far, bool normalize)
        {
            float a, b, c, d = 0;

            //Near
            a = projMatrix.M14 + projMatrix.M31;
            b = projMatrix.M24 + projMatrix.M23;
            c = projMatrix.M34 + projMatrix.M33;
            d = projMatrix.M44 + projMatrix.M43;
            planes[0] = new Plane(a, b, c, near);
            // planes[0].Normal = -planes[0].Normal;
            // planes[0].Normalized();
            // cam.GetCameraTransform().xFromPlane(ref planes[0]);

            //Far
            a = projMatrix.M14 - projMatrix.M13;
            b = projMatrix.M24 - projMatrix.M23;
            c = projMatrix.M34 - projMatrix.M33;
            d = projMatrix.M44 - projMatrix.M43;
            planes[1] = new Plane(a, b, c, far);
            // planes[1].Normal = -planes[1].Normal;
            // planes[1].Normalized();
            // cam.GetCameraTransform().xFromPlane(ref planes[1]);


            //Left
            a = projMatrix.M14 + projMatrix.M11;
            b = projMatrix.M24 + projMatrix.M21;
            c = projMatrix.M34 + projMatrix.M31;
            d = projMatrix.M44 + projMatrix.M41;
            planes[2] = new Plane(a, b, c, d);
            // planes[2].Normal = -planes[2].Normal;
            // planes[2].Normalized();
            // cam.GetCameraTransform().xFromPlane(ref planes[2]);

             //Top
            a = projMatrix.M14 - projMatrix.M12;
            b = projMatrix.M24 - projMatrix.M22;
            c = projMatrix.M34 - projMatrix.M32;
            d = projMatrix.M44 - projMatrix.M42;
            planes[3] = new Plane(a, b, c, d);
            // planes[3].Normal = -planes[3].Normal;
            // planes[3].Normalized();
            // cam.GetCameraTransform().xFromPlane(ref planes[3]);

            //Right
            a = projMatrix.M14 - projMatrix.M11;
            b = projMatrix.M24 - projMatrix.M21;
            c = projMatrix.M34 - projMatrix.M31;
            d = projMatrix.M44 - projMatrix.M41;
            planes[4] = new Plane(a, b, c, d);
            // planes[4].Normal = -planes[4].Normal;
            // planes[4].Normalized();
            // cam.GetCameraTransform().xFromPlane(ref planes[4]);

            //Bottom
            a = projMatrix.M14 + projMatrix.M12;
            b = projMatrix.M24 + projMatrix.M22;
            c = projMatrix.M34 + projMatrix.M32;
            d = projMatrix.M44 + projMatrix.M42;
            planes[5] = new Plane(a, b, c, d);
            // planes[5].Normal = -planes[5].Normal;
            // planes[5].Normalized();
            // cam.GetCameraTransform().xFromPlane(ref planes[5]);



            if (normalize)
            {
                planes[0].Normalized();
                planes[1].Normalized();
                planes[2].Normalized();
                planes[3].Normalized();
                planes[4].Normalized();
                planes[5].Normalized();
            }
        }
    }
    class HeightMapPopulator : IVoxelDataPopulate
    {
        private Image heightImage;

        private Image colorImage;
        public HeightMapPopulator(Image colorImage, Image heightImage)
        {
            this.colorImage = colorImage;
            this.heightImage = heightImage;
        }
        public void PopulateVoxelData(ref VoxelChunk chunk, List<Color> colors)
        {
            int DX = chunk.DX;
            int DY = chunk.DY;
            int DZ = chunk.DZ;
            int CHUNK_SIZE = VoxelConstants.CHUNK_SIZE;

            for (int i = 0; i < VoxelConstants.CHUNK_SIZE_MAX; i++)
            {
                var x = i % CHUNK_SIZE;
                var y = (i / CHUNK_SIZE) % CHUNK_SIZE;
                var z = i / (CHUNK_SIZE * CHUNK_SIZE);

                var x1 = x + (CHUNK_SIZE * DX);
                var z1 = z + (CHUNK_SIZE * DZ);
                heightImage.Lock();
                Color heightColor = heightImage.GetPixel(x1, z1);
                heightImage.Unlock();
                int height = (int)Mathf.Lerp(0, VoxelConstants.WORLD_SIZE_MAX_Y, heightColor.g);

                if ((y + (DY * CHUNK_SIZE)) <= height)
                {
                    colorImage.Lock();
                    Color colorImageColor = colorImage.GetPixel(x1, z1);
                    colorImage.Unlock();
                    if (!colors.Contains(colorImageColor))
                    {
                        colors.Add(colorImageColor);
                    }
                    byte b = (byte)colors.IndexOf(colorImageColor);
                    chunk.set(x, y, z, b);
                    // if ((x + y + z) % 2 == 0)
                    // {
                    //     chunk.set(x, y, z, 1);
                    // }
                    // else
                    // {
                    //     chunk.set(x, y, z, 2);
                    // }
                    chunk.need_update = true;
                }
                else
                {
                    chunk.set(x, y, z, 0);
                }

            }
            // Console.WriteLine("Complete chunk population: {0},{1},{2}", DX, DY, DZ);
        }
    }
    class BasicPopulator : IVoxelDataPopulate
    {
        public void PopulateVoxelData(ref VoxelChunk chunk, List<Color> colors)
        {
            int DX = chunk.DX;
            int DY = chunk.DY;
            int DZ = chunk.DZ;
            int CHUNK_SIZE = VoxelConstants.CHUNK_SIZE;
            var noise = new FastNoise(1337);
            noise.SetInterp(FastNoise.Interp.Hermite);
            for (int i = 0; i < VoxelConstants.CHUNK_SIZE_MAX; i++)
            {
                var x = i % CHUNK_SIZE;
                var y = (i / CHUNK_SIZE) % CHUNK_SIZE;
                var z = i / (CHUNK_SIZE * CHUNK_SIZE);

                int height = (int)Mathf.Lerp(0, VoxelConstants.WORLD_SIZE_MAX_Y, noise.GetPerlinFractal(x + (CHUNK_SIZE * DX), z + (CHUNK_SIZE * DZ)));
                if ((y + (DY * CHUNK_SIZE)) <= height)
                {
                    if ((x + y + z) % 2 == 0)
                    {
                        chunk.set(x, y, z, 1);
                    }
                    else
                    {
                        chunk.set(x, y, z, 2);
                    }
                    chunk.need_update = true;
                }
                else
                {
                    chunk.set(x, y, z, 0);
                }

            }
            // Console.WriteLine("Complete chunk population: {0},{1},{2}", DX, DY, DZ);
        }
    }

    public interface IVoxelDataPopulate
    {

        void PopulateVoxelData(ref VoxelChunk chunk, List<Color> colors);
    }
}
