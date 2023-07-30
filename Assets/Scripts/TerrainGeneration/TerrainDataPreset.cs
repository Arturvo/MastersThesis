using UnityEngine;

[CreateAssetMenu(fileName = "TerrainDataPreset", menuName = "TerrainDataPreset")]
public class TerrainDataPreset : ScriptableObject
{
    [field: SerializeField] public bool IsDefault { get; private set; }
    [field: SerializeField] public string DisplayName { get; private set; }
    [field: SerializeField] public float ChunkSize { get; private set; }
    [field: SerializeField] public int ChunkTriangleLimit { get; private set; }
    [field: SerializeField] public float TerrainSizeX { get; private set; }
    [field: SerializeField] public float TerrainSizeY { get; private set; }
    [field: SerializeField] public float TerrainSizeZ { get; private set; }
    [field: SerializeField] public int CubesPerUnit { get; private set; }
    [field: SerializeField] public int Seed { get; private set; }
    [field: SerializeField] public float NoiseFrequency { get; private set; }
    [field: SerializeField] public float NoiseStrength { get; private set; }
    [field: SerializeField] public int NoiseLayers { get; private set; }
    [field: SerializeField] public float NoiseLayerFrequencyMultiplier { get; private set; }
}