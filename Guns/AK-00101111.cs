using System;
using System.Collections;
using Gungeon;
using MonoMod;
using UnityEngine;
using Alexandria.ItemAPI;
using System.Collections.Generic;
using Alexandria.VisualAPI;

namespace GunRev
{

    public class AK00101111 : GunBehaviour
    {

        public static void Add()
        {
            Gun gun = ETGMod.Databases.Items.NewGun("AK-00101111", "ak-00101111");
            Game.Items.Rename("outdated_gun_mods:ak00101111", "ai:ak00101111");
            gun.gameObject.AddComponent<AK00101111>();
            gun.SetShortDescription("Does Not Comshoot");
            gun.SetLongDescription("01010000 01010010 01001001 01001101 01000001 01010010 01011001 00100000 01000100 01001001 01010010 01000101 01000011 01010100 01001001 01010110 01000101 00111010 00100000 01000101 01001100 01001001 01001101 01001001 01001110 01000001 01010100 01000101 00100000 01010100 01000001 01010010 01000111 01000101 01010100 00101110\n\n01000001 01000011 01000011 01000101 01010000 01010100 00100000 01001110 01001111 00100000 01010011 01010101 01000010 01010011 01010100 01001001 01010100 01010101 01010100 01000101 01010011 00101110");
            gun.SetupSprite(null, "ak-00101111_idle_001", 8);
            gun.TrimGunSprites();
            gun.SetAnimationFPS(gun.shootAnimation, 16);
            Gun other = PickupObjectDatabase.GetById(38) as Gun;
            gun.AddProjectileModuleFrom(other, true, false);
            gun.DefaultModule.ammoCost = 1;
            gun.DefaultModule.shootStyle = ProjectileModule.ShootStyle.Automatic;
            gun.DefaultModule.sequenceStyle = ProjectileModule.ProjectileSequenceStyle.Ordered;

            gun.usesContinuousFireAnimation = true;

            gun.GetComponent<tk2dSpriteAnimator>().GetClipByName(gun.shootAnimation).wrapMode = tk2dSpriteAnimationClip.WrapMode.LoopSection;
            gun.GetComponent<tk2dSpriteAnimator>().GetClipByName(gun.shootAnimation).loopStart = 0;
            gun.GetComponent<tk2dSpriteAnimator>().GetClipByName(gun.shootAnimation).fps = 8;

            gun.muzzleFlashEffects = VFXBuilder.CreateVFXPool("Binary Muzzleflash", //Name of the muzzleflash
                new List<string>()
                { //Sprite paths
                    "GunRev/Resources/VFX/AK00101111/ak-00101111muzzlefx"
                },
                10, //FPS
                new IntVector2(15, 12), //Dimensions
                tk2dBaseSprite.Anchor.MiddleLeft, //Anchor
                false, //Uses a Z height off the ground
                0, //The Z height, if used
                false,
               VFXAlignment.Fixed
                  );

            gun.usesContinuousMuzzleFlash = false;

            gun.barrelOffset.localPosition = new Vector3(1.6875f, 0.375f, 0f);

            gun.reloadTime = 0.8f;
            gun.DefaultModule.cooldownTime = 0.11f;
            gun.DefaultModule.numberOfShotsInClip = 32;
            gun.SetBaseMaxAmmo(512);
            gun.DefaultModule.angleVariance = 4;
            gun.quality = PickupObject.ItemQuality.A;
            gun.gunClass = GunClass.FULLAUTO;

            Projectile zero = UnityEngine.Object.Instantiate<Projectile>(gun.DefaultModule.projectiles[0]);
            zero.SetProjectileSpriteRight("binary_zero", 6, 6, false, tk2dBaseSprite.Anchor.MiddleCenter, 4, 4);
            zero.shouldRotate = false;
            zero.gameObject.SetActive(false);
            FakePrefab.MarkAsFakePrefab(zero.gameObject);
            UnityEngine.Object.DontDestroyOnLoad(zero);
            gun.DefaultModule.projectiles[0] = zero;
            zero.baseData.damage = 8f;
            zero.baseData.speed = 24f;
            zero.baseData.range = 2147483647f;

            Projectile one = UnityEngine.Object.Instantiate<Projectile>(gun.DefaultModule.projectiles[0]);
            one.SetProjectileSpriteRight("binary_one", 6, 6, false, tk2dBaseSprite.Anchor.MiddleCenter, 4, 4);
            one.shouldRotate = false;
            one.gameObject.SetActive(false);
            FakePrefab.MarkAsFakePrefab(one.gameObject);
            UnityEngine.Object.DontDestroyOnLoad(one);
            gun.DefaultModule.projectiles.Add(one);

            gun.DefaultModule.ammoType = GameUIAmmoType.AmmoType.CUSTOM;
            gun.DefaultModule.customAmmoType = "green_small";
            ETGMod.Databases.Items.Add(gun, null, "ANY");
        }
        public override void OnPostFired(PlayerController player, Gun gun)
        {
            gun.PreventNormalFireAudio = true;
            AkSoundEngine.PostEvent("Play_OBJ_mine_beep_01", gameObject);
        }
        private bool HasReloaded;
        public override void Update()
        {
            if (gun.CurrentOwner)
            {

                if (!gun.PreventNormalFireAudio)
                {
                    this.gun.PreventNormalFireAudio = true;
                }
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
                AkSoundEngine.PostEvent("SND_WPN_ak47_reload_01", base.gameObject);
            }
        }
    }
}