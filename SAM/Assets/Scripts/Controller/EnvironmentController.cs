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
    #endregion

    private AudioSource environmentAudioSource;
    private AudioClip initialClip;
	// Use this for initialization
	void Start ()
    {
        environmentAudioSource = GameObject.Find("/Environment").GetComponent<AudioSource>();
        initialClip = environmentAudioSource.clip;

    }
	
	// Update is called once per frame
	void Update ()
    {
		
	}

    public void Manage(MessageResponse response, Dictionary<string, object> contexts)
    {
        string intent = response.intents[0].intent;

        if (intent.Equals("MUSIC_PLAY_COMMANDS") || intent.Equals("MUSIC_NEXT_COMMANDS") || intent.Equals("MUSIC_STOP_COMMANDS"))
            MusicPlayer(contexts["music"].ToString());

        /*
        switch (intent)
        {
            case "MUSIC_PLAY_COMMANDS":
                MusicPlayer(contexts["music"].ToString());
                break;
            case "MUSIC_NEXT_COMMANDS":
                MusicPlayer(contexts["music"].ToString());
                break;

        }
        */
        
    }

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
