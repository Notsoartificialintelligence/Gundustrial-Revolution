using Alexandria.ItemAPI;
using Dungeonator;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using HarmonyLib;
using System.Collections;

namespace GunRev
{
    class LowShieldPack : PlayerItem
    {
        public static void Init()
        {
            string itemName = "Low-Shield Pack";

            string resourceName = "GunRev/Resources/Actives/low_shield_pack";

            GameObject obj = new GameObject(itemName);

            var item = obj.AddComponent<LowShieldPack>();

            ItemBuilder.AddSpriteToObject(itemName, resourceName, obj);

            string shortDesc = "Glitter Tech";
            string longDesc = "Deploys a circular shield which allows bullets out, but not in. Burns out after a short time.\n\nTechnology heralding from a distant spacefaring civilisation. Many of their combat squads owe their lives to a well-timed deployment.";

            ItemBuilder.SetupItem(item, shortDesc, longDesc, "ai");

            ItemBuilder.SetCooldownType(item, ItemBuilder.CooldownType.None, 0f);

            item.consumable = true;
            item.quality = ItemQuality.A;
        }

        public override void DoEffect(PlayerController user)
        {
            IntVector2 playerPos = user.specRigidbody.UnitCenter.ToIntVector2(VectorConversions.Floor);

            CellData nearestTile = GameManager.Instance.Dungeon.data[playerPos];

            GameObject shieldpack = UnityEngine.GameObject.Instantiate<GameObject>(SpriteBuilder.SpriteFromResource("GunRev/Resources/Placeables/LowShieldPack/low_shield_pack.png"), new Vector3(0, 0), Quaternion.identity);

            GameObject shieldprojection = UnityEngine.GameObject.Instantiate<GameObject>(SpriteBuilder.SpriteFromResource("GunRev/Resources/Placeables/LowShieldPack/lowshield.png"), new Vector3(0, 0), Quaternion.identity);

            shieldpack.GetComponent<tk2dSprite>().sprite.PlaceAtPositionByAnchor((Vector2)nearestTile.position + new Vector2(0.5f, 0.5f), tk2dBaseSprite.Anchor.MiddleCenter);
            shieldpack.GetComponent<tk2dSprite>().sprite.HeightOffGround = -1f;
            shieldpack.GetComponent<tk2dSprite>().sprite.UpdateZDepth();

            shieldprojection.GetComponent<tk2dSprite>().sprite.PlaceAtPositionByAnchor((Vector2)nearestTile.position + new Vector2(0.5f, 0.5f), tk2dBaseSprite.Anchor.MiddleCenter);
            shieldprojection.GetComponent<tk2dSprite>().sprite.HeightOffGround = 4f;
            shieldprojection.GetComponent<tk2dSprite>().sprite.UpdateZDepth();

            shieldprojection.GetComponent<tk2dSprite>().sprite.renderer.material.shader = ShaderCache.Acquire("Brave/Internal/SimpleAlphaFadeUnlit");
            shieldprojection.GetComponent<tk2dSprite>().sprite.renderer.material.SetFloat("_Fade", 0.4f);

            ShieldPackDeteriorationBehav packbehav = shieldpack.GetOrAddComponent<ShieldPackDeteriorationBehav>();

            ShieldProjectionBehav projectionbehav = shieldprojection.GetOrAddComponent<ShieldProjectionBehav>();

            projectionbehav.collider = OtherTools.GenerateOrAddToRigidBody(shieldprojection, CollisionLayer.BulletBlocker, PixelCollider.PixelColliderGeneration.Circle, false, true, false, false, false, false, false, true, null, null, 80);
            projectionbehav.collider.transform.position = shieldprojection.GetComponent<tk2dSprite>().sprite.WorldBottomLeft;
            projectionbehav.collider.Reinitialize();

            ETGModConsole.Log(projectionbehav.collider.transform.position);
            ETGModConsole.Log(shieldprojection.GetComponent<tk2dSprite>().sprite.WorldBottomLeft);
            ETGModConsole.Log(shieldprojection.GetComponent<tk2dSprite>().sprite.transform.position);
            projectionbehav.collider.OnPreRigidbodyCollision += ShieldCollisionHandler;

            // Future Synergy to affect deterioration time

            packbehav.timeUntilDeterioration = 17f;
            projectionbehav.timeUntilDeterioration = 17f;

            base.DoEffect(user);
        }

        public override bool CanBeUsed(PlayerController user)
        {
            IntVector2 playerPos = user.specRigidbody.UnitCenter.ToIntVector2(VectorConversions.Floor);

            CellData nearestTile = GameManager.Instance.Dungeon.data[playerPos];

            return (nearestTile != null && nearestTile.type == CellType.FLOOR);
        }

        public void ShieldCollisionHandler(SpeculativeRigidbody myRigidbody, PixelCollider myCollider, SpeculativeRigidbody other, PixelCollider otherCollider)
        {
            if(otherCollider.CollisionLayer == CollisionLayer.Projectile)
            {
                if (other != null && other.projectile != null)
                {
                    ProjectileSpawnTracker t = other.projectile.gameObject.GetOrAddComponent<ProjectileSpawnTracker>();

                    if (t != null && t.initialPosition != null)
                    {
                        if (myCollider.ContainsPixel(PhysicsEngine.UnitToPixel(t.initialPosition)))
                        {
                            PhysicsEngine.SkipCollision = true;
                        }
                    }
                }
            }
        }
    }

    class ShieldPackDeteriorationBehav : MonoBehaviour
    {
        public float timeUntilDeterioration;
        protected float m_timeElapsed;
        public void Start()
        {
            this.m_timeElapsed = 0f;
        }

        public void Update()
        {
            if (!GameManager.Instance.IsPaused)
            {
                this.m_timeElapsed += BraveTime.DeltaTime;
            }

            if (this.m_timeElapsed >= this.timeUntilDeterioration)
            {
                ExplosionData explosionData = DungeonDatabase.GetOrLoadByName("base_castle").sharedSettingsPrefab.DefaultExplosionData;
                explosionData.doDamage = false;
                Exploder.Explode(this.gameObject.GetComponent<tk2dSprite>().WorldCenter, explosionData, this.gameObject.GetComponent<tk2dSprite>().WorldCenter);
                Destroy(this.gameObject);
            }
        }
    }

    class ShieldProjectionBehav : ShieldPackDeteriorationBehav
    {
        public SpeculativeRigidbody collider;

        new public void Update()
        {
            if (!GameManager.Instance.IsPaused)
            {
                this.m_timeElapsed += BraveTime.DeltaTime;
            }

            if (this.m_timeElapsed >= this.timeUntilDeterioration)
            {
                collider = null;
                StartCoroutine(ShieldDisappearEffect(3f));
            }
        }

        public IEnumerator ShieldDisappearEffect(float time)
        {
            if (!GameManager.Instance.IsPaused)
                for (float i = 0f; i <= time; i += BraveTime.DeltaTime)
                {
                    this.gameObject.GetComponent<tk2dSprite>().scale = new Vector3(1,1,1) * (1 - (time / i));
                }
                Destroy(this.gameObject);
            yield break;
        }
    }

    [HarmonyPatch]
    static class ProjectileSpawnApplier
    {

        [HarmonyPatch(typeof(Projectile), nameof(Projectile.Start))]
        [HarmonyPostfix]
        public static void TrackProjectileSpawn(Projectile __instance)
        {
            __instance.gameObject.GetOrAddComponent<ProjectileSpawnTracker>().initialPosition = __instance.transform.position;
        }
    }

    class ProjectileSpawnTracker : MonoBehaviour
    {
        public Vector2 initialPosition;
    }
}