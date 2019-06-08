using Godot;
using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Threading.Tasks;

namespace Goxlap.src.Goxlap
{
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

        public VoxelVolume(int height, int length, int width, int CHUNK_SIZE, float voxelSize, string materialLoc, IVoxelDataPopulate populator)
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
            voxelMat.SetShaderParam("screen_size", screenRes);
            voxelMat.SetShaderParam("viewport_pos", screenPos);
            voxelMat.SetShaderParam("voxelSize", voxelSize);
            GetViewport().Connect("size_changed", this, nameof(ScreenResChanged));
            await ChunkDataPopulate();
            await ChunkMeshCreation();
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
                // Console.WriteLine("{0}", this.GetChildCount());
            }
        }

        public async Task<bool> ChunkDataPopulate()
        {
            return await Task.Run(() =>
            {
                for (int i = 0; i < chunkArr.Length; i++)
                {
                    populator.PopulateVoxelData(ref chunkArr[i]);
                    SetChunkNeighbors(ref chunkArr[i]);
                    chunkCreationQueue.Enqueue(chunkArr[i]);
                    // Console.WriteLine("Complete Populating voxel: {0}", chunkArr[i]);
                }
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
                        meshingQueue.Enqueue(m);
                }
                return true;
            });
        }
    }
    class BasicPopulator : IVoxelDataPopulate
    {
        public void PopulateVoxelData(ref VoxelChunk chunk)
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

                int height = (int)Mathf.Lerp(0, VoxelConstants.WORLD_SIZE_MAX_Y, noise.GetPerlinFractal(x, z));
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
            Console.WriteLine("Complete chunk population: {0},{1},{2}", DX, DY, DZ);
        }
    }

    public interface IVoxelDataPopulate
    {

        void PopulateVoxelData(ref VoxelChunk chunk);
    }
}
