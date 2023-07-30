using System;
using System.Collections.Generic;
using System.Diagnostics;

public static class Timer
{
    private static MeasurementSet[,,] chunkMeasurements;
    private static Stopwatch totalMultithreadingTimeStopWatch;
    private static Stopwatch renderingStopWatch;

    private class MeasurementSet
    {
        private Stopwatch stopwatch;
        private MeasurementType currentMeasurement;
        private TimeSpan voxelGenerationMeasurement;
        private TimeSpan marchingCubesMeasurement;
        private TimeSpan meshSmoothingMeasurement;
        private TimeSpan meshBuildingMeasurement;
        private TimeSpan bufferTrimmingMeasurement;

        public MeasurementSet()
        {
            stopwatch = new Stopwatch();
        }

        public void StartMeasurement(MeasurementType measurementType)
        {
            stopwatch.Start();
            currentMeasurement = measurementType;
        }

        public void StopMeasurement()
        {
            stopwatch.Stop();

            switch (currentMeasurement)
            {
                case MeasurementType.Voxel_Generation:
                    voxelGenerationMeasurement = stopwatch.Elapsed;
                    break;
                case MeasurementType.Marching_Cubes:
                    marchingCubesMeasurement = stopwatch.Elapsed;
                    break;
                case MeasurementType.Mesh_Smoothing:
                    meshSmoothingMeasurement = stopwatch.Elapsed;
                    break;
                case MeasurementType.Mesh_Building:
                    meshBuildingMeasurement = stopwatch.Elapsed;
                    break;
                case MeasurementType.MeshAndBuffer_Trimming:
                    bufferTrimmingMeasurement = stopwatch.Elapsed;
                    break;
            }

            stopwatch.Reset();
        }
        
        public float GetMeasurement(MeasurementType measurementType)
        {
            return measurementType switch
            {
                MeasurementType.Voxel_Generation => (float) voxelGenerationMeasurement.TotalMilliseconds,
                MeasurementType.Marching_Cubes => (float) marchingCubesMeasurement.TotalMilliseconds,
                MeasurementType.Mesh_Smoothing => (float) meshSmoothingMeasurement.TotalMilliseconds,
                MeasurementType.Mesh_Building => (float) meshBuildingMeasurement.TotalMilliseconds,
                MeasurementType.MeshAndBuffer_Trimming => (float) bufferTrimmingMeasurement.TotalMilliseconds,
                _ => 0.0f,
            };
        }
    }

    public enum MeasurementType
    {
        Voxel_Generation,
        Marching_Cubes,
        Mesh_Smoothing,
        Mesh_Building,
        MeshAndBuffer_Trimming,
        Mesh_Rendering,
        Total_Time
    }

    public static void Init()
    {
        renderingStopWatch = new Stopwatch();
        if (TerrainData.multithreading)
        {
            totalMultithreadingTimeStopWatch = new Stopwatch();
            totalMultithreadingTimeStopWatch.Start();
        }

        chunkMeasurements = new MeasurementSet[TerrainData.chunksX, TerrainData.chunksY, TerrainData.chunksZ];

        for (int x = 0; x < TerrainData.chunksX; x++)
        {
            for (int y = 0; y < TerrainData.chunksY; y++)
            {
                for(int z = 0; z < TerrainData.chunksZ; z++)
                {
                    chunkMeasurements[x,y,z] = new MeasurementSet();
                }
            }
        }
    }

    public static void StartMeasurement(MeasurementType measurementType, Chunk.ChunkData chunkData)
    {
        MeasurementSet measurementSet = chunkMeasurements[chunkData.chunkX, chunkData.chunkY, chunkData.chunkZ];
        measurementSet.StartMeasurement(measurementType);
    }

    public static void EndMeasurement(Chunk.ChunkData chunkData)
    {
        MeasurementSet measurementSet = chunkMeasurements[chunkData.chunkX, chunkData.chunkY, chunkData.chunkZ];
        measurementSet.StopMeasurement();
    }

    public static void StartRenderingMeasurement()
    {
        renderingStopWatch.Start();
    }

    public static void EndRenderingMeasurement()
    {
        renderingStopWatch.Stop();
    }

    public static Dictionary<MeasurementType, float> GetMeasurements()
    {
        float totalVoxelGenerationTime = 0;
        float totalMarchingCubesTime = 0;
        float totalMeshSmoothingTime = 0;
        float totalMeshBuildingTime = 0;
        float totalBufferTrimmingTime = 0;

        for (int x = 0; x < TerrainData.chunksX; x++)
        {
            for (int y = 0; y < TerrainData.chunksY; y++)
            {
                for (int z = 0; z < TerrainData.chunksZ; z++)
                {
                    MeasurementSet measurementSet = chunkMeasurements[x, y, z];
                    totalVoxelGenerationTime += measurementSet.GetMeasurement(MeasurementType.Voxel_Generation);
                    totalMarchingCubesTime += measurementSet.GetMeasurement(MeasurementType.Marching_Cubes);
                    totalMeshSmoothingTime += measurementSet.GetMeasurement(MeasurementType.Mesh_Smoothing);
                    totalMeshBuildingTime += measurementSet.GetMeasurement(MeasurementType.Mesh_Building);
                    totalBufferTrimmingTime += measurementSet.GetMeasurement(MeasurementType.MeshAndBuffer_Trimming);
                }
            }
        }

        float renderingTime = (float)renderingStopWatch.Elapsed.TotalMilliseconds;
        float totalTime = totalVoxelGenerationTime + totalMarchingCubesTime + totalMeshSmoothingTime + totalMeshBuildingTime + totalBufferTrimmingTime + renderingTime;

        if (TerrainData.multithreading)
        {
            totalMultithreadingTimeStopWatch.Stop();
            totalTime = (float)totalMultithreadingTimeStopWatch.Elapsed.TotalMilliseconds - renderingTime;
            float averageVoxelGenerationTime = totalVoxelGenerationTime / TerrainData.totalChunks;
            float averageMarchingCubesTime = totalMarchingCubesTime / TerrainData.totalChunks;
            float averageMeshSmoothingTime = totalMeshSmoothingTime / TerrainData.totalChunks;
            float averageMeshBuildingTime = totalMeshBuildingTime / TerrainData.totalChunks;
            float averageBufferTrimmingTime = totalBufferTrimmingTime / TerrainData.totalChunks;
            float totalAverageTime = averageVoxelGenerationTime + averageMarchingCubesTime + averageMeshSmoothingTime + averageMeshBuildingTime + averageBufferTrimmingTime;

            totalVoxelGenerationTime = (averageVoxelGenerationTime / totalAverageTime) * totalTime;
            totalMarchingCubesTime = (averageMarchingCubesTime / totalAverageTime) * totalTime;
            totalMeshSmoothingTime = (averageMeshSmoothingTime / totalAverageTime) * totalTime;
            totalMeshBuildingTime = (averageMeshBuildingTime / totalAverageTime) * totalTime;
            totalBufferTrimmingTime = (averageBufferTrimmingTime / totalAverageTime) * totalTime;
        }

        return new Dictionary<MeasurementType, float>
        {
            {MeasurementType.Voxel_Generation, totalVoxelGenerationTime },
            {MeasurementType.Marching_Cubes, totalMarchingCubesTime },
            {MeasurementType.Mesh_Smoothing, totalMeshSmoothingTime },
            {MeasurementType.MeshAndBuffer_Trimming, totalBufferTrimmingTime },
            {MeasurementType.Mesh_Building, totalMeshBuildingTime },
            {MeasurementType.Mesh_Rendering, renderingTime },
            {MeasurementType.Total_Time, totalTime }
        };
    }
}
