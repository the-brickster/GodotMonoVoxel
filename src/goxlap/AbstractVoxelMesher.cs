using FPSGame.src.Common.goxlap;
using System;
using Godot;
namespace FPSGame.src.goxlap
{
    abstract class AbstractVoxelMesher
    {

        public int CHUNK_X_COUNT;
        public int CHUNK_Y_COUNT;
        public int CHUNK_Z_COUNT;
        public int CHUNK_SIZE;
        public float VOX_SIZE;
        public Vector3[] vertices;
        public VoxelVolume volume { get; set; }

        public AbstractVoxelMesher(int CHUNK_X_COUNT, int CHUNK_Y_COUNT, int CHUNK_Z_COUNT, int CHUNK_SIZE, float VOX_SIZE){
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

        public abstract MeshInstance CreateChunkMesh(ref ChunkStruct c);

      
    }
}