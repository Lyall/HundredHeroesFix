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
using FieldStage;
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
        public static ConfigEntry<bool> bSkipOpeningMovie;
        public static ConfigEntry<bool> bDisableCursor;
        public static ConfigEntry<bool> bControllerGlyphs;
        public static ConfigEntry<int> iControllerStyle;
        public static ConfigEntry<bool> bAutoDialogDelay;

        // Graphical Tweaks
        public static ConfigEntry<bool> bGraphicalTweaks;
        public static ConfigEntry<bool> bVsync;
        public static ConfigEntry<float> fRenderScale;
        public static ConfigEntry<int> iAnisotropicFiltering;
        public static ConfigEntry<float> fShadowDistance;
        public static ConfigEntry<int> iShadowResolution;
        public static ConfigEntry<int> iShadowCascades;
        public static ConfigEntry<bool> bEnableSMAA;
        public static ConfigEntry<bool> bChromaticAberration;
        public static ConfigEntry<bool> bVignette;

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

        // Variables
        public static bool bHasSkippedOpeningMovie;

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

            bVsync = Config.Bind("Vsync",
                                "Enabled",
                                true,
                                "Set vsync on/off");

            bSkipIntroLogos = Config.Bind("Intro Skip",
                                "SkipLogos",
                                true,
                                "Skips intro logos.");

            bSkipOpeningMovie = Config.Bind("Intro Skip",
                                "SkipOpeningMovie",
                                true,
                                "Skips opening movie.");

            bDisableCursor = Config.Bind("Disable Cursor",
                                "Enabled",
                                true,
                                "Set to true to disable showing the mouse cursor.");

            bAutoDialogDelay = Config.Bind("Disable Dialog Auto-Advance Delay",
                                "Enabled",
                                true,
                                "Removes the forced 2-second delay on auto-advancing voiced dialog.");

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
                                false,
                                "Enables tweaking various graphical options.");

            fRenderScale = Config.Bind("Graphical Tweaks",
                                "RenderScale",
                                (float)1f,
                                new ConfigDescription("Set Render Scale. Higher than 1 downsamples and lower than 1 upsamples.",
                                new AcceptableValueRange<float>(0.1f, 2f)));

            iAnisotropicFiltering = Config.Bind("Graphical Tweaks",
                                "AnisotropicFiltering",
                                (int)16,
                                new ConfigDescription("Set Anisotropic Filtering level.",
                                new AcceptableValueRange<int>(1, 16)));

            fShadowDistance = Config.Bind("Graphical Tweaks",
                                "ShadowDistance",
                                (float)150f,
                                new ConfigDescription("Set distance of shadow rendering. Default High = 150",
                                new AcceptableValueRange<float>(15f, 500f)));

            iShadowResolution = Config.Bind("Graphical Tweaks",
                                "ShadowResolution",
                                (int)5,
                                new ConfigDescription("Set shadow resolution. 1 = 256, 2 = 512, 3 = 1024, 4 = 2048, 5 = 4096, 6 = 8192. Default High = 3",
                                new AcceptableValueRange<int>(1, 6)));

            iShadowCascades = Config.Bind("Graphical Tweaks",
                                "ShadowCascades",
                                (int)4,
                                new ConfigDescription("Set number of shadow cascades. Default High = 3",
                                new AcceptableValueRange<int>(1, 4)));

            bEnableSMAA = Config.Bind("Graphical Tweaks",
                                "AntiAliasing",
                                true,
                                "Enables high quality post-process SMAA.");

            bChromaticAberration = Config.Bind("Graphical Tweaks",
                                "DisableChromaticAberration",
                                false,
                                "Set to true to disable chromatic abberation (colour fringing at edges of screen).");

            bVignette = Config.Bind("Graphical Tweaks",
                                "DisableVignette",
                                false,
                                "Set to true to disable vignette (darkening at edges of screen).");

            // Calculate aspect ratio
            fAspectRatio = (float)iCustomResX.Value / iCustomResY.Value;
            fAspectMultiplier = fAspectRatio / fNativeAspect;

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
                fHUDHeightOffset = (float)(iCustomResY.Value - fHUDHeight) / 2;
            }

            // Log HUD variables/aspect ratio
            Log.LogInfo($"HUD Variables: Resolution = {iCustomResX.Value}x{iCustomResY.Value}");
            Log.LogInfo($"HUD Variables: fAspectRatio = {fAspectRatio}");
            Log.LogInfo($"HUD Variables: fAspectMultiplier = {fAspectMultiplier}");
            Log.LogInfo($"HUD Variables: fHUDWidth = {fHUDWidth}");
            Log.LogInfo($"HUD Variables: fHUDHeight = {fHUDHeight}"); 
            Log.LogInfo($"HUD Variables: fHUDWidthOffset = {fHUDWidthOffset}");
            Log.LogInfo($"HUD Variables: fHUDHeightOffset = {fHUDHeightOffset}");

            // Apply patches
            Log.LogInfo($"Patches: Applying resolution patch.");
            Harmony.CreateAndPatchAll(typeof(ResolutionPatch));
            Log.LogInfo($"Patches: Applying miscellaneous patch.");
            Harmony.CreateAndPatchAll(typeof(MiscPatch));

            if (fAspectRatio != fNativeAspect)
            {
                Log.LogInfo($"Patches: Applying aspect ratio patch.");
                Harmony.CreateAndPatchAll(typeof(AspectRatioPatch));
            }
            if (bSkipIntroLogos.Value || bSkipOpeningMovie.Value)
            {
                Log.LogInfo($"Patches: Applying skip intro patch.");
                Harmony.CreateAndPatchAll(typeof(SkipIntroPatch));
            }
            if (bControllerGlyphs.Value)
            {
                Log.LogInfo($"Patches: Applying controller glyph patch.");
                Harmony.CreateAndPatchAll(typeof(ControllerGlyphPatch));
            }
            if (bGraphicalTweaks.Value)
            {
                Log.LogInfo($"Patches: Applying graphical tweaks patch.");
                Harmony.CreateAndPatchAll(typeof(GraphicsTweakPatch));
            }
            if (bDisableCursor.Value)
            {
                Cursor.visible = false;
                Log.LogInfo($"Disabled mouse cursor visibility.");
            }
        }

        [HarmonyPatch]
        public class SkipIntroPatch
        {
            // Skip intro logos
            [HarmonyPatch(typeof(UI.Title.TitleLogoSequenceController), nameof(UI.Title.TitleLogoSequenceController.FadeInOut))]
            [HarmonyPrefix]
            public static void LogoSkip(UI.Title.TitleLogoSequenceController __instance, ref CanvasGroup __0, ref float __1, ref float __2, ref float __3)
            {
                if (bSkipIntroLogos.Value)
                {
                    __1 = 0.0001f;
                    __2 = 0.0001f;
                    __3 = 0.0001f;
                    Log.LogInfo($"Intro Skip: Skipping intro logos.");
                }
            }

            // Skip opening movie
            [HarmonyPatch(typeof(UI.Title.TitleCanvas), nameof(UI.Title.TitleCanvas.IsOpeningMovieStarted))]
            [HarmonyPostfix]
            public static void OpeningMovieSkip(UI.Title.TitleCanvas __instance, ref bool __result)
            {
                if (__result == true && bSkipOpeningMovie.Value && !bHasSkippedOpeningMovie)
                {
                    __instance.Stop();
                    // Only skip it the first time in case someone wants to idle the main menu and watch it again I guess?
                    bHasSkippedOpeningMovie = true;
                    Log.LogInfo($"Intro Skip: Skipped opening movie.");
                }
            }
        }

        [HarmonyPatch]
        public class MiscPatch
        {
            // Remove 2 second delay from auto-advancing dialogue
            [HarmonyPatch(typeof(TextData.UI.KaeruText), nameof(TextData.UI.KaeruText.AutomaticSubmit))]
            [HarmonyPrefix]
            public static void RemoveDialogueDelay(TextData.UI.KaeruText __instance, ref float __0)
            {
                var sndMngr = SoundManager.Instance;
                // Only remove dialogue delay for voiced lines
                if (bAutoDialogDelay.Value && sndMngr.UseEventSE)
                {
                    // 100ms delay seems about right?
                    __0 = 0.1f;
                }
            }

            // Enable skippable intro
            [HarmonyPatch(typeof(UI.Title.Context), nameof(UI.Title.Context.Initialize))]
            [HarmonyPrefix]
            public static void LogoSkip(UI.Title.Context __instance)
            {
                if (__instance != null)
                {
                    __instance.ChangeLogoSkipEnable(true);
                    Log.LogInfo($"Intro Skip: Enabled skippable logos.");
                }
            }

            // Set vsync/target framerate
            [HarmonyPatch(typeof(DisplaySetting), nameof(DisplaySetting.UpdateFrameRate))]
            [HarmonyPostfix]
            public static void SetVSync()
            {
                if (bVsync.Value)
                {
                    QualitySettings.vSyncCount = 1;
                    Log.LogInfo($"Misc: Enabled VSync.");
                }
                else if (!bVsync.Value)
                {
                    QualitySettings.vSyncCount = 0;
                    Log.LogInfo($"Misc: Disabled VSync.");
                }

                // Uncap framerate when vsync is disabled. Has no effect with VSync on.
                Application.targetFrameRate = 0;
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
                if (bCustomRes.Value)
                {
                    __0 = iCustomResX.Value;
                    __1 = iCustomResY.Value;
                    __2 = bFullscreen.Value;
                    Log.LogInfo($"Resolution: Applied custom resolution of {iCustomResX.Value}x{iCustomResY.Value} : Fullscreen = {bFullscreen.Value}");
                    return true;
                }
                return true;
            }
        }

        [HarmonyPatch]
        public class AspectRatioPatch
        {
            // Disable camera viewport from being adjusted
            [HarmonyPatch(typeof(CameraViewPortFitting), nameof(CameraViewPortFitting.AdjustCameraViewport))]
            [HarmonyPrefix]
            public static bool FixAspectRatio(CameraViewPortFitting __instance)
            {
                // There's possibly a better way of doing this.
                return false;
            }

            // Change main camera background colour and set correct FOV for <16:9
            [HarmonyPatch(typeof(FieldCamera), nameof(FieldCamera.Awake))]
            [HarmonyPostfix]
            public static void AntiAliasing(FieldCamera __instance)
            {
                if (__instance.MainCamera != null)
                {
                    // Set background colour to black instead of blue. Makes the void outside of building stand out less, also good for OLEDs?
                    __instance.MainCamera.backgroundColor = Color.black;
                    Log.LogInfo("AspectRatio: Changed background colour of Main Camera.");

                    if (fAspectRatio < fNativeAspect)
                    {
                        __instance.MainCamera.usePhysicalProperties = true;
                        __instance.MainCamera.gateFit = Camera.GateFitMode.Horizontal;
                        __instance.MainCamera.sensorSize = new Vector2(16f, 9f);
                        Log.LogInfo("AspectRatio: Set Main Camera gate fit mode to horizontal.");
                    }
                }
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
                    __instance.transform.GetChild(0).localScale = new Vector3(1f * fAspectMultiplier, 1f, 1f); // blackBarDown
                    __instance.transform.GetChild(1).localScale = new Vector3(1f * fAspectMultiplier, 1f, 1f); // blackBarUp
                    __instance.transform.GetChild(2).localScale = new Vector3(1f * fAspectMultiplier, 1f, 1f); // blackSheet
                }
                else if (fAspectRatio < fNativeAspect)
                {
                    if (__instance.gameObject.transform.localPosition.y == 0)
                    {
                        float fHeightOffset = (float)((1920 / fAspectRatio) - 1080) / 2;
                        __instance.gameObject.transform.AddLocalPositionY(-fHeightOffset);
                    }

                    // Span background elements
                    float fPosAnchorOffset = (float)1f + (fHUDHeightOffset / iCustomResY.Value);
                    float fNegAnchorOffset = (float)1f - (fHUDHeightOffset / iCustomResY.Value);
                    __instance.transform.GetChild(0).GetComponent<RectTransform>().anchorMax = new Vector2(0f, fNegAnchorOffset); // blackBarDown
                    __instance.transform.GetChild(0).GetComponent<RectTransform>().anchorMin = new Vector2(0f, fNegAnchorOffset); // blackBarDown
                    __instance.transform.GetChild(1).GetComponent<RectTransform>().anchorMax = new Vector2(0f, fPosAnchorOffset); // blackBarUp
                    __instance.transform.GetChild(1).GetComponent<RectTransform>().anchorMin = new Vector2(0f, fPosAnchorOffset); // blackBarUp
                    __instance.transform.GetChild(2).localScale = new Vector3(1f, 1f / fAspectMultiplier, 1f); ; // blackSheet
                }

                Log.LogInfo($"AspectRatio: Offset and spanned add unit screen.");
            }

            // Offset blacksmith build up screen
            [HarmonyPatch(typeof(BlackSmithWindow), nameof(BlackSmithWindow.Awake))]
            [HarmonyPostfix]
            public static void BlacksmithPos(BlackSmithWindow __instance)
            {
                if (fAspectRatio > fNativeAspect)
                {
                    if (__instance.gameObject != null)
                    {
                        float fWidthOffset = (float)((1080 * fAspectRatio) - 1920) / 2;
                        __instance.gameObject.GetComponent<RectTransform>().AddLocalPositionX(fWidthOffset);

                        if (__instance.gameObject.transform.GetChild(0).gameObject.transform.GetChild(0).gameObject.name == "BGblackLeft")
                        {
                            var localPos = __instance.gameObject.transform.GetChild(0).gameObject.transform.GetChild(0).gameObject.GetComponent<RectTransform>().localPosition;
                            localPos.x -= fWidthOffset;

                            __instance.gameObject.transform.GetChild(0).gameObject.transform.GetChild(0).gameObject.GetComponent<RectTransform>().localPosition = localPos;
                            __instance.gameObject.transform.GetChild(0).gameObject.transform.GetChild(0).gameObject.GetComponent<RectTransform>().localScale = new Vector3(1f * fAspectMultiplier, 1f, 1f);
                        }

                        Log.LogInfo($"AspectRatio: Offset blacksmith buildup screen.");
                    }
                }
            }

            // Offset inn screen
            [HarmonyPatch(typeof(InnCanvas), nameof(InnCanvas.Open))]
            [HarmonyPostfix]
            public static void InnCanvasPos(InnCanvas __instance)
            {
                if (__instance._header != null && __instance._dialogRoot != null)
                {
                    if (fAspectRatio > fNativeAspect)
                    {
                        float fAnchorOffset = (float)fHUDWidthOffset / iCustomResX.Value;
                        __instance._dialogRoot.gameObject.GetComponent<RectTransform>().anchorMax = new Vector2(fAnchorOffset, 1f);
                        __instance._dialogRoot.gameObject.GetComponent<RectTransform>().anchorMin = new Vector2(fAnchorOffset, 1f);
                        __instance._header.gameObject.GetComponent<RectTransform>().anchorMax = new Vector2(fAnchorOffset, 1f);
                        __instance._header.gameObject.GetComponent<RectTransform>().anchorMin = new Vector2(fAnchorOffset, 1f);
                    }
                    else if (fAspectRatio < fNativeAspect)
                    {
                        float fAnchorOffset = (float)1f - (fHUDHeightOffset / iCustomResY.Value);
                        __instance._dialogRoot.gameObject.GetComponent<RectTransform>().anchorMax = new Vector2(0f, fAnchorOffset);
                        __instance._dialogRoot.gameObject.GetComponent<RectTransform>().anchorMin = new Vector2(0f, fAnchorOffset);
                        __instance._header.gameObject.GetComponent<RectTransform>().anchorMax = new Vector2(0f, fAnchorOffset);
                        __instance._header.gameObject.GetComponent<RectTransform>().anchorMin = new Vector2(0f, fAnchorOffset);
                    }

                    Log.LogInfo($"AspectRatio: Offset inn screen.");
                }
            }

            // Fix broken screens that are zoomed in
            [HarmonyPatch(typeof(CanvasScaler), nameof(CanvasScaler.OnEnable))]
            [HarmonyPostfix]
            public static void LoadScreenFix(CanvasScaler __instance)
            {
                if (fAspectRatio > fNativeAspect)
                {
                    if (__instance != null)
                    {
                        if (__instance.referenceResolution == new Vector2(1920f, 600f) || (__instance.referenceResolution == new Vector2(1920f, 1080f) && __instance.screenMatchMode == CanvasScaler.ScreenMatchMode.MatchWidthOrHeight))
                        {
                            __instance.referenceResolution = new Vector2(1920f, 1080f);
                            __instance.screenMatchMode = CanvasScaler.ScreenMatchMode.Expand;
                            Log.LogInfo($"AspectRatio: Fixed broken screen {__instance.transform.parent.gameObject.name}.");
                        }
                    }
                }
            }

            // Adjust vignette
            [HarmonyPatch(typeof(UnityEngine.Rendering.Volume), nameof(UnityEngine.Rendering.Volume.OnEnable))]
            [HarmonyPostfix]
            public static void VignetteFix(UnityEngine.Rendering.Volume __instance)
            {
                if (fAspectRatio > fNativeAspect)
                {
                    __instance.profile.TryGet(out Vignette vignette);
                    if (vignette)
                    {
                        vignette.intensity.value /= fAspectMultiplier;
                        Log.LogInfo($"AspectRatio: Changed intestity of {__instance.gameObject.name}'s vignette to {vignette.intensity.value}");
                    }
                }
            }

            // Span screen fades
            [HarmonyPatch(typeof(Framework.FadeOrganizer), nameof(Framework.FadeOrganizer.Start))]
            [HarmonyPostfix]
            public static void FadeFix(Framework.FadeOrganizer __instance)
            {
                if (__instance._fadeImage != null)
                {
                    var fadeTransform = __instance._fadeImage.gameObject.GetComponent<RectTransform>();

                    if (fAspectRatio > fNativeAspect)
                    {
                        fadeTransform.localScale = new Vector3(1f * fAspectMultiplier, 1f, 1f);
                    }
                    else if (fAspectRatio < fNativeAspect)
                    {
                        //fadeTransform.localScale = new Vector3(1f, 1f / fAspectMultiplier, 1f);
                        // Not necessary
                    }

                    Log.LogInfo($"AspectRatio: Scaled fade.");
                }
            }

            // Span several backgrounds
            [HarmonyPatch(typeof(Image), nameof(Image.OnEnable))]
            [HarmonyPostfix]
            public static void FilterFix(Image __instance)
            {
                if (__instance.gameObject.name == "filter" || __instance.gameObject.name == "Filter" || __instance.gameObject.name == "FIlter" || __instance.gameObject.name == "blackSheet" || __instance.gameObject.name == "bgFilter" || __instance.gameObject.name == "filterBlack")
                {
                    var transform = __instance.gameObject.GetComponent<RectTransform>();

                    if (transform.sizeDelta == new Vector2(1920f, 1080f) || transform.sizeDelta == new Vector2(2000f, 1200f))
                    {
                        if (fAspectRatio > fNativeAspect)
                        {
                            transform.sizeDelta = new Vector2(1080f * fAspectRatio, 1080f);
                        }
                        else if (fAspectRatio < fNativeAspect)
                        {
                            transform.sizeDelta = new Vector2(1920f, 1920f / fAspectRatio);
                        }

                        Log.LogInfo($"AspectRatio: Adjusted the size of {__instance.gameObject.transform.parent.gameObject.name}->{__instance.gameObject.name}");
                    }
                }

                if (__instance.gameObject.name == "statusBlind")
                {
                    float fWidthOffset = (float)((1080 * fAspectRatio) - 1920) / 2;
                    var transform = __instance.gameObject.GetComponent<RectTransform>();

                    if (fAspectRatio > fNativeAspect)
                    {
                        transform.localScale = new Vector3(fAspectMultiplier + (fAspectMultiplier - 1f), 1f, 1f);
                        transform.anchoredPosition = new Vector2(50f + fWidthOffset, -50f);
                    }
                    else if (fAspectRatio < fNativeAspect)
                    {
                        transform.localScale = new Vector3(1f, 1f / fAspectMultiplier, 1f);
                    }

                    Log.LogInfo($"AspectRatio: Adjusted the size of {__instance.gameObject.transform.parent.gameObject.name}->{__instance.gameObject.name}");
                }
            }

            // Span background blur
            [HarmonyPatch(typeof(UIBackgroundBlur), nameof(UIBackgroundBlur.OnEnable))]
            [HarmonyPostfix]
            public static void MapBackground(UIBackgroundBlur __instance)
            {
                var transform = __instance.GetComponent<RectTransform>();

                if (transform.sizeDelta == new Vector2(1920f, 1080f))
                {
                    if (fAspectRatio > fNativeAspect)
                    {
                        transform.sizeDelta = new Vector2(1080f * fAspectRatio, 1080f);
                    }
                    else if (fAspectRatio < fNativeAspect)
                    {
                        transform.sizeDelta = new Vector2(1920f, 1920f / fAspectRatio);
                    }
                    Log.LogInfo($"AspectRatio: Adjusted the size of background blur to {transform.sizeDelta}");
                }
            }

            // Span tutorial background
            [HarmonyPatch(typeof(Tutorial), nameof(Tutorial.RefleshPage))]
            [HarmonyPostfix]
            public static void TutorialBackground(Tutorial __instance)
            {
                if (__instance.gameObject.transform.GetChild(0) != null)
                {
                    var transform = __instance.gameObject.transform.GetChild(0).GetComponent<RectTransform>();

                    if (fAspectRatio != fNativeAspect)
                    {
                        transform.sizeDelta = new Vector2(10000f, 10000f); // This one is already 3000x3000 so it's basically a giant square.
                    }

                    Log.LogInfo($"AspectRatio: Adjusted the size of tutorial background to {transform.sizeDelta}");
                }
            }

            // Span screen wipes
            [HarmonyPatch(typeof(Transition.ScreenFadeSetting), nameof(Transition.ScreenFadeSetting.Play))]
            [HarmonyPostfix]
            public static void ScreenWipes(Transition.ScreenFadeSetting __instance)
            {
                if (__instance._fader != null)
                {
                    var transform = __instance._fader.transform.parent.GetComponent<Transform>();

                    if (fAspectRatio > fNativeAspect)
                    {
                        transform.localScale = new Vector3(1.78f * fAspectMultiplier, 1.78f, 1.78f);
                    }
                    else if (fAspectRatio < fNativeAspect)
                    {
                        //transform.localScale = new Vector3(1.78f * fAspectMultiplier, 1.78f, 1.78f);
                        // Not necessary
                    }

                    Log.LogInfo($"AspectRatio: Adjusted the size of screen transitions.");
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
                var URPAsset = UniversalRenderPipeline.asset;
                if (URPAsset != null)
                {
                    // Render scale
                    URPAsset.renderScale = fRenderScale.Value; // 1f default
                    Log.LogInfo($"Graphical Tweaks: Set render scale to {URPAsset.renderScale}");

                    // Anisotropic filtering
                    QualitySettings.anisotropicFiltering = AnisotropicFiltering.ForceEnable;
                    Texture.SetGlobalAnisotropicFilteringLimits(iAnisotropicFiltering.Value, iAnisotropicFiltering.Value);
                    Log.LogInfo($"Graphical Tweaks: Set anisotropic filtering to x{iAnisotropicFiltering.Value}");

                    // Shadows
                    var shadowRes = iShadowResolution.Value switch
                    {
                        1 => 256,
                        2 => 512,
                        3 => 1024,
                        4 => 2048,
                        5 => 4096,
                        6 => 8192,
                        _ => 1024,
                    };

                    URPAsset.shadowDistance = fShadowDistance.Value; // 150f high
                    Log.LogInfo($"Graphical Tweaks: Set shadow distance to {URPAsset.shadowDistance}");
                    URPAsset.mainLightShadowmapResolution = shadowRes; // 1024 high
                    Log.LogInfo($"Graphical Tweaks: Set main light shadowmap resolution to {URPAsset.mainLightShadowmapResolution}");
                    URPAsset.additionalLightsShadowmapResolution = shadowRes; // 1024 high
                    Log.LogInfo($"Graphical Tweaks: Set additional lights shadowmap resolution to {URPAsset.additionalLightsShadowmapResolution}");
                    URPAsset.shadowCascadeCount = iShadowCascades.Value; // 3 high
                    Log.LogInfo($"Graphical Tweaks: Set shadow cascades to {URPAsset.shadowCascadeCount}");
                }
            }

            // Enable SMAA on main camera
            [HarmonyPatch(typeof(FieldCamera), nameof(FieldCamera.Awake))]
            [HarmonyPostfix]
            public static void AntiAliasing(FieldCamera __instance)
            {
                if (bEnableSMAA.Value)
                {
                    if (__instance.MainCamera != null)
                    {
                        __instance.MainCamera.backgroundColor = Color.black;
                        var UACD = __instance.MainCamera.GetComponent<UnityEngine.Rendering.Universal.UniversalAdditionalCameraData>();

                        UACD.antialiasing = AntialiasingMode.SubpixelMorphologicalAntiAliasing;
                        UACD.antialiasingQuality = AntialiasingQuality.High;

                        Log.LogInfo("Graphical Tweaks: Enabled high quality SMAA on Main Camera.");
                    }
                }
            }

            // Adjust post-process effects
            [HarmonyPatch(typeof(UnityEngine.Rendering.Volume), nameof(UnityEngine.Rendering.Volume.OnEnable))]
            [HarmonyPostfix]
            public static void PostProcessTweaks(UnityEngine.Rendering.Volume __instance)
            {
                if (bVignette.Value)
                {
                    __instance.profile.TryGet(out Vignette vignette);
                    if (vignette)
                    {
                        vignette.active = false;
                        Log.LogInfo($"Graphical Tweaks: Disabled vignette on {__instance.gameObject.name}.");
                    }
                }

                if (bChromaticAberration.Value)
                {
                    __instance.profile.TryGet(out ChromaticAberration ca);
                    if (ca)
                    {
                        ca.active = false;
                        Log.LogInfo($"Graphical Tweaks: Disabled chromatic aberration on {__instance.gameObject.name}.");
                    }
                }
            }
        }
    }
}