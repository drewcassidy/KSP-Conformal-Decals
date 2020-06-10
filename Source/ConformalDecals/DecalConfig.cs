using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.Rendering;

namespace ConformalDecals {
    public static class DecalConfig {
        private static Texture2D _blankNormal;
        private static List<string> _shaderBlacklist;
        
        public static Texture2D BlankNormal => _blankNormal;

        public static bool IsBlacklisted(Shader shader) {
            return IsBlacklisted(shader.name);
        }

        public static bool IsBlacklisted(string shaderName) {
            return _shaderBlacklist.Contains(shaderName);
        }

        private static void ParseConfig(ConfigNode node) {
            foreach (var blacklist in node.GetNodes("SHADERBLACKLIST")) {
                foreach (var shaderName in blacklist.GetValuesList("shader")) {
                    _shaderBlacklist.Add(shaderName);
                }
            }
        }

        private static Texture2D MakeBlankNormal() {
            Debug.Log("ConformalDecals: Generating neutral normal map texture");
            var width = 2;
            var height = 2;
            var color = new Color32(255, 128, 128, 128);
            var colors = new Color32[] { color, color, color, color };

            var tex = new Texture2D(width, height, TextureFormat.RGBA32, false);
            for (var x = 0; x <= width; x++) {
                for (var y = 0; y < height; y++) {
                    tex.SetPixels32(colors);
                }
            }
            tex.Apply();

            return tex;
        }

        public static void ModuleManagerPostLoad() {
            _shaderBlacklist = new List<string>();

            var configs = GameDatabase.Instance.GetConfigs("CONFORMALDECALS");

            if (configs.Length > 0) {
                Debug.Log("ConformalDecals: loading config");
                foreach (var config in configs) {
                    ParseConfig(config.config);
                }
            }

            _blankNormal = MakeBlankNormal();
        }
    }
}