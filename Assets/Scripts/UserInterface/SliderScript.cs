using System;
using UnityEngine;
using UnityEngine.EventSystems;

public class SliderScript : MonoBehaviour, IPointerUpHandler
{
    public event Action OnRelease;

    public void OnPointerUp(PointerEventData eventData)
    {
        OnRelease?.Invoke();
    }
}