using System.IO;
using System.Collections;
using System.Collections.Generic;
using ConformalDecals.Util;
using TMPro;
using UniLinq;
using UnityEngine;

namespace ConformalDecals.Text {
    /// KSP database loader for KSPFont files which contain TextMeshPro font assets
    [DatabaseLoaderAttrib(new[] {"decalfont"})]
    public class FontLoader : DatabaseLoader<GameDatabase.TextureInfo> {
        private const  string        FallbackName = "NotoSans-Regular SDF";

        public static TMP_FontAsset FallbackFont { get; private set; }

        public override IEnumerator Load(UrlDir.UrlFile urlFile, FileInfo fileInfo) {
            if (FallbackFont == null) {
                FallbackFont = Resources.FindObjectsOfTypeAll<TMP_FontAsset>().First(o => o.name == FallbackName);
                if (FallbackFont == null) Logging.LogError($"Could not find fallback font '{FallbackName}'");
            }

            Logging.Log($"Loading font file '{urlFile.fullPath}'");
            var bundle = AssetBundle.LoadFromFile(urlFile.fullPath);
            if (!bundle) {
                Logging.Log($"Could not load font asset {urlFile.fullPath}");
            }
            else {
                var loadedFonts = bundle.LoadAllAssets<TMP_FontAsset>();
                foreach (var font in loadedFonts) {
                    Logging.Log($"Adding font {font.name}");
                }
            }

            yield break;
        }
    }
}