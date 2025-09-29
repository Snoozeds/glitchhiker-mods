using System;
using System.Reflection;
using HarmonyLib;
using MelonLoader;
using UnityEngine;

[assembly: MelonInfo(
    typeof(SuperUltrawideFix.SuperUltrawideFixMod),
    "Super Ultrawide Fix",
    "1.0.0",
    "snoozeds"
)]
[assembly: MelonGame("Silverstring Media", "Glitchhikers The Spaces Between")]

namespace SuperUltrawideFix
{
    public class SuperUltrawideFixMod : MelonMod
    {
        public override void OnApplicationStart()
        {
            Melon<SuperUltrawideFixMod>.Logger.Msg("Super Ultrawide Fix loaded");

            var harmony = new HarmonyLib.Harmony("com.snoozeds.superultrawidepatch");

            // Patch the GetAttr method to return custom attributes for ultrawide resolutions
            var getAttrMethod = typeof(SupportedResolutionTypes).GetMethod(
                "GetAttr",
                BindingFlags.Public | BindingFlags.Static
            );

            if (getAttrMethod != null)
            {
                harmony.Patch(
                    getAttrMethod,
                    postfix: new HarmonyMethod(
                        typeof(SupportedResolutionTypesPatch),
                        "GetAttrPostfix"
                    )
                );
                Melon<SuperUltrawideFixMod>.Logger.Msg(
                    "Successfully patched SupportedResolutionTypes.GetAttr"
                );
            }
            else
            {
                Melon<SuperUltrawideFixMod>.Logger.Error("Could not find GetAttr method");
            }

            // Also patch GetClosestRes to handle custom resolutions
            var getClosestResMethod = typeof(SupportedResolutionTypes).GetMethod(
                "GetClosestRes",
                new Type[] { typeof(int), typeof(int) }
            );

            if (getClosestResMethod != null)
            {
                harmony.Patch(
                    getClosestResMethod,
                    postfix: new HarmonyMethod(
                        typeof(SupportedResolutionTypesPatch),
                        "GetClosestResPostfix"
                    )
                );
                Melon<SuperUltrawideFixMod>.Logger.Msg(
                    "Successfully patched SupportedResolutionTypes.GetClosestRes"
                );
            }
        }
    }

    public class SupportedResolutionTypesPatch
    {
        public static void GetAttrPostfix(
            SupportedResolutionType fromType,
            ref SupportedResolutionTypeAttr __result
        )
        {
            switch (fromType)
            {
                case SupportedResolutionType.RES_3440_X_1440:
                    // Override 21:9 3440x1440 with 32:9 5120x1440
                    __result = CreateCustomAttribute(
                        ResolutionType.RES_21_9,
                        5120,
                        1440,
                        __result.SaveID
                    );
                    Melon<SuperUltrawideFixMod>.Logger.Msg("Overriding 3440x1440 → 5120x1440");
                    break;

                case SupportedResolutionType.RES_3840_X_2160:
                    // Override 4K with 32:9 equivalent 7680x2160
                    __result = CreateCustomAttribute(
                        ResolutionType.RES_16_9,
                        7680,
                        2160,
                        __result.SaveID
                    );
                    Melon<SuperUltrawideFixMod>.Logger.Msg("Overriding 3840x2160 → 7680x2160");
                    break;
            }
        }

        public static void GetClosestResPostfix(
            int width,
            int height,
            ref SupportedResolutionType __result
        )
        {
            if (width == 5120 && height == 1440)
            {
                __result = SupportedResolutionType.RES_3440_X_1440; // Will be overridden to 5120x1440
                Melon<SuperUltrawideFixMod>.Logger.Msg("Mapped 5120x1440 → RES_3440_X_1440");
            }
            else if (width == 7680 && height == 2160)
            {
                __result = SupportedResolutionType.RES_3840_X_2160; // Will be overridden to 7680x2160
                Melon<SuperUltrawideFixMod>.Logger.Msg("Mapped 7680x2160 → RES_3840_X_2160");
            }
        }

        private static SupportedResolutionTypeAttr CreateCustomAttribute(
            ResolutionType resType,
            int width,
            int height,
            int saveID
        )
        {
            // Create SupportedResolutionTypeAttr with custom values
            var constructor = typeof(SupportedResolutionTypeAttr).GetConstructor(
                BindingFlags.NonPublic | BindingFlags.Instance,
                null,
                new Type[] { typeof(ResolutionType), typeof(int), typeof(int), typeof(int) },
                null
            );

            return (SupportedResolutionTypeAttr)
                constructor.Invoke(new object[] { resType, width, height, saveID });
        }
    }
}
