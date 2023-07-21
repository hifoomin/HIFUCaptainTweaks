using RoR2;
using RoR2.Skills;
using UnityEngine.AddressableAssets;
using UnityEngine;
using RoR2.Projectile;
using R2API;

namespace HIFUCaptainTweaks.Skills
{
    public class PowerTazer : TweakBase
    {
        public static float Damage;
        public static float Cooldown;
        public static int Charges;
        public static int ChargesToRecharges;
        public static float AoE;
        public static float EmpowerDuration;
        public static float Size;

        public static SkillDef CaptainShotgun;
        public static SkillDef PrepAirstrike;
        public static SkillDef PrepAirstrikeAlt;

        public override string Name => ": Secondary : Thunder Tazer";

        public override string SkillToken => "secondary";

        public override string DescText => "<style=cIsUtility>Empowering</style>. <style=cIsDamage>Shocking</style>. Fire a fast tazer that deals <style=cIsDamage>" + d(Damage) + " damage</style> and <style=cIsUtility>empowers</style> your other abilities.";

        public override void Init()
        {
            Damage = ConfigOption(3.8f, "Damage", "Decimal. Vanilla is 1");
            Cooldown = ConfigOption(7f, "Cooldown", "Vanilla is 6");
            Charges = ConfigOption(1, "Charges", "Vanilla is 1");
            ChargesToRecharges = ConfigOption(1, "Charges to Recharge", "Vanilla is 1");
            AoE = ConfigOption(8f, "Area of Effect", "Vanilla is 2");
            EmpowerDuration = ConfigOption(1.5f, "Empower Duration", "Default is 1.5");
            Size = ConfigOption(2f, "Size Multiplier", "Vanilla is 1");
            CaptainShotgun = Addressables.LoadAssetAsync<SkillDef>("RoR2/Base/Captain/CaptainShotgun.asset").WaitForCompletion();
            PrepAirstrike = Addressables.LoadAssetAsync<SkillDef>("RoR2/Base/Captain/PrepAirstrike.asset").WaitForCompletion();
            PrepAirstrikeAlt = Addressables.LoadAssetAsync<SkillDef>("RoR2/Base/Captain/PrepAirstrikeAlt.asset").WaitForCompletion();
            base.Init();
        }

        public override void Hooks()
        {
            Changes();
        }

        public static void Changes()
        {
            LanguageAPI.Add("KEYWORD_EMPOWERING", "<style=cKeywordName>Empowering</style><style=cSub>Makes your abilities stronger in unique ways.\nVulcan Shotgun: <style=cIsDamage>+75% Attack Speed</style>.\nOrbital Probe: <style=cIsDamage>+400% Damage</style>.\nOGM-72 'DIABLO' Strike: <style=cIsDamage>+100% Radius</style>.</style>");
            string[] TazerKeywords = new string[2];
            TazerKeywords[0] = "KEYWORD_EMPOWERING";
            TazerKeywords[1] = "KEYWORD_SHOCKING";
            var TazerF = Addressables.LoadAssetAsync<SkillFamily>("RoR2/Base/Captain/CaptainSecondarySkillFamily.asset").WaitForCompletion();
            TazerF.variants[0].skillDef.keywordTokens = TazerKeywords;

            LanguageAPI.Add("CAPTAIN_SECONDARY_NAME", "Thunder Tazer");
            var TeslaSprite = Addressables.LoadAssetAsync<Texture2D>("RoR2/Base/ShockNearby/texBuffTeslaIcon.tif").WaitForCompletion();
            BuffDef Empowered = ScriptableObject.CreateInstance<BuffDef>();
            Empowered.name = "Empowered";
            Empowered.buffColor = new Color32(41, 99, 181, 255);
            Empowered.canStack = false;
            Empowered.iconSprite = Sprite.Create(TeslaSprite, new Rect(0f, 0f, (float)TeslaSprite.width, (float)TeslaSprite.height), new Vector2(0f, 0f));
            Empowered.isDebuff = false;

            ContentAddition.AddBuffDef(Empowered);

            On.EntityStates.Captain.Weapon.FireTazer.OnEnter += (orig, self) =>
            {
                EntityStates.Captain.Weapon.FireTazer.baseDuration = 1f;
                EntityStates.Captain.Weapon.FireTazer.baseDurationUntilPriorityLowers = 0.3f;
                EntityStates.Captain.Weapon.FireTazer.damageCoefficient = Damage;
                EntityStates.Captain.Weapon.FireTazer.force = 500f;
                if (self.gameObject.GetComponent<Empower>() == null)
                {
                    self.gameObject.AddComponent<Empower>();
                }
                self.gameObject.GetComponent<Empower>().IsEmpowered = true;
                self.gameObject.GetComponent<Empower>().EmpowerDuration = EmpowerDuration;
                self.characterBody.AddTimedBuffAuthority(Empowered.buffIndex, EmpowerDuration);
                orig(self);
            };
            var TazerSD = Addressables.LoadAssetAsync<SkillDef>("RoR2/Base/Captain/CaptainTazer.asset").WaitForCompletion();
            TazerSD.baseRechargeInterval = Cooldown;
            TazerSD.baseMaxStock = Charges;
            TazerSD.rechargeStock = ChargesToRecharges;

            var TazerP = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Captain/CaptainTazer.prefab").WaitForCompletion();
            TazerP.GetComponent<ProjectileSimple>().lifetime = 6f;
            TazerP.transform.localScale = new Vector3(Size, Size, Size);
            TazerP.GetComponent<ProjectileController>().ghostPrefab.transform.localScale = new Vector3(Size, Size, Size);
            var TPPIE = TazerP.GetComponent<ProjectileImpactExplosion>();
            TPPIE.lifetimeAfterImpact = 5f;
            TPPIE.lifetime = 6f;
            TPPIE.blastRadius = AoE;

            var EmpoweredSprites = Main.hifucaptaintweaks.LoadAsset<Texture2D>("texCaptainIcons.png");

            var CaptainSprites = Addressables.LoadAssetAsync<Texture2D>("RoR2/Base/Captain/texCaptainSkillIcons.png").WaitForCompletion();
            var RealVulcan = Sprite.Create(CaptainSprites, new Rect(128f, 384f, 128f, 128f), new Vector2(0f, 0f));
            var RealEmpoweredVulcan = Sprite.Create(EmpoweredSprites, new Rect(128f, 384f, 128f, 128f), new Vector2(0f, 0f));

            var RealProbe = Sprite.Create(CaptainSprites, new Rect(256f, 384f, 128f, 128f), new Vector2(0f, 0f));
            var RealEmpoweredProbe = Sprite.Create(EmpoweredSprites, new Rect(256f, 384f, 128f, 128f), new Vector2(0f, 0f));

            var RealProbe2 = Sprite.Create(CaptainSprites, new Rect(384f, 384f, 128f, 128f), new Vector2(0f, 0f));
            var RealEmpoweredProbe2 = Sprite.Create(EmpoweredSprites, new Rect(384f, 384f, 128f, 128f), new Vector2(0f, 0f));

            var RealDiablo = Sprite.Create(CaptainSprites, new Rect(384f, 0f, 128f, 128f), new Vector2(0f, 0f));
            var RealEmpoweredDiablo = Sprite.Create(EmpoweredSprites, new Rect(384f, 0f, 128f, 128f), new Vector2(0f, 0f));

            On.RoR2.CharacterBody.FixedUpdate += (orig, self) =>
            {
                Empower empower = self.gameObject.GetComponent<Empower>();
                if (empower != null && empower.IsEmpowered)
                {
                    empower.EmpowerDuration -= Time.fixedDeltaTime;
                    if (self.skillLocator.primary.skillDef == CaptainShotgun)
                    {
                        self.skillLocator.primary.skillDef.icon = RealEmpoweredVulcan;
                    }
                    if (self.skillLocator.utility.skillDef == PrepAirstrike)
                    {
                        self.skillLocator.utility.skillDef.icon = RealEmpoweredProbe;
                    }
                    if (self.skillLocator.utility.skillDef == PrepAirstrikeAlt)
                    {
                        self.skillLocator.utility.skillDef.icon = RealEmpoweredDiablo;
                    }
                    if (empower.EmpowerDuration <= 0)
                    {
                        if (self.skillLocator.primary.skillDef == CaptainShotgun)
                        {
                            self.skillLocator.primary.skillDef.icon = RealVulcan;
                        }
                        if (self.skillLocator.utility.skillDef == PrepAirstrike)
                        {
                            self.skillLocator.utility.skillDef.icon = RealProbe;
                        }
                        if (self.skillLocator.utility.skillDef == PrepAirstrikeAlt)
                        {
                            self.skillLocator.utility.skillDef.icon = RealDiablo;
                        }
                        empower.IsEmpowered = false;
                    }
                }
                orig(self);
            };
        }
    }

    public class Empower : MonoBehaviour
    {
        public bool IsEmpowered;
        public float EmpowerDuration;
    }
}