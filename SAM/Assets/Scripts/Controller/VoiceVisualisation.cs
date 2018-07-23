/**
* Rafael Peixoto 2018 - All Rights Reserved
* Virtual Reality with AI chatbot - VRAI Project
* 
* This is the controller that analyse the sound from
* the Text to Speech Service and change the size of the sound cubes
*/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VoiceVisualisation : MonoBehaviour
{

    #region PLEASE SET THESE VARIABLES IN THE INSPECTOR
    [Space(10)]
    [Header("Parameters for sound visualisation")]
    [Tooltip("Cubes game object for visualisation")]
    [SerializeField]
    private GameObject[] cubes;
    [Tooltip("The maximum scale")]
    [SerializeField]
    private float maxVisualisationScale = 25.0f;
    [Tooltip("The visual modifier")]
    [SerializeField]
    private float visualModifier = 50.0f;
    [Tooltip("The smooth speed")]
    [SerializeField]
    private float smoothSpeed = 10.0f;
    [Tooltip("The keep percentage")]
    [SerializeField]
    private float keePercentage = 0.5f;
    [Header("Text to speech script")]
    [Tooltip("The script of the text to speech")]
    [SerializeField]
    private Speak speak;
    #endregion

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
    private int amountOfVisualisations;

    private Speak textToSpeech;

    // Use this for initialization
    private void Start()
    {

        samples = new float[SAMPLE_SIZE];
        spectrum = new float[SAMPLE_SIZE];
        amountOfVisualisations = cubes.Length;


        SpawnLine();

    }

    /*
     *  SpawnLine() method
     *  Store the transform values of the cubes in an array
     */
    private void SpawnLine()
    {
        visualisationScale = new float[amountOfVisualisations];
        visualisationList = new Transform[amountOfVisualisations];

        for (int i = 0; i < amountOfVisualisations; i++)
        {
            visualisationList[i] = cubes[i].transform;
        }
    }

    /*
     *  Update() method
     *  Update is called once per frame
     *  Check if the Text to Speech is synthesizing
     *  If it is, set it to current source
     */
    private void Update()
    {
        if (speak.synthesizedVoice != null)
        {
            source = speak.synthesizedVoice;
            // Read from the audio file
            sampleRate = AudioSettings.outputSampleRate;
            AnalyseSound();
            UpdateVisual();

        }

    }

    /*
     *  UpdateVisual() method
     *  Depending of the source sound valus the method
     *  change the cubes size to visualise the sound in the game
     */
    private void UpdateVisual()
    {
        int spectrumIndex = 0;
        int averageSize = (int)((SAMPLE_SIZE * keePercentage) / amountOfVisualisations);

        for (int visualIndex = 0; visualIndex < amountOfVisualisations; visualIndex++)
        {

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

            visualisationList[visualIndex].localScale = new Vector3(visualisationList[visualIndex].localScale.x, (1 + visualisationScale[visualIndex]), visualisationList[visualIndex].localScale.z);
        }
    }


    /*
     *  AnalyseSound() method
     *  It analyse the current sound source
     */
    private void AnalyseSound()
    {
        // 0 is the channel
        source.GetOutputData(samples, 0);

        float sum = 0;
        // Retrieve the rms value
        for (int i = 0; i < SAMPLE_SIZE; i++)
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
