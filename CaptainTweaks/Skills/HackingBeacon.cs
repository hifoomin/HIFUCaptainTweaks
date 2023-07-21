using R2API;
using RoR2;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace HIFUCaptainTweaks.Skills
{
    public class HackingBeacon : TweakBase
    {
        public static float Radius;
        public static float BaseDuration;
        public static int DurationPerGold;
        public static bool Formula;

        public override string Name => ": Special :::: Hacking Beacon";

        public override string SkillToken => "supply_hacking";

        public override string DescText => "<style=cIsUtility>Hack</style> all nearby purchasables to a cost of <style=cIsUtility>$0</style> over time.";

        public override void Init()
        {
            Radius = ConfigOption(16f, "Radius", "Vanilla is 10");
            BaseDuration = ConfigOption(2f, "Duration Multiplier", "Vanilla is 3");
            DurationPerGold = ConfigOption(25, "Duration Per Gold", "Vanilla is 25");
            Formula = ConfigOption(true, "Hacking Speed Formula", "(Interactable Cost / Duration Per Gold) * Duration Multiplier");
            base.Init();
        }

        public override void Hooks()
        {
            Changes();
        }

        public static void Changes()
        {
            LanguageAPI.Add("CAPTAIN_SUPPLY_HACKING_NAME", "Hacking Beacon");

            var Hack = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Captain/CaptainSupplyDrop, Hacking.prefab").WaitForCompletion();
            var Indicator = Hack.GetComponent<ModelLocator>().modelTransform.GetChild(4);
            Indicator.GetComponent<ObjectScaleCurve>().baseScale = new Vector3(Radius / 1.5f, Radius / 1.5f, Radius / 1.5f);

            On.EntityStates.CaptainSupplyDrop.HackingMainState.OnEnter += (orig, self) =>
            {
                EntityStates.CaptainSupplyDrop.HackingMainState.baseRadius = Radius;
                orig(self);
            };
            On.EntityStates.CaptainSupplyDrop.HackingInProgressState.OnEnter += (orig, self) =>
            {
                EntityStates.CaptainSupplyDrop.HackingInProgressState.baseGoldForBaseDuration = DurationPerGold;
                EntityStates.CaptainSupplyDrop.HackingInProgressState.baseDuration = BaseDuration;
                orig(self);
            };
        }
    }
}