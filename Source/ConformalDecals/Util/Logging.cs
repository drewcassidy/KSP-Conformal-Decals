using System;
using UnityEngine;

namespace ConformalDecals.Util {
    public static class Logging {
        public static void Log(string message) => Debug.Log("[ConformalDecals] " + message);

        public static void Log(this PartModule module, string message) => Debug.Log(FormatMessage(module, message));

        public static void LogWarning(string message) => Debug.LogWarning("[ConformalDecals] " + message);

        public static void LogWarning(this PartModule module, string message) => Debug.LogWarning(FormatMessage(module, message));

        public static void LogError(string message) => Debug.LogError("[ConformalDecals] " + message);

        public static void LogError(this PartModule module, string message) => Debug.LogError(FormatMessage(module, message));

        public static void LogException(string message, Exception exception) => Debug.LogException(new Exception("[ConformalDecals]  " + message, exception));

        public static void LogException(this PartModule module, string message, Exception exception) =>
            Debug.LogException(new Exception(FormatMessage(module, message), exception));

        private static string FormatMessage(PartModule module, string message) =>
            $"[{GetPartName(module.part)} {module.GetType()}] {message}";

        private static string GetPartName(Part part) => part.partInfo?.name ?? part.name;
    }
}