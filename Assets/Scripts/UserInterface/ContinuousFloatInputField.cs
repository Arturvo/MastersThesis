using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;


public class ContinuousFloatInputField : MonoBehaviour
{
    [SerializeField] Slider inputSlider;
    [SerializeField] SliderScript sliderScript;
    [SerializeField] TMP_InputField inputField;

    public bool Interactable
    {
        set
        {
            inputSlider.interactable = value;
            inputField.interactable = value;
        }
    }

    public event Action<float> OnValueChange;
    public event Action<float> OnValueChanged;

    private bool valueChangedSinceLastRelease;
    private float step;
    private float minValue;
    private bool preventSliderUpdateEvent = false;
    private bool preventTextUpdateEvent = false;

    private void OnEnable()
    {
        inputSlider.onValueChanged.AddListener(OnSliderValueChange);
        sliderScript.OnRelease += OnSliderRelease;
        inputField.onValueChanged.AddListener(OnTextChange);
        inputField.onEndEdit.AddListener(OnTextChanged);
    }

    private void OnDisable()
    {
        inputSlider.onValueChanged.RemoveListener(OnSliderValueChange);
        sliderScript.OnRelease -= OnSliderRelease;
    }

    public void Init(float minValue, float maxValue, float startValue, float step)
    {
        this.step = step;
        this.minValue = minValue;
        int stepCount = Mathf.RoundToInt((maxValue - minValue) / step);
        inputSlider.minValue = 0;
        inputSlider.maxValue = stepCount;
        UpdateTextField(startValue);
        UpdateSliderValue(FindStep(startValue));
        
    }

    private float GetValue(int sliderValue)
    {
        return minValue + sliderValue * step;
    }

    private int FindStep(float value)
    {
        return Mathf.RoundToInt((value - minValue) / step);
    }

    private void OnSliderValueChange(float newValue)
    {
        if (!preventSliderUpdateEvent)
        {
            int newValueInt = Mathf.RoundToInt(newValue);
            float actualValue = GetValue(newValueInt);
            UpdateTextField(actualValue);
            OnValueChange?.Invoke(actualValue);
            valueChangedSinceLastRelease = true;
        }
    }

    private void OnSliderRelease()
    {
        if (!preventSliderUpdateEvent)
        {
            if (valueChangedSinceLastRelease)
            {
                int currentValueInt = Mathf.RoundToInt(inputSlider.value);
                float actualValue = GetValue(currentValueInt);
                OnValueChanged?.Invoke(actualValue);
            }
            valueChangedSinceLastRelease = false;
        }
    }

    private void OnTextChange(string newValue)
    {
        if (!preventTextUpdateEvent && !IsTextValid(newValue))
        {
            int currentValueInt = Mathf.RoundToInt(inputSlider.value);
            float actualValue = GetValue(currentValueInt);
            inputField.text = actualValue.ToString("F2");
            inputField.MoveToEndOfLine(false, false);
        }
    }

    private void OnTextChanged(string newValue)
    {
        if (!preventTextUpdateEvent && IsTextValid(newValue))
        {
            float newValueFloat = float.Parse(newValue);
            int newSliderValue = FindStep(newValueFloat);
            float actialValueFloat = GetValue(newSliderValue);

            if (newSliderValue != Mathf.RoundToInt(inputSlider.value))
            {
                UpdateSliderValue(newSliderValue);
                OnValueChange?.Invoke(actialValueFloat);
                OnValueChanged?.Invoke(actialValueFloat);
            }

            inputField.text = actialValueFloat.ToString("F2");
        }
    }

    private bool IsTextValid(string text)
    {
        foreach (char letter in text)
        {
            if (!char.IsDigit(letter) && !letter.Equals(','))
            {
                return false;
            }
        }
        return true;
    }

    private void UpdateSliderValue(int newSliderValue)
    {
        preventSliderUpdateEvent = true;
        inputSlider.value = newSliderValue;
        preventSliderUpdateEvent = false;
    }

    private void UpdateTextField(float newTextFieldValue)
    {
        preventTextUpdateEvent = true;
        inputField.text = newTextFieldValue.ToString("F2");
        preventTextUpdateEvent = false;
    }
}
