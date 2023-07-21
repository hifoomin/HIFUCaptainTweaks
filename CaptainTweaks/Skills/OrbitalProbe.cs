using RoR2.Skills;
using UnityEngine.AddressableAssets;

namespace HIFUCaptainTweaks.Skills
{
    public class OrbitalProbe : TweakBase
    {
        public static float Damage;
        public static float Cooldown;
        public static float EmpoweredDamage;

        public override string Name => ": Utility : Orbital Probe";

        public override string SkillToken => "utility";

        public override string DescText => "<style=cIsDamage>Stunning</style>. Request up to <style=cIsDamage>3</style> Orbital Probes from the <style=cIsDamage>UES Safe Travels</style>. Each probe deals <style=cIsDamage>" + d(Damage) + " damage</style>.";

        public override void Init()
        {
            Damage = ConfigOption(10f, "Damage", "Decimal. Vanilla is 10");
            Cooldown = ConfigOption(9f, "Cooldown", "Vanilla is 11");
            EmpoweredDamage = ConfigOption(14f, "Empowered Damage", "Decimal. Vanilla is 10");
            base.Init();
        }

        public override void Hooks()
        {
            Changes();
        }

        public static void Changes()
        {
            On.EntityStates.AimThrowableBase.OnEnter += (orig, self) =>
            {
                if (self is EntityStates.Captain.Weapon.CallAirstrike1 || self is EntityStates.Captain.Weapon.CallAirstrike2 || self is EntityStates.Captain.Weapon.CallAirstrike3)
                {
                    self.damageCoefficient = Damage;
                    var amp = self.gameObject.GetComponent<Empower>();
                    if (amp != null && amp.IsEmpowered)
                    {
                        self.damageCoefficient = EmpoweredDamage;
                    }
                }
                orig(self);
            };
            var Probe = Addressables.LoadAssetAsync<SkillDef>("RoR2/Base/Captain/PrepAirstrike.asset").WaitForCompletion();
            Probe.baseRechargeInterval = Cooldown;
        }
    }
}