using System;
using System.Collections;
using System.Runtime.InteropServices;
using Godot;
using System.Threading.Tasks;
using Snappy;

namespace FPSGame.src.Common.goxlap{
    class VoxelVolume : Node{
        private int worldHeight {get; set;} = 64;
        private int worldLength {get; set;} = 64;
        private int worldWidth {get;set;} = 64;
        private int CHUNK_SIZE;

        private float voxelSize = 1.0f;
        private VoxelTypes[][][] worldData;
        private ChunkStruct[,,] chunks;
        private Queue chunkQueue;

        private float chunkDelay = 0;
        ChunkManager chunkManager;

        IVoxelDataPopulate voxelDataPopulate;
        public VoxelVolume(IVoxelDataPopulate dataPopulate, int height =64,int length =64 , int width = 64,int chunkSize = 16,float voxelSize = 1.0f)
        {
            worldHeight = height;
            worldLength = length;
            worldWidth = width;
            this.voxelSize = voxelSize;
            worldData = new VoxelTypes[worldHeight][][];
            CHUNK_SIZE = chunkSize;
            chunkQueue = new Queue();
            chunkManager = new ChunkManager(worldWidth,worldHeight,worldLength,
            CHUNK_SIZE,this.voxelSize,out chunks);

            this.voxelDataPopulate = dataPopulate;
            this.voxelDataPopulate.CHUNK_SIZE = CHUNK_SIZE;
            this.voxelDataPopulate.VOX_SIZE = this.voxelSize;
            this.voxelDataPopulate.CHUNK_X_COUNT = this.chunkManager.CHUNK_X_COUNT;
            this.voxelDataPopulate.CHUNK_Y_COUNT = this.chunkManager.CHUNK_Y_COUNT;
            this.voxelDataPopulate.CHUNK_Z_COUNT = this.chunkManager.CHUNK_Z_COUNT;
            chunkManager.mesher.volume = this;
        }
        
        public async override void _Ready(){
            Console.WriteLine(DateTime.Now.ToString("yyyyMMdd HH:mm:ss:ffff"));
            await PopulateVoxelData();
            //await chunkManager.Initialize();
            Console.WriteLine(DateTime.Now.ToString("yyyyMMdd HH:mm:ss:ffff"));
        }
        public int this[int x, int y, int z]{
            get{
                // Console.WriteLine(string.Format("{0} {1} {2} {3}", x / CHUNK_SIZE, y / CHUNK_SIZE, z / CHUNK_SIZE,this.chunks.GetLength(2)));
                // Console.WriteLine(string.Format("{0} {1} {2} {3}",x % CHUNK_SIZE, y % CHUNK_SIZE, z % CHUNK_SIZE,this.chunks.GetLength(2)));
                if(x < 0 || y < 0 || z < 0 || x >= worldWidth || y >= worldHeight || z >= worldLength){
                    return (int)VoxelTypes.Air;
                }
                ChunkStruct c = this.chunks[x / CHUNK_SIZE, y / CHUNK_SIZE, z / CHUNK_SIZE];
                if(c.chunkData.Length ==1){
                    c.currentlyWorked = true;
                    c.chunkData = SnappyCodec.Uncompress(c.compChunkData);
                    // c.chunkData = new byte[1];
                    return c[x % CHUNK_SIZE, y % CHUNK_SIZE, z % CHUNK_SIZE];
                }
                return c[x % CHUNK_SIZE, y % CHUNK_SIZE, z % CHUNK_SIZE];
            }
            set {
                this.chunks[x / CHUNK_SIZE, y / CHUNK_SIZE, z / CHUNK_SIZE][x % CHUNK_SIZE, y % CHUNK_SIZE, z % CHUNK_SIZE] = (byte)value;
            }
        }
        public override void _Process(float delta){
            // if(chunkQueue.Count > 0){
            //     this.AddChild((Chunk)chunkQueue.Dequeue());
            //     chunkDelay = 0;
            // }
            // chunkDelay+=delta;
            
            chunkManager.update(delta);
            MeshInstance voxMesh;
            if(chunkManager.chunkQueue.TryDequeue(out voxMesh)){
                this.AddChild(voxMesh);
            }
        }
        public override void _PhysicsProcess(float delta){}

        private async Task PopulateVoxelData()
        {
            Console.WriteLine("Curr Thread ID: " + System.Threading.Thread.CurrentThread.ManagedThreadId + " Total MEM Usage: " + GC.GetTotalMemory(true));
            bool result = await Task.Run(()=>voxelDataPopulate.InitializeVoxelData(chunks));
            if (result) {
                Console.WriteLine("Task completed");
                result = await Task.Run(()=> chunkManager.CreateMeshs());
            }

        }

    }
    class NoisePopulator : IVoxelDataPopulate {
        public int CHUNK_X_COUNT { get; set; }
        public int CHUNK_Y_COUNT { get; set; }
        public int CHUNK_Z_COUNT { get; set; }
        public int CHUNK_SIZE { get; set; }
        public float VOX_SIZE { get; set; }
        public FastNoise noise;
        private Random random = new Random();
        public bool InitializeVoxelData(ChunkStruct[,,] chunkList)
        {
            noise = new FastNoise(random.Next(1337));
            noise.SetNoiseType(FastNoise.NoiseType.Simplex);
            Console.WriteLine("Curr Thread ID: " + System.Threading.Thread.CurrentThread.ManagedThreadId + " Total MEM Usage: " + GC.GetTotalMemory(true));
            for (int i = 0; i < CHUNK_X_COUNT; i++)
            {
                for (int j = 0; j < CHUNK_Y_COUNT; j++)
                {
                    for (int k = 0; k < CHUNK_Z_COUNT; k++)
                    {
                        chunkList[i, j, k] = new ChunkStruct(CHUNK_SIZE, VOX_SIZE, i, j, k);
                        setChunkValues(chunkList[i, j, k]);
                        chunkList[i, j, k].compChunkData = Snappy.SnappyCodec.Compress(chunkList[i, j, k].chunkData);
                        chunkList[i, j, k].chunkData = new byte[1];
                    }
                }
            }

            Console.WriteLine("Curr Thread ID: " + System.Threading.Thread.CurrentThread.ManagedThreadId + " Total MEM Usage: " + GC.GetTotalMemory(true));
            return true;
        }
        private void setChunkValues(ChunkStruct chunk)
        {
            for (int x = 0; x < CHUNK_SIZE; x++)
            {
                for (int z = 0; z < CHUNK_SIZE; z++)
                {
                    //Console.WriteLine(string.Format("DX: {0}, DZ: {1}",chunk.Dx+x,chunk.Dz+z));
                    int height = (int)Mathf.Lerp(1, CHUNK_SIZE*CHUNK_Y_COUNT, noise.GetSimplex(chunk.Dx*CHUNK_SIZE+x,chunk.Dz*CHUNK_SIZE+z));
                    // int height = (int)Mathf.Lerp(1, CHUNK_SIZE*CHUNK_Y_COUNT, 0.25f);
                    for (int y = 0; y < CHUNK_SIZE; y++)
                    {
                        if((chunk.Dy*CHUNK_SIZE+y)<=height)
                        chunk[x, y, z] = 1;
                    }
                }
            }
        }
    }

    class NaiveVoxelDataPopulator : IVoxelDataPopulate
    {
        public int CHUNK_X_COUNT { get; set; }
        public int CHUNK_Y_COUNT { get ; set; }
        public int CHUNK_Z_COUNT { get; set ; }
        public int CHUNK_SIZE { get; set; }
        public float VOX_SIZE { get; set; }

        private Random random = new Random();


        bool IVoxelDataPopulate.InitializeVoxelData(ChunkStruct[,,] chunkList)
        {
            Console.WriteLine("Curr Thread ID: " + System.Threading.Thread.CurrentThread.ManagedThreadId + " Total MEM Usage: " + GC.GetTotalMemory(true));
            for (int i = 0; i < CHUNK_X_COUNT; i++) {
                for (int j = 0; j < CHUNK_Y_COUNT; j++){
                    for (int k = 0; k < CHUNK_Z_COUNT; k++)
                    {
                        chunkList[i,j,k] = new ChunkStruct(CHUNK_SIZE, VOX_SIZE, i, j, k);
                        setChunkValues(chunkList[i, j, k]);
                        chunkList[i, j, k].compChunkData = Snappy.SnappyCodec.Compress(chunkList[i, j, k].chunkData);
                        chunkList[i, j, k].chunkData = new byte[1];
                    }
                }
            }
            
            Console.WriteLine("Curr Thread ID: " + System.Threading.Thread.CurrentThread.ManagedThreadId + " Total MEM Usage: " + GC.GetTotalMemory(true));
            return true;
        }
        private void setChunkValues(ChunkStruct chunk) {
            for (int x = 0; x < CHUNK_SIZE; x++) {
                
                for (int y = 0; y < CHUNK_SIZE; y++) {
                    for (int z = 0; z < CHUNK_SIZE; z++) {
                        
                        chunk[x, y, z] = (byte) VoxelTypes.Default;
                    }
                }
            }
        }
        
    }

    interface IVoxelDataPopulate {
        int CHUNK_X_COUNT { get; set; }
        int CHUNK_Y_COUNT { get; set; }
        int CHUNK_Z_COUNT { get; set; }
        int CHUNK_SIZE { get; set; }
        float VOX_SIZE { get; set; }

        bool InitializeVoxelData(ChunkStruct[,,] chunkList);

    }
}