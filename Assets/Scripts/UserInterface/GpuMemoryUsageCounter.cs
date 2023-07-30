using TMPro;
using UnityEngine;

public class GpuMemoryUsageCounter : MonoBehaviour
{
    [SerializeField] long lowUsageBytesTreshold = 5 * Mathf.RoundToInt(Mathf.Pow(1024, 3));
    [SerializeField] long mediumUsageBytesTreshold = 10 * Mathf.RoundToInt(Mathf.Pow(1024, 3));
    [SerializeField] Color highUsageColor = Color.red;
    [SerializeField] Color mediumUsageColor = Color.yellow;
    [SerializeField] Color lowUsageColor = Color.green;
    [SerializeField] TextMeshProUGUI gpuMemoryUsageText;

    private TerrainGenerator terrainGenerator;

    private void Awake()
    {
        terrainGenerator = FindObjectOfType<TerrainGenerator>();
    }

    private void OnEnable()
    {
        terrainGenerator.TerrainGenerationCompleted += OnTerrainGenerationComplete;
    }

    private void OnDisable()
    {
        terrainGenerator.TerrainGenerationCompleted -= OnTerrainGenerationComplete;
    }

    private void OnTerrainGenerationComplete()
    {
        if (TerrainData.IsKeepingGpuMemory())
        {
            gpuMemoryUsageText.gameObject.SetActive(true);
            SetGpuMemoryUsageText();
        }
        else
        {
            gpuMemoryUsageText.gameObject.SetActive(false);
        }
    }

    private void SetGpuMemoryUsageText()
    {
        long gpuMemoryUsed = TerrainData.gpuMemoryUsedBytes;
        string gpuMemoryUsedText;

        if (gpuMemoryUsed < Mathf.RoundToInt(Mathf.Pow(1024, 1)))
        {
            gpuMemoryUsedText = gpuMemoryUsed + " B";
        }
        else if (gpuMemoryUsed < Mathf.RoundToInt(Mathf.Pow(1024, 2)))
        {
            gpuMemoryUsedText = (gpuMemoryUsed / Mathf.Pow(1024, 1)).ToString("F2") + " KB";
        }
        else if (gpuMemoryUsed < Mathf.RoundToInt(Mathf.Pow(1024, 3)))
        {
            gpuMemoryUsedText = (gpuMemoryUsed / Mathf.Pow(1024, 2)).ToString("F2") + " MB";
        }
        else
        {
            gpuMemoryUsedText = (gpuMemoryUsed / Mathf.Pow(1024, 3)).ToString("F2") + " GB";
        }
        gpuMemoryUsedText += " GPU memory used";
        gpuMemoryUsageText.text = gpuMemoryUsedText;

        if (gpuMemoryUsed < lowUsageBytesTreshold)
        {
            gpuMemoryUsageText.color = lowUsageColor;
        }
        else if (gpuMemoryUsed < mediumUsageBytesTreshold)
        {
            gpuMemoryUsageText.color = mediumUsageColor;
        }
        else
        {
            gpuMemoryUsageText.color = highUsageColor;
        }
    }
}
