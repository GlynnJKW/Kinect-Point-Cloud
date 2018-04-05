using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CielaSpike;
using System.Threading;
using System.IO;

public class PointHandler : MonoBehaviour {

    public int totX = 1920;
    public int totY = 1080;

    public int sizeX = 96; // 96 is the size of the letter X
    public int sizeY = 54; // 54 is the size of the letter Y

    public GameObject cloud;
    public Transform trans;

    private Texture2D ctex;
    private Texture2D dtex;

    public bool recording;
    public bool write = false;

    bool texturesUpdated;

    public int currFrameNumber = 0;
    public string filePath = "";


    // Use this for initialization
    void Start () {

        recording = false;
        texturesUpdated = false;


        ctex = new Texture2D(320, 240, TextureFormat.ARGB32, false);
        dtex = new Texture2D(320, 240, TextureFormat.ARGB32, false);
        dtex.filterMode = FilterMode.Point;


        for (int x = 0; x < totX; x = x + sizeX)
        {
            for(int y = 0; y < totY; y = y + sizeY)
            {
                GameObject cld = Instantiate(cloud) as GameObject;
                cld.GetComponent<MeshFilter>().mesh = CreateMesh(x, y);
                cld.transform.localScale = transform.localScale;
                cld.transform.position = transform.position;
                cld.transform.rotation = transform.rotation;
                cld.transform.parent = transform;

                cld.GetComponent<PointCloudPiece>().trans = trans;
                cld.GetComponent<PointCloudPiece>().parent = this;
            }
        }



    }

    // Update is called once per frame
    void Update () {
        if (texturesUpdated && recording)
        {
            StartCoroutine(encodeAndSave(ctex, dtex));
        }
        texturesUpdated = false;

        if (write)
        {
            write = false;
            writeToPCD();
        }

    }

    public void updateTextures(Color32[] cstream, Color32[] dstream)
    {
        ctex.SetPixels32(cstream);
        ctex.Apply(false);
        dtex.SetPixels32(dstream);
        dtex.Apply(false);
        texturesUpdated = true;
    }

    public void updateTextures(byte[] cstream, byte[] dstream)
    {
        ctex.LoadImage(cstream);
        ctex.Apply(false);
        dtex.LoadImage(dstream);
        dtex.Apply(false);
        texturesUpdated = true;
    }

    public Texture2D getColorTex()
    {
        return ctex;
    }

    public Texture2D getDepthTex()
    {
        return dtex;
    }

    public IEnumerator encodeAndSave(Texture2D tempctex, Texture2D tempdtex)
    {
        currFrameNumber++;
        int frameNum = currFrameNumber;
        byte[] cjpg = tempctex.EncodeToJPG(100);
        byte[] djpg = tempdtex.EncodeToPNG();

        string path = Application.dataPath + "/Recordings/" + filePath;

        if (Directory.Exists(path))
        {

        }
        else
        {
            Directory.CreateDirectory(path);
        }
        File.WriteAllBytes(path + "Frame_" + frameNum + "_C.jpg", cjpg);
        File.WriteAllBytes(path + "Frame_" + frameNum + "_D.png", djpg);
        yield return Ninja.JumpToUnity;
        Debug.Log("saved frame " + frameNum);
        Debug.Log(cjpg.Length);
    }

    private void writeToPCD()
    {
        int frameNum = currFrameNumber;

        string path = Application.dataPath + "/PCDS/" + filePath;

        if (Directory.Exists(path))
        {

        }
        else
        {
            Directory.CreateDirectory(path);
        }

        path += "Frame_" + frameNum + ".pca";

        PointCloudIO.WriteTexturesToPCD(ctex.GetPixels32(), dtex.GetPixels32(), ctex.width, ctex.height, path);

    }

    Mesh CreateMesh(int lastX, int lastY)
    {
        Mesh mesh = new Mesh();

        int nPoints = sizeX * sizeY;

        Vector3[] myPoints = new Vector3[nPoints];
        int[] indecies = new int[nPoints];
        Vector2[] uvs = new Vector2[nPoints];

        for (int i = 0; i < sizeX; ++i)
        {
            for (int j = 0; j < sizeY; ++j)
            {
                myPoints[i * sizeY + j] = Vector3.zero;
                indecies[i * sizeY + j] = i * sizeY + j;
                uvs[i * sizeY + j] = new Vector2((float)((double)(i + lastX + 0.5) / (double)totX), (float)((double)(j + lastY + 0.5) / (double)totY));
            }
        }

        mesh.vertices = myPoints;
        mesh.uv = uvs;
        mesh.SetIndices(indecies, MeshTopology.Points, 0);
        mesh.bounds = new UnityEngine.Bounds(mesh.bounds.center, new Vector3(100, 100, 100));
        return mesh;
    }

    public void toggleRecord()
    {
        recording = !recording;
    }
}
