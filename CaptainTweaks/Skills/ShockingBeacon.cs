using Mono.Cecil.Cil;
using MonoMod.Cil;
using R2API;
using RoR2;
using RoR2.Skills;
using System;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace HIFUCaptainTweaks.Skills
{
    public class ShockingBeacon : TweakBase
    {
        public static float BigDamage;
        public static float BigFireRate;
        public static float BigRadius;

        public static float SmolDamage;
        public static float SmolFireRate;
        public static float SmolRadius;
        public static float SmolProcCoefficient;

        public static int StunType;

        public override string Name => ": Special :: Thunder Beacon";

        public override string SkillToken => "supply_shocking";

        public override string DescText => "Periodically <style=cIsDamage>stun</style>" +
                                           (BigDamage > 0f || SmolDamage > 0 ? " and <style=cIsDamage>damage</style>" : "") +
                                           " all nearby enemies, immobilizing them.";

        public override void Init()
        {
            BigDamage = ConfigOption(5f, "Burst Damage", "Decimal. Vanilla is 0");
            BigFireRate = ConfigOption(0.25f, "Burst Fire Rate", "Vanilla is 0.33334");
            BigRadius = ConfigOption(24f, "Burst Radius", "Vanilla is 10");
            SmolDamage = ConfigOption(1f, "Constant Damage", "Decimal. Vanilla is 0");
            SmolFireRate = ConfigOption(1f, "Constant Fire Rate", "Vanilla is 0");
            SmolRadius = ConfigOption(24f, "Constant Radius", "Vanilla is 0");
            SmolProcCoefficient = ConfigOption(0f, "Constant Proc Coefficient", "Vanilla is 0");
            StunType = ConfigOption(2, "Stun Type", "0 = None, 1 = Shock for 5s, 2 = Stun for 1s, Vanilla is 1");
            base.Init();
        }

        public override void Hooks()
        {
            Changes();
            IL.EntityStates.CaptainSupplyDrop.ShockZoneMainState.Shock += ShockZoneMainState_Shock;
            IL.EntityStates.CaptainSupplyDrop.ShockZoneMainState.Shock += ShockZoneMainState_Shock1;
            // IL.EntityStates.CaptainSupplyDrop.ShockZoneMainState.Shock += ShockZoneMainState_Shock2;
        }

        // private void ShockZoneMainState_Shock2(ILContext il)
        // {
        //    ILCursor c = new(il);
        //
        //      c.GotoNext(MoveType.After,
        //         x => x.MatchStfld("RoR2.BlastAttack", "radius")
        //      );
        //      c.Emit(OpCodes.Dup);
        //      c.Emit(OpCodes.Ldc_R4, BigProcCoefficient);
        //      c.Emit(OpCodes.Stfld, ("RoR2.BlastAttack", "procCoefficient"));
        //   }

        private void ShockZoneMainState_Shock1(ILContext il)
        {
            ILCursor c = new(il);

            if (c.TryGotoNext(MoveType.After,
                x => x.MatchLdcR4(0.0f)
            ))
            {
                c.Emit(OpCodes.Ldarg_0);
                c.EmitDelegate<Func<float, EntityStates.CaptainSupplyDrop.ShockZoneMainState, float>>((Damage, self) =>
                {
                    var owner = self.gameObject.GetComponent<Deployable>().ownerMaster;
                    float dam = owner.GetBody().damage * BigDamage;
                    return Damage + dam;
                });
            }
            else
            {
                Main.HCAPTLogger.LogError("Failed changing damage for Thunder Beacon.");
            }
        }

        public float timer = 69696969f;

        private void ShockZoneMainState_Shock(ILContext il)
        {
            timer += Time.fixedDeltaTime;
            float interval = 69696969f;

            ILCursor c = new(il);

            if (c.TryGotoNext(x => x.MatchLdcI4(16779264)))
            {
                c.Next.Operand = StunType switch
                {
                    0 => (int)DamageType.Silent,
                    2 => (int)DamageType.Silent | (int)DamageType.Stun1s,
                    _ => (int)DamageType.Silent | (int)DamageType.Shock5s
                };
            }
            else
            {
                if (timer > interval)
                {
                    Main.HCAPTLogger.LogError("Failed changing damage type for Thunder Beacon.");
                    timer = 0f;
                }
            }
        }

        public static void Changes()
        {
            LanguageAPI.Add("CAPTAIN_SUPPLY_SHOCKING_NAME", "Thunder Beacon");
            On.EntityStates.CaptainSupplyDrop.ShockZoneMainState.OnEnter += (orig, self) =>
            {
                EntityStates.CaptainSupplyDrop.ShockZoneMainState.shockFrequency = BigFireRate;
                EntityStates.CaptainSupplyDrop.ShockZoneMainState.shockRadius = BigRadius;
                orig(self);
            };
            float timer = 0f;
            On.EntityStates.CaptainSupplyDrop.ShockZoneMainState.FixedUpdate += (orig, self) =>
            {
                timer += Time.fixedDeltaTime;
                if (timer > (1f / SmolFireRate))
                {
                    if (self != null && self.gameObject != null && self.gameObject.GetComponent<Deployable>() != null && self.gameObject.GetComponent<Deployable>().ownerMaster != null && self.gameObject.GetComponent<Deployable>().ownerMaster.GetBodyObject() != null && self.gameObject.GetComponent<Deployable>().ownerMaster.GetBody() != null)
                    {
                        var owner = self.gameObject.GetComponent<Deployable>().ownerMaster;
                        new BlastAttack
                        {
                            attacker = owner.GetBodyObject(),
                            baseDamage = owner.GetBody().damage * SmolDamage,
                            baseForce = 0f,
                            crit = owner.GetBody().RollCrit(),
                            damageType = DamageType.Generic,
                            procCoefficient = SmolProcCoefficient,
                            radius = SmolRadius,
                            falloffModel = BlastAttack.FalloffModel.None,
                            position = self.transform.position,
                            attackerFiltering = AttackerFiltering.NeverHitSelf,
                            teamIndex = owner.GetBody().teamComponent.teamIndex
                        }.Fire();
                        EffectManager.SpawnEffect(EntityStates.CaptainSupplyDrop.ShockZoneMainState.shockEffectPrefab, new EffectData
                        {
                            origin = self.transform.position,
                            scale = SmolRadius,
                            color = new Color32(121, 184, 255, 50),
                        }, true);

                        timer = 0f;
                    }
                }
                orig(self);
            };
        }
    }
}