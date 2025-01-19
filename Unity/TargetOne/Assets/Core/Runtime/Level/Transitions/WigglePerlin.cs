using UnityEngine;

public class WigglePerlin : MonoBehaviour
{
    public Vector3 Speed;
    public Vector3 Travel;
    public float Seed;
    private Quaternion _originalLocalRotation;

    void Awake()
    {
        _originalLocalRotation = transform.localRotation; // Save the original local rotation
        if (Seed < 0f)
            Seed = Random.Range(0f, 10000f);
    }

    void Update()
    {
        var gameSpeed = GameSessionController.Instance?.GameSpeed ?? 1f;
        float seededTime = Time.time + Seed;
        float timex = seededTime * Speed.x * gameSpeed;
        float timey = seededTime * Speed.y * gameSpeed;
        float timez = seededTime * Speed.z * gameSpeed;

        // Calculate Perlin noise-based rotation offset
        Vector3 rotationOffset = new Vector3(
            (Mathf.PerlinNoise(timex, timex) - 0.5f) * Travel.x,
            (Mathf.PerlinNoise(timey + 1f, timey + 1f) - 0.5f) * Travel.y,
            (Mathf.PerlinNoise(timez + 2f, timez + 2f) - 0.5f) * Travel.z
        );

        // Combine the rotation offset with the original local rotation
        Quaternion noiseRotation = Quaternion.Euler(rotationOffset);
        transform.localRotation = _originalLocalRotation * noiseRotation;
    }
}