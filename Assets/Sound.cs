using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Sound : MonoBehaviour
{
    AudioSource audioSource;

    [Range(0.1f, 5f)]  // Slider in the inspector to control the speed of sound synthesis
    public float speed = 1f;

    [Range(0f, 3f)]  // Slider in the inspector for amplitude
    public float amplitude = 3f;

    public float sampleRate = 44100f;
    float phase = 0;

    void Start()
    {
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.loop = true;  // Loop the audio source
        audioSource.Play();        // Start playing the sound
    }

    void OnAudioFilterRead(float[] data, int channels)
    {
        for (int i = 0; i < data.Length; i += channels)
        {
            phase += (2 * Mathf.PI * speed) / sampleRate;  // Update phase based on speed

            // Create a continuous wave using the FM function
            float wave = FM(phase, 1f, 0.5f); // Adjust modulation parameters as needed
            data[i] = amplitude * wave; // Set the audio sample
            data[i + 1] = data[i]; // For stereo, same signal for left and right

            // Reset phase if it exceeds 2π
            if (phase >= 2 * Mathf.PI)
            {
                phase -= 2 * Mathf.PI;
            }
        }
    }

    // Frequency Modulation computation
    public float FM(float phase, float carMul, float modMul)
    {
        return Mathf.Sin(phase * carMul) + Mathf.Sin(phase * modMul); // Fluctuating FM
    }

    // Function to update volume based on the average distance between boids
    public void CalculateDistance(List<Transform> boidTransforms)
    {
        if (boidTransforms.Count == 0) return;

        float totalDistance = 0f;
        int count = 0;

        // Calculate average distance between each pair of boids
        for (int i = 0; i < boidTransforms.Count; i++)
        {
            for (int j = i + 1; j < boidTransforms.Count; j++)
            {
                float distance = Vector3.Distance(boidTransforms[i].position, boidTransforms[j].position);
                totalDistance += distance;
                count++;
            }
        }

        // Calculate average distance
        float averageDistance = totalDistance / count;

        // Update amplitude based on average distance
        amplitude = Mathf.Clamp(1 / (1 + averageDistance), 0f, 1f); // Volume decreases with distance
    }
}
