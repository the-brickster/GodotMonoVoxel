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
        uint width;
        uint height;
        //Combined Projection and View Matrix
        System.Numerics.Matrix4x4 comboMatrix;
        System.Numerics.Matrix4x4 projMatrix;
        System.Numerics.Matrix4x4 viewMatrix;
        System.Numerics.Matrix4x4 inverseMat;
        float angleOfView;
        float near;
        float far;
        float imageAspectRatio;
        float b = 0, t = 0, l = 0, r = 0;// l - left, r - right, b - bottom, t - top
        public Godot.Plane[] frustumPlanes = new Godot.Plane[6];
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

                // gluPerspective(ref angleOfView, ref imageAspectRatio, ref near, ref far, ref b, ref t, ref l, ref r);
                // glFrustum(ref b, ref t, ref l, ref r, ref near, ref far, ref projMatrix);
                // createViewMatrix(cam, ref viewMatrix);
                // System.Numerics.Matrix4x4.Invert(viewMatrix,out inverseMat);
                // comboMatrix = projMatrix.multiplyColMaj(viewMatrix);
                // extractGLPlanes(ref frustumPlanes,cam,ref comboMatrix,true);
                foreach (VoxelChunk chunk in collection)
                {
                    utils.AABB box = chunk.boundingBox;
                    // int res = VoxelVolume.boxInFrustum(cam.GetFrustum(), box);
                    bool res = VoxelVolume.insideFrustum(cam.GetFrustum(),box);
                    // Console.WriteLine(chunk.mesh.GetSurfaceMaterial(0));
                    var m = chunk.mesh.MaterialOverride as ShaderMaterial;
                    // Console.WriteLine("-------------------------------");
                    
                    //Red shown
                    m.SetShaderParam("albedo", new Color(1, 0, 0, 1));
                    
                    if (!res)
                    {
                        //Blue not shown
                        m.SetShaderParam("albedo", new Color(0, 0, 1, 1));
                    }



                    // Console.WriteLine("Chunk AABB: {0}, frustum val: {1}",box.min,res);
                }

            }
            

        }
        public static bool insideFrustum(Godot.Collections.Array planes, utils.AABB box){
            Vector3 half_extents = box.size.toGDVector3() * 0.5f;
            Vector3 ofs = box.center.toGDVector3();

            for(int i =0; i < planes.Count;i++){
                Plane p = (Plane)planes[i];
                Vector3 point = new Vector3(
				(p.Normal.x <= 0) ? -half_extents.x : half_extents.x,
				(p.Normal.y <= 0) ? -half_extents.y : half_extents.y,
				(p.Normal.z <= 0) ? -half_extents.z : half_extents.z);
		        point += ofs;
                if(p.Normal.Dot(point) >p.D){
                    return false;
                }
            }

            return true;
        }
        public static bool intersectsFrustum(Godot.Collections.Array planes, utils.AABB box){
            Vector3 half_extents = box.size.toGDVector3() * 0.5f;
            Vector3 ofs = box.center.toGDVector3();

            for(int i =0; i < planes.Count;i++){
                Plane p = (Plane)planes[i];
                Vector3 point = new Vector3(
				(p.Normal.x > 0) ? -half_extents.x : half_extents.x,
				(p.Normal.y > 0) ? -half_extents.y : half_extents.y,
				(p.Normal.z > 0) ? -half_extents.z : half_extents.z);
		        point += ofs;
                if(p.Normal.Dot(point) > (p.D*100f)){
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

                
                distance = System.Numerics.Vector3.Dot(plane.Normal, center)+plane.D;
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
                    // if (sw.ElapsedMilliseconds > 0)
                    //     Console.WriteLine("Time taken: {0}ms, {1}", sw.ElapsedMilliseconds, chunkArr[i]);
                    if (m != null)
                    {
                        CubeMesh c = new CubeMesh();
                        
                        c.Size = chunkArr[i].boundingBox.size.toGDVector3();
                        MeshInstance mesh = new MeshInstance();
                        mesh.Mesh = c;
                        mesh.Translation = chunkArr[i].boundingBox.center.toGDVector3();
                        Console.WriteLine("Mesh AABB {0}",mesh.GetAabb().Position);
                        Console.WriteLine("AABB custom {0}", chunkArr[i].boundingBox.center.toGDVector3());
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
            width = (uint)this.GetViewport().Size.x;
            height = (uint)this.GetViewport().Size.y;
            comboMatrix = new System.Numerics.Matrix4x4();
            angleOfView = 80.0f;
            near = cam.Near;
            far = cam.Far;
            imageAspectRatio = width / (float)height;
            Console.WriteLine("FOV: {0}, NEAR: {1}, FAR {2}, Aspect Ratio: {3}, Width {4}, Height {5}",angleOfView,near,far,imageAspectRatio,width,height);
            gluPerspective(ref angleOfView, ref imageAspectRatio, ref near, ref far, ref b, ref t, ref l, ref r);
            // glFrustum(ref b, ref t, ref l, ref r, ref near, ref far, ref comboMatrix);
            // gluPerspective(ref angleOfView, ref imageAspectRatio, ref near, ref far, ref b, ref t, ref l, ref r);
                glFrustum(ref b, ref t, ref l, ref r, ref near, ref far, ref projMatrix);
                createViewMatrix(cam, ref viewMatrix);
                // System.Numerics.Matrix4x4.Invert(viewMatrix,out inverseMat);
                comboMatrix = projMatrix.multiplyColMaj(viewMatrix);
                extractGLPlanes(ref frustumPlanes,cam,ref comboMatrix,true);
            
        }
        public static void createViewMatrix(Camera camera, ref System.Numerics.Matrix4x4 viewMat){
            Basis camBasis = camera.GlobalTransform.basis;
            Vector3 translation = camera.GlobalTransform.origin;
            Vector3 col0 = camBasis.Column0;
            Vector3 col1 = camBasis.Column1;
            Vector3 col2 = camBasis.Column2;

            viewMat.M11 = col0.x;viewMat.M21 = col0.y; viewMat.M31 = col0.z; viewMat.M41 = 0;
            viewMat.M12 = col1.x;viewMat.M22 = col1.y; viewMat.M32 = col1.z; viewMat.M42 = 0;
            viewMat.M13 = col2.x;viewMat.M23 = col2.y; viewMat.M33 = col2.z; viewMat.M43 = 0;
            viewMat.M14 = translation.x; viewMat.M24=translation.y; viewMat.M34=translation.z; viewMat.M44 = 0;

        }
        public static void gluPerspective(ref float angleOfView,
            ref float imageAspectRatio,
            ref float n, ref float f,
            ref float b, ref float t, ref float l, ref float r)
        {
            float scale = Mathf.Tan(angleOfView * 0.5f * Mathf.Pi / 180) * n;
            r = imageAspectRatio * scale;
            l = -r;
            t = scale;
            b = -t;
        }
        public static void glFrustum(
            ref float b, ref float t, ref float l, ref float r,
            ref float n, ref float f,
            ref System.Numerics.Matrix4x4 M
        )
        {
            M.M11 = 2 * n / (r - 1);
            M.M12 = 0;
            M.M13 = 0;
            M.M14 = 0;

            M.M21 = 0;
            M.M22 = 2 * n / (t - b);
            M.M23 = 0;
            M.M24 = 0;

            M.M31 = (r + l) / (r - l);
            M.M32 = (t + b) / (t - b);
            M.M33 = -(f + n) / (f - n);
            M.M34 = -1;

            M.M41 = 0;
            M.M42 = 0;
            M.M43 = -2 * f * n / (f - n);
            M.M44 = 0;

        }
        public static void extractGLPlanes(ref Plane[] planes, Camera cam, ref System.Numerics.Matrix4x4 projMatrix, bool normalize)
        {
            float a, b, c, d = 0;
            //Left
            a = projMatrix.M41 + projMatrix.M11;
            b = projMatrix.M42 + projMatrix.M12;
            c = projMatrix.M43 + projMatrix.M13;
            d = projMatrix.M44 + projMatrix.M14;
            planes[0] = new Plane(a, b, c, d);
            planes[0].Normal = -planes[0].Normal;
            // planes[0].Normalized();
            // planes[0] = cam.GetCameraTransform().xFromPlane(ref planes[0]);

            //Right
            a = projMatrix.M41 - projMatrix.M11;
            b = projMatrix.M42 - projMatrix.M12;
            c = projMatrix.M43 - projMatrix.M13;
            d = projMatrix.M44 - projMatrix.M14;
            planes[1] = new Plane(a, b, c, d);
            planes[1].Normal = -planes[1].Normal;
            // planes[1].Normalized();
            // planes[1] = cam.GetCameraTransform().xFromPlane(ref planes[1]);

            //Top
            a = projMatrix.M41 - projMatrix.M21;
            b = projMatrix.M42 - projMatrix.M22;
            c = projMatrix.M43 - projMatrix.M23;
            d = projMatrix.M44 - projMatrix.M24;
            planes[2] = new Plane(a, b, c, d);
            planes[2].Normal = -planes[2].Normal;
            // planes[2].Normalized();
            // planes[2] = cam.GetCameraTransform().xFromPlane(ref planes[2]);

            //Bottom
            a = projMatrix.M41 + projMatrix.M21;
            b = projMatrix.M42 + projMatrix.M22;
            c = projMatrix.M43 + projMatrix.M23;
            d = projMatrix.M44 + projMatrix.M24;
            planes[3] = new Plane(a, b, c, d);
            planes[3].Normal = -planes[3].Normal;
            // planes[3].Normalized();
            // planes[3] = cam.GetCameraTransform().xFromPlane(ref planes[3]);

            //Near
            a = projMatrix.M41 + projMatrix.M31;
            b = projMatrix.M42 + projMatrix.M32;
            c = projMatrix.M43 + projMatrix.M33;
            d = projMatrix.M44 + projMatrix.M34;
            planes[4] = new Plane(a, b, c, d);
            planes[4].Normal = -planes[4].Normal;
            // planes[4].Normalized();
            // planes[4] = cam.GetCameraTransform().xFromPlane(ref planes[4]);

            //Far
            a = projMatrix.M41 - projMatrix.M31;
            b = projMatrix.M42 - projMatrix.M32;
            c = projMatrix.M43 - projMatrix.M33;
            d = projMatrix.M44 - projMatrix.M34;
            planes[5] = new Plane(a, b, c, d);
            planes[5].Normal = -planes[5].Normal;
            // planes[5].Normalized();
            // planes[5] = cam.GetCameraTransform().xFromPlane(ref planes[5]);
            

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
            Console.WriteLine("Complete chunk population: {0},{1},{2}", DX, DY, DZ);
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
