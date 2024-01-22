using UnityEngine;
using System.Collections;

public class Floating : MonoBehaviour
{
    
    public float RottationSpeed = 15.0f;
    public float Bounciness = 0.5f;
    public float Frequency = 1f;

    
    Vector3 posOffset = new Vector3();
    Vector3 tempPos = new Vector3();

    
    void Start()
    {
        
        posOffset = transform.position;
    }

    
    void Update()
    {
        
        transform.Rotate(new Vector3(0f, Time.deltaTime * RottationSpeed, 0f), Space.World);

        tempPos = posOffset;
        tempPos.y += Mathf.Sin(Time.fixedTime * Mathf.PI * Frequency) * Bounciness;

        transform.position = tempPos;
    }
}