using UnityEngine;

public class Chunk
{
    private ChunkData chunkData;
    private TerrainGenerator terrainGenerator;
    private TerrainShaderManager terrainShaderManager;

    public Material Material { get; private set; }
    public Mesh Mesh { get; private set; }
    private bool currentlyRendering = false;

    // variables for indirect rendering
    private GraphicsBuffer meshTriangles;
    private GraphicsBuffer meshPositions;
    private GraphicsBuffer meshNormals;
    private GraphicsBuffer commandBuf;
    private GraphicsBuffer.IndirectDrawArgs[] commandData;
    private Bounds worldBounds;
    private RenderParams renderParams;
    private int vertexCount;
    private const int commandCount = 1;

    public ChunkData GetChunkData()
    {
        return chunkData;
    }

    public class ChunkData 
    {
        public Vector3 position;
        public int chunkX;
        public int chunkY;
        public int chunkZ;
        public int chunkVoxelsSideX;
        public int chunkVoxelsSideY;
        public int chunkVoxelsSideZ;
        public int chunkVoxelsSideRealX;
        public int chunkVoxelsSideRealY;
        public int chunkVoxelsSideRealZ;
        public int chunkCubesSideX;
        public int chunkCubesSideY;
        public int chunkCubesSideZ;
        public int chunkCubesSideRealX;
        public int chunkCubesSideRealY;
        public int chunkCubesSideRealZ;
        public int[] triangles;
        public Vector3[] vertices;
        public Vector3[] normals;
        public float[,,] voxelValues;
        public Vector3[,,] voxelPositions;
        public MarchingCubes.Cube[,,] cubes;
    }

    public Chunk(int chunkX, int chunkY, int chunkZ, Vector3 position, Material material, TerrainGenerator terrainGenerator)
    {
        this.terrainGenerator = terrainGenerator;
        Material = material;

        chunkData = new ChunkData()
        {
            position = position,
            chunkX = chunkX,
            chunkY = chunkY,
            chunkZ = chunkZ,
            chunkVoxelsSideX = chunkX == TerrainData.chunksX - 1 ? TerrainData.lastChunkVoxelsSideX : TerrainData.chunkVoxelsSide,
            chunkVoxelsSideY = chunkY == TerrainData.chunksY - 1 ? TerrainData.lastChunkVoxelsSideY : TerrainData.chunkVoxelsSide,
            chunkVoxelsSideZ = chunkZ == TerrainData.chunksZ - 1 ? TerrainData.lastChunkVoxelsSideZ : TerrainData.chunkVoxelsSide,
            chunkVoxelsSideRealX = chunkX == TerrainData.chunksX - 1 ? TerrainData.lastChunkVoxelsSideRealX : TerrainData.chunkVoxelsSideReal,
            chunkVoxelsSideRealY = chunkY == TerrainData.chunksY - 1 ? TerrainData.lastChunkVoxelsSideRealY : TerrainData.chunkVoxelsSideReal,
            chunkVoxelsSideRealZ = chunkZ == TerrainData.chunksZ - 1 ? TerrainData.lastChunkVoxelsSideRealZ : TerrainData.chunkVoxelsSideReal,
            chunkCubesSideX = chunkX == TerrainData.chunksX - 1 ? TerrainData.lastChunkCubesSideX : TerrainData.chunkCubesSide,
            chunkCubesSideY = chunkY == TerrainData.chunksY - 1 ? TerrainData.lastChunkCubesSideY : TerrainData.chunkCubesSide,
            chunkCubesSideZ = chunkZ == TerrainData.chunksZ - 1 ? TerrainData.lastChunkCubesSideZ : TerrainData.chunkCubesSide,
            chunkCubesSideRealX = chunkX == TerrainData.chunksX - 1 ? TerrainData.lastChunkCubesSideRealX : TerrainData.chunkCubesSideReal,
            chunkCubesSideRealY = chunkY == TerrainData.chunksY - 1 ? TerrainData.lastChunkCubesSideRealY : TerrainData.chunkCubesSideReal,
            chunkCubesSideRealZ = chunkZ == TerrainData.chunksZ - 1 ? TerrainData.lastChunkCubesSideRealZ : TerrainData.chunkCubesSideReal,
        };
    }

    public void GenerateTerrain()
    {
        switch (TerrainData.generationMethod)
        {
            case TerrainData.GenerationMethod.CPU:
                GenerateMeshDataCPU();
                GenerateMeshCPU();
                break;

            case TerrainData.GenerationMethod.CPU_Multithreading:
                GenerateMeshDataCPU();
                terrainGenerator.AddChunkToQueue(this);
                break;

            case TerrainData.GenerationMethod.CPU_GPU:
                RunComputeShader();
                GenerateMeshDataGPU();
                CleanUpComputeBuffers(true);
                GenerateMeshCPU();
                break;

            case TerrainData.GenerationMethod.GPU_Vertex:
                RunComputeShader();
                GenerateMeshGPU(false);
                CleanUpComputeBuffers(false);
                break;

            case TerrainData.GenerationMethod.GPU_Geometry:
                RunComputeShader();
                GenerateMeshGPU(true);
                CleanUpComputeBuffers(false);
                break;
        }

        currentlyRendering = true;
    }

    public void Update()
    {
        if (currentlyRendering && (TerrainData.useIndirectRendering || Mesh == null) && commandData != null)
        {
            Graphics.RenderPrimitivesIndirect(renderParams, MeshTopology.Triangles, commandBuf, commandCount);
        }
        else if (currentlyRendering && Mesh != null)
        {
            RenderParams renderParams = new RenderParams(Material);
            renderParams.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On;
            Graphics.RenderMesh(renderParams, Mesh, 0, Matrix4x4.Translate(chunkData.position));
        }
    }

    private void GenerateMeshDataCPU()
    {        
        VoxelGenerator.GenerateVoxelValuesCPU(chunkData);
        MarchingCubes.RunMarchinCubes(chunkData);
        MarchingCubes.GenerateMeshData(chunkData);
    }

    private void RunComputeShader()
    {
        terrainShaderManager = new TerrainShaderManager();
        terrainShaderManager.InitShader(chunkData);
        terrainShaderManager.RunShader();
    }

    private void GenerateMeshDataGPU()
    {
        Timer.StartMeasurement(Timer.MeasurementType.Mesh_Building, chunkData);
        terrainShaderManager.GenerateMeshData();
    }

    private void CleanUpComputeBuffers(bool clearOutputBuffer)
    {
        terrainShaderManager.CleanUp(clearOutputBuffer);
    }

    public void GenerateMeshCPU()
    {
        TerrainData.actualTriangleCount += chunkData.vertices.LongLength / 3;

        if (TerrainData.useIndirectRendering)
        {
            if (chunkData.vertices.Length > 0)
            {
                meshTriangles = new GraphicsBuffer(GraphicsBuffer.Target.Structured, chunkData.triangles.Length, sizeof(int));
                meshTriangles.SetData(chunkData.triangles);
                meshPositions = new GraphicsBuffer(GraphicsBuffer.Target.Structured, chunkData.vertices.Length, 3 * sizeof(float));
                meshPositions.SetData(chunkData.vertices);
                meshNormals = new GraphicsBuffer(GraphicsBuffer.Target.Structured, chunkData.normals.Length, 3 * sizeof(float));
                meshNormals.SetData(chunkData.normals);
                vertexCount = chunkData.vertices.Length;
                PrepareIndirectRendering();
            }
        }
        else
        {
            Mesh = new Mesh();
            Mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
            Mesh.vertices = chunkData.vertices;
            Mesh.triangles = chunkData.triangles;
            Mesh.normals = chunkData.normals;
        }
        Timer.EndMeasurement(chunkData);
    }

    public void GenerateMeshGPU(bool useIndices)
    {
        terrainGenerator.DestroyChunkMesh(this);

        Timer.StartMeasurement(Timer.MeasurementType.Mesh_Building, chunkData);

        if (TerrainData.shouldMeshesAndBuffersBeTrimmed)
        {
            int triangleCount = terrainShaderManager.GetTriangleCount();
            TerrainData.actualTriangleCount += triangleCount;

            if (TerrainData.useIndirectRendering)
            {
                if (triangleCount > 0)
                {
                    vertexCount = triangleCount * 3;
                    PrepareIndirectRendering();
                }
            }
            else
            {
                Mesh = new Mesh();
                Mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;

                int vertexCount = useIndices ? triangleCount : triangleCount * 3;
                Vector3[] vertices = new Vector3[vertexCount];
                int[] triangles = new int[vertexCount];
                for (int i = 0; i < vertexCount; i++)
                {
                    triangles[i] = i;
                }

                Mesh.vertices = vertices;
                if (useIndices)
                {
                    Mesh.SetIndices(triangles, MeshTopology.Points, 0);
                }
                else
                {
                    Mesh.triangles = triangles;
                }
                Mesh.bounds = new Bounds(Vector3.zero, new Vector3(TerrainData.chunksSize, TerrainData.chunksSize, TerrainData.chunksSize));
            }
        }
        else
        {
            Mesh genericMesh = useIndices ? terrainGenerator.GetGenericChunkMesh_Geometry() : terrainGenerator.GetGenericChunkMesh_Vertex();

            if (TerrainData.useIndirectRendering)
            {
                vertexCount = genericMesh.vertexCount;
                PrepareIndirectRendering();
            }
            else
            {
                Mesh = genericMesh;
            }
        }

        Material.SetBuffer("triangleBuffer", terrainShaderManager.GetOutputBuffer());
        Timer.EndMeasurement(chunkData);
    }

    private void PrepareIndirectRendering()
    {
        commandBuf = new GraphicsBuffer(GraphicsBuffer.Target.IndirectArguments, commandCount, GraphicsBuffer.IndirectDrawArgs.size);
        commandData = new GraphicsBuffer.IndirectDrawArgs[commandCount];
        worldBounds = new Bounds(chunkData.position, new Vector3(TerrainData.chunksSize, TerrainData.chunksSize, TerrainData.chunksSize));

        renderParams = new RenderParams(Material);
        renderParams.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On;
        renderParams.worldBounds = worldBounds;
        renderParams.matProps = new MaterialPropertyBlock();
        if (!TerrainData.IsKeepingGpuMemory())
        {
            renderParams.matProps.SetBuffer("_Triangles", meshTriangles);
            renderParams.matProps.SetBuffer("_Positions", meshPositions);
            renderParams.matProps.SetBuffer("_Normals", meshNormals);
        }
        renderParams.matProps.SetInt("_BaseVertexIndex", 0);
        renderParams.matProps.SetMatrix("_ObjectToWorld", Matrix4x4.Translate(chunkData.position));

        commandData[0].vertexCountPerInstance = (uint)vertexCount;
        commandData[0].instanceCount = commandCount;

        commandBuf.SetData(commandData);
    }

    public void OnDestroy()
    {
        currentlyRendering = false;
        terrainShaderManager?.CleanUp(true);
        meshTriangles?.Dispose();
        meshTriangles = null;
        meshPositions?.Dispose();
        meshPositions = null;
        meshNormals?.Dispose();
        meshNormals = null;
        commandBuf?.Dispose();
        commandBuf = null;
    }
}