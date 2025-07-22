using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Collider2D))]
public class Buoyancy2D : MonoBehaviour
{
    public float buoyancyForce = 20f;
    public float waterLinearDrag = 3f;
    public string waterTag = "Water";
    public float maxDepth = 1.5f;

    private Rigidbody2D rb;
    private float originalDrag;
    private bool isInWater = false;
    private Collider2D waterCollider;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        originalDrag = rb.drag;
    }

    void FixedUpdate()
    {
        if (!isInWater || waterCollider == null)
        {
            rb.drag = originalDrag;
            return;
        }

        Bounds waterBounds = waterCollider.bounds;
        float waterSurfaceY = waterBounds.max.y;
        float depth = waterSurfaceY - transform.position.y;

        if (depth > 0)
        {
            float normalizedDepth = Mathf.Clamp01(depth / maxDepth);
            float force = buoyancyForce * normalizedDepth;

            rb.AddForce(Vector2.up * force, ForceMode2D.Force);
            rb.drag = waterLinearDrag;
        }
        else
        {
            rb.drag = originalDrag;
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag(waterTag))
        {
            isInWater = true;
            waterCollider = other;
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag(waterTag))
        {
            isInWater = false;
            waterCollider = null;
        }
    }
}