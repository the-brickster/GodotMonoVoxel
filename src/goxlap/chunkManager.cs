using System;
using System.Threading;
using System.Threading.Tasks;
using Godot;
using Snappy;
using System.Collections;
using System.Collections.Concurrent;
using FPSGame.src.goxlap;

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
        public BlockMesher mesher;

        private GodotTaskScheduler taskMan;

        public ChunkManager(int world_x_size, int world_y_size, int world_z_size,
        int chunk_size, float vox_size, out ChunkStruct[,,] chunkArr, ref GodotTaskScheduler manager)
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
            mesher = new BlockMesher(CHUNK_X_COUNT, CHUNK_Y_COUNT, CHUNK_Z_COUNT, CHUNK_SIZE, VOX_SIZE);
            this.taskMan = manager;
        }

        public async Task Initialize()
        {

            var isReady = false;
            Console.WriteLine("Curr Thread ID: " + System.Threading.Thread.CurrentThread.ManagedThreadId);
            isReady = await Task.Run(() => AsyncVoxelDataInitialize());
            if (isReady)
            {
                isReady = false;
                Console.WriteLine("Checking done");
                
                isReady = await Task.Factory.StartNew(CreateMeshs,TaskCreationOptions.LongRunning);
                Console.WriteLine("Checking done...2");
            }
        }
        public bool AsyncVoxelDataInitialize()
        {
            System.Threading.Thread.CurrentThread.IsBackground = true;
            int chunkDataCounter = 0;
            Console.WriteLine("Curr Thread ID: " + System.Threading.Thread.CurrentThread.ManagedThreadId + " Total MEM Usage: " + GC.GetTotalMemory(true));

            for (int i = 0; i < CHUNK_X_COUNT; i++)
            {
                for (int j = 0; j < CHUNK_Y_COUNT; j++)
                {
                    for (int k = 0; k < CHUNK_Z_COUNT; k++)
                    {
                        chunksList[i, j, k] = new ChunkStruct(CHUNK_SIZE, VOX_SIZE, i, j, k);
                        chunksList[i, j, k].initializedVoxelData();
                        chunksList[i, j, k].compChunkData = SnappyCodec.Compress(chunksList[i, j, k].chunkData);
                        chunksList[i, j, k].chunkData = new byte[1];
                        chunkDataCounter += System.Runtime.InteropServices.Marshal.SizeOf(chunksList[i, j, k]);
                    }
                }
            }
            Console.WriteLine("Curr Thread ID: " + System.Threading.Thread.CurrentThread.ManagedThreadId + " Total MEM Usage: " + GC.GetTotalMemory(true));
            // Console.WriteLine("Completed, size of one chunk object: "+System.Runtime.InteropServices.Marshal.SizeOf(chunksList[1,1,1])+
            // " "+CHUNK_X_COUNT+" "+CHUNK_Y_COUNT+" "+CHUNK_Z_COUNT+"\n Total Chunk obj size: "+chunkDataCounter);
            //chunksList[0, 0, 0].chunkData = SnappyCodec.Uncompress(chunksList[0, 0, 0].compChunkData);
            // Console.WriteLine("Compressed size: "+chunksList[0,0,0].compChunkData.Length);
            // Console.WriteLine("UnCompressed size: "+chunksList[0,0,0].chunkData.Length);
            Console.WriteLine(DateTime.Now.ToString("yyyyMMdd HH:mm:ss:ffff"));
            return true;
        }

        public void update(float delta)
        {

        }

        public bool CreateMeshs()
        {
            System.Threading.Thread.CurrentThread.IsBackground = true;
            for (int i = 0; i < CHUNK_X_COUNT; i++)
            {
                for (int j = 0; j < CHUNK_Y_COUNT; j++)
                {
                    for (int k = 0; k < CHUNK_Z_COUNT; k++)
                    {
                        //createChunkMesh(ref this.chunksList[i,j,k]);
                        MeshInstance mesh = this.mesher.createChunkMesh(ref this.chunksList[i, j, k]);
                        this.chunkQueue.Enqueue(mesh);
                    }
                }
            }
            return true;
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
        public volatile bool currentlyWorked;
        public byte[] chunkData;
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
            currentlyWorked = false;
        }

        public void initializedVoxelData()
        {

            for (int i = 0; i < CHUNK_SIZE; i++)
            {

                for (int j = 0; j < CHUNK_SIZE; j++)
                {

                    for (int k = 0; k < CHUNK_SIZE; k++)
                    {
                        // if(j <= 10){
                        this[i, j, k] = (byte)random.Next(2);

                        // }
                        // else{
                        //     chunkData[i][j][k] = int.Air;
                        // }
                    }
                }
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