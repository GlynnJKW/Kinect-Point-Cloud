using UnityEngine;
using System.Collections;
using System;
using CielaSpike;
using System.Threading;
using System.IO;



[RequireComponent(typeof(PointHandler))]
public class DisplayCloudFromVid : MonoBehaviour
{
    public Transform trans;
    private PointHandler handler;

    public string filePath = "";

    private int currFrame;
    public playback options = playback.ONCE;
    public bool playing = false;
    public int reverse = 1;

    
    private bool working;

    private int maxFrame = int.MaxValue;

    private byte[] cstream;
    private byte[] dstream;

    private const float TICKS_PER_SEC = 10000000;
    private const int TEX_WIDTH = 320;
    private const int TEX_HEIGHT = 240;

    long timeStamp;
    public float targetFrameRate = 30.0f;
    float minimumTimePerFrame;

    public enum playback
    {
        ONCE,
        LOOP,
        LOOPREVERSE
    }


    // Use this for initialization
    void Start()
    {
        currFrame = 1;
        timeStamp = DateTime.Now.Ticks;
        handler = GetComponent<PointHandler>();
        minimumTimePerFrame = (TICKS_PER_SEC / targetFrameRate); // max # of ticks per frame
    }

    // Update is called once per frame
    void Update()
    {
        minimumTimePerFrame = (TICKS_PER_SEC / targetFrameRate); // max # of ticks per frame

        if (currFrame < 1)
        {
            switch (options)
            {
                case playback.ONCE:
                    //currFrame = maxFrame;
                    break;
                case playback.LOOP:
                    currFrame = maxFrame;
                    break;
                case playback.LOOPREVERSE:
                    currFrame = 1;
                    reversePlayback();
                    break;
            }

        }
        else if(currFrame > maxFrame)
        {
            switch (options)
            {
                case playback.ONCE:
                    break;
                case playback.LOOP:
                    currFrame = 1;
                    break;
                case playback.LOOPREVERSE:
                    currFrame = maxFrame;
                    reversePlayback();
                    break;
            }
        }

        if (!working && DateTime.Now.Ticks >= timeStamp + minimumTimePerFrame && playing)
        {
            working = true;
            StartCoroutine(getStreams());
        }
    }

    IEnumerator getStreams()
    {
        yield return Ninja.JumpBack;

        int frameNum = currFrame;

        string path = Application.dataPath + "/Recordings/" + filePath;

        if (File.Exists(path + "Frame_" + currFrame + "_C.jpg") && File.Exists(path + "Frame_" + currFrame + "_D.png"))
        {
            cstream = File.ReadAllBytes(path + "Frame_" + frameNum + "_C.jpg");
            dstream = File.ReadAllBytes(path + "Frame_" + frameNum + "_D.png");
            handler.updateTextures(cstream, dstream);
            currFrame += reverse;
        }
        else
        {
            maxFrame = frameNum - 1;
        }

        timeStamp = DateTime.Now.Ticks;
        working = false;
    }

    public void reversePlayback()
    {
        reverse = -reverse;
    }

}
