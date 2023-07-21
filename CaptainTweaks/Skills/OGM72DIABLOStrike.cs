using RoR2.Projectile;
using RoR2.Skills;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace HIFUCaptainTweaks.Skills
{
    public class OGM72DIABLOStrike : TweakBase
    {
        public static float Damage;
        public static float Radius;
        public static float Cooldown;
        public static float TimeToLand;
        public static float EmpoweredRadius;
        public static SkillDef Diablo;

        public override string Name => ": Utility :: OGM-72 DIABLO Strike";

        public override string SkillToken => "utility_alt1";

        public override string DescText => "<style=cIsDamage>Stunning</style>. Request a <style=cIsDamage>kinetic strike</style> from the <style=cIsDamage>UES Safe Travels</style>. After <style=cIsUtility>" + TimeToLand + " seconds</style>, it deals <style=cIsDamage>" + d(Damage) + " damage</style> to ALL characters.";

        public override void Init()
        {
            Damage = ConfigOption(140f, "Damage", "Decimal. Vanilla is 400");
            Radius = ConfigOption(16f, "Radius", "Vanilla is 16");
            Cooldown = ConfigOption(24f, "Cooldown", "Vanilla is 40");
            TimeToLand = ConfigOption(15f, "Explosion Timer", "Vanilla is 20");
            EmpoweredRadius = ConfigOption(32f, "Empowered Radius", "Vanilla is 16");
            base.Init();
        }

        public override void Hooks()
        {
            Changes();
        }

        public static void Changes()
        {
            On.EntityStates.Captain.Weapon.CallAirstrikeBase.OnEnter += (orig, self) =>
            {
                if (self is EntityStates.Captain.Weapon.CallAirstrikeAlt)
                {
                    self.damageCoefficient = Damage;
                    self.projectilePrefab.transform.localScale = new Vector3(Radius / 16f, Radius / 16f, Radius / 16f);
                    self.projectilePrefab.GetComponent<ProjectileImpactExplosion>().blastRadius = Radius;
                    self.projectilePrefab.GetComponent<ProjectileImpactExplosion>().falloffModel = RoR2.BlastAttack.FalloffModel.None;
                    self.projectilePrefab.GetComponent<ProjectileController>().ghostPrefab.transform.localScale = new Vector3(Radius / 16f, Radius / 16f, Radius / 16f);
                    var amp = self.gameObject.GetComponent<Empower>();
                    if (amp != null && amp.IsEmpowered)
                    {
                        self.projectilePrefab.GetComponent<ProjectileImpactExplosion>().blastRadius = EmpoweredRadius;
                        self.projectilePrefab.transform.localScale = new Vector3(EmpoweredRadius / 16f, EmpoweredRadius / 16f, EmpoweredRadius / 16f);
                        self.projectilePrefab.GetComponent<ProjectileController>().ghostPrefab.transform.localScale = new Vector3(EmpoweredRadius / 16f, EmpoweredRadius / 16f, EmpoweredRadius / 16f);
                    }
                }
                orig(self);
            };
            On.EntityStates.AimThrowableBase.ModifyProjectile += (On.EntityStates.AimThrowableBase.orig_ModifyProjectile orig, EntityStates.AimThrowableBase self, ref FireProjectileInfo fireProjectileInfo) =>
            {
                orig(self, ref fireProjectileInfo);
                if (self is EntityStates.Captain.Weapon.CallAirstrikeAlt)
                {
                    fireProjectileInfo.useFuseOverride = true;
                    fireProjectileInfo.fuseOverride = TimeToLand;
                }
            };
            Addressables.LoadAssetAsync<SkillDef>("RoR2/Base/Captain/PrepAirstrikeAlt.asset").WaitForCompletion().baseRechargeInterval = Cooldown;
        }
    }
}