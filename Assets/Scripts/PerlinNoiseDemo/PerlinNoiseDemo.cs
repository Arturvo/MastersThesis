using System.Collections.Generic;
using UnityEngine;

public class PerlinNoiseDemo : MonoBehaviour
{
    [SerializeField] private Renderer demoTexture;
    [SerializeField] private MeshFilter terrain;

    private const int demoTextureSize = 128;
    private const int demoNoiseLayers = 9;
    private const float demoNoisefrequency = 10f;
    private const float demoNoiselayerFrequencyMultiplier = 1.8f;
    private const float demoTerrainNoiseStrength = 0.2f;

    private void Start()
    {
        Texture2D texture = new Texture2D(demoTextureSize, demoTextureSize);
        float[,] perlinNoise = GetPerlinNoise(demoTextureSize, demoNoiseLayers, demoNoisefrequency, demoNoiselayerFrequencyMultiplier);

        Color[] colors = new Color[demoTextureSize * demoTextureSize];
        for (int x = 0; x < demoTextureSize; x++)
        {
            for (int y = 0; y < demoTextureSize; y++)
            {
                colors[x * demoTextureSize + y] = new Color(perlinNoise[x, y], perlinNoise[x, y], perlinNoise[x, y], 1);
            }
        }
        texture.SetPixels(colors);
        texture.Apply();

        MaterialPropertyBlock block = new MaterialPropertyBlock();
        block.SetTexture("_MainTex", texture);
        demoTexture.SetPropertyBlock(block);

        Vector3[] demoTerrainVertices = new Vector3[demoTextureSize * demoTextureSize];
        List<int> demoTerrainTriangles = new List<int>();
        float vertexDistance = 1f / demoTextureSize;

        for (int x = 0; x < demoTextureSize; x++)
        {
            for (int y = 0; y < demoTextureSize; y++)
            {
                int currentVertexIndex = x * demoTextureSize + y;
                demoTerrainVertices[x * demoTextureSize + y] = new Vector3(x * vertexDistance, perlinNoise[x, y] * demoTerrainNoiseStrength, y * vertexDistance);
                if (x < demoTextureSize - 1 && y < demoTextureSize - 1)
                {
                    demoTerrainTriangles.Add(currentVertexIndex + demoTextureSize + 1);
                    demoTerrainTriangles.Add(currentVertexIndex + demoTextureSize);
                    demoTerrainTriangles.Add(currentVertexIndex);
                    demoTerrainTriangles.Add(currentVertexIndex + 1);
                    demoTerrainTriangles.Add(currentVertexIndex + demoTextureSize + 1);
                    demoTerrainTriangles.Add(currentVertexIndex);
                }
            }
        }

        Mesh demoTerrainMesh = new Mesh();
        demoTerrainMesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
        terrain.mesh = demoTerrainMesh;
        demoTerrainMesh.vertices = demoTerrainVertices;
        demoTerrainMesh.triangles = demoTerrainTriangles.ToArray();
        demoTerrainMesh.RecalculateNormals();
    }

    private float[,] GetPerlinNoise (int size, int layers, float frequency, float layerFrequencyMultiplier)
    {
        float[,] perlinNoise = new float[size, size];

        for (int x = 0; x < size; x++)
        {
            for (int y = 0; y < size; y++)
            {
                float noiseValue = 0;
                float currentAmplitude = 1;
                float currentFrequency = frequency;
                float maxPossibleValue = 0;

                for (int i = 0; i < layers; i++)
                {
                    float xCoord = (float) x / size * currentFrequency;
                    float yCoord = (float) y / size * currentFrequency;

                    noiseValue += Mathf.PerlinNoise(xCoord, yCoord) * currentAmplitude;
                    maxPossibleValue += currentAmplitude;

                    currentAmplitude /= 2;
                    currentFrequency *= layerFrequencyMultiplier;

                }

                perlinNoise[x, y] = noiseValue / maxPossibleValue;
            }
        }

        return perlinNoise;
    }
}
