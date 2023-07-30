using System.Collections;
using TMPro;
using UnityEngine;

public class FpsCounter : MonoBehaviour
{
    [SerializeField] float updateFrequency = 0.1f;
    [SerializeField] int lowFpsTreshold = 30;
    [SerializeField] int mediumFpsTreshold = 60;
    [SerializeField] Color highFpsColor = Color.green;
    [SerializeField] Color mediumFpsColor = Color.yellow;
    [SerializeField] Color lowFpsColor = Color.red;
    [SerializeField] TextMeshProUGUI fpsCounterText;

    private IEnumerator Start()
    {
        while (true)
        {
            int fps = Mathf.RoundToInt(1f / Time.unscaledDeltaTime);
            SetFpsCounterText(fps);
            yield return new WaitForSeconds(updateFrequency);
        }
    }

    private void SetFpsCounterText(int fps)
    {
        fpsCounterText.text = fps + " FPS";
        if (fps > mediumFpsTreshold)
        {
            fpsCounterText.color = highFpsColor;
        }
        else if (fps > lowFpsTreshold)
        {
            fpsCounterText.color = mediumFpsColor;
        }
        else
        {
            fpsCounterText.color = lowFpsColor;
        }
    }
}
