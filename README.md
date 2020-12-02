# CS472 Finger Motion

Drive a virtual character's motion based on your finger movement.

## TODO

- With walking data, make some amount of time for just standing
- Slow down the walking animation, too fast for useful features, OR speed up the polling rate of the unity client

## How to train your dragon

1. Start up the brain

```
python main.py
```

2. Start the training interface in the client

```
Window > App > Training Interface

~Enter all the fields and set the dataframe asset~

Press gather data and match up your hand motion to what appears in the animation
```

3. Open up your persistent data path were it saved the file. For more info on where your persistent data path is, check out the [unity docs](https://docs.unity3d.com/ScriptReference/Application-persistentDataPath.html).

4. From there you can modify the `run_*.py` files to run your dataset.

## How this project is setup

The project uses a client and a brain (server). The client uses the brain to predict output.

**Client**

The virtual character simulator, built in Unity. It communicates with the python brain to drive the virtual character.

**Brain**

All machine learning code. This package uses tensorflow to compute outputs.

**Client to brain communication**

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
nc -u 127.0.0.1 5065 < some_file.txt
```

**2. Conda**

Create the conda environment by navigating to the brain folder and downloading the dependencies from the `environment.yml` file.

```
cd brain
conda env create -f environment.yml
```

Activate conda environment

```
conda activate tf-cpu
```