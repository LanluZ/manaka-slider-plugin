using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using BepInEx;
using BepInEx.Logging;
using BepInEx.Unity.IL2CPP;
using HarmonyLib;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.UI;

namespace SliderWatch
{
    internal static class PluginInfo
    {
        public const string Guid = "com.lanluz.sliderplugin";
        public const string Name = "SliderWatch";
        public const string Version = "1.2.0";
    }

    [BepInPlugin(PluginInfo.Guid, PluginInfo.Name, PluginInfo.Version)]
    public class Plugin : BasePlugin
    {
        internal static ManualLogSource LogSrc;
        internal static readonly Dictionary<int, (float min, float max)> IdToRange = new Dictionary<int, (float min, float max)>();

        public override void Load()
        {
            LogSrc = Log;
            LoadRanges();
            new Harmony(PluginInfo.Guid).PatchAll();
            LogSrc.LogInfo($"{PluginInfo.Name} loaded");
        }

        private static void LoadRanges()
        {
            try
            {
                var pluginDir = Path.Combine(Paths.PluginPath, "SliderWatch");
                var jsonPath = Path.Combine(pluginDir, "data.json");

                if (!File.Exists(jsonPath))
                {
                    LogSrc.LogWarning($"data.json not found at: {jsonPath}. Range forcing will be skipped.");
                    return;
                }

                var json = File.ReadAllText(jsonPath);
                var entries = JsonConvert.DeserializeObject<List<RangeEntry>>(json);
                if (entries == null || entries.Count == 0)
                {
                    LogSrc.LogWarning("data.json parsed but no entries found.");
                    return;
                }

                int add = 0, dup = 0;
                foreach (var e in entries)
                {
                    if (e == null) continue;
                    if (IdToRange.ContainsKey(e.id)) dup++;
                    IdToRange[e.id] = (e.min, e.max);
                    add++;
                }

                LogSrc.LogInfo($"Loaded {add} range entries from data.json (duplicates overwritten: {dup}).");
            }
            catch (Exception ex)
            {
                LogSrc.LogError($"Failed to load ranges from data.json: {ex}");
            }
        }

        private class RangeEntry
        {
            public int id;
            public string description;
            public float min;
            public float max;
        }
    }

    [HarmonyPatch(typeof(Slider))]
    internal static class SliderPatches
    {
        // 判断前后是否变化
        [HarmonyPrefix]
        [HarmonyPatch("set_value")]
        private static void Prefix(Slider __instance, float value, ref bool __state)
        {
            // 与设置前的当前值比较
            __state = !Mathf.Approximately(__instance.value, value);
        }

        [HarmonyPostfix]
        [HarmonyPatch("set_value")]
        private static void Postfix(Slider __instance, float value, bool __state)
        {
            if (!__state) return;

            try
            {
                string name = __instance.gameObject ? __instance.gameObject.name : "<unnamed>";
                string parent = GetAncestorName(__instance, 1) ?? "<no-parent>";
                float min = __instance.minValue;
                float max = __instance.maxValue;
                float cur = __instance.value;

                // Plugin.LogSrc?.LogInfo($"[Slider changed] name={name}, parent={parent}, value={cur}, min={min}, max={max}");
                // 父级名称为 CustomizeBarItemn (id)匹配
                if (TryGetDirectParentId(__instance, out int matchedId) &&
                    Plugin.IdToRange.TryGetValue(matchedId, out var range))
                {
                    float targetMin = range.min;
                    float targetMax = range.max;

                    if (!Mathf.Approximately(__instance.minValue, targetMin) ||
                        !Mathf.Approximately(__instance.maxValue, targetMax))
                    {
                        __instance.minValue = targetMin;
                        __instance.maxValue = targetMax;
                        Plugin.LogSrc?.LogInfo($"[Slider range forced] name={name}, parentId={matchedId}, newMin={targetMin}, newMax={targetMax}");
                    }
                }
            }
            catch (Exception e)
            {
                Plugin.LogSrc?.LogError($"Failed to process slider change: {e}");
            }
        }

        [HarmonyPostfix]
        [HarmonyPatch("set_minValue")]
        private static void OnMinChanged(Slider __instance, float value)
        {
            try
            {
                string name = __instance.gameObject ? __instance.gameObject.name : "<unnamed>";
                Plugin.LogSrc?.LogInfo($"[Slider.minValue changed] name={name}, min={__instance.minValue}, max={__instance.maxValue}, value={__instance.value}");
            }
            catch (Exception e)
            {
                Plugin.LogSrc?.LogError($"Failed logging min change: {e}");
            }
        }

        [HarmonyPostfix]
        [HarmonyPatch("set_maxValue")]
        private static void OnMaxChanged(Slider __instance, float value)
        {
            try
            {
                string name = __instance.gameObject ? __instance.gameObject.name : "<unnamed>";
                Plugin.LogSrc?.LogInfo($"[Slider.maxValue changed] name={name}, min={__instance.minValue}, max={__instance.maxValue}, value={__instance.value}");
            }
            catch (Exception e)
            {
                Plugin.LogSrc?.LogError($"Failed logging max change: {e}");
            }
        }

        private static string GetAncestorName(Slider s, int upLevels)
        {
            try
            {
                var t = s ? s.transform : null;
                while (upLevels-- > 0 && t != null) t = t.parent;
                return t && t.gameObject ? t.gameObject.name : null;
            }
            catch { return null; }
        }

        // 解析父级
        private static bool TryGetDirectParentId(Slider s, out int id)
        {
            id = 0;
            try
            {
                var t = s ? s.transform : null;
                var parent = t != null ? t.parent : null;
                var name = parent && parent.gameObject ? parent.gameObject.name : null;
                if (string.IsNullOrEmpty(name)) return false;

                var m = Regex.Match(name, @"^CustomizeBarItem \((\d+)\)$", RegexOptions.CultureInvariant);
                if (!m.Success) return false;

                if (int.TryParse(m.Groups[1].Value, out id))
                    return true;
            }
            catch { /* ignore */ }

            return false;
        }
    }
}