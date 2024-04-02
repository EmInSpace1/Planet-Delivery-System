using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovingPlanet : MonoBehaviour
{
    [SerializeField] private Transform[] movePoints;

    private int currentPoint;
    void Start()
    {
        
    }
    void FixedUpdate()
    {
        transform.position = Vector3.Lerp(transform.position, movePoints[currentPoint].position, 0.01f);

        if(Vector3.Distance(transform.position, movePoints[currentPoint].position) < 0.1f)
        {
            currentPoint++;

            if (currentPoint > movePoints.Length - 1) currentPoint = 0;
        }
    }
}
