using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class GlowEffect : MonoBehaviour
{
    public Color glowColor = Color.white;
    public float glowIntensity = 0.5f;
    public float glowSpeed = 2f;

    private SpriteRenderer sr;
    private Color baseColor;
    private float t;

    void Start()
    {
        sr = GetComponent<SpriteRenderer>();
        baseColor = sr.color;
    }

    void Update()
    {
        // t будет меняться от 0 до 1 как синусоида
        t = (Mathf.Sin(Time.time * glowSpeed) + 1f) / 2f;

        // создаём эффект отблеска: накладываем цвет свечения на базовый
        sr.color = Color.Lerp(baseColor, glowColor, t * glowIntensity);
    }
}