using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AutoFloat : MonoBehaviour
{
    public float amplitude = 1f; // The amplitude of the sine function
    public float frequency = 1f; // The frequency of the sine function

    private Vector3 startPos; // The starting position of the GameObject

    private void Start()
    {
        startPos = transform.position; // Save the starting position
        frequency = Random.Range(0.2f, 1.2f);
    }

    private void Update()
    {
        float newY = startPos.y + amplitude * Mathf.Sin(Time.time * frequency); // Calculate the new y position using the sine function

        transform.position = new Vector3(transform.position.x, newY, transform.position.z); // Set the new position
    }
}
