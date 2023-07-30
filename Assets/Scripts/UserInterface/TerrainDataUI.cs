using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TerrainDataUI : MonoBehaviour
{
    [SerializeField] private TMP_Dropdown terrainUpdateMethodDropdown;
    [SerializeField] private TMP_Dropdown generationMethodDropdown;
    [SerializeField] private TMP_Dropdown terrainDataPresetDropdown;
    [SerializeField] private Toggle interpolateVerticesToggle;
    [SerializeField] private Toggle smoothNormalsToggle;
    [SerializeField] private Toggle showGenerationTimeToggle;
    [SerializeField] private Toggle useAppendBuffersToggle;
    [SerializeField] private Toggle trimMeshesAndBuffersToggle;
    [SerializeField] private Toggle useIndirectRenderingToggle;
    [SerializeField] private Toggle useSimpleShadersToggle;
    [SerializeField] private Toggle showChunkBordersToggle;
    [SerializeField] private ContinuousFloatInputField chunkSizeInputField;
    [SerializeField] private ContinuousIntInputField chunkTriangleLimitInputField;
    [SerializeField] private ContinuousIntInputField seedInputField;
    [SerializeField] private ContinuousFloatInputField terrainSizeXInputField;
    [SerializeField] private ContinuousFloatInputField terrainSizeYInputField;
    [SerializeField] private ContinuousFloatInputField terrainSizeZInputField;
    [SerializeField] private ContinuousIntInputField cubesPerUnitInputField;
    [SerializeField] private ContinuousFloatInputField noiseFrequencyInputField;
    [SerializeField] private ContinuousFloatInputField noiseStrengthInputField;
    [SerializeField] private ContinuousIntInputField noiseLayersInputField;
    [SerializeField] private ContinuousFloatInputField noiseLayerFrequencyMultiplierInputField;
    [SerializeField] private Button updateTerrainButton;

    public bool BlockUpdateOnValueChange { get; set; }

    private TerrainUpdateMethod terrainUpdateMethod = TerrainUpdateMethod.OnValueChanged;
    private TerrainGenerator terrainGenerator;
    private TerrainDataPreset[] terrainDataPresets;
    private Action<float> chunkSizeInputFieldOnValueChange;
    private Action<float> chunkSizeInputFieldOnValueChanged;
    private Action<int> chunkTriangleLimitInputFieldOnValueChange;
    private Action<int> chunkTriangleLimitInputFieldOnValueChanged;
    private Action<int> seedInputFieldOnValueChange;
    private Action<int> seedInputFieldOnValueChanged;
    private Action<float> terrainSizeXInputFieldOnValueChange;
    private Action<float> terrainSizeXInputFieldOnValueChanged;
    private Action<float> terrainSizeYInputFieldOnValueChange;
    private Action<float> terrainSizeYInputFieldOnValueChanged;
    private Action<float> terrainSizeZInputFieldOnValueChange;
    private Action<float> terrainSizeZInputFieldOnValueChanged;
    private Action<int> cubesPerUnitInputFieldOnValueChange;
    private Action<int> cubesPerUnitInputFieldOnValueChanged;
    private Action<float> noiseFrequencyInputFieldOnValueChange;
    private Action<float> noiseFrequencyInputFieldOnValueChanged;
    private Action<float> noiseStrengthInputFieldOnValueChange;
    private Action<float> noiseStrengthInputFieldOnValueChanged;
    private Action<int> noiseLayersInputFieldOnValueChange;
    private Action<int> noiseLayersInputFieldOnValueChanged;
    private Action<float> noiseLayerFrequencyMultiplierInputFieldOnValueChange;
    private Action<float> noiseLayerFrequencyMultiplierInputFieldOnValueChanged;

    private enum TerrainUpdateMethod
    {
        OnValueChange,
        OnValueChanged,
        OnButtonPress
    }

    private void Awake()
    {
        terrainGenerator = FindObjectOfType<TerrainGenerator>();
        PopulateDropdownWithEnum(terrainUpdateMethodDropdown, terrainUpdateMethod);
        PopulateDropdownWithEnum(generationMethodDropdown, TerrainData.generationMethod);

        UnityEngine.Object[] terrainDataPresetsObjects = Resources.LoadAll("TerrainDataPresets");
        terrainDataPresets = new TerrainDataPreset[terrainDataPresetsObjects.Length];
        terrainDataPresetsObjects.CopyTo(terrainDataPresets, 0);
        List<TMP_Dropdown.OptionData> newOptions = new List<TMP_Dropdown.OptionData>();
        int defaultPreset = 0;
        for (int i = 0; i < terrainDataPresets.Length; i++)
        {
            if (terrainDataPresets[i].IsDefault) defaultPreset = i;
            newOptions.Add(new TMP_Dropdown.OptionData(terrainDataPresets[i].DisplayName));
        }
        terrainDataPresetDropdown.ClearOptions();
        terrainDataPresetDropdown.AddOptions(newOptions);
        terrainDataPresetDropdown.value = defaultPreset;
        TerrainData.ApplyPreset(terrainDataPresets[defaultPreset]);

        chunkSizeInputFieldOnValueChange = (float newValue) => OnContinuousValueChange(() => TerrainData.chunkSize = newValue);
        chunkSizeInputFieldOnValueChanged = (float newValue) => OnContinuousValueChanged(() => TerrainData.chunkSize = newValue);
        chunkTriangleLimitInputFieldOnValueChange = (int newValue) => OnContinuousValueChange(() => TerrainData.chunkTriangleLimit = newValue);
        chunkTriangleLimitInputFieldOnValueChanged = (int newValue) => OnContinuousValueChanged(() => TerrainData.chunkTriangleLimit = newValue);
        seedInputFieldOnValueChange = (int newValue) => OnContinuousValueChange(() => TerrainData.seed = newValue);
        seedInputFieldOnValueChanged = (int newValue) => OnContinuousValueChanged(() => TerrainData.seed = newValue);
        terrainSizeXInputFieldOnValueChange = (float newValue) => OnContinuousValueChange(() => TerrainData.terrainSizeX = newValue);
        terrainSizeXInputFieldOnValueChanged = (float newValue) => OnContinuousValueChanged(() => TerrainData.terrainSizeX = newValue);
        terrainSizeYInputFieldOnValueChange = (float newValue) => OnContinuousValueChange(() => TerrainData.terrainSizeY = newValue);
        terrainSizeYInputFieldOnValueChanged = (float newValue) => OnContinuousValueChanged(() => TerrainData.terrainSizeY = newValue);
        terrainSizeZInputFieldOnValueChange = (float newValue) => OnContinuousValueChange(() => TerrainData.terrainSizeZ = newValue);
        terrainSizeZInputFieldOnValueChanged = (float newValue) => OnContinuousValueChanged(() => TerrainData.terrainSizeZ = newValue);
        cubesPerUnitInputFieldOnValueChange = (int newValue) => OnContinuousValueChange(() => TerrainData.cubesPerUnit = newValue);
        cubesPerUnitInputFieldOnValueChanged = (int newValue) => OnContinuousValueChanged(() => TerrainData.cubesPerUnit = newValue);
        noiseFrequencyInputFieldOnValueChange = (float newValue) => OnContinuousValueChange(() => TerrainData.noiseFrequency = newValue);
        noiseFrequencyInputFieldOnValueChanged = (float newValue) => OnContinuousValueChanged(() => TerrainData.noiseFrequency = newValue);
        noiseStrengthInputFieldOnValueChange = (float newValue) => OnContinuousValueChange(() => TerrainData.noiseStrength = newValue);
        noiseStrengthInputFieldOnValueChanged = (float newValue) => OnContinuousValueChanged(() => TerrainData.noiseStrength = newValue);
        noiseLayersInputFieldOnValueChange = (int newValue) => OnContinuousValueChange(() => TerrainData.noiseLayers = newValue);
        noiseLayersInputFieldOnValueChanged = (int newValue) => OnContinuousValueChanged(() => TerrainData.noiseLayers = newValue);
        noiseLayerFrequencyMultiplierInputFieldOnValueChange = (float newValue) => OnContinuousValueChange(() => TerrainData.noiseLayerFrequencyMultiplier = newValue);
        noiseLayerFrequencyMultiplierInputFieldOnValueChanged = (float newValue) => OnContinuousValueChanged(() => TerrainData.noiseLayerFrequencyMultiplier = newValue);
}

    private void OnEnable()
    {
        RefreshAllInputs();

        terrainUpdateMethodDropdown.onValueChanged.AddListener((int newValue) => OnTerrainUpdateMethodChange(newValue));
        generationMethodDropdown.onValueChanged.AddListener((int newValue) => OnDescreteValueChange(() => TerrainData.generationMethod = (TerrainData.GenerationMethod)newValue));
        terrainDataPresetDropdown.onValueChanged.AddListener((int newValue) => OnDescreteValueChange(() => { TerrainData.ApplyPreset(terrainDataPresets[newValue]); RefreshAllInputs();}));
        interpolateVerticesToggle.onValueChanged.AddListener((bool newValue) => OnDescreteValueChange(() => TerrainData.interpolateVertices = newValue));
        smoothNormalsToggle.onValueChanged.AddListener((bool newValue) => OnDescreteValueChange(() => TerrainData.smoothNormals = newValue));
        showGenerationTimeToggle.onValueChanged.AddListener((bool newValue) => TerrainData.showGenerationTime = newValue);
        useAppendBuffersToggle.onValueChanged.AddListener((bool newValue) => OnDescreteValueChange(() => TerrainData.useAppendBuffers = newValue));
        trimMeshesAndBuffersToggle.onValueChanged.AddListener((bool newValue) => OnDescreteValueChange(() => TerrainData.trimMeshesAndBuffers = newValue));
        useIndirectRenderingToggle.onValueChanged.AddListener((bool newValue) => OnDescreteValueChange(() => TerrainData.useIndirectRendering = newValue));
        useSimpleShadersToggle.onValueChanged.AddListener((bool newValue) => OnDescreteValueChange(() => TerrainData.useSimpleShaders = newValue));
        showChunkBordersToggle.onValueChanged.AddListener((bool newValue) => OnDescreteValueChange(() => TerrainData.showChunkBorders = newValue));
        chunkSizeInputField.OnValueChange += chunkSizeInputFieldOnValueChange;
        chunkSizeInputField.OnValueChanged += chunkSizeInputFieldOnValueChanged;
        chunkTriangleLimitInputField.OnValueChange += chunkTriangleLimitInputFieldOnValueChange;
        chunkTriangleLimitInputField.OnValueChanged += chunkTriangleLimitInputFieldOnValueChanged;
        seedInputField.OnValueChange += seedInputFieldOnValueChange;
        seedInputField.OnValueChanged += seedInputFieldOnValueChanged;
        terrainSizeXInputField.OnValueChange += terrainSizeXInputFieldOnValueChange;
        terrainSizeXInputField.OnValueChanged += terrainSizeXInputFieldOnValueChanged;
        terrainSizeYInputField.OnValueChange += terrainSizeYInputFieldOnValueChange;
        terrainSizeYInputField.OnValueChanged += terrainSizeYInputFieldOnValueChanged;
        terrainSizeZInputField.OnValueChange += terrainSizeZInputFieldOnValueChange;
        terrainSizeZInputField.OnValueChanged += terrainSizeZInputFieldOnValueChanged;
        cubesPerUnitInputField.OnValueChange += cubesPerUnitInputFieldOnValueChange;
        cubesPerUnitInputField.OnValueChanged += cubesPerUnitInputFieldOnValueChanged;
        noiseFrequencyInputField.OnValueChange += noiseFrequencyInputFieldOnValueChange;
        noiseFrequencyInputField.OnValueChanged += noiseFrequencyInputFieldOnValueChanged;
        noiseStrengthInputField.OnValueChange += noiseStrengthInputFieldOnValueChange;
        noiseStrengthInputField.OnValueChanged += noiseStrengthInputFieldOnValueChanged;
        noiseLayersInputField.OnValueChange += noiseLayersInputFieldOnValueChange;
        noiseLayersInputField.OnValueChanged += noiseLayersInputFieldOnValueChanged;
        noiseLayerFrequencyMultiplierInputField.OnValueChange += noiseLayerFrequencyMultiplierInputFieldOnValueChange;
        noiseLayerFrequencyMultiplierInputField.OnValueChanged += noiseLayerFrequencyMultiplierInputFieldOnValueChanged;
        updateTerrainButton.onClick.AddListener(OnUpdateTerrainButtonPress);
    }

    private void OnDisable()
    {
        terrainUpdateMethodDropdown.onValueChanged.RemoveAllListeners();
        generationMethodDropdown.onValueChanged.RemoveAllListeners();
        terrainDataPresetDropdown.onValueChanged.RemoveAllListeners();
        interpolateVerticesToggle.onValueChanged.RemoveAllListeners();
        smoothNormalsToggle.onValueChanged.RemoveAllListeners();
        useAppendBuffersToggle.onValueChanged.RemoveAllListeners();
        trimMeshesAndBuffersToggle.onValueChanged.RemoveAllListeners();
        useIndirectRenderingToggle.onValueChanged.RemoveAllListeners();
        useSimpleShadersToggle.onValueChanged.RemoveAllListeners();
        showChunkBordersToggle.onValueChanged.RemoveAllListeners();
        chunkSizeInputField.OnValueChange -= chunkSizeInputFieldOnValueChange;
        chunkSizeInputField.OnValueChanged -= chunkSizeInputFieldOnValueChanged;
        chunkTriangleLimitInputField.OnValueChange -= chunkTriangleLimitInputFieldOnValueChange;
        chunkTriangleLimitInputField.OnValueChanged -= chunkTriangleLimitInputFieldOnValueChanged;
        seedInputField.OnValueChange -= seedInputFieldOnValueChange;
        seedInputField.OnValueChanged -= seedInputFieldOnValueChanged;
        terrainSizeXInputField.OnValueChange -= terrainSizeXInputFieldOnValueChange;
        terrainSizeXInputField.OnValueChanged -= terrainSizeXInputFieldOnValueChanged;
        terrainSizeYInputField.OnValueChange -= terrainSizeYInputFieldOnValueChange;
        terrainSizeYInputField.OnValueChanged -= terrainSizeYInputFieldOnValueChanged;
        terrainSizeZInputField.OnValueChange -= terrainSizeZInputFieldOnValueChange;
        terrainSizeZInputField.OnValueChanged -= terrainSizeZInputFieldOnValueChanged;
        cubesPerUnitInputField.OnValueChange -= cubesPerUnitInputFieldOnValueChange;
        cubesPerUnitInputField.OnValueChanged -= cubesPerUnitInputFieldOnValueChanged;
        noiseFrequencyInputField.OnValueChange -= noiseFrequencyInputFieldOnValueChange;
        noiseFrequencyInputField.OnValueChanged -= noiseFrequencyInputFieldOnValueChanged;
        noiseStrengthInputField.OnValueChange -= noiseStrengthInputFieldOnValueChange;
        noiseStrengthInputField.OnValueChanged -= noiseStrengthInputFieldOnValueChanged;
        noiseLayersInputField.OnValueChange -= noiseLayersInputFieldOnValueChange;
        noiseLayersInputField.OnValueChanged -= noiseLayersInputFieldOnValueChanged;
        noiseLayerFrequencyMultiplierInputField.OnValueChange -= noiseLayerFrequencyMultiplierInputFieldOnValueChange;
        noiseLayerFrequencyMultiplierInputField.OnValueChanged -= noiseLayerFrequencyMultiplierInputFieldOnValueChanged;
        updateTerrainButton.onClick.RemoveAllListeners();
    }

    public void RefreshAllInputs()
    {
        terrainUpdateMethodDropdown.value = (int)terrainUpdateMethod;
        generationMethodDropdown.value = (int)TerrainData.generationMethod;
        interpolateVerticesToggle.isOn = TerrainData.interpolateVertices;
        smoothNormalsToggle.isOn = TerrainData.smoothNormals;
        showGenerationTimeToggle.isOn = TerrainData.showGenerationTime;
        useAppendBuffersToggle.isOn = TerrainData.useAppendBuffers;
        trimMeshesAndBuffersToggle.isOn = TerrainData.trimMeshesAndBuffers;
        useIndirectRenderingToggle.isOn = TerrainData.useIndirectRendering;
        useSimpleShadersToggle.isOn = TerrainData.trimMeshesAndBuffers;
        showChunkBordersToggle.isOn = TerrainData.showChunkBorders;
        updateTerrainButton.gameObject.SetActive(terrainUpdateMethod == TerrainUpdateMethod.OnButtonPress);
        chunkSizeInputField.Init(TerrainData.chunkSize_MIN, TerrainData.chunkSize_MAX, TerrainData.chunkSize, TerrainData.chunkSize_STEP);
        chunkTriangleLimitInputField.Init(TerrainData.chunkTriangleLimit_MIN, TerrainData.chunkTriangleLimit_MAX, TerrainData.chunkTriangleLimit);
        seedInputField.Init(TerrainData.seed_MIN, TerrainData.seed_MAX, TerrainData.seed);
        terrainSizeXInputField.Init(TerrainData.terrainSizeX_MIN, TerrainData.terrainSizeX_MAX, TerrainData.terrainSizeX, TerrainData.terrainSize_STEP);
        terrainSizeYInputField.Init(TerrainData.terrainSizeY_MIN, TerrainData.terrainSizeY_MAX, TerrainData.terrainSizeY, TerrainData.terrainSize_STEP);
        terrainSizeZInputField.Init(TerrainData.terrainSizeZ_MIN, TerrainData.terrainSizeZ_MAX, TerrainData.terrainSizeZ, TerrainData.terrainSize_STEP);
        cubesPerUnitInputField.Init(TerrainData.cubesPerUnit_MIN, TerrainData.cubesPerUnit_MAX, TerrainData.cubesPerUnit);
        noiseFrequencyInputField.Init(TerrainData.noiseFrequency_MIN, TerrainData.noiseFrequency_MAX, TerrainData.noiseFrequency, TerrainData.noiseFrequency_STEP);
        noiseStrengthInputField.Init(TerrainData.noiseStrength_MIN, TerrainData.noiseStrength_MAX, TerrainData.noiseStrength, TerrainData.noiseStrength_STEP);
        noiseLayersInputField.Init(TerrainData.noiseLayers_MIN, TerrainData.noiseLayers_MAX, TerrainData.noiseLayers);
        noiseLayerFrequencyMultiplierInputField.Init(TerrainData.noiseLayerFrequencyMultiplier_MIN, TerrainData.noiseLayerFrequencyMultiplier_MAX, TerrainData.noiseLayerFrequencyMultiplier, TerrainData.noiseLayerFrequencyMultiplier_STEP);

        RefreshInputAccessibility();
    }

    public void RefreshInputAccessibility()
    {
        useAppendBuffersToggle.interactable = TerrainData.IsKeepingGpuMemory();
        trimMeshesAndBuffersToggle.interactable = TerrainData.IsKeepingGpuMemory() && useAppendBuffersToggle.isOn;
        chunkTriangleLimitInputField.Interactable = TerrainData.IsKeepingGpuMemory() && !TerrainData.trimMeshesAndBuffers && TerrainData.useAppendBuffers;
    }

    private void PopulateDropdownWithEnum(TMP_Dropdown dropdown, Enum targetEnum)
    {
        Type enumType = targetEnum.GetType();
        List<TMP_Dropdown.OptionData> newOptions = new List<TMP_Dropdown.OptionData>();

        for (int i = 0; i < Enum.GetNames(enumType).Length; i++)
        {
            newOptions.Add(new TMP_Dropdown.OptionData(Enum.GetName(enumType, i)));
        }

        dropdown.ClearOptions();
        dropdown.AddOptions(newOptions);
    }

    private void OnTerrainUpdateMethodChange(int newValue)
    {
        terrainUpdateMethod = (TerrainUpdateMethod) newValue;
        updateTerrainButton.gameObject.SetActive(terrainUpdateMethod == TerrainUpdateMethod.OnButtonPress);
    }

    private void OnDescreteValueChange(Action valueChange)
    {
        if (!BlockUpdateOnValueChange)
        {
            valueChange?.Invoke();
            RefreshInputAccessibility();
            if (terrainUpdateMethod == TerrainUpdateMethod.OnValueChange || terrainUpdateMethod == TerrainUpdateMethod.OnValueChanged)
            {
                terrainGenerator.GenerateTerrain();
            }
        }
    }

    private void OnContinuousValueChange(Action valueChange)
    {
        if (!BlockUpdateOnValueChange)
        {
            valueChange?.Invoke();
            if (terrainUpdateMethod == TerrainUpdateMethod.OnValueChange)
            {
                terrainGenerator.GenerateTerrain();
            }
        } 
    }

    private void OnContinuousValueChanged(Action valueChange)
    {
        if (!BlockUpdateOnValueChange)
        {
            valueChange?.Invoke();
            if (terrainUpdateMethod == TerrainUpdateMethod.OnValueChanged)
            {
                terrainGenerator.GenerateTerrain();
            }
        }
    }

    private void OnUpdateTerrainButtonPress()
    {
        if (terrainUpdateMethod == TerrainUpdateMethod.OnButtonPress)
        {
            terrainGenerator.GenerateTerrain();
        }
    }
}