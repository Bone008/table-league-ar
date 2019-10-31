using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OrbitAnimation : MonoBehaviour
{
    public float anglePerSecond = 90;
    private float startAngle;

    void Start()
    {
        startAngle = transform.localEulerAngles.y;
    }

    void Update()
    {
        float angle = (startAngle + Time.time * anglePerSecond) % 360;
        transform.localRotation = Quaternion.Euler(0, angle, 0);
    }
}
