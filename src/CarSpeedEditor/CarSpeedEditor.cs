using System.Reflection;
using HarmonyLib;
using MelonLoader;
using UnityEngine;
using UnityEngine.InputSystem;

[assembly: MelonInfo(
    typeof(CarSpeedEditor.CarSpeedEditorMod),
    "Car Speed Editor",
    "1.0.0",
    "snoozeds"
)]
[assembly: MelonGame("Silverstring Media", "Glitchhikers The Spaces Between")]

namespace CarSpeedEditor
{
    public class CarSpeedEditorMod : MelonMod
    {
        public static bool isEnabled = true;
        private float displayTimer = 0f;
        private const float displayDuration = 3f;

        // MelonPreferences
        private static MelonPreferences_Category configCategory;
        private static MelonPreferences_Entry<float> minSpeedEntry;
        private static MelonPreferences_Entry<float> maxSpeedEntry;
        private static MelonPreferences_Entry<float> speedAdjustmentStep;

        // Current speed values
        public static float currentMinSpeed = 32f;
        public static float currentMaxSpeed = 48f;
        public static float adjustmentStep = 2f;

        // Original values
        public static float originalMinSpeed = 32f;
        public static float originalMaxSpeed = 48f;
        public static bool originalValuesSaved = false;

        public override void OnApplicationStart()
        {
            Melon<CarSpeedEditorMod>.Logger.Msg("CarSpeedEditor loaded");

            // Setup MelonPreferences
            configCategory = MelonPreferences.CreateCategory("CarSpeedEditor");
            minSpeedEntry = configCategory.CreateEntry<float>("MinSpeed", 32f, "Minimum Speed");
            maxSpeedEntry = configCategory.CreateEntry<float>("MaxSpeed", 48, "Maximum Speed");
            speedAdjustmentStep = configCategory.CreateEntry<float>(
                "AdjustmentStep",
                2f,
                "Speed Adjustment Step"
            );

            // Load saved values
            currentMinSpeed = minSpeedEntry.Value;
            currentMaxSpeed = maxSpeedEntry.Value;
            adjustmentStep = speedAdjustmentStep.Value;

            // Apply Harmony patches
            var harmony = new HarmonyLib.Harmony("com.snoozeds.carspeedediitor");
            harmony.PatchAll();
        }

        public override void OnUpdate()
        {
            if (Keyboard.current == null || !Application.isFocused)
                return;

            // F6 to toggle mod
            if (Keyboard.current.f6Key.wasPressedThisFrame)
            {
                isEnabled = !isEnabled;
                displayTimer = displayDuration;
                Melon<CarSpeedEditorMod>.Logger.Msg(
                    $"CarSpeedEditor {(isEnabled ? "Enabled" : "Disabled")}"
                );
            }

            // Only process speed adjustments if mod is enabled
            if (!isEnabled)
            {
                // Count down display timer
                if (displayTimer > 0f)
                    displayTimer -= Time.deltaTime;
                return;
            }

            bool speedChanged = false;
            bool ctrlHeld =
                Keyboard.current.leftCtrlKey.isPressed || Keyboard.current.rightCtrlKey.isPressed;
            bool shiftHeld =
                Keyboard.current.leftShiftKey.isPressed || Keyboard.current.rightShiftKey.isPressed;

            // Arrow key controls for speed adjustment
            if (Keyboard.current.upArrowKey.wasPressedThisFrame) // Increase max speed
            {
                currentMaxSpeed += adjustmentStep;
                speedChanged = true;
                Melon<CarSpeedEditorMod>.Logger.Msg($"Max Speed increased to: {currentMaxSpeed}");
            }
            else if (Keyboard.current.downArrowKey.wasPressedThisFrame) // Decrease max speed
            {
                currentMaxSpeed = Mathf.Max(currentMinSpeed + 1f, currentMaxSpeed - adjustmentStep);
                speedChanged = true;
                Melon<CarSpeedEditorMod>.Logger.Msg($"Max Speed decreased to: {currentMaxSpeed}");
            }
            else if (Keyboard.current.rightArrowKey.wasPressedThisFrame) // Increase min speed
            {
                currentMinSpeed = Mathf.Min(currentMaxSpeed - 1f, currentMinSpeed + adjustmentStep);
                speedChanged = true;
                Melon<CarSpeedEditorMod>.Logger.Msg($"Min Speed increased to: {currentMinSpeed}");
            }
            else if (Keyboard.current.leftArrowKey.wasPressedThisFrame) // Decrease min speed
            {
                currentMinSpeed = Mathf.Max(0f, currentMinSpeed - adjustmentStep);
                speedChanged = true;
                Melon<CarSpeedEditorMod>.Logger.Msg($"Min Speed decreased to: {currentMinSpeed}");
            }
            else if (Keyboard.current.rKey.wasPressedThisFrame && ctrlHeld) // Ctrl+R to reset to original values
            {
                if (originalValuesSaved)
                {
                    currentMinSpeed = originalMinSpeed;
                    currentMaxSpeed = originalMaxSpeed;
                    speedChanged = true;
                    displayTimer = displayDuration;
                    Melon<CarSpeedEditorMod>.Logger.Msg(
                        $"Reset to original values - Min: {currentMinSpeed}, Max: {currentMaxSpeed}"
                    );
                }
            }
            else if (Keyboard.current.digit1Key.wasPressedThisFrame && ctrlHeld) // Ctrl+1 to change adjustment step
            {
                adjustmentStep = adjustmentStep == 1f ? 2f : (adjustmentStep == 2f ? 5f : 1f);
                speedAdjustmentStep.Value = adjustmentStep;
                configCategory.SaveToFile();
                displayTimer = displayDuration;
                Melon<CarSpeedEditorMod>.Logger.Msg(
                    $"Adjustment step changed to: {adjustmentStep}"
                );
            }

            // Save to preferences if speed changed
            if (speedChanged)
            {
                minSpeedEntry.Value = currentMinSpeed;
                maxSpeedEntry.Value = currentMaxSpeed;
                configCategory.SaveToFile();
                displayTimer = displayDuration;
            }

            // Count down display timer
            if (displayTimer > 0f)
                displayTimer -= Time.deltaTime;
        }

        public override void OnGUI()
        {
            if (displayTimer > 0f)
            {
                GUI.matrix = Matrix4x4.identity; // Reset matrix so it actually appears bottom right lol.

                // Calculate bottom-right position
                float panelWidth = 280f;
                float panelHeight = 80f;
                float panelX = Screen.width - panelWidth - 10f; // 10px from right edge
                float panelY = Screen.height - panelHeight - 10f; // 10px from bottom edge

                // Background box
                GUI.Box(new Rect(panelX, panelY, panelWidth, panelHeight), "");

                // Status color
                GUI.color = isEnabled ? Color.green : Color.red;
                string status = isEnabled ? "Enabled" : "Disabled";
                GUI.Label(new Rect(panelX + 5, panelY + 5, 200, 20), $"CarSpeedEditor: {status}");

                if (isEnabled)
                {
                    GUI.color = Color.white;
                    GUI.Label(
                        new Rect(panelX + 5, panelY + 25, 270, 20),
                        $"Min Speed: {currentMinSpeed:F1} | Max Speed: {currentMaxSpeed:F1}"
                    );
                    GUI.color = Color.yellow;
                    GUI.Label(
                        new Rect(panelX + 5, panelY + 45, 270, 20),
                        "Arrows: ↑↓=MaxSpd, ←→=MinSpd, Ctrl+R=Reset, Ctrl+1=Step"
                    );
                }

                GUI.color = Color.white;
            }
        }
    }

    [HarmonyPatch(typeof(CarController), "Updated")]
    public class CarSpeedPatch
    {
        static void Postfix(CarController __instance)
        {
            if (__instance == null || !CarSpeedEditorMod.isEnabled)
                return;

            var minSpeedField = typeof(CarController).GetField(
                "minSpeed",
                BindingFlags.NonPublic | BindingFlags.Instance
            );
            var maxSpeedField = typeof(CarController).GetField(
                "maxSpeed",
                BindingFlags.NonPublic | BindingFlags.Instance
            );

            if (minSpeedField == null || maxSpeedField == null)
                return;

            // Save original values once
            if (!CarSpeedEditorMod.originalValuesSaved)
            {
                CarSpeedEditorMod.originalMinSpeed = (float)minSpeedField.GetValue(__instance);
                CarSpeedEditorMod.originalMaxSpeed = (float)maxSpeedField.GetValue(__instance);
                CarSpeedEditorMod.originalValuesSaved = true;

                // If this is the first time and preferences are still at defaults, use original values
                if (
                    CarSpeedEditorMod.currentMinSpeed == 32f
                    && CarSpeedEditorMod.currentMaxSpeed == 48f
                )
                {
                    CarSpeedEditorMod.currentMinSpeed = CarSpeedEditorMod.originalMinSpeed;
                    CarSpeedEditorMod.currentMaxSpeed = CarSpeedEditorMod.originalMaxSpeed;
                }
            }

            // Apply modified speed values
            minSpeedField.SetValue(__instance, CarSpeedEditorMod.currentMinSpeed);
            maxSpeedField.SetValue(__instance, CarSpeedEditorMod.currentMaxSpeed);
        }
    }
}
