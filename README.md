# GodotMonoVoxel
This is my attempt at making a voxel engine using C# and Godot game engine.

Libraries used:

- [https://github.com/paulohyy/linerenderer](https://github.com/paulohyy/linerenderer)
    - For bullet line tracers
- [https://www.nuget.org/packages/Snappy.NET/](https://www.nuget.org/packages/Snappy.NET/)
    - For voxel chunk compression and decompression
- [https://github.com/Zylann/godot_terrain_plugin](https://github.com/Zylann/godot_terrain_plugin)
    - I have this as an addon, but haven't gotten around to using it


## Usage

```
VoxelVolume basicVoxel = new VoxelVolume(new NoisePopulator(),128,1024,1024,64,0.25f);
```
`VoxelVolume` takes in an implementation of `IVoxelDataPopulate`, volume height, volume length, volume width, chunk size, and voxel size.
- The main voxel code can be found under `src/goxlap`
    - `voxelworld.cs` the entry point to the voxel engine, contains the `VoxelVolume`, `IVoxelDataPopulate` along with the implementations of the interface
    - `chunkmanager.cs` contains the `ChunkManager` class
        - `ChunkManager` basic loading of populated voxel chunks
        - `ChunkStruct` voxel chunk data structure that contains a volume offset, voxel data, compressed voxel data
    - `blockMesher.cs` contains the block meshing code
    - `chunk.cs` depricated
    - `voxelUtils.cs` contains the enum for the voxel types (I need to refactor to use this throughout the code)


## Planned Features (updated as I go)

- Threaded voxel chunk generation
- Some way to batch the voxel chunks using `MeshInstance`
- Camera location based chunk loading
- Greedy meshing for block mesher
    - Preliminary implementation done, need to speed up mesh generation 
- Other voxel mesher algorithm (marching cubes, dual contouring)
- Voxel chunk collision based on the type of mesher used
    - i.e. block mesher would use box colliders 