using System.Collections;
using System.Collections.Generic;
using UnityEngine.U2D.Animation;
using UnityEngine;

public class CapybaraDeathController : MonoBehaviour
{
    public SpriteRenderer normalRenderer;
    public SpriteSkin normalSkin;
    public GameObject deathSprite;
    public GameObject bubblePrefab;
    public Transform bubbleAnchor;

    public void PlayDeathPose()
    {
        if (normalRenderer != null)
            normalRenderer.enabled = false;

        if (normalSkin != null)
            normalSkin.enabled = false;

        if (deathSprite != null)
            deathSprite.SetActive(true);
            
        SpawnBubbleUnderCapybara();
        StartCoroutine(FloatUp());
    }

    private void SpawnBubbleUnderCapybara()
    {
        if (bubblePrefab == null || bubbleAnchor == null)
            return;

        GameObject bubble = Instantiate(bubblePrefab, bubbleAnchor.position, Quaternion.identity);

        bubble.transform.SetParent(deathSprite.transform);

        bubble.transform.position = bubbleAnchor.position;

        var rise = bubble.GetComponent<BubbleRise>();
        if (rise != null)
            rise.enabled = false;

        var rb = bubble.GetComponent<Rigidbody2D>();
        if (rb != null)
            rb.velocity = Vector2.zero;
    }

    public IEnumerator FloatUp(float duration = 2f, float distance = 1f)
    {
        Vector3 startPos = transform.position;
        Vector3 endPos = startPos + new Vector3(0, distance, 0);

        float t = 0;
        while (t < duration)
        {
            t += Time.deltaTime;
            transform.position = Vector3.Lerp(startPos, endPos, t / duration);
            yield return null;
        }
    }
}