using MonoMod.RuntimeDetour;
using RoR2.Skills;
using R2API.Utils;

namespace HIFUCaptainTweaks.Misc
{
    public static class OrbitalsInHiddenRealms
    {
        private static Hook fuckyou;

        public static void HopooLore()
        {
            fuckyou = new Hook(
                typeof(CaptainOrbitalSkillDef).GetMethodCached("get_isAvailable"),
                typeof(OrbitalsInHiddenRealms).GetMethodCached(nameof(OrbitalSkillsHook)));
        }

        private static bool OrbitalSkillsHook(CaptainOrbitalSkillDef self)
        {
            return true;
        }
    }
}