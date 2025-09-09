using System;
using System.Collections;
using Gungeon;
using MonoMod;
using UnityEngine;
using Alexandria.ItemAPI;
using Alexandria.Misc;

namespace GunRev
{

    public class NanoCannon : GunBehaviour
    {
        public static Projectile dupeNanobot;
        public static void Add()
        {
            Gun gun = ETGMod.Databases.Items.NewGun("Nanobot Cluster Cannon", "nanocannon");
            Game.Items.Rename("outdated_gun_mods:nanobot_cluster_cannon", "ai:nanobot_cluster_cannon");
            gun.gameObject.AddComponent<NanoCannon>();
            gun.SetShortDescription("High-Velocity Machine Atrocity");
            gun.SetLongDescription("Fires a payload of killer nanobots which splits into several controllable projectiles.\n\nThe result of a lengthy and rather unfortunate research process involving deadly micromachines, high-strength duct tape, and countless lawsuits. Nobody knows why (or how) it was completed.");
            gun.SetupSprite(null, "nanocannon_idle_001", 8);
            gun.TrimGunSprites();
            gun.SetAnimationFPS(gun.shootAnimation, 13);
            Gun other = PickupObjectDatabase.GetById(38) as Gun;
            gun.AddProjectileModuleFrom(other, true, false);
            gun.DefaultModule.ammoCost = 1;
            gun.DefaultModule.shootStyle = ProjectileModule.ShootStyle.SemiAutomatic;
            gun.DefaultModule.sequenceStyle = ProjectileModule.ProjectileSequenceStyle.Random;
            gun.reloadTime = 1f;
            gun.DefaultModule.cooldownTime = 0.6f;
            gun.DefaultModule.numberOfShotsInClip = 6;
            gun.SetBaseMaxAmmo(168);
            gun.quality = PickupObject.ItemQuality.A;
            gun.gunClass = GunClass.NONE;
            gun.barrelOffset.localPosition = new Vector3(1.875f, 0.6875f, 0f);
            gun.carryPixelOffset = new IntVector2(2, -8);
            gun.muzzleFlashEffects = (PickupObjectDatabase.GetById(32) as Gun).muzzleFlashEffects;
            gun.SetAnimationFPS(gun.shootAnimation, 20);

            Projectile projectile = UnityEngine.Object.Instantiate<Projectile>(gun.DefaultModule.projectiles[0]);
            projectile.SetProjectileSpriteRight("nanocannon_mainproj", 12, 6, false, tk2dBaseSprite.Anchor.MiddleCenter, 10, 4);
            projectile.gameObject.name = "canister";
            projectile.shouldRotate = true;
            projectile.gameObject.SetActive(false);
            FakePrefab.MarkAsFakePrefab(projectile.gameObject);
            UnityEngine.Object.DontDestroyOnLoad(projectile);
            gun.DefaultModule.projectiles[0] = projectile;
            projectile.baseData.damage = 8f;
            projectile.baseData.speed = 18f;
            projectile.baseData.range = 12f;
            projectile.hitEffects.alwaysUseMidair = true;
            projectile.hitEffects.overrideMidairDeathVFX = (PickupObjectDatabase.GetById(543) as Gun).DefaultModule.projectiles[0].hitEffects.overrideMidairDeathVFX;

            Projectile nanobot = UnityEngine.Object.Instantiate<Projectile>((PickupObjectDatabase.GetById(38) as Gun).DefaultModule.projectiles[0]);
            nanobot.SetProjectileSpriteRight("nanocannon_nanobot", 2, 2, false, tk2dBaseSprite.Anchor.MiddleCenter, 2, 2);
            nanobot.gameObject.name = "nanobot";
            nanobot.shouldRotate = true;
            nanobot.gameObject.SetActive(false);
            FakePrefab.MarkAsFakePrefab(nanobot.gameObject);
            UnityEngine.Object.DontDestroyOnLoad(nanobot);
            nanobot.baseData.damage = 2f;
            nanobot.baseData.speed = 16f;
            nanobot.baseData.range = 25f;
            nanobot.gameObject.AddComponent<RemoteBulletsProjectileBehaviour>();
            nanobot.hitEffects.alwaysUseMidair = true;
            nanobot.hitEffects.overrideMidairDeathVFX = (PickupObjectDatabase.GetById(32) as Gun).DefaultModule.projectiles[0].hitEffects.overrideMidairDeathVFX;
            nanobot.gameObject.AddComponent<NanobotSynergyBehaviour>();

            dupeNanobot = UnityEngine.Object.Instantiate<Projectile>(nanobot);
            dupeNanobot.gameObject.name = "nanobot dupe";
            dupeNanobot.gameObject.SetActive(false);
            FakePrefab.MarkAsFakePrefab(dupeNanobot.gameObject);
            UnityEngine.Object.DontDestroyOnLoad(dupeNanobot);

            SpawnProjModifier spawnProjModifier = projectile.gameObject.AddComponent<SpawnProjModifier>();
            spawnProjModifier.collisionSpawnStyle = SpawnProjModifier.CollisionSpawnStyle.RADIAL;
            spawnProjModifier.projectileToSpawnOnCollision = nanobot;
            spawnProjModifier.numberToSpawnOnCollison = 8;
            spawnProjModifier.spawnProjectilesOnCollision = true;

            gun.DefaultModule.ammoType = GameUIAmmoType.AmmoType.MEDIUM_BLASTER;
            ETGMod.Databases.Items.Add(gun, null, "ANY");
        }

        public override void OnPostFired(PlayerController player, Gun gun)
        {
            gun.PreventNormalFireAudio = true;
            AkSoundEngine.PostEvent("Play_WPN_m79grenadelauncher_shot_01", gun.gameObject);
        }

        private bool HasReloaded;
        public override void Update()
        {
            if (gun.CurrentOwner)
            {
                if (!gun.IsReloading && !HasReloaded)
                {
                    this.HasReloaded = true;
                }
            }
        }

        public override void OnReloadPressed(PlayerController player, Gun gun, bool bSOMETHING)
        {
            if (gun.IsReloading && this.HasReloaded)
            {
                HasReloaded = false;
                AkSoundEngine.PostEvent("Stop_WPN_All", base.gameObject);
                base.OnReloadPressed(player, gun, bSOMETHING);
                AkSoundEngine.PostEvent("Play_WPN_SAA_spin_01", gun.gameObject);
            }
        }
    }
    public class NanobotSynergyBehaviour : MonoBehaviour
    {
        public void Start()
        {
            Projectile projectile = base.GetComponent<Projectile>();
            if (projectile.ProjectilePlayerOwner() != null)
            {
                if (projectile.ProjectilePlayerOwner().PlayerHasActiveSynergy("Self-Replication"))
                {
                    if (projectile.gameObject.name.Contains("nanobot") && !projectile.gameObject.name.Contains("dupe"))
                    {
                        SpawnProjModifier spawnProjModifier = projectile.gameObject.AddComponent<SpawnProjModifier>();
                        spawnProjModifier.collisionSpawnStyle = SpawnProjModifier.CollisionSpawnStyle.RADIAL;
                        spawnProjModifier.projectileToSpawnOnCollision = NanoCannon.dupeNanobot;
                        spawnProjModifier.numberToSpawnOnCollison = 2;
                        spawnProjModifier.spawnProjectilesOnCollision = true;
                    }
                }
                if (projectile.ProjectilePlayerOwner().PlayerHasActiveSynergy("Shredder Swarm"))
                {
                    if (projectile.gameObject.name.Contains("nanobot"))
                    {
                        PierceProjModifier pierceProjModifier = projectile.gameObject.GetOrAddComponent<PierceProjModifier>();
                        pierceProjModifier.penetration += 3;
                        pierceProjModifier.penetratesBreakables = false;
                        projectile.baseData.range *= 1.6f;
                        projectile.AdjustPlayerProjectileTint(new Color(74f/255f, 246f/255f, 28f/255f), 2);
                        projectile.hitEffects.overrideMidairDeathVFX = (PickupObjectDatabase.GetById(89) as Gun).DefaultModule.projectiles[0].hitEffects.overrideMidairDeathVFX;
                    }
                }
                if (projectile.ProjectilePlayerOwner().PlayerHasActiveSynergy("Molecular Magnets"))
                {
                    if (projectile.gameObject.name.Contains("nanobot"))
                    {
                        RemoteBulletsProjectileBehaviour remote = projectile.gameObject.GetOrAddComponent<RemoteBulletsProjectileBehaviour>();
                        remote.trackingSpeed *= 2f;
                        remote.trackingTime *= 2f;
                        projectile.baseData.range *= 1.6f;
                    }
                }
            }
        }
    }
}