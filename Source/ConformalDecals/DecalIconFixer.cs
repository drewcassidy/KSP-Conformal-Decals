using System.Collections.Generic;
using ConformalDecals.Util;
using UnityEngine;

namespace ConformalDecals {
    [KSPAddon(KSPAddon.Startup.EditorAny, true)]
    public class DecalIconFixer : MonoBehaviour {
        private static readonly List<string> PartNames = new List<string>();

        public static void QueuePart(string name) {
            PartNames.Add(name);
        }

        public void Start() {
            foreach (var partName in PartNames) {
                Logging.Log($"Unf*&king decal preview on '{partName}'");
                var partInfo = PartLoader.getPartInfoByName(partName);

                if (partInfo == null) {
                    Logging.LogError($"Part {partName} not found!");
                    continue;
                }

                var icon = partInfo.iconPrefab;

                var decalModule = partInfo.partPrefab.FindModuleImplementing<ModuleConformalDecal>();

                var frontTransform = Part.FindHeirarchyTransform(icon.transform, decalModule.decalFront);
                var backTransform = Part.FindHeirarchyTransform(icon.transform, decalModule.decalBack);

                if (frontTransform == null) {
                    Logging.Log($"Part '{partName}' has no frontTransform");
                    continue;
                }

                if (backTransform == null) {
                    Logging.Log($"Part '{partName}' has no backTransform");
                    continue;
                }

                Vector2 backScale = default;
                if (decalModule.updateBackScale) {
                    var aspectRatio = decalModule.materialProperties.AspectRatio;
                    var size = new Vector2(decalModule.scale, decalModule.scale * aspectRatio);
                    backScale.x = size.x * decalModule.backTextureBaseScale.x;
                    backScale.y = size.y * decalModule.backTextureBaseScale.y;
                }

                backTransform.GetComponent<MeshRenderer>().material = decalModule.backMaterial;
                frontTransform.GetComponent<MeshRenderer>().material = decalModule.materialProperties.PreviewMaterial;
                    
                if (decalModule.updateBackScale) {
                    backTransform.GetComponent<MeshRenderer>().material.SetTextureScale(PropertyIDs._MainTex, backScale);
                }
            }
        }
    }
}