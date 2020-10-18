# CS472 Finger Motion

Drive a virtual character's motion based on your finger movement.

## How this project is setup

### Client

The virtual character simulator. This is a unity app. It communicates with the python brain to driver the virtual character.

### Brain

All python code for our ML would go here.

### Client to brain communication

Protocol buffers are a really fast way to communicate between processes. We can use this to communicate between C# code (Unity) and python code (Open CV, tensorflow, etc).

---

## Helpful resources
- [Open CV with unity using protocol buffers (python to unity communication)](https://www.raywenderlich.com/5475-introduction-to-using-opencv-with-unity)
- [Protocol buffers for C#](https://developers.google.com/protocol-buffers/docs/csharptutorial)
- [Protocol buffers for Python](https://developers.google.com/protocol-buffers/docs/pythontutorial)
- [Protocol buffer repo](https://github.com/protocolbuffers/protobuf)
- [Protocol buffer homepage](https://developers.google.com/protocol-buffers)
