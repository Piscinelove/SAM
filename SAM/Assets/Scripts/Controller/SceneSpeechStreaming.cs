/**
* Rafael Peixoto 2018 - All Rights Reserved
* Virtual Reality with AI chatbot - VRAI Project
* 
* NOTE: Based on example code from IBM which is subject to Apache license as noted below:
* 
* ---------------------------------------------------------------------------------------
* Copyright 2015 IBM Corp. All Rights Reserved.
*
* Licensed under the Apache License, Version 2.0 (the "License");
* you may not use this file except in compliance with the License.
* You may obtain a copy of the License at
*
*      http://www.apache.org/licenses/LICENSE-2.0
*
* Unless required by applicable law or agreed to in writing, software
* distributed under the License is distributed on an "AS IS" BASIS,
* WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
* See the License for the specific language governing permissions and
* limitations under the License.
* 
* ---------------------------------------------------------------------------------------
* 
* This is the main controller of the scene for interact by voice with the SAM AI Assistant.
* The following class records the real-time broadcast from the device's microphone and transmits
* it to the IBM Watson' Speech To Text Service. This service currently only offers a "always listening" mode.
* When the voice transcript of the message sent to Speech To Text is received, it is managed by the WatsonConversation class.
*/

using UnityEngine;
using System.Collections;
using IBM.Watson.DeveloperCloud.Logging;
using IBM.Watson.DeveloperCloud.Services.SpeechToText.v1;
using IBM.Watson.DeveloperCloud.Utilities;
using IBM.Watson.DeveloperCloud.DataTypes;
using System.Collections.Generic;
using UnityEngine.UI;

public class SceneSpeechStreaming : MonoBehaviour
{
    #region PLEASE SET THESE VARIABLES IN THE INSPECTOR
    [Space(10)]
    [Tooltip("The service URL (optional). This defaults to \"https://stream.watsonplatform.net/speech-to-text/api\"")]
    [SerializeField]
    private string serviceURL;
    [Header("Cloud Service Authentication")]
    [Tooltip("The authentication username.")]
    [SerializeField]
    private string username;
    [Tooltip("The authentication password.")]
    [SerializeField]
    private string password;
    [Header("Language Configuration")]
    [Tooltip("The language model")]
    [SerializeField]
    private string language;
    [Header("Chatbot conversation script")]
    [Tooltip("The script of the chatbot")]
    [SerializeField]
    private WatsonConversation conversation;
    #endregion

    private int recordingRoutine = 0;
    private string microphoneID = null;
    private AudioClip recording = null;
    // Microphone default unit is 1 second
    private int recordingBufferSize = 1;
    // Reduce the sample rate for lower bandwith
    // Overrides OS level setting
    // Choose the recommended setting according to the IBM's model used
    // 16000 Hz for the IBM's Broadband model
    // 8000 Hz to the IBM's Narrow model
    private int recordingHZ = 16000;
    // Adjusting chunkSize to 800 to correspond to a more even size given 16000 Hz sample rate
    // Each chunk will represent 1/20th of a second of recording
    private static int chunkSize = 800;
    // Sensitivity multiplier for average loudness calculation
    // RMS value for 0 dB
    private static float refValue = 0.1f;


    private string runningProcess = "SAM Voice Streaming Controller";

    private SpeechToText speechToText;


    void Start()
    {
        LogSystem.InstallDefaultReactors();
        CreateService();
    }

    private void CreateService()
    {
        //  Create credential and instantiate service
        Credentials credentials = null;
        if (!string.IsNullOrEmpty(username) && !string.IsNullOrEmpty(password))
        {
            //  Authenticate using username and password
            credentials = new Credentials(username, password, serviceURL);
        }
        else
        {
            throw new WatsonException("Please provide either username and password to authenticate the service.");
        }

        speechToText = new SpeechToText(credentials);
        speechToText.StreamMultipart = true;

        Active = true;
        StartRecording();
    }

    public bool Active
    {
        get { return speechToText.IsListening; }
        set
        {
            if (value && !speechToText.IsListening)
            {
                speechToText.RecognizeModel = language;
                speechToText.DetectSilence = true;
                speechToText.EnableWordConfidence = true;
                speechToText.EnableTimestamps = true;
                speechToText.SilenceThreshold = 0.00f;
                speechToText.MaxAlternatives = 0;
                speechToText.EnableInterimResults = true;
                speechToText.OnError = OnError;
                speechToText.InactivityTimeout = -1;
                speechToText.ProfanityFilter = false;
                speechToText.SmartFormatting = true;
                speechToText.SpeakerLabels = false;
                speechToText.WordAlternativesThreshold = null;
                speechToText.StartListening(OnRecognize, OnRecognizeSpeaker);
            }
            else if (!value && speechToText.IsListening)
            {
                speechToText.StopListening();
            }
        }
    }

    private void StartRecording()
    {
        if (recordingRoutine == 0)
        {
            UnityObjectUtil.StartDestroyQueue();
            recordingRoutine = Runnable.Run(RecordingHandler());
        }
    }

    private void StopRecording()
    {
        if (recordingRoutine != 0)
        {
            Microphone.End(microphoneID);
            Runnable.Stop(recordingRoutine);
            recordingRoutine = 0;
        }
    }

    private void OnError(string error)
    {
        Active = false;

        Log.Debug("ExampleStreaming.OnError()", "Error! {0}", error);
    }

    /*
     *  RecordingHandler() records the real-time broadcast from the device's microphone
     *  Recording relies on the microphone recording sample rate
     *  Warning : recordingHZ must be an even integer multiple of chunkSize
     *  Thanks to Michael Pickering - RMichaelPickering (GitHub) for the explanation how to
     *  reduce the important recording delay.
     */
    private IEnumerator RecordingHandler()
    {

        // Allows to keep track how much has been already written
        // Starts at zero
        int chunckEnd = 0;
        // Current read position of broadcast
        int readPosition = 0;
        // Factor for downsampling
        // Value 1 allows to not downsample
        int downSampleFactor = 1;
        // RMS value for sound level
        // It will be calculated per chunck of samples
        float rmsValue;
        // DB value of sound level
        // It will be calculated per chunck of samples
        float dbValue;
        // Float array of samples chunck for processing each chuck of audio samples
        float[] samplesChunk = null;

        Log.Debug("{0}", "devices: {1}", runningProcess, Microphone.devices);

        // Start recording
        // boolean value is for allowing looping records
        recording = Microphone.Start(microphoneID, true, recordingBufferSize, recordingHZ);

        Log.Debug("{0}", " Microphone Ring Buffer includes: {1} channels with a total of: {2} samples.", runningProcess, recording.channels.ToString(), recording.samples.ToString());



        // Microphone.Start returns null only on failure
        // Testing if the recording failed
        if (recording == null)
        {
            StopRecording();
            yield break;
        }

        // End of the first chuck is calculated with 'chuck * downSampleFactor -1'
        // First sample is at position zero
        chunckEnd = chunkSize * downSampleFactor - 1;

        // Calculate how long to wait for at least 1 audioChuck is ready
        yield return new WaitForSecondsRealtime(chunkSize * downSampleFactor / recordingHZ);


        while (recordingRoutine != 0 && recording != null)
        {
            // Get current writePosition of the microphone in the recording
            int writePosition = Microphone.GetPosition(microphoneID);
            // Testing if the microphone is still recording
            if (!Microphone.IsRecording(microphoneID))
            {
                Log.Error("MicrophoneWidget", "Microphone disconnected.");
                StopRecording();
                yield break;
            }

            // Make sure that at least chunckSize samples have been written
            while (writePosition > readPosition + chunckEnd || writePosition < readPosition)
            {
                // at least one chunk is recorded, make a RecordClip and pass it onto our callback.
                // We are now sure that at least one chuck is recorded
                // Creation of a RecordClip
                samplesChunk = new float[chunkSize * downSampleFactor];
                recording.GetData(samplesChunk, readPosition);

                
                AudioData record = new AudioData();
                // 20171018 RMPickering - The next statement seems to be setting the MaxLevel to the highest value from the samples, not taking into account the negative values.
                // record.MaxLevel = Mathf.Max(samples);


                // Calculate the max level of the highest value from the samples
                // Don't take into account the negative values (only absolute values)
                float sumSquaredSamples = 0; // sum squared samples
                float sumAbsoluteSamples = 0; // sum absolute values

                // Implementation of an anti-aliasing filter
                // Must be lower than 8000 Hz
                float CUTOFF = 6500.0f;
                float RC = 1.0f / (CUTOFF * 2.0f * 3.14f);
                // Using initial sample rate
                float dt = 1.0f / 16000.0f; 
                float alpha = dt / (RC + dt);

                // Calculate RMS and DB values
                sumSquaredSamples += samplesChunk[0] * samplesChunk[0];
                sumAbsoluteSamples += Mathf.Abs(samplesChunk[0]);

                // Application of the low pass filter
                int i = 0;
                for (i = 1; i < chunkSize * downSampleFactor; i++)
                {
                    // Low pass filter allows smoothing audio recorded above the cutoff frequency
                    samplesChunk[i] = samplesChunk[i - 1] + alpha * (samplesChunk[i] - samplesChunk[i - 1]);
                    sumSquaredSamples += samplesChunk[i] * samplesChunk[i]; // sum squared samples
                    sumAbsoluteSamples += Mathf.Abs(samplesChunk[i]);
                }

                // Calculate the square root of average = rmsValue
                rmsValue = Mathf.Sqrt(sumSquaredSamples / chunkSize);
                // Calculate the DB value
                dbValue = 20 * Mathf.Log10(rmsValue / refValue);
                // Set minimum dbValue to -160 dB
                if (dbValue < -160) dbValue = -160;

                // Set MaxLevel
                record.MaxLevel = rmsValue;

                // Set the clip recorded
                record.Clip = AudioClip.Create("audioChunk", chunkSize, 1, recordingHZ, false);

                // Copy the audio samples from the array samplesChuck into the clip recorded
                record.Clip.SetData(samplesChunk, 0);

                // Send the recorded clip to IBM Watson Speech To Text
                speechToText.OnListen(record);

                // Remember which block has been copied
                readPosition += chunkSize * downSampleFactor;
                if (readPosition > recordingHZ * recording.channels - 1)
                { 
                    //Reset readPosition to initial value and chunckEnd to begin a new buffer
                    readPosition = 0;
                    chunckEnd = chunkSize * downSampleFactor - 1;
                }
                else chunckEnd += chunkSize * downSampleFactor;
            }

            // Calculate wait time for nex Update and continue streaming of the micrphone
            yield return new WaitForSecondsRealtime(chunkSize * downSampleFactor / recordingHZ);

        }

        yield break;
    }

    private void OnRecognize(SpeechRecognitionEvent result, Dictionary<string, object> customData)
    {
        if (result != null && result.results.Length > 0)
        {
            foreach (var res in result.results)
            {
                foreach (var alt in res.alternatives)
                {
                    // Get Speech To Text Transcript result
                    string text = alt.transcript;
                    Log.Debug("{0}", string.Format("{1} ({2}, {3:0.00})\n", runningProcess, text, res.final ? "Final" : "Interim", alt.confidence));
                    // If the result is the final transcript
                    // Send it to the chatbot
                    if (res.final)
                        // Start of a coroutine
                        // If the chatbot still didn't respond to the previous message
                        // The programm waits before sending another request
                        StartCoroutine(TransferRecording(text));
                }

                if (res.keywords_result != null && res.keywords_result.keyword != null)
                {
                    foreach (var keyword in res.keywords_result.keyword)
                    {
                        Log.Debug("{0}", "keyword: {1}, confidence: {2}, start time: {3}, end time: {4}", runningProcess, keyword.normalized_text, keyword.confidence, keyword.start_time, keyword.end_time);
                    }
                }
            }
        }
    }

    private void OnRecognizeSpeaker(SpeakerRecognitionEvent result, Dictionary<string, object> customData)
    {
        if (result != null)
        {
            foreach (SpeakerLabelsResult labelResult in result.speaker_labels)
            {
                Log.Debug("ExampleStreaming.OnRecognize()", string.Format("speaker result: {0} | confidence: {3} | from: {1} | to: {2}", labelResult.speaker, labelResult.from, labelResult.to, labelResult.confidence));
            }
        }
    }

    private IEnumerator TransferRecording(string text)
    {
        while (conversation.isWaitingForResponse())
            yield return null;

        conversation.AskQuestion(text);
    }
}
