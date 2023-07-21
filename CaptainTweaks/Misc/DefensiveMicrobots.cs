using R2API;
using RoR2;
using System;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace HIFUCaptainTweaks.Misc
{
    public class DefensiveMicrobots : MiscBase
    {
        public static float Radius;
        public static float MinimumFireFrequency;
        public static float BaseRechargeFrequency;
        public static bool Formula;
        public static float SpeedBuff;
        public static bool StackSpeedBuff;
        public static float SpeedBuffDuration;
        public static BuffDef MoveSpeedBuff;
        public override string Name => ":: Misc : Defensive Microbots";

        public override void Init()
        {
            Radius = ConfigOption(20f, "Range", "Vanilla is 20");
            MinimumFireFrequency = ConfigOption(1.3f, "Maximum Blocks Per Second", "Vanilla is 10");
            BaseRechargeFrequency = ConfigOption(0.8f, "Blocks Per Second", "Vanilla is 2");
            Formula = ConfigOption(true, "Microbots Formula", "Lowest value between 1 / Maximum Blocks Per Second and (1 / (Blocks Per Second * Attack Speed Stat))");
            SpeedBuff = ConfigOption(0.12f, "Buff Speed", "Decimal. Vanilla is 0");
            StackSpeedBuff = ConfigOption(true, "Stack Speed Buff?", "Vanilla is false");
            SpeedBuffDuration = ConfigOption(3.5f, "Speed Buff Duration", "Vanilla is 0");
            base.Init();
        }

        public override void Hooks()
        {
            Changes();
            RecalculateStatsAPI.GetStatCoefficients += RecalculateStatsAPI_GetStatCoefficients;
        }

        private void RecalculateStatsAPI_GetStatCoefficients(CharacterBody sender, RecalculateStatsAPI.StatHookEventArgs args)
        {
            if (sender && sender.GetBuffCount(MoveSpeedBuff) > 0)
            {
                args.moveSpeedMultAdd += SpeedBuff * sender.GetBuffCount(MoveSpeedBuff);
            }
        }

        public static void Changes()
        {
            var HarpoonSprite = Addressables.LoadAssetAsync<Texture2D>("RoR2/DLC1/MoveSpeedOnKill/texBuffKillMoveSpeed.tif").WaitForCompletion();
            MoveSpeedBuff = ScriptableObject.CreateInstance<BuffDef>();
            MoveSpeedBuff.name = "Microbot Speed";
            MoveSpeedBuff.buffColor = new Color32(240, 196, 174, 255);
            MoveSpeedBuff.canStack = StackSpeedBuff;
            MoveSpeedBuff.iconSprite = Sprite.Create(HarpoonSprite, new Rect(0f, 0f, (float)HarpoonSprite.width, (float)HarpoonSprite.height), new Vector2(0f, 0f));
            MoveSpeedBuff.isDebuff = false;

            ContentAddition.AddBuffDef(MoveSpeedBuff);

            On.EntityStates.CaptainDefenseMatrixItem.DefenseMatrixOn.OnEnter += (orig, self) =>
            {
                EntityStates.CaptainDefenseMatrixItem.DefenseMatrixOn.baseRechargeFrequency = BaseRechargeFrequency;
                EntityStates.CaptainDefenseMatrixItem.DefenseMatrixOn.minimumFireFrequency = MinimumFireFrequency;
                EntityStates.CaptainDefenseMatrixItem.DefenseMatrixOn.projectileEraserRadius = Radius;
                orig(self);
            };
            On.EntityStates.CaptainDefenseMatrixItem.DefenseMatrixOn.DeleteNearbyProjectile += (orig, self) =>
            {
                bool original = orig(self);
                if (self.attachedBody != null)
                {
                    if (self.attachedBody.GetComponent<CharacterBody>() != null)
                    {
                        if (original)
                        {
                            self.attachedBody.GetComponent<CharacterBody>().AddTimedBuffAuthority(MoveSpeedBuff.buffIndex, SpeedBuffDuration);
                        }
                    }
                }
                return original;
            };

            LanguageAPI.Add("ITEM_CAPTAINDEFENSEMATRIX_DESC", "Shoot down <style=cIsDamage>1</style> <style=cStack>(+1 per stack)</style> projectiles within <style=cIsDamage>" + Radius + "m</style> every <style=cIsDamage>" +
                                                              (1f / BaseRechargeFrequency == 1f ? "second" : Math.Round(1f / BaseRechargeFrequency, 2) + " seconds</style>") +
                                                              (SpeedBuff > 0f ? ", gaining a small" + (StackSpeedBuff ? " stackable" : "") + " <style=cIsUtility>speed buff</style>." : ".") +
                                                              " <style=cIsUtility>Recharge rate scales with attack speed</style>.");
            LanguageAPI.Add("CAPTAIN_PASSIVE_DESCRIPTION", "Passively gain <style=cIsUtility>Microbots</style> that <style=cIsUtility>shoot down nearby enemy projectiles</style>" +
                                                            (SpeedBuff > 0 ? " and <style=cIsUtility>increase movement speed</style> for a short duration." : ".") +
                                                            " <style=cIsUtility>Drones</style> are also given <style=cIsUtility>Microbots</style>.");
        }
    }
}