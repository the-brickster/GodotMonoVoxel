using System;
using Godot;
using FPSGame.src.Common.goxlap;

namespace FPSGame.src.Common.goxlap
{
    [Obsolete]
    public class Chunk:MeshInstance{
        private int CHUNK_SIZE;
        private float VOX_SIZE = 1.0f;
        private VoxelTypes[][][] chunkData;
        private SurfaceTool surfaceTool;
        private Vector3[] vertices;
        private int Dx  {get ; set;}
        private int Dy  {get ; set;}
        private int Dz  {get ; set;}
        private Random rand = new Random();

        public Chunk(int x, int y, int z, int CHUNK_SIZE = 16, float VOX_SIZE = 1.0f){
            Dx = x;
            Dy = y;
            Dz = z;
            this.CHUNK_SIZE = CHUNK_SIZE;
            surfaceTool = new SurfaceTool();
            this.VOX_SIZE = VOX_SIZE;
            vertices = new Vector3[]{new Vector3(0,0,0),
                                                new Vector3(VOX_SIZE,0,0),
                                                new Vector3(VOX_SIZE,0,VOX_SIZE),
                                                new Vector3(0,0,VOX_SIZE),

                                                new Vector3(0,VOX_SIZE,0),
                                                new Vector3(VOX_SIZE,VOX_SIZE,0),
                                                new Vector3(VOX_SIZE,VOX_SIZE,VOX_SIZE),
                                                new Vector3(0,VOX_SIZE,VOX_SIZE)};
            InitializedVoxelData();
            CreateMesh();
        }

        private void InitializedVoxelData(){
            chunkData = new VoxelTypes[CHUNK_SIZE][][];
            for(int i = 0; i < CHUNK_SIZE; i++){
                chunkData[i] = new VoxelTypes[CHUNK_SIZE][];
                for(int j = 0; j < CHUNK_SIZE; j++){
                    chunkData[i][j] = new VoxelTypes[CHUNK_SIZE];
                    for(int k = 0; k < CHUNK_SIZE; k++){
                        // if(j <= 10){
                            chunkData[i][j][k] = VoxelTypes.Default;
                        // }
                        // else{
                        //     chunkData[i][j][k] = VoxelTypes.Air;
                        // }
                    }
                }
            }


        }

        private void CreateMesh(){
            surfaceTool.Begin(Mesh.PrimitiveType.Triangles);

            for(int i = 0; i < CHUNK_SIZE; i++){
                for(int j = 0; j < CHUNK_SIZE; j++){
                    for(int k = 0; k < CHUNK_SIZE; k++){
                        if(chunkData[i][j][k] == VoxelTypes.Air){
                            continue;
                        }
                        CreateFaces(i,j,k);
                    }
                }
            }
            surfaceTool.Index();
            this.SetMesh(surfaceTool.Commit());
        }

        private void CreateFaces(int x, int y, int z)
        {
            Vector3 voxPosition = new Vector3((x) * VOX_SIZE, (y) * VOX_SIZE, (z) * VOX_SIZE);
            voxPosition.x = voxPosition.x + (Dx*CHUNK_SIZE*VOX_SIZE);
            voxPosition.y = voxPosition.y + (Dy*CHUNK_SIZE*VOX_SIZE);
            voxPosition.z = voxPosition.z + (Dz*CHUNK_SIZE*VOX_SIZE);
            if(canCreateFace(x,y-1,z)){
                
                surfaceTool.AddNormal(new Vector3(0.0f, -1.0f, 0.0f));
                surfaceTool.AddVertex(vertices[1]+voxPosition);
                surfaceTool.AddVertex(vertices[3]+voxPosition);
                surfaceTool.AddVertex(vertices[2]+voxPosition);

                surfaceTool.AddVertex(vertices[1]+voxPosition);
                surfaceTool.AddVertex(vertices[0]+voxPosition);
                surfaceTool.AddVertex(vertices[3]+voxPosition);
            }
            if(canCreateFace(x,y+1,z)){
                surfaceTool.AddNormal(new Vector3(0.0f, 1.0f, 0.0f));
                surfaceTool.AddVertex(vertices[4]+voxPosition);
                surfaceTool.AddVertex(vertices[5]+voxPosition);
                surfaceTool.AddVertex(vertices[7]+voxPosition);

                surfaceTool.AddVertex(vertices[5]+voxPosition);
                surfaceTool.AddVertex(vertices[6]+voxPosition);
                surfaceTool.AddVertex(vertices[7]+voxPosition);
            }
            if(canCreateFace(x+1,y,z)){
                surfaceTool.AddNormal(new Vector3(1.0f,0.0f,0.0f));
                surfaceTool.AddVertex(vertices[2]+voxPosition);
                surfaceTool.AddVertex(vertices[5]+voxPosition);
                surfaceTool.AddVertex(vertices[1]+voxPosition);

                surfaceTool.AddVertex(vertices[2]+voxPosition);
                surfaceTool.AddVertex(vertices[6]+voxPosition);
                surfaceTool.AddVertex(vertices[5]+voxPosition);
            }
            if(canCreateFace(x-1,y,z)){
                surfaceTool.AddNormal(new Vector3(-1.0f,0.0f,0.0f));
                surfaceTool.AddVertex(vertices[0]+voxPosition);
                surfaceTool.AddVertex(vertices[7]+voxPosition);
                surfaceTool.AddVertex(vertices[3]+voxPosition);

                surfaceTool.AddVertex(vertices[0]+voxPosition);
                surfaceTool.AddVertex(vertices[4]+voxPosition);
                surfaceTool.AddVertex(vertices[7]+voxPosition);
            }
            if(canCreateFace(x,y,z+1)){
                surfaceTool.AddNormal(new Vector3(0.0f, 0.0f, 1.0f));
                
                surfaceTool.AddVertex(vertices[3]+voxPosition);
                surfaceTool.AddVertex(vertices[6]+voxPosition);
                surfaceTool.AddVertex(vertices[2]+voxPosition);

                surfaceTool.AddVertex(vertices[3]+voxPosition);
                surfaceTool.AddVertex(vertices[7]+voxPosition);
                surfaceTool.AddVertex(vertices[6]+voxPosition);
            }
            if(canCreateFace(x,y,z-1)){
                surfaceTool.AddNormal(new Vector3(0.0f, 0.0f, -1.0f));
                surfaceTool.AddVertex(vertices[0]+voxPosition);
                surfaceTool.AddVertex(vertices[1]+voxPosition);
                surfaceTool.AddVertex(vertices[5]+voxPosition);

                surfaceTool.AddVertex(vertices[5]+voxPosition);
                surfaceTool.AddVertex(vertices[4]+voxPosition);
                surfaceTool.AddVertex(vertices[0]+voxPosition);
            }
        }

        private bool canCreateFace(int x, int y, int z)
        {
            if(!IsInData(x,y,z)){
                return true;
            }
            else if(chunkData[x][y][z] == VoxelTypes.Air){
                return true;
            }
            return false;
        }

        private bool IsInData(int x, int y, int z)
        {
            if(x < 0 || y < 0 || z < 0 || x >= CHUNK_SIZE || y >= CHUNK_SIZE || z >= CHUNK_SIZE){
                return false;
            }
            return true;
        }

        public override void _Ready(){
            // createMesh();
        }
        public override void _PhysicsProcess(float delta){}

        public override void _Process(float delta){}
    }

    #region chunk deprecated
    // class chunk : IDisposable
    // {
    //     private static int CHUNK_SIZE;

    //     private static float VOX_SIZE = 0.25f;
    //     private SurfaceTool surfaceTool;

    //     private blockMesher mesher;
    //     private Mesh chunkMesh;
    //     private VoxelTypes[][][] chunkVoxData;
    //     Vector3[] vertices;
    //     public chunk(int chunkSize)
    //     {
    //         CHUNK_SIZE = chunkSize;
    //         surfaceTool = new SurfaceTool();
    //         vertices = new Vector3[]{new Vector3(0,0,0),
    //                                             new Vector3(VOX_SIZE,0,0),
    //                                             new Vector3(VOX_SIZE,0,VOX_SIZE),
    //                                             new Vector3(0,0,VOX_SIZE),

    //                                             new Vector3(0,VOX_SIZE,0),
    //                                             new Vector3(VOX_SIZE,VOX_SIZE,0),
    //                                             new Vector3(VOX_SIZE,VOX_SIZE,VOX_SIZE),
    //                                             new Vector3(0,VOX_SIZE,VOX_SIZE)};
    //     }
    //     public void setVoxData(ref VoxelTypes[][][] voxData)
    //     {
    //         chunkVoxData = voxData;
    //     }

    //     public void createMesh()
    //     {
    //         surfaceTool.Begin(Mesh.PrimitiveType.Triangles);

    //         for (int i = 0; i < CHUNK_SIZE; i++)
    //         {
    //             for (int j = 0; j < CHUNK_SIZE; j++)
    //             {
    //                 for (int k = 0; k < CHUNK_SIZE; k++)
    //                 {
    //                     createCube(i, j, k);
    //                 }
    //             }
    //         }
    //         surfaceTool.Index();
    //         chunkMesh = surfaceTool.Commit();
    //     }
    //     public Mesh getChunkMesh(){
    //         return chunkMesh;
    //     }
    //     public void Dispose()
    //     {
    //         Console.WriteLine("Calling dispose function");
    //     }

    //     private void createCube(int x, int y, int z)
    //     {
    //         if(chunkVoxData[x][y][z] == VoxelTypes.Air){
    //             return;
    //         }

    //         Vector3 voxPosition = new Vector3(x * VOX_SIZE, y * VOX_SIZE, z * VOX_SIZE);
    //         if (canCreateFace(x, y - 1, z))
    //         {
    //             surfaceTool.AddNormal(new Vector3(0.0f, -1.0f, 0.0f));
    //             surfaceTool.AddVertex(vertices[1]+voxPosition);
    //             surfaceTool.AddVertex(vertices[3]+voxPosition);
    //             surfaceTool.AddVertex(vertices[2]+voxPosition);

    //             surfaceTool.AddVertex(vertices[1]+voxPosition);
    //             surfaceTool.AddVertex(vertices[0]+voxPosition);
    //             surfaceTool.AddVertex(vertices[3]+voxPosition);
    //         }
    //         if (canCreateFace(x, y + 1, z))
    //         {
    //             surfaceTool.AddNormal(new Vector3(0.0f, 1.0f, 0.0f));
    //             surfaceTool.AddVertex(vertices[4]+voxPosition);
    //             surfaceTool.AddVertex(vertices[5]+voxPosition);
    //             surfaceTool.AddVertex(vertices[7]+voxPosition);

    //             surfaceTool.AddVertex(vertices[5]+voxPosition);
    //             surfaceTool.AddVertex(vertices[6]+voxPosition);
    //             surfaceTool.AddVertex(vertices[7]+voxPosition);
    //         }
    //         if (canCreateFace(x + 1, y, z))
    //         {
    //             surfaceTool.AddNormal(new Vector3(1.0f,0.0f,0.0f));
    //             surfaceTool.AddVertex(vertices[2]+voxPosition);
    //             surfaceTool.AddVertex(vertices[5]+voxPosition);
    //             surfaceTool.AddVertex(vertices[1]+voxPosition);

    //             surfaceTool.AddVertex(vertices[2]+voxPosition);
    //             surfaceTool.AddVertex(vertices[6]+voxPosition);
    //             surfaceTool.AddVertex(vertices[5]+voxPosition);

    //         }
    //         if (canCreateFace(x - 1, y, z))
    //         {
    //             surfaceTool.AddNormal(new Vector3(-1.0f,0.0f,0.0f));
    //             surfaceTool.AddVertex(vertices[0]+voxPosition);
    //             surfaceTool.AddVertex(vertices[7]+voxPosition);
    //             surfaceTool.AddVertex(vertices[3]+voxPosition);

    //             surfaceTool.AddVertex(vertices[0]+voxPosition);
    //             surfaceTool.AddVertex(vertices[4]+voxPosition);
    //             surfaceTool.AddVertex(vertices[7]+voxPosition);

    //         }
    //         if (canCreateFace(x, y, z + 1))
    //         {
    //             surfaceTool.AddNormal(new Vector3(0.0f, 0.0f, 1.0f));
                
    //             surfaceTool.AddVertex(vertices[3]+voxPosition);
    //             surfaceTool.AddVertex(vertices[6]+voxPosition);
    //             surfaceTool.AddVertex(vertices[2]+voxPosition);

    //             surfaceTool.AddVertex(vertices[3]+voxPosition);
    //             surfaceTool.AddVertex(vertices[7]+voxPosition);
    //             surfaceTool.AddVertex(vertices[6]+voxPosition);
    //         }
    //         if (canCreateFace(x, y, z - 1))
    //         {
    //             surfaceTool.AddNormal(new Vector3(0.0f, 0.0f, -1.0f));
    //             surfaceTool.AddVertex(vertices[0]+voxPosition);
    //             surfaceTool.AddVertex(vertices[1]+voxPosition);
    //             surfaceTool.AddVertex(vertices[5]+voxPosition);

    //             surfaceTool.AddVertex(vertices[5]+voxPosition);
    //             surfaceTool.AddVertex(vertices[4]+voxPosition);
    //             surfaceTool.AddVertex(vertices[0]+voxPosition);
    //         }
    //     }

    //     private bool canCreateFace(int x, int y, int z)
    //     {
    //         if(!isInData(x,y,z)){
    //             // Console.WriteLine("Out of bounds");
    //             return true;
    //         }
    //         else if(chunkVoxData[x][y][z] == VoxelTypes.Air){
    //             return true;
    //         }
    //         return false;
    //     }

    //     private bool isInData(int x, int y, int z)
    //     {
    //         if (x < 0 || y < 0 || z < 0 || x >= CHUNK_SIZE || y >= CHUNK_SIZE || z >= CHUNK_SIZE)
    //         {
    //             return false;
    //         }
    //         return true;
    //     }
    // }
    #endregion
    

}