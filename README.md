# VRAI PROJECT - VIRTUAL REALITY WITH AI CHATBOT
![presentation](https://user-images.githubusercontent.com/19705441/43368825-717ed75c-9363-11e8-9363-561281a45f04.png)

## DESCRIPTION
Despite a timid and difficult start, the virtual reality sector has managed to establish itself on the world market in recent years with the arrival of a multitude of headsets offering VR experiences to users.

At the same time, the market for intelligent voice assistants is also enjoying great success and is expanding rapidly, displaying artificial intelligences capable of understanding natural language and responding to the requests of different users.

In order to analyze the potential of combining these two successful technologies within a single VR application, we developed a solution that integrates the different features of a voice assistant into a virtual reality application using the IBM Watson service as part of our Bachelor's thesis.

The final application offers the user a virtual reality experience allowing him to communicate directly with artificial intelligence. In short, the user is able to have a natural discussion with a conversational agent and interact with the objects around him through the latter

## ARCHITECTURE
![architecture](https://user-images.githubusercontent.com/19705441/43368859-e0d603d2-9363-11e8-96fe-377b4d6a0853.png)

1. The audio stream from the microphone is recorded and stored by the application. 
2. A temporary audio file is then created and sent to the IBM Watson Speech to Text service. The latter performs voice recognition on the audio file and returns the text transcription to the application. 
3. The application sends the text message from the IBM Watson Speech to Text service to the IBM Watson Conversation service. The application adds the user's current context to the message to determine what the user is currently viewing. The conversational agent executes the natural language understanding of the input and returns a response to the application.
4. The conversational agent's response is then returned to the IBM Watson Text to Speech service to synthesize a voice message. The application reads the voice message as soon as it is received
