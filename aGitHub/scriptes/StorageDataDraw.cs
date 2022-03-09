using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;
using UnityEngine.Experimental.Rendering;

public class StorageDataDraw : MonoBehaviour
    {
    public string idName = "1";

    public bool estSetTextureMask = false;
    public bool estCreatNouveauTexture = false;

    public Texture2D maskDraw;
    public Texture2D maskNorm;
    public Vector3 bias;
    public float scale;

    private Vector3 prevPos;

    public int sizeTex = 4096;

    [HideInInspector]
    public MeshRenderer mr;

    [HideInInspector]
    public RenderTexture rt, rt_2;
    string path;

    string nameTextureMaskAlt = "_MainTex";          
    /// <summary>
    /// имя текстуры для карты меша
    /// </summary>
    public string nameTextureAlbedo;

    // Start is called before the first frame update
    void Start()
        {
        path = Application.dataPath + "/" + "map_draw_sys" + "/";

        mr = GetComponent<MeshRenderer>();

        try
            {
            AutoLoad();
            }
        catch (System.Exception)
            {
            throw;
            }
        
        }

    private void AutoLoad()
        {
        MeshFilter meshF = GetComponent<MeshFilter>();
        MeshRenderer meshRenderer = gameObject.GetComponent<MeshRenderer>();

        if (meshF == null)
            {
            throw new System.Exception("MeshFilterNonExist");          
            }

        if ( !meshF.mesh.isReadable)
            {
            //throw new System.Exception("Not allow write/read mesh!");
            }

        if (meshRenderer == null)
            {
            throw new System.Exception("MehsRenderer not exist");
            }

        if (meshRenderer.materials.Length == 0)
            {
            throw new System.Exception("Not set materials to meshRenderer");
            }


        //если материал не содержит текстуры - перейти к резервному
        if (!meshRenderer.material.HasProperty(nameTextureAlbedo))
            {
            nameTextureAlbedo = nameTextureMaskAlt;
            }

        if (estCreatNouveauTexture)
            {
            System.Diagnostics.Stopwatch stopwatch = new System.Diagnostics.Stopwatch();
            stopwatch.Start();

            var res = InterpoleTreangle.GetTextureCoord(meshF.mesh, out bias, out scale, sizeTex);
            maskDraw = res.Item1;
            maskNorm = res.Item2;

            stopwatch.Stop();
            Debug.Log("Создание на ЦП " + stopwatch.ElapsedMilliseconds);

            SouveTextureMap(idName);
            
            
            }
        else
            {
            if (TryLoadTextureMap( idName ) )
                {

                }
            else
                {                                                           
                var res = InterpoleTreangle.GetTextureCoord(meshF.mesh, out bias, out scale, sizeTex);
                maskDraw = res.Item1;
                maskNorm = res.Item2;
                SouveTextureMap( idName );
                }       
        
            if (estSetTextureMask)
                {
                meshRenderer.material.mainTexture = maskDraw;
                }        
           
            }

        if (estSetTextureMask)
            {
            meshRenderer.material.mainTexture = maskDraw;
            }

        TryLoadtextureAlbedo( idName);

        rt = new RenderTexture(maskDraw.width, maskDraw.height, 16);
        rt.format = RenderTextureFormat.ARGB32;
        rt.enableRandomWrite = true;
        rt.Create();

        rt_2 = new RenderTexture(maskDraw.width, maskDraw.height, 16);
        rt_2.format = RenderTextureFormat.ARGB32;
        rt_2.enableRandomWrite = true;
        rt_2.Create();

        Graphics.Blit(meshRenderer.material.GetTexture("_MainTex"), rt);           
        //meshRenderer.material.SetTexture("_MainTex", rt);
        }

    Texture2D ToTexture2D(RenderTexture rTex)
        {
        Texture2D tex = new Texture2D(512, 512, TextureFormat.RGB24, false);
        RenderTexture.active = rTex;
        tex.ReadPixels(new Rect(0, 0, rTex.width, rTex.height), 0, 0);
        tex.Apply();
        return tex;
        }

    public void SouveTextureMap(string nom)
        {        
        string pathLocal = path;

        string toutaPath = pathLocal + "/" + nom + "/";

        if (!Directory.Exists(toutaPath))
            {
            Directory.CreateDirectory(toutaPath);                       
            }

        var bytes = maskDraw.EncodeToPNG();
        File.WriteAllBytes(toutaPath + "/" + nom + ".png", bytes);

        var bytesN = maskNorm.EncodeToPNG();
        File.WriteAllBytes(toutaPath + "/" + nom + "N.png", bytesN);

        MapDrawData data = new MapDrawData();
        data.bias  = bias;
        data.scale = scale;
        string res = JsonUtility.ToJson(data);

        File.WriteAllText(toutaPath + "data.txt", res);
        }

    public bool TryLoadTextureMap(string nom)
        {
        string pathLocal = path;
        string toutaPath = pathLocal + "/" + nom + "/" ;
        string filePath  = toutaPath + "/" + nom + ".png";

        if (File.Exists(filePath))
            {
            var fileData = File.ReadAllBytes(toutaPath + "/" + nom + ".png");
            Texture2D tex = new Texture2D(sizeTex, sizeTex, TextureFormat.RGBA32, false);
            tex.LoadImage(fileData);
            maskDraw = tex;

            var fileDataN = File.ReadAllBytes(toutaPath + "/" + nom + "N.png");
            Texture2D texN = new Texture2D(sizeTex, sizeTex, TextureFormat.RGBA32, false);
            texN.LoadImage(fileData);
            maskNorm = texN;

            string dataText = File.ReadAllText(toutaPath + "/data.txt");

            MapDrawData mapDrawData = JsonUtility.FromJson<MapDrawData>(dataText);

            scale = mapDrawData.scale;
            bias = mapDrawData.bias;

            return true;
            }
        else
            {
            return false;
            }
        }

    public bool SouverTextureDrawAlbedo( )
        {
        string nom = idName;
        string pathLocal = path;
        string toutaPath = pathLocal + "/" + nom + "/";

        if (!Directory.Exists(toutaPath))
            {
            Directory.CreateDirectory(toutaPath);
            }


        Texture tex = mr.material.GetTexture(nameTextureAlbedo);

        Texture2D tex_2 = new Texture2D(tex.width, tex.height, GraphicsFormat.R16G16B16A16_SFloat, TextureCreationFlags.None);// = tex as Texture2D;

        //rt.sRGB = true;

        RenderTexture.active = rt;
        tex_2.ReadPixels(new Rect(0, 0, rt.width, rt.height), 0, 0);
        tex_2.Apply();

        var data = tex_2.EncodeToPNG();
        File.WriteAllBytes(toutaPath + "albedo.png", data);

        return true;
        }

    public bool TryLoadtextureAlbedo(string nom)
        {
        string pathLocal = path;
        string toutaPath = pathLocal + "/" + nom + "/";
        string filePath = toutaPath + "albedo.png";

        if (File.Exists(filePath))
            {
            var fileData = File.ReadAllBytes(filePath);
            Texture2D tex = new Texture2D(sizeTex, sizeTex, TextureFormat.RGBA32, false);
            tex.LoadImage(fileData);
            mr.material.SetTexture(nameTextureAlbedo, tex);

            return true;
            }
        else
            {
            return false;
            }
        }

    public void SetData(Vector3 _bias, float _scale, Texture2D tex)
        {
        bias = _bias;
        scale = _scale;
        maskDraw = tex;
        }

    public class MapDrawData
        {
        public Vector3 bias;
        public float scale;

        public MapDrawData()
            {

            }
        }
    
    public Vector3 TransformPoint(Vector3 point)
        {
        Matrix4x4 matrix = this.gameObject.transform.worldToLocalMatrix;
        Vector3 pointPos = matrix.MultiplyPoint3x4(point);
        pointPos -= bias;
        pointPos /= scale;
        return pointPos;
        }
    }

[System.Serializable]
internal class TextureBin
    {
    public int size;
    public Color[,] color;// = new Color[size][size];

    public TextureBin(int _size)
        {
        color = new Color[_size, _size];
        size = _size;
        }
    }