using Godot;
using System;

namespace Goxlap.src.Goxlap
{
    public class VoxelChunk
    {
        public int DX { get; }
        public int DY { get; }
        public int DZ { get; }
        public bool need_update = false;
        public byte[] voxel_data;
        public int CHUNK_SIZE;
        public SurfaceTool surface_tool;
        public MeshInstance mesh;

        public utils.AABB boundingBox;
        /*
        - 0 - Top Chunk
        - 1 - Botton Chunk
        - 2 - Left Chunk
        - 3 - Right Chunk
        - 4 - Front Chunk
        - 5 - Back Chunk
         */
        public VoxelChunk[] neighbors = new VoxelChunk[6] { null, null, null, null, null, null };

        public VoxelChunk(int DX, int DY, int DZ)
        {
            this.DX = DX;
            this.DY = DY;
            this.DZ = DZ;
            CHUNK_SIZE = VoxelConstants.CHUNK_SIZE;
            voxel_data = new byte[VoxelConstants.CHUNK_SIZE_MAX];
            surface_tool = new SurfaceTool();
            
            float offset = CHUNK_SIZE*VoxelConstants.VOX_SIZE;
            System.Numerics.Vector3 min = new System.Numerics.Vector3(DX*offset,DY*offset,DZ*offset);
            System.Numerics.Vector3 max = new System.Numerics.Vector3(offset+DX*offset,offset+DY*offset,offset+DZ*offset);
            boundingBox = new utils.AABB(min,max);
        }

        public byte get(int x, int y, int z)
        {
            return voxel_data[x + CHUNK_SIZE * (y + CHUNK_SIZE * z)];
        }

        public void set(int x, int y, int z, byte data)
        {
            voxel_data[x + CHUNK_SIZE * (y + CHUNK_SIZE * z)] = data;
        }

        public override string ToString()
        {
            return "Chunk: " + DX + ", " + DY + ", " + DZ + " Size:" + CHUNK_SIZE +
            " Neighbors:" + neighbors;
        }
    }

}