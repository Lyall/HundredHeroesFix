using BepInEx;
using BepInEx.Unity.IL2CPP;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using UnityEngine;
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

        // Auto-Advance Tweaks
        public static ConfigEntry<bool> bAutoAdvanceTweaks;
        public static ConfigEntry<bool> bAutoVoiceDialog;
        public static ConfigEntry<float> fAutoAdvanceDelay;
        public static ConfigEntry<float> fTextSpeedMultiplier;

        // Battle Tweaks
        public static ConfigEntry<bool> bBattleTweaks;
        public static ConfigEntry<float> fAutoBattleSpeed;
        public static ConfigEntry<float> fBattleSpeed;

        // Graphical Tweaks
        public static ConfigEntry<bool> bGraphicalTweaks;
        public static ConfigEntry<bool> bVsync;
        public static ConfigEntry<float> fRenderScale;
        public static ConfigEntry<int> iAnisotropicFiltering;
        public static ConfigEntry<float> fShadowDistance;
        public static ConfigEntry<int> iShadowResolution;
        public static ConfigEntry<int> iShadowCascades;
        public static ConfigEntry<bool> bEnableSMAA;
        public static ConfigEntry<bool> bDisableChromaticAberration;
        public static ConfigEntry<bool> bDisableVignette;
        public static ConfigEntry<bool> bDisableColorGrading;

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
        public static bool bHasChangedTimescale;

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

            bControllerGlyphs = Config.Bind("Force Controller Icons",
                                "Enabled",
                                false,
                                "Allows forcing desired controller button icons.");


            iControllerStyle = Config.Bind("Force Controller Icons",
                                "IconStyle",
                                1,
                                new ConfigDescription("Set controller icon style. 1 = Dualshock (DS4), 2 = DualSense (DS5), 3 = Xbox",
                                new AcceptableValueRange<int>(1, 3)));

            // Battle Tweaks
            bBattleTweaks = Config.Bind("Battle Tweaks",
                                "Enabled",
                                false,
                                "Enables battle tweaks.");

            fAutoBattleSpeed = Config.Bind("Battle Tweaks",
                                "AutoBattleSpeed",
                                1f,
                                new ConfigDescription("Set auto-battle speed.",
                                new AcceptableValueRange<float>(1f, 8f)));

            fBattleSpeed = Config.Bind("Battle Tweaks",
                                "BattleSpeed",
                                1f,
                                new ConfigDescription("Set manual battle speed.",
                                new AcceptableValueRange<float>(1f, 8f)));

            // Auto-Advance Tweaks
            bAutoAdvanceTweaks = Config.Bind("Auto Dialog Advance Tweaks",
                                "Enabled",
                                true,
                                "Enables auto dialog advance tweaks.");

            bAutoVoiceDialog = Config.Bind("Dialog Tweaks",
                                "AutoAdvanceVoicedDialog",
                                true,
                                "Enables auto-advancing voiced dialog and removes the forced 2-second delay.");

            fAutoAdvanceDelay = Config.Bind("Dialog Tweaks",
                                "AutoAdvanceDelay",
                                2f,
                                new ConfigDescription("Set auto-advance dialog delay. Controls when non-voiced dialog automatically moves to the next line.",
                                new AcceptableValueRange<float>(0f, 10f)));

            fTextSpeedMultiplier = Config.Bind("Dialog Tweaks",
                                "TextSpeed",
                                1f,
                                new ConfigDescription("Set dialog text speed. Controls how fast non-voiced dialog is displayed.",
                                new AcceptableValueRange<float>(0.1f, 10f)));


            // Graphical Tweaks
            bGraphicalTweaks = Config.Bind("Graphical Tweaks",
                                "Enabled",
                                false,
                                "Enables tweaking various graphical options.");

            fRenderScale = Config.Bind("Graphical Tweaks",
                                "RenderScale",
                                1f,
                                new ConfigDescription("Set Render Scale. Higher than 1 downsamples and lower than 1 upsamples.",
                                new AcceptableValueRange<float>(0.1f, 10f)));

            iAnisotropicFiltering = Config.Bind("Graphical Tweaks",
                                "AnisotropicFiltering",
                                16,
                                new ConfigDescription("Set Anisotropic Filtering level.",
                                new AcceptableValueRange<int>(1, 16)));

            fShadowDistance = Config.Bind("Graphical Tweaks",
                                "ShadowDistance",
                                150f,
                                new ConfigDescription("Set distance of shadow rendering. Default High = 150",
                                new AcceptableValueRange<float>(15f, 500f)));

            iShadowResolution = Config.Bind("Graphical Tweaks",
                                "ShadowResolution",
                                5,
                                new ConfigDescription("Set shadow resolution. 1 = 256, 2 = 512, 3 = 1024, 4 = 2048, 5 = 4096, 6 = 8192. Default High = 3",
                                new AcceptableValueRange<int>(1, 6)));

            iShadowCascades = Config.Bind("Graphical Tweaks",
                                "ShadowCascades",
                                4,
                                new ConfigDescription("Set number of shadow cascades. Default High = 3",
                                new AcceptableValueRange<int>(1, 4)));

            bEnableSMAA = Config.Bind("Graphical Tweaks",
                                "AntiAliasing",
                                true,
                                "Enables high quality post-process SMAA.");

            bDisableChromaticAberration = Config.Bind("Graphical Tweaks",
                                "DisableChromaticAberration",
                                false,
                                "Set to true to disable chromatic abberation (color fringing at edges of screen).");

            bDisableVignette = Config.Bind("Graphical Tweaks",
                                "DisableVignette",
                                false,
                                "Set to true to disable vignette (darkening at edges of screen).");

            bDisableColorGrading = Config.Bind("Graphical Tweaks",
                                "DisableColorGrading",
                                false,
                                "Set to true to disable color grading.");

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
            if (bAutoAdvanceTweaks.Value)
            {
                Log.LogInfo($"Patches: Applying dialog patch.");
                Harmony.CreateAndPatchAll(typeof(DialogPatch));
            }
            if (bBattleTweaks.Value)
            {
                Log.LogInfo($"Patches: Applying auto-battle patch.");
                Harmony.CreateAndPatchAll(typeof(BattlePatch));
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
        public class DialogPatch
        {
            // Remove 2 second delay from auto-advancing dialog
            [HarmonyPatch(typeof(TextData.UI.KaeruText), nameof(TextData.UI.KaeruText.AutomaticSubmit))]
            [HarmonyPrefix]
            public static void RemoveDialogDelay(TextData.UI.KaeruText __instance, ref float __0)
            {
                // Set auto-advance dialog delay
                if (fAutoAdvanceDelay.Value != 2f)
                {
                    __0 = fAutoAdvanceDelay.Value;
                }

                // Only remove dialog delay for voiced lines
                var evtVoiceMngr = GameManager.Instance.EventManager.EventVoiceManager;
                if (bAutoVoiceDialog.Value && evtVoiceMngr.IsPlayingAll())
                {
                    // 100ms delay seems about right?
                    __0 = 0.1f;
                }
            }

            // Always auto-advance voiced dialog
            [HarmonyPatch(typeof(TextData.UI.KaeruText), nameof(TextData.UI.KaeruText.IsAuto), MethodType.Getter)]
            [HarmonyPostfix]
            public static void RemoveDialogDelay(ref bool __result)
            {
                // Force auto-advance for voiced dialog
                var evtVoiceMngr = GameManager.Instance.EventManager.EventVoiceManager;
                if (bAutoVoiceDialog.Value && evtVoiceMngr.IsPlayingAll())
                {
                    __result = true;
                }
            }

            // Set text speed
            [HarmonyPatch(typeof(TextData.UI.KaeruText), nameof(TextData.UI.KaeruText.Play))]
            [HarmonyPostfix]
            public static void RemoveDialogDelay(TextData.UI.KaeruText __instance)
            {
                // Check if non-voiced dialog
                var evtVoiceMngr = GameManager.Instance.EventManager.EventVoiceManager;
                if ((fTextSpeedMultiplier.Value != 1f) && !evtVoiceMngr.IsPlayingAll())
                {
                    __instance._speed /= fTextSpeedMultiplier.Value;
                }
            }
        }

        [HarmonyPatch]
        public class BattlePatch
        {
            // Enable manual battle turbo
            [HarmonyPatch(typeof(Battle.Engine), nameof(Battle.Engine.EndCommandSelect))]
            [HarmonyPostfix]
            public static void BattleTurboEnable(Battle.Engine __instance)
            {
                if ((__instance.CommandSelectOperation.SelectedMainCommand == Battle.Command.MainCommandType.Battle) && (fBattleSpeed.Value != 1.0f))
                {
                    bHasChangedTimescale = true;
                    Time.timeScale = fBattleSpeed.Value;
                    Log.LogInfo($"Battle: Manual: Changed game speed to {fBattleSpeed.Value}.");
                }

                if ((__instance.CommandSelectOperation.SelectedMainCommand == Battle.Command.MainCommandType.AutoCommand) && (fAutoBattleSpeed.Value != 1.0f))
                {
                    bHasChangedTimescale = true;
                    Time.timeScale = fAutoBattleSpeed.Value;
                    Log.LogInfo($"Battle: Auto: Changed game speed to {fAutoBattleSpeed.Value}.");
                }
            }

            // Disable manual battle turbo
            [HarmonyPatch(typeof(Battle.Engine), nameof(Battle.Engine.BeginCommandSelect))]
            [HarmonyPostfix]
            public static void ManualBattleTurboDisable(Battle.Engine __instance)
            {
                // Don't reset game speed if it's an auto battle
                if (bHasChangedTimescale && !__instance.CommandSelectOperation.ContinuateAutoCommand)
                {
                    bHasChangedTimescale = false;
                    Time.timeScale = 1.0f;
                    Log.LogInfo($"Battle: Manual: Reset game speed.");
                }
            }

            // Disable auto-battle turbo
            [HarmonyPatch(typeof(Battle.Command.CommandSelectOperation), nameof(Battle.Command.CommandSelectOperation.ContinuateAutoCommand), MethodType.Setter)]
            [HarmonyPostfix]
            public static void AutoBattleTurboDisable(Battle.Command.CommandSelectOperation __instance, ref bool __0)
            {
                if (__0 == false && bHasChangedTimescale)
                {
                    bHasChangedTimescale = false;
                    Time.timeScale = 1;
                    Log.LogInfo($"Battle: Auto: Cancelled auto battle and reset game speed.");
                }
            }
        }

        [HarmonyPatch]
        public class MiscPatch
        {
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
            [HarmonyPatch(typeof(GameData.DisplaySetting), nameof(GameData.DisplaySetting.UpdateFrameRate))]
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
                    Log.LogInfo($"Resolution: Applied custom resolution of {iCustomResX.Value}x{iCustomResY.Value} : Fullscreen = {bFullscreen.Value}.");
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

            // Change main camera background color and set correct FOV for <16:9
            [HarmonyPatch(typeof(UnityEngine.Rendering.Universal.UniversalAdditionalCameraData), nameof(UnityEngine.Rendering.Universal.UniversalAdditionalCameraData.OnAfterDeserialize))]
            [HarmonyPostfix]
            public static void AntiAliasing(UnityEngine.Rendering.Universal.UniversalAdditionalCameraData __instance)
            {
                if (__instance.gameObject.name == "Main Camera")
                {
                    // Set background color to black instead of blue. Makes the void outside of building stand out less, also good for OLEDs?
                    __instance.gameObject.GetComponent<Camera>().backgroundColor = Color.black;
                    Log.LogInfo("AspectRatio: Changed background color of Main Camera.");

                    if (fAspectRatio < fNativeAspect)
                    {
                        __instance.gameObject.GetComponent<Camera>().usePhysicalProperties = true;
                        __instance.gameObject.GetComponent<Camera>().gateFit = Camera.GateFitMode.Horizontal;
                        __instance.gameObject.GetComponent<Camera>().sensorSize = new Vector2(16f, 9f);
                        Log.LogInfo("AspectRatio: Set Main Camera gate fit mode to horizontal.");
                    }
                }
            }

            // Offset and span the add unit screen
            [HarmonyPatch(typeof(FieldStage.UI.AddUnit), nameof(FieldStage.UI.AddUnit.Show))]
            [HarmonyPostfix]
            public static void AddUnitPos(FieldStage.UI.AddUnit __instance)
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
            [HarmonyPatch(typeof(FieldStage.UI.BlackSmithWindow), nameof(FieldStage.UI.BlackSmithWindow.Awake))]
            [HarmonyPostfix]
            public static void BlacksmithPos(FieldStage.UI.BlackSmithWindow __instance)
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
            [HarmonyPatch(typeof(FieldStage.UI.InnCanvas), nameof(FieldStage.UI.InnCanvas.Open))]
            [HarmonyPostfix]
            public static void InnCanvasPos(FieldStage.UI.InnCanvas __instance)
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
            [HarmonyPatch(typeof(UnityEngine.UI.CanvasScaler), nameof(UnityEngine.UI.CanvasScaler.OnEnable))]
            [HarmonyPostfix]
            public static void LoadScreenFix(UnityEngine.UI.CanvasScaler __instance)
            {
                if (fAspectRatio > fNativeAspect)
                {
                    if (__instance != null)
                    {
                        if (__instance.referenceResolution == new Vector2(1920f, 600f) || (__instance.referenceResolution == new Vector2(1920f, 1080f) && __instance.screenMatchMode == UnityEngine.UI.CanvasScaler.ScreenMatchMode.MatchWidthOrHeight))
                        {
                            __instance.referenceResolution = new Vector2(1920f, 1080f);
                            __instance.screenMatchMode = UnityEngine.UI.CanvasScaler.ScreenMatchMode.Expand;
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
                    __instance.profile.TryGet(out UnityEngine.Rendering.Universal.Vignette vignette);
                    if (vignette)
                    {
                        vignette.intensity.value /= fAspectMultiplier;
                        Log.LogInfo($"AspectRatio: Changed intestity of {__instance.gameObject.name}'s vignette to {vignette.intensity.value}.");
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
                        // Not necessary
                    }

                    Log.LogInfo($"AspectRatio: Scaled fade.");
                }
            }

            // Span several backgrounds
            [HarmonyPatch(typeof(UnityEngine.UI.Image), nameof(UnityEngine.UI.Image.OnEnable))]
            [HarmonyPostfix]
            public static void FilterFix(UnityEngine.UI.Image __instance)
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

                        Log.LogInfo($"AspectRatio: Adjusted the size of {__instance.gameObject.transform.parent.gameObject.name}->{__instance.gameObject.name}.");
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

                    Log.LogInfo($"AspectRatio: Adjusted the size of {__instance.gameObject.transform.parent.gameObject.name}->{__instance.gameObject.name}.");
                }
            }

            // Span background blur
            [HarmonyPatch(typeof(Kaeru.UI.UIBackgroundBlur), nameof(Kaeru.UI.UIBackgroundBlur.OnEnable))]
            [HarmonyPostfix]
            public static void MapBackground(Kaeru.UI.UIBackgroundBlur __instance)
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

                    Log.LogInfo($"AspectRatio: Adjusted the size of background blur to {transform.sizeDelta}.");
                }
            }

            // Span tutorial background
            [HarmonyPatch(typeof(Common.UI.Tutorial), nameof(Common.UI.Tutorial.RefleshPage))]
            [HarmonyPostfix]
            public static void TutorialBackground(Common.UI.Tutorial __instance)
            {
                if (__instance.gameObject.transform.GetChild(0) != null)
                {
                    var transform = __instance.gameObject.transform.GetChild(0).GetComponent<RectTransform>();

                    if (fAspectRatio != fNativeAspect)
                    {
                        transform.sizeDelta = new Vector2(10000f, 10000f); // This one is already 3000x3000 so it's basically a giant square.
                    }

                    Log.LogInfo($"AspectRatio: Adjusted the size of tutorial background to {transform.sizeDelta}.");
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
            [HarmonyPatch(typeof(GameData.DisplaySetting), nameof(GameData.DisplaySetting.UpdateShadowOption))]
            [HarmonyPostfix]
            public static void GraphicalTweaks()
            {
                var URPAsset = UnityEngine.Rendering.Universal.UniversalRenderPipeline.asset;
                if (URPAsset != null)
                {
                    // Render scale
                    // Need to clamp value to avoid going over 16384px on either resolution axis.
                    int iHighestAxis = Math.Max(Screen.currentResolution.width, Screen.currentResolution.height);
                    float fMaxRenderScale = (float)16384 / iHighestAxis;
                    float fClampedRenderScale = Math.Clamp(fRenderScale.Value, 0.1f, fMaxRenderScale);
                    if (fClampedRenderScale < fRenderScale.Value)
                    {
                        Log.LogInfo($"Graphical Tweaks: Render scale value is invalid and has been clamped to {fClampedRenderScale}.");
                    }

                    URPAsset.m_RenderScale = fClampedRenderScale; // 1f default. Normally clamped to 2f but setting member variable instead of renderScale will get us past that limit.
                    Log.LogInfo($"Graphical Tweaks: Set render scale to {URPAsset.m_RenderScale}");

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
                    Log.LogInfo($"Graphical Tweaks: Set shadow distance to {URPAsset.shadowDistance}.");
                    URPAsset.mainLightShadowmapResolution = shadowRes; // 1024 high
                    Log.LogInfo($"Graphical Tweaks: Set main light shadowmap resolution to {URPAsset.mainLightShadowmapResolution}.");
                    URPAsset.additionalLightsShadowmapResolution = shadowRes; // 1024 high
                    Log.LogInfo($"Graphical Tweaks: Set additional lights shadowmap resolution to {URPAsset.additionalLightsShadowmapResolution}.");
                    URPAsset.shadowCascadeCount = iShadowCascades.Value; // 3 high
                    Log.LogInfo($"Graphical Tweaks: Set shadow cascades to {URPAsset.shadowCascadeCount}.");
                }
            }

            // Enable SMAA on main camera
            [HarmonyPatch(typeof(UnityEngine.Rendering.Universal.UniversalAdditionalCameraData), nameof(UnityEngine.Rendering.Universal.UniversalAdditionalCameraData.OnAfterDeserialize))]
            [HarmonyPostfix]
            public static void AntiAliasing(UnityEngine.Rendering.Universal.UniversalAdditionalCameraData __instance)
            {
                if (bEnableSMAA.Value)
                {
                    if (__instance.gameObject.name == "Main Camera")
                    {
                        __instance.antialiasing = UnityEngine.Rendering.Universal.AntialiasingMode.SubpixelMorphologicalAntiAliasing;
                        __instance.antialiasingQuality = UnityEngine.Rendering.Universal.AntialiasingQuality.High;

                        Log.LogInfo("Graphical Tweaks: Enabled high quality SMAA on Main Camera.");
                    }
                }
            }

            // Adjust post-process effects
            [HarmonyPatch(typeof(UnityEngine.Rendering.Volume), nameof(UnityEngine.Rendering.Volume.OnEnable))]
            [HarmonyPostfix]
            public static void PostProcessTweaks(UnityEngine.Rendering.Volume __instance)
            {
                if (bDisableColorGrading.Value)
                {
                    __instance.profile.TryGet(out UnityEngine.Rendering.Universal.ColorCurves colorCurves);
                    if (colorCurves)
                    {
                        colorCurves.active = false;
                        Log.LogInfo($"Graphical Tweaks: Disabled colorCurves on {__instance.gameObject.name}.");
                    }
                }

                if (bDisableVignette.Value)
                {
                    __instance.profile.TryGet(out UnityEngine.Rendering.Universal.Vignette vignette);
                    if (vignette)
                    {
                        vignette.active = false;
                        Log.LogInfo($"Graphical Tweaks: Disabled vignette on {__instance.gameObject.name}.");
                    }
                }

                if (bDisableChromaticAberration.Value)
                {
                    __instance.profile.TryGet(out UnityEngine.Rendering.Universal.ChromaticAberration ca);
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