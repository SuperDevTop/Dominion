using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AutoRotator : MonoBehaviour
{
    public Vector3 dir;
    public float speed;
    void Update()
    {
        transform.Rotate(dir * speed * Time.smoothDeltaTime); 
    }
}
