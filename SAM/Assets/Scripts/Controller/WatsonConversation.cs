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
    #endregion

    private Conversation conversation;

    private fsSerializer serializer = new fsSerializer();
    private Dictionary<string, object> contexts = null;
    private bool waitingForResponse = false;

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

        conversation = new Conversation(credentials);
        conversation.VersionDate = versionDate;
    }

    public void AskQuestion(string message)
    {
        waitingForResponse = true;
        MessageRequest messageRequest = new MessageRequest()
        {
            input = new Dictionary<string, object>()
            {
                { "text", message }
            },
            context = contexts
        };

        if (!conversation.Message(OnMessage, OnFail, workspaceID, messageRequest))
            Log.Debug("ExampleConversation.AskQuestion()", "Failed to message!");
    }

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

        waitingForResponse = false;
        textToSpeech.Synthesize(messageResponse.output.text[0]);
    }

    private void OnFail(RESTConnector.Error error, Dictionary<string, object> customData)
    {
        Log.Error("ExampleConversation.OnFail()", "Error received: {0}", error.ToString());
    }

    public bool isWaitingForResponse()
    {
        return waitingForResponse;
    }
}
