using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

namespace ProFlashlightPlus
{
    [BepInPlugin(MyPluginInfo.PLUGIN_GUID,
                 MyPluginInfo.PLUGIN_NAME,
                 MyPluginInfo.PLUGIN_VERSION)]
    public class ProFlashlightPlus : BaseUnityPlugin
    {
        internal new static ManualLogSource Logger { get; private set; } = null!;
        internal static bool UIOpen = false;

        internal const float DefaultIntensityValue = 100f;
        internal const float DefaultAngleValue     = 60f;
        internal const float DefaultR             = 1f;
        internal const float DefaultG             = 1f;
        internal const float DefaultB             = 1f;
        internal const bool  DefaultStrobe        = false;
        internal const float DefaultSpeed         = 1f;

        internal static float IntensityValue = DefaultIntensityValue;
        internal static float AngleValue     = DefaultAngleValue;
        internal static float RValue         = DefaultR;
        internal static float GValue         = DefaultG;
        internal static float BValue         = DefaultB;
        internal static bool  StrobeEnabled  = DefaultStrobe;
        internal static float SpeedValue     = DefaultSpeed;

        private void Awake()
        {
            Logger = base.Logger;
            Logger.LogInfo("ProFlashlightPlus Awake – subscribing to sceneLoaded.");

            var harmony = new Harmony(MyPluginInfo.PLUGIN_GUID);
            harmony.PatchAll();

            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            Logger.LogInfo($"Scene '{scene.name}' loaded – spawning UI now");
            var go = new GameObject("ProFlashlightUI");
            DontDestroyOnLoad(go);
            go.AddComponent<FlashlightUI>();
            Logger.LogInfo("FlashlightUI component attached.");
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }
    }

    public class FlashlightUI : MonoBehaviour
    {
        private Rect _windowRect;
        private const int WindowId = 123456;

        void Start()
        {
            float w = 500f, h = 350f;
            _windowRect = new Rect(
                (Screen.width  - w) / 2,
                (Screen.height - h) / 2,
                w, h
            );
        }

        void Update()
        {
            if (Keyboard.current?.f8Key.wasPressedThisFrame == true)
            {
                ProFlashlightPlus.UIOpen = !ProFlashlightPlus.UIOpen;
                Cursor.visible   = ProFlashlightPlus.UIOpen;
                Cursor.lockState = ProFlashlightPlus.UIOpen
                    ? CursorLockMode.None
                    : CursorLockMode.Locked;
                Debug.Log($"[FlashlightUI] F8 pressed — UIOpen = {ProFlashlightPlus.UIOpen}");
            }
        }

        void OnGUI()
        {
            if (!ProFlashlightPlus.UIOpen) return;

            _windowRect = GUI.ModalWindow(
                WindowId,
                _windowRect,
                DrawWindow,
                "Flashlight Control"
            );
        }

        private void DrawWindow(int id)
        {
            float w = _windowRect.width;
            float h = _windowRect.height;

            // close button
            if (GUI.Button(new Rect(w - 28, 4, 24, 24), "X"))
            {
                ProFlashlightPlus.UIOpen = false;
                Cursor.visible = false;
                Cursor.lockState = CursorLockMode.Locked;
            }

            // intensity slider 0–2500
            GUI.Label(new Rect(10, 40, 140, 20), "Intensity (0–2500):");
            float newInt = GUI.HorizontalSlider(
                new Rect(160, 45, w - 180, 20),
                ProFlashlightPlus.IntensityValue,
                0f, 2500f
            );
            if (!Mathf.Approximately(newInt, ProFlashlightPlus.IntensityValue))
                ProFlashlightPlus.IntensityValue = newInt;

            // angle slider
            GUI.Label(new Rect(10, 80, 140, 20), "Spread (°):");
            float newAng = GUI.HorizontalSlider(
                new Rect(160, 85, w - 180, 20),
                ProFlashlightPlus.AngleValue,
                5f, 160f
            );
            if (!Mathf.Approximately(newAng, ProFlashlightPlus.AngleValue))
                ProFlashlightPlus.AngleValue = newAng;

            // color R,G,B
            GUI.Label(new Rect(10, 120, 140, 20), "Color - R:");
            float newR = GUI.HorizontalSlider(
                new Rect(160, 125, w - 180, 20),
                ProFlashlightPlus.RValue,
                0f, 1f
            );
            if (!Mathf.Approximately(newR, ProFlashlightPlus.RValue))
                ProFlashlightPlus.RValue = newR;

            GUI.Label(new Rect(10, 160, 140, 20), "Color - G:");
            float newG = GUI.HorizontalSlider(
                new Rect(160, 165, w - 180, 20),
                ProFlashlightPlus.GValue,
                0f, 1f
            );
            if (!Mathf.Approximately(newG, ProFlashlightPlus.GValue))
                ProFlashlightPlus.GValue = newG;

            GUI.Label(new Rect(10, 200, 140, 20), "Color - B:");
            float newB = GUI.HorizontalSlider(
                new Rect(160, 205, w - 180, 20),
                ProFlashlightPlus.BValue,
                0f, 1f
            );
            if (!Mathf.Approximately(newB, ProFlashlightPlus.BValue))
                ProFlashlightPlus.BValue = newB;

            // strobe toggle
            GUI.Label(new Rect(10, 240, 140, 20), "Strobe:");
            bool newStrobe = GUI.Toggle(
                new Rect(160, 240, 20, 20),
                ProFlashlightPlus.StrobeEnabled,
                ""
            );
            if (newStrobe != ProFlashlightPlus.StrobeEnabled)
                ProFlashlightPlus.StrobeEnabled = newStrobe;

            // speed slider
            GUI.Label(new Rect(10, 280, 140, 20), "Strobe Speed:");
            float newSpeed = GUI.HorizontalSlider(
                new Rect(160, 285, w - 180, 20),
                ProFlashlightPlus.SpeedValue,
                0.1f, 10f
            );
            if (!Mathf.Approximately(newSpeed, ProFlashlightPlus.SpeedValue))
                ProFlashlightPlus.SpeedValue = newSpeed;

            // reset button
            if (GUI.Button(new Rect((w - 100) / 2, h - 40, 100, 30), "Reset"))
            {
                ProFlashlightPlus.IntensityValue = ProFlashlightPlus.DefaultIntensityValue;
                ProFlashlightPlus.AngleValue     = ProFlashlightPlus.DefaultAngleValue;
                ProFlashlightPlus.RValue         = ProFlashlightPlus.DefaultR;
                ProFlashlightPlus.GValue         = ProFlashlightPlus.DefaultG;
                ProFlashlightPlus.BValue         = ProFlashlightPlus.DefaultB;
                ProFlashlightPlus.StrobeEnabled  = ProFlashlightPlus.DefaultStrobe;
                ProFlashlightPlus.SpeedValue     = ProFlashlightPlus.DefaultSpeed;
            }

            // numeric displays
            GUI.Label(new Rect(w - 100, 40, 90, 20),  ProFlashlightPlus.IntensityValue.ToString("0"));
            GUI.Label(new Rect(w - 100, 80, 90, 20),  ProFlashlightPlus.AngleValue.ToString("0"));
            GUI.Label(new Rect(w - 100, 120, 90, 20), ProFlashlightPlus.RValue.ToString("0.00"));
            GUI.Label(new Rect(w - 100, 160, 90, 20), ProFlashlightPlus.GValue.ToString("0.00"));
            GUI.Label(new Rect(w - 100, 200, 90, 20), ProFlashlightPlus.BValue.ToString("0.00"));
            GUI.Label(new Rect(w - 100, 240, 90, 20), ProFlashlightPlus.StrobeEnabled ? "On" : "Off");
            GUI.Label(new Rect(w - 100, 280, 90, 20), ProFlashlightPlus.SpeedValue.ToString("0.0"));

            GUI.DragWindow(new Rect(0, 0, w, 24));
        }
    }

    [HarmonyPatch(typeof(FlashlightItem), "Update")]
    public static class FlashlightItem_Update_Patch
    {
        static void Postfix(FlashlightItem __instance)
        {
            var bulb = __instance.flashlightBulb;
            if (bulb != null)
            {
                // intensity + angle
                bulb.intensity = ProFlashlightPlus.IntensityValue;
                if (bulb.type == LightType.Spot)
                    bulb.spotAngle = ProFlashlightPlus.AngleValue;

                // color or strobe
                if (ProFlashlightPlus.StrobeEnabled)
                {
                    float hue = (Time.time * (ProFlashlightPlus.SpeedValue/2)) % 1f;
                    Color rainbow = Color.HSVToRGB(hue, 1f, 1f);
                    bulb.color = rainbow;
                }
                else
                {
                    bulb.color = new Color(
                        ProFlashlightPlus.RValue,
                        ProFlashlightPlus.GValue,
                        ProFlashlightPlus.BValue
                    );
                }
            }
        }
    }
}
