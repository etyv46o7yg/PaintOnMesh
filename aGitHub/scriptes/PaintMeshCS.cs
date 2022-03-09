using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PaintMeshCS : MonoBehaviour
    {
    public ComputeShader shader;

    [Range(0.0f, 40.0f)]
    public float radius;
    public Color color;
    public Texture2D stikerTex;

    ChaderSysteme sys;
    // Start is called before the first frame update
    void Start()
        {
        string [] nomKerneles = new string [] { "CSMain", "PaintStiker" };

        sys = new ChaderSysteme(shader, nomKerneles);
        sys.AddDict("Main", nomKerneles [0]);
        sys.AddDict("Stik", nomKerneles [1]);
        }

    // Update is called once per frame
    void Update()
        {

        if (Input.GetMouseButtonDown(0) || Input.GetKey(KeyCode.G))
            {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit = new RaycastHit();

            if (Physics.Raycast(ray, out hit))
                {
                //outImage.texture = InterpoleTreangle.GetTextureCoord(hit.collider.gameObject.GetComponent<MeshFilter>().mesh, out bias, out scale);
                var colls = Physics.OverlapSphere(hit.point, 1.0f);

                foreach (var item in colls)
                    {
                    if (item.gameObject.CompareTag("paint"))
                        {
                        //PaintColorAvecCS (hit.point, item.gameObject, color, radius);
                        PaintCtikerCS(hit.point, hit.point - this.gameObject.transform.position, item.gameObject, stikerTex, color, radius);
                        }

                    }


                }
            }
        }

    public int PaintColorAvecCS(Vector3 point, GameObject obj, Color color, float epasseur)
        {
        string nameTextureMask = "_MainTex";
        //string nameTextureMaskAlt = ;
        Texture2D masque;

        var st = obj.GetComponent<StorageDataDraw>();

        if (st == null || !st.enabled)
            {
            return -2;
            }

        masque = st.maskDraw;

        if (masque == null)
            {
            return -1;
            }

        Matrix4x4 matrix = obj.transform.worldToLocalMatrix;
        Vector3 pointPos = matrix.MultiplyPoint3x4(point);
        
        MeshRenderer meshRenderer = obj.GetComponent<MeshRenderer>();            

        if (!meshRenderer.material.HasProperty(nameTextureMask))
            {
            nameTextureMask = "_Mask";
            }

        pointPos -= st.bias;
        pointPos /= st.scale;

        Texture albedoMask = meshRenderer.material.GetTexture(nameTextureMask);

        sys.SetTexture(albedoMask, "MainTex");
        sys.SetTexture(masque, "MaskPaint");
        sys.SetTexture(st.rt, "ResTex");
        sys.shader.SetVector("posDraw", pointPos);  //цвет позиции рисования
        sys.shader.SetVector("posDraw_2", pointPos);

        sys.shader.SetVector("colorFarb", color);    //цвет рисования
        sys.SetFloat("scale", st.scale * obj.transform.lossyScale.magnitude);     //масштаб
        sys.SetFloat("epasseur", epasseur);  //ширина 

        sys.Dispatch("Main", 32, 32, 1);

        //RenderTexture.active = st.rt;
        //st.tempTex.ReadPixels(new Rect(0, 0, st.rt.width, st.rt.height), 0, 0);
        //st.tempTex.Apply();
        meshRenderer.material.SetTexture(nameTextureMask, st.rt);


        return 0;
        }
    
    public void PaintCtikerCS(Vector3 point, Vector3 normal, GameObject obj, Texture2D stiker, Color _color, float _radius)
        {
        GameObject aux = new GameObject();
        aux.transform.forward = normal;
        Vector3 refVecX = aux.transform.up;
        Vector3 refVecY = aux.transform.right;
        Destroy(aux);

        Matrix4x4 matritia = Matrix4x4.LookAt(normal, -Vector3.forward, Vector3.up);

        string nameTextureMask = "_MainTex";
        //string nameTextureMaskAlt = ;
        Texture2D masque;

        var st = obj.GetComponent<StorageDataDraw>();

        masque = st.maskDraw;

        Vector3 pointPos = st.TransformPoint(point);

        MeshRenderer meshRenderer = obj.GetComponent<MeshRenderer>();

        if (!meshRenderer.material.HasProperty(nameTextureMask))
            {
            nameTextureMask = "_Mask";
            }

        Texture albedoMask = meshRenderer.material.GetTexture(nameTextureMask);

        sys.SetTexture(albedoMask, "MainTex");
        sys.SetTexture(masque, "MaskPaint");
        sys.SetTexture(st.maskNorm, "MaskNormal");
        sys.SetTexture(st.rt, "ResTex");
        sys.SetTexture(stiker, "Stiker");
        sys.shader.SetVector("posDraw", pointPos);  //цвет позиции рисования
        sys.shader.SetVector("posDraw_2", pointPos);
        sys.shader.SetVector("refVecX", refVecX);
        sys.shader.SetVector("refVecY", refVecY);
        sys.shader.SetVector("normal",  normal);
        sys.shader.SetVector("colorFarb", _color);    //цвет рисования
        sys.shader.SetMatrix("matritia", matritia);
        sys.SetFloat("scale", st.scale * obj.transform.lossyScale.magnitude);     //масштаб
        sys.SetFloat("epasseur", _radius);  //ширина 
        sys.SetFloat("scaleStiker", 2);
        sys.SetInt("sizeSkiker", stiker.width);

        sys.Dispatch("Stik", 32, 32, 1);
        Graphics.Blit(st.rt, st.rt_2);
        //RenderTexture.active = st.rt;
        //st.tempTex.ReadPixels(new Rect(0, 0, st.rt.width, st.rt.height), 0, 0);
        //st.tempTex.Apply();
        meshRenderer.material.SetTexture(nameTextureMask, st.rt_2);
        }
    }
