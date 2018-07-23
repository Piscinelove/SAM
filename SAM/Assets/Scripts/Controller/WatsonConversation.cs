/**
* Rafael Peixoto 2018 - All Rights Reserved
* Virtual Reality with AI chatbot - VRAI Project
* 
* This is the controller of the chatbot for interact directly SAM AI Assistant Chatbot.
* The following class send the message recorded of the current user to the chatbot configured by using
* the IBM's Watson Conversation Service. 
* When the output is received, it transmits it to the IBM's Watson Text To Speech Service
*/

using UnityEngine;
using IBM.Watson.DeveloperCloud.Services.Conversation.v1;
using IBM.Watson.DeveloperCloud.Utilities;
using IBM.Watson.DeveloperCloud.Logging;
using System.Collections;
using FullSerializer;
using System.Collections.Generic;
using IBM.Watson.DeveloperCloud.Connection;
using UnityEngine.UI;

public class WatsonConversation : MonoBehaviour
{
    #region PLEASE SET THESE VARIABLES IN THE INSPECTOR
    [Space(10)]
    [Tooltip("The service URL (optional). This defaults to \"https://gateway.watsonplatform.net/conversation/api\"")]
    [SerializeField]
    private string serviceURL;
    [Tooltip("The workspaceId to run the example.")]
    [SerializeField]
    private string workspaceID;
    [Tooltip("The version date with which you would like to use the service in the form YYYY-MM-DD.")]
    [SerializeField]
    private string versionDate;
    [Header("Cloud Service Authentication")]
    [Tooltip("The authentication username.")]
    [SerializeField]
    private string username;
    [Tooltip("The authentication password.")]
    [SerializeField]
    private string password;
    [Header("Text to speech script")]
    [Tooltip("The script of the Text To Speech Service")]
    [SerializeField]
    private Speak textToSpeech;
    [Header("Environment controller script")]
    [Tooltip("The script of environment controls")]
    [SerializeField]
    private EnvironmentController environment;
    #endregion

    private Conversation conversation;

    private fsSerializer serializer = new fsSerializer();
    private Dictionary<string, object> contexts = null;
    private bool waitingForResponse = false;

    void Start()
    {
        LogSystem.InstallDefaultReactors();
        contexts = new Dictionary<string, object>();
        CreateService();
    }

    /*
     *  CreateService() method
     *  Create the Speech to Text service using credentials submited
     */
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

        conversation = new Conversation(credentials);
        conversation.VersionDate = versionDate;
    }

    /*
     *  AskQuestion() method
     *  Ask the question received from the Speech to Text Service to the Conversation Service
     *  Try to find the active camera's target and transmit it with the message to teh chatbot
     */
    public void AskQuestion(string message)
    {
        // The application is waiting for the response from the chatbot
        waitingForResponse = true;
        
        // Find the active camera's target
        if(GameObject.Find("Camera (eye)") != null)
            contexts["target"] = GameObject.Find("Camera (eye)").GetComponent<TargetObject>().target;
        else if(GameObject.Find("CenterEyeAnchor") != null)
            contexts["target"] = GameObject.Find("CenterEyeAnchor").GetComponent<TargetObject>().target;
        else if (GameObject.Find("Main Camera") != null)
            contexts["target"] = GameObject.Find("Main Camera").GetComponent<TargetObject>().target;

        //  Build the message request
        MessageRequest messageRequest = new MessageRequest()
        {
            input = new Dictionary<string, object>()
            {
                { "text", message }
            },
            // Adds the target context if there is one
            context = contexts
        };

        // If the send method fails
        if (!conversation.Message(OnMessage, OnFail, workspaceID, messageRequest))
            Log.Debug("ExampleConversation.AskQuestion()", "Failed to message!");


    }

    /*
     *  OnMessage() callback method
     *  The method is called when a response from the chatbot is received
     */
    private void OnMessage(object resp, Dictionary<string, object> customData)
    {
        Log.Debug("ExampleConversation.OnMessage()", "Conversation: Message Response: {0}", customData["json"].ToString());

        //  Convert resp to fsdata
        fsData fsdata = null;
        fsResult r = serializer.TrySerialize(resp.GetType(), resp, out fsdata);
        if (!r.Succeeded)
            throw new WatsonException(r.FormattedMessages);

        //  Convert fsdata to MessageResponse
        MessageResponse messageResponse = new MessageResponse();
        object obj = messageResponse;
        r = serializer.TryDeserialize(fsdata, obj.GetType(), ref obj);
        if (!r.Succeeded)
            throw new WatsonException(r.FormattedMessages);

        //  Set context for next round of messaging
        object _tempContext = null;
        (resp as Dictionary<string, object>).TryGetValue("context", out _tempContext);

        if (_tempContext != null)
            contexts = _tempContext as Dictionary<string, object>;
        else
            Log.Debug("ExampleConversation.OnMessage()", "Failed to get context");

        //  Synthesize the message response
        textToSpeech.Synthesize(messageResponse.output.text[0]);
        //  Send the message response to the EnvironmentManager script
        environment.Manage(messageResponse, contexts);
        //  The chatbot has responded
        waitingForResponse = false;
    }

    /*
     *  OnFail() callback method
     *  The method is called when there is a problem with connexion to the chatbot
     */
    private void OnFail(RESTConnector.Error error, Dictionary<string, object> customData)
    {
        Log.Error("ExampleConversation.OnFail()", "Error received: {0}", error.ToString());
    }

    /*
     *  isWaitingForResponse() callback method
     *  Public method for the SceneStreaming class
     *  Returns the current state of the service
     */
    public bool isWaitingForResponse()
    {
        return waitingForResponse;
    }
}
