using R2API;
using RoR2;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace HIFUCaptainTweaks.Skills
{
    public class ResupplyBeacon : TweakBase
    {
        public static float AttackSpeed;
        public static float CooldownReduction;
        public static float Radius;
        public static BuffDef AttackSpeedBuff;

        public override string Name => ": Special ::: Artillery Beacon";

        public override string SkillToken => "supply_equipment_restock";

        public override string DescText => "<style=cIsUtility>Recharge Equipment</style> on use" +
                                            (AttackSpeed > 0 ? ", <style=cIsDamage>increase attack speed</style> and <style=cIsUtility>reduce cooldowns</style>." : ".");

        public override void Init()
        {
            AttackSpeed = ConfigOption(0.25f, "Buff Attack Speed", "Decimal. Vanilla is 0");
            CooldownReduction = ConfigOption(0.1f, "Buff Cooldown Reduction", "Decimal. Vanilla is 0");
            Radius = ConfigOption(20f, "Radius", "Vanilla is 0");
            base.Init();
        }

        public override void Hooks()
        {
            Changes();
            RecalculateStatsAPI.GetStatCoefficients += RecalculateStatsAPI_GetStatCoefficients;
        }

        private void RecalculateStatsAPI_GetStatCoefficients(CharacterBody sender, RecalculateStatsAPI.StatHookEventArgs args)
        {
            if (sender && sender.HasBuff(AttackSpeedBuff))
            {
                args.attackSpeedMultAdd += AttackSpeed;
                args.cooldownMultAdd -= CooldownReduction;
            }
        }

        public static void Changes()
        {
            LanguageAPI.Add("CAPTAIN_SUPPLY_EQUIPMENT_RESTOCK_NAME", "Artillery Beacon");
            var WarCrySprite = Addressables.LoadAssetAsync<Texture2D>("RoR2/Base/WarCryOnMultiKill/texWarcryBuffIcon.tif").WaitForCompletion();
            AttackSpeedBuff = ScriptableObject.CreateInstance<BuffDef>();
            AttackSpeedBuff.name = "Artillery Beacon Attack Speed";
            AttackSpeedBuff.buffColor = new Color32(205, 133, 49, 255);
            AttackSpeedBuff.canStack = false;
            AttackSpeedBuff.iconSprite = Sprite.Create(WarCrySprite, new Rect(0f, 0f, (float)WarCrySprite.width, (float)WarCrySprite.height), new Vector2(0f, 0f));
            AttackSpeedBuff.isDebuff = false;

            ContentAddition.AddBuffDef(AttackSpeedBuff);

            var Resupp = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Captain/CaptainSupplyDrop, EquipmentRestock.prefab").WaitForCompletion();
            var Indicator = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Captain/CaptainHealingWard.prefab").WaitForCompletion().transform.GetChild(0);
            Resupp.AddComponent<BuffWard>();
            var BW = Resupp.GetComponent<BuffWard>();
            BW.Networkradius = Radius;
            BW.shape = BuffWard.BuffWardShape.Sphere;
            BW.radius = Radius;
            BW.rangeIndicator = Indicator.transform;
            BW.buffDef = AttackSpeedBuff;
            BW.buffDuration = 1.5f;
            BW.interval = 0.5f;
            BW.floorWard = true;
            BW.expires = false;
            BW.invertTeamFilter = false;
            BW.expireDuration = 0f;
            BW.animateRadius = false;
            BW.requireGrounded = false;
            BW.teamFilter = Resupp.GetComponent<TeamFilter>();

            var WarbannerVisual = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/WardOnLevel/WarbannerWard.prefab").WaitForCompletion();
            var WarbannerVisual1 = WarbannerVisual.transform.GetChild(0);
            var WarbannerVisual2 = WarbannerVisual.transform.GetChild(0).GetChild(0);

            On.EntityStates.CaptainSupplyDrop.BaseCaptainSupplyDropState.OnEnter += (orig, self) =>
            {
                try
                {
                    var ResuppObj = GameObject.Find("CaptainSupplyDrop, EquipmentRestock(Clone)");
                    if (ResuppObj != null)
                    {
                        Object.Instantiate(WarbannerVisual1, ResuppObj.transform);
                        WarbannerVisual1.transform.localScale = new Vector3(Radius, Radius, Radius);
                        Object.Instantiate(WarbannerVisual2, ResuppObj.transform);
                    }
                }
                catch { Main.HCAPTLogger.LogError("GUHHHHHH IM MORBIN... 3... 2... 1.."); }
                orig(self);
            };
        }
    }
}