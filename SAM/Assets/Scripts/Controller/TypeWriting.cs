using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(AudioSource))]
[RequireComponent(typeof(Text))]
public class TypeWriting : MonoBehaviour {

    public string message = "Replace";
    public float startDelay = 2f;
    public float typeDelay = 0.01f;
    public AudioClip typeSound;
    public bool isStarted = false;

    private Text textComponent;


	// Use this for initialization
	void Start ()
    {

	}
	
	// Update is called once per frame
	void Update () {
		
	}


    void Awake()
    {
        textComponent = GetComponent<Text>();
    }

    public IEnumerator TypeIn()
    {
        isStarted = true;
        yield return new WaitForSeconds(startDelay);

        for(int i = 0; i < message.Length; i++)
        {
            textComponent.text = message.Substring(0, i);
            GetComponent<AudioSource>().PlayOneShot(typeSound);
            yield return new WaitForSeconds(typeDelay);
        }
    }

}
