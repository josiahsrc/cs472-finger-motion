# CS472 Finger Motion

Drive a virtual character's motion based on your finger movement.

## How this project is setup

The project uses a client and a brain (server). The client uses the brain to predict output.

### Client

The virtual character simulator, built in Unity. It communicates with the python brain to drive the virtual character.

### Brain

All machine learning code. This package uses tensorflow to compute outputs.

### Client to brain communication

The client and brain each have two additional running threads
- A response thread, which receives incoming packets
- A request thread, which sends outgoing packets

Both the client and the brain run on their respective ports. The client will send and receive info from the brain via UDP.

## Helpful commands

**1. Netcat**

This command can be used to debug connections between the client and the server. 

Setup a listening server over UDP on your localhost's (127.0.0.1) 8080 port:

```
nc -u -lk 127.0.0.1 8080
```

Send a message to a server over UDP to your localhost's (127.0.0.1) 5065 port:

```

```