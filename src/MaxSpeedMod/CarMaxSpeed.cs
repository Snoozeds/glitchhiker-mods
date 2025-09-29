using MelonLoader;
using HarmonyLib;
using System.Reflection;
using UnityEngine;
using UnityEngine.InputSystem;

[assembly: MelonInfo(typeof(MaxSpeedMod.CarMaxSpeedMod), "Max Speed Mod", "1.0.0", "snoozeds")]
[assembly: MelonGame("Silverstring Media", "Glitchhikers The Spaces Between")]

namespace MaxSpeedMod
{
    public class CarMaxSpeedMod : MelonMod
    {
        public static bool isEnabled = true;
        private float displayTimer = 0f;          // HUD display timer
        private const float displayDuration = 2f; // Show HUD text for 2s

        public override void OnApplicationStart()
        {
            Melon<CarMaxSpeedMod>.Logger.Msg("MaxSpeedMod loaded");
            var harmony = new HarmonyLib.Harmony("com.snoozeds.maxspeedmod");
            harmony.PatchAll();
        }

        public override void OnUpdate()
        {
            // F5 to toggle mod
            if (Keyboard.current != null && Keyboard.current.f5Key.wasPressedThisFrame && Application.isFocused)
            {
                isEnabled = !isEnabled; // toggle mod
                displayTimer = displayDuration; // reset HUD timer
                Melon<CarMaxSpeedMod>.Logger.Msg($"MaxSpeedMod {(isEnabled ? "Enabled" : "Disabled")}");
            }

            // Count down HUD timer
            if (displayTimer > 0f)
                displayTimer -= Time.deltaTime;
        }

        // Display "Enabled/Disabled" message when toggling the mod.
        public override void OnGUI()
        {
            if (displayTimer > 0f)
            {
                GUI.matrix = Matrix4x4.identity; // Reset matrix so it actually appears bottom right lol.

                GUI.color = isEnabled ? Color.green : Color.red;
                string status = isEnabled ? "Enabled" : "Disabled";

                // Calculate bottom-right position
                float textWidth = 200f;
                float textHeight = 20f;
                float xPos = Screen.width - textWidth - 10f;  // 10px from right edge
                float yPos = Screen.height - textHeight - 10f; // 10px from bottom edge

                GUI.Label(new Rect(xPos, yPos, textWidth, textHeight), $"MaxSpeedMod: {status}");
                GUI.color = Color.white;
            }
        }
    }

    [HarmonyPatch(typeof(CarController), "Updated")]
    public class MaxSpeedPatch
    {
        static bool logged = false;

        static void Postfix(CarController __instance)
        {
            if (__instance == null || !CarMaxSpeedMod.isEnabled)
                return;

            var targetSpeedField = typeof(CarController).GetField("targetSpeed", BindingFlags.NonPublic | BindingFlags.Instance);
            var speedValueField = typeof(CarController).GetField("speedValue", BindingFlags.NonPublic | BindingFlags.Instance);
            var maxSpeedField = typeof(CarController).GetField("maxSpeed", BindingFlags.NonPublic | BindingFlags.Instance);

            if (targetSpeedField == null || speedValueField == null || maxSpeedField == null)
                return;

            var maxSpeed = maxSpeedField.GetValue(__instance);
            targetSpeedField.SetValue(__instance, maxSpeed);
            speedValueField.SetValue(__instance, maxSpeed);

            if (!logged)
            {
                Melon<CarMaxSpeedMod>.Logger.Msg($"Forced car speed to max ({maxSpeed})");
                logged = true;
            }
        }
    }
}
