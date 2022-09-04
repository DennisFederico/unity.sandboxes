using UnityEngine;
using System.Collections;

// A very simplistic car driving on the x-z plane.

public class Drive : MonoBehaviour
{
    public float speed = 0.1f;
    public GameObject fuel;

    void Update()
    {
        Vector3 direction = Vector3.Normalize(fuel.transform.position - transform.position);
        
        transform.position += direction * speed;

    }
}