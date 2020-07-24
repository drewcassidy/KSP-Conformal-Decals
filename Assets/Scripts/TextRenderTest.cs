using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Rendering;

public class TextRenderTest : MonoBehaviour {
    //[InspectorButton("go")] public bool button;

    public Camera _camera;

    public GameObject _cameraObject;

    public TextMeshPro _tmp;

    public Material _blitMaterial;

    public Material _targetMaterial;

    public  RenderTexture renderTex;
    private float         pixelDensity   = 5;
    private int           MaxTextureSize = 4096;

    public const TextureFormat       TextTextureFormat       = TextureFormat.RG16;
    public const RenderTextureFormat TextRenderTextureFormat = RenderTextureFormat.R8;

    // Start is called before the first frame update
    void Start() {
        Debug.Log("starting...");

        StartCoroutine(OnRender());
    }

    // Update is called once per frame
    void Update() { }

    private IEnumerator OnRender() {
        Debug.Log("starting...2");

        // calculate camera and texture size
        _tmp.ForceMeshUpdate();
        var mesh = _tmp.mesh;
        mesh.RecalculateBounds();
        var bounds = mesh.bounds;
        Debug.Log(bounds.size);

        var width = bounds.size.x * pixelDensity;
        var height = bounds.size.y * pixelDensity;

        var widthPoT = Mathf.NextPowerOfTwo((int) width);
        var heightPoT = Mathf.NextPowerOfTwo((int) height);

        if (widthPoT > MaxTextureSize) {
            widthPoT /= widthPoT / MaxTextureSize;
            heightPoT /= widthPoT / MaxTextureSize;
        }

        if (heightPoT > MaxTextureSize) {
            widthPoT /= heightPoT / MaxTextureSize;
            heightPoT /= heightPoT / MaxTextureSize;
        }

        widthPoT = Mathf.Min(widthPoT, MaxTextureSize);
        heightPoT = Mathf.Min(heightPoT, MaxTextureSize);

        var widthRatio = widthPoT / width;
        var heightRatio = heightPoT / height;

        var sizeRatio = Mathf.Min(widthRatio, heightRatio);

        int scaledHeight = (int) (sizeRatio * height);
        int scaledWidth = (int) (sizeRatio * width);

        Debug.Log($"width = {scaledWidth}");
        Debug.Log($"height = {scaledHeight}");

        _camera.orthographicSize = scaledHeight / pixelDensity / 2;
        _camera.aspect = (float) widthPoT / heightPoT;

        _cameraObject.transform.localPosition = new Vector3(bounds.center.x, bounds.center.y, -1);

        var halfHeight = scaledHeight / pixelDensity / 2;
        var halfWidth = scaledWidth / pixelDensity / 2;
        var matrix = Matrix4x4.Ortho(bounds.center.x - halfWidth, bounds.center.x + halfWidth,
            bounds.center.y - halfHeight, bounds.center.y + halfHeight, -1, 1);

        // setup texture
        var texture = new Texture2D(widthPoT, heightPoT, TextTextureFormat, true);
        _targetMaterial.mainTexture = texture;


        // setup render texture
        renderTex = RenderTexture.GetTemporary(widthPoT, heightPoT, 0, TextRenderTextureFormat, RenderTextureReadWrite.Linear, 1);
        renderTex.autoGenerateMips = false;

        RenderTexture.active = renderTex;
        GL.PushMatrix();
        GL.LoadProjectionMatrix(matrix);
        _blitMaterial.SetPass(0);
        Graphics.DrawMeshNow(mesh, Matrix4x4.identity);
        GL.PopMatrix();

        // setup material
        _blitMaterial.mainTexture = _tmp.font.atlas;

        yield return null;

        RenderTexture.active = renderTex;
        texture.ReadPixels(new Rect(0, 0, widthPoT, heightPoT), 0, 0, true);
        texture.Apply(false, true);

        RenderTexture.ReleaseTemporary(renderTex);
    }
}