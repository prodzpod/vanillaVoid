﻿using BepInEx.Configuration;
using R2API;
using RoR2;
using vanillaVoid.Utils;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.AddressableAssets;
using HarmonyLib;
using static vanillaVoid.vanillaVoidPlugin;
using On.RoR2.Items;
using VoidItemAPI;
using RoR2.Projectile;

namespace vanillaVoid.Items
{
    public class ExtraterrestrialExhaust : ItemBase<ExtraterrestrialExhaust>
    {
        //public ConfigEntry<float> rocketsPerSecond;

        public ConfigEntry<float> rocketDamage;

        public ConfigEntry<float> rocketDamageStacking;

        public ConfigEntry<float> secondsPerRocket; 

        public override string ItemName => "Extraterrestrial Exhaust";

        public override string ItemLangTokenName => "EXT_EXHAUST_ITEM";

        public override string ItemPickupDesc => $"Upon activating a skill, fire a number of rockets depending on the skill's cooldown. <style=cIsVoid>Corrupts all {"{CORRUPTION}"}</style>.";

        public override string ItemFullDescription => $"Upon <style=cIsUtility>activating a skill</style>, <style=cIsDamage>fire a rocket</style> for <style=cIsUtility>every {secondsPerRocket.Value} seconds</style> of the skill's <style=cIsUtility>cooldown</style>, dealing <style=cIsDamage>{rocketDamage.Value}%</style> <style=cStack>(+{rocketDamageStacking.Value}% per stack)</style> base damage. <style=cIsVoid>Corrupts all {"{CORRUPTION}"}</style>.";

        public override string ItemLore => $"<style=cMono>//-- AUTO-TRANSCRIPTION FROM RALLYPOINT EPSILON RECORDER 7 --//</style>" +
            "\n\n\"I... I had a dream...and..and, I know it was a dream. There's no way it c-could've been anything else. I..it felt so real, but....it..It Could Not Have Been. It..it ccould not have been anything else, I-I refused to acceptt it. I have.. and will.. continue to remain safe in this shelter while the others are out, and and they'll, they'll be back soon. Any time now rreally. I will tend to my p-plants, and not worry myself ssick over this nonsense..it was a dream. It was all a ddream. I must've just..p-planted this one while I was tired..I...maybe I should get more sleep... \"";

        public override ItemTier Tier => ItemTier.VoidTier1;

        public override GameObject ItemModel => vanillaVoidPlugin.MainAssets.LoadAsset<GameObject>("mdlVentPickup.prefab");
       

        public override Sprite ItemIcon => vanillaVoidPlugin.MainAssets.LoadAsset<Sprite>("ventIcon512.png"); //amgous!!! AAAAAAA

        public static GameObject RocketProjectile;
        //public static GameObject RocketExplosion;

        public static GameObject ItemBodyModelPrefab;

        public override ItemTag[] ItemTags => new ItemTag[2] { ItemTag.Damage, ItemTag.AIBlacklist };

        public override void Init(ConfigFile config)
        {
            CreateConfig(config);
            CreateLang();
            CreateItem();
            ItemDef.requiredExpansion = vanillaVoidPlugin.sotvDLC;
            VoidItemAPI.VoidTransformation.CreateTransformation(ItemDef, voidPair.Value);
            CreateProjectile();
            Hooks(); 
        }

        public override void CreateConfig(ConfigFile config)
        {
            //rocketsPerSecond = config.Bind<float>("Item: " + ItemName, "Rockets per Second", 2f, "Adjust the number of rockets fired for each second of skill cooldown.");
            secondsPerRocket = config.Bind<float>("Item: " + ItemName, "Seconds per Rocket", 2f, "Adjust the number of seconds of skill cooldown needed to fire a rocket.");
            rocketDamage = config.Bind<float>("Item: " + ItemName, "Rocket Base Damage Percent", 20f, "Adjust the percent damage dealt on the first stack.");
            rocketDamageStacking = config.Bind<float>("Item: " + ItemName, "Rocket Base Damage Percent Stacking", 20f, "Adjust the percent damage gained per stack.");
            voidPair = config.Bind<string>("Item: " + ItemName, "Item to Corrupt", "Firework", "Adjust which item this is the void pair of.");
        }

        private void CreateProjectile() 
        {
            RocketProjectile = PrefabAPI.InstantiateClone(Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Firework/FireworkProjectile.prefab").WaitForCompletion(), "RocketProjectile", true);
            var impactExplosion = RocketProjectile.GetComponent<ProjectileImpactExplosion>();
            impactExplosion.impactEffect = Addressables.LoadAssetAsync<GameObject>("RoR2/DLC1/MissileVoid/VoidImpactEffect.prefab").WaitForCompletion();
            

            var model = MainAssets.LoadAsset<GameObject>("ventOrb.prefab");
            
            model.AddComponent<NetworkIdentity>();
            model.AddComponent<ProjectileGhostController>();

            //var netID = model.GetComponent<NetworkIdentity>();
            

            var projectileController = RocketProjectile.GetComponent<ProjectileController>();
            projectileController.ghostPrefab = model;
            projectileController.startSound = "Play_item_void_critGlasses";
            projectileController.allowPrediction = false;
            //RocketProjectile.GetComponent<EffectComponent>().soundName = "Play_item_void_critGlasses";
            //RocketProjectile.GetComponent<ProjectileExplosion>();

            PrefabAPI.RegisterNetworkPrefab(RocketProjectile);
            //ProjectileAPI.Add(Projectile);
            ContentAddition.AddProjectile(RocketProjectile);

        }

        public override ItemDisplayRuleDict CreateItemDisplayRules()
        {
            
            ItemBodyModelPrefab = MainAssets.LoadAsset<GameObject>("mdlVentDisplay.prefab");
            var projectileModel = MainAssets.LoadAsset<GameObject>("ventOrb.prefab");
            
            string guardTexture = "RoR2/DLC1/gauntlets/mVoidRaidPortalEdge.mat";
            string ballTexture = "RoR2/DLC1/VoidSurvivor/matVoidSurvivorBlasterSphereAreaIndicator.mat";
            //string outerBall = "RoR2/DLC1/VoidRaidCrab/matVoidRaidCrabEye.mat";


            var itemDisplay = ItemBodyModelPrefab.AddComponent<ItemDisplay>();
            itemDisplay.rendererInfos = ItemHelpers.ItemDisplaySetup(ItemBodyModelPrefab);

            var guardPickup = ItemModel.transform.Find("Guard").GetComponent<MeshRenderer>();
            guardPickup.material = Addressables.LoadAssetAsync<Material>(guardTexture).WaitForCompletion();

            //var testShell = ItemModel.transform.Find("Orb").GetComponent<MeshRenderer>();
            //testShell.material = Addressables.LoadAssetAsync<Material>(ballTexture).WaitForCompletion();


            var guardDisplay = ItemBodyModelPrefab.transform.Find("Guard").GetComponent<MeshRenderer>();
            guardDisplay.material = Addressables.LoadAssetAsync<Material>(guardTexture).WaitForCompletion();

            var projCenter = projectileModel.transform.Find("Center").GetComponent<MeshRenderer>();
            projCenter.material = Addressables.LoadAssetAsync<Material>(ballTexture).WaitForCompletion();

            var projShell = projectileModel.transform.Find("Orb").GetComponent<MeshRenderer>();
            projShell.material = Addressables.LoadAssetAsync<Material>(ballTexture).WaitForCompletion();

            ItemDisplayRuleDict rules = new ItemDisplayRuleDict();
            rules.Add("mdlCommandoDualies", new RoR2.ItemDisplayRule[]{
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Chest",
                    localPos = new Vector3(0.0110546f, 0.4341825f, -0.2253848f),
                    localAngles = new Vector3(359.2442f, 167.7944f, 354.3296f),
                    localScale = new Vector3(0.03f, 0.03f, 0.03f)
                }
            });
            rules.Add("mdlHuntress", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Chest",
                    localPos = new Vector3(0.1399796f, 0.2696737f, -0.1383592f),
                    localAngles = new Vector3(29.00727f, 121.5664f, 2.413216f),
                    localScale = new Vector3(0.0225f, 0.0225f, 0.0225f)
                }
            });
            rules.Add("mdlBandit2", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Chest",
                    localPos = new Vector3(0.008324906f, 0.2495107f, -0.2114665f),
                    localAngles = new Vector3(4.861009f, 157.5361f, 355.5444f),
                    localScale = new Vector3(0.025f, 0.025f, 0.025f)
                }
            });
            rules.Add("mdlToolbot", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Chest",
                    localPos = new Vector3(0.4376241f, 2.32833f, -2.461288f),
                    localAngles = new Vector3(1.790418f, 91.38383f, 351.9803f),
                    localScale = new Vector3(0.1f, 0.1f, 0.1f)
                }
            });
            rules.Add("mdlEngi", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Chest",
                    localPos = new Vector3(-0.01026348f, 0.4762573f, -0.2577912f),
                    localAngles = new Vector3(0.757121f, 84.66977f, 4.730944f),
                    localScale = new Vector3(0.035f, 0.035f, 0.035f)
                }
            });
            rules.Add("mdlEngiTurret", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule //alt turret
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Head",
                    localPos = new Vector3(-0.4434818f, 1.443157f, -1.160039f),
                    localAngles = new Vector3(0F, 270F, 0f),
                    localScale = new Vector3(0.05f, 0.05f, 0.05f)

                    //localPos = new Vector3(0.3982559f, 0.5157748f, 1.197929f), //std turret
                    //localAngles = new Vector3(2.650187f, 268.003f, 247.601f),
                    //localScale = new Vector3(.25f, .25f, .25f)
                }
            });
            rules.Add("mdlMage", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Chest",
                    localPos = new Vector3(0.1097722f, 0.378505f, -0.1722545f),
                    localAngles = new Vector3(355.193f, 120.7892f, 7.731801f),
                    localScale = new Vector3(0.0175f, 0.0175f, 0.0175f)
                },
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Chest",
                    localPos = new Vector3(-0.1149599f, 0.3788371f, -0.1733119f),
                    localAngles = new Vector3(355.193f, 120.7892f, 7.731801f),
                    localScale = new Vector3(0.0175f, 0.0175f, 0.0175f)
                }

            });
            rules.Add("mdlMerc", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Chest",
                    localPos = new Vector3(-0.006229824f, 0.1942842f, -0.3125757f),
                    localAngles = new Vector3(9.990238f, 94.72073f, 283.2024f),
                    localScale = new Vector3(0.015f, 0.0125f, 0.015f)
                }
            });
            rules.Add("mdlTreebot", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "FootFrontR",
                    localPos = new Vector3(-0.1326539f, 0.3482078f, -0.02275122f),
                    localAngles = new Vector3(349.3916f, 271.9016f, 183.2796f),
                    localScale = new Vector3(0.02f, 0.02f, 0.02f)
                }
            });
            rules.Add("mdlLoader", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Chest",
                    localPos = new Vector3(0.1311413f, 0.5595166f, -0.2539013f),
                    localAngles = new Vector3(0f, 91.21104f, 4.466461f),
                    localScale = new Vector3(0.02f, 0.02f, 0.02f)
                }
            });
            rules.Add("mdlCroco", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Chest",
                    localPos = new Vector3(2.481646f, 0.7097853f, 3.123438f),
                    localAngles = new Vector3(322.2655f, 121.3876f, 105.6704f),
                    localScale = new Vector3(0.125f, 0.125f, 0.125f)
                }
            });
            rules.Add("mdlCaptain", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Chest",
                    localPos = new Vector3(0.02279982f, 0.3192666f, -0.2125499f),
                    localAngles = new Vector3(357.248f, 212.8494f, 12.82024f),
                    localScale = new Vector3(0.025f, 0.025f, 0.025f)
                }
            });
            rules.Add("mdlRailGunner", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "BottomRail",
                    localPos = new Vector3(-0.001957409f, 0.7204793f, -0.06810854f),
                    localAngles = new Vector3(5.573401f, 188.9093f, 0f),
                    localScale = new Vector3(0.02f, 0.02f, 0.02f)
                }
            });
            rules.Add("mdlVoidSurvivor", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "ForeArmL",
                    localPos = new Vector3(0.08418835f, 0.1827631f, 0.024172f),
                    localAngles = new Vector3(349.521f, 175.745f, 74.09998f),
                    localScale = new Vector3(0.01f, 0.01f, 0.01f)
                },
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "ForeArmL",
                    localPos = new Vector3(0.09908976f, 0.1211472f, 0.003674036f),
                    localAngles = new Vector3(356.6176f, 198.4006f, 71.32525f),
                    localScale = new Vector3(0.0075f, 0.0075f, 0.0075f)
                },
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "ForeArmL",
                    localPos = new Vector3(0.05727483f, 0.239799f, 0.04521416f),
                    localAngles = new Vector3(326.0425f, 334.65f, 283.8331f),
                    localScale = new Vector3(0.0045f, 0.0045f, 0.0045f)
                }
            });
            rules.Add("mdlScav", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Chest",
                    localPos = new Vector3(6.042785f, 3.98034f, 1.600928f),
                    localAngles = new Vector3(16.82158f, 136.9528f, 52.64933f),
                    localScale = new Vector3(1f, 1f, 1f)
                }
            });

            //Modded Chars 
            rules.Add("EnforcerBody", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName =  "Pelvis",
                    localPos =   new Vector3(0.1125845f, 0.007865861f, 0.2631455f),
                    localAngles = new Vector3(343.1581f, 318.323f, 260.9673f),
                    localScale = new Vector3(.025f, .035f, .025f)
                }
            });
            rules.Add("NemesisEnforcerBody", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Pelvis",
                    localPos =   new Vector3(0.0001687468f, -0.0008735295f, 0.008880665f),
                    localAngles = new Vector3(53.58856f, 299.2462f, 291.1205f),
                    localScale = new Vector3(.0004f, .0004f, .0004f)
                }
            });
            rules.Add("mdlPaladin", new RoR2.ItemDisplayRule[] //these ones don't work for some reason!
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Chest",
                    localPos =   new Vector3(-0.2453761f, 0.582325f, 0.009619509f),
                    localAngles = new Vector3(0.6984521f, 82.43644f, 0.5239916f),
                    localScale = new Vector3(.025f, .025f, .025f)
                }
            });
            //rules.Add("mdlChef", new RoR2.ItemDisplayRule[]
            //{
            //    new RoR2.ItemDisplayRule
            //    {
            //        ruleType = ItemDisplayRuleType.ParentedPrefab,
            //        followerPrefab = ItemBodyModelPrefab,
            //        childName = "Door",
            //        localPos = new Vector3(0f, 0f, 0f),
            //        localAngles = new Vector3(0f, 0f, 0f),
            //        localScale = new Vector3(0f, 0f, 0f)
            //    }
            //});
            rules.Add("mdlMiner", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "PickL",
                    localPos =   new Vector3(-0.001340121f, 0.0004611782f, 0.001501753f),
                    localAngles = new Vector3(20.54011f, 359.8128f, 8.332644f),
                    localScale = new Vector3(.00025f, .00025f, .00025f)
                }
            });
            //rules.Add("mdlSniper", new RoR2.ItemDisplayRule[]
            //{
            //    new RoR2.ItemDisplayRule
            //    {
            //        ruleType = ItemDisplayRuleType.ParentedPrefab,
            //        followerPrefab = ItemBodyModelPrefab,
            //        childName = "Body",
            //        localPos = new Vector3(0f, 0f, 0f),
            //        localAngles = new Vector3(0f, 0f, 0f),
            //        localScale = new Vector3(0f, 0f, 0f)
            //    }
            //});
            rules.Add("DancerBody", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "HandR",
                    localPos =   new Vector3(0.03721922f, 0.10253f, 0.07642128f),
                    localAngles = new Vector3(85.65891f, 155.6693f, 142.5529f),
                    localScale = new Vector3(.01f, .01f, .01f)
                }
            });
            rules.Add("JavangleMystBody", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "LowerTorso",
                    localPos =   new Vector3(0.1349856f, 0.2377253f, 0.03491673f),
                    localAngles = new Vector3(12.96118f, 99.2438f, 6.065573f),
                    localScale = new Vector3(.01f, .01f, .01f)
                }
            });
            rules.Add("mdlExecutioner", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Pelvis",
                    localPos = new Vector3(-0.001051476f, -0.0006094702f, 0.001399369f),
                    localAngles = new Vector3(355.4796f, 337.4572f, 181.8065f),
                    localScale = new Vector3(0.00015f, 0.00015f, 0.00015f)
                }
            });
            rules.Add("mdlNemmando", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Chest",
                    localPos = new Vector3(-0.001057187f, -0.0001041672f, -0.001190624f),
                    localAngles = new Vector3(5.730634f, 25.73074f, 1.60499f),
                    localScale = new Vector3(0.0001f, 0.0001f, 0.0001f)
                }
            });
            rules.Add("mdlDeputy", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "ThighL",
                    localPos = new Vector3(0.05601266f, 0.09349241f, 0.06325182f),
                    localAngles = new Vector3(357.7929f, 171.6497f, 170.6236f),
                    localScale = new Vector3(.0125f, .0125f, .0125f)
                }
            });
            rules.Add("mdlPathfinder", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "ThighR",
                    localPos = new Vector3(-0.07003837f, 0.1822689f, 0.04212472f),
                    localAngles = new Vector3(14.81966f, 107.9849f, 169.126f),
                    localScale = new Vector3(.0125f, .0125f, .0125f)
                }
            });
            return rules;

        }

        public override void Hooks()
        {
           // On.RoR2.CharacterBody.OnSkillActivated += FireProjectile; //implimented in main
        }

    }

}
