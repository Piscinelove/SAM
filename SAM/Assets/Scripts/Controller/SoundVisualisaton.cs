using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundVisualisaton : MonoBehaviour {

    public GameObject[] cubes;
    public float maxVisualisationScale = 25.0f;
    public float visualModifier = 50.0f;
    public float smoothSpeed = 10.0f;
    public float keePercentage = 0.5f;

    private const int SAMPLE_SIZE = 1024;

    private AudioSource source;
    private float[] samples;
    private float[] spectrum;
    private float sampleRate;

    // Values calculated every frame
    // Average power output of the sound
    public float rmsValue;
    // Decibel value during one frame
    public float dbValue;

    // Visualisation transform list

    private Transform[] visualisationList;
    private float[] visualisationScale;
    public int test = 8;

    public SoundVisualisaton(AudioSource source)
    {
        this.source = source;
    }


    // Use this for initialization
    private void Start ()
    {

        samples = new float[SAMPLE_SIZE];
        spectrum = new float[SAMPLE_SIZE];


        SpawnLine();

    }

    private void SpawnLine()
    {
        visualisationScale = new float[test];
        visualisationList = new Transform[test];

        //GameObject[] cubes = GameObject.FindGameObjectsWithTag("SoundVisualcube");
        for (int i = 0; i < test; i++)
        {
            //GameObject go = GameObject.CreatePrimitive(PrimitiveType.Cube) as GameObject;
            //GameObject go = cubes[i];
            visualisationList[i] = cubes[i].transform;
            //visualisationList[i].position = Vector3.right * i;
        }
    }


    // Update is called once per frame
    private void Update()
    {
        if (GetComponent<Speak>().source != null)
        {
            source = GetComponent<Speak>().source;
            // Read from the audio file
            sampleRate = AudioSettings.outputSampleRate;
            AnalyseSound();
            UpdateVisual();

        }

    }

    private void UpdateVisual()
    {
        int spectrumIndex = 0;
        int averageSize = (int) ((SAMPLE_SIZE* keePercentage) / test);

        Debug.Log(" Amount of Visual : " + test);
        for (int visualIndex = 0; visualIndex < test; visualIndex++)
        {
            Debug.Log("VisualIndex : " + visualIndex + " Amount of Visual : " + test);
            float sum = 0;
            for (int j = 0; j < averageSize; j++)
            {
                sum += spectrum[spectrumIndex];
                spectrumIndex++;

            }

            float scaleY = sum / averageSize * visualModifier;
            visualisationScale[visualIndex] -= Time.deltaTime * smoothSpeed;
            if (visualisationScale[visualIndex] < scaleY)
                visualisationScale[visualIndex] = scaleY;

            if (visualisationScale[visualIndex] > maxVisualisationScale)
                visualisationScale[visualIndex] = maxVisualisationScale;

            visualisationList[visualIndex].localScale = new Vector3(visualisationList[visualIndex].localScale.x, (20+20*visualisationScale[visualIndex]), visualisationList[visualIndex].localScale.z);
                //Vector3.one + Vector3.up * visualisationScale[visualIndex];
        }
    }



    private void AnalyseSound()
    {
        // 0 is the channel
        source.GetOutputData(samples, 0);

        float sum = 0;
        // Retrieve the rms value
        for (int i = 0; i < SAMPLE_SIZE;  i++)
        {
            sum += samples[i] * samples[i];
        }

        rmsValue = Mathf.Sqrt(sum / SAMPLE_SIZE);

        // Retrieve the DB value
        dbValue = 20 * Mathf.Log10(rmsValue / 0.1f);

        // Retrieve sound spectrum

        source.GetSpectrumData(spectrum, 0, FFTWindow.BlackmanHarris);

    }
}
