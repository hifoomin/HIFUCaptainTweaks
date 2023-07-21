using RoR2;
using UnityEngine;
using RoR2.Navigation;
using UnityEngine.AddressableAssets;
using RoR2.Skills;
using UnityEngine.Networking;
using System.Linq;

#pragma warning disable Publicizer001 // Accessing a member that was not originally public
#pragma warning disable IDE0051 // Remove unused private members

namespace HIFUCaptainTweaks.Misc
{
    public static class TryRespawn
    {
        public static void HopooBugs()
        {
            On.RoR2.Run.HandlePlayerFirstEntryAnimation += Run_HandlePlayerFirstEntryAnimation;
        }

        private static void Run_HandlePlayerFirstEntryAnimation(On.RoR2.Run.orig_HandlePlayerFirstEntryAnimation orig, Run self, CharacterBody body, Vector3 spawnPosition, Quaternion spawnRotation)
        {
            orig(self, body, spawnPosition, spawnRotation);
            if (NetworkServer.active)
            {
                switch (GameObject.Find("CaptainBody(Clone)") == null)
                {
                    case false:
                        foreach (var captainBodies in CharacterBody.instancesList.Where(x => x.name == "CaptainBody(Clone)"))
                        {
                            if (captainBodies.GetComponent<RespawnTimerILoveHopooGames>() == null)
                            {
                                captainBodies.gameObject.AddComponent<RespawnTimerILoveHopooGames>();
                            }
                        }
                        break;

                    default:
                        break;
                }
            }
        }

        public static void RespawnPlayer(CharacterMaster master)
        {
            if (master != null)
            {
                Vector3 playerSpawnTransform = Stage.instance.GetPlayerSpawnTransform().position;
                NodeGraph nodes = SceneInfo.instance.groundNodes;
                NodeGraph.NodeIndex nodeIndex = nodes.FindClosestNode(playerSpawnTransform, master.GetBody().hullClassification);
                nodes.GetNodePosition(nodeIndex, out playerSpawnTransform);
                master.Respawn(playerSpawnTransform, master.transform.rotation);
            }
            else
            {
                Main.HCAPTLogger.LogError("CaptainBody(Clone) Master null");
            }
        }
    }

    public class RespawnTimerILoveHopooGames : MonoBehaviour
    {
        public bool Active;
        public float Timer;
        public SkillDef Bullshit;
        public CharacterBody Body;

        private void Start()
        {
            Active = true;
            Bullshit = Addressables.LoadAssetAsync<SkillDef>("RoR2/Base/Captain/PrepSupplyDrop.asset").WaitForCompletion();
            Body = GetComponent<CharacterBody>();
        }

        private void FixedUpdate()
        {
            if (Active)
            {
                Timer += Time.fixedDeltaTime;
                if (Timer >= 3f)
                {
                    Active = false;
                    if (Body.GetComponent<SkillLocator>().special.skillDef == Bullshit && !((CaptainSupplyDropSkillDef.InstanceData)Body.GetComponent<SkillLocator>().special.skillInstanceData).anySupplyDropsAvailable)
                    {
                        TryRespawn.RespawnPlayer(Body.master);
                    }
                }
            }
        }
    }
}