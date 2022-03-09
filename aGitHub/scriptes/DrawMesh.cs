using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class DrawMesh : MonoBehaviour
    {
    static Vector3 prevPos = Vector3.zero;

    public static DrawMesh instance;
    public Color color;

    [Range(0.0f, 40.0f)]
    public float radius;

    [Range(0.0f, 1.0f)]
    public float koeffSpot;

    public Material mat;

    /// <summary>
    /// single spot regime
    /// </summary>
    public bool estDraw = false;

    /// <summary>
    /// continuous drawing
    /// </summary>
    public bool estDrawPermanent = false;

    /// <summary>
    /// actif regime
    /// </summary>
    public bool Enable = false;

    void Start()
        {
        if (instance == null)
            { // Экземпляр менеджера был найден
            instance = this; // Задаем ссылку на экземпляр объекта
            }
        else if (instance == this)
            { // Экземпляр объекта уже существует на сцене
            Destroy(gameObject); // Удаляем объект
            }
        }

    private void DrawSurPosition()
        {
        var col = Physics.OverlapSphere(transform.position, 1.0f);

        foreach (var item in col)
            {
            DrawMesh.Draw(transform.position, item.gameObject, mat, color, radius, koeffSpot);

            Debug.Log("по позиции, точка рисования  = " + transform.position);
            }
        
        }

    // Update is called once per frame
    void Update()
        {
        if (!Enable)
            {
            return;
            }       

        if (Input.GetMouseButton(0))
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
                        DrawMesh.Draw(hit.point, item.gameObject, mat, color, radius, koeffSpot);
                        }
                    
                    }

              
                }
            }

        if (estDraw || estDrawPermanent)
            {
            DrawSurPosition();
            estDraw = false;
            }
        }

    public static int Draw(Vector3 point, GameObject obj, Material mat, Color color, float epasseur, float spot)
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
        

        bool estCorrectCoord = (pointPos.x > 1.0f || pointPos.y > 1.0f || pointPos.z > 1.0f) || (pointPos.x < 0.0f || pointPos.y < 0.0f || pointPos.z < 0.0f);
        MeshRenderer meshRenderer = obj.GetComponent<MeshRenderer>();

        Texture albedoMask; //карта рисования пятен             

        if (!meshRenderer.material.HasProperty(nameTextureMask))
            {
            nameTextureMask = "_Mask";
            }

        pointPos -= st.bias;
        pointPos /= st.scale;
        Color uvColor = new Color(pointPos.x, pointPos.y, pointPos.z);

        Vector3 poitnPos_2 = matrix.MultiplyPoint3x4(prevPos);
        poitnPos_2 -= st.bias;
        poitnPos_2 /= st.scale;
        Color uvColor_2 = new Color(poitnPos_2.x, poitnPos_2.y, poitnPos_2.z);

        if ( (prevPos - point).sqrMagnitude > 1.5f)
            {
            uvColor_2 = uvColor;
            }

        albedoMask = meshRenderer.material.GetTexture(nameTextureMask);       

        mat.SetTexture("_MainTex",   albedoMask);
        mat.SetTexture("_Mask",      masque);
        mat.SetColor  ("_Color",     uvColor);  //цвет позиции рисования
        mat.SetColor  ("_Color_B",   uvColor_2);
        mat.SetColor  ("_ColorFarb", color);    //цвет рисования
        mat.SetFloat  ("_Scale",     st.scale * obj.transform.lossyScale.magnitude);     //масштаб
        mat.SetFloat  ("_Epasseur",  epasseur);  //ширина 
        mat.SetFloat("_KoeffSpot",   spot); 

        //Graphics.Blit(albedoMask, st.rt, mat);
        Graphics.Blit(st.rt, st.rt_2, mat);
        Graphics.Blit(st.rt_2, st.rt);

        //RenderTexture.active = st.rt;
        //st.tempTex.ReadPixels(new Rect(0, 0, st.rt.width, st.rt.height), 0, 0);
        //st.tempTex.Apply();
        meshRenderer.material.SetTexture(nameTextureMask, st.rt);

        prevPos = point;

        return 0;
        }

    public int DrawPos(Vector3 point, Color color, float epasseur)
        {       
        var colls = Physics.OverlapSphere(point, 1.0f);

        foreach (var item in colls)
            {
            DrawMesh.Draw(point, item.gameObject, mat, color, epasseur, koeffSpot);
            }
            


        prevPos = point;

        return 0;
        }

    private void OnGUI()
        {

        
        }

    public void SauverToutaTextures()
        {
        var objets = FindObjectsOfType<StorageDataDraw>();

        foreach (var item in objets)
            {
            item.SouverTextureDrawAlbedo();
            }

        Debug.Log("сохранение готово");
        }
    }



public class InterpoleTreangle
    {
    /// <summary>
    /// 
    /// </summary>
    /// <param name="mesh">входной меш</param>
    /// <param name="bias"></param>
    /// <param name="scale"></param>
    /// <param name="sizeTexture"></param>
    /// <returns>текстураная карта положения и нормали (tex1, tex2)</returns>
    public static (Texture2D, Texture2D) GetTextureCoord(Mesh mesh, out Vector3 bias, out float scale, int sizeTexture)
        {
        Texture2D texMapPos = new Texture2D(sizeTexture, sizeTexture);
        Texture2D texMapNorm = new Texture2D(sizeTexture, sizeTexture);

        for (int i = 0; i < sizeTexture; i++)
            {
            for (int j = 0; j < sizeTexture; j++)
                {
                texMapPos.SetPixel(i, j, Color.black);
                texMapNorm.SetPixel(i, j, Color.black);
                }
            }

        texMapPos.Apply();
        texMapNorm.Apply();

        Vector3[] vertex  = mesh.vertices;
        Vector3[] normals = mesh.normals;
        Vector2[] uvS     = mesh.uv;
        int[] tringles    = mesh.triangles;       
        int treangleCount = tringles.Length / 3;

        Vector3 biasRef;
        float scaleRef;
        vertex = InterpoleTreangle.normalizeVertex(vertex, out biasRef, out scaleRef);

        bias = biasRef;
        scale = scaleRef;

        TreangeMesh[] toutaTrengle = new TreangeMesh[treangleCount];      

        for (int i = 0; i < treangleCount; i++)
            {
            toutaTrengle[i] = new TreangeMesh();

            int j = i * 3;

            toutaTrengle[i].A.vertex = vertex[ tringles[j] ];
            toutaTrengle[i].B.vertex = vertex[ tringles[j + 1] ];
            toutaTrengle[i].C.vertex = vertex[ tringles[j + 2] ];

            toutaTrengle[i].A.uv = uvS[tringles[j]];
            toutaTrengle[i].B.uv = uvS[tringles[j + 1]];
            toutaTrengle[i].C.uv = uvS[tringles[j + 2]];

            toutaTrengle[i].A.normal = normals[tringles [j]];
            toutaTrengle[i].B.normal = normals[tringles [j + 1]];
            toutaTrengle[i].C.normal = normals[tringles [j + 2]];
            }      

        foreach (var item in toutaTrengle)
            {
            //Debug.Log(item.ToString());
            }
        //Debug.Log("число вершин = " + vertex.Length + " число uv = " + uvS.Length);

        foreach (var item in toutaTrengle)
            {
            Vector3 Avex = item.A.vertex;
            Vector3 Bvex = item.B.vertex;
            Vector3 Cvex = item.C.vertex;

            Vector3 Anorm = item.A.normal;
            Vector3 Bnorm = item.B.normal;
            Vector3 Cnorm = item.C.normal;

            Vector2 min = Vector2.zero;
            min.x = Mathf.Min(item.A.uv.x, item.B.uv.x, item.C.uv.x);
            min.y = Mathf.Min(item.A.uv.y, item.B.uv.y, item.C.uv.y);

            Vector2 max = Vector2.zero;
            max.x = Mathf.Max(item.A.uv.x, item.B.uv.x, item.C.uv.x);
            max.y = Mathf.Max(item.A.uv.y, item.B.uv.y, item.C.uv.y);

            int xMin = Mathf.RoundToInt(sizeTexture * min.x);
            int yMin = Mathf.RoundToInt(sizeTexture * min.y);
            int xMax = Mathf.RoundToInt(sizeTexture * max.x);
            int yMax = Mathf.RoundToInt(sizeTexture * max.y);

            for (int i = xMin; i < xMax; i++)
                {
                for (int j = yMin; j < yMax; j++)
                    {
                    Vector2 courrPoint;
                    courrPoint.x = (float) i / sizeTexture;
                    courrPoint.y = (float) j / sizeTexture;

                    float x_1 = item.A.uv.x;
                    float y_1 = item.A.uv.y;
                    //----------------------
                    float x_2 = item.B.uv.x;
                    float y_2 = item.B.uv.y;
                    //----------------------
                    float x_3 = item.C.uv.x;
                    float y_3 = item.C.uv.y;

                    if ( InterpoleTreangle.EstPointSurTriangle(item.A.uv, item.B.uv, item.C.uv, courrPoint) )
                        {
                        Vector3 resultColorPos  = Vector3.zero;
                        Vector3 resultColorNorm = Vector3.zero;

                        float det = (y_2 - y_3) * (x_1 - x_3) + (x_3 - x_2) * (y_1 - y_3);

                        float massA = ( (y_2 - y_3) * (courrPoint.x - x_3) + (x_3 - x_2) * (courrPoint.y - y_3) )/det;
                        float massB = ( (y_3 - y_1) * (courrPoint.x - x_3) + (x_1 - x_3) * (courrPoint.y - y_3) )/det;
                        float massC = 1.0f - massA - massB;

                        resultColorPos  = Avex  * massA + Bvex  * massB + Cvex  * massC;
                        resultColorNorm = Anorm * massA + Bnorm * massB + Cnorm * massC;

                        resultColorNorm = resultColorNorm * 0.5f + new Vector3(0.5f, 0.5f, 0.5f);

                        texMapPos.SetPixel(i, j, new Color(resultColorPos.x, resultColorPos.y, resultColorPos.z, 1.0f));
                        texMapNorm.SetPixel(i, j, new Color(resultColorNorm.x, resultColorNorm.y, resultColorNorm.z, 1.0f));
                        }
                    
                        
                    }
                }

            //Debug.Log(min + " " + max);
            }

        texMapPos.Apply();
        texMapNorm.Apply();
        return (texMapPos, texMapNorm);
        }

    /// <summary>
    /// наивная интерполяция атрибутов между тремя точками
    /// </summary>
    /// <param name="A">атрибут А</param>
    /// <param name="B">атрибут Б</param>
    /// <param name="C">атрибут С</param>
    /// <param name="uvA">точка А</param>
    /// <param name="uvB">точка Б</param>
    /// <param name="uvC">точка С</param>
    /// <param name="point">заданная точка</param>
    /// <returns>значение цвета в точке</returns>
    public static Vector3 NaifInterpolacio(Vector3 A, Vector3 B, Vector3 C, Vector2 uvA, Vector2 uvB, Vector2 uvC, Vector2 point)
        {
        float distA = Vector2.Distance(uvA, point);
        float distB = Vector2.Distance(uvB, point);
        float distC = Vector2.Distance(uvC, point);

        float wA = 1 / distA;
        float wB = 1 / distB;
        float wC = 1 / distC;

        return (wA * A + wB * B + wC * C) / (wA + wB + wC);
        }

    public static Vector3 NaifInterpolacio_2(Vector3 A, Vector3 B, Vector3 C, Vector2 uvA, Vector2 uvB, Vector2 uvC, Vector2 point)
        {
        Vector2 ab = uvB - uvA;
        ab.Normalize();

        float f = Vector2.Dot( point - uvA,   ab );

        Vector3 promRes = (A * f + B * (1.0f - f)) * point.x;

        return promRes;
        }

    /// <summary>
    /// лежит ли точка внутри треугольника
    /// </summary>
    /// <param name="A">вершина А</param>
    /// <param name="B">вершина B</param>
    /// <param name="C">вершина C</param>
    /// <param name="point">точка</param>
    /// <returns></returns>
    public static bool EstPointSurTriangle(Vector2 A, Vector2 B, Vector2 C, Vector2 point)
        {
        float x1 = A.x;
        float x2 = B.x;
        float x3 = C.x;
        float y1 = A.y;
        float y2 = B.y;
        float y3 = C.y;

        float x0 = point.x;
        float y0 = point.y;

        float f1 = (x1 - x0) * (y2 - y1) - (x2 - x1) * (y1 - y0);
        float f2 = (x2 - x0) * (y3 - y2) - (x3 - x2) * (y2 - y0);
        float f3 = (x3 - x0) * (y1 - y3) - (x1 - x3) * (y3 - y0);
        
        if( (f1 >= 0 && f2 >= 0 && f3 >= 0) || (f1 <= 0 && f2 <= 0 && f3 <= 0 ) )
            {
            return true;
            }
        else
            {
            return false;
            }
        }
    
    private static TreangeMesh[] normalizeMesh(TreangeMesh[] arr, Vector3 bias)
        {


        for (int i = 0; i < arr.Length; i++)
            {
            arr[i].A.vertex -= bias;
            arr[i].B.vertex -= bias;
            arr[i].C.vertex -= bias;
            }

        return arr;
        }
    
    public static Vector3[] normalizeVertex(Vector3[] array, out Vector3 bias, out float scale)
        {     

        Vector3 min;
        min.x = array[0].x;
        min.y = array[0].y;
        min.z = array[0].z;

        Vector3 max;
        max.x = array[0].x;
        max.y = array[0].y;
        max.z = array[0].z;

        for (int i = 0; i < array.Length; i++)
            {
            if (array[i].x < min.x)
                {
                min.x = array[i].x;
                }

            if (array[i].y < min.y)
                {
                min.y = array[i].y;
                }

            if (array[i].z < min.z)
                {
                min.z = array[i].z;
                }

            //-------------
            if (array[i].x > max.x)
                {
                max.x = array[i].x;
                }

            if (array[i].y > max.y)
                {
                max.y = array[i].y;
                }

            if (array[i].z > max.z)
                {
                max.z = array[i].z;
                }
            }
        //-------------------------


        for (int i = 0; i < array.Length; i++)
            {
            array[i] -= min;
            }

        max -= min;

        scale = Mathf.Max(max.x, max.y, max.z);
        bias = min; 

        for (int i = 0; i < array.Length; i++)
            {
            array[i] = array[i] / scale;
            }


        return array;
        }
    
    /// <summary>
    /// класс для хранения информации о положении и uv в одном месте
    /// </summary>
    private class VertexTout
        {
        public VertexTout()
            {
            vertex = Vector3.zero;
            uv = Vector2.zero;
            }

        public Vector3 vertex;
        public Vector3 normal;
        public Vector2 uv;
        }

    /// <summary>
    /// треугольник меша
    /// </summary>
    private class TreangeMesh
        {
        public TreangeMesh()
            {
            A = new VertexTout();
            B = new VertexTout();
            C = new VertexTout();
            }

        public VertexTout A, B, C;

        public override string ToString()
            {
            string res = "Pos " + A.vertex + B.vertex + C.vertex + "; UV = " + A.uv + B.uv + C.uv;
            return res;
            }
        }
    }
