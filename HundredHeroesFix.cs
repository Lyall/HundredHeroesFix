using BepInEx;
using BepInEx.Unity.IL2CPP;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using FieldStage.UI;
using UnityEngine.UI;
using Common.UI;
using Kaeru.UI;
using GameData;
using System;

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

        // Graphical Tweaks
        public static ConfigEntry<bool> bGraphicalTweaks;
        public static ConfigEntry<float> fRenderScale;
        public static ConfigEntry<float> fShadowDistance;
        public static ConfigEntry<int> iShadowResolution;

        // Aspect Ratio
        public static float fAspectRatio;
        public static float fAspectMultiplier;
        public static float fNativeAspect = (float)16 / 9;
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

            // Graphical Tweaks
            bGraphicalTweaks = Config.Bind("Graphical Tweaks",
                                "Enabled",
                                true,
                                "Enables tweaking various graphical options.");

            fRenderScale = Config.Bind("Graphical Tweaks",
                                "RenderScale",
                                (float)1f,
                                new ConfigDescription("Set Render Scale. Higher than 1 downsamples and lower than 1 upsamples.",
                                new AcceptableValueRange<float>(0.5f, 4f)));

            fShadowDistance = Config.Bind("Graphical Tweaks",
                                "ShadowDistance",
                                (float)150f,
                                new ConfigDescription("Set distance of shadow rendering. Default High = 150",
                                new AcceptableValueRange<float>(15f, 500f)));

            iShadowResolution = Config.Bind("Graphical Tweaks",
                                "ShadowResolution",
                                (int)3,
                                new ConfigDescription("Set shadow resolution. 1 = 256, 2 = 512, 3 = 1024, 4 = 2048, 5 = 4096",
                                new AcceptableValueRange<int>(1, 5)));

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
            if (bGraphicalTweaks.Value)
            {
                Harmony.CreateAndPatchAll(typeof(GraphicsTweakPatch));
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
            // Apply resolution
            [HarmonyPatch(typeof(Screen), nameof(Screen.SetResolution), new Type[] { typeof(int), typeof(int), typeof(bool) })]
            [HarmonyPrefix]
            public static bool ApplyResolution(Screen __instance, ref int __0, ref int __1, ref bool __2)
            {
                __0 = iCustomResX.Value;
                __1 = iCustomResY.Value;
                __2 = bFullscreen.Value;
                Log.LogInfo($"Applied custom resolution of {iCustomResX.Value}x{iCustomResY.Value} : Fullscreen = {bFullscreen.Value}");
                return true;
            }

            // Disable camera viewport from being adjusted
            [HarmonyPatch(typeof(CameraViewPortFitting), nameof(CameraViewPortFitting.AdjustCameraViewport))]
            [HarmonyPrefix]
            public static bool FixAspectRatio(CameraViewPortFitting __instance)
            {
                // There's possibly a better way of doing this.
                return false;
            }

            // Offset and span the add unit screen
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

            // Fix broken screens that are zoomed in
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

            // Span screen fades
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

            // Adjust vignette
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

            // Span several backgrounds
            [HarmonyPatch(typeof(Image), nameof(Image.OnEnable))]
            [HarmonyPostfix]
            public static void FilterFix(Image __instance)
            {
                if (fAspectRatio > fNativeAspect)
                {
                    if (__instance.gameObject.name == "filter" || __instance.gameObject.name == "Filter" || __instance.gameObject.name == "blackSheet")
                    {
                        var transform = __instance.gameObject.GetComponent<RectTransform>();
                        if (transform.sizeDelta == new Vector2(1920f, 1080f) || transform.sizeDelta == new Vector2(2000f, 1200f))
                        {
                            transform.sizeDelta = new Vector2(1080f * fAspectRatio, 1080f);
                            Log.LogInfo($"Adjusted the size of {__instance.gameObject.transform.parent.gameObject.name}->{__instance.gameObject.name}");
                        }
                    }
                }
            }

            // Span background blur
            [HarmonyPatch(typeof(UIBackgroundBlur), nameof(UIBackgroundBlur.OnEnable))]
            [HarmonyPostfix]
            public static void MapBackground(UIBackgroundBlur __instance)
            {
                if (fAspectRatio > fNativeAspect)
                {
                    var transform = __instance.GetComponent<RectTransform>();
                    if (transform.sizeDelta == new Vector2(1920f, 1080f))
                    {
                        transform.sizeDelta = new Vector2(1080f * fAspectRatio, 1080f);
                        Log.LogInfo($"Adjusted the size of background blur");
                    }
                }
            }

            // Span tutorial background
            [HarmonyPatch(typeof(Tutorial), nameof(Tutorial.RefleshPage))]
            [HarmonyPostfix]
            public static void TutorialBackground(Tutorial __instance)
            {
                if (fAspectRatio > fNativeAspect)
                {
                    if (__instance.gameObject.transform.GetChild(0) != null)
                    {
                        var transform = __instance.gameObject.transform.GetChild(0).GetComponent<RectTransform>();
                        transform.sizeDelta = new Vector2(10000f, 10000f); // This one is already 3000x3000 so it's basically a giant square.
                        Log.LogInfo($"Adjusted the size of tutorial background");
                    }
                }
            }

            // Span screen wipes
            [HarmonyPatch(typeof(Transition.ScreenFadeSetting), nameof(Transition.ScreenFadeSetting.Play))]
            [HarmonyPostfix]
            public static void ScreenWipes(Transition.ScreenFadeSetting __instance)
            {
                if (fAspectRatio > fNativeAspect)
                {
                    if (__instance._fader != null)
                    {
                        var transform = __instance._fader.transform.parent.GetComponent<Transform>();
                        transform.localScale = new Vector3(1.78f * fAspectMultiplier, 1.78f, 1.78f);
                        Log.LogInfo($"Adjusted the size of screen transitions.");
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

        [HarmonyPatch]
        public class GraphicsTweakPatch
        {
            [HarmonyPatch(typeof(DisplaySetting), nameof(DisplaySetting.UpdateShadowOption))]
            [HarmonyPostfix]
            public static void GraphicalTweaks()
            {
                Log.LogInfo($"Applying graphical tweaks");
                var URPAsset = UniversalRenderPipeline.asset;
                if (URPAsset != null)
                {
                    URPAsset.renderScale = fRenderScale.Value;
                    URPAsset.shadowDistance = fShadowDistance.Value; // 150f high

                    var shadowRes = iShadowResolution.Value switch
                    {
                        1 => UnityEngine.Rendering.Universal.ShadowResolution._256,
                        2 => UnityEngine.Rendering.Universal.ShadowResolution._512,
                        3 => UnityEngine.Rendering.Universal.ShadowResolution._1024,
                        4 => UnityEngine.Rendering.Universal.ShadowResolution._2048,
                        5 => UnityEngine.Rendering.Universal.ShadowResolution._4096,
                        _ => UnityEngine.Rendering.Universal.ShadowResolution._1024,
                    };
                    URPAsset.m_MainLightShadowmapResolution = shadowRes; // 1024 high
                }

            }
        }
    }
}