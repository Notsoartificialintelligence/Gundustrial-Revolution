using System;
using System.Collections;
using Gungeon;
using MonoMod;
using UnityEngine;
using Alexandria.ItemAPI;
using System.Collections.Generic;
using Alexandria.VisualAPI;
using Alexandria.BreakableAPI;

namespace GunRev
{

    public class JunK47 : GunBehaviour
    {
        public GunAffixBehaviour affix;
        public static void Add()
        {
            Gun gun = ETGMod.Databases.Items.NewGun("JunK-47", "junk-47");
            Game.Items.Rename("outdated_gun_mods:junk47", "ai:junk47");
            var x = gun.gameObject.AddComponent<JunK47>();
            gun.SetShortDescription("Robo Rifle");
            gun.SetLongDescription("Has random affixes.");
            gun.SetupSprite(null, "junk-47_idle_001", 8);
            gun.TrimGunSprites();
            gun.SetAnimationFPS(gun.shootAnimation, 16);
            Gun other = PickupObjectDatabase.GetById(15) as Gun;
            gun.AddProjectileModuleFrom(other, true, false);
            gun.DefaultModule.ammoCost = 1;
            gun.DefaultModule.shootStyle = ProjectileModule.ShootStyle.Automatic;
            gun.DefaultModule.sequenceStyle = ProjectileModule.ProjectileSequenceStyle.Random;
            gun.gunSwitchGroup = (PickupObjectDatabase.GetById(15) as Gun).gunSwitchGroup;

            gun.usesContinuousFireAnimation = true;

            gun.GetComponent<tk2dSpriteAnimator>().GetClipByName(gun.shootAnimation).wrapMode = tk2dSpriteAnimationClip.WrapMode.LoopSection;
            gun.GetComponent<tk2dSpriteAnimator>().GetClipByName(gun.shootAnimation).loopStart = 0;
            gun.GetComponent<tk2dSpriteAnimator>().GetClipByName(gun.shootAnimation).fps = 8;

            gun.muzzleFlashEffects = other.muzzleFlashEffects;

            gun.usesContinuousMuzzleFlash = false;

            gun.barrelOffset.localPosition = new Vector3(1.6875f, 0.375f, 0f);

            gun.clipObject = BreakableAPIToolbox.GenerateDebrisObject("GunRev/Resources/Debris/junk47_clip.png", true, 1, 5, 60, 20, null, 1, null, null, 1).gameObject;
            gun.reloadClipLaunchFrame = 2;
            gun.clipsToLaunchOnReload = 1;

            gun.shellCasing = (PickupObjectDatabase.GetById(15) as Gun).shellCasing;
            gun.shellsToLaunchOnFire = 1;
            gun.shellsToLaunchOnReload = 0;

            gun.reloadTime = 0.5f;
            gun.DefaultModule.cooldownTime = 0.11f;
            gun.DefaultModule.numberOfShotsInClip = 30;
            gun.SetBaseMaxAmmo(500);
            gun.DefaultModule.angleVariance = 4;
            gun.quality = PickupObject.ItemQuality.A;
            gun.gunClass = GunClass.FULLAUTO;

            x.affix = gun.gameObject.AddComponent<GunAffixBehaviour>();
            gun.gameObject.GetComponent<GunAffixBehaviour>().GuaranteedAffixes = ["grenade"];

            gun.DefaultModule.ammoType = GameUIAmmoType.AmmoType.SMALL_BULLET;
            ETGMod.Databases.Items.Add(gun, null, "ANY");
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
            }
        }
    }
}