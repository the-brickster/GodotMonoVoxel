using FPSGame.src.Common.goxlap;
using Godot;
using Snappy;
using System;
using System.Collections.Concurrent;

namespace FPSGame.src.goxlap
{
    class BlockMesher
    {
        int CHUNK_X_COUNT;
        int CHUNK_Y_COUNT;
        int CHUNK_Z_COUNT;
        int CHUNK_SIZE;
        float VOX_SIZE;
        private Vector3[] vertices;
        public VoxelVolume volume { get; set; }

        public BlockMesher(int CHUNK_X_COUNT, int CHUNK_Y_COUNT, int CHUNK_Z_COUNT, int CHUNK_SIZE, float VOX_SIZE)
        {
            this.CHUNK_X_COUNT = CHUNK_X_COUNT;
            this.CHUNK_Y_COUNT = CHUNK_Y_COUNT;
            this.CHUNK_Z_COUNT = CHUNK_Z_COUNT;
            this.CHUNK_SIZE = CHUNK_SIZE;
            this.VOX_SIZE = VOX_SIZE;
            vertices = vertices = new Vector3[]{new Vector3(0,0,0),
                                                new Vector3(VOX_SIZE,0,0),
                                                new Vector3(VOX_SIZE,0,VOX_SIZE),
                                                new Vector3(0,0,VOX_SIZE),

                                                new Vector3(0,VOX_SIZE,0),
                                                new Vector3(VOX_SIZE,VOX_SIZE,0),
                                                new Vector3(VOX_SIZE,VOX_SIZE,VOX_SIZE),
                                                new Vector3(0,VOX_SIZE,VOX_SIZE)};

        }

        public MeshInstance createChunkMesh(ref ChunkStruct c)
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
            c.surfaceTool.Clear();
            c.chunkData = new byte[1];
            return mesh;
        }

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
                if (volume[c.Dx * CHUNK_SIZE+x, c.Dy * CHUNK_SIZE+y, c.Dz * CHUNK_SIZE+z] != (byte)VoxelTypes.Air){
                    return false;
                }
                return true;
            }
            // else if (volume[x * CHUNK_SIZE, y * CHUNK_SIZE, z * CHUNK_SIZE] == VoxelTypes.Default)
            // {
            //     Console.WriteLine(string.Format("{0} {1} {2} ",x,y,z)+volume[x * CHUNK_SIZE, y * CHUNK_SIZE, z * CHUNK_SIZE]);
            //    return false;
            // }
            // Console.WriteLine(string.Format("{0} {1} {2}", x, y, z));
            if(c[x,y,z] == (byte)VoxelTypes.Air){
                return true;
            }
            // if (volume[c.Dx * CHUNK_SIZE+x, c.Dy * CHUNK_SIZE+y, c.Dz * CHUNK_SIZE+z] == (byte)VoxelTypes.Air)
            // {
            //     return true;
            // }
            return false;
        }
        private bool isInData(int x, int y, int z)
        {
            bool result = true;
            if (x < 0 || y < 0 || z < 0 || x >= CHUNK_SIZE || y >= CHUNK_SIZE || z >= CHUNK_SIZE)
            {
                result = false;
                
            }
            // if (volume[x * CHUNK_SIZE, y * CHUNK_SIZE, z * CHUNK_SIZE] == -1)
            // {
            //     // Console.WriteLine(string.Format("{0} {1} {2} ",x,y,z)+volume[x * CHUNK_SIZE, y * CHUNK_SIZE, z * CHUNK_SIZE]);
            //    result = false;
            // }

            return result;
        }
    }
}
