using System.Collections.Generic;
using UnityEngine;
using System.Threading;
using System;
using System.Collections.Concurrent;
using static Chunk;

public class TerrainGenerator : MonoBehaviour
{
    [SerializeField] private LineRenderer chunkBorderLinePrefab;
    [SerializeField] private Material chunkMaterialCPU;
    [SerializeField] private Material chunkMaterialGPU_Vertex;
    [SerializeField] private Material chunkMaterialGPU_Geometry;
    [SerializeField] private Material chunkMaterialCPUSimple;
    [SerializeField] private Material chunkMaterialGPU_SimpleVertex;
    [SerializeField] private Material chunkMaterialGPU_SimpleGeometry;
    [SerializeField] private Material chunkMaterialCPUIndirect;
    [SerializeField] private Material chunkMaterialGPU_IndirectVertex;
    [SerializeField] private Material chunkMaterialGPU_IndirectGeometry;
    [SerializeField] private Material chunkMaterialCPUIndirectSimple;
    [SerializeField] private Material chunkMaterialGPU_IndirectSimpleVertex;
    [SerializeField] private Material chunkMaterialGPU_IndirectSimpleGeometry;

    private Dictionary<TerrainData.GenerationMethod, Material> chunkMaterials;
    private Dictionary<TerrainData.GenerationMethod, Material> chunkMaterialsSimple;
    private Dictionary<TerrainData.GenerationMethod, Material> chunkMaterialsIndirect;
    private Dictionary<TerrainData.GenerationMethod, Material> chunkMaterialsIndirectSimple;

    private ConcurrentQueue<Chunk> chunkQueue;
    private int chunksGenerated = 0;
    private bool currentlyMultithreading = false;
    private Mesh genericChunkMesh_Vertex;
    private Mesh genericChunkMesh_Geometry;
    private Mesh genericChunkMesh_VertexNoAppend;
    private Mesh genericChunkMesh_GeometryNoAppend;
    private bool wasTerrainGenerated = false;
    private int waitFramesForRendering = -1;
    private bool areChunkBordersDrawn = false;

    public event Action TerrainGenerationCompleted;

    private const int totalFramesToWaitForRendering = 5;

    private void Awake()
    {
        chunkMaterials = new Dictionary<TerrainData.GenerationMethod, Material>()
        {
            { TerrainData.GenerationMethod.CPU, chunkMaterialCPU},
            { TerrainData.GenerationMethod.CPU_Multithreading, chunkMaterialCPU},
            { TerrainData.GenerationMethod.CPU_GPU, chunkMaterialCPU},
            { TerrainData.GenerationMethod.GPU_Vertex, chunkMaterialGPU_Vertex},
            { TerrainData.GenerationMethod.GPU_Geometry, chunkMaterialGPU_Geometry},
        };
        chunkMaterialsSimple = new Dictionary<TerrainData.GenerationMethod, Material>()
        {
            { TerrainData.GenerationMethod.CPU, chunkMaterialCPUSimple},
            { TerrainData.GenerationMethod.CPU_Multithreading, chunkMaterialCPUSimple},
            { TerrainData.GenerationMethod.CPU_GPU, chunkMaterialCPUSimple},
            { TerrainData.GenerationMethod.GPU_Vertex, chunkMaterialGPU_SimpleVertex},
            { TerrainData.GenerationMethod.GPU_Geometry, chunkMaterialGPU_SimpleGeometry},
        };
        chunkMaterialsIndirectSimple = new Dictionary<TerrainData.GenerationMethod, Material>()
        {
            { TerrainData.GenerationMethod.CPU, chunkMaterialCPUIndirectSimple},
            { TerrainData.GenerationMethod.CPU_Multithreading, chunkMaterialCPUIndirectSimple},
            { TerrainData.GenerationMethod.CPU_GPU, chunkMaterialCPUIndirectSimple},
            { TerrainData.GenerationMethod.GPU_Vertex, chunkMaterialGPU_IndirectSimpleVertex},
            { TerrainData.GenerationMethod.GPU_Geometry, chunkMaterialGPU_IndirectSimpleGeometry},
        };
        chunkMaterialsIndirect = new Dictionary<TerrainData.GenerationMethod, Material>()
        {
            { TerrainData.GenerationMethod.CPU, chunkMaterialCPUIndirect},
            { TerrainData.GenerationMethod.CPU_Multithreading, chunkMaterialCPUIndirect},
            { TerrainData.GenerationMethod.CPU_GPU, chunkMaterialCPUIndirect},
            { TerrainData.GenerationMethod.GPU_Vertex, chunkMaterialGPU_IndirectVertex},
            { TerrainData.GenerationMethod.GPU_Geometry, chunkMaterialGPU_IndirectGeometry},
        };
    }

    void Start()
    {
        GenerateTerrain();
    }

    private void OnDestroy()
    {
        DestroyTerrain();
    }

    public void GenerateTerrain()
    {
        if (wasTerrainGenerated)
        {
            DestroyTerrain();
        }
        wasTerrainGenerated = true;

        TerrainData.ComputeParameters();
        GenerateGenericMeshes();

        if (areChunkBordersDrawn)
        {
            DestroyChunkBorders();
        }
        if (TerrainData.showChunkBorders)
        {
            DrawChunkBorders();
        }
        if (TerrainData.multithreading)
        {
            currentlyMultithreading = true;
            chunkQueue = new ConcurrentQueue<Chunk>();
            chunksGenerated = 0;
        }

        TerrainData.chunks = new Chunk[TerrainData.chunksX, TerrainData.chunksY, TerrainData.chunksZ];

        Vector3 terrainStartPoint = new(-TerrainData.terrainSizeX * 0.5f, -TerrainData.terrainSizeY * 0.5f, -TerrainData.terrainSizeZ * 0.5f);

        Material materialToUse;
        if (TerrainData.useIndirectRendering && TerrainData.useSimpleShaders)
        {
            materialToUse = chunkMaterialsIndirectSimple[TerrainData.generationMethod];
        }
        else if (TerrainData.useIndirectRendering)
        {
            materialToUse = chunkMaterialsIndirect[TerrainData.generationMethod];
        }
        else if (TerrainData.useSimpleShaders)
        {
            materialToUse = chunkMaterialsSimple[TerrainData.generationMethod];
        }
        else
        {
            materialToUse = chunkMaterials[TerrainData.generationMethod];
        }

        for (int x = 0; x < TerrainData.chunksX; x++)
        {
            for (int y = 0; y < TerrainData.chunksY; y++)
            {
                for (int z = 0; z < TerrainData.chunksZ; z++)
                {
                    Vector3 chunkPosition = terrainStartPoint + new Vector3((x + 0.5f) * TerrainData.chunksSize, (y + 0.5f) * TerrainData.chunksSize, (z + 0.5f) * TerrainData.chunksSize);
                    Chunk chunk = new Chunk(x, y, z, chunkPosition, new Material(materialToUse), this);
                    TerrainData.chunks[x, y, z] = chunk;
                }
            }
        }

        Timer.Init();

        for (int x = 0; x < TerrainData.chunksX; x++)
        {
            for (int y = 0; y < TerrainData.chunksY; y++)
            {
                for (int z = 0; z < TerrainData.chunksZ; z++)
                {
                    Chunk chunk = TerrainData.chunks[x, y, z];

                    if (TerrainData.multithreading)
                    {
                        ThreadPool.QueueUserWorkItem(delegate { chunk.GenerateTerrain(); });
                    }
                    else
                    {
                        chunk.GenerateTerrain();
                    }
                }
            }
        }

        if (!TerrainData.multithreading)
        {
            OnTerrainBuildingCompleted();
        }
    }

    private void DestroyTerrain()
    {
        for (int x = 0; x < TerrainData.chunksX; x++)
        {
            for (int y = 0; y < TerrainData.chunksY; y++)
            {
                for (int z = 0; z < TerrainData.chunksZ; z++)
                {
                    TerrainData.chunks[x, y, z].OnDestroy();
                    Destroy(TerrainData.chunks[x, y, z].Mesh);
                    Destroy(TerrainData.chunks[x, y, z].Material);
                    TerrainData.chunks[x, y, z] = null;
                }
            }
        }
    }

    public void AddChunkToQueue(Chunk chunk)
    {
        //ChunkData chunkData = chunk.GetChunkData();
        //Chunk chunkCopy = new Chunk(chunkData, chunk.Material, this);
        //TerrainData.chunks[chunkData.chunkX, chunkData.chunkY, chunkData.chunkZ] = chunk;
        chunkQueue.Enqueue(chunk);
    }

    private void Update()
    {
        for (int x = 0; x < TerrainData.chunksX; x++)
        {
            for (int y = 0; y < TerrainData.chunksY; y++)
            {
                for (int z = 0; z < TerrainData.chunksZ; z++)
                {
                    TerrainData.chunks[x, y, z]?.Update();
                }
            }
        }

        if (currentlyMultithreading)
        {
            while (!chunkQueue.IsEmpty)
            {
                bool success = chunkQueue.TryDequeue(out Chunk chunk);
                if (success)
                {
                    chunk.GenerateMeshCPU();
                    chunksGenerated++;
                }
            }
            if (chunksGenerated == TerrainData.totalChunks)
            {
                currentlyMultithreading = false;
                OnTerrainBuildingCompleted();
            }
        }
        if (waitFramesForRendering > 0)
        {
            waitFramesForRendering -= 1;
        }
        else if (waitFramesForRendering == 0)
        {
            waitFramesForRendering = -1;
            OnTerrainRenderingCompleted();
        }
    }

    private void OnTerrainBuildingCompleted()
    {
        Timer.StartRenderingMeasurement();
        waitFramesForRendering = totalFramesToWaitForRendering;
    }

    private void OnTerrainRenderingCompleted()
    {
        Timer.EndRenderingMeasurement();
        TerrainGenerationCompleted?.Invoke();
    }

    private void GenerateGenericMeshes()
    {
        if (genericChunkMesh_Vertex != null) Destroy(genericChunkMesh_Vertex);
        if (genericChunkMesh_Geometry != null) Destroy(genericChunkMesh_Geometry);
        if (genericChunkMesh_VertexNoAppend != null) Destroy(genericChunkMesh_VertexNoAppend);
        if (genericChunkMesh_GeometryNoAppend != null) Destroy(genericChunkMesh_GeometryNoAppend);

        int triangleCount = TerrainData.chunkCubesSideReal * TerrainData.chunkCubesSideReal * TerrainData.chunkCubesSideReal * 5;
        if (TerrainData.chunkTriangleLimit < 100)
        {
            triangleCount = Mathf.RoundToInt(triangleCount * (TerrainData.chunkTriangleLimit / 100f));
        }
        int triangleCountNoAppend = TerrainData.chunkCubesSide * TerrainData.chunkCubesSide * TerrainData.chunkCubesSide * 5;

        genericChunkMesh_Vertex = GenerateGenericMesh(triangleCount, false);
        genericChunkMesh_Geometry = GenerateGenericMesh(triangleCount, true);
        genericChunkMesh_VertexNoAppend = GenerateGenericMesh(triangleCountNoAppend, false);
        genericChunkMesh_GeometryNoAppend = GenerateGenericMesh(triangleCountNoAppend, true);
    }

    private Mesh GenerateGenericMesh(int triangleCount, bool useIndices)
    {
        Mesh genericMesh = new Mesh();
        genericMesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;

        int vertexCount = useIndices ? triangleCount : triangleCount  * 3;
        Vector3[] vertices = new Vector3[vertexCount];
        int[] triangles = new int[vertexCount];
        for (int i = 0; i < vertexCount; i++)
        {
            triangles[i] = i;
        }

        genericMesh.vertices = vertices;
        if (useIndices)
        {
            genericMesh.SetIndices(triangles, MeshTopology.Points, 0);
        }
        else
        {
            genericMesh.triangles = triangles;
        }

        Bounds bounds = new Bounds(Vector3.zero, new Vector3(TerrainData.chunksSize, TerrainData.chunksSize, TerrainData.chunksSize));
        genericMesh.bounds = bounds;

        return genericMesh;
    }

    public Mesh GetGenericChunkMesh_Vertex()
    {
        return TerrainData.useAppendBuffers ? genericChunkMesh_Vertex : genericChunkMesh_VertexNoAppend;
    }

    public Mesh GetGenericChunkMesh_Geometry()
    {
        return TerrainData.useAppendBuffers ? genericChunkMesh_Geometry : genericChunkMesh_GeometryNoAppend;
    }

    public void DestroyChunkMesh(Chunk chunk)
    {
        Destroy(chunk.Mesh);
    }

    private void DestroyChunkBorders()
    {
        LineRenderer[] previousLines = GetComponentsInChildren<LineRenderer>();
        foreach (LineRenderer previousLine in previousLines)
        {
            Destroy(previousLine);
        }
        areChunkBordersDrawn = false;
    }

    private void DrawChunkBorders()
    {
        if (areChunkBordersDrawn)
        {
            DestroyChunkBorders();
        }
        
        Vector3 terrainStartPoint = new(-TerrainData.terrainSizeX * 0.5f, -TerrainData.terrainSizeY * 0.5f, -TerrainData.terrainSizeZ * 0.5f);
        for (int x = 0; x < TerrainData.chunksX; x++)
        {
            for (int y = 0; y < TerrainData.chunksY; y++)
            {
                for (int z = 0; z < TerrainData.chunksZ; z++)
                {
                    Vector3 chunkPosition = terrainStartPoint + new Vector3((x + 0.5f) * TerrainData.chunksSize, (y + 0.5f) * TerrainData.chunksSize, (z + 0.5f) * TerrainData.chunksSize);
                    Vector3[] chunkCornerPositions = new Vector3[]
                    {
                        new Vector3(chunkPosition.x - 0.5f * TerrainData.chunksSize,chunkPosition.y - 0.5f * TerrainData.chunksSize,chunkPosition.z + 0.5f * TerrainData.chunksSize),
                        new Vector3(chunkPosition.x + 0.5f * TerrainData.chunksSize,chunkPosition.y - 0.5f * TerrainData.chunksSize,chunkPosition.z + 0.5f * TerrainData.chunksSize),
                        new Vector3(chunkPosition.x + 0.5f * TerrainData.chunksSize,chunkPosition.y - 0.5f * TerrainData.chunksSize,chunkPosition.z - 0.5f * TerrainData.chunksSize),
                        new Vector3(chunkPosition.x - 0.5f * TerrainData.chunksSize,chunkPosition.y - 0.5f * TerrainData.chunksSize,chunkPosition.z - 0.5f * TerrainData.chunksSize),
                        new Vector3(chunkPosition.x - 0.5f * TerrainData.chunksSize,chunkPosition.y + 0.5f * TerrainData.chunksSize,chunkPosition.z + 0.5f * TerrainData.chunksSize),
                        new Vector3(chunkPosition.x + 0.5f * TerrainData.chunksSize,chunkPosition.y + 0.5f * TerrainData.chunksSize,chunkPosition.z + 0.5f * TerrainData.chunksSize),
                        new Vector3(chunkPosition.x + 0.5f * TerrainData.chunksSize,chunkPosition.y + 0.5f * TerrainData.chunksSize,chunkPosition.z - 0.5f * TerrainData.chunksSize),
                        new Vector3(chunkPosition.x - 0.5f * TerrainData.chunksSize,chunkPosition.y + 0.5f * TerrainData.chunksSize,chunkPosition.z - 0.5f * TerrainData.chunksSize),
                    };
                    for (int i = 0; i < 4; i++)
                    {
                        LineRenderer lineRenderer = Instantiate(chunkBorderLinePrefab, (chunkCornerPositions[i] + chunkCornerPositions[(i + 1) % 4])/(Mathf.Pow(2,16)), Quaternion.identity, gameObject.transform).GetComponent<LineRenderer>();
                        lineRenderer.SetPositions(new Vector3[]
                        {
                            chunkCornerPositions[i],
                            chunkCornerPositions[(i + 1)%4]
                        });
                    }
                    for (int i = 0; i < 4; i++)
                    {
                        LineRenderer lineRenderer = Instantiate(chunkBorderLinePrefab, (chunkCornerPositions[i] + chunkCornerPositions[i + 4]) / (Mathf.Pow(2, 16)), Quaternion.identity, gameObject.transform).GetComponent<LineRenderer>();
                        lineRenderer.SetPositions(new Vector3[]
                        {
                            chunkCornerPositions[i],
                            chunkCornerPositions[i + 4]
                        });
                    }
                    for (int i = 0; i < 4; i++)
                    {
                        LineRenderer lineRenderer = Instantiate(chunkBorderLinePrefab, (chunkCornerPositions[i + 4] + chunkCornerPositions[(i + 1) % 4 + 4]) / (Mathf.Pow(2, 16)), Quaternion.identity, gameObject.transform).GetComponent<LineRenderer>();
                        lineRenderer.SetPositions(new Vector3[]
                        {
                            chunkCornerPositions[i + 4],
                            chunkCornerPositions[(i + 1) % 4 + 4]
                        });
                    }
                }
            }
        }

        areChunkBordersDrawn = true;
    }
}
