using Godot;
using System;

namespace Goxlap.src.Goxlap
{
    public class VoxelConstants
    {
        public static int CHUNK_SIZE { get; set; } = 64;
        public static int CHUNK_SIZE_MAX { get; set; } = CHUNK_SIZE * CHUNK_SIZE * CHUNK_SIZE;

        public static float VOX_SIZE { get; set; } = 1.0f;
        public static int WORLD_SIZE_MAX_X { get; set; }
        public static int WORLD_SIZE_MAX_Y { get; set; }
        public static int WORLD_SIZE_MAX_Z { get; set; }
    }

}