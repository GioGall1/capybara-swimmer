using UnityEngine;

public class FramerateUnlocker : MonoBehaviour
{
    void Start()
    {
        Application.targetFrameRate = 60; // или 120, если планшет поддерживает
        QualitySettings.vSyncCount = 0;   // отключаем VSync (на всякий случай)
    }
}