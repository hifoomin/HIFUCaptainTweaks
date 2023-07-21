using R2API;
using RoR2;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace HIFUCaptainTweaks.Skills
{
    public class HealingBeacon : TweakBase
    {
        public static float Healing;
        public static float Radius;
        public static float Interval;
        public static float MoveSpeed;
        public static BuffDef MoveSpeedBuff;

        public override string Name => ": Special : Remedy Beacon";

        public override string SkillToken => "supply_heal";

        public override string DescText => "<style=cIsHealing>Heal</style> all nearby allies constantly" +
                                           (MoveSpeed > 0 ? " and <style=cIsUtility>increase movement speed</style>." : ".");

        public override void Init()
        {
            Healing = ConfigOption(0.07f, "Healing", "Decimal. Vanilla is 0.1");
            Interval = ConfigOption(0.2f, "Interval", "Vanilla is 0.2");
            Radius = ConfigOption(24f, "Radius", "Vanilla is 10");
            MoveSpeed = ConfigOption(0.2f, "Buff Move Speed", "Decimal. Vanilla is 0");
            base.Init();
        }

        public override void Hooks()
        {
            Changes();
            RecalculateStatsAPI.GetStatCoefficients += RecalculateStatsAPI_GetStatCoefficients;
        }

        private void RecalculateStatsAPI_GetStatCoefficients(CharacterBody sender, RecalculateStatsAPI.StatHookEventArgs args)
        {
            if (sender && sender.HasBuff(MoveSpeedBuff))
            {
                args.moveSpeedMultAdd += MoveSpeed;
            }
        }

        public static void Changes()
        {
            LanguageAPI.Add("CAPTAIN_SUPPLY_HEAL_NAME", "Remedy Beacon");
            var WhipSprite = Addressables.LoadAssetAsync<Texture2D>("RoR2/Base/Common/texMovespeedBuffIcon.tif").WaitForCompletion();
            MoveSpeedBuff = ScriptableObject.CreateInstance<BuffDef>();
            MoveSpeedBuff.name = "Remedy Beacon Speed";
            MoveSpeedBuff.buffColor = new Color32(128, 209, 132, 255);
            MoveSpeedBuff.canStack = false;
            MoveSpeedBuff.iconSprite = Sprite.Create(WhipSprite, new Rect(0f, 0f, (float)WhipSprite.width, (float)WhipSprite.height), new Vector2(0f, 0f));
            MoveSpeedBuff.isDebuff = false;

            ContentAddition.AddBuffDef(MoveSpeedBuff);

            On.EntityStates.CaptainSupplyDrop.HealZoneMainState.OnEnter += (orig, self) =>
            {
                var ward = EntityStates.CaptainSupplyDrop.HealZoneMainState.healZonePrefab.GetComponent<HealingWard>();
                ward.radius = Radius;
                ward.healFraction = Interval * Healing;
                orig(self);
            };

            var HealingBeacon = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Captain/CaptainSupplyDrop, Healing.prefab").WaitForCompletion();
            var Indicator = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Captain/CaptainHealingWard.prefab").WaitForCompletion().transform.GetChild(0);
            HealingBeacon.AddComponent<BuffWard>();
            var BW = HealingBeacon.GetComponent<BuffWard>();
            BW.Networkradius = Radius;
            BW.shape = BuffWard.BuffWardShape.Sphere;
            BW.radius = Radius;
            BW.interval = 0.5f;
            BW.rangeIndicator = Indicator.transform;
            BW.buffDef = MoveSpeedBuff;
            BW.buffDuration = 1.5f;
            BW.floorWard = true;
            BW.expires = false;
            BW.invertTeamFilter = false;
            BW.expireDuration = 0f;
            BW.animateRadius = false;
            BW.removalTime = 0f;
            BW.requireGrounded = false;
            BW.teamFilter = HealingBeacon.GetComponent<TeamFilter>();
        }
    }
}