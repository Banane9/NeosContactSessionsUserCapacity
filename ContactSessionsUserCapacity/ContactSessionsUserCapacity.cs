using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using BaseX;
using CloudX.Shared;
using CodeX;
using FrooxEngine;
using FrooxEngine.LogiX;
using FrooxEngine.LogiX.Data;
using FrooxEngine.LogiX.ProgramFlow;
using FrooxEngine.UIX;
using HarmonyLib;
using NeosModLoader;

namespace ContactSessionsUserCapacity
{
    public class ContactSessionsUserCapacity : NeosMod
    {
        public static ModConfiguration Config;

        [AutoRegisterConfigKey]
        private static ModConfigurationKey<color> EmptySessionColor = new ModConfigurationKey<color>("EmptySessionColor", "Color of the user count when only the host is there.", () => color.Green.SetValue(.65f));

        [AutoRegisterConfigKey]
        private static ModConfigurationKey<color> FullSessionColor = new ModConfigurationKey<color>("FullSessionColor", "Color of the user count when the session is full.", () => color.Red.SetValue(.8f));

        [AutoRegisterConfigKey]
        private static ModConfigurationKey<bool> ShowUsageLevelWithColorGradient = new ModConfigurationKey<bool>("ShowUsageLevelWithColorGradient", "Color the user count based on capacity usage.", () => true);

        [AutoRegisterConfigKey]
        private static ModConfigurationKey<bool> ShowUserCapacityInSessionList = new ModConfigurationKey<bool>("ShowUserCapacityInSessionList", "Show the user capacity of contacts' joinable sessions.", () => true);

        public override string Author => "Banane9";
        public override string Link => "https://github.com/Banane9/NeosContactSessionsUserCapacity";
        public override string Name => "ContactSessionsUserCapacity";
        public override string Version => "1.0.0";

        public override void OnEngineInit()
        {
            Harmony harmony = new Harmony($"{Author}.{Name}");
            Config = GetConfiguration();
            Config.Save(true);
            harmony.PatchAll();
        }

        [HarmonyPatch(typeof(SessionItem))]
        private static class SessionItemPatches
        {
            [HarmonyPostfix]
            [HarmonyPatch(nameof(SessionItem.Update))]
            private static void UpdatePostfix(SessionInfo session, SyncRef<Text> ____userCount)
            {
                var format = "{0} ({1})";

                if (Config.GetValue(ShowUserCapacityInSessionList))
                    format += " / {2}";

                if (Config.GetValue(ShowUsageLevelWithColorGradient))
                {
                    var usage = ((float)session.JoinedUsers - 1) / (float)session.MaximumUsers;

                    var value = usage > 1 ?
                        new ColorHSV(Config.GetValue(FullSessionColor)).v - .2f
                        : MathX.Lerp(new ColorHSV(Config.GetValue(EmptySessionColor)).v, new ColorHSV(Config.GetValue(FullSessionColor)).v, usage);

                    var color = MathX.Lerp(Config.GetValue(EmptySessionColor), Config.GetValue(FullSessionColor), usage).SetValue(value);

                    format = $"<color={color.ToHexString()}>{format}</color>";
                }

                format = "Users: " + format;
                ____userCount.Target.Content.Value = string.Format(format, session.ActiveUsers, session.JoinedUsers, session.MaximumUsers);
            }
        }
    }
}