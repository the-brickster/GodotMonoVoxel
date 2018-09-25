using System;
using System.Threading;
using System.Threading.Tasks;
using Godot;
using Snappy;
using System.Collections;
using System.Collections.Concurrent;
using FPSGame.src.goxlap;
using System.Diagnostics;

namespace FPSGame.src.Common.goxlap
{
    class ChunkManager
    {
        int World_X_Size;
        int World_Y_Size;
        int World_Z_Size;
        int CHUNK_SIZE;

        public int CHUNK_X_COUNT { get; set; }
        public int CHUNK_Y_COUNT { get; set; }
        public int CHUNK_Z_COUNT { get; set; }
        float VOX_SIZE = 1.0f;
        int VIEW_DISTANCE;
        private Vector3[] vertices;
        private ChunkStruct[,,] chunksList;

        public ConcurrentQueue<MeshInstance> chunkQueue { get; } = new ConcurrentQueue<MeshInstance>();
        public static Random rand = new Random();
        public PointVoxelMesher mesher;


        public ChunkManager(int world_x_size, int world_y_size, int world_z_size,
        int chunk_size, float vox_size, out ChunkStruct[,,] chunkArr,ShaderMaterial s = null)
        {
            this.World_X_Size = world_x_size;
            this.World_Y_Size = world_y_size;
            this.World_Z_Size = world_z_size;
            this.VOX_SIZE = vox_size;
            this.CHUNK_SIZE = chunk_size;

            this.CHUNK_X_COUNT = World_X_Size / CHUNK_SIZE;
            this.CHUNK_Y_COUNT = World_Y_Size / CHUNK_SIZE;
            this.CHUNK_Z_COUNT = World_Z_Size / CHUNK_SIZE;

            chunksList = new ChunkStruct[CHUNK_X_COUNT, CHUNK_Y_COUNT, CHUNK_Z_COUNT];
            chunkArr = chunksList;

            vertices = vertices = new Vector3[]{new Vector3(0,0,0),
                                                new Vector3(VOX_SIZE,0,0),
                                                new Vector3(VOX_SIZE,0,VOX_SIZE),
                                                new Vector3(0,0,VOX_SIZE),

                                                new Vector3(0,VOX_SIZE,0),
                                                new Vector3(VOX_SIZE,VOX_SIZE,0),
                                                new Vector3(VOX_SIZE,VOX_SIZE,VOX_SIZE),
                                                new Vector3(0,VOX_SIZE,VOX_SIZE)};
            mesher = new PointVoxelMesher(CHUNK_X_COUNT, CHUNK_Y_COUNT, CHUNK_Z_COUNT, CHUNK_SIZE, VOX_SIZE,s);
            
        }

        public void update(float delta)
        {

        }

        public bool CreateMeshs()
        {
            // var pOpts = new ParallelOptions();
            
            // pOpts.MaxDegreeOfParallelism = 7;
            
            // // System.Threading.Thread.CurrentThread.IsBackground = true;
            // Parallel.For(0,CHUNK_X_COUNT,pOpts,i=>{
            //     for (int j = 0; j < CHUNK_Y_COUNT; j++)
            //     {
            //         for (int k = 0; k < CHUNK_Z_COUNT; k++)
            //         {
            //             //createChunkMesh(ref this.chunksList[i,j,k]);
            //             // this.chunksList[i,j,k].uncompressVoxData();
            //             // Stopwatch sw = Stopwatch.StartNew();
            //             MeshInstance mesh = this.mesher.CreateChunkMesh(ref this.chunksList[i, j, k]);
            //             // Console.WriteLine("Mesh created in {0}ms ");
            //             // this.chunksList[i,j,k].compressVoxData();
            //             this.chunkQueue.Enqueue(mesh);
                        
            //         }
            //     }
            // });
            CancellationTokenSource source = new CancellationTokenSource();
            CancellationToken cts = source.Token;
            for (int i = 0; i < CHUNK_X_COUNT; i++)
            {
                for (int j = 0; j < CHUNK_Y_COUNT; j++)
                {
                    for (int k = 0; k < CHUNK_Z_COUNT; k++)
                    {
                        //createChunkMesh(ref this.chunksList[i,j,k]);
                        // this.chunksList[i,j,k].uncompressVoxData();
                        // Stopwatch sw = Stopwatch.StartNew();
                        Tuple<int, int, int> t = new Tuple<int, int, int>(i, j, k);
                        Task.Factory.StartNew(action: ActionCreateChunkMesh(t), state: t,cancellationToken:cts,
                        creationOptions:TaskCreationOptions.None,scheduler:QueueTaskSchedWrapper.Instance.GetPriorityQueueScheduler(0));


                    }
                }
            }
            return true;
        }

        private Action<object> ActionCreateChunkMesh(Tuple<int,int,int> value)
        {
            return t =>
            {
                if (t == null)
                {
                    throw new ArgumentNullException(nameof(t));
                }
                
                // Tuple<int, int, int> value = t1;
                // Console.WriteLine("{0},{1},{2}", value.Item1, value.Item2, value.Item3);
                MeshInstance mesh = this.mesher.CreateChunkMesh(ref this.chunksList[value.Item1, value.Item2, value.Item3]);
                this.chunkQueue.Enqueue(mesh);
            };
        }

        /*
         [Obsolete]
                private void CreateChunkMesh(ref ChunkStruct c)
                {
                    c.chunkData = SnappyCodec.Uncompress(c.compChunkData);
                    c.surfaceTool.Begin(Mesh.PrimitiveType.Triangles);
                    for (int i = 0; i < CHUNK_SIZE; i++)
                    {
                        for (int j = 0; j < CHUNK_SIZE; j++)
                        {
                            for (int k = 0; k < CHUNK_SIZE; k++)
                            {
                                if (c[i, j, k] == 0)
                                {
                                    continue;
                                }
                                createFaces(i, j, k, c.Dx, c.Dy, c.Dz, ref c.surfaceTool, ref c);
                            }
                        }
                    }
                    c.surfaceTool.Index();
                    MeshInstance mesh = new MeshInstance();
                    mesh.SetMesh(c.surfaceTool.Commit());
                    this.chunkQueue.Enqueue(mesh);
                    c.surfaceTool.Clear();
                    c.chunkData = new byte[1];
                }
                [Obsolete]
                private void createFaces(int x, int y, int z, int Dx, int Dy, int Dz,
                ref SurfaceTool surfaceTool, ref ChunkStruct c)
                {
                    Vector3 voxPosition = new Vector3((x) * VOX_SIZE, (y) * VOX_SIZE, (z) * VOX_SIZE);
                    voxPosition.x = voxPosition.x + (Dx * CHUNK_SIZE * VOX_SIZE);
                    voxPosition.y = voxPosition.y + (Dy * CHUNK_SIZE * VOX_SIZE);
                    voxPosition.z = voxPosition.z + (Dz * CHUNK_SIZE * VOX_SIZE);
                    if (canCreateFace(x, y - 1, z, ref c))
                    {

                        surfaceTool.AddNormal(new Vector3(0.0f, -1.0f, 0.0f));
                        surfaceTool.AddVertex(vertices[1] + voxPosition);
                        surfaceTool.AddVertex(vertices[3] + voxPosition);
                        surfaceTool.AddVertex(vertices[2] + voxPosition);

                        surfaceTool.AddVertex(vertices[1] + voxPosition);
                        surfaceTool.AddVertex(vertices[0] + voxPosition);
                        surfaceTool.AddVertex(vertices[3] + voxPosition);
                    }
                    if (canCreateFace(x, y + 1, z, ref c))
                    {
                        surfaceTool.AddNormal(new Vector3(0.0f, 1.0f, 0.0f));
                        surfaceTool.AddVertex(vertices[4] + voxPosition);
                        surfaceTool.AddVertex(vertices[5] + voxPosition);
                        surfaceTool.AddVertex(vertices[7] + voxPosition);

                        surfaceTool.AddVertex(vertices[5] + voxPosition);
                        surfaceTool.AddVertex(vertices[6] + voxPosition);
                        surfaceTool.AddVertex(vertices[7] + voxPosition);
                    }
                    if (canCreateFace(x + 1, y, z, ref c))
                    {
                        surfaceTool.AddNormal(new Vector3(1.0f, 0.0f, 0.0f));
                        surfaceTool.AddVertex(vertices[2] + voxPosition);
                        surfaceTool.AddVertex(vertices[5] + voxPosition);
                        surfaceTool.AddVertex(vertices[1] + voxPosition);

                        surfaceTool.AddVertex(vertices[2] + voxPosition);
                        surfaceTool.AddVertex(vertices[6] + voxPosition);
                        surfaceTool.AddVertex(vertices[5] + voxPosition);
                    }
                    if (canCreateFace(x - 1, y, z, ref c))
                    {
                        surfaceTool.AddNormal(new Vector3(-1.0f, 0.0f, 0.0f));
                        surfaceTool.AddVertex(vertices[0] + voxPosition);
                        surfaceTool.AddVertex(vertices[7] + voxPosition);
                        surfaceTool.AddVertex(vertices[3] + voxPosition);

                        surfaceTool.AddVertex(vertices[0] + voxPosition);
                        surfaceTool.AddVertex(vertices[4] + voxPosition);
                        surfaceTool.AddVertex(vertices[7] + voxPosition);
                    }
                    if (canCreateFace(x, y, z + 1, ref c))
                    {
                        surfaceTool.AddNormal(new Vector3(0.0f, 0.0f, 1.0f));

                        surfaceTool.AddVertex(vertices[3] + voxPosition);
                        surfaceTool.AddVertex(vertices[6] + voxPosition);
                        surfaceTool.AddVertex(vertices[2] + voxPosition);

                        surfaceTool.AddVertex(vertices[3] + voxPosition);
                        surfaceTool.AddVertex(vertices[7] + voxPosition);
                        surfaceTool.AddVertex(vertices[6] + voxPosition);
                    }
                    if (canCreateFace(x, y, z - 1, ref c))
                    {
                        surfaceTool.AddNormal(new Vector3(0.0f, 0.0f, -1.0f));
                        surfaceTool.AddVertex(vertices[0] + voxPosition);
                        surfaceTool.AddVertex(vertices[1] + voxPosition);
                        surfaceTool.AddVertex(vertices[5] + voxPosition);

                        surfaceTool.AddVertex(vertices[5] + voxPosition);
                        surfaceTool.AddVertex(vertices[4] + voxPosition);
                        surfaceTool.AddVertex(vertices[0] + voxPosition);
                    }
                }

                private bool canCreateFace(int x, int y, int z, ref ChunkStruct c)
                {
                    if (!isInData(x, y, z))
                    {
                        return true;
                    }
                    else if (c[x, y, z] == (byte)VoxelTypes.Air)
                    {
                        return true;
                    }
                    return false;
                }
                private bool isInData(int x, int y, int z)
                {
                    if (x < 0 || y < 0 || z < 0 || x >= CHUNK_SIZE || y >= CHUNK_SIZE || z >= CHUNK_SIZE)
                    {
                        return false;
                    }
                    return true;
                }
                // private bool isInData(int x, int y, int z, int Dx, int Dy, int Dz){

                // }
         */

    }
    struct ChunkStruct
    {
        public long currentlyWorked;
        public volatile byte[] chunkData;
        public byte[] compChunkData;
        public int CHUNK_SIZE;
        public float VOX_SIZE;
        public int Dx;
        public int Dy;
        public int Dz;
        public SurfaceTool surfaceTool;
        public Random random;


        public ChunkStruct(int chunkSize, float voxSize, int Dx, int Dy, int Dz)
        {
            this.CHUNK_SIZE = chunkSize;
            this.VOX_SIZE = voxSize;
            this.Dx = Dx;
            this.Dy = Dy;
            this.Dz = Dz;
            chunkData = new byte[CHUNK_SIZE * CHUNK_SIZE * CHUNK_SIZE];
            surfaceTool = new SurfaceTool();
            compChunkData = new byte[1];
            random = new Random();
            currentlyWorked = 0;
        }


        public bool uncompressVoxData(){
            //0 -> chunk isn't in use
            //1 -> chunk is in use
            if(0 == Interlocked.Exchange(ref this.currentlyWorked,1)){
                Console.WriteLine("Chunk compressing at thread {0}",System.Threading.Thread.CurrentThread.ManagedThreadId);
                if(this.chunkData.Length  ==1){
                    this.chunkData = SnappyCodec.Uncompress(this.compChunkData);
                }
                
                Interlocked.Exchange(ref this.currentlyWorked,0);
                return true;
            }
            else{
                while(Interlocked.Read(ref this.currentlyWorked) == 1){
                    Console.WriteLine("Waiting on other thread to complete their operation: {0}",System.Threading.Thread.CurrentThread.ManagedThreadId);
                }
                Console.WriteLine("Chunk already uncompressed");
                return false;
            }
        }

        public bool compressVoxData(){
            //0 -> chunk isn't in use
            //1 -> chunk is in use
            if(0 == Interlocked.Exchange(ref this.currentlyWorked,1)){
                if(this.chunkData.Length != 1){
                    this.compChunkData = SnappyCodec.Compress(this.chunkData);
                    this.chunkData = new byte[1];
                }
                
                Interlocked.Exchange(ref this.currentlyWorked,0);
                return true;
            }
            else{
                Console.WriteLine("Chunk already compressed");
                return false;
            }
        }

        // public void initializedVoxelData(){}

        public byte this[int x, int y, int z]
        {
            get
            {
                return chunkData[x + CHUNK_SIZE * (y + CHUNK_SIZE * z)];
            }
            set
            {
                chunkData[x + CHUNK_SIZE * (y + CHUNK_SIZE * z)] = value;
            }
        }



    }
}