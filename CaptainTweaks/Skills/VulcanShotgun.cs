using R2API;
using RoR2;
using RoR2.Skills;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace HIFUCaptainTweaks.Skills
{
    public class VulcanShotgun : TweakBase
    {
        public static float Damage;
        public static float ProcCoefficient;
        public static float FireRate;
        public static int PelletCount;
        public static int Charges;
        public static int ChargesToRecharge;
        public static float Cooldown;
        public static float EmpoweredFireRate;
        public static float ChargeDuration;
        public static float EmpoweredCooldown;

        public override string Name => ": Primary : Vulcan Shotgun";

        public override string SkillToken => "primary";

        public override string DescText => "Fire a blast of pellets that deal <style=cIsDamage>" + PelletCount + "x" + d(Damage) + " damage</style>. Charging the attack narrows the <style=cIsUtility>spread</style>.";

        public override void Init()
        {
            Damage = ConfigOption(0.75f, "Damage", "Decimal. Vanilla is 1.2");
            ProcCoefficient = ConfigOption(0.55f, "Proc Coefficient", "Vanilla is 0.75");
            FireRate = ConfigOption(3f, "Fire Rate", "Vanilla is 1");
            PelletCount = ConfigOption(6, "Pellet Count", "Vanilla is 8");
            Charges = ConfigOption(3, "Charges", "Vanilla is 0");
            ChargesToRecharge = ConfigOption(3, "Charges to Recharge", "Vanilla is 0");
            Cooldown = ConfigOption(1f, "Cooldown", "Vanilla is 0");
            EmpoweredFireRate = ConfigOption(5.25f, "Empowered Fire Rate", "Vanilla is 1");
            ChargeDuration = ConfigOption(0.4f, "Charging Duration", "Vanilla is 1.2");
            base.Init();
        }

        public override void Hooks()
        {
            Changes();
        }

        public static void Changes()
        {
            On.EntityStates.Captain.Weapon.ChargeCaptainShotgun.OnEnter += (orig, self) =>
            {
                EntityStates.Captain.Weapon.ChargeCaptainShotgun.baseChargeDuration = ChargeDuration;
                EntityStates.Captain.Weapon.ChargeCaptainShotgun.baseMinChargeDuration = 0f;
                orig(self);
            };
            On.EntityStates.Captain.Weapon.ChargeCaptainShotgun.FixedUpdate += (orig, self) =>
            {
                if (Util.HasEffectiveAuthority(self.outer.networkIdentity))
                {
                    if (self.fixedAge / self.chargeDuration > 1f && !self.outer.gameObject.GetComponent<CharacterBody>().isSprinting)
                    {
                        self.released = true;
                    }
                }
                orig(self);
            };
            On.EntityStates.GenericBulletBaseState.OnEnter += (orig, self) =>
            {
                if (self is EntityStates.Captain.Weapon.FireCaptainShotgun)
                {
                    self.bulletCount = PelletCount;
                    self.damageCoefficient = Damage;
                    self.procCoefficient = ProcCoefficient;
                    self.baseDuration = 1f / FireRate;
                    self.spreadPitchScale = 0.8f;
                    self.spreadYawScale = 0.8f;
                    self.force = 350f;
                    var amp = self.gameObject.GetComponent<Empower>();
                    if (amp != null && amp.IsEmpowered)
                    {
                        self.baseDuration = 1f / EmpoweredFireRate;
                    }
                }
                orig(self);
            };
            On.EntityStates.Captain.Weapon.FireCaptainShotgun.ModifyBullet += (On.EntityStates.Captain.Weapon.FireCaptainShotgun.orig_ModifyBullet orig, EntityStates.Captain.Weapon.FireCaptainShotgun self, BulletAttack bulletAttack) =>
            {
                orig(self, bulletAttack);
                bulletAttack.falloffModel = BulletAttack.FalloffModel.None;
            };
            var Shotgun = Addressables.LoadAssetAsync<SkillDef>("RoR2/Base/Captain/CaptainShotgun.asset").WaitForCompletion();
            Shotgun.baseMaxStock = Charges;
            Shotgun.rechargeStock = ChargesToRecharge;
            Shotgun.baseRechargeInterval = Cooldown;
            Shotgun.mustKeyPress = false;
            Shotgun.resetCooldownTimerOnUse = true;
        }
    }
}