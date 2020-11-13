using System.Collections.Generic;
using System.Text;

namespace ConformalDecals.Text {
    public static class TextEncoder {
        private static readonly Dictionary<string, string> _escapeSequences = new Dictionary<string, string>() {
            {"\n", "\\n"},
            {"\\", "\\\\"},
            {"/", "\\/"},
            {"=", "\\="}
        };

        public static string Encode(string input) {
            var builder = new StringBuilder(input);
            foreach (var escapePair in _escapeSequences) {
                builder.Replace(escapePair.Key, escapePair.Value);
            }

            return builder.ToString();
        }

        public static string Decode(string input) {
            var builder = new StringBuilder(input);
            foreach (var escapePair in _escapeSequences) {
                builder.Replace(escapePair.Value, escapePair.Key);
            }

            return builder.ToString();
        }
    }
}