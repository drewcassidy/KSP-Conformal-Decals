using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Rendering;

namespace ConformalDecals.Text {
    [KSPAddon(KSPAddon.Startup.FlightAndEditor, true)]
    public class TextRenderer : MonoBehaviour {
        public static TextRenderer Instance {
            get {
                if (!_instance._isSetup) {
                    _instance.Setup();
                }

                return _instance;
            }
        }

        public const TextureFormat       TextTextureFormat       = TextureFormat.Alpha8;
        public const RenderTextureFormat TextRenderTextureFormat = RenderTextureFormat.R8;

        private const string BlitShader     = "ConformalDecals/TMP_Blit";
        private const int    MaxTextureSize = 4096;

        private static TextRenderer _instance;

        private bool        _isSetup;
        private TextMeshPro _tmp;
        private GameObject  _cameraObject;
        private Camera      _camera;
        private Material    _blitMaterial;

        private void Start() {
            if (_instance._isSetup) {
                Debug.Log("[ConformalDecals] Duplicate TextRenderer created???");
            }

            Debug.Log("[ConformalDecals] Creating TextRenderer Object");
            _instance = this;
            DontDestroyOnLoad(gameObject);
        }

        public void Setup() {
            if (_isSetup) return;


            Debug.Log("[ConformalDecals] Setting Up TextRenderer Object");

            _tmp = gameObject.AddComponent<TextMeshPro>();
            _tmp.renderer.enabled = false; // dont automatically render

            _cameraObject = new GameObject("ConformalDecals text camera");
            _cameraObject.transform.parent = transform;
            _cameraObject.transform.SetPositionAndRotation(Vector3.back, Quaternion.identity);

            _camera = _cameraObject.AddComponent<Camera>();
            _camera.enabled = false; // dont automatically render
            _camera.orthographic = true;
            _camera.depthTextureMode = DepthTextureMode.None;
            _camera.nearClipPlane = 0.1f;
            _camera.farClipPlane = 2f;
            _isSetup = true;

            _blitMaterial = new Material(Shabby.Shabby.FindShader(BlitShader));
        }
    }
}