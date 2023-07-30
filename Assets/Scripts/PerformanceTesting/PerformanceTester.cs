using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PerformanceTester : MonoBehaviour
{
    private CsvWriter csvWriter;

    // parameters
    [NonSerialized] public int cubesPerUnitLimit = 10;
    [NonSerialized] public int numberOfShortMeasurments = 10;
    [NonSerialized] public int numberOfLongMeasurments = 3;
    [NonSerialized] public float shortMeasurmentTresholdSeconds = 10f;
    [NonSerialized] public float measurmentLengthLimitSeconds = 180f;
    [NonSerialized] public float fpsMeasurmentDurationSeconds = 5f;
    [NonSerialized] public bool skipCpu = false;

    // paramter limits and slider step
    [NonSerialized] public int cubesPerUnitLimit_MIN = 5;
    [NonSerialized] public int cubesPerUnitLimit_MAX = 30;
    [NonSerialized] public int numberOfShortMeasurments_MIN = 1;
    [NonSerialized] public int numberOfShortMeasurments_MAX = 10;
    [NonSerialized] public int numberOfLongMeasurments_MIN = 1;
    [NonSerialized] public int numberOfLongMeasurments_MAX = 10;
    [NonSerialized] public float shortMeasurmentTresholdSeconds_STEP = 0.5f;
    [NonSerialized] public float shortMeasurmentTresholdSeconds_MIN = 3f;
    [NonSerialized] public float shortMeasurmentTresholdSeconds_MAX = 20f;
    [NonSerialized] public float measurmentLengthLimitSeconds_STEP = 1f;
    [NonSerialized] public float measurmentLengthLimitSeconds_MIN = 10f;
    [NonSerialized] public float measurmentLengthLimitSeconds_MAX = 300;
    [NonSerialized] public float fpsMeasurmentDurationSeconds_STEP = 1f;
    [NonSerialized] public float fpsMeasurmentDurationSeconds_MIN = 2f;
    [NonSerialized] public float fpsMeasurmentDurationSeconds_MAX = 10f;

    public event Action<string> PerformanceTestFinished;
    public event Action<string> PerformanceTestProgressUpdate;

    private bool performanceTestIsRunning = false;
    private bool fpsMeasurmentIsRunning = false;
    private int frameCount;
    private TerrainDataUI terrainDataUI;
    private TerrainGenerator terrainGenerator;
    private Dictionary<TerrainData.GenerationMethod, Measurment[]> measurments;
    private Measurment currentMeasurment;

    private const string fileNamePrefix = "PerformanceTest_";

    private class Measurment
    {
        public int cubesPerUnit;
        public long totalCubes;
        public bool usesGpuMemory;
        public long gpuMemoryUsed;
        public long actualTriangleCount;
        public TerrainData.GenerationMethod generationMethod;
        public Dictionary<Timer.MeasurementType, List<float>> measurments;
        public bool measuredFrameRate;
        public float averageFrameRate;
        public int iterationIndex;
        public bool discardFirstMeasurment;

        public Measurment(int cubesPerUnit, TerrainData.GenerationMethod generationMethod)
        {
            this.cubesPerUnit = cubesPerUnit;
            this.generationMethod = generationMethod;
            measurments = new Dictionary<Timer.MeasurementType, List<float>>();
            foreach (Timer.MeasurementType measurementType in Enum.GetValues(typeof(Timer.MeasurementType)))
            {
                measurments.Add(measurementType, new List<float>());
            }
            iterationIndex = 0;
            discardFirstMeasurment = cubesPerUnit == 1;
        }
    }

    private void Awake()
    {
        csvWriter = new CsvWriter();
        terrainDataUI = FindObjectOfType<TerrainDataUI>();
        terrainGenerator = FindObjectOfType<TerrainGenerator>();
    }

    private void Update()
    {
        if (fpsMeasurmentIsRunning)
        {
            frameCount += 1;
        }
    }

    private void OnEnable()
    {
        terrainGenerator.TerrainGenerationCompleted += CollectCurrentMeasurmentsResults;
    }

    private void OnDisable()
    {
        terrainGenerator.TerrainGenerationCompleted -= CollectCurrentMeasurmentsResults;
    }

    public void RunPerformanceTest()
    {
        performanceTestIsRunning = true;
        measurments = new Dictionary<TerrainData.GenerationMethod, Measurment[]>();

        foreach (TerrainData.GenerationMethod generationMethod in Enum.GetValues(typeof(TerrainData.GenerationMethod)))
        {
            if (skipCpu && (generationMethod == TerrainData.GenerationMethod.CPU || generationMethod == TerrainData.GenerationMethod.CPU_Multithreading)) continue;

            Measurment[] measurmentSet = new Measurment[cubesPerUnitLimit];
            for (int i = 0; i < cubesPerUnitLimit; i++)
            {
                measurmentSet[i] = new Measurment(i + 1, generationMethod);
            }
            measurments.Add(generationMethod, measurmentSet);
        }

        currentMeasurment = measurments[skipCpu ? TerrainData.GenerationMethod.CPU_GPU : 0][0];
        StartCurrentMeasurment();
    }

    private void StartCurrentMeasurment()
    {
        terrainDataUI.BlockUpdateOnValueChange = true;
        PerformanceTestProgressUpdate?.Invoke("Now testing:\n" + currentMeasurment.generationMethod.ToString() + " generation time, iteration " + (currentMeasurment.discardFirstMeasurment ? 0 : currentMeasurment.iterationIndex + 1) + "...");
        TerrainData.generationMethod = currentMeasurment.generationMethod;
        TerrainData.cubesPerUnit = currentMeasurment.cubesPerUnit;
        terrainDataUI.RefreshAllInputs();
        terrainGenerator.GenerateTerrain();
    }

    private IEnumerator MeasureFps()
    {
        fpsMeasurmentIsRunning = true;
        frameCount = 0;
        PerformanceTestProgressUpdate?.Invoke("Now testing:\n" + currentMeasurment.generationMethod.ToString() + " average frame rate...");

        yield return new WaitForSeconds(fpsMeasurmentDurationSeconds);

        fpsMeasurmentIsRunning = false;
        currentMeasurment.averageFrameRate = frameCount / fpsMeasurmentDurationSeconds;
        currentMeasurment.measuredFrameRate = true;

        SetNewCurrentMeasurment(0);
    }

    private void CollectCurrentMeasurmentsResults()
    {
        if (performanceTestIsRunning)
        {
            currentMeasurment.usesGpuMemory = TerrainData.IsKeepingGpuMemory();
            currentMeasurment.gpuMemoryUsed = TerrainData.gpuMemoryUsedBytes;
            currentMeasurment.actualTriangleCount = TerrainData.actualTriangleCount;
            currentMeasurment.totalCubes = TerrainData.totalCubesX * TerrainData.totalCubesY * TerrainData.totalCubesZ;

            Dictionary<Timer.MeasurementType, float> outputMeasurments = Timer.GetMeasurements();
            if (!currentMeasurment.discardFirstMeasurment)
            {
                Dictionary<Timer.MeasurementType, List<float>> measurmentsToPopulate = measurments[currentMeasurment.generationMethod][currentMeasurment.cubesPerUnit - 1].measurments;
                foreach (Timer.MeasurementType measurementType in Enum.GetValues(typeof(Timer.MeasurementType)))
                {
                    measurmentsToPopulate[measurementType].Add(outputMeasurments[measurementType]);
                }
            }
            SetNewCurrentMeasurment(outputMeasurments[Timer.MeasurementType.Total_Time]);
        }
    }

    private void SetNewCurrentMeasurment(float previousMeasurementTotalTime)
    {
        if (currentMeasurment.discardFirstMeasurment)
        {
            currentMeasurment.discardFirstMeasurment = false;
            StartCurrentMeasurment();
        }
        else if (currentMeasurment.iterationIndex < numberOfLongMeasurments - 1 || ((previousMeasurementTotalTime/1000) < shortMeasurmentTresholdSeconds && currentMeasurment.iterationIndex < numberOfShortMeasurments - 1))
        {
            currentMeasurment.iterationIndex += 1;
            StartCurrentMeasurment();
        }
        else if (!currentMeasurment.measuredFrameRate)
        {
            StartCoroutine(MeasureFps());
        }
        else if (currentMeasurment.cubesPerUnit < cubesPerUnitLimit && ((previousMeasurementTotalTime / 1000) < measurmentLengthLimitSeconds))
        {
            currentMeasurment = measurments[currentMeasurment.generationMethod][currentMeasurment.cubesPerUnit];
            StartCurrentMeasurment();
        }
        else if ((int)currentMeasurment.generationMethod < Enum.GetNames(typeof(TerrainData.GenerationMethod)).Length - 1)
        {
            currentMeasurment = measurments[currentMeasurment.generationMethod + 1][0];
            StartCurrentMeasurment();
        }
        else
        {
            FinishPerformanceTest();
        }
    }

    private void FinishPerformanceTest()
    {
        string filePath = SavePerformanceTestResults();
        performanceTestIsRunning = false;
        PerformanceTestFinished?.Invoke("Results saved in:\n" + filePath);
        terrainDataUI.BlockUpdateOnValueChange = false;
    }

    private string SavePerformanceTestResults()
    {
        string filePath = csvWriter.StartWriting(GetFileName());
        csvWriter.WriteStaticTerrainParameters();
        csvWriter.WriteTestParameters(this);
        csvWriter.AddEmptyLine();

        List<string> minorLabels = new List<string>()
            {
                "Cubes per unit",
                "Total cubes",
                "Actual triangles"
            };
        foreach (Timer.MeasurementType measurementType in Enum.GetValues(typeof(Timer.MeasurementType)))
        {
            minorLabels.Add(measurementType.ToString() + " time (ms)");
            minorLabels.Add(measurementType.ToString() + " min time (ms)");
            minorLabels.Add(measurementType.ToString() + " max time (ms)");
        }
        minorLabels.Add("GPU memory used (b)");
        minorLabels.Add("Average frame rate");
        string[] minorLabelsArray = minorLabels.ToArray();

        foreach (TerrainData.GenerationMethod generationMethod in Enum.GetValues(typeof(TerrainData.GenerationMethod)))
        {
            if (skipCpu && (generationMethod == TerrainData.GenerationMethod.CPU || generationMethod == TerrainData.GenerationMethod.CPU_Multithreading)) continue;

            csvWriter.WriteEntry(generationMethod.ToString());
            csvWriter.WriteEntries(minorLabelsArray);
            for (int i = 0; i < cubesPerUnitLimit; i++)
            {
                Measurment measurment = measurments[generationMethod][i];
                List<string> outputValues = new List<string>()
                {
                    measurment.cubesPerUnit.ToString(),
                    measurment.totalCubes.ToString(),
                    measurment.actualTriangleCount == 0 ? "?" : measurment.actualTriangleCount.ToString()
                };
                foreach (Timer.MeasurementType measurementType in Enum.GetValues(typeof(Timer.MeasurementType)))
                {
                    outputValues.Add(CalculateAverage(measurment.measurments[measurementType]).ToString("F2"));
                    outputValues.Add(Mathf.Min(measurment.measurments[measurementType].ToArray()).ToString("F2"));
                    outputValues.Add(Mathf.Max(measurment.measurments[measurementType].ToArray()).ToString("F2"));
                }
                outputValues.Add(measurment.usesGpuMemory ? measurment.gpuMemoryUsed.ToString() : "0");
                outputValues.Add(measurment.averageFrameRate.ToString("F2"));
                csvWriter.WriteEntries(outputValues.ToArray());
            }
            csvWriter.AddEmptyLine();
        }

        csvWriter.StopWriting();
        return filePath;
    }

    private string GetFileName()
    {
        DateTime date = DateTime.Now;
        return fileNamePrefix + date.ToString("dd-MM-yy_H;mm;ss");
    }

    private float CalculateAverage(List<float> measurements)
    {
        float sum = 0;
        foreach (var measurement in measurements)
        {
            sum += measurement;
        }
        return sum/measurements.Count;
    }
}
