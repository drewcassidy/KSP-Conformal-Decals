using UnityEngine;

namespace ConformalDecals.Util {
    public static class ColorUtil {
        /// Returns an RGBA 32-bit hex string 
        public static string ToHexString(this Color32 color) {
            return $"#{color.r:x2}{color.g:x2}{color.b:x2}{color.a:x2}";
        }

        // Returns an RGBA 32-bit unsigned integer representation of the color
        public static uint ToUint(this Color32 color) {
            uint rgba = color.r;
            rgba <<= 8;
            rgba |= color.g;
            rgba <<= 8;
            rgba |= color.b;
            rgba <<= 8;
            rgba |= color.a;
            return rgba;
        }
    }
}