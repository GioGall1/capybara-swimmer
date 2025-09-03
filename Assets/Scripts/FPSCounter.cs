using UnityEngine;
using UnityEngine.UI;

public class FPSCounter : MonoBehaviour
{
    public Text fpsText;
    private float timer;

    void Update()
    {
        timer += Time.unscaledDeltaTime;
        if (timer >= 0.5f) // обновление раз в полсекунды
        {
            float fps = 1f / Time.unscaledDeltaTime;
            fpsText.text = $"FPS: {Mathf.RoundToInt(fps)}";
            timer = 0f;
        }
    }
}