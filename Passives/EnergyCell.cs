using System;
using System.Collections;
using System.Collections.Generic;
using Dungeonator;
using EnemyAPI;
using Gungeon;
using Alexandria.ItemAPI;
using UnityEngine;
using SaveAPI;
using Microsoft.Win32;
using Alexandria.Misc;
using Alexandria;
using HutongGames.PlayMaker.Actions;

namespace GunRev
{
    public class EnergyCell : PassiveItem
    {

        public static void Init()
        {
            string name = "Energy Cell";
            string resourcePath = "GunRev/Resources/Passives/energycell";
            GameObject gameObject = new GameObject();
            var companionItem = gameObject.AddComponent<CompanionItem>();
            ItemBuilder.AddSpriteToObject(name, resourcePath, gameObject);
            string shortDesc = "Do You Still Not Get It?";
            string longDesc = "Spawns an energy module which gains charge over time while in combat. At max charge, you gain a fire rate and reload speed boost. Dodge rolling or taking damage while in combat will reset the charge.\n\nThe former property of an evil, arrogant machine. It seems to be part of a larger system, but several components are missing.";
            companionItem.SetupItem(shortDesc, longDesc, "ai");
            companionItem.quality = PickupObject.ItemQuality.B;
            companionItem.CompanionGuid = EnergyCell.energyModuleGuid;

            companionItem.Synergies = new CompanionTransformSynergy[]
                {
                new()
                {
                    RequiredSynergy = Synergies.moxModuleSynergyType,
                    SynergyCompanionGuid = EnergyCell.moxModuleGuid
                }
                };

            EnergyCell.BuildEnergyModulePrefab();
            EnergyCell.BuildMoxModulePrefab();
        }
        public static void BuildEnergyModulePrefab()
        {
            bool flag = EnergyCell.energyModulePrefab != null || CompanionBuilder.companionDictionary.ContainsKey(EnergyCell.energyModuleGuid);
            if (!flag)
            {
                EnergyCell.energyModulePrefab = CompanionBuilder.BuildPrefab("Energy Module", EnergyCell.energyModuleGuid, "GunRev/Resources/Companions/EnergyModule/ZeroEnergy/energymodule_idle_001", new IntVector2(0, 0), new IntVector2(7, 5));
                energyModulePrefab.AddAnimation("idle_empty", "GunRev/Resources/Companions/EnergyModule/ZeroEnergy/energymodule_idle", 7, CompanionBuilder.AnimationType.Idle, DirectionalAnimation.DirectionType.Single, DirectionalAnimation.FlipType.None);
                energyModulePrefab.AddAnimation("idle_one", "GunRev/Resources/Companions/EnergyModule/OneEnergy/energymodule_idle", 7, CompanionBuilder.AnimationType.Idle, DirectionalAnimation.DirectionType.Single, DirectionalAnimation.FlipType.None);
                energyModulePrefab.AddAnimation("idle_two", "GunRev/Resources/Companions/EnergyModule/TwoEnergy/energymodule_idle", 7, CompanionBuilder.AnimationType.Idle, DirectionalAnimation.DirectionType.Single, DirectionalAnimation.FlipType.None);
                energyModulePrefab.AddAnimation("idle_three", "GunRev/Resources/Companions/EnergyModule/ThreeEnergy/energymodule_idle", 7, CompanionBuilder.AnimationType.Idle, DirectionalAnimation.DirectionType.Single, DirectionalAnimation.FlipType.None);
                energyModulePrefab.AddAnimation("idle_four", "GunRev/Resources/Companions/EnergyModule/FourEnergy/energymodule_idle", 7, CompanionBuilder.AnimationType.Idle, DirectionalAnimation.DirectionType.Single, DirectionalAnimation.FlipType.None);
                energyModulePrefab.AddAnimation("idle_five", "GunRev/Resources/Companions/EnergyModule/FiveEnergy/energymodule_idle", 7, CompanionBuilder.AnimationType.Idle, DirectionalAnimation.DirectionType.Single, DirectionalAnimation.FlipType.None);
                energyModulePrefab.AddAnimation("idle_full", "GunRev/Resources/Companions/EnergyModule/MaxEnergy/energymodule_idle", 7, CompanionBuilder.AnimationType.Idle, DirectionalAnimation.DirectionType.Single, DirectionalAnimation.FlipType.None);
                var companionController = EnergyCell.energyModulePrefab.AddComponent<EnergyModuleCompanionBehaviour>();
                companionController.CanInterceptBullets = false;
                companionController.CanCrossPits = true;
                companionController.companionID = CompanionController.CompanionIdentifier.NONE;
                companionController.aiActor.MovementSpeed = 12f;
                companionController.aiActor.healthHaver.PreventAllDamage = true;
                companionController.aiActor.CollisionDamage = 0f;
                companionController.aiActor.specRigidbody.CollideWithOthers = false;
                companionController.aiActor.specRigidbody.CollideWithTileMap = false;
                companionController.aiActor.IsWorthShootingAt = false;
                BehaviorSpeculator component = EnergyCell.energyModulePrefab.GetComponent<BehaviorSpeculator>();
                component.MovementBehaviors.Add(new CompanionFollowPlayerBehavior
                {
                    IdleAnimations = new string[]
                    {
                        "idle_empty"
                    },
                    IdealRadius = 1.5f
                });
            }
        }

        public static void BuildMoxModulePrefab()
        {
            bool flag = EnergyCell.moxModulePrefab != null || CompanionBuilder.companionDictionary.ContainsKey(EnergyCell.moxModuleGuid);
            if (!flag)
            {
                EnergyCell.moxModulePrefab = CompanionBuilder.BuildPrefab("Mox Module", EnergyCell.moxModuleGuid, "GunRev/Resources/Companions/MoxModule/ZeroEnergy/moxmodule_idle_001", new IntVector2(0, 0), new IntVector2(7, 5));
                moxModulePrefab.AddAnimation("idle_empty", "GunRev/Resources/Companions/MoxModule/ZeroEnergy/moxmodule_idle", 7, CompanionBuilder.AnimationType.Idle, DirectionalAnimation.DirectionType.Single, DirectionalAnimation.FlipType.None);
                moxModulePrefab.AddAnimation("idle_one", "GunRev/Resources/Companions/MoxModule/OneEnergy/moxmodule_idle", 7, CompanionBuilder.AnimationType.Idle, DirectionalAnimation.DirectionType.Single, DirectionalAnimation.FlipType.None);
                moxModulePrefab.AddAnimation("idle_two", "GunRev/Resources/Companions/MoxModule/TwoEnergy/moxmodule_idle", 7, CompanionBuilder.AnimationType.Idle, DirectionalAnimation.DirectionType.Single, DirectionalAnimation.FlipType.None);
                moxModulePrefab.AddAnimation("idle_three", "GunRev/Resources/Companions/MoxModule/ThreeEnergy/moxmodule_idle", 7, CompanionBuilder.AnimationType.Idle, DirectionalAnimation.DirectionType.Single, DirectionalAnimation.FlipType.None);
                moxModulePrefab.AddAnimation("idle_four", "GunRev/Resources/Companions/MoxModule/FourEnergy/moxmodule_idle", 7, CompanionBuilder.AnimationType.Idle, DirectionalAnimation.DirectionType.Single, DirectionalAnimation.FlipType.None);
                moxModulePrefab.AddAnimation("idle_five", "GunRev/Resources/Companions/MoxModule/FiveEnergy/moxmodule_idle", 7, CompanionBuilder.AnimationType.Idle, DirectionalAnimation.DirectionType.Single, DirectionalAnimation.FlipType.None);
                moxModulePrefab.AddAnimation("idle_full", "GunRev/Resources/Companions/MoxModule/MaxEnergy/moxmodule_idle", 7, CompanionBuilder.AnimationType.Idle, DirectionalAnimation.DirectionType.Single, DirectionalAnimation.FlipType.None);
                var companionController = EnergyCell.moxModulePrefab.AddComponent<MoxModuleCompanionBehaviour>();
                companionController.CanInterceptBullets = false;
                companionController.CanCrossPits = true;
                companionController.companionID = CompanionController.CompanionIdentifier.NONE;
                companionController.aiActor.MovementSpeed = 12f;
                companionController.aiActor.healthHaver.PreventAllDamage = true;
                companionController.aiActor.CollisionDamage = 0f;
                companionController.aiActor.specRigidbody.CollideWithOthers = false;
                companionController.aiActor.specRigidbody.CollideWithTileMap = false;
                companionController.aiActor.IsWorthShootingAt = false;
                BehaviorSpeculator component = EnergyCell.moxModulePrefab.GetComponent<BehaviorSpeculator>();
                component.MovementBehaviors.Add(new CompanionFollowPlayerBehavior
                {
                    IdleAnimations = new string[]
                    {
                        "idle_empty"
                    },
                    IdealRadius = 1.5f
                });
            }
        }

        public class EnergyModuleCompanionBehaviour : CompanionController
        {
            public EnergyModuleCompanionBehaviour()
            {
                this.DamagePerHit = 0f;
            }
            public override void OnDestroy()
            {
                Owner.OnPreDodgeRoll -= OnPlayerDodgeRoll;
                Owner.OnReceivedDamage -= OnPlayerDamaged;
                if (m_ModifiedStats)
                {
                    m_ModifiedStats = false;
                    Owner.ownerlessStatModifiers.Remove(fireSpeedMod);
                    Owner.ownerlessStatModifiers.Remove(reloadSpeedMod);
                    Owner.stats.RecalculateStats(Owner, true);
                }
                base.OnDestroy();
            }

            public void Start()
            {
                this.m_ModifiedStats = false;
                this.m_ChargeLevel = 0;
                this.m_ChargeTime = 0f;

                this.animNames = new string[]
                {
                    "idle_empty",
                    "idle_one",
                    "idle_two",
                    "idle_three",
                    "idle_four",
                    "idle_five",
                    "idle_full"
                };

                this.Owner = this.m_owner;

                Owner.OnPreDodgeRoll += OnPlayerDodgeRoll;
                Owner.OnReceivedDamage += OnPlayerDamaged;

                fireSpeedMod = new StatModifier
                {
                    statToBoost = PlayerStats.StatType.RateOfFire,
                    amount = 2f,
                    modifyType = StatModifier.ModifyMethod.MULTIPLICATIVE
                };

                reloadSpeedMod = new StatModifier
                {
                    statToBoost = PlayerStats.StatType.ReloadSpeed,
                    amount = 0.5f,
                    modifyType = StatModifier.ModifyMethod.MULTIPLICATIVE
                };

            }
            public override void Update()
            {
                if (Owner.IsInCombat && !GameManager.Instance.IsPaused)
                {
                    m_ChargeTime += BraveTime.DeltaTime;
                }

                m_ChargeLevel = (int)Math.Floor(m_ChargeTime) / 3;
                m_ChargeLevel = Math.Min(Math.Max(m_ChargeLevel, 0), 6);

                if (!this.aiAnimator.IsPlaying(this.animNames[m_ChargeLevel]))
                {
                    AkSoundEngine.PostEvent("Play_OBJ_mine_beep_01", this.gameObject);
                    this.aiAnimator.spriteAnimator.deferNextStartClip = false;
                    this.aiAnimator.PlayUntilCancelled(this.animNames[m_ChargeLevel]);
                }

                if (this.m_ChargeLevel == 6 && !m_ModifiedStats)
                {
                    m_ModifiedStats = true;
                    Owner.ownerlessStatModifiers.Add(fireSpeedMod);
                    Owner.ownerlessStatModifiers.Add(reloadSpeedMod);
                    Owner.stats.RecalculateStats(Owner, true);
                }

                if (this.m_ChargeLevel < 6 && m_ModifiedStats)
                {
                    m_ModifiedStats = false;
                    Owner.ownerlessStatModifiers.Remove(fireSpeedMod);
                    Owner.ownerlessStatModifiers.Remove(reloadSpeedMod);
                    Owner.stats.RecalculateStats(Owner, true);
                }

                base.Update();
            }
            private void OnPlayerDodgeRoll(PlayerController player)
            {
                if (player.IsInCombat)
                {
                    this.m_ChargeLevel = 0;
                    this.m_ChargeTime = 0f;
                }
                this.Update();
            }

            private void OnPlayerDamaged(PlayerController player)
            {
                if (player.IsInCombat)
                {
                    this.m_ChargeLevel = 0;
                    this.m_ChargeTime = 0f;
                }
                this.Update();
            }

            public float DamagePerHit;
            protected PlayerController Owner;
            protected string[] animNames;
            protected int m_ChargeLevel;
            protected float m_ChargeTime;
            protected bool m_ModifiedStats;
            protected StatModifier fireSpeedMod;
            protected StatModifier reloadSpeedMod;
        }

        public class MoxModuleCompanionBehaviour : EnergyModuleCompanionBehaviour
        {
            new public void Start()
            {
                emeraldMoxProj = UnityEngine.Object.Instantiate<Projectile>((PickupObjectDatabase.GetById(38) as Gun).DefaultModule.projectiles[0]);
                emeraldMoxProj.SetProjectileSpriteRight("emeraldmox", 8, 8, false, tk2dBaseSprite.Anchor.MiddleCenter, 6, 6);
                emeraldMoxProj.shouldRotate = true;
                emeraldMoxProj.gameObject.SetActive(false);
                FakePrefab.MarkAsFakePrefab(emeraldMoxProj.gameObject);
                UnityEngine.Object.DontDestroyOnLoad(emeraldMoxProj);
                emeraldMoxProj.baseData.damage = 12f;
                emeraldMoxProj.baseData.speed = 18f;
                emeraldMoxProj.baseData.range = 96f;
                emeraldMoxProj.AppliesPoison = true;
                emeraldMoxProj.PoisonApplyChance = 0.2f;

                rubyMoxProj = UnityEngine.Object.Instantiate<Projectile>((PickupObjectDatabase.GetById(38) as Gun).DefaultModule.projectiles[0]);
                rubyMoxProj.SetProjectileSpriteRight("rubymox", 8, 8, false, tk2dBaseSprite.Anchor.MiddleCenter, 6, 6);
                rubyMoxProj.shouldRotate = true;
                rubyMoxProj.gameObject.SetActive(false);
                FakePrefab.MarkAsFakePrefab(rubyMoxProj.gameObject);
                UnityEngine.Object.DontDestroyOnLoad(rubyMoxProj);
                rubyMoxProj.baseData.damage = 16f;
                rubyMoxProj.baseData.speed = 12f;
                rubyMoxProj.baseData.range = 32f;
                rubyMoxProj.AppliesFire = true;
                rubyMoxProj.FireApplyChance = 0.2f;

                sapphireMoxProj = UnityEngine.Object.Instantiate<Projectile>((PickupObjectDatabase.GetById(38) as Gun).DefaultModule.projectiles[0]);
                sapphireMoxProj.SetProjectileSpriteRight("sapphiremox", 8, 8, false, tk2dBaseSprite.Anchor.MiddleCenter, 6, 6);
                sapphireMoxProj.shouldRotate = true;
                sapphireMoxProj.gameObject.SetActive(false);
                FakePrefab.MarkAsFakePrefab(sapphireMoxProj.gameObject);
                UnityEngine.Object.DontDestroyOnLoad(sapphireMoxProj);
                sapphireMoxProj.baseData.damage = 12f;
                sapphireMoxProj.baseData.speed = 18f;
                sapphireMoxProj.baseData.range = 64f;
                var h = sapphireMoxProj.gameObject.GetOrAddComponent<HomingModifier>();
                h.HomingRadius = 32f;
                h.AngularVelocity = 32f;

                base.Start();
            }

            public override void Update()
            {
                if (this.m_ChargeLevel == 6)
                {
                    if ((int)Math.Floor(this.m_ChargeTime) % 2 == 0)
                    {
                        if (!this.m_HasFiredProjectile)
                        {
                            this.m_HasFiredProjectile = true;

                            AIActor closestEnemy = Owner.CurrentRoom.GetNearestEnemy(this.aiActor.CenterPosition, out _, true, true);

                            if (closestEnemy != null)
                            {
                                float angle = (closestEnemy.CenterPosition - this.aiActor.CenterPosition).ToAngle();

                                int proj = UnityEngine.Random.Range(1, 3);

                                GameObject tProj = null;

                                switch (proj)
                                {
                                    case 1:
                                        tProj = this.emeraldMoxProj.InstantiateAndFireInDirection(this.aiActor.CenterPosition, angle);
                                        tProj.GetComponent<Projectile>().Owner = Owner;
                                        tProj.GetComponent<Projectile>().Shooter = Owner.specRigidbody;
                                        break;
                                    case 2:
                                        tProj = this.rubyMoxProj.InstantiateAndFireInDirection(this.aiActor.CenterPosition, angle);
                                        tProj.GetComponent<Projectile>().Owner = Owner;
                                        tProj.GetComponent<Projectile>().Shooter = Owner.specRigidbody;
                                        break;
                                    case 3:
                                        tProj = this.sapphireMoxProj.InstantiateAndFireInDirection(this.aiActor.CenterPosition, angle);
                                        tProj.GetComponent<Projectile>().Owner = Owner;
                                        tProj.GetComponent<Projectile>().Shooter = Owner.specRigidbody;
                                        break;
                                }
                            }
                        }
                    }
                    else
                    {
                        this.m_HasFiredProjectile = false;
                    }
                }
                base.Update();
            }

            private Projectile emeraldMoxProj;
            private Projectile rubyMoxProj;
            private Projectile sapphireMoxProj;

            private bool m_HasFiredProjectile;
        }
        public static GameObject energyModulePrefab;
        public static GameObject moxModulePrefab;

        private static readonly string energyModuleGuid = "ai:energymodule";
        private static readonly string moxModuleGuid = "ai:moxmodule";
    }
}