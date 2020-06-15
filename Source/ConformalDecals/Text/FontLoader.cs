using System.IO;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace ConformalDecals.Text {
    
    [DatabaseLoaderAttrib(new[] {"kspfont"})]
    public class FontLoader : DatabaseLoader<GameDatabase.TextureInfo> {
        public static List<TMP_FontAsset> fonts;
        
        public override IEnumerator Load(UrlDir.UrlFile urlFile, FileInfo fileInfo) {
            fonts ??= new List<TMP_FontAsset>();
            
            Debug.Log($"[ConformalDecals] '{urlFile.fullPath}'");
            var bundle = AssetBundle.LoadFromFile(urlFile.fullPath);
            if (!bundle) {
                Debug.Log($"[ConformalDecals] could not load font asset {urlFile.fullPath}");
            }
            else {
                var loadedFoo = bundle.LoadAllAssets<DecalFont>();
                Debug.Log(loadedFoo[0].foo1);
                Debug.Log(loadedFoo[0].foo2);
                var loadedFonts = bundle.LoadAllAssets<TMP_FontAsset>();
                foreach (var font in loadedFonts) {
                    Debug.Log($"[ConformalDecals] adding font {font.name}" );
                    fonts.Add(font);
                    Debug.Log($"ConformalDecals] isReadable: {font.atlas.isReadable}");
                }
            }
            
            yield break;
        }
    }
}