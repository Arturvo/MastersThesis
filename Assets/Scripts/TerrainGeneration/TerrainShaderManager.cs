using System.Runtime.InteropServices;
using UnityEngine;

public class TerrainShaderManager
{
    private ComputeShader terrainComputeShader;
    private ComputeBuffer voxelBuffer;
    private ComputeBuffer triangleTmpBuffer;
    private ComputeBuffer triangleBuffer;
    private ComputeBuffer outputTriangleBuffer;
    private ComputeBuffer triangleCountBuffer;

    Kernel generateVoxelValuesKernel;
    Kernel runMarchinCubesKernel;
    Kernel smoothNormalsKernel;
    Kernel clearOutputAppendBufferKernel;
    Kernel createOutputAppendKernel;
    Kernel createOutputNoAppendKernel;

    private Chunk.ChunkData chunkData;
    private int totalVoxels;
    private int totalCubes;
    private int totalTriangles;
    private int outputTringlesCount;
    private int exactTriangleCount;
    private bool wasExactTriangleCountCalculated;

    public void InitShader(Chunk.ChunkData chunkData)
    {
        this.chunkData = chunkData;
        terrainComputeShader = (ComputeShader)Resources.Load("Shaders/TerrainGenerator");
        terrainComputeShader.SetFloat("chunkPositionX", chunkData.position.x);
        terrainComputeShader.SetFloat("chunkPositionY", chunkData.position.y);
        terrainComputeShader.SetFloat("chunkPositionZ", chunkData.position.z);
        terrainComputeShader.SetInt("interpolateVertices", TerrainData.interpolateVertices ? 1 : 0);
        terrainComputeShader.SetInt("normalSmoothRange", TerrainData.smoothNormals ? 1 : 0);
        terrainComputeShader.SetInt("isTopChunk", chunkData.chunkY == TerrainData.chunksY - 1 ? 1 : 0);
        terrainComputeShader.SetInt("isBottomChunk", chunkData.chunkY == 0 ? 1 : 0);
        terrainComputeShader.SetInt("useGlobalPosition", TerrainData.useGlobalPosition ? 1 : 0);
        terrainComputeShader.SetInt("chunkMaxVoxelsSide", TerrainData.chunkVoxelsSide);
        terrainComputeShader.SetInt("chunkVoxelsSideX", chunkData.chunkVoxelsSideX);
        terrainComputeShader.SetInt("chunkVoxelsSideY", chunkData.chunkVoxelsSideY);
        terrainComputeShader.SetInt("chunkVoxelsSideZ", chunkData.chunkVoxelsSideZ);
        terrainComputeShader.SetInt("chunkMaxCubesSide", TerrainData.chunkCubesSide);
        terrainComputeShader.SetInt("chunkCubesSideX", chunkData.chunkCubesSideX);
        terrainComputeShader.SetInt("chunkCubesSideY", chunkData.chunkCubesSideY);
        terrainComputeShader.SetInt("chunkCubesSideZ", chunkData.chunkCubesSideZ);
        terrainComputeShader.SetFloat("cubeSize", TerrainData.cubeSize);
        terrainComputeShader.SetFloat("maxTerrainSize", TerrainData.maxTerrainSize);
        // noise
        terrainComputeShader.SetInt("seed", TerrainData.seed);
        terrainComputeShader.SetInt("noiseLayers", TerrainData.noiseLayers);
        terrainComputeShader.SetFloat("noiseFrequency", TerrainData.noiseFrequency);
        terrainComputeShader.SetFloat("noiseStrength", TerrainData.noiseStrength);
        terrainComputeShader.SetFloat("noiseLayerFrequencyMultiplier", TerrainData.noiseLayerFrequencyMultiplier);
        generateVoxelValuesKernel = new Kernel(terrainComputeShader, "GenerateVoxelValues");
        runMarchinCubesKernel = new Kernel(terrainComputeShader, "RunMarchinCubes");
        smoothNormalsKernel = new Kernel(terrainComputeShader, "SmoothNormals");
        createOutputAppendKernel = new Kernel(terrainComputeShader, "CreateOutputAppend");
        createOutputNoAppendKernel = new Kernel(terrainComputeShader, "CreateOutputNoAppend");
        clearOutputAppendBufferKernel = new Kernel(terrainComputeShader, "ClearOutputAppendBuffer");
    }

    public void RunShader()
    {
        wasExactTriangleCountCalculated = false;
        totalVoxels = TerrainData.chunkVoxelsSide * TerrainData.chunkVoxelsSide * TerrainData.chunkVoxelsSide;
        totalCubes = TerrainData.chunkCubesSide * TerrainData.chunkCubesSide * TerrainData.chunkCubesSide;
        totalTriangles = totalCubes * 5;
        outputTringlesCount = chunkData.chunkCubesSideRealX * chunkData.chunkCubesSideRealY * chunkData.chunkCubesSideRealZ * 5;
        if (TerrainData.chunkTriangleLimit < 100 && TerrainData.IsKeepingGpuMemory() && !TerrainData.trimMeshesAndBuffers && TerrainData.useAppendBuffers)
        {
            outputTringlesCount = Mathf.RoundToInt(outputTringlesCount * (TerrainData.chunkTriangleLimit / 100f));
        }

        GenerateVoxels();
        RunMarchingCubes();
        SmoothNormals();
        CreateOutput();

        if (TerrainData.shouldMeshesAndBuffersBeTrimmed)
        {
            TrimOutput();
        }
        else if (TerrainData.IsKeepingGpuMemory() && !TerrainData.useAppendBuffers)
        {
            TerrainData.gpuMemoryUsedBytes += totalTriangles * Marshal.SizeOf(typeof(TerrainData.Triangle));
        }
        else
        {
            TerrainData.gpuMemoryUsedBytes += outputTringlesCount * Marshal.SizeOf(typeof(TerrainData.Triangle));
        }
    }

    private void GenerateVoxels()
    {
        Timer.StartMeasurement(Timer.MeasurementType.Voxel_Generation, chunkData);
        voxelBuffer = new ComputeBuffer(totalVoxels, Marshal.SizeOf(typeof(TerrainData.Voxel)));
        generateVoxelValuesKernel.SetBuffer("voxelBuffer", voxelBuffer);
        generateVoxelValuesKernel.Dispatch(totalVoxels);
        Timer.EndMeasurement(chunkData);
    }

    private void RunMarchingCubes()
    {
        Timer.StartMeasurement(Timer.MeasurementType.Marching_Cubes, chunkData);
        triangleTmpBuffer = new ComputeBuffer(totalTriangles, Marshal.SizeOf(typeof(TerrainData.TriangleTmp)));
        runMarchinCubesKernel.SetBuffer("voxelBuffer", voxelBuffer);
        runMarchinCubesKernel.SetBuffer("triangleTmpBuffer", triangleTmpBuffer);
        runMarchinCubesKernel.Dispatch(totalCubes);
        voxelBuffer?.Dispose();
        Timer.EndMeasurement(chunkData);
    }

    private void SmoothNormals()
    {
        Timer.StartMeasurement(Timer.MeasurementType.Mesh_Smoothing, chunkData);
        triangleBuffer = new ComputeBuffer(totalTriangles, Marshal.SizeOf(typeof(TerrainData.Triangle)));
        smoothNormalsKernel.SetBuffer("triangleTmpBuffer", triangleTmpBuffer);
        smoothNormalsKernel.SetBuffer("triangleBuffer", triangleBuffer);
        smoothNormalsKernel.Dispatch(totalTriangles);
        triangleTmpBuffer?.Dispose();
        Timer.EndMeasurement(chunkData);
    }

    private void CreateOutput()
    {
        if (TerrainData.IsKeepingGpuMemory() && !TerrainData.useAppendBuffers)
        {
            outputTriangleBuffer = new ComputeBuffer(totalTriangles, Marshal.SizeOf(typeof(TerrainData.Triangle)));
            createOutputNoAppendKernel.SetBuffer("triangleBuffer", triangleBuffer);
            createOutputNoAppendKernel.SetBuffer("outputTriangleBufferNoAppend", outputTriangleBuffer);
            createOutputNoAppendKernel.Dispatch(totalTriangles);
        }
        else if (outputTringlesCount > 0)
        {
            outputTriangleBuffer = new ComputeBuffer(outputTringlesCount, Marshal.SizeOf(typeof(TerrainData.Triangle)), ComputeBufferType.Append);
            outputTriangleBuffer.SetCounterValue(0);
            clearOutputAppendBufferKernel.SetBuffer("triangleBuffer", triangleBuffer);
            clearOutputAppendBufferKernel.SetBuffer("outputTriangleBufferAppend", outputTriangleBuffer);
            clearOutputAppendBufferKernel.Dispatch(outputTringlesCount);
            outputTriangleBuffer.SetCounterValue(0);
            createOutputAppendKernel.SetBuffer("triangleBuffer", triangleBuffer);
            createOutputAppendKernel.SetBuffer("outputTriangleBufferAppend", outputTriangleBuffer);
            createOutputAppendKernel.Dispatch(totalTriangles);
            if (!TerrainData.shouldMeshesAndBuffersBeTrimmed)
            {
                triangleBuffer?.Dispose();
            }
        }
    }

    private void TrimOutput()
    {
        Timer.StartMeasurement(Timer.MeasurementType.MeshAndBuffer_Trimming, chunkData);
        
        int exactTriangleCount = GetTriangleCount();
        if (exactTriangleCount == 0) return;

        outputTriangleBuffer.Release();
        outputTriangleBuffer = new ComputeBuffer(exactTriangleCount, Marshal.SizeOf(typeof(TerrainData.Triangle)), ComputeBufferType.Append);
        outputTriangleBuffer.SetCounterValue(0);
        createOutputAppendKernel.SetBuffer("triangleBuffer", triangleBuffer);
        createOutputAppendKernel.SetBuffer("outputTriangleBufferAppend", outputTriangleBuffer);
        createOutputAppendKernel.Dispatch(totalTriangles);
        triangleBuffer?.Dispose();

        Timer.EndMeasurement(chunkData);

        TerrainData.gpuMemoryUsedBytes += exactTriangleCount * Marshal.SizeOf(typeof(TerrainData.Triangle));
    }

    public void GenerateMeshData()
    {
        // Get triangle data from shader
        int triangleCount = GetTriangleCount();
        TerrainData.Triangle[] triangleStructs = new TerrainData.Triangle[triangleCount];
        outputTriangleBuffer.GetData(triangleStructs, 0, 0, triangleCount);

        Vector3[] vertices = new Vector3[triangleCount * 3];
        int[] triangles = new int[triangleCount * 3];
        Vector3[] normals = new Vector3[triangleCount * 3];

        for (int i = 0; i < triangleCount; i++)
        {
            TerrainData.Triangle triangleStruct = triangleStructs[i];
            vertices[i * 3] = triangleStruct.v1;
            vertices[i * 3 + 1] = triangleStruct.v2;
            vertices[i * 3 + 2] = triangleStruct.v3;
            triangles[i * 3] = i * 3;
            triangles[i * 3 + 1] = i * 3 + 1;
            triangles[i * 3 + 2] = i * 3 + 2;
            normals[i * 3] = triangleStruct.n1;
            normals[i * 3 + 1] = triangleStruct.n2;
            normals[i * 3 + 2] = triangleStruct.n3;
        }

        chunkData.vertices = vertices;
        chunkData.triangles = triangles;
        chunkData.normals = normals;
    }

    public int GetTriangleCount()
    {
        if (wasExactTriangleCountCalculated) return exactTriangleCount;

        // Get number of triangles in the triangle buffer
        triangleCountBuffer = new ComputeBuffer(1, sizeof(int), ComputeBufferType.Raw);
        ComputeBuffer.CopyCount(outputTriangleBuffer, triangleCountBuffer, 0);
        int[] triangleCountArray = { 0 };
        triangleCountBuffer.GetData(triangleCountArray);
        
        exactTriangleCount = triangleCountArray[0];
        wasExactTriangleCountCalculated = true;
        return exactTriangleCount;
    }

    public ComputeBuffer GetOutputBuffer()
    {
        return outputTriangleBuffer;
    }

    public struct Kernel
    {
        private readonly int kernelNum;
        private readonly int threadNum;
        private readonly ComputeShader computeShader;

        public Kernel(ComputeShader computeShader, string kernelName)
        {
            this.computeShader = computeShader;
            kernelNum = this.computeShader.FindKernel(kernelName);
            this.computeShader.GetKernelThreadGroupSizes(kernelNum, out uint threadNumOutX, out _, out _);
            threadNum = (int) threadNumOutX;
        }

        public void SetBuffer(string bufferName, ComputeBuffer computeBuffer)
        {
            computeShader.SetBuffer(kernelNum, bufferName, computeBuffer);
        }

        public void Dispatch(int totalThreads)
        {
            computeShader.Dispatch(kernelNum, Mathf.CeilToInt((float)totalThreads / threadNum), 1, 1);
        }
    }

    public void CleanUp(bool clearOutputBuffer)
    {
        voxelBuffer?.Dispose();
        triangleTmpBuffer?.Dispose();
        triangleBuffer?.Dispose();
        triangleCountBuffer?.Dispose();
        if (clearOutputBuffer)
        {
            outputTriangleBuffer?.Dispose();
        }
    }
}
