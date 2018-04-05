using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PointCloudPiece : MonoBehaviour {

    public Transform trans;
    public PointHandler parent;

    // Use this for initialization
    void Start () {
    }

    // Update is called once per frame
    void Update () {
        GetComponent<Renderer>().material.SetMatrix("_cam2World", trans.transform.localToWorldMatrix);

        setDepth(parent.getDepthTex());
        setColor(parent.getColorTex());
    }

    public void setDepth(Texture2D t)
    {
        GetComponent<Renderer>().material.SetTexture("_DepthTex", t);
    }

    public void setColor(Texture2D t)
    {
        GetComponent<Renderer>().material.SetTexture("_SpriteTex", t);
    }


}
