using Alexandria.ItemAPI;
using MonoMod.RuntimeDetour;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;
using Alexandria.DungeonAPI;
using BepInEx;
using System.IO;
using HarmonyLib;
using Alexandria;
using SoundAPI;

namespace GunRev
{
    [BepInPlugin(GUID, "Gundustrial Revolution", "0.1.0")]
    [BepInDependency(Alexandria.Alexandria.GUID)] // this mod depends on the Alexandria API: https://enter-the-gungeon.thunderstore.io/package/Alexandria/Alexandria/
    [BepInDependency(ETGModMainBehaviour.GUID)]
    public class Module : BaseUnityPlugin
    {
        public const string GUID = "notsoai.etg.gunrev";
        public const string NAME = "Gundustrial Revolution";
        public const string VERSION = "0.1.0";
        public const string TEXT_COLOR = "#39FF14";
        public static AdvancedStringDB Strings;
        public static string ZipFilePath;
        public static string FilePath;

        public void Start()
        {
            ETGModMainBehaviour.WaitForGameManagerStart(GMStart);
        }

        public void GMStart(GameManager g)
        {
            var harmony = new Harmony(GUID);
            harmony.PatchAll();

            Log($"{NAME} v{VERSION} started successfully.", TEXT_COLOR);
            ETGMod.Assets.SetupSpritesFromAssembly(Assembly.GetExecutingAssembly(), "GunRev/sprites");
            FilePath = Path.Combine(Info.Location, "..");

            OtherTools.Init();
            Module.Strings = new AdvancedStringDB();
            CustomClipAmmoTypeToolbox.Init();
            SoundManager.Init();

            //Grab sounds
            AudioResourceLoader.LoadAllAutoloadResourcesFromAssembly(Assembly.GetExecutingAssembly(), "ai");

            //Guns
            Piston.Add();
            NanoCannon.Add();
            AK00101111.Add();
            JunK47.Add();

            //Items
            EnergyCell.Init();
            //EvolverBullets.Init();
            //BombNeumannProbe.Init();
            LowShieldPack.Init();

            //Synergy initialisation
            Synergies.Add();
        }

        public static void Log(string text, string color="#FFFFFF")
        {
            ETGModConsole.Log($"<color={color}>{text}</color>");
        }
    }
}
