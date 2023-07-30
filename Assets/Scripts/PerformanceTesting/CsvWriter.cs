using System.IO;
using UnityEngine;
using UnityEngine.Assertions;

public class CsvWriter
{
    private StreamWriter writer;

    private readonly string dataPath;
    private const string outputFileDirectory = "PerformanceResults";

    public CsvWriter()
    {
        if (Application.isEditor)
        {
            dataPath = Application.dataPath;
        }
        else
        {
            dataPath = Application.streamingAssetsPath;
        }
    }

    public string StartWriting(string fileName)
    {
        string directoryPath = Path.Combine(dataPath, outputFileDirectory);
        Directory.CreateDirectory(directoryPath);

        string filePath = Path.Combine(directoryPath, fileName + ".csv");
        Assert.IsFalse(File.Exists(filePath));
        writer = new StreamWriter(filePath, true);

        return filePath;
    }

    public void WriteEntry(string entry)
    {
        writer.WriteLine(entry);
    }

    public void WriteEntries(string[] entries)
    {
        string stringToWrite = "";
        for (int i = 0; i < entries.Length; i++)
        {
            stringToWrite += entries[i];
            if (i < entries.Length - 1)
            {
                stringToWrite += ";";
            }
        }
        writer.WriteLine(stringToWrite);
    }

    public void WriteStaticTerrainParameters()
    {
        writer.WriteLine(
            "Seed;" + TerrainData.seed.ToString() + "\n" +
            "SmoothNormals;" + TerrainData.smoothNormals.ToString() + "\n" +
            "InterpolateVertices;" + TerrainData.interpolateVertices.ToString() + "\n" +
            "UseAppendBuffers;" + TerrainData.useAppendBuffers.ToString() + "\n" +
            "TrimMeshesAndBuffers;" + TerrainData.trimMeshesAndBuffers.ToString() + "\n" +
            "UseIndirectRendering;" + TerrainData.useIndirectRendering.ToString() + "\n" +
            "UseSimpleShaders;" + TerrainData.useSimpleShaders.ToString() + "\n" +
            "ChunkSize;" + TerrainData.chunkSize.ToString("F2") + "\n" +
            "ChunkTriangleLimit;" + TerrainData.chunkTriangleLimit.ToString() + "\n" +
            "TerrainSizeX;" + TerrainData.terrainSizeX.ToString("F2") + "\n" +
            "TerrainSizeY;" + TerrainData.terrainSizeY.ToString("F2") + "\n" +
            "TerrainSizeZ;" + TerrainData.terrainSizeZ.ToString("F2") + "\n" +
            "NoiseFrequency;" + TerrainData.noiseFrequency.ToString("F2") + "\n" +
            "NoiseStrength;" + TerrainData.noiseStrength.ToString("F2") + "\n" +
            "NoiseLayers;" + TerrainData.noiseLayers.ToString() + "\n" +
            "NoiseLayerFrequencyMultiplier;" + TerrainData.noiseLayerFrequencyMultiplier.ToString("F2")
        );
    }

    public void WriteTestParameters(PerformanceTester performanceTester)
    {
        writer.WriteLine(
            "cubesPerUnitLimit;" + performanceTester.cubesPerUnitLimit.ToString() + "\n" +
            "numberOfShortMeasurments;" + performanceTester.numberOfShortMeasurments.ToString() + "\n" +
            "numberOfLongMeasurments;" + performanceTester.numberOfLongMeasurments.ToString() + "\n" +
            "shortMeasurmentTresholdSeconds;" + performanceTester.shortMeasurmentTresholdSeconds.ToString("F2") + "\n" +
            "measurmentLengthLimitSeconds;" + performanceTester.measurmentLengthLimitSeconds.ToString("F2") + "\n" +
            "fpsMeasurmentDurationSeconds;" + performanceTester.fpsMeasurmentDurationSeconds.ToString("F2") + "\n" +
            "skipCpu;" + performanceTester.skipCpu.ToString()
        );
    }

    public void AddEmptyLine()
    {
        writer.WriteLine("");
    }

    public void StopWriting()
    {
        writer.Close();
    }
}
