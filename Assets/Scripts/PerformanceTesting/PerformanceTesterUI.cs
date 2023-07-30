using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class PerformanceTesterUI : MonoBehaviour
{
    [SerializeField] private Button runPerformanceTestButton;
    [SerializeField] private ContinuousIntInputField cubesPerUnitLimitInputField;
    [SerializeField] private ContinuousIntInputField numberOfShortMeasurmentsInputField;
    [SerializeField] private ContinuousIntInputField numberOfLongMeasurmentsInputField;
    [SerializeField] private ContinuousFloatInputField shortMeasurmentTresholdSecondsInputField;
    [SerializeField] private ContinuousFloatInputField measurmentLengthLimitSecondsInputField;
    [SerializeField] private ContinuousFloatInputField fpsMeasurmentDurationSecondsInputField;
    [SerializeField] private Toggle skipCpuToggle;
    [SerializeField] private TextMeshProUGUI progressText;

    private EventSystem eventSystem;
    private PerformanceTester performanceTester;
    private Action<int> cubesPerUnitLimitInputFieldOnValueChanged;
    private Action<int> numberOfShortMeasurmentsInputFieldOnValueChanged;
    private Action<int> numberOfLongMeasurmentsInputFieldOnValueChanged;
    private Action<float> shortMeasurmentTresholdSecondsInputFieldOnValueChanged;
    private Action<float> measurmentLengthLimitSecondsInputFieldOnValueChanged;
    private Action<float> fpsMeasurmentDurationSecondsInputFieldOnValueChanged;

    private void Awake()
    {
       eventSystem = FindObjectOfType<EventSystem>();
       performanceTester = FindObjectOfType<PerformanceTester>();

       cubesPerUnitLimitInputFieldOnValueChanged = (int newValue) => performanceTester.cubesPerUnitLimit = newValue;
       numberOfShortMeasurmentsInputFieldOnValueChanged = (int newValue) => performanceTester.numberOfShortMeasurments = newValue;
       numberOfLongMeasurmentsInputFieldOnValueChanged = (int newValue) => performanceTester.numberOfLongMeasurments = newValue;
       shortMeasurmentTresholdSecondsInputFieldOnValueChanged = (float newValue) => performanceTester.shortMeasurmentTresholdSeconds = newValue;
       measurmentLengthLimitSecondsInputFieldOnValueChanged = (float newValue) => performanceTester.measurmentLengthLimitSeconds = newValue;
       fpsMeasurmentDurationSecondsInputFieldOnValueChanged = (float newValue) => performanceTester.fpsMeasurmentDurationSeconds = newValue;

        progressText.gameObject.SetActive(false);
    }

    private void OnEnable()
    {
        performanceTester.PerformanceTestFinished += OnPerformanceTestFinished;
        performanceTester.PerformanceTestProgressUpdate += OnPerformanceTestProgressUpdate;

        cubesPerUnitLimitInputField.Init(performanceTester.cubesPerUnitLimit_MIN, performanceTester.cubesPerUnitLimit_MAX, performanceTester.cubesPerUnitLimit);
        numberOfShortMeasurmentsInputField.Init(performanceTester.numberOfShortMeasurments_MIN, performanceTester.numberOfShortMeasurments_MAX, performanceTester.numberOfShortMeasurments);
        numberOfLongMeasurmentsInputField.Init(performanceTester.numberOfLongMeasurments_MIN, performanceTester.numberOfLongMeasurments_MAX, performanceTester.numberOfLongMeasurments);
        shortMeasurmentTresholdSecondsInputField.Init(performanceTester.shortMeasurmentTresholdSeconds_MIN, performanceTester.shortMeasurmentTresholdSeconds_MAX, performanceTester.shortMeasurmentTresholdSeconds, performanceTester.shortMeasurmentTresholdSeconds_STEP);
        measurmentLengthLimitSecondsInputField.Init(performanceTester.measurmentLengthLimitSeconds_MIN, performanceTester.measurmentLengthLimitSeconds_MAX, performanceTester.measurmentLengthLimitSeconds, performanceTester.measurmentLengthLimitSeconds_STEP);
        fpsMeasurmentDurationSecondsInputField.Init(performanceTester.fpsMeasurmentDurationSeconds_MIN, performanceTester.fpsMeasurmentDurationSeconds_MAX, performanceTester.fpsMeasurmentDurationSeconds, performanceTester.fpsMeasurmentDurationSeconds_STEP);
        skipCpuToggle.isOn = performanceTester.skipCpu;

        runPerformanceTestButton.onClick.AddListener(OnRunPerformanceTestButtonClick);
        cubesPerUnitLimitInputField.OnValueChanged += cubesPerUnitLimitInputFieldOnValueChanged;
        numberOfShortMeasurmentsInputField.OnValueChanged += numberOfShortMeasurmentsInputFieldOnValueChanged;
        numberOfLongMeasurmentsInputField.OnValueChanged += numberOfLongMeasurmentsInputFieldOnValueChanged;
        shortMeasurmentTresholdSecondsInputField.OnValueChanged += shortMeasurmentTresholdSecondsInputFieldOnValueChanged;
        measurmentLengthLimitSecondsInputField.OnValueChanged += measurmentLengthLimitSecondsInputFieldOnValueChanged;
        fpsMeasurmentDurationSecondsInputField.OnValueChanged += fpsMeasurmentDurationSecondsInputFieldOnValueChanged;
        skipCpuToggle.onValueChanged.AddListener((bool newValue) => performanceTester.skipCpu = newValue);
    }

    private void OnDisable()
    {
        performanceTester.PerformanceTestFinished -= OnPerformanceTestFinished;
        performanceTester.PerformanceTestProgressUpdate -= OnPerformanceTestProgressUpdate;

        runPerformanceTestButton.onClick.RemoveListener(OnRunPerformanceTestButtonClick);
        cubesPerUnitLimitInputField.OnValueChanged -= cubesPerUnitLimitInputFieldOnValueChanged;
        numberOfShortMeasurmentsInputField.OnValueChanged -= numberOfShortMeasurmentsInputFieldOnValueChanged;
        numberOfLongMeasurmentsInputField.OnValueChanged -= numberOfLongMeasurmentsInputFieldOnValueChanged;
        shortMeasurmentTresholdSecondsInputField.OnValueChanged -= shortMeasurmentTresholdSecondsInputFieldOnValueChanged;
        measurmentLengthLimitSecondsInputField.OnValueChanged -= measurmentLengthLimitSecondsInputFieldOnValueChanged;
        fpsMeasurmentDurationSecondsInputField.OnValueChanged -= fpsMeasurmentDurationSecondsInputFieldOnValueChanged;
        skipCpuToggle.onValueChanged.RemoveAllListeners();
    }

    private void OnRunPerformanceTestButtonClick()
    {
        eventSystem.gameObject.SetActive(false);
        progressText.gameObject.SetActive(true);
        performanceTester.RunPerformanceTest();
    }

    private void OnPerformanceTestFinished(string finishMessage)
    {
        eventSystem.gameObject.SetActive(true);
        progressText.text = finishMessage;
    }

    private void OnPerformanceTestProgressUpdate(string progressMessage)
    {
        progressText.text = progressMessage;
    }
}
