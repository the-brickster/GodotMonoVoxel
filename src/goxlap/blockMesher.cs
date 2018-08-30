using FPSGame.src.Common.goxlap;
using Godot;
using Snappy;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace FPSGame.src.goxlap
{
    class GreedyBlockMesher : AbstractVoxelMesher
    {
        int CHUNK_X_COUNT;
        int CHUNK_Y_COUNT;
        int CHUNK_Z_COUNT;
        int CHUNK_SIZE;
        float VOX_SIZE;
        private Vector3[] vertices;
        public VoxelVolume volume { get; set; }
        public SpatialMaterial mat;

        private byte[,] firstMask;
        private byte[,] secondMask;

        public delegate void CreateFace(ref ChunkStruct c, float x,float y,float z);
        public GreedyBlockMesher(int CHUNK_X_COUNT, int CHUNK_Y_COUNT, int CHUNK_Z_COUNT, int CHUNK_SIZE, float VOX_SIZE){
            mat = new SpatialMaterial();
            mat.VertexColorUseAsAlbedo = true;
            mat.VertexColorIsSrgb = true;
            mat.FlagsVertexLighting = true;
            

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
            
            firstMask = new byte[CHUNK_SIZE,CHUNK_SIZE];
            secondMask = new byte[CHUNK_SIZE,CHUNK_SIZE];
        }

        public override MeshInstance CreateChunkMesh(ref ChunkStruct c)
        {
            MeshInstance mesh = new MeshInstance();
            for(int x = 0; x < CHUNK_SIZE; x++){
                this.FillMasksWithLeftRight(ref c,x);

                this.FillWithQuads(firstMask,x,ref c,CreateLeftQuad);
                this.FillWithQuads(secondMask,x+1,ref c,CreateRightQuad);
            }
            for(int y = 0; y < CHUNK_SIZE; y++){
                this.FillMasksWithBottomTop(ref c, y);

                this.FillWithQuads(firstMask,y,ref c,CreateBottomQuad);
                this.FillWithQuads(secondMask,y+1,ref c,CreateTopQuad);
            }
            for(int z = 0; z < CHUNK_SIZE; z++){
                this.FillMasksWithBackFront(ref c, z);

                this.FillWithQuads(firstMask,z,ref c,CreateBackQuad);
                this.FillWithQuads(secondMask,z+1,ref c,CreateFrontQuad);
            }
            return mesh;
        }

        private static void CreateBackQuad(ref ChunkStruct c, float x,float y,float z){
            throw new NotImplementedException();
        }
        private static void CreateBottomQuad(ref ChunkStruct c, float x,float y,float z){
            throw new NotImplementedException();
        }
        private static void CreateFrontQuad(ref ChunkStruct c, float x,float y,float z){
            throw new NotImplementedException();
        }
        private static void CreateLeftQuad(ref ChunkStruct c, float x,float y,float z){
            throw new NotImplementedException();
        }
        private static void CreateRightQuad(ref ChunkStruct c, float x,float y,float z){
            throw new NotImplementedException();
        }
        private static void CreateTopQuad(ref ChunkStruct c, float x,float y,float z){
            throw new NotImplementedException();
        }

        private void FillWithQuads(byte[,] firstMask, int x, ref ChunkStruct c, CreateFace createFace)
        {
            throw new NotImplementedException();
        }

        private void FillMasksWithBackFront(ref ChunkStruct c, int z)
        {
            throw new NotImplementedException();
        }

        private void FillMasksWithBottomTop(ref ChunkStruct c, int y)
        {
            throw new NotImplementedException();
        }

        private void FillMasksWithLeftRight(ref ChunkStruct c, int x)
        {
            throw new NotImplementedException();
        }
    }

    class BlockMesher:AbstractVoxelMesher
    {
        int CHUNK_X_COUNT;
        int CHUNK_Y_COUNT;
        int CHUNK_Z_COUNT;
        int CHUNK_SIZE;
        float VOX_SIZE;
        private Vector3[] vertices;
        public VoxelVolume volume { get; set; }
        public SpatialMaterial mat;

        public BlockMesher(int CHUNK_X_COUNT, int CHUNK_Y_COUNT, int CHUNK_Z_COUNT, int CHUNK_SIZE, float VOX_SIZE)
        {
            mat = new SpatialMaterial();
            mat.VertexColorUseAsAlbedo = true;
            mat.VertexColorIsSrgb = true;
            mat.FlagsVertexLighting = true;
            

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

        public override MeshInstance CreateChunkMesh(ref ChunkStruct c)
        {
            // c.chunkData = SnappyCodec.Uncompress(c.compChunkData);
            // c.uncompressVoxData();
            c.surfaceTool.Begin(Mesh.PrimitiveType.Triangles);
            c.surfaceTool.SetMaterial(mat);
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
            // c.chunkData = new byte[1];
            // c.compressVoxData();
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
                surfaceTool.AddColor(new Color(1.0f, 1.0f, .7f, 1f));
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
                surfaceTool.AddColor(new Color(0.7f, 0.0f, .7f, 1f));
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
                surfaceTool.AddColor(new Color(1f, 1f, 1f, 1f));
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
                surfaceTool.AddColor(new Color(1f, 1f, 1f, 1f));
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
                surfaceTool.AddColor(new Color(1f, 1f, 1f, 1f));
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
                surfaceTool.AddColor(new Color(1f, 1f, 1f, 1f));
                surfaceTool.AddVertex(vertices[0] + voxPosition);
                surfaceTool.AddVertex(vertices[1] + voxPosition);
                surfaceTool.AddVertex(vertices[5] + voxPosition);

                surfaceTool.AddVertex(vertices[5] + voxPosition);
                surfaceTool.AddVertex(vertices[4] + voxPosition);
                surfaceTool.AddVertex(vertices[0] + voxPosition);
            }
            
        }



    }


    public class Face {
       public static readonly Face TOP = new Face(new Vector3(0, 1, 0));
       public static readonly Face BOTTOM = new Face(new Vector3(0, -1, 0));
       public static readonly Face LEFT = new Face(new Vector3(-1, 0, 0));
       public static readonly Face RIGHT = new Face(new Vector3(1, 0, 0));
       public static readonly Face FRONT = new Face(new Vector3(0, 0, 1));
       public static readonly Face BACK = new Face(new Vector3(0, 0, -1));

       public Face(Vector3 offset){
           this.offset = offset;
           this.normals = new List<float>{offset.x,offset.y,offset.z}.AsReadOnly();
       }

       public readonly Vector3 offset;
       public readonly ReadOnlyCollection<float> normals;
    }
}
