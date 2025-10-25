using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FruitSway : MonoBehaviour
{
    private Quaternion initialRotation;
    private float swayAmount;
    private float swaySpeed;

    void Start()
    {
        initialRotation = transform.rotation;
        swayAmount = Random.Range(5f, 15f);   // угол колебания
        swaySpeed = Random.Range(2f, 3f);     // скорость колебания
    }

    void Update()
    {
        float angle = Mathf.Sin(Time.time * swaySpeed) * swayAmount;
        transform.rotation = initialRotation * Quaternion.Euler(0f, 0f, angle);
    }
}