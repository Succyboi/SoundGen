using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FloatTransform : MonoBehaviour
{
    public float rotationSpeed;
    public float movementSpeed;

    public Vector3 rotationRange;
    public Vector3 movementRange;

    private float rotationTimer;
    private float movementTimer;
    private Vector3 rotationSeed;
    private Vector3 movementSeed;

    private Vector3 startPosition;
    private Vector3 startRotation;

    private void Start()
    {
        rotationSeed = new Vector3(Random.value * 100, Random.value * 100, Random.value * 100);
        movementSeed = new Vector3(Random.value * 100, Random.value * 100, Random.value * 100);

        startPosition = transform.localPosition;
        startRotation = transform.localRotation.eulerAngles;
    }

    private void Update()
    {
        rotationTimer += Time.deltaTime * rotationSpeed;
        movementTimer += Time.deltaTime * movementSpeed;

        transform.localRotation =
            Quaternion.Euler(
                new Vector3(
                    Mathf.Lerp(-rotationRange.x, rotationRange.x, Mathf.PerlinNoise(0.5f, rotationSeed.x + rotationTimer)),
                    Mathf.Lerp(-rotationRange.y, rotationRange.y, Mathf.PerlinNoise(0.5f, rotationSeed.y + rotationTimer)),
                    Mathf.Lerp(-rotationRange.z, rotationRange.z, Mathf.PerlinNoise(0.5f, rotationSeed.z + rotationTimer))
                    ) + startRotation);

        transform.localPosition =
            new Vector3(
                    Mathf.Lerp(-movementRange.x, movementRange.x, Mathf.PerlinNoise(0.5f, movementSeed.x + movementTimer)),
                    Mathf.Lerp(-movementRange.y, movementRange.y, Mathf.PerlinNoise(0.5f, movementSeed.y + movementTimer)),
                    Mathf.Lerp(-movementRange.z, movementRange.z, Mathf.PerlinNoise(0.5f, movementSeed.z + movementTimer))
                ) + startPosition;
    }
}
