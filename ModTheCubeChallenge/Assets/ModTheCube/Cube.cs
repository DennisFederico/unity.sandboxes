using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cube : MonoBehaviour
{
    Vector3 positionLimits = new Vector3(3.5f, 3.5f, 3.5f);
    Vector3 startPosition;
    Vector3 targetPosition;
    float maxSpeed = 6f; //Units per second
    float minSpeed = 2f; //Units per second
    public float moveSpeed;
    float moveTimeStart;
    float moveDistance;
    float moveTime;
    public float lerpFraction;

    public Vector2 scaleRange = new Vector2(0.5f, 3f);
    float scaleStart;
    float scaleTarget;

    Color startColor;
    Color targetColor;

    public float rotationX;
    float rotationY;
    float rotationZ;


    public MeshRenderer Renderer;
    
    void Start()
    {
        transform.position = GetRandomPosition();
        transform.localScale = Vector3.one * Random.Range(scaleRange.x, scaleRange.y);
        Renderer.material.color = GetRandomColor();
        InitChanges();
    }
    
    void Update()
    {
        lerpFraction = (Time.time - moveTimeStart) / moveTime;
        transform.position = Vector3.Lerp(startPosition, targetPosition, lerpFraction);
        transform.localScale = Vector3.one * Mathf.Lerp(scaleStart, scaleTarget, lerpFraction);
        Renderer.material.color = Color.Lerp(startColor, targetColor, lerpFraction);
        if (lerpFraction > 1) {
            InitChanges();
        }
        transform.Rotate(rotationX * Time.deltaTime, rotationY * Time.deltaTime, rotationZ * Time.deltaTime);
    }

    Vector3 GetRandomPosition() {
        return new Vector3(Random.Range(-positionLimits.x, positionLimits.x), Random.Range(-positionLimits.y, positionLimits.y), Random.Range(-positionLimits.z, positionLimits.z));
    }

    Color GetRandomColor() {
        return Random.ColorHSV(0, 1, 0, 1, 0, 1, .25f, 1f);
    }


    void InitChanges() {
        //Position Changes
        startPosition = transform.position;
        targetPosition = GetRandomPosition();
        moveSpeed = Random.Range(minSpeed, maxSpeed);
        moveTimeStart = Time.time;
        moveDistance = Vector3.Distance(startPosition, targetPosition);
        moveTime = moveDistance / moveSpeed;

        //Scale Changes
        scaleStart = transform.localScale.x;
        scaleTarget = Random.Range(scaleRange.x, scaleRange.y);

        startColor = Renderer.material.color;
        targetColor = GetRandomColor();

        rotationX = Random.Range(-120, 120);
        rotationY = Random.Range(-120, 120);
        rotationZ = Random.Range(-120, 120);

    }
    
}
