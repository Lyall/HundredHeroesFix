using BepInEx;
using BepInEx.Unity.IL2CPP;
using BepInEx.Configuration;
using BepInEx.Logging;

using HarmonyLib;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using FieldStage.UI;
using UnityEngine.UI;

namespace HundredHeroesFix
{
    [BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
    public class HHFix : BasePlugin
    {
        internal static new ManualLogSource Log;

        // Custom Resolution
        public static ConfigEntry<bool> bCustomRes;
        public static ConfigEntry<int> iCustomResX;
        public static ConfigEntry<int> iCustomResY;
        public static ConfigEntry<bool> bFullscreen;

        // Features
        public static ConfigEntry<bool> bSkipIntroLogos;
        public static ConfigEntry<bool> bControllerGlyphs;
        public static ConfigEntry<int> iControllerStyle;
        // Aspect Ratio
        public static float fAspectRatio;
        public static float fAspectMultiplier;
        public static float fNativeAspect = (float)16/9;
        public static float fNativeWidth;
        public static float fNativeHeight;
        public static float fHUDWidth;
        public static float fHUDWidthOffset;
        public static float fHUDHeight;
        public static float fHUDHeightOffset;

        public override void Load()
        {
            // Plugin startup logic
            Log = base.Log;
            Log.LogInfo($"Plugin {MyPluginInfo.PLUGIN_GUID} is loaded!");

            // Custom Resolution
            bCustomRes = Config.Bind("Set Custom Resolution",
                                "Enabled",
                                true,
                                "Enables the usage of a custom resolution.");

            iCustomResX = Config.Bind("Set Custom Resolution",
                                "ResolutionWidth",
                                Display.main.systemWidth,
                                "Set desired resolution width.");

           iCustomResY = Config.Bind("Set Custom Resolution",
                                "ResolutionHeight",
                                Display.main.systemHeight,
                                "Set desired resolution height.");

            bFullscreen = Config.Bind("Set Custom Resolution",
                                "Fullscreen",
                                 true,
                                "Set to true for fullscreen or false for windowed.");

            // Features
            bSkipIntroLogos = Config.Bind("Intro Skip",
                                "SkipLogos",
                                true,
                                "Skips intro logos.");

            bControllerGlyphs = Config.Bind("Force Controller Icons",
                                "Enabled",
                                false,
                                "Allows forcing desired controller button icons.");


            iControllerStyle = Config.Bind("Force Controller Icons",
                                "IconStyle",
                                 (int)1,
                                new ConfigDescription("Set controller icon style. 1 = Dualshock (DS4), 2 = DualSense (DS5), 3 = Xbox",
                                new AcceptableValueRange<int>(1, 3)));

            // Calculate aspect ratio
            fAspectRatio = (float)iCustomResX.Value / iCustomResY.Value;
            fAspectMultiplier = fAspectRatio / fNativeAspect;
            fNativeWidth = (float)iCustomResY.Value * fNativeAspect;
            fNativeHeight = (float)iCustomResX.Value / fNativeAspect;

            // HUD variables
            fHUDWidth = (float)iCustomResY.Value * fNativeAspect;
            fHUDHeight = (float)iCustomResY.Value;
            fHUDWidthOffset = (float)(iCustomResX.Value - fHUDWidth) / 2;
            fHUDHeightOffset = 0;
            if (fAspectRatio < fNativeAspect)
            {
                fHUDWidth = (float)iCustomResX.Value;
                fHUDHeight = (float)iCustomResX.Value / fNativeAspect;
                fHUDWidthOffset = 0;
                fHUDHeightOffset = (float)(iCustomResX.Value - fHUDHeight) / 2;
            }

            // Apply patches
            if (bSkipIntroLogos.Value)
            {
                Harmony.CreateAndPatchAll(typeof(SkipIntroPatch));
            }
            if (bCustomRes.Value)
            {
                Harmony.CreateAndPatchAll(typeof(ResolutionPatch));
            }
            if (bControllerGlyphs.Value)
            {
                Harmony.CreateAndPatchAll(typeof(ControllerGlyphPatch));
            }
        }

        [HarmonyPatch]
        public class SkipIntroPatch
        {
            // Skip intro
            [HarmonyPatch(typeof(UI.Title.TitleLogoSequenceController), nameof(UI.Title.TitleLogoSequenceController.FadeInOut))]
            [HarmonyPrefix]
            public static void LogoSkip(UI.Title.TitleLogoSequenceController __instance, ref CanvasGroup __0, ref float __1, ref float __2, ref float __3)
            {
                if (bSkipIntroLogos.Value)
                {
                    __1 = 0.0001f;
                    __2 = 0.0001f;
                    __3 = 0.0001f;
                    Log.LogInfo($"Skipping intro logos.");
                }
            }
        }

        [HarmonyPatch]
        public class ResolutionPatch
        {
            // Resolution
            [HarmonyPatch(typeof(UnityEngine.Screen), nameof(UnityEngine.Screen.SetResolution), [typeof(int), typeof(int), typeof(bool)])]
            [HarmonyPrefix]
            public static bool ApplyResolution(Screen __instance, ref int __0, ref int __1, ref bool __2)
            {
                __0 = iCustomResX.Value;
                __1 = iCustomResY.Value;
                __2 = bFullscreen.Value;
                Log.LogInfo($"Applied custom resolution of {iCustomResX.Value}x{iCustomResY.Value} : Fullscreen = {bFullscreen.Value}");
                return true;
            }

            [HarmonyPatch(typeof(CameraViewPortFitting), nameof(CameraViewPortFitting.AdjustCameraViewport))]
            [HarmonyPrefix]
            public static bool FixAspectRatio(CameraViewPortFitting __instance)
            {
                // There's possibly a better way of doing this.
                return false;
            }

            [HarmonyPatch(typeof(AddUnit), nameof(AddUnit.Show))]
            [HarmonyPostfix]
            public static void AddUnitPos(AddUnit __instance)
            {
                if (fAspectRatio > fNativeAspect)
                {
                    if (__instance.gameObject.transform.localPosition.x == 0)
                    {
                        float fWidthOffset = (float)((1080 * fAspectRatio) - 1920) / 2;
                        __instance.gameObject.transform.AddLocalPositionX(fWidthOffset);
                    }

                    // Span background elements
                    __instance.transform.GetChild(0).localScale = new Vector3(1f * fAspectMultiplier, 1f, 1f);
                    __instance.transform.GetChild(1).localScale = new Vector3(1f * fAspectMultiplier, 1f, 1f);
                    __instance.transform.GetChild(2).localScale = new Vector3(1f * fAspectMultiplier, 1f, 1f);
                }
            }

            [HarmonyPatch(typeof(CanvasScaler), nameof(CanvasScaler.OnEnable))]
            [HarmonyPostfix]
            public static void LoadScreenFix(CanvasScaler __instance)
            {
                if (fAspectRatio > fNativeAspect)
                {
                    if (__instance != null && __instance.referenceResolution == new Vector2(1920f, 600f))
                    {
                        __instance.referenceResolution = new Vector2(1920f, 1080f);
                        __instance.screenMatchMode = CanvasScaler.ScreenMatchMode.Expand;
                        Log.LogInfo($"Fixed broken screen {__instance.transform.parent.gameObject.name}.");
                    }
                }
            }

            [HarmonyPatch(typeof(Framework.FadeOrganizer), nameof(Framework.FadeOrganizer.Start))]
            [HarmonyPostfix]
            public static void FadeFix(Framework.FadeOrganizer __instance)
            {
                if (fAspectRatio > fNativeAspect)
                {
                    if (__instance._fadeImage != null)
                    {
                        var fadeTransform = __instance._fadeImage.gameObject.GetComponent<RectTransform>();
                        fadeTransform.localScale = new Vector3(1f * fAspectMultiplier, 1f, 1f);
                        Log.LogInfo($"Scaled fade.");
                    }
                }
            }

            [HarmonyPatch(typeof(UnityEngine.Rendering.Volume), nameof(UnityEngine.Rendering.Volume.OnEnable))]
            [HarmonyPostfix]
            public static void VignetteFix(UnityEngine.Rendering.Volume __instance)
            {
                __instance.profile.TryGet(out Vignette vignette);
                if (vignette)
                {
                    if (fAspectRatio > fNativeAspect)
                    {
                        vignette.intensity.value /= fAspectMultiplier;
                        Log.LogInfo($"Changed intestity of {__instance.gameObject.name}'s vignette to {vignette.intensity.value}");
                    }
                }
            }
        }

        [HarmonyPatch]
        public class ControllerGlyphPatch
        {
            // Change controller glyph
            [HarmonyPatch(typeof(InputManager), nameof(InputManager.CheckDevice))]
            [HarmonyPostfix]
            public static void ChangeControllerGlyph(InputManager __instance)
            {
                var controllerStyle = iControllerStyle.Value switch
                {
                    1 => InputManager.InputDeviceType.DualShock,
                    2 => InputManager.InputDeviceType.DualSense,
                    3 => InputManager.InputDeviceType.XInput,
                    _ => InputManager.InputDeviceType.XInput,
                };

                __instance._deviceType = controllerStyle;
            }
        }
    }
}