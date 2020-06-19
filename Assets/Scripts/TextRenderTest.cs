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

    public RenderTexture renderTex;
    private float pixelDensity = 36;
    private int MaxTextureSize = 4096;

    public const TextureFormat TextTextureFormat = TextureFormat.RG16;
    public const RenderTextureFormat TextRenderTextureFormat = RenderTextureFormat.R8;
    
    // Start is called before the first frame update
    void Start() {
        Debug.Log("starting...");

        StartCoroutine(OnRender());
    }

    // Update is called once per frame
    void Update() {
    }

    private void go() {
        Debug.Log("starting...");
    }

    private IEnumerator OnRender() {
        Debug.Log("starting...2");

        // calculate camera and texture size
        _tmp.ForceMeshUpdate();
        var mesh = _tmp.mesh;
        mesh.RecalculateBounds();
        var bounds = mesh.bounds;
        Debug.Log(bounds.size);

        var width = Mathf.NextPowerOfTwo((int) (bounds.size.x * pixelDensity));
        var height = Mathf.NextPowerOfTwo((int) (bounds.size.y * pixelDensity));
        
        Debug.Log($"width = {width}");
        Debug.Log($"height = {height}");
        
        _camera.orthographicSize = height / pixelDensity / 2;
        _camera.aspect = (float) width / height;

        _cameraObject.transform.localPosition = new Vector3(bounds.center.x, bounds.center.y, -1);

        width = Mathf.Min(width, MaxTextureSize);
        height = Mathf.Min(height, MaxTextureSize);
        
        // setup texture
        var texture = new Texture2D(width, height, TextTextureFormat, true);
        _targetMaterial.mainTexture = texture;

        // setup render texture
        renderTex = RenderTexture.GetTemporary(width, height, 0, TextRenderTextureFormat, RenderTextureReadWrite.Linear, 1);
        renderTex.autoGenerateMips = true;
        _camera.targetTexture = renderTex;

        // setup material
        _blitMaterial.mainTexture = _tmp.font.atlas;

        // draw the mesh
        Graphics.DrawMesh(mesh, _tmp.renderer.localToWorldMatrix, _blitMaterial, 0, _camera, 0);
        _camera.Render();

        yield return null;

        RenderTexture.active = renderTex;
        texture.ReadPixels(new Rect(0, 0, width, height), 0, 0, true);
        texture.Apply(false, true);
        
        RenderTexture.ReleaseTemporary(renderTex);
    }
}
