using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;

public class UnwrapExp : MonoBehaviour
    {
    public Mesh mesh;
    public Material mat, mirrorMat;
    public RawImage screen;

    public RenderTexture courrTex, resTex;
    public Vector3 pos;
    CommandBuffer buffer;
    public MeshRenderer dend;
    Matrix4x4 objectMatrix;
    public Vector3 meshPosition = new Vector3(0.5f, 0.5f, -1);
    // Start is called before the first frame update
    void Start()
        {
        //resTex = new RenderTexture(1024, 1024, 32);
        //resTex.Create();
        screen.texture = courrTex;
        buffer = new CommandBuffer();
        //Camera.main.AddCommandBuffer(CameraEvent.AfterFinalPass, buffer);
        }

    // Update is called once per frame
    void Update()
        {
        /*
        Debug.Log("рендер");
        //resTex.Release();
        buffer.SetRenderTarget(resTex);
        buffer.DrawMesh(mesh, Matrix4x4.TRS(pos, Quaternion.identity, Vector3.one), mat, 0);
        buffer.DrawRenderer(dend, dend.material);
        Graphics.ExecuteCommandBuffer(buffer);
        */
        //objectMatrix = Matrix4x4.TRS(meshPosition, Quaternion.identity, Vector3.one * 0.3f);
        
        Blit();
        }

    void Blit()
        {
        Vector3 minCorn = mesh.bounds.min;
        Vector3 maxCorn = mesh.bounds.max;
        Vector3 delta = maxCorn - minCorn;
        maxCorn -= minCorn;
        float scale = Mathf.Max(maxCorn.x, maxCorn.y, maxCorn.z);
        Vector3 bias = minCorn;

        // Create an orthographic matrix (for 2D rendering)
        // You can otherwise use Matrix4x4.Perspective()
        Matrix4x4 projectionMatrix = Matrix4x4.Ortho(0, 1, 0, 1, 0.1f, 100);

        // This fixes flickering (by @guycalledfrank)
        // (because there's some switching back and forth between cameras, I don't fully understand)

        // Remember the current texture and set our own as "active".
        RenderTexture prevRT = RenderTexture.active;
        RenderTexture.active = courrTex;

        // Set material as "active". Without this, Unity editor will freeze.
        mat.SetPass(0);
        mat.SetFloat("scale", scale);
        mat.SetVector("bias", new Vector4(bias.x, bias.y, bias.z, 0));

        // Push the projection matrix
        GL.PushMatrix();
        GL.LoadProjectionMatrix(projectionMatrix);

        // It seems that the faces are in a wrong order, so we need to flip them
        GL.invertCulling = true;

        // Clear the texture
        GL.Clear(true, true, Color.black);

        // Draw the mesh!
        Graphics.DrawMeshNow(mesh, objectMatrix);
        //Graphics.Blit(resTex, courrTex);

        // Pop the projection matrix to set it back to the previous one
        GL.PopMatrix();

        // Revert culling
        GL.invertCulling = false;

        // Re-set the RenderTexture to the last used one
        RenderTexture.active = prevRT;
        }
    }
