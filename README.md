Fixing Unity's tendency to object coupling: The MessageBus
================================

All in all I'm happy enough working with Unity. I mean it has its problems and all, but at Nastycloud we've been able to solve most of them successfully and reached a point where we're super productive using it.

I only have one major thing that bugs me. Unity feels like the PHP of game development. Tons of people is doing things like caveman and creating awful code that doesn't scale, proudly showing it to other random script kiddies that don't fullt understand the implications of what they're doing (or copy-pasting).

This is in some part, Unity's fault. It allows you to do things that you shouldn't be doing, or not if you want to work in a team. One of those things, and one that bugs me a lot is its tendency to generate [coupled](http://en.wikipedia.org/wiki/Coupling_%28computer_programming%29) objects.

In a perfect world, every single module of your game should be reusable, classes should talk about themselves, and try not to talk about other classes, or at least not force behaviour on other classes. This is mainly because, when you have a big enough game, having a lot of references to components form here and there easily becomes a code nightmare that slows you down and complicates reading. And I'm all about readable and reusable code. I only tackle performance when performance is an issue, if not, I put readability before.

Unity forces coupling straight from their first tutorial: the player controller hides pickups when colliding with them and it’s also setting the text that the UI needs to show score. That's absurd. Each pickup should be in charge of hiding itself and UI shouln't be updated by the player controller, at least not directly. These are the kind of situations that bother me a lot.

In order to solve this, we've created a Messaging system that allows objects to broadcast messages when events happen and have other objects subscribe to messages and act accordingly when messages are received.

This is what we called the MessageBus, and in this series of articles I'm going to show you how to build an extensible MessageBus that will allow you to live happy without coupling. Forgetting once and for all of all those public GameObject members that Unity forces you to put all over the place.


Overview of MessageBus pattern
------------

The main goal of this pattern is to allow objects communicate among them but without forcing actions on each other. We'll achieve this by allowing objects to subscribe to messages and handle messages when they arrive. Each object will have the responsibility to act accordingly to each message it receives.

The MessageBus will be a singleton class that will be in charge of handling MessageSubscribers and broadcasting Messages to them.

Each MessageSubscriber will define to which messages it subscribes, and will also have a MessageHandler that will be poked whenever a Message arrives.

MessageHandlers will be in charge of defining what to do when a Message arrives. Usually you'll have one script that inherits from MessageHandler per object, overriding the OnMessage() method to do what you want.

Once you want to send a message, you will call MessageBus.SendMessage(message) with a proper instance of the Message class, and it will be broadcasted to all the MessageSubscribers that are listening to the type of that Message instance.

Message
------

The Message class is the first thing we'll work on. The goal for it is to communicate which kind of message we're sending and its parameters.
Message Type
We have two main ways of implementing the message type. One is using a string, such as "PlayerPosition" and the other is creating an enum type that lists all the possible messages.

If your game requires the ability to create message types at runtime (because you don't know what all the messages are, or because you give your player the ability to create message types for example), you'll need to use strings.

But beware, using strings is not really happy, mostly because a typo can bring you a few hours of clueless debugging.

Because of this and since we’ll be making use of a nifty feature of Unity, in this article we’ll use an enum type. We’ll create this enum with three message types LevelStart, LevelEnd and PlayerPosition.
```csharp
public enum MessageType
{
    NONE,
    LevelStart,
    LevelEnd,
    PlayerPosition
}
```
Now that we have our message types defined, we’ll use a struct to define our messages.

Parameters on messages could be implemented using a Dictionary if you want to pass a lot of data around, but to keep things easy in this article we’ll define just a few members with the most used data types we may want to pass around in our games: int, float, Vector3 and GameObject.
```csharp
public struct Message
{
    public MessageType Type;
    public int IntValue;
    public float FloatValue;
    public Vector3 Vector3Value;
    public GameObject GameObjectValue;
}
```
That should cover most of the uses for our messaging system. You can always change this to match your needs.

MessageSubscriber
----------
The message subscriber is just a simple struct too, it stores an array of MessateTypes it will subscribe to and a MessageHandler that will be called each time one of those messages arrives. This subscriber will be saved in the MessageBus and used each time a message is broadcasted.

```csharp
public struct MessageSubscriber
{
    public MessageType[] MessageTypes;
    public MessageHandler Handler;
}
```

MessageBus
----------
As previously stated, the message bus has two main responsibilities: subscribing and message broadcasting.

We’ll need to store subscribers in a Dictionary, indexed by MessageType in order to retrieve them easily.

For this on top of MessageBus.cs we need to add 
```csharp
using System.Collections.Generic;
```

It will allow us to use the polymorphic version of Dictionary that comes with C#.
As the value type for the Dictionary we’ll use C#’s polymorphic List, in order to store a List of MessageSubscribers (the class we’ll build next to define a subscriber).

```csharp
    Dictionary<MessageType, List<MessageSubscriber>> subscriberLists = 
        new Dictionary<MessageType, List<MessageSubscriber>>();
```

Now, we’ll have to create a method to allow us to register MessageSubscribers and assign them to each message.

```csharp
    public void AddSubscriber( MessageSubscriber subscriber )
    {
        MessageType[] messageTypes = subscriber.MessageTypes;
        for (int i = 0; i < messageTypes.Length; i++)
            AddSubscriberToMessage (messageTypes[i], subscriber);
    }
```

Here we’re iterating on each message type that comes defined in the subscriber, and adding it to the list indexed by each of those messages. Let’s write AddSubscriberToMessage now.

```csharp
    void AddSubscriberToMessage ( MessageType messageType, 
                                 MessageSubscriber subscriber)
    {
        if (!subscriberLists.ContainsKey (messageType))
            subscriberLists [messageType] = 
                new List<MessageSubscriber> ();

        subscriberLists [messageType].Add (subscriber);
    }
```

To avoid calling an uninitialized list, we first check if the list of MessageSubscribers was created, if not, we create it. After that, we just add the subscriber to the correct list. As easy as that.

The other responsibility for our MessageBus class is to broadcast messages, so let’s write the SendMessage method:
```csharp
    public void SendMessage (Message message)
    {
        if (!subscriberLists.ContainsKey (message.Type))
            return;

        List<MessageSubscriber> subscriberList = 
            subscriberLists [message.Type];

        for (int i = 0; i < subscriberList.Count; i++)
            SendMessageToSubscriber (message, subscriberList [i]);
    }
```
This method first looks if the message type actually has subscribers, if it has, it iterates through them and sends the message. Now let’s see how SendMessageToSubscriber works.

```csharp
    void SendMessageToSubscriber(Message message, 
                                 MessageSubscriber subscriber)
    {
        subscriber.Handler.HandleMessage (message);
    }
```

Simple enough, right? We’re calling the method HandleMessage on our subscriber’s MessageHandler. We’ll get HandleMessage in a moment, but first...
Singleton
In order to make it easy to interact with the MessageBus, we’re going to make it a Singleton. In this way we’ll only have one MessageBus to route our messages and it will be easily accessible from wherever we want in our code.

In order to achieve this we need to create a private static member that will hold the single instance of MessageBus, a static method that returns that instance and also make the constructor private.
```csharp
    static MessageBus instance;

    public static MessageBus Instance
    {
        get
        {
            if(instance == null)
                instance = new MessageBus();

            return instance;
        }
    }

    private MessageBus() {}
```

Explaining what a Signleton is and how it works exceeds the scope of this article, but let’s say that now whenever we want to interact with the MessageBus is just a matter of calling MessageBus.Instance.

MessageHandler
----------
Up until now, we’ve been creating raw C# classes, nothing that was strictly Unity related (well, except for using Vector3 and GameObject in the Message struct). But now we’re going to focus on bringing our MessageBus to Unity starting from our MessageHandler. The MessageHandler is going to inherit from MonoBehaviour, and you’ll see why in the next section.

Besides inheriting from MonoBehaviour, MessageHandler is going to be an abstract class. We’ll inherit from it to create our handlers and override the HandleMessage method.

```csharp
public abstract class MessageHandler : MonoBehaviour
{
    public abstract void HandleMessage( Message message );
}
```

MessageSubscriberController
---------
This script will be what glues everything inside Unity’s interface, and the reason why the MessageHandler needs to inherit from MonoBehaviour.

MessageSubscriberController will be attached to each GameObject that we want to turn into a message listener. We’re going to add two public members: MessageTypes and Handler, that we’ll be able to make use of UnityEditor to set.

```csharp
    public MessageType[] MessageTypes;
    public MessageHandler Handler;
```

And on Start we’ll create a MessageSubscriber and register it.

```csharp
    void Start()
    {
        MessageSubscriber subscriber = new MessageSubscriber ();
        subscriber.MessageTypes = MessageTypes;
        subscriber.Handler = Handler;

        MessageBus.Instance.AddSubscriber (subscriber);
    }
```

That’s it! We’re almost done. Now look how cool is this, since we defined MessageType as an enum type, Unity will create a dropdown with all our messages already there to select, and since the public member MessageTypes is an array of MessageType, unity will also create a nice interface to define them. Here you can see a screenshot:

Fixing Unity's bad pattern from first tutorial
--------------------
In order to see this pattern working, I created this GitHub repository with all the code I’ve been discussing in this article plus a fix for Unity’s first tutorial. What I did in that project was mostly to remove responsibilities from PlayerController that shouldn’t be there (hiding pickups, storing score and updating UI) and moving everything to a message based approach, also the camera was coupled to the player, and I fixed it using a PlayerPosition message.

I hope that, for your teammates sake, you like my changes and think twice before making a public GameObject member next time!
