using System.Collections.Generic;
using UnityEngine;

namespace ConformalDecals {
    [KSPAddon(KSPAddon.Startup.Instantly, true)]
    public class ConformalDecalConfig : MonoBehaviour {
        private static List<string> _shaderBlacklist;

        public static bool IsBlacklisted(Shader shader) {
            return IsBlacklisted(shader.name);
        }

        public static bool IsBlacklisted(string shaderName) {
            return _shaderBlacklist.Contains(shaderName);
        }

        private void ParseConfig(ConfigNode node) {
            foreach (var blacklist in node.GetNodes("SHADERBLACKLIST")) {
                foreach (var shaderName in blacklist.GetValuesList("shader")) {
                    _shaderBlacklist.Add(shaderName);
                }
            }
        }

        private void Start() {
            _shaderBlacklist = new List<string>();

            var configs = GameDatabase.Instance.GetConfigs("CONFORMALDECALS");

            if (configs.Length > 0) {
                Debug.Log("DecalConfig loading config");
                foreach (var config in configs) {
                    ParseConfig(config.config);
                }
            }
        }
    }
}