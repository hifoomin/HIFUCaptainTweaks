using R2API;
using RoR2;
using UnityEngine;

using UnityEngine.AddressableAssets;

namespace HIFUCaptainTweaks.Misc
{
    public class AllBeacons : MiscBase
    {
        public static float Damage;

        public override string Name => ": Special ::::: All Beacons";

        public override void Init()
        {
            Damage = ConfigOption(13f, "Damage", "Decimal. Vanilla is 20");
            base.Init();
        }

        public override void Hooks()
        {
            Changes();
        }

        public static void Changes()
        {
            On.EntityStates.Captain.Weapon.CallSupplyDropBase.OnEnter += (orig, self) =>
            {
                EntityStates.Captain.Weapon.CallSupplyDropBase.impactDamageCoefficient = Damage;
                orig(self);
            };
        }
    }
}