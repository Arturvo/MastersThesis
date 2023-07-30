using UnityEngine;
using System.Runtime.InteropServices;

public static class TerrainData
{
    // data
    public static Chunk[,,] chunks;

    // GPU buffer size determines size of chunks
    // public static readonly uint maxBufferSize = 2147483648;

    // parameters
    public static GenerationMethod generationMethod = GenerationMethod.CPU_GPU;
    public static bool smoothNormals = true;
    public static bool interpolateVertices = true;
    public static bool useAppendBuffers = true;
    public static bool trimMeshesAndBuffers = false;
    public static bool useIndirectRendering = false;
    public static bool useSimpleShaders = false;
    public static bool showChunkBorders = false;
    public static float chunkSize;
    public static int chunkTriangleLimit;
    public static float terrainSizeX;
    public static float terrainSizeY;
    public static float terrainSizeZ;
    public static int cubesPerUnit;
    // noise
    public static int seed;
    public static float noiseFrequency;
    public static float noiseStrength;
    public static int noiseLayers;
    public static float noiseLayerFrequencyMultiplier;
    // debug
    public static bool showGenerationTime = true;
    public static bool useOnlyMiliseconds = true;

    // paramter limits and slider step
    public static float chunkSize_STEP = 0.1f;
    public static float chunkSize_MIN = 20f;
    public static float chunkSize_MAX = 30f;
    public static int chunkTriangleLimit_STEP = 1;
    public static int chunkTriangleLimit_MIN = 1;
    public static int chunkTriangleLimit_MAX = 100;
    public static float terrainSize_STEP = 0.5f;
    public static float terrainSizeX_MIN = 1f;
    public static float terrainSizeX_MAX = 100f;
    public static float terrainSizeY_MIN = 1f;
    public static float terrainSizeY_MAX = 100f;
    public static float terrainSizeZ_MIN = 1f;
    public static float terrainSizeZ_MAX = 100f;
    public static int cubesPerUnit_MIN = 1;
    public static int cubesPerUnit_MAX = 50;
    public static int seed_MIN = 0;
    public static int seed_MAX = 999999;
    public static float noiseFrequency_STEP = 0.01f;
    public static float noiseFrequency_MIN = 0.01f;
    public static float noiseFrequency_MAX = 100f;
    public static float noiseStrength_STEP = 0.01f;
    public static float noiseStrength_MIN = 0.01f;
    public static float noiseStrength_MAX = 1f;
    public static int noiseLayers_MIN = 1;
    public static int noiseLayers_MAX = 10;
    public static float noiseLayerFrequencyMultiplier_STEP = 0.05f;
    public static float noiseLayerFrequencyMultiplier_MIN = 1f;
    public static float noiseLayerFrequencyMultiplier_MAX = 3f;

    // computed parameters
    public static bool multithreading;
    public static bool shouldMeshesAndBuffersBeTrimmed;
    public static long gpuMemoryUsedBytes;
    public static long actualTriangleCount;
    public static bool useGlobalPosition;
    public static uint bufferSize;
    public static float cubeSize;
    public static float maxTerrainSize;
    public static int voxelStructSize;
    public static int triangleStructSize;
    public static int cubeStructSize;
    public static int maxCubesInBuffer;
    // chunk dimentions in cubes (chunkCubesSide * chunkCubesSide * chunkCubesSide)
    public static int chunkCubesSide;
    // one layer of cubes on each side is used only to calculate smooth normals and discarded
    public static int chunkCubesSideReal;
    public static int chunkVoxelsSide;
    public static int chunkVoxelsSideReal;
    public static float chunksSize;
    public static int chunksX;
    public static int chunksY;
    public static int chunksZ;
    public static int totalChunks;
    // last chunks in each axis will be a little bit smaller
    public static int totalCubesX;
    public static int totalCubesY;
    public static int totalCubesZ;
    public static int lastChunkCubesSideRealX;
    public static int lastChunkCubesSideRealY;
    public static int lastChunkCubesSideRealZ;
    public static int lastChunkCubesSideX;
    public static int lastChunkCubesSideY;
    public static int lastChunkCubesSideZ;
    public static int lastChunkVoxelsSideX;
    public static int lastChunkVoxelsSideY;
    public static int lastChunkVoxelsSideZ;
    public static int lastChunkVoxelsSideRealX;
    public static int lastChunkVoxelsSideRealY;
    public static int lastChunkVoxelsSideRealZ;

    public enum GenerationMethod
    {
        CPU,
        CPU_Multithreading,
        CPU_GPU,
        GPU_Vertex,
        GPU_Geometry,
    }

    // structures
    public struct Voxel
    {
        public Vector3 position;
        public float value;
    }

    public struct TriangleTmp
    {
        public Vector3 v1, v2, v3;
        public Vector3 n;
    };

    public struct Triangle
    {
        public Vector3 v1, v2, v3;
        public Vector3 n1, n2, n3;
    }

    public static void ComputeParameters()
    {
        bufferSize = (uint) Mathf.RoundToInt(Mathf.Pow(2f, chunkSize));
        multithreading = generationMethod == GenerationMethod.CPU_Multithreading;
        useGlobalPosition = (generationMethod == GenerationMethod.GPU_Geometry && !(useSimpleShaders && !useIndirectRendering)) || (generationMethod == GenerationMethod.GPU_Vertex && useIndirectRendering);
        shouldMeshesAndBuffersBeTrimmed = useAppendBuffers && trimMeshesAndBuffers && IsKeepingGpuMemory();
        cubeSize = 1f / cubesPerUnit;
        maxTerrainSize = Mathf.Max(terrainSizeX_MAX, terrainSizeY_MAX, terrainSizeZ_MAX);
        voxelStructSize = Marshal.SizeOf(typeof(Voxel));
        triangleStructSize = Marshal.SizeOf(typeof(Triangle));
        cubeStructSize = 5 * triangleStructSize;
        maxCubesInBuffer = (int)(bufferSize / cubeStructSize);
        chunkCubesSide = Mathf.FloorToInt(Mathf.Pow(maxCubesInBuffer, 1f / 3f));
        chunkCubesSideReal = chunkCubesSide - 2;
        chunkVoxelsSide = chunkCubesSide + 1;
        chunkVoxelsSideReal = chunkVoxelsSide - 2;
        chunksSize = chunkCubesSideReal * cubeSize;
        chunksX = Mathf.CeilToInt(terrainSizeX / chunksSize);
        chunksY = Mathf.CeilToInt(terrainSizeY / chunksSize);
        chunksZ = Mathf.CeilToInt(terrainSizeZ / chunksSize);
        totalChunks = chunksX * chunksY * chunksZ;
        totalCubesX = Mathf.RoundToInt(terrainSizeX / cubeSize);
        totalCubesY = Mathf.RoundToInt(terrainSizeY / cubeSize);
        totalCubesZ = Mathf.RoundToInt(terrainSizeZ / cubeSize);
        lastChunkCubesSideRealX = totalCubesX - (chunksX - 1) * chunkCubesSideReal;
        lastChunkCubesSideRealY = totalCubesY - (chunksY - 1) * chunkCubesSideReal;
        lastChunkCubesSideRealZ = totalCubesZ - (chunksZ - 1) * chunkCubesSideReal;
        lastChunkCubesSideX = lastChunkCubesSideRealX + 2;
        lastChunkCubesSideY = lastChunkCubesSideRealY + 2;
        lastChunkCubesSideZ = lastChunkCubesSideRealZ + 2;
        lastChunkVoxelsSideX = lastChunkCubesSideX + 1;
        lastChunkVoxelsSideY = lastChunkCubesSideY + 1;
        lastChunkVoxelsSideZ = lastChunkCubesSideZ + 1;
        lastChunkVoxelsSideRealX = lastChunkVoxelsSideX - 2;
        lastChunkVoxelsSideRealY = lastChunkVoxelsSideY - 2;
        lastChunkVoxelsSideRealZ = lastChunkVoxelsSideZ - 2;
        gpuMemoryUsedBytes = 0;
        actualTriangleCount = 0;
    }

    public static bool IsKeepingGpuMemory()
    {
        return generationMethod == GenerationMethod.GPU_Vertex || generationMethod == GenerationMethod.GPU_Geometry;
    }

    public static void ApplyPreset(TerrainDataPreset preset)
    {
        chunkSize = preset.ChunkSize;
        chunkTriangleLimit = preset.ChunkTriangleLimit;
        terrainSizeX = preset.TerrainSizeX;
        terrainSizeY = preset.TerrainSizeY;
        terrainSizeZ = preset.TerrainSizeZ;
        cubesPerUnit = preset.CubesPerUnit;
        seed = preset.Seed;
        noiseFrequency = preset.NoiseFrequency;
        noiseStrength = preset.NoiseStrength;
        noiseLayers = preset.NoiseLayers;
        noiseLayerFrequencyMultiplier = preset.NoiseLayerFrequencyMultiplier;
    }
}
