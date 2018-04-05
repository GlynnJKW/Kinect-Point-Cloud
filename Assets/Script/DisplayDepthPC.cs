using UnityEngine;
using System.Collections;
using System;

[RequireComponent(typeof(PointHandler))]
public class DisplayDepthPC : MonoBehaviour {
	
	public DepthWrapper dw;
    public Camera cam;

    public DeviceOrEmulator devOrEmu;
    private Kinect.KinectInterface kinect;


    private Texture2D tex;
	// Use this for initialization
	void Start () {
        kinect = devOrEmu.getKinect();
        //tex = new Texture2D(320, 240, TextureFormat.R16, false);
        tex = new Texture2D(320, 240, TextureFormat.ARGB32, false);
        tex.filterMode = FilterMode.Bilinear;

        //GetComponent<Renderer>().material.mainTexture = tex;
        //GetComponent<PointHandler>().dtex = tex;
    }

    // Update is called once per frame
    void Update () {
		if (dw.pollDepth())
		{
            //tex.GetRawTextureData(dw.depthImg);
			tex.SetPixels32(convertDepthToColor(dw.depthImg));
			//tex.SetPixels32(convertPlayersToCutout(dw.segmentations));
			tex.Apply(false);
		}
	}


    private Color32[] convertDepthToColor(short[] depthBuf)
    {
        Color32[] img = new Color32[depthBuf.Length];

        for (int pix = 0; pix < depthBuf.Length; pix++)
        {
            byte[] b = System.BitConverter.GetBytes(depthBuf[pix]);

            //System.BitConverter.ToUInt16
            img[pix].r = (byte)b[0];
            img[pix].g = (byte)(b[1]);
        }
        return img;
    }

    private Color32[] convertPlayersToCutout(bool[,] players)
	{
		Color32[] img = new Color32[320*240];
		for (int pix = 0; pix < 320*240; pix++)
		{
			if(players[0,pix]|players[1,pix]|players[2,pix]|players[3,pix]|players[4,pix]|players[5,pix])
			{
				img[pix].a = (byte)255;
			} else {
				img[pix].a = (byte)0;
			}
		}
		return img;
	}
}
