using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System;

public static class PointCloudIO {

    public static void WriteTexturesToPCD(Color32[] cstream, Color32[] dstream, int width, int height, string path)
    {
        StreamWriter sw = new StreamWriter(path);

        sw.WriteLine("FIELDS x y z rgb");
        sw.WriteLine("SIZE 4 4 4 4");
        sw.WriteLine("TYPE F F F U");
        sw.WriteLine("WIDTH " + width);
        sw.WriteLine("HEIGHT " + height);
        sw.WriteLine("VIEWPOINT 0 0 0 1 0 0 0");
        sw.WriteLine("POINTS " + width * height);
        sw.WriteLine("DATA ascii");


        float fx_d = 594.21434211923247f;
        float fy_d = 591.04053696870778f;
        float cx_d = 339.30780975300314f;
        float cy_d = 242.73913761751615f;

        for (int y = 0; y < height; ++y)
        {
            for (int x = 0; x < width; ++x)
            {
                Color32 d = dstream[y * width + x];
                //byte[] dep = { d.r, d.g, d.b, d.a };
                //float depth = BitConverter.ToSingle(dep, 0);

                float rawdepth = d.r * 255 + d.g * 255 * 255;

                Color32 c = cstream[y * width + x];
                byte[] col = { c.r, c.g, c.b, c.a };
                uint color = BitConverter.ToUInt32(col, 0);

                //if (float.IsNaN(rawdepth) || float.IsNaN(color) || rawdepth == 0)
                //{

                //}
                //else
                //{
                    float x_d = x + 160; //(float)((double)(x + 0.5) / (double)320);
                    float y_d = y + 120; //(float)((double)(y + 0.5) / (double)240);
                    //x_d = (x_d + 0.5f) * 320;
                    //y_d = (y_d + 0.5f) * 320;

                    float xr = (x_d - cx_d) * rawdepth / (fx_d) / 320;
                    float yr = (y_d - cy_d) * rawdepth / (fy_d) / 240;
                    float zr = rawdepth / 1000;


                    sw.WriteLine("" + xr + " " + yr + " " + zr + " " + color);
                //}
            }
        }
        sw.Close();
    }

}
