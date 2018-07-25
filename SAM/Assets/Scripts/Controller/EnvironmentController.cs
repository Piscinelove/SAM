/**
* Rafael Peixoto 2018 - All Rights Reserved
* Virtual Reality with AI chatbot - VRAI Project
* 
* This is the controller that allows to manage the environment
* depending of the intent received form the chatbot.
*/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using IBM.Watson.DeveloperCloud.Services.Conversation.v1;

public class EnvironmentController : MonoBehaviour {

    #region PLEASE SET THESE VARIABLES IN THE INSPECTOR
    [Space(10)]
    [Tooltip("First song")]
    [SerializeField]
    private AudioClip firstSong;
    [Tooltip("Second song")]
    [SerializeField]
    private AudioClip secondSong;
    [Tooltip("Earth")]
    [SerializeField]
    private GameObject earth;
    [Tooltip("Flag")]
    [SerializeField]
    private GameObject flag;
    [Tooltip("Module")]
    [SerializeField]
    private GameObject module;
    #endregion

    private AudioSource environmentAudioSource;
    private AudioClip initialClip;

    private bool isEarthDiscovered = false;
    private TypeWriting earthTypeWriting;
    private Animator earthAnimator;

    private bool isFlagDiscovered = false;
    private TypeWriting flagTypeWriting;
    private Animator flagAnimator;

    private bool isModuleDiscovered = false;
    private TypeWriting moduleTypeWriting;
    private Animator moduleAnimator;


    // Use this for initialization
    void Start ()
    {

        environmentAudioSource = GameObject.Find("/Environment").GetComponent<AudioSource>();
        initialClip = environmentAudioSource.clip;

        earthTypeWriting = earth.GetComponentInChildren<TypeWriting>();
        earthAnimator = earth.GetComponentInChildren<Animator>();


        flagTypeWriting = flag.GetComponentInChildren<TypeWriting>();
        flagAnimator = flag.GetComponentInChildren<Animator>();

        moduleTypeWriting = module.GetComponentInChildren<TypeWriting>();
        moduleAnimator = module.GetComponentInChildren<Animator>();

    }

    // Update is called once per frame
    void Update ()
    {

    }

    /*
     *  Manage() method
     *  When a response is received from the chatbot
     *  Checks the intent of the response and manage the environment if needed
     */
    public void Manage(MessageResponse response, Dictionary<string, object> contexts)
    {
        // Test if the chatbot recognised an intent
        if (response.intents.Length > 0)
        {
            string intent = response.intents[0].intent;

            if (intent.Equals("MUSIC_PLAY_COMMANDS") || intent.Equals("MUSIC_NEXT_COMMANDS") || intent.Equals("MUSIC_STOP_COMMANDS"))
                MusicPlayer(contexts["music"].ToString());
            else if (intent.Equals("ENVIRONMENT_GET_INFORMATION"))
            {
                switch (contexts["target"].ToString())
                {
                    case "Earth":

                        if (!isEarthDiscovered && !earthTypeWriting.isStarted)
                        {
                            earthTypeWriting.StartCoroutine("TypeIn");
                            earthAnimator.SetTrigger("FadeIn");
                            isEarthDiscovered = true;
                            break;
                        }
                        break;
                    case "Flag":
                        if (!isFlagDiscovered && !flagTypeWriting.isStarted)
                        {
                            flagTypeWriting.StartCoroutine("TypeIn");
                            flagAnimator.SetTrigger("FadeIn");
                            isFlagDiscovered = true;
                            break;
                        }
                        break;
                    case "Module":
                        if (!isModuleDiscovered && !moduleTypeWriting.isStarted)
                        {
                            moduleTypeWriting.StartCoroutine("TypeIn");
                            moduleAnimator.SetTrigger("FadeIn");
                            isModuleDiscovered = true;
                            break;
                        }
                        break;
                }
            }
        }
    }

    /*
     *  MusicPlayer() method
     *  Depending of the context sent by the chatbot
     *  Change the music
     */
    private void MusicPlayer(string context)
    {
        
        switch (context)
        {
            case "Bowie":
                environmentAudioSource.clip = firstSong;
                environmentAudioSource.Play();
                break;
            case "Floyd":
                environmentAudioSource.clip = secondSong;
                environmentAudioSource.Play();
                break;
            case "None":
                environmentAudioSource.clip = initialClip;
                environmentAudioSource.Play();
                break;
        }
       
    }
}
