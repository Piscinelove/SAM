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

## PREREQUISITE
> **DEVELOPMENT ENVIRONMENT** :
1. Unity3D 5 Personal v2018.2.0f2
2. VisualStudio Community v7.4.3 (RUNTIME .NET 4.x Equivalent)
3. Java JDK v1.8.0_171
4. Android NDK vR13b
5. Android SDK v8.1 (Oreo)
### SUPPORTED HEADSETS
> **INFO** : The project currently only supports HTC Vive and Oculus GO.
### STRUCTURE
```
  . 
  ├── README.md               
  ├── SAM 
  │   └── Assets 
  │       └── Animation 
  │       └── Audio 
  │       └── Materials 
  │       └── Models 
  │       └── Moon 
  │       └── Oculus         #The folder contains the Oculus Utilities SDK to support Oculus GO. 
  │       └── SteamVR        #The folder contains the SteamVR SDK to support HTC Vive.
  │       └── VRTK           #The folder contains the VRTK SDK to support multiplatform.
  │       └── Watson         #The folder contains the IBM Watson Unity SDK to support all IBM Watson services
  │       └── Scripts        #The folder contains all scripts created for the application
  │       └── Settings 
  │       └── Scenes 
  │       └── Skybox 
  │       └── SolarSystem 
  │       └── Textures 
  └── .gitignore
```
> **SDK** : [Oculus Utilities v1.27.0](https://developer.oculus.com/downloads/package/oculus-utilities-for-unity-5/) `Oculus`, [SteamVR v1.2.3](https://assetstore.unity.com/packages/templates/systems/steamvr-plugin-32647) `SteamVR`, [VRTK v3.3.0-alpha](https://github.com/thestonefox/VRTK) `VRTK`, [WATSON v2.5.0](https://github.com/watson-developer-cloud/unity-sdk) `Watson`
> **ASSETS** : [Moon Environment v2.0](https://assetstore.unity.com/packages/3d/environments/moon-environment-40424) `Moon`,[Neutron Solar System Pack v2.2](https://assetstore.unity.com/packages/3d/environments/sci-fi/neutron-solar-system-pack-20959) `SolarSystem`,[Cloth animation-based flag v1.0](https://assetstore.unity.com/packages/3d/props/exterior/cloth-animation-based-flag-65842) `Models`,[Apollo Lunar Module VR / AR / low-poly 3D model](https://www.cgtrader.com/3d-models/space/spaceship/apollo-lunar-module-b19344473680a608f6216314a9df63a2) `Models`,

### INSTALLATION
```
git clone https://github.com/Piscinelove/SAM
```
### CONFIGURATION
> **INFO** : We do not provide our credentials and IBM Watson Conversation service workspace on this GitHub platform. The latter was hand-delivered to the Bachelor's follow-up teacher.
1. Log in to IBM Cloud at https://console.bluemix.net.
2. Select the services you want to use.
3. Select Service credentials.
4. Select View credentials to access your credentials.
5. Save it.
6. Enter credentials in the scripts inspector using the services.

### SCREENSHOTS
![function1](https://user-images.githubusercontent.com/19705441/43369229-ae725358-936a-11e8-8f9d-7967a09b86aa.png)
![function3](https://user-images.githubusercontent.com/19705441/43369230-b2fdd398-936a-11e8-9a86-7979c4d7f54d.png)
![function4](https://user-images.githubusercontent.com/19705441/43369232-ba0a260a-936a-11e8-8ac9-67e5702dc93d.png)
