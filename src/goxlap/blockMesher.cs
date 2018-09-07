using FPSGame.src.Common.goxlap;
using Godot;
using Snappy;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using static Godot.SpatialMaterial;

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

        

        public delegate void CreateFace(ref ChunkStruct c, int firstDimensionStart,
				int secondDimensionStart,
				int firstDimensionEnd,
				int secondDimensionEnd,
				int thirdDimension);
        public GreedyBlockMesher(int CHUNK_X_COUNT, int CHUNK_Y_COUNT, int CHUNK_Z_COUNT, int CHUNK_SIZE, float VOX_SIZE) : base(CHUNK_X_COUNT, CHUNK_Y_COUNT, CHUNK_Z_COUNT, CHUNK_SIZE, VOX_SIZE)
        {
            mat = new SpatialMaterial();
            mat.VertexColorUseAsAlbedo = true;
            mat.VertexColorIsSrgb = true;
            mat.FlagsVertexLighting = true;
            // mat.SetCullMode(CullMode.Disabled);


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
        private void initializeJaggedArr(ref byte[][] maskArr)
        {
            for (int i = 0; i < CHUNK_SIZE; i++)
            {
                maskArr[i] = new byte[CHUNK_SIZE];
            }
        }

        // public override MeshInstance CreateChunkMesh(ref ChunkStruct c)
        // {

        //     // c.chunkData = SnappyCodec.Uncompress(c.compChunkData);
        //     c.surfaceTool.Begin(Mesh.PrimitiveType.Triangles);
        //     c.surfaceTool.SetMaterial(mat);
        //     for (int i = 0; i < CHUNK_SIZE; i++)
        //     {
        //         for (int j = 0; j < CHUNK_SIZE; j++)
        //         {
        //             for (int k = 0; k < CHUNK_SIZE; k++)
        //             {
        //                 if (c[i, j, k] == 0)
        //                 {
        //                     continue;
        //                 }
        //                 createFaces(i, j, k, c.Dx, c.Dy, c.Dz, ref c.surfaceTool, ref c);
        //             }
        //         }
        //     }
        //     c.surfaceTool.Index();
        //     MeshInstance mesh = new MeshInstance();
        //     mesh.SetMesh(c.surfaceTool.Commit());
        //     c.surfaceTool.Clear();
        //     // c.chunkData = new byte[1];
        //     return mesh;
        // }



        public override MeshInstance CreateChunkMesh(ref ChunkStruct c)
        {   
            String startTime = DateTime.Now.ToString("HH:mm:ss:ffff");
            
            byte[][] firstMask;
            byte[][] secondMask;
            MeshInstance mesh = new MeshInstance();
            c.surfaceTool.Begin(Mesh.PrimitiveType.Triangles);
            c.surfaceTool.SetMaterial(mat);
            firstMask = new byte[CHUNK_SIZE][];
            secondMask = new byte[CHUNK_SIZE][];
            initializeJaggedArr(ref firstMask);
            initializeJaggedArr(ref secondMask);
            String initTime = DateTime.Now.ToString("HH:mm:ss:ffff");
            
            for (int x = 0; x < CHUNK_SIZE; x++)
            {
                this.FillMasksWithLeftRight(ref c, x,ref firstMask,ref secondMask);

                this.FillWithQuads(firstMask, x, ref c, CreateLeftQuad);
                this.FillWithQuads(secondMask, x + 1, ref c, CreateRightQuad);
            }
            for (int y = 0; y < CHUNK_SIZE; y++)
            {
                this.FillMasksWithBottomTop(ref c, y,ref firstMask,ref secondMask);

                this.FillWithQuads(firstMask, y, ref c, CreateBottomQuad);
                this.FillWithQuads(secondMask, y + 1, ref c, CreateTopQuad);
            }
            for (int z = 0; z < CHUNK_SIZE; z++)
            {
                this.FillMasksWithBackFront(ref c, z,ref firstMask,ref secondMask);

                this.FillWithQuads(firstMask, z, ref c, CreateBackQuad);
                this.FillWithQuads(secondMask, z + 1, ref c, CreateFrontQuad);
            }
            String greedyMeshingTime = DateTime.Now.ToString("HH:mm:ss:ffff");
            
            c.surfaceTool.Index();
            mesh.SetMesh(c.surfaceTool.Commit());
            c.surfaceTool.Clear();
            String indexTime = DateTime.Now.ToString("HH:mm:ss:ffff");
            Console.WriteLine("Curr Thread: {0}, StartTime: {1}, Initialization Time: {2}, Greedy Time: {3}, Index Time {4}",System.Threading.Thread.CurrentThread.ManagedThreadId,startTime
            ,initTime,greedyMeshingTime,indexTime);
            return mesh;
        }


        private void CreateBackQuad(ref ChunkStruct c, int x, int y, int x2, int y2, int z)
        {
            //The Z+1 is more of a hack because the face previously was shown on the front side rather than the back
            Vector3 voxPosition1 = new Vector3((x) * VOX_SIZE, (y) * VOX_SIZE, (z) * VOX_SIZE);
            voxPosition1.x = voxPosition1.x + (c.Dx * CHUNK_SIZE * VOX_SIZE);
            voxPosition1.y = voxPosition1.y + (c.Dy * CHUNK_SIZE * VOX_SIZE);
            voxPosition1.z = voxPosition1.z + (c.Dz * CHUNK_SIZE * VOX_SIZE);

            Vector3 voxPosition2 = new Vector3((x2) * VOX_SIZE, (y2) * VOX_SIZE, (z+1) * VOX_SIZE);
            voxPosition2.x = voxPosition2.x + (c.Dx * CHUNK_SIZE * VOX_SIZE);
            voxPosition2.y = voxPosition2.y + (c.Dy * CHUNK_SIZE * VOX_SIZE);
            voxPosition2.z = voxPosition2.z + (c.Dz * CHUNK_SIZE * VOX_SIZE);
            SurfaceTool surfaceTool = c.surfaceTool;
            surfaceTool.AddNormal(new Vector3(0.0f, 0.0f, 1.0f));

            surfaceTool.AddVertex(new Vector3(voxPosition1.x, voxPosition1.y, voxPosition2.z));
            surfaceTool.AddVertex(new Vector3(voxPosition2.x, voxPosition2.y, voxPosition2.z));
            surfaceTool.AddVertex(new Vector3(voxPosition2.x, voxPosition1.y, voxPosition2.z));

            surfaceTool.AddVertex(new Vector3(voxPosition1.x, voxPosition1.y, voxPosition2.z));
            surfaceTool.AddVertex(new Vector3(voxPosition1.x, voxPosition2.y, voxPosition2.z));
            surfaceTool.AddVertex(new Vector3(voxPosition2.x, voxPosition2.y, voxPosition2.z));
            // addQuad(ref c,
			// 	new Vector3(x2, y, z),
			// 	new Vector3(x, y, z),
			// 	new Vector3(x,y2,z),
			// 	new Vector3(x2,y2,z));
        }
        private void CreateBottomQuad(ref ChunkStruct c, int x, int z, int x2, int z2, int y)
        {
            Vector3 voxPosition1 = new Vector3((x) * VOX_SIZE, (y) * VOX_SIZE, (z) * VOX_SIZE);
            voxPosition1.x = voxPosition1.x + (c.Dx * CHUNK_SIZE * VOX_SIZE);
            voxPosition1.y = voxPosition1.y + (c.Dy * CHUNK_SIZE * VOX_SIZE);
            voxPosition1.z = voxPosition1.z + (c.Dz * CHUNK_SIZE * VOX_SIZE);

            Vector3 voxPosition2 = new Vector3((x2) * VOX_SIZE, (y) * VOX_SIZE, (z2) * VOX_SIZE);
            voxPosition2.x = voxPosition2.x + (c.Dx * CHUNK_SIZE * VOX_SIZE);
            voxPosition2.y = voxPosition2.y + (c.Dy * CHUNK_SIZE * VOX_SIZE);
            voxPosition2.z = voxPosition2.z + (c.Dz * CHUNK_SIZE * VOX_SIZE);
            SurfaceTool surfaceTool = c.surfaceTool;
            surfaceTool.AddNormal(new Vector3(0.0f, -1.0f, 0.0f));

            surfaceTool.AddVertex(new Vector3(voxPosition2.x, voxPosition1.y, voxPosition1.z));
            surfaceTool.AddVertex(new Vector3(voxPosition1.x, voxPosition1.y, voxPosition2.z));
            surfaceTool.AddVertex(new Vector3(voxPosition2.x, voxPosition1.y, voxPosition2.z));

            surfaceTool.AddVertex(new Vector3(voxPosition2.x, voxPosition1.y, voxPosition1.z));
            surfaceTool.AddVertex(new Vector3(voxPosition1.x, voxPosition1.y, voxPosition1.z));
            surfaceTool.AddVertex(new Vector3(voxPosition1.x, voxPosition1.y, voxPosition2.z));
            // addQuad(ref c,
			// 	new Vector3(x2,y,z2),
			// 	new Vector3(x,y,z2),
			// 	new Vector3(x, y, z),
			// 	new Vector3(x2, y, z));
        }
        private void CreateFrontQuad(ref ChunkStruct c, int x, int y, int x2, int y2, int z)
        {
            //The Z-1 is more of a hack because the face previously was shown on the back side rather than the front
            Vector3 voxPosition1 = new Vector3((x) * VOX_SIZE, (y) * VOX_SIZE, (z-1) * VOX_SIZE);
            voxPosition1.x = voxPosition1.x + (c.Dx * CHUNK_SIZE * VOX_SIZE);
            voxPosition1.y = voxPosition1.y + (c.Dy * CHUNK_SIZE * VOX_SIZE);
            voxPosition1.z = voxPosition1.z + (c.Dz * CHUNK_SIZE * VOX_SIZE);

            Vector3 voxPosition2 = new Vector3((x2) * VOX_SIZE, (y2) * VOX_SIZE, (z) * VOX_SIZE);
            voxPosition2.x = voxPosition2.x + (c.Dx * CHUNK_SIZE * VOX_SIZE);
            voxPosition2.y = voxPosition2.y + (c.Dy * CHUNK_SIZE * VOX_SIZE);
            voxPosition2.z = voxPosition2.z + (c.Dz * CHUNK_SIZE * VOX_SIZE);
            SurfaceTool surfaceTool = c.surfaceTool;
            surfaceTool.AddNormal(new Vector3(0.0f, 0.0f, -1.0f));

            surfaceTool.AddVertex(new Vector3(voxPosition1.x, voxPosition1.y, voxPosition1.z));
            surfaceTool.AddVertex(new Vector3(voxPosition2.x, voxPosition1.y, voxPosition1.z));
            surfaceTool.AddVertex(new Vector3(voxPosition2.x, voxPosition2.y, voxPosition1.z));

            surfaceTool.AddVertex(new Vector3(voxPosition2.x, voxPosition2.y, voxPosition1.z));
            surfaceTool.AddVertex(new Vector3(voxPosition1.x, voxPosition2.y, voxPosition1.z));
            surfaceTool.AddVertex(new Vector3(voxPosition1.x, voxPosition1.y, voxPosition1.z));
            // addQuad(ref c,
			// 	new Vector3(x,y,z),
			// 	new Vector3(x2,y,z),
			// 	new Vector3(x2,y2,z),
			// 	new Vector3(x,y2,z));
        }
        private void CreateLeftQuad(ref ChunkStruct c, int z, int y, int z2, int y2, int x)
        {
            Vector3 voxPosition1 = new Vector3((x) * VOX_SIZE, (y) * VOX_SIZE, (z) * VOX_SIZE);
            voxPosition1.x = voxPosition1.x + (c.Dx * CHUNK_SIZE * VOX_SIZE);
            voxPosition1.y = voxPosition1.y + (c.Dy * CHUNK_SIZE * VOX_SIZE);
            voxPosition1.z = voxPosition1.z + (c.Dz * CHUNK_SIZE * VOX_SIZE);

            Vector3 voxPosition2 = new Vector3((x) * VOX_SIZE, (y2) * VOX_SIZE, (z2) * VOX_SIZE);
            voxPosition2.x = voxPosition2.x + (c.Dx * CHUNK_SIZE * VOX_SIZE);
            voxPosition2.y = voxPosition2.y + (c.Dy * CHUNK_SIZE * VOX_SIZE);
            voxPosition2.z = voxPosition2.z + (c.Dz * CHUNK_SIZE * VOX_SIZE);
            SurfaceTool surfaceTool = c.surfaceTool;
            surfaceTool.AddNormal(new Vector3(-1.0f, 0.0f, 0.0f));

            surfaceTool.AddVertex(new Vector3(voxPosition1.x, voxPosition1.y, voxPosition1.z));
            surfaceTool.AddVertex(new Vector3(voxPosition1.x, voxPosition2.y, voxPosition2.z));
            surfaceTool.AddVertex(new Vector3(voxPosition1.x, voxPosition1.y, voxPosition2.z));

            surfaceTool.AddVertex(new Vector3(voxPosition1.x, voxPosition1.y, voxPosition1.z));
            surfaceTool.AddVertex(new Vector3(voxPosition1.x, voxPosition2.y, voxPosition1.z));
            surfaceTool.AddVertex(new Vector3(voxPosition1.x, voxPosition2.y, voxPosition2.z));
            // addQuad(ref c,
			// 	new Vector3(x, y, z),
			// 	new Vector3(x, y, z2),
			// 	new Vector3(x,y2,z2),
			// 	new Vector3(x,y2,z));
        }
        private void CreateRightQuad(ref ChunkStruct c, int z, int y, int z2, int y2, int x)
        {
            Vector3 voxPosition1 = new Vector3((x) * VOX_SIZE, (y) * VOX_SIZE, (z) * VOX_SIZE);
            voxPosition1.x = voxPosition1.x + (c.Dx * CHUNK_SIZE * VOX_SIZE);
            voxPosition1.y = voxPosition1.y + (c.Dy * CHUNK_SIZE * VOX_SIZE);
            voxPosition1.z = voxPosition1.z + (c.Dz * CHUNK_SIZE * VOX_SIZE);

            Vector3 voxPosition2 = new Vector3((x) * VOX_SIZE, (y2) * VOX_SIZE, (z2) * VOX_SIZE);
            voxPosition2.x = voxPosition2.x + (c.Dx * CHUNK_SIZE * VOX_SIZE);
            voxPosition2.y = voxPosition2.y + (c.Dy * CHUNK_SIZE * VOX_SIZE);
            voxPosition2.z = voxPosition2.z + (c.Dz * CHUNK_SIZE * VOX_SIZE);
            SurfaceTool surfaceTool = c.surfaceTool;
            surfaceTool.AddNormal(new Vector3(1.0f, 0.0f, 0.0f));

            surfaceTool.AddVertex(new Vector3(voxPosition2.x, voxPosition1.y, voxPosition2.z));
            surfaceTool.AddVertex(new Vector3(voxPosition2.x, voxPosition2.y, voxPosition1.z));
            surfaceTool.AddVertex(new Vector3(voxPosition2.x, voxPosition1.y, voxPosition1.z));

            surfaceTool.AddVertex(new Vector3(voxPosition2.x, voxPosition1.y, voxPosition2.z));
            surfaceTool.AddVertex(new Vector3(voxPosition2.x, voxPosition2.y, voxPosition2.z));
            surfaceTool.AddVertex(new Vector3(voxPosition2.x, voxPosition2.y, voxPosition1.z));
            // addQuad(ref c,
			// 	new Vector3(x, y, z2),
			// 	new Vector3(x, y, z),
			// 	new Vector3(x, y2, z),
			// 	new Vector3(x, y2, z2));
        }
        private void CreateTopQuad(ref ChunkStruct c, int x, int z, int x2, int z2, int y)
        {
            Vector3 voxPosition1 = new Vector3((x) * VOX_SIZE, (y) * VOX_SIZE, (z) * VOX_SIZE);
            voxPosition1.x = voxPosition1.x + (c.Dx * CHUNK_SIZE * VOX_SIZE);
            voxPosition1.y = voxPosition1.y + (c.Dy * CHUNK_SIZE * VOX_SIZE);
            voxPosition1.z = voxPosition1.z + (c.Dz * CHUNK_SIZE * VOX_SIZE);

            Vector3 voxPosition2 = new Vector3((x2) * VOX_SIZE, (y) * VOX_SIZE, (z2) * VOX_SIZE);
            voxPosition2.x = voxPosition2.x + (c.Dx * CHUNK_SIZE * VOX_SIZE);
            voxPosition2.y = voxPosition2.y + (c.Dy * CHUNK_SIZE * VOX_SIZE);
            voxPosition2.z = voxPosition2.z + (c.Dz * CHUNK_SIZE * VOX_SIZE);
            SurfaceTool surfaceTool = c.surfaceTool;
            surfaceTool.AddNormal(new Vector3(0.0f, 1.0f, 0.0f));

            surfaceTool.AddVertex(new Vector3(voxPosition1.x, voxPosition2.y, voxPosition1.z));
            surfaceTool.AddVertex(new Vector3(voxPosition2.x, voxPosition2.y, voxPosition1.z));
            surfaceTool.AddVertex(new Vector3(voxPosition1.x, voxPosition2.y, voxPosition2.z));

            surfaceTool.AddVertex(new Vector3(voxPosition2.x, voxPosition2.y, voxPosition1.z));
            surfaceTool.AddVertex(new Vector3(voxPosition2.x, voxPosition2.y, voxPosition2.z));
            surfaceTool.AddVertex(new Vector3(voxPosition1.x, voxPosition2.y, voxPosition2.z));
            // this.addQuad(ref c,
			// 	new Vector3(x, y, z2),
			// 	new Vector3(x2, y, z2),
			// 	new Vector3(x2, y, z),
			// 	new Vector3(x, y, z));
            
        }

        private void addQuad(ref ChunkStruct chunk,Vector3 a, Vector3 b, Vector3 c, Vector3 d){
            a.x = (a.x*VOX_SIZE) + (chunk.Dx * CHUNK_SIZE * VOX_SIZE);
            a.y = (a.y*VOX_SIZE) + (chunk.Dy * CHUNK_SIZE * VOX_SIZE);
            a.z = (a.z*VOX_SIZE) + (chunk.Dz * CHUNK_SIZE * VOX_SIZE);

            b.x = (b.x*VOX_SIZE) + (chunk.Dx * CHUNK_SIZE * VOX_SIZE);
            b.y = (b.y*VOX_SIZE) + (chunk.Dy * CHUNK_SIZE * VOX_SIZE);
            b.z = (b.z*VOX_SIZE) + (chunk.Dz * CHUNK_SIZE * VOX_SIZE);

            c.x = (c.x*VOX_SIZE) + (chunk.Dx * CHUNK_SIZE * VOX_SIZE);
            c.y = (c.y*VOX_SIZE) + (chunk.Dy * CHUNK_SIZE * VOX_SIZE);
            c.z = (c.z*VOX_SIZE) + (chunk.Dz * CHUNK_SIZE * VOX_SIZE);

            d.x = (d.x*VOX_SIZE) + (chunk.Dx * CHUNK_SIZE * VOX_SIZE);
            d.y = (d.y*VOX_SIZE) + (chunk.Dy * CHUNK_SIZE * VOX_SIZE);
            d.z = (d.z*VOX_SIZE) + (chunk.Dz * CHUNK_SIZE * VOX_SIZE);

            List<Vector3> verts = new List<Vector3>();
            verts.Add(a);
            verts.Add(b);
            verts.Add(c);
            verts.Add(d);

            chunk.surfaceTool.AddVertex(a);
            chunk.surfaceTool.AddVertex(b);
            chunk.surfaceTool.AddVertex(c);
            chunk.surfaceTool.AddVertex(d);

            chunk.surfaceTool.AddIndex(verts.Count-4);
            chunk.surfaceTool.AddIndex(verts.Count-3);
            chunk.surfaceTool.AddIndex(verts.Count-2);
            chunk.surfaceTool.AddIndex(verts.Count-4);
            chunk.surfaceTool.AddIndex(verts.Count-2);
            chunk.surfaceTool.AddIndex(verts.Count-1);
        }

        private void createFaces(int x, int y, int z, int Dx, int Dy, int Dz,
        ref SurfaceTool surfaceTool, ref ChunkStruct c)
        {
            Vector3 voxPosition = new Vector3((x) * VOX_SIZE, (y) * VOX_SIZE, (z) * VOX_SIZE);
            voxPosition.x = voxPosition.x + (Dx * CHUNK_SIZE * VOX_SIZE);
            voxPosition.y = voxPosition.y + (Dy * CHUNK_SIZE * VOX_SIZE);
            voxPosition.z = voxPosition.z + (Dz * CHUNK_SIZE * VOX_SIZE);
            HashSet<Face> faceSet = this.CheckFaces(x, y, z, ref c);
            if (faceSet.Contains(Face.BOTTOM))
            {
                // this.CreateBottomQuad(ref c, x, y, z, x + 1, y, z + 1);
                surfaceTool.AddNormal(new Vector3(0.0f, -1.0f, 0.0f));
                surfaceTool.AddColor(new Color(1.0f, 1.0f, .7f, 1f));
                surfaceTool.AddVertex(vertices[1] + voxPosition);
                surfaceTool.AddVertex(vertices[3] + voxPosition);
                surfaceTool.AddVertex(vertices[2] + voxPosition);

                surfaceTool.AddVertex(vertices[1] + voxPosition);
                surfaceTool.AddVertex(vertices[0] + voxPosition);
                surfaceTool.AddVertex(vertices[3] + voxPosition);
            }
            if (faceSet.Contains(Face.TOP))
            {
                // this.CreateTopQuad(ref c, x,z, x+10, z + 1,y+1);
                surfaceTool.AddNormal(new Vector3(0.0f, 1.0f, 0.0f));
                surfaceTool.AddColor(new Color(0.7f, 0.0f, .7f, 1f));
                surfaceTool.AddVertex(vertices[4] + voxPosition);
                surfaceTool.AddVertex(vertices[5] + voxPosition);
                surfaceTool.AddVertex(vertices[7] + voxPosition);

                surfaceTool.AddVertex(vertices[5] + voxPosition);
                surfaceTool.AddVertex(vertices[6] + voxPosition);
                surfaceTool.AddVertex(vertices[7] + voxPosition);
            }
            if (faceSet.Contains(Face.RIGHT))
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
            if (faceSet.Contains(Face.LEFT))
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
            if (faceSet.Contains(Face.BACK))
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
            if (faceSet.Contains(Face.FRONT))
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
        private HashSet<Face> CheckFaces(int x, int y, int z, ref ChunkStruct c)
        {

            var faceSet = new HashSet<Face>();
            if (c[x, y, z] == (byte)VoxelTypes.Air)
            {
                faceSet.Add(Face.NONE);
                return faceSet;
            }
            faceSet.Add(canCreateFace(x, y - 1, z, ref c) == true ? Face.BOTTOM : Face.NO_SIDE);
            faceSet.Add(canCreateFace(x, y + 1, z, ref c) == true ? Face.TOP : Face.NO_SIDE);
            faceSet.Add(canCreateFace(x + 1, y, z, ref c) == true ? Face.RIGHT : Face.NO_SIDE);
            faceSet.Add(canCreateFace(x - 1, y, z, ref c) == true ? Face.LEFT : Face.NO_SIDE);
            faceSet.Add(canCreateFace(x, y, z + 1, ref c) == true ? Face.BACK : Face.NO_SIDE);
            faceSet.Add(canCreateFace(x, y, z - 1, ref c) == true ? Face.FRONT : Face.NO_SIDE);

            return faceSet;
        }
        private bool canCreateFace(int x, int y, int z, ref ChunkStruct c)
        {


            if (!isInData(x, y, z))
            {
                if (volume[c.Dx * CHUNK_SIZE + x, c.Dy * CHUNK_SIZE + y, c.Dz * CHUNK_SIZE + z] != (byte)VoxelTypes.Air)
                {
                    return false;
                }
                return true;
            }
            if (c[x, y, z] == (byte)VoxelTypes.Air)
            {
                return true;
            }
            return false;
        }
        private bool isInData(int x, int y, int z)
        {
            bool result = true;
            if (x < 0 || y < 0 || z < 0 || x >= CHUNK_SIZE || y >= CHUNK_SIZE || z >= CHUNK_SIZE)
            {
                result = false;

            }

            return result;
        }

        private void FillWithQuads(byte[][] mask, int currentSlice, ref ChunkStruct c, CreateFace createFace)
        {
            for(int x=0; x< mask.Length;x++){
                byte[] row = mask[x];
                for(int y=0;y<row.Length;y++){
                    if(row[y] != (byte) VoxelTypes.Air){
                        byte currentId = row[y];

                        int startX = x;
                        int startY = y;

                        int endY = findYEnd(row,currentId,startY);
                        int endX = findXEnd(mask, currentId, startY, endY, x);

                        y = endY -1;
                        //public delegate void CreateFace(ref ChunkStruct c, int x, int y, int z, int x2, int y2, int z2);
                        createFace(ref c,startX,startY,endX,endY,currentSlice);
                    }
                }
            }
        }

        private void FillMasksWithBackFront(ref ChunkStruct c, int currentSlice,ref byte[][] firstMask,ref byte[][] secondMask)
        {
            for (int x = 0; x < CHUNK_SIZE; x++)
            {
                for (int y = 0; y < CHUNK_SIZE; y++)
                {
                    HashSet<Face> faceSet = CheckFaces(x, y, currentSlice, ref c);

                    if (faceSet.Contains(Face.BACK))
                    {
                        firstMask[x][y] = c[x, y, currentSlice];
                    }
                    else
                    {
                        firstMask[x][y] = 0;
                    }

                    if (faceSet.Contains(Face.FRONT))
                    {
                        secondMask[x][y] = c[x, y, currentSlice];
                    }
                    else
                    {
                        secondMask[x][y] = 0;
                    }
                }
            }
        }

        private void FillMasksWithBottomTop(ref ChunkStruct c, int currentSlice,ref byte[][]firstMask,ref byte[][]secondMask)
        {
            for (int x = 0; x < CHUNK_SIZE; x++)
            {
                for (int z = 0; z < CHUNK_SIZE; z++)
                {
                    HashSet<Face> faceSet = CheckFaces(x, currentSlice, z, ref c);

                    if (faceSet.Contains(Face.BOTTOM))
                    {
                        firstMask[x][z] = c[x, currentSlice, z];
                    }
                    else
                    {
                        firstMask[x][z] = 0;
                    }

                    if (faceSet.Contains(Face.TOP))
                    {
                        secondMask[x][z] = c[x, currentSlice, z];
                    }
                    else
                    {
                        secondMask[x][z] = 0;
                    }
                }
            }
        }

        private void FillMasksWithLeftRight(ref ChunkStruct c, int currentSlice,ref byte[][]firstMask,ref byte[][]secondMask)
        {
            for (int y = 0; y < CHUNK_SIZE; y++)
            {
                for (int z = 0; z < CHUNK_SIZE; z++)
                {
                    HashSet<Face> faceSet = CheckFaces(currentSlice, y, z, ref c);

                    if (faceSet.Contains(Face.LEFT))
                    {
                        firstMask[z][y] = c[currentSlice, y, z];
                    }
                    else
                    {
                        firstMask[z][y] = 0;
                    }

                    if (faceSet.Contains(Face.RIGHT))
                    {
                        secondMask[z][y] = c[currentSlice, y, z];
                    }
                    else
                    {
                        secondMask[z][y] = 0;
                    }
                }
            }
        }
        private int findXEnd(byte[][] mask, int currentId, int startY, int endY, int x)
        {
            int end = x + 1;

            while (end < mask.Length)
            {
                for (int checkY = startY; checkY < endY; checkY++)
                {
                    if (mask[end][checkY] != currentId)
                    {
                        return end;
                    }
                }

                for (int checkY = startY; checkY < endY; checkY++)
                {
                    mask[end][checkY] = 0;
                }

                end++;
            }

            return end;
        }

        private int findYEnd(byte[] row, int currentId, int startY)
        {
            int end = startY;

            while (end < row.Length && row[end] == currentId)
            {
                row[end] = 0;
                end++;
            }

            return end;
        }

    }

    class BlockMesher : AbstractVoxelMesher
    {
        int CHUNK_X_COUNT;
        int CHUNK_Y_COUNT;
        int CHUNK_Z_COUNT;
        int CHUNK_SIZE;
        float VOX_SIZE;
        private Vector3[] vertices;
        public VoxelVolume volume { get; set; }
        public SpatialMaterial mat;

        public BlockMesher(int CHUNK_X_COUNT, int CHUNK_Y_COUNT, int CHUNK_Z_COUNT, int CHUNK_SIZE, float VOX_SIZE) : base(CHUNK_X_COUNT, CHUNK_Y_COUNT, CHUNK_Z_COUNT, CHUNK_SIZE, VOX_SIZE)
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

        private bool canCreateFace(int x, int y, int z, ref ChunkStruct c)
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


    public class Face
    {
        public static readonly Face NONE = new Face(Vector3.Inf);
        public static readonly Face NO_SIDE = new Face(Vector3.Inf);
        public static readonly Face TOP = new Face(new Vector3(0, 1, 0));
        public static readonly Face BOTTOM = new Face(new Vector3(0, -1, 0));
        public static readonly Face LEFT = new Face(new Vector3(-1, 0, 0));
        public static readonly Face RIGHT = new Face(new Vector3(1, 0, 0));
        public static readonly Face FRONT = new Face(new Vector3(0, 0, 1));
        public static readonly Face BACK = new Face(new Vector3(0, 0, -1));

        public Face(Vector3 offset)
        {
            this.offset = offset;
            this.normals = new List<float> { offset.x, offset.y, offset.z }.AsReadOnly();
        }

        public readonly Vector3 offset;
        public readonly ReadOnlyCollection<float> normals;
    }
}
