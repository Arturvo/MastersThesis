using UnityEngine;
using TMPro;
using System.Collections.Generic;

public class TimerUI : MonoBehaviour
{
    [SerializeField] private GameObject content;
    [SerializeField] private GameObject background;
    [SerializeField] private TextMeshProUGUI terrainGenerationTimesText;

    private TerrainGenerator terrainGenerator;

    private void Awake()
    {
        terrainGenerator = FindObjectOfType<TerrainGenerator>();
        content.SetActive(false);
        background.SetActive(false);
    }

    private void OnEnable()
    {
        terrainGenerator.TerrainGenerationCompleted += OnTerrainGenerationCompleted;
    }

    private void OnDisable()
    {
        terrainGenerator.TerrainGenerationCompleted -= OnTerrainGenerationCompleted;
    }

    private void OnTerrainGenerationCompleted()
    {
        Timer.EndRenderingMeasurement();

        if (TerrainData.showGenerationTime)
        {
            content.SetActive(true);
            background.SetActive(true);
            Dictionary<Timer.MeasurementType, float> measurements = Timer.GetMeasurements();
            terrainGenerationTimesText.text = 
                "Voxels were generated in " + GetMeasurement(measurements[Timer.MeasurementType.Voxel_Generation]) + "\n" +
                "Marching Cubes completed in " + GetMeasurement(measurements[Timer.MeasurementType.Marching_Cubes]) + "\n" +
                (TerrainData.smoothNormals ? "Smoothing normals took " + GetMeasurement(measurements[Timer.MeasurementType.Mesh_Smoothing]) + "\n" : "") +
                (TerrainData.trimMeshesAndBuffers ? "Mesh was trimmed in " + GetMeasurement(measurements[Timer.MeasurementType.MeshAndBuffer_Trimming]) + "\n" : "") +
                "Mesh was constructed in " + GetMeasurement(measurements[Timer.MeasurementType.Mesh_Building]) + "\n" +
                "Mesh was rendered in " + GetMeasurement(measurements[Timer.MeasurementType.Mesh_Rendering]) + "\n" +
                "Total time spent was " + GetMeasurement(measurements[Timer.MeasurementType.Total_Time]);
        }
        else
        {
            content.SetActive(false);
            background.SetActive(false);
        }
    }

    private string GetMeasurement(float totalMilliseconds)
    {
        if (totalMilliseconds < 1000 || TerrainData.useOnlyMiliseconds)
        {
            return Mathf.RoundToInt(totalMilliseconds) + " miliseconds";
        }
        else if (totalMilliseconds < 60000)
        {
            return (totalMilliseconds / 1000).ToString("F3") + " seconds";
        }
        else
        {
            return (totalMilliseconds / 60000).ToString("F3") + " minutes";
        }
    }
}
