using UnityEngine;

public class SeaweedSwing : MonoBehaviour
{
    public float swingSpeed = 1f;
    public float swingAmplitude = 10f;

    private float baseAngle;

    void Start()
    {
        baseAngle = transform.localEulerAngles.z;
    }

    void Update()
    {
        float angle = Mathf.Sin(Time.time * swingSpeed) * swingAmplitude;
        transform.localEulerAngles = new Vector3(0, 0, baseAngle + angle);
    }
}