using System;
using System.Diagnostics;
using Godot;

namespace Goxlap.src.Goxlap
{
    public class PointVoxelMesher : IVoxelMesher
    {
        /*
                            face = CanCreateVoxel(x, y - 1, z, ref c) == true ? face | 0b000001 : face | 0b000000;
                    face = CanCreateVoxel(x, y + 1, z, ref c) == true ? face | 0b000010 : face | 0b000000;
                    face = CanCreateVoxel(x + 1, y, z, ref c) == true ? face | 0b000100 : face | 0b000000;
                    face = CanCreateVoxel(x - 1, y, z, ref c) == true ? face | 0b001000 : face | 0b000000;
                    face = CanCreateVoxel(x, y, z + 1, ref c) == true ? face | 0b010000 : face | 0b000000;
                    face = CanCreateVoxel(x, y, z - 1, ref c) == true ? face | 0b100000 : face | 0b000000;
         */
        static readonly Vector3[] Normals = new Vector3[]{
            new Vector3(0f,-1f,0f),//Bottom
            new Vector3(0f,1f,0f),//Top
            new Vector3(1,0,0),//Right
            new Vector3(-1,0,0),//Left
            new Vector3(0,0,1),//front
            new Vector3(0,0,-1)//back
        };
        ShaderMaterial chunkMaterial;
        float VOX_SIZE = VoxelConstants.VOX_SIZE;
        int CHUNK_SIZE = VoxelConstants.CHUNK_SIZE;
        public Color[] voxelTypes = new Color[] { new Color(0f, 0f, 0.55f, 1f), new Color(1f, 1f, 1f, 1f) };
        public PointVoxelMesher(ShaderMaterial material)
        {
            chunkMaterial = material;

        }
        public void SetVoxelTypes(ref Color[] colorArr)
        {
            voxelTypes = colorArr;
        }
        public MeshInstance CreateChunkMesh(ref VoxelChunk c)
        {
            if (!c.need_update)
            {
                return null;
            }
            c.surface_tool.Begin(Mesh.PrimitiveType.Points);
            c.surface_tool.SetMaterial(chunkMaterial);
            int count = 0;

            // Random random = new Random();
            // float r = (float)random.NextDouble();
            // float g = (float)random.NextDouble();
            // float b = (float)random.NextDouble();

            for (int i = 0; i < VoxelConstants.CHUNK_SIZE_MAX; i++)
            {

                var x = i % CHUNK_SIZE;
                var y = (i / CHUNK_SIZE) % CHUNK_SIZE;
                var z = i / (CHUNK_SIZE * CHUNK_SIZE);
                if (c.get(x, y, z) != 0)
                {
                    byte val = c.get(x, y, z);
                    int face = 0b000000;
                    face = CanCreateVoxel(x, y - 1, z, ref c) == true ? face | 0b000001 : face | 0b000000;
                    face = CanCreateVoxel(x, y + 1, z, ref c) == true ? face | 0b000010 : face | 0b000000;
                    face = CanCreateVoxel(x + 1, y, z, ref c) == true ? face | 0b000100 : face | 0b000000;
                    face = CanCreateVoxel(x - 1, y, z, ref c) == true ? face | 0b001000 : face | 0b000000;
                    face = CanCreateVoxel(x, y, z + 1, ref c) == true ? face | 0b010000 : face | 0b000000;
                    face = CanCreateVoxel(x, y, z - 1, ref c) == true ? face | 0b100000 : face | 0b000000;
                    // 0/0.7/0.4
                    if (face != 0b000000)
                    {
                        int counter = 0;
                        Vector3 normalAvg = Vector3.Zero;
                        for (int j = 0; j < 6; j++)
                        {
                            int bitFlagN = (face >> j) & 1;
                            if (bitFlagN == 1)
                            {
                                normalAvg = normalAvg + Normals[j];
                                counter += 1;
                            }
                        }

                        // Console.WriteLine("Value: {0}", Convert.ToString(face, 2).PadLeft(6, '0'));
                        count += 1;
                        c.surface_tool.AddColor( new Color(1f, 1f, 1f, 1f));
                        Vector3 voxPosition = new Vector3((x) * VOX_SIZE, (y) * VOX_SIZE, (z) * VOX_SIZE);
                        voxPosition.x = voxPosition.x + (c.DX * CHUNK_SIZE * VOX_SIZE);
                        voxPosition.y = voxPosition.y + (c.DY * CHUNK_SIZE * VOX_SIZE);
                        voxPosition.z = voxPosition.z + (c.DZ * CHUNK_SIZE * VOX_SIZE);
                        if (counter > 0)
                        {
                            normalAvg = normalAvg / counter;
                            c.surface_tool.AddNormal(normalAvg);
                        }
                        c.surface_tool.AddVertex(voxPosition);


                    }
                }
            }

            c.surface_tool.Index();
            c.mesh = new MeshInstance();

            c.mesh.SetMesh(c.surface_tool.Commit());
            c.surface_tool.Clear();
            c.mesh.MaterialOverride = chunkMaterial.Duplicate() as ShaderMaterial;

            // Console.WriteLine("Mesh AABB Pos: {0} , Size: {1}, End: {2}",bb.Position,bb.Size,bb.End);
            if (count != 0)
                return c.mesh;
            else
                return null;
        }
        /*
        - 0 - Top Chunk (chunk.DX, chunk.DY + 1, chunk.DZ);
        - 1 - Botton Chunk (chunk.DX, chunk.DY - 1, chunk.DZ);
        - 2 - Left Chunk (chunk.DX - 1, chunk.DY, chunk.DZ);
        - 3 - Right Chunk (chunk.DX + 1, chunk.DY, chunk.DZ);
        - 4 - Front Chunk (chunk.DX, chunk.DY, chunk.DZ - 1);
        - 5 - Back Chunk (chunk.DX, chunk.DY, chunk.DZ + 1);
         */
        public bool CanCreateVoxel(int x, int y, int z, ref VoxelChunk c)
        {
            //Check if the coordinates are inbounds
            if (x < 0 || y < 0 || z < 0 || x >= CHUNK_SIZE || y >= CHUNK_SIZE || z >= CHUNK_SIZE)
            {
                // return true;
                if (x < 0)
                {
                    if (c.neighbors[2] == null)
                    {
                        return true;
                    }
                    int n_x = CHUNK_SIZE - 1;
                    return c.neighbors[2].get(n_x, y, z) == 0;
                }
                if (x >= CHUNK_SIZE)
                {
                    if (c.neighbors[3] == null)
                    {
                        return true;
                    }
                    return c.neighbors[3].get(0, y, z) == 0;
                }
                if (y < 0)
                {
                    if (c.neighbors[1] == null)
                    {
                        return true;
                    }
                    int n_y = CHUNK_SIZE - 1;
                    return c.neighbors[1].get(x, n_y, z) == 0;
                }
                if (y >= CHUNK_SIZE)
                {
                    if (c.neighbors[0] == null)
                    {
                        return true;
                    }
                    return c.neighbors[0].get(x, 0, z) == 0;
                }
                if (z < 0)
                {
                    if (c.neighbors[4] == null)
                    {
                        return true;
                    }
                    int n_z = CHUNK_SIZE - 1;
                    return c.neighbors[4].get(x, y, n_z) == 0;
                }
                if (z >= CHUNK_SIZE)
                {
                    if (c.neighbors[5] == null)
                    {
                        return true;
                    }
                    return c.neighbors[5].get(x, y, 0) == 0;
                }
            }

            return c.get(x, y, z) == 0;

        }
    }
    public interface IVoxelMesher
    {

        MeshInstance CreateChunkMesh(ref VoxelChunk c);
    }

}