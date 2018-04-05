using UnityEngine;
using System.Collections;
using System;
using CielaSpike;
using System.Threading;


[RequireComponent(typeof(PointHandler))]
public class DisplayDepthAndAdjustedColorPC : MonoBehaviour
{

    public DepthWrapper dw;

    private PointHandler handler;

    public DeviceOrEmulator devOrEmu;
    private Kinect.KinectInterface kinect;

    public bool color;

    private bool working;

    private bool depth;

    private Color32[] cstream;
    private Color32[] dstream;

    public float nearClippingInMeters;
    public float farClippingInMeters;

    private const float TICKS_PER_SEC = 10000000;
    private const int TEX_WIDTH = 320;
    private const int TEX_HEIGHT = 240;

    long timeStamp; // This will keep track of how long an operation takes.
    public float targetFrameRate = 45.0f; // Your intended framerate. Details below script.
    float maximumTimePerFrame; // Here, you'll enforce a maximum delay between frames.


    // Use this for initialization
    void Start()
    {
        handler = GetComponent<PointHandler>();
        maximumTimePerFrame = (1.0f / targetFrameRate); // max # of secs per frame

        kinect = devOrEmu.getKinect();
        working = true;
        StartCoroutine(getStreams());
    }

    // Update is called once per frame
    void Update()
    {
        maximumTimePerFrame = (TICKS_PER_SEC / targetFrameRate); // max # of ticks per frame
        if (!working)
        {
            working = true;
            StartCoroutine(getStreams());
        }
    }

    //Asynchronously computes depth and color streams and applies them to the handler's textures
    IEnumerator getStreams()
    {
        timeStamp = DateTime.Now.Ticks;
        yield return Ninja.JumpBack;

        if (kinect.pollColor() && color)
        {
            cstream = mipmapImg(kinect.getColor(), 640, 480);
        }
        if (dw.pollDepth())
        {
            dstream = convertDepthToColor(dw.depthImg);
            
            if (color)
            {
                yield return colorToDepthAsync();
            }
            else
            {
                cstream = dstream;
            }
            
            handler.updateTextures(cstream, dstream);
        }
        
        working = false;
    }

    //Converts depth buffer to a color stream that can be sent to the shader
    private Color32[] convertDepthToColor(short[] depthBuf)
    {
        Color32[] img = new Color32[depthBuf.Length];

        for (int pix = 0; pix < depthBuf.Length; pix++)
        {
            ushort d = (ushort)depthBuf[pix];
            if (d > nearClippingInMeters * 1000 && d < farClippingInMeters * 1000)
            {
                byte[] b = System.BitConverter.GetBytes(d);

                //System.BitConverter.ToUInt16
                img[pix].r = b[0];
                img[pix].g = b[1];
            }
        }
        return img;
    }

    //Convert color stream so that it lays on top of depth stream correctly
    IEnumerator colorToDepthAsync()
    {
        yield return Ninja.JumpBack;

        Color32[] img = new Color32[dw.depthImg.Length];

        Kinect.NuiImageViewArea pcViewArea = new Kinect.NuiImageViewArea
        {
            eDigitalZoom = 0,
            lCenterX = 0,
            lCenterY = 0
        };

        for (int x = 0; x < TEX_WIDTH; ++x)
        {
            for (int y = 0; y < TEX_HEIGHT; ++y)
            {
                ushort d = (ushort)dw.depthImg[y * TEX_WIDTH + x];
                if (d > nearClippingInMeters * 1000 && d < farClippingInMeters * 1000)
                {
                    if (TimeSpan.FromTicks(DateTime.Now.Ticks - timeStamp).TotalSeconds > maximumTimePerFrame)
                    {
                        yield return Ninja.JumpToUnity;
                        Debug.Log("Yielded after: " + TimeSpan.FromTicks(DateTime.Now.Ticks - timeStamp).TotalMilliseconds + " ms, at (" + x + ", " + y);
                        yield return new WaitForEndOfFrame(); // wait for next frame of gameplay
                        yield return Ninja.JumpBack;
                        timeStamp = DateTime.Now.Ticks;
                    }

                    long xc = 0;
                    long yc = 0;


                    //d is millimeter depth

                    Kinect.NativeMethods.NuiImageGetColorPixelCoordinatesFromDepthPixelAtResolution(
                    Kinect.NuiImageResolution.resolution320x240,
                    Kinect.NuiImageResolution.resolution320x240,
                    ref pcViewArea,
                    x, y,
                    (ushort)(d * 7),
                    out xc, out yc);

                    img[y * TEX_WIDTH + x] = cstream[yc * TEX_WIDTH + xc];

                }
            }
        }

        cstream = img;
    }

    //Unneeded
    private Color32[] convertPlayersToCutout(bool[,] players)
    {
        Color32[] img = new Color32[320 * 240];
        for (int pix = 0; pix < 320 * 240; pix++)
        {
            if (players[0, pix] | players[1, pix] | players[2, pix] | players[3, pix] | players[4, pix] | players[5, pix])
            {
                img[pix].a = (byte)255;
            }
            else
            {
                img[pix].a = (byte)0;
            }
        }
        return img;
    }

    //Converts given image to half resolution
    private Color32[] mipmapImg(Color32[] src, int width, int height)
    {
        int newWidth = width / 2;
        int newHeight = height / 2;
        Color32[] dst = new Color32[newWidth * newHeight];
        for (int yy = 0; yy < newHeight; yy++)
        {
            for (int xx = 0; xx < newWidth; xx++)
            {
                /*
                int TLidx = (xx * 2) + yy * 2 * width;
                int TRidx = (xx * 2 + 1) + yy * width * 2;
                int BLidx = (xx * 2) + (yy * 2 + 1) * width;
                int BRidx = (xx * 2 + 1) + (yy * 2 + 1) * width;
                dst[xx + yy * newWidth] = Color32.Lerp(Color32.Lerp(src[BLidx], src[BRidx], .5F),
                                                       Color32.Lerp(src[TLidx], src[TRidx], .5F), .5F);
                                                       */
                dst[xx + yy * newWidth] = src[xx * 2 + yy * 2 * width]; //Convert without antialiasing to reduce computation time
            }
        }
        return dst;
    }
}
