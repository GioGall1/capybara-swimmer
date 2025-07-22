using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class CapybaraAnimationController : MonoBehaviour
{
    public Sprite idleSprite;
    public Sprite bendSprite;
    public Sprite jumpSprite;
    public Sprite fallSprite;
    public Sprite blinkSprite;

    public float blinkDuration = 0.1f;
    public float blinkIntervalMin = 2f;
    public float blinkIntervalMax = 5f;

    private SpriteRenderer spriteRenderer;
    private float blinkTimer;
    private bool isBlinking = false;

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        SetIdle();
        blinkTimer = Random.Range(blinkIntervalMin, blinkIntervalMax);
    }

    void Update()
    {
        // Только если не в прыжке и не моргает
        if (!isBlinking)
        {
            blinkTimer -= Time.deltaTime;
            if (blinkTimer <= 0f)
            {
                Blink();
            }
        }
    }

    // Публичные методы для других скриптов:
    public void SetIdle()
    {
        if (!isBlinking)
            spriteRenderer.sprite = idleSprite;
    }

    public void SetBend()
    {
        if (!isBlinking)
            spriteRenderer.sprite = bendSprite;
    }

    public void SetJump()
    {
        if (!isBlinking)
            spriteRenderer.sprite = jumpSprite;
    }

    public void SetFall()
    {
        if (!isBlinking)
            spriteRenderer.sprite = fallSprite;
    }

    private void Blink()
    {
        isBlinking = true;
        spriteRenderer.sprite = blinkSprite;
        Invoke(nameof(EndBlink), blinkDuration);
    }

    private void EndBlink()
    {
        isBlinking = false;
        SetIdle(); // вернём нейтральное лицо
        blinkTimer = Random.Range(blinkIntervalMin, blinkIntervalMax);
    }
}