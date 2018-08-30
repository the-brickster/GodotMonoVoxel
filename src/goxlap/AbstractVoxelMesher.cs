using FPSGame.src.Common.goxlap;
using System;
using Godot;
namespace FPSGame.src.goxlap
{
    abstract class AbstractVoxelMesher
    {

        int CHUNK_X_COUNT;
        int CHUNK_Y_COUNT;
        int CHUNK_Z_COUNT;
        int CHUNK_SIZE;
        float VOX_SIZE;
        private Vector3[] vertices;
        public VoxelVolume volume { get; set; }
        public abstract MeshInstance CreateChunkMesh(ref ChunkStruct c);

        protected bool canCreateFace(int x, int y, int z, ref ChunkStruct c)
        {


            if (!isInData(x, y, z))
            {
                if (volume[c.Dx * CHUNK_SIZE + x, c.Dy * CHUNK_SIZE + y, c.Dz * CHUNK_SIZE + z] != (byte)VoxelTypes.Air)
                {
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
            if (c[x, y, z] == (byte)VoxelTypes.Air)
            {
                return true;
            }
            // if (volume[c.Dx * CHUNK_SIZE+x, c.Dy * CHUNK_SIZE+y, c.Dz * CHUNK_SIZE+z] == (byte)VoxelTypes.Air)
            // {
            //     return true;
            // }
            return false;
        }
        protected bool isInData(int x, int y, int z)
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