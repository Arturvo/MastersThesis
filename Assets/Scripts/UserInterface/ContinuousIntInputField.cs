using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ContinuousIntInputField : MonoBehaviour
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

    public event Action<int> OnValueChange;
    public event Action<int> OnValueChanged;

    private bool valueChangedSinceLastRelease;
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

    public void Init(int minValue, int maxValue, int startValue)
    {
        inputSlider.minValue = minValue;
        inputSlider.maxValue = maxValue;
        UpdateSliderValue(startValue);
        UpdateTextField(startValue);
    }

    private void OnSliderValueChange(float newValue)
    {
        if (!preventSliderUpdateEvent)
        {
            int newValueInt = Mathf.RoundToInt(newValue);
            UpdateTextField(newValueInt);
            OnValueChange?.Invoke(newValueInt);
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
                OnValueChanged?.Invoke(currentValueInt);
            }
            valueChangedSinceLastRelease = false;
        }
    }

    private void OnTextChange(string newValue)
    {
        if (!preventTextUpdateEvent && !IsTextValid(newValue))
        {
            int currentValueInt = Mathf.RoundToInt(inputSlider.value);
            UpdateTextField(currentValueInt);
            inputField.MoveToEndOfLine(false, false);
        }
    }

    private void OnTextChanged(string newValue)
    {
        if (!preventTextUpdateEvent && IsTextValid(newValue))
        {
            int newValueInt = Convert.ToInt32(newValue);
            if (newValueInt != Mathf.RoundToInt(inputSlider.value))
            {
                UpdateSliderValue(newValueInt);
                OnValueChange?.Invoke(newValueInt);
                OnValueChanged?.Invoke(newValueInt);
            }
        }
    }

    private bool IsTextValid(string text)
    {
        foreach (char letter in text)
        {
            if (!char.IsDigit(letter))
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

    private void UpdateTextField(int newTextFieldValue)
    {
        preventTextUpdateEvent = true;
        inputField.text = newTextFieldValue.ToString();
        preventTextUpdateEvent = false;
    }
}
