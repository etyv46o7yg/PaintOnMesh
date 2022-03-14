using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;

public class UnwrapExp : MonoBehaviour
    {
    public Mesh mesh;
    public Material mat;
    public RawImage screen;

    public RenderTexture courrTex, resTex;
    public Vector3 pos;
    CommandBuffer buffer;
    // Start is called before the first frame update
    void Start()
        {
        resTex = new RenderTexture(1024, 1024, 32);
        resTex.Create();
        screen.texture = resTex;

        buffer = new CommandBuffer();
        Camera.main.AddCommandBuffer(CameraEvent.AfterFinalPass, buffer);
        }

    // Update is called once per frame
    void Update()
        {
        //Debug.Log("рендер");
        //resTex.Release();
        buffer.SetRenderTarget(resTex);
        buffer.DrawMesh(mesh, Matrix4x4.TRS(pos, Quaternion.identity, Vector3.one), mat, 0);

        
        Graphics.ExecuteCommandBuffer(buffer);
        }
    }
