/**
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
    private string _serviceUrl;
    [Tooltip("Text field to display the results of streaming.")]
    public Text ResultsField;
    [Header("CF Authentication")]
    [Tooltip("The authentication username.")]
    [SerializeField]
    private string _username;
    [Tooltip("The authentication password.")]
    [SerializeField]
    private string _password;
    [Header("IAM Authentication")]
    [Tooltip("The IAM apikey.")]
    [SerializeField]
    private string _iamApikey;
    [Tooltip("The IAM url used to authenticate the apikey (optional). This defaults to \"https://iam.bluemix.net/identity/token\".")]
    [SerializeField]
    private string _iamUrl;
    [Header("Language Configuration")]
    [Tooltip("The user language")]
    [SerializeField]
    private string _language;
    #endregion


    private int _recordingRoutine = 0;
    private string _microphoneID = null;
    private AudioClip _recording = null;
    private int _recordingBufferSize = 1;
    private int _recordingHZ = 16000;

    private static float refValue = 0.1f; // RMS value for 0 dB //private static float sensitivity = 10.0f; // Sensitivity multiplier for average loudness calculation
    private static int chunkSize = 800; // 20171031 RMPickering - Adjusting chunkSize to 800 to correspond to a more even size given 16000 Hz sample rate. Each chunk will represent 1/20th of a second of audio.

    private string _runningprocess = "AIVoiceController";

    private SpeechToText _speechToText;

    void Start()
    {
        LogSystem.InstallDefaultReactors();
        Runnable.Run(CreateService());
    }

    private IEnumerator CreateService()
    {
        //  Create credential and instantiate service
        Credentials credentials = null;
        if (!string.IsNullOrEmpty(_username) && !string.IsNullOrEmpty(_password))
        {
            //  Authenticate using username and password
            credentials = new Credentials(_username, _password, _serviceUrl);
        }
        else if (!string.IsNullOrEmpty(_iamApikey))
        {
            //  Authenticate using iamApikey
            TokenOptions tokenOptions = new TokenOptions()
            {
                IamApiKey = _iamApikey,
                IamUrl = _iamUrl
            };

            credentials = new Credentials(tokenOptions, _serviceUrl);

            //  Wait for tokendata
            while (!credentials.HasIamTokenData())
                yield return null;
        }
        else
        {
            throw new WatsonException("Please provide either username and password or IAM apikey to authenticate the service.");
        }

        _speechToText = new SpeechToText(credentials);
        _speechToText.StreamMultipart = true;

        Active = true;
        StartRecording();
    }

    public bool Active
    {
        get { return _speechToText.IsListening; }
        set
        {
            if (value && !_speechToText.IsListening)
            {
                _speechToText.RecognizeModel = _language;
                //_service.SmartFormatting = true;
                _speechToText.DetectSilence = true;
                _speechToText.EnableWordConfidence = true;
                _speechToText.EnableTimestamps = true;
                _speechToText.SilenceThreshold = 0.00f;
                _speechToText.MaxAlternatives = 0;
                _speechToText.EnableInterimResults = true;
                _speechToText.OnError = OnError;
                _speechToText.InactivityTimeout = -1;
                _speechToText.ProfanityFilter = false;
                _speechToText.SmartFormatting = true;
                _speechToText.SpeakerLabels = false;
                _speechToText.WordAlternativesThreshold = null;
                _speechToText.StartListening(OnRecognize, OnRecognizeSpeaker);
            }
            else if (!value && _speechToText.IsListening)
            {
                _speechToText.StopListening();
            }
        }
    }

    private void StartRecording()
    {
        if (_recordingRoutine == 0)
        {
            UnityObjectUtil.StartDestroyQueue();
            _recordingRoutine = Runnable.Run(RecordingHandler());
        }
    }

    private void StopRecording()
    {
        if (_recordingRoutine != 0)
        {
            Microphone.End(_microphoneID);
            Runnable.Stop(_recordingRoutine);
            _recordingRoutine = 0;
        }
    }

    private void OnError(string error)
    {
        Active = false;

        Log.Debug("ExampleStreaming.OnError()", "Error! {0}", error);
    }

    private IEnumerator RecordingHandler()
    {
        /* 20171019 RMPickering - The "RecordingHandler" has been upgraded to better pseudo-stream the audio captured from the microphone in chunks of chunkSize samples at a time.
        * Initially will try 1200 chunks as chunkSize, which means at sample rate of 48 kHz there will be 40 chunks per second of audio per channel.
        * NOTE: This implementation relies on the microphone recording sample rate, _recordingHZ, being an even integer multiple of chunkSize. Also, the audio ring buffer size is just one second.
        * If for any reason capture latency nears or exceeds one second, buffer overflow or lost audio chunks will result. Unity framerates normally range up close to 100 fps in my testing.
        * In case latency is found to be a problem it might be an option to increase audio ring buffer size.
        */
        // 20171018 RMPickering - Need to keep track of how far we've already written, which of course starts at zero
        int ChunkEndSpot = 0;
        // private variables to check sample rate capabilities of mic and set pseudo-streaming chunk size
        int minSampleRate, maxSampleRate, readPos, downSampleFactor;
        readPos = 0;
        downSampleFactor = 1; // 20171026 RMPickering - Adding downsampling with initial target to downsample from 48 kHz, stereo to 48 kHz, mono. This means taking only every 1 in 2 samples.
                              // NOTE: It is critical in correctly computing downSampleFactor to know the number of channels and sample rate of the default or selected microphone. On Windows, this
                              // is an o/s level setting. It is being over-ridden by setting _recordingHZ to our desired value. Hopefully, the actual sample rate is changing like magic, too!
                              // 20171031 RMPickering - Updating to 1 to not downsample, assuming _recordingHZ is magically setting sample rate to required value.

        // 20171019 RMPickering - A better way to compute "MaxLevel" using either average loudness, db or RMS value for sound level:
        // float rmsValue, dbValue, Loudness;   // sound level - RMS, db, Loudness (each of which is calculated per chunk of samples)
        float rmsValue, dbValue;   // 20171103 RMPickering - Removed calculation of Loudness as it seems redundant with rms & db.

        // 20171018 RMPickering - We'll use the float arrays samplesChunk and downSamplesChunk for processing each chunk of audio samples
        float[] samplesChunk = null;
        //float[] downSamplesChunk = null;

        Log.Debug("{0}", "devices: {1}", _runningprocess, Microphone.devices);

        //Microphone.GetDeviceCaps(null, out minSampleRate, out maxSampleRate);
        //Log.Debug("{0}", " Selected mic minimum sample rate: {1}, maximum sample rate: {2}", _runningprocess, minSampleRate.ToString(), maxSampleRate.ToString());
        // 20171101 RMPickering - This is not documented in Unity docs at all, but "GetDeviceCaps" seems to be completely meaningless. Unity over-rides the mic sample rate and sets our specified rate regardless of the 'capabilies' as reported by the device!
        // 20171018 RMPickering - For greater clarity, using name "AudioCaptureBuffer" for the name of our private audioclip.
        // To be clear, this is being initialized using selected _microphoneID, looping == true, recording clip size defaults to 1 seconds, and using specified sample rate.
        // NOTE: Given the provided defaults, our AudioClip will contain 16,000 samples (being comprised of 1 second of audio at 16,000 samples per second).
        // 20171018 RMPickering - Just one more confounding factor: it seems that my microphone is determined to record in stereo, with discrete L & R channels!
        // 20171101 RMPickering - Again, it's not documented but Unity captures only 1 channel from my mic. No idea how we'd capture both available channels but this isn't needed anyway.
        _recording = Microphone.Start(_microphoneID, true, _recordingBufferSize, _recordingHZ);

        Log.Debug("{0}", " Microphone Ring Buffer includes: {1} channels with a total of: {2} samples.", _runningprocess, _recording.channels.ToString(), _recording.samples.ToString());
        // 20171101 RMPickering - also log # channels! It looks like Unity is always capturing only 1 channel from mic but we need to know if this changes.


        // 20171018 RMPickering - According to the Unity documentation, Microphone.Start returns null only on failure, so...
        if (_recording == null)
        {
            StopRecording();
            yield break;
        }

        ChunkEndSpot = chunkSize * downSampleFactor - 1; // 20171024 RMPickering - Initially, the end of the first chunk is size of a 'chunk' * downsamplefactor - 1 (because first sample is at position zero)

        // 20171018 RMPickering - At this point we should be capturing!
        yield return new WaitForSecondsRealtime(chunkSize * downSampleFactor / _recordingHZ);      // let _recordingRoutine get set, calculate how long to wait for at least one audioChunk to be ready

        // 20171018 RMPickering - We are at this point INSIDE the '_recordingRoutine' so I would probably disappear myself if somehow _recordingRoutine were 0. Similarly for AudioCaptureBuffer!
        while (_recordingRoutine != 0 && _recording != null)
        {
            // 20171018 RMPickering - This gets the current write position of the microphone in the active AudioCaptureBuffer (which is an AudioClip).
            // The write position is being maintained by Unity's microphone routines. It may (likely will) change by some amount after each yield return of this coroutine.
            int writePos = Microphone.GetPosition(_microphoneID);
            // 20171018 RMPickering - Not sure why mic would stop recording but...
            if (!Microphone.IsRecording(_microphoneID))
            {
                Log.Error("MicrophoneWidget", "Microphone disconnected.");

                StopRecording();
                yield break;
            }

            // 20171018 RMPickering - Check to make sure that we waited until at least chunkSize samples have been written to AuidoCaptureBuffer been written before reading from it, or writer is toward end of Buffer.
            while (writePos > readPos + ChunkEndSpot || writePos < readPos) // Just in case there is more than one chunk waiting, keep processing; doesn't matter if writing is heading for end of ring buffer or restarted at beginning again!
            {
                // at least one chunk is recorded, make a RecordClip and pass it onto our callback.
                samplesChunk = new float[chunkSize * downSampleFactor]; // 20171025 RMPickering - This is a 'BIG' chunk of samples -- many will be discarded due to downsampling!
                                                                        // downSamplesChunk = new float[chunkSize];  // 20171024 RMPickering - Make a smaller audiochunk for downsampling. Only 1 channel!!
                                                                        // 20171018 RMPickering - This is filling the 'samplesChunk' array with data from our AudioBuffer. We're copying from part of the AudioCaptureBuffer that isn't being written now, I hope! 
                _recording.GetData(samplesChunk, readPos);

                // 20171018 RMPickering - Not sure what an AudioData type means. This is not documented by Unity so may be an IBM SDK thing...                
                AudioData record = new AudioData();
                // 20171018 RMPickering - The next statement seems to be setting the MaxLevel to the highest value from the samples, not taking into account the negative values.
                // record.MaxLevel = Mathf.Max(samples);

                int i, j = 0;
                float sumSqSamples = 0;
                float sumAbsSamples = 0;

                // RMPickering - implement an anti-aliasing filter -- low pass with cutoff above 6500 kHz
                /* 20171101 RMPickering - Have now tested this with Watson STT using various values for CUTOFF to determine how well it works. Unfortunately, it's hard to say! I had to set it down somewhere below
                 * 1 kHz before there was a significant effect on the STT transcript results. I'm assuming this is because in my test environment and system, there's no high frequency noise that would
                 * reduce the accuracy of the Watson STT transcription. For now, it seems prudent to keep the LPF in the loop and set CUTOFF to 6.5 kHz just to be safe.
                 */

                float CUTOFF = 6500.0f; // RMPickering - We need something less than 8000 Hz
                float RC = 1.0f / (CUTOFF * 2.0f * 3.14f);
                float dt = 1.0f / 16000.0f;  // RMPickering - Using the original sampling rate, which is 16000 Hz
                float alpha = dt / (RC + dt);

                // 20171103 RMPickering - Consolidating code to calculate rms, db, loudness while applying LPF. Since LPF starts processing at sample i = 1, need to pre-compute zeroth sample.
                sumSqSamples += samplesChunk[0] * samplesChunk[0]; // sum squared samples
                sumAbsSamples += Mathf.Abs(samplesChunk[0]);

                // RMPickering - This section covers startup of the low pass filter -- in this case, we have no previous sample to use in computing lpf.
                // RMPickering - First sample will be unchanged.
                for (i = 1; i < chunkSize * downSampleFactor; i++) // RMPickering - leave the first sample (number 0) unchanged, then compute lpf for each sample. 
                {
                    // RMPickering - low pass filter is essentially 'smoothing' the audio above the cutoff frequency. This is computed using the previously calculated sample, calculated alpha and the next sample.
                    samplesChunk[i] = samplesChunk[i - 1] + alpha * (samplesChunk[i] - samplesChunk[i - 1]);
                    sumSqSamples += samplesChunk[i] * samplesChunk[i]; // sum squared samples
                    sumAbsSamples += Mathf.Abs(samplesChunk[i]);
                }

                // 20171101 RMPickering - We're calculating a LPF filter just above, then we do a lot of the same thing to calculate a downsample here. Not sure we'll need either of these functions now!
                // If it turns out that we do need both they'll need to be consolidated, otherwise just remove both!
                //for (i = 0; i < chunkSize; i++)
                //{
                //    downSamplesChunk[i] = samplesChunk[j];
                //    sumSqSamples += downSamplesChunk[i] * downSamplesChunk[i]; // sum squared samples
                //    sumAbsSamples += Mathf.Abs(downSamplesChunk[i]);
                //    j = j + downSampleFactor;   // skip over the samples not in downSample set
                //}
                rmsValue = Mathf.Sqrt(sumSqSamples / chunkSize); // rms = square root of average
                dbValue = 20 * Mathf.Log10(rmsValue / refValue); // calculate dB
                if (dbValue < -160) dbValue = -160; // clamp it to -160dB min
                                                    //Loudness = sumAbsSamples / chunkSize * sensitivity; // And the Loudness is actually the combined or mean loudness across the audio samples
                                                    //audioLevel = Loudness;

                // 20171018 RMPickering - Now we need to set MaxLevel for Clip, using whichever calculated value makes the most sense. Let's try Loudness for now.

                /* 20171019 RMPickering - NOTE: At this point, if Loudness is below a certain threshold we might consider the entire audioChunk as silence.
                 * In this case, it might not need to be sent to Watson STT at all, or only if we don't have more than n consecutive silent audioChunks.
                 * I would seriously consider n = 8 as a reasonable value, as this would represent a half a second of silence.
                 * However, it is probably better to catch this in the Watson STT listener itself, and have it automatically stop and then restart a Recognize session.
                 */
                record.MaxLevel = rmsValue;
                //Log.Debug("Microphone Widget", "Calculated Loudness for sample chunk is {0}", Loudness.ToString());

                // 20171018 RMPickering - Now we're setting the 'Clip' part of AudioData 'record' named "audioChunk" to contain the downscaled samples, not streamed.
                record.Clip = AudioClip.Create("audioChunk", chunkSize, 1, _recordingHZ, false);

                // 20171018 RMPickering - This copies the audio samples from the array 'samplesChunk' into the Clip part of "record", starting at the start of the Clip. This should fill it completely.
                record.Clip.SetData(samplesChunk, 0);

                // 20171018 RMPickering - And here we send the clip with "audioChunk" audiodata to the Watson STT listener.
                _speechToText.OnListen(record);

                // 20171018 RMPickering - Remember which block we just copied!
                readPos += chunkSize * downSampleFactor;
                if (readPos > _recordingHZ * _recording.channels - 1)
                { // 20171025 RMPickering - The size of the ring buffer is the sample rate * # channels * num seconds (1) and it starts at zero!
                  // We just processed the audiochunk at the end of the buffer. Reset readPos and last chunk end pointers so we can start at beginning of buffer again!
                    readPos = 0;
                    ChunkEndSpot = chunkSize * downSampleFactor - 1;
                }
                else ChunkEndSpot += chunkSize * downSampleFactor;
            }

            // 20171019 RMPickering - if we reach this point, we need to wait for next Update, recheck writePos, and continue pseudo-streaming once the Microphone gets far enough along again.
            // We could calculate a specific wait time but given that Unity fps maxes out around 100, worst case is we may have to wait again for about ten consecutive cycles.
            // I'm assuming that a better balance is to try to follow Unity's microphone recording position more closely so as to minimize audio streaming latency.
            // 20171024 RMPickering - We're now using a somewhat bigger chunkSize. Let's calculate the wait time.
            yield return new WaitForSecondsRealtime(chunkSize * downSampleFactor / _recordingHZ);

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
                    //string text = string.Format("{0} ({1}, {2:0.00})\n", alt.transcript, res.final ? "Final" : "Interim", alt.confidence);
                    //Log.Debug("ExampleStreaming.OnRecognize()", text);
                    //ResultsField.text = text;
                    string text = alt.transcript;
                    Log.Debug("{0}", string.Format("{1} ({2}, {3:0.00})\n", _runningprocess, text, res.final ? "Final" : "Interim", alt.confidence));
                    ResultsField.text = text;
                }

                /*if (res.keywords_result != null && res.keywords_result.keyword != null)
                {
                    foreach (var keyword in res.keywords_result.keyword)
                    {
                        Log.Debug("ExampleStreaming.OnRecognize()", "keyword: {0}, confidence: {1}, start time: {2}, end time: {3}", keyword.normalized_text, keyword.confidence, keyword.start_time, keyword.end_time);
                    }
                }*/

                /*if (res.word_alternatives != null)
                {
                    foreach (var wordAlternative in res.word_alternatives)
                    {
                        Log.Debug("ExampleStreaming.OnRecognize()", "Word alternatives found. Start time: {0} | EndTime: {1}", wordAlternative.start_time, wordAlternative.end_time);
                        foreach (var alternative in wordAlternative.alternatives)
                            Log.Debug("ExampleStreaming.OnRecognize()", "\t word: {0} | confidence: {1}", alternative.word, alternative.confidence);
                    }
                }*/
                if (res.keywords_result != null && res.keywords_result.keyword != null)
                {
                    foreach (var keyword in res.keywords_result.keyword)
                    {
                        Log.Debug("{0}", "keyword: {1}, confidence: {2}, start time: {3}, end time: {4}", _runningprocess, keyword.normalized_text, keyword.confidence, keyword.start_time, keyword.end_time);
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
}
