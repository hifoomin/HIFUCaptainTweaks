using RoR2;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace HIFUCaptainTweaks.Misc
{
    public class RefreshBeacons : MiscBase
    {
        public static float Damage;

        public override string Name => ": Special ::::: Refresh Beacons";

        public override void Init()
        {
            base.Init();
        }

        public List<GameObject> beacons = new();

        public override void Hooks()
        {
            On.EntityStates.CaptainSupplyDrop.BaseCaptainSupplyDropState.OnEnter += BaseCaptainSupplyDropState_OnEnter;
            On.RoR2.TeleporterInteraction.IdleToChargingState.OnEnter += IdleToChargingState_OnEnter;
        }

        private void BaseCaptainSupplyDropState_OnEnter(On.EntityStates.CaptainSupplyDrop.BaseCaptainSupplyDropState.orig_OnEnter orig, EntityStates.CaptainSupplyDrop.BaseCaptainSupplyDropState self)
        {
            beacons.Add(self.gameObject);
            orig(self);
        }

        private void IdleToChargingState_OnEnter(On.RoR2.TeleporterInteraction.IdleToChargingState.orig_OnEnter orig, EntityStates.BaseState self)
        {
            var explosion = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Common/VFX/ExplosionVFX.prefab").WaitForCompletion();
            orig(self);
            foreach (var playerCharacterMasterController in PlayerCharacterMasterController.instances)
            {
                var body = playerCharacterMasterController.body;
                if (body)
                {
                    var captainSupplyDropController = body.GetComponent<CaptainSupplyDropController>();
                    if (captainSupplyDropController)
                    {
                        captainSupplyDropController.supplyDrop1Skill.stock = 1;
                        captainSupplyDropController.supplyDrop2Skill.stock = 1;
                    }
                }
            }
            for (int i = 0; i < beacons.Count; i++)
            {
                var index = beacons[i];
                if (index)
                {
                    Object.Destroy(index);
                    EffectManager.SpawnEffect(explosion, new EffectData
                    {
                        origin = index.transform.position,
                        scale = 10f
                    }, false);
                }
                beacons.Remove(index);
            }
        }
    }
}