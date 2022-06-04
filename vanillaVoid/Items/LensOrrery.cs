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
using VoidItemAPI;
using System.Linq;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using vanillaVoid.Misc;

namespace vanillaVoid.Items
{
    public class LensOrrery : ItemBase<LensOrrery>
    {
        public static ConfigEntry<float> newLensBonus;

        public static ConfigEntry<float> newStackingLensBonus;

        public static ConfigEntry<float> critModifier; 

        public static ConfigEntry<float> critModifierStacking;

        public ConfigEntry<float> additionalCritLevels;

        public static ConfigEntry<float> baseCrit;

        public ConfigEntry<string> voidPair;

        public override string ItemName => "Lens-Maker's Orrery";

        public override string ItemLangTokenName => "ORRERY_ITEM";

        //public override string ItemPickupDesc => "Increased effectiveness of lens-related items. Your Critical Strikes can dip an additional time. <style=cIsVoid>Corrupts all Laser Scopes</style>.";
        public override string ItemPickupDesc => "Lens-related items are more effective. Critical Strikes can occur an additional time, with half the chance of the previous one. <style=cIsVoid>Corrupts all Laser Scopes</style>.";
        
        //public override string ItemFullDescription => $"Gain <style=cIsDamage>{baseCrit.Value}% critical chance</style>. Lens-Maker's Glasses and Lost Seer's Lenses are <style=cIsUtility>{lensBonus.Value * 100}%</style> <style=cStack>(+{stackingLensBonus.Value * 100}% per stack)</style> <style=cIsUtility>more effective</style>. <style=cIsDamage>Critical strikes</style> can dip <style=cIsDamage>{additionalCritLevels.Value}</style> <style=cStack>(+{additionalCritLevels.Value} per stack)</style> additional times. <style=cIsVoid>Corrupts all Laser Scopes</style>.";
        public override string ItemFullDescription => tempItemFullDescription;
        //public override string ItemFullDescription => $"Gain <style=cIsDamage>{baseCrit.Value}% critical chance</style>. Lens-Maker's Glasses and Lost Seer's Lenses are <style=cIsUtility>{newLensBonus.Value * 100}%</style> <style=cIsUtility>more effective</style>. <style=cIsDamage>Critical strikes</style> can occur <style=cIsDamage>{additionalCritLevels.Value}</style> <style=cStack>(+{additionalCritLevels.Value} per stack)</style> additional times, with each additional occurance having <style=cIsDamage>{critModifier.Value * 100}%</style> <style=cStack>(+{critModifierStacking.Value * 100}% per stack)</style> of the crit chance of the previous crit. <style=cIsVoid>Corrupts all Laser Scopes</style>.";

        public override string ItemLore => $"<style=cSub>Order: Lens-Maker's Orrery \nTracking Number: ******** \nEstimated Delivery: 1/13/2072 \nShipping Method: High Priority/Fragile/Confidiential \nShipping Address: [REDACTED] \nShipping Details: \n\n</style>" + 
            "The Lens-Maker, as mysterious as they are influential. From my research I have surmised that she has been appointed to \"Final Verdict\", the most prestigious role of leadership in the House Beyond. Our team managed to locate a workshop of hers where she was supposedly working on some never-before concieved tech - but something was off. " +
            "Looking through her schematics and trinkets I found something odd - something unlike what I was anticipating. A simple orrery, clearly her design, but without her classic red, replaced with a peculiar purple. At first I worried that when she learned of our arrival, when she left in a rush, that we had ruined some of her masterpieces...but maybe it's best we interrupted her. " +
            "\n\nGiven that this is one of a kind, and quite a special work of hers at that; I expect much more than just currency in payment.";

        public override ItemTier Tier => ItemTier.VoidTier3;

        public override GameObject ItemModel => vanillaVoidPlugin.MainAssets.LoadAsset<GameObject>("mdlOrreryPickup.prefab");

        public override Sprite ItemIcon => vanillaVoidPlugin.MainAssets.LoadAsset<Sprite>("orreryIcon512.png");

        public static GameObject ItemBodyModelPrefab;

        public override ItemTag[] ItemTags => new ItemTag[1] { ItemTag.Damage };

        public static DamageColorIndex indexRed;
        public static DamageColorIndex indexOrange;
        public static DamageColorIndex indexYellow;
        public static DamageColorIndex indexGreen;
        public static DamageColorIndex indexCyan;
        public static DamageColorIndex indexBlue;
        public static DamageColorIndex indexPurple;
        public static DamageColorIndex indexPink;

        string tempItemFullDescription;


        public override void Init(ConfigFile config)
        {
            CreateConfig(config);
            if(newStackingLensBonus.Value == 0)
            {
                tempItemFullDescription = $"Gain <style=cIsDamage>{baseCrit.Value}% critical chance</style>. Lens-Maker's Glasses and Lost Seer's Lenses are <style=cIsUtility>{newLensBonus.Value * 100}%</style> <style=cIsUtility>more effective</style>. <style=cIsDamage>Critical strikes</style> can occur <style=cIsDamage>{additionalCritLevels.Value}</style> <style=cStack>(+{additionalCritLevels.Value} per stack)</style> additional times, with each additional occurance having <style=cIsDamage>{critModifier.Value * 100}%</style> <style=cStack>(+{critModifierStacking.Value * 100}% per stack)</style> of the crit chance of the previous crit. <style=cIsVoid>Corrupts all Laser Scopes</style>.";
            }
            else
            {
                tempItemFullDescription = $"Gain <style=cIsDamage>{baseCrit.Value}% critical chance</style>. Lens-Maker's Glasses and Lost Seer's Lenses are <style=cIsUtility>{newLensBonus.Value * 100}%</style> <style=cStack>(+{newStackingLensBonus.Value * 100} per stack)</style> <style=cIsUtility>more effective</style>. <style=cIsDamage>Critical strikes</style> can occur <style=cIsDamage>{additionalCritLevels.Value}</style> <style=cStack>(+{additionalCritLevels.Value} per stack)</style> additional times, with each additional occurance having <style=cIsDamage>{critModifier.Value * 100}%</style> <style=cStack>(+{critModifierStacking.Value * 100}% per stack)</style> of the crit chance of the previous crit. <style=cIsVoid>Corrupts all Laser Scopes</style>.";
            }
            CreateLang();
            CreateItem();
            ItemDef.requiredExpansion = vanillaVoidPlugin.sotvDLC;
            VoidItemAPI.VoidTransformation.CreateTransformation(ItemDef, voidPair.Value);

            ModdedDamageColors.ReserveColor(new Color(1f, .2f, .2f), out indexRed);    //old: (.95f, .05f, .05f) 
            ModdedDamageColors.ReserveColor(new Color(1f, .7f, .2f), out indexOrange); //old: (.95f, .5f, 0f)
            ModdedDamageColors.ReserveColor(new Color(1f, 1f, .2f), out indexYellow);  //old: (.95f, .9f, .2f)
            ModdedDamageColors.ReserveColor(new Color(.2f, 1f, .2f), out indexGreen);  //old: (.25f, .95f, .25f)
            ModdedDamageColors.ReserveColor(new Color(.2f, 1f, 1f), out indexCyan);    //old: (.2f, .95f, .9f)
            ModdedDamageColors.ReserveColor(new Color(.1f, .6f, 1f), out indexBlue);   //old: (0f, .5f, .95f)
            ModdedDamageColors.ReserveColor(new Color(.7f, .5f, 1f), out indexPurple); //old: (.6f, .2f, 1f)
            ModdedDamageColors.ReserveColor(new Color(1f, .6f, 1f), out indexPink);    //old: (.95f, .45f, 1f)

            Hooks();   
        }

        public override void CreateConfig(ConfigFile config)
        {
            string name = ItemName == "Lens-Maker's Orrery" ? "Lens-Makers Orrery" : ItemName;

            newLensBonus = config.Bind<float>("Item: " + name, "Glasses Buff", .3f, "Adjust the percent buff to crit glasses on the first stack.");
            newStackingLensBonus = config.Bind<float>("Item: " + name, "Glasses Buff per Stack", 0f, "Adjust the percent buff to crit glasses per stack. Recommened value is .1 if you want this mechanic.");
            additionalCritLevels = config.Bind<float>("Item: " + name, "Additional Crit Levels", 1f, "Adjust the number of additional crit levels each stack allows.");
            critModifier = config.Bind<float>("Item: " + name, "Crit Reduction", .5f, "Adjust how much the chance for additional crits are reduced. .5 is 50%, meaning subsequent crit chances are halved.");
            critModifierStacking = config.Bind<float>("Item: " + name, "Crit Reduction Reduction", .05f, "Adjust how much the crit reduction is reduced per stack. Basically, for every stack above the first, the crit reduction on subsequent crits is reduced by this amount. Having two stacks would make additonal crits have 55% of the previous's chance, assuming this number is default.");
            baseCrit = config.Bind<float>("Item: " + name, "Base Crit Increase", 5f, "Adjust the percent crit increase the first stack provides.");
            voidPair = config.Bind<string>("Item: " + name, "Item to Corrupt", "CritDamage", "Adjust which item this is the void pair of.");
        }

        public override ItemDisplayRuleDict CreateItemDisplayRules()
        {
            
            ItemBodyModelPrefab = vanillaVoidPlugin.MainAssets.LoadAsset<GameObject>("mdlOrreryDisplay.prefab");

            var itemDisplay = ItemBodyModelPrefab.AddComponent<ItemDisplay>();
            itemDisplay.rendererInfos = ItemHelpers.ItemDisplaySetup(ItemBodyModelPrefab);

            

            ItemDisplayRuleDict rules = new ItemDisplayRuleDict();
            rules.Add("mdlCommandoDualies", new RoR2.ItemDisplayRule[]{
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "GunR",
                    localPos = new Vector3(-0.3363554f, 0.07221243f, 0.002661751f),
                    localAngles = new Vector3(21.02493f, 182.9109f, 268.9313f),
                    localScale = new Vector3(1.825f, 1.825f, 1.825f)
                },
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "GunL",
                    localPos = new Vector3(0.3358954f, 0.07311542f, 0.0006113871f),
                    localAngles = new Vector3(340.4638f, 179.7426f, 90.17532f),
                    localScale = new Vector3(1.825f, 1.825f, 1.825f)
                }
            });
            rules.Add("mdlHuntress", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "BowBase",
                    localPos = new Vector3(0.0003809399f, -0.01058143f, -0.03347841f),
                    localAngles = new Vector3(53.28633f, 90.95795f, 271.0485f),
                    localScale = new Vector3(1.8f, 1.8f, 1.8f)
                }
            });
            rules.Add("mdlBandit2", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "SideWeapon",
                    localPos = new Vector3(0.001474246f, -0.3588007f, 0.1164518f),
                    localAngles = new Vector3(357.4492f, 62.51617f, 179.9416f),
                    localScale = new Vector3(1.385f, 1.4f, 1.385f)
                }
            });
            rules.Add("mdlToolbot", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Head",
                    localPos = new Vector3(1.815378f, 2.793304f, -0.5036061f),
                    localAngles = new Vector3(328.8118f, 359.6891f, 269.949f),
                    localScale = new Vector3(16f, 16f, 16f)
                }
            });
            rules.Add("mdlEngi", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule //i thought it'd be funny
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "CannonHeadL",
                    localPos = new Vector3(0.188132f, 0.2346339f, 0.1911812f),
                    localAngles = new Vector3(359.3283f, 134.9317f, 89.44088f),
                    localScale = new Vector3(3.4f, 3.4f, 3.4f)
                },
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "CannonHeadL",
                    localPos = new Vector3(0.1892091f, 0.3821511f, 0.192695f),
                    localAngles = new Vector3(45.06831f, 314.9317f, 269.0563f),
                    localScale = new Vector3(3.4f, 3.4f, 3.4f)
                },
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "CannonHeadR",
                    localPos = new Vector3(-0.188657f, 0.3832896f, 0.1915042f),
                    localAngles = new Vector3(359.4401f, 44.99557f, 90.67099f),
                    localScale = new Vector3(3.4f, 3.4f, 3.4f)
                },
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "CannonHeadR",
                    localPos = new Vector3(-0.1879471f, 0.2341934f, 0.1929879f),
                    localAngles = new Vector3(359.4401f, 44.99557f, 90.67099f),
                    localScale = new Vector3(3.4f, 3.4f, 3.4f)
                }
            });
            rules.Add("mdlEngiTurret", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule //alt turret
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Head",
                    localPos = new Vector3(0.009062123f, 0.7938066f, 0.8134468f),
                    localAngles = new Vector3(359.9267f, 271.632f, 271.0816f),
                    localScale = new Vector3(8.5f, 8.5f, 8.5f)

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
                    childName = "LowerArmL",
                    localPos = new Vector3(0.0158468f, 0.1841859f, -0.1127526f),
                    localAngles = new Vector3(357.0237f, 87.78452f, 270.8785f),
                    localScale = new Vector3(2.1f, 2.1f, 2.1f)
                },
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "LowerArmR",
                    localPos = new Vector3(0.03253355f, 0.1829497f, 0.1086318f),
                    localAngles = new Vector3(3.080307f, 277.278f, 269.6242f),
                    localScale = new Vector3(2.1f, 2.1f, 2.1f)
                },

            });
            rules.Add("mdlMerc", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Head",
                    localPos = new Vector3(0.1566377f, 0.07203895f, 0.05660409f),
                    localAngles = new Vector3(88.95958f, 195.8192f, 107.3834f),
                    localScale = new Vector3(2.1f, 2.1f, 2.1f)
                }
            });
            rules.Add("mdlTreebot", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "WeaponPlatform",
                    localPos = new Vector3(-0.0004612767f, 0.2993181f, 0.3306652f),
                    localAngles = new Vector3(0f, 270f, 270f),
                    localScale = new Vector3(4.25f, 4.25f, 4.25f)
                }
            });
            rules.Add("mdlLoader", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Head",
                    localPos = new Vector3(0.1467841f, 0.1565329f, 0.007586033f),
                    localAngles = new Vector3(0f, 0f, 281.5431f),
                    localScale = new Vector3(2.25f, 2.25f, 2.25f)
                }
            });
            rules.Add("mdlCroco", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "LowerArmL",
                    localPos = new Vector3(-0.7995564f, 3.801572f, -1.064525f),
                    localAngles = new Vector3(300.2426f, 317.4773f, 77.4535f),
                    localScale = new Vector3(20f, 20f, 20f)
                }
            });
            rules.Add("mdlCaptain", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "HandL",
                    localPos = new Vector3(0.06453719f, 0.4238173f, 0.00661204f),
                    localAngles = new Vector3(0, 121.5928f, 0),
                    localScale = new Vector3(2.15f, 2.15f, 2.15f)
                }
            });
            rules.Add("mdlRailGunner", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "GunScope",
                    localPos = new Vector3(-0.0005926741f, 0.2593113f, 0.04241726f),
                    localAngles = new Vector3(0f, 270f, 0f),
                    localScale = new Vector3(2.5f, 2.5f, 2.5f)
                }
            });
            rules.Add("mdlVoidSurvivor", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Hand",
                    localPos = new Vector3(0.05485039f, 0.09546384f, 0.008321242f),
                    localAngles = new Vector3(349.6481f, 185.366f, 88.44344f),
                    localScale = new Vector3(1.75f, 1.75f, 1.75f)
                }
            });
            rules.Add("mdlScav", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Weapon",
                    localPos = new Vector3(3.440224f, -0.4554433f, 2.75263f),
                    localAngles = new Vector3(1.632177f, 320.1595f, 268.687f),
                    localScale = new Vector3(64f, 64f, 64f)
                }
            });

            //Modded Chars 
            rules.Add("EnforcerBody", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName =  "Head",
                    localPos =   new Vector3(-0.0487977f, 0.1387017f, 0.1768962f),
                    localAngles = new Vector3(79.1538f, 262.0918f, 264.8381f),
                    localScale = new Vector3(3, 3, 3)
                }
            });
            rules.Add("NemesisEnforcerBody", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Hammer",
                    localPos =   new Vector3(-0.0002129737f, 0.0109493f, -0.01328757f),
                    localAngles = new Vector3(304.4799f, 91.18078f, 269.6079f),
                    localScale = new Vector3(.145f, .145f, .145f)
                }
            });
            rules.Add("mdlPaladin", new RoR2.ItemDisplayRule[] //these ones don't work for some reason!
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "Chest",
                    localPos = new Vector3(0f, 0f, 0f),
                    localAngles = new Vector3(0f, 0f, 0f),
                    localScale = new Vector3(0f, 0f, 0f)
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
                    localPos = new Vector3(-0.004894961f, 0.00135323f, 0f),
                    localAngles = new Vector3(359.5036f, 9.72984f, 88.70664f),
                    localScale = new Vector3(.025f, .025f, .025f)
                },
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "PickR",
                    localPos = new Vector3(0.004504088f, 0.0009339395f, -0.0008276533f),
                    localAngles = new Vector3(358.6043f, 5.191614f, 267.6915f),
                    localScale = new Vector3(.025f, .025f, .025f)
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
                    childName = "LanceBase",
                    localPos = new Vector3(0.00413096f, -0.05797194f, -0.002079286f),
                    localAngles = new Vector3(358.6873f, 180, 180),
                    localScale = new Vector3(2.4f, 2.4f, 2.4f)
                }
            });
            rules.Add("JavangleMystBody", new RoR2.ItemDisplayRule[]
            {
                new RoR2.ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = ItemBodyModelPrefab,
                    childName = "SwordTipLeft",
                    localPos = new Vector3(-0.01015798f, -1.148081f, -0.02965853f),
                    localAngles = new Vector3(0, 0, 0),
                    localScale = new Vector3(1.6f, 1.6f, 1.6f)
                }
            });
            return rules;

        }

        public override void Hooks()
        {
            //On.RoR2.HealthComponent.TakeDamage += OrreryCritBonus;
            On.RoR2.HealthComponent.TakeDamage += OrreryCritRework;
            RecalculateStatsAPI.GetStatCoefficients += CalculateStatsOrreryHook;
        }
        private static void CalculateStatsOrreryHook(CharacterBody sender, RecalculateStatsAPI.StatHookEventArgs args)
        {
            if (sender && sender.inventory)
            {
                //float levelBonus = sender.level - 1f;
                int glassesCount = sender.inventory.GetItemCount(RoR2Content.Items.CritGlasses);
                int orreryCount = sender.inventory.GetItemCount(ItemBase<LensOrrery>.instance.ItemDef);
                if (orreryCount > 0)
                {
                    args.critAdd += baseCrit.Value;
                    if (glassesCount > 0)
                    {
                        args.critAdd += (glassesCount * 10 * (newLensBonus.Value + ((orreryCount - 1) * newStackingLensBonus.Value)));
                    }
                }
            }
            
        }
        private void OrreryCritRework(On.RoR2.HealthComponent.orig_TakeDamage orig, HealthComponent self, DamageInfo damageInfo)
        {
            CharacterBody victimBody = self.body;
            if (damageInfo.attacker && damageInfo.attacker.GetComponent<CharacterBody>())
            {
                CharacterBody attackerBody = damageInfo.attacker.GetComponent<CharacterBody>();
                if (attackerBody.inventory)
                {
                    float orreryCount = GetCount(attackerBody);
                    //int glassesCount = attackerBody.inventory.GetItemCount(RoR2Content.Items.CritGlasses);
                    if (orreryCount > 0)
                    {
                        damageInfo.crit = false;
                        float critChanceModified = attackerBody.crit;
                        float critMult = attackerBody.critMultiplier;
                        float critMod = critModifier.Value + (critModifierStacking.Value * (orreryCount - 1));
                        float critCount = 0;
                        //bool test = false;
                        if(attackerBody.crit > 100)
                        {
                            critMult = attackerBody.critMultiplier + ((attackerBody.crit / 1000)); // crits are slightly stronger
                        }
                        for(int i = 0; i <= orreryCount; i++)
                        {
                            bool orreryCrit = Util.CheckRoll(critChanceModified, attackerBody.master.luck, attackerBody.master);
                            //Debug.Log("attempt " + i + ":");
                            if (orreryCrit)
                            {
                                critCount++;
                                //hitDamage *= critMult;
                                critChanceModified *= critMod;
                                damageInfo.crit = true;
                                
                                //Debug.Log("critted: " + orreryCrit);
                                //Debug.Log("random check 1 " + Util.CheckRoll(critChanceModified, attackerBody.master));
                                //Debug.Log("random check 2 " + Util.CheckRoll(critChanceModified, attackerBody.master));
                            }
                            else
                            {
                                break;
                            }
                        }
                        //damageInfo.damage = hitDamage;
                        //Debug.Log("critted: " + critCount + " times");
                        if (damageInfo.crit)    
                        {
                            //damageInfo.damage = hitDamage;
                            var temp = (damageInfo.damage * critMult) - damageInfo.damage;

                            damageInfo.damage = (critCount + 1) * temp;
                            if(critCount > 1)
                            {
                                //damageInfo.damage /= attackerBody.critMultiplier; //remove the extra crit 

                            }
                            //damageInfo.damage /= attackerBody.critMultiplier; //remove the extra crit 
                            //Debug.Log("critted: " + critCount + " times");
                            switch(critCount % 8) {
                                case 1:
                                    damageInfo.damageColorIndex = indexOrange;
                                    break;
                                case 2:
                                    damageInfo.damageColorIndex = indexRed;
                                    break;
                                case 3:
                                    damageInfo.damageColorIndex = indexPink;
                                    break;
                                case 4:
                                    damageInfo.damageColorIndex = indexPurple;
                                    break;
                                case 5:
                                    damageInfo.damageColorIndex = indexBlue;
                                    break;
                                case 6:
                                    damageInfo.damageColorIndex = indexCyan;
                                    break;
                                case 7:
                                    damageInfo.damageColorIndex = indexGreen;
                                    break;
                                case 0:
                                    damageInfo.damageColorIndex = indexYellow;
                                    break;
                            }
                        }
                    }
                }
            }
            orig(self, damageInfo);
        }
                    

        //private void OrreryCritBonus(On.RoR2.HealthComponent.orig_TakeDamage orig, HealthComponent self, DamageInfo damageInfo) {
        //    CharacterBody victimBody = self.body;
        //    if (damageInfo.attacker && damageInfo.attacker.GetComponent<CharacterBody>())
        //    {
        //        CharacterBody attackerBody = damageInfo.attacker.GetComponent<CharacterBody>();
        //        if (attackerBody.inventory)
        //        {
        //            var orreryCount = GetCount(attackerBody);
        //            //int glassesCount = attackerBody.inventory.GetItemCount(RoR2Content.Items.CritGlasses);
        //            if (orreryCount > 0)
        //            {
        //                var critChance = attackerBody.crit;
        //                //self.crit returns 100 if you have 100% chance
        //
        //                if (critChance > 100)
        //                {
        //                    float critMod = critChance % 100; //chance for next tier of crit
        //                    float baseLevel = ((critChance - critMod) / 100);
        //                    //Debug.Log("crit bonus level is " + baseLevel);
        //                    if (baseLevel >= orreryCount + 1)
        //                    {
        //                        baseLevel = orreryCount + 1; //cap it based on number of orrerys
        //                        //Debug.Log("crit was too high! bonus level is now " + baseLevel);
        //                    }
        //                    else
        //                    {
        //                        if (Util.CheckRoll(critMod, attackerBody.master))
        //                        {
        //                            baseLevel += 1;
        //
        //                            //Debug.Log("crited! bonus level is" + baseLevel);
        //                        }
        //                        else
        //                        {
        //                            //Debug.Log("no crit. bonus level is" + baseLevel);
        //                        }
        //                    }
        //                    //Debug.Log("damage was " + damageInfo.damage);
        //                    if (baseLevel > 1)
        //                    {
        //                        damageInfo.damage *= (attackerBody.critMultiplier * baseLevel);
        //                        //damageInfo.damageType |= DamageType.VoidDeath; 
        //                        damageInfo.damageColorIndex = DamageColorIndex.Void;
        //                        damageInfo.damage /= attackerBody.critMultiplier; //this is because the last crit (the normal one) isn't really handled here, and i didn't want to do an IL hook again
        //                    }
        //                    //Debug.Log("damage is " + damageInfo.damage);
        //
        //                    //sorry this is complete ass coding. i am stupid today.
        //                }
        //            }
        //
        //        }
        //        
        //    }
        //    orig(self, damageInfo);
        //}
    }
}
