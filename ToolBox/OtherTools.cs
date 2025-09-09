using Dungeonator;
using Alexandria.ItemAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using tk2dRuntime.TileMap;
using UnityEngine;
using static DirectionalAnimation;
using static EnemyAPI.EnemyBuilder;
using System.Collections;

namespace GunRev
{
    public static class OtherTools
    {
        public static GameObject laserSightPrefab;
        public static void Init()
        {
            laserSightPrefab = LoadHelper.LoadAssetFromAnywhere("assets/resourcesbundle/global vfx/vfx_lasersight.prefab") as GameObject;
        }
        public static tk2dSpriteAnimationClip BuildAnimation(AIAnimator aiAnimator, string name, string spriteDirectory, int fps)
        {
            tk2dSpriteCollectionData collection = aiAnimator.GetComponent<tk2dSpriteCollectionData>();
            if (!collection)
                collection = SpriteBuilder.ConstructCollection(aiAnimator.gameObject, $"{aiAnimator.name}_collection");

            string[] resources = ResourceExtractor.GetResourceNames();
            List<int> indices = new List<int>();
            for (int i = 0; i < resources.Length; i++)
            {
                if (resources[i].StartsWith(spriteDirectory.Replace('/', '.') + ".", StringComparison.OrdinalIgnoreCase))
                {
                    indices.Add(SpriteBuilder.AddSpriteToCollection(resources[i], collection));
                }
            }
            tk2dSpriteAnimationClip clip = SpriteBuilder.AddAnimation(aiAnimator.spriteAnimator, collection, indices, name, tk2dSpriteAnimationClip.WrapMode.Once);
            clip.fps = fps;
            return clip;
        }
        public static GameObject RenderLaserSight(Vector2 position, float length, float angle, bool alterColour = false, Color? colour = null)
        {
            GameObject gameObject = SpawnManager.SpawnVFX(laserSightPrefab, position, Quaternion.Euler(0, 0, angle));

            tk2dTiledSprite component2 = gameObject.GetComponent<tk2dTiledSprite>();
            component2.dimensions = new Vector2(length, 1f);
            if (alterColour && colour != null)
            {
                component2.usesOverrideMaterial = true;
                component2.sprite.renderer.material.shader = ShaderCache.Acquire("Brave/LitTk2dCustomFalloffTintableTiltedCutoutEmissive");
                component2.sprite.renderer.material.SetColor("_OverrideColor", (Color)colour);
                component2.sprite.renderer.material.SetColor("_EmissiveColor", (Color)colour);
                component2.sprite.renderer.material.SetFloat("_EmissivePower", 100);
                component2.sprite.renderer.material.SetFloat("_EmissiveColorPower", 1.55f);
            }
            return gameObject;
        }
        public static Vector2 GetCursorPosition(this PlayerController user, float noCursorControllerRange)
        {
            Vector2 m_cachedBlinkPosition = Vector2.zero;

            GungeonActions m_activeActions = OMITBReflectionHelpers.ReflectGetField<GungeonActions>(typeof(PlayerController), "m_activeActions", user);

            bool IsKeyboardAndMouse = BraveInput.GetInstanceForPlayer(user.PlayerIDX).IsKeyboardAndMouse(false);
            if (IsKeyboardAndMouse) { m_cachedBlinkPosition = user.unadjustedAimPoint.XY() - (user.CenterPosition - user.specRigidbody.UnitCenter); }
            else
            {
                m_cachedBlinkPosition = user.PositionInDistanceFromAimDir(5);
                //if (m_activeActions != null) { m_cachedBlinkPosition += m_activeActions.Aim.Vector.normalized * BraveTime.DeltaTime * 15f; }
            }

            m_cachedBlinkPosition = BraveMathCollege.ClampToBounds(m_cachedBlinkPosition, GameManager.Instance.MainCameraController.MinVisiblePoint, GameManager.Instance.MainCameraController.MaxVisiblePoint);
            return m_cachedBlinkPosition;
        }
        public static Vector2 PositionInDistanceFromAimDir(this PlayerController player, float distance)
        {
            Vector2 vector = player.CenterPosition;
            Vector2 normalized = (player.unadjustedAimPoint.XY() - vector).normalized;
            Vector2 final = player.CenterPosition + normalized * distance;
            return final;
        }
        public static PlayerController ProjectilePlayerOwner(this Projectile bullet)
        {
            if (bullet && bullet.Owner && bullet.Owner is PlayerController) return bullet.Owner as PlayerController;
            else return null;
        }

        public static void DoRisingStringFade(string text, Vector2 point, Color colour, float heightOffGround = 3f, float opacity = 1f)
        {

            GameObject gameObject = (GameObject)UnityEngine.Object.Instantiate(BraveResources.Load("DamagePopupLabel", ".prefab"), GameUIRoot.Instance.transform);

            dfLabel label = gameObject.GetComponent<dfLabel>();
            label.gameObject.SetActive(true);
            label.Text = text;
            label.Color = colour;
            label.Opacity = opacity;
            label.TextAlignment = TextAlignment.Center;
            label.Anchor = dfAnchorStyle.Bottom;
            label.Pivot = dfPivotPoint.BottomCenter;
            label.Invalidate();
            label.transform.position = dfFollowObject.ConvertWorldSpaces(point, GameManager.Instance.MainCameraController.Camera, GameManager.Instance.MainCameraController.Camera).WithZ(0f);
            label.transform.position = label.transform.position.QuantizeFloor(label.PixelsToUnits() / (Pixelator.Instance.ScaleTileScale / Pixelator.Instance.CurrentTileScale));

            label.StartCoroutine(HandleDamageNumberRiseCR(point, point.y - heightOffGround, label));
        }
        private static IEnumerator HandleDamageNumberRiseCR(Vector3 startWorldPosition, float worldFloorHeight, dfLabel damageLabel)
        {
            float elapsed = 0f;
            float duration = 1.5f;
            float holdTime = 0f;
            Camera mainCam = GameManager.Instance.MainCameraController.Camera;
            Vector3 worldPosition = startWorldPosition;
            Vector3 lastVelocity = new Vector3(Mathf.Lerp(-8f, 8f, UnityEngine.Random.value), UnityEngine.Random.Range(15f, 25f), 0f);
            while (elapsed < duration)
            {
                float dt = BraveTime.DeltaTime;
                elapsed += dt;
                if (GameManager.Instance.IsPaused)
                {
                    break;
                }
                if (elapsed > holdTime)
                {

                    damageLabel.transform.position = dfFollowObject.ConvertWorldSpaces(new Vector3(worldPosition.x, worldPosition.y + (elapsed / duration)), mainCam, GameUIRoot.Instance.Manager.RenderCamera).WithZ(0f);
                }
                float t = elapsed / duration;
                damageLabel.Opacity = 1f - t;
                yield return null;
            }
            damageLabel.gameObject.SetActive(false);
            UnityEngine.Object.Destroy(damageLabel.gameObject, 1);
            yield break;
        }

        public static SpeculativeRigidbody GenerateOrAddToRigidBody(GameObject targetObject, CollisionLayer collisionLayer, PixelCollider.PixelColliderGeneration colliderGenerationMode = PixelCollider.PixelColliderGeneration.Tk2dPolygon, bool collideWithTileMap = false, bool CollideWithOthers = true, bool CanBeCarried = true, bool CanBePushed = false, bool RecheckTriggers = false, bool IsTrigger = false, bool replaceExistingColliders = false, bool UsesPixelsAsUnitSize = false, IntVector2? dimensions = null, IntVector2? offset = null, int ManualDiameter = -1)
        {
            SpeculativeRigidbody orAddComponent = targetObject.GetOrAddComponent<SpeculativeRigidbody>();
            orAddComponent.CollideWithOthers = CollideWithOthers;
            orAddComponent.CollideWithTileMap = collideWithTileMap;
            orAddComponent.Velocity = Vector2.zero;
            orAddComponent.MaxVelocity = Vector2.zero;
            orAddComponent.ForceAlwaysUpdate = false;
            orAddComponent.CanPush = false;
            orAddComponent.CanBePushed = CanBePushed;
            orAddComponent.PushSpeedModifier = 1f;
            orAddComponent.CanCarry = false;
            orAddComponent.CanBeCarried = CanBeCarried;
            orAddComponent.PreventPiercing = false;
            orAddComponent.SkipEmptyColliders = false;
            orAddComponent.RecheckTriggers = RecheckTriggers;
            orAddComponent.UpdateCollidersOnRotation = false;
            orAddComponent.UpdateCollidersOnScale = false;
            IntVector2 intVector = IntVector2.Zero;
            IntVector2 intVector2 = IntVector2.Zero;
            if (colliderGenerationMode != PixelCollider.PixelColliderGeneration.Tk2dPolygon)
            {
                if (dimensions != null)
                {
                    intVector2 = dimensions.Value;
                    if (!UsesPixelsAsUnitSize)
                    {
                        intVector2 = new IntVector2(intVector2.x * 16, intVector2.y * 16);
                    }
                }
                if (offset != null)
                {
                    intVector = offset.Value;
                    if (!UsesPixelsAsUnitSize)
                    {
                        intVector = new IntVector2(intVector.x * 16, intVector.y * 16);
                    }
                }
            }
            PixelCollider item = new PixelCollider
            {
                ColliderGenerationMode = colliderGenerationMode,
                CollisionLayer = collisionLayer,
                IsTrigger = IsTrigger,
                BagleUseFirstFrameOnly = (colliderGenerationMode == PixelCollider.PixelColliderGeneration.Tk2dPolygon),
                SpecifyBagelFrame = string.Empty,
                BagelColliderNumber = 0,
                ManualOffsetX = intVector.x,
                ManualOffsetY = intVector.y,
                ManualWidth = intVector2.x,
                ManualHeight = intVector2.y,
                ManualDiameter = (int)ManualDiameter,
                ManualLeftX = 0,
                ManualLeftY = 0,
                ManualRightX = 0,
                ManualRightY = 0
            };
            if (replaceExistingColliders | orAddComponent.PixelColliders == null)
            {
                orAddComponent.PixelColliders = new List<PixelCollider>
                {
                    item
                };
            }
            else
            {
                orAddComponent.PixelColliders.Add(item);
            }
            if (orAddComponent.sprite && colliderGenerationMode == PixelCollider.PixelColliderGeneration.Tk2dPolygon)
            {
                Bounds bounds = orAddComponent.sprite.GetBounds();
                orAddComponent.sprite.GetTrueCurrentSpriteDef().colliderVertices = new Vector3[]
                {
                    bounds.center - bounds.extents,
                    bounds.center + bounds.extents
                };
            }
            return orAddComponent;
        }
        public static void DisableSuperTinting(AIActor actor)
        {
            Material mat = actor.sprite.renderer.material;
            mat.mainTexture = actor.sprite.renderer.material.mainTexture;
            mat.EnableKeyword("BRIGHTNESS_CLAMP_ON");
            mat.DisableKeyword("BRIGHTNESS_CLAMP_OFF");
        }
        public static tk2dSpriteDefinition CopyDefinitionFrom(this tk2dSpriteDefinition other)
        {
            tk2dSpriteDefinition result = new tk2dSpriteDefinition
            {
                boundsDataCenter = new Vector3
                {
                    x = other.boundsDataCenter.x,
                    y = other.boundsDataCenter.y,
                    z = other.boundsDataCenter.z
                },
                boundsDataExtents = new Vector3
                {
                    x = other.boundsDataExtents.x,
                    y = other.boundsDataExtents.y,
                    z = other.boundsDataExtents.z
                },
                colliderConvex = other.colliderConvex,
                colliderSmoothSphereCollisions = other.colliderSmoothSphereCollisions,
                colliderType = other.colliderType,
                colliderVertices = other.colliderVertices,
                collisionLayer = other.collisionLayer,
                complexGeometry = other.complexGeometry,
                extractRegion = other.extractRegion,
                flipped = other.flipped,
                indices = other.indices,
                material = new Material(other.material),
                materialId = other.materialId,
                materialInst = new Material(other.materialInst),
                metadata = other.metadata,
                name = other.name,
                normals = other.normals,
                physicsEngine = other.physicsEngine,
                position0 = new Vector3
                {
                    x = other.position0.x,
                    y = other.position0.y,
                    z = other.position0.z
                },
                position1 = new Vector3
                {
                    x = other.position1.x,
                    y = other.position1.y,
                    z = other.position1.z
                },
                position2 = new Vector3
                {
                    x = other.position2.x,
                    y = other.position2.y,
                    z = other.position2.z
                },
                position3 = new Vector3
                {
                    x = other.position3.x,
                    y = other.position3.y,
                    z = other.position3.z
                },
                regionH = other.regionH,
                regionW = other.regionW,
                regionX = other.regionX,
                regionY = other.regionY,
                tangents = other.tangents,
                texelSize = new Vector2
                {
                    x = other.texelSize.x,
                    y = other.texelSize.y
                },
                untrimmedBoundsDataCenter = new Vector3
                {
                    x = other.untrimmedBoundsDataCenter.x,
                    y = other.untrimmedBoundsDataCenter.y,
                    z = other.untrimmedBoundsDataCenter.z
                },
                untrimmedBoundsDataExtents = new Vector3
                {
                    x = other.untrimmedBoundsDataExtents.x,
                    y = other.untrimmedBoundsDataExtents.y,
                    z = other.untrimmedBoundsDataExtents.z
                }
            };
            List<Vector2> uvs = new List<Vector2>();
            foreach (Vector2 vector in other.uvs)
            {
                uvs.Add(new Vector2
                {
                    x = vector.x,
                    y = vector.y
                });
            }
            result.uvs = uvs.ToArray();
            List<Vector3> colliderVertices = new List<Vector3>();
            foreach (Vector3 vector in other.colliderVertices)
            {
                colliderVertices.Add(new Vector3
                {
                    x = vector.x,
                    y = vector.y,
                    z = vector.z
                });
            }
            result.colliderVertices = colliderVertices.ToArray();
            return result;
        }

        public static void SetProjectileSpriteRight(this Projectile proj, string name, int pixelWidth, int pixelHeight, bool v, tk2dBaseSprite.Anchor middleCenter, int? overrideColliderPixelWidth = null, int? overrideColliderPixelHeight = null)
        {
            try
            {
                bool flag = overrideColliderPixelWidth == null;
                if (flag)
                {
                    overrideColliderPixelWidth = new int?(pixelWidth);
                }
                bool flag2 = overrideColliderPixelHeight == null;
                if (flag2)
                {
                    overrideColliderPixelHeight = new int?(pixelHeight);
                }
                float num = (float)pixelWidth / 16f;
                float num2 = (float)pixelHeight / 16f;
                float x = (float)overrideColliderPixelWidth.Value / 16f;
                float y = (float)overrideColliderPixelHeight.Value / 16f;
                proj.GetAnySprite().spriteId = ETGMod.Databases.Items.ProjectileCollection.inst.GetSpriteIdByName(name);
                tk2dSpriteDefinition tk2dSpriteDefinition = ETGMod.Databases.Items.ProjectileCollection.inst.spriteDefinitions[(PickupObjectDatabase.GetById(12) as Gun).DefaultModule.projectiles[0].GetAnySprite().spriteId].CopyDefinitionFrom();
                tk2dSpriteDefinition.boundsDataCenter = new Vector3(num / 2f, num2 / 2f, 0f);
                tk2dSpriteDefinition.boundsDataExtents = new Vector3(num, num2, 0f);
                tk2dSpriteDefinition.untrimmedBoundsDataCenter = new Vector3(num / 2f, num2 / 2f, 0f);
                tk2dSpriteDefinition.untrimmedBoundsDataExtents = new Vector3(num, num2, 0f);
                tk2dSpriteDefinition.position0 = new Vector3(0f, 0f, 0f);
                tk2dSpriteDefinition.position1 = new Vector3(0f + num, 0f, 0f);
                tk2dSpriteDefinition.position2 = new Vector3(0f, 0f + num2, 0f);
                tk2dSpriteDefinition.position3 = new Vector3(0f + num, 0f + num2, 0f);
                tk2dSpriteDefinition.colliderVertices[1].x = x;
                tk2dSpriteDefinition.colliderVertices[1].y = y;
                tk2dSpriteDefinition.name = name;
                tk2dSpriteDefinition.materialInst.mainTexture = proj.GetAnySprite().CurrentSprite.materialInst.mainTexture;
                tk2dSpriteDefinition.uvs = proj.GetAnySprite().CurrentSprite.uvs.ToArray();
                ETGMod.Databases.Items.ProjectileCollection.inst.spriteDefinitions[proj.GetAnySprite().spriteId] = tk2dSpriteDefinition;
                proj.baseData.force = 0f;
            }
            catch (Exception ex)
            {
                ETGModConsole.Log("Ooops! Seems like something got very, Very, VERY wrong. Here's the exception:", false);
                ETGModConsole.Log(ex.ToString(), false);
            }
        }
        public static bool PlayerHasActiveSynergy(this PlayerController player, string synergyNameToCheck)
        {
            foreach (int num in player.ActiveExtraSynergies)
            {
                AdvancedSynergyEntry advancedSynergyEntry = GameManager.Instance.SynergyManager.synergies[num];
                bool flag = advancedSynergyEntry.NameKey == synergyNameToCheck;
                if (flag)
                {
                    return true;
                }
            }
            return false;
        }

        public static bool OwnerHasSynergy(this Gun gun, string synergyName)
        {
            return gun.CurrentOwner is PlayerController && (gun.CurrentOwner as PlayerController).PlayerHasActiveSynergy(synergyName);
        }

        public static void AddItemToSynergy(this PickupObject obj, string nameKey)
        {
            AddItemToSynergy(nameKey, obj.PickupObjectId);
        }

        public static void AddItemToSynergy(string nameKey, int id)
        {
            foreach (AdvancedSynergyEntry entry in GameManager.Instance.SynergyManager.synergies)
            {
                if (entry.NameKey == nameKey)
                {
                    if (PickupObjectDatabase.GetById(id) != null)
                    {
                        PickupObject obj = PickupObjectDatabase.GetById(id);
                        if (obj is Gun)
                        {
                            if (entry.OptionalGunIDs != null)
                            {
                                entry.OptionalGunIDs.Add(id);
                            }
                            else
                            {
                                entry.OptionalGunIDs = new List<int> { id };
                            }
                        }
                        else
                        {
                            if (entry.OptionalItemIDs != null)
                            {
                                entry.OptionalItemIDs.Add(id);
                            }
                            else
                            {
                                entry.OptionalItemIDs = new List<int> { id };
                            }
                        }
                    }
                }
            }
        }
        public static void AddCurrentGunStatModifier(this Gun gun, PlayerStats.StatType statType, float amount, StatModifier.ModifyMethod modifyMethod)
        {
            gun.currentGunStatModifiers = gun.currentGunStatModifiers.Concat(new StatModifier[]
            {
                new StatModifier
                {
                    statToBoost = statType,
                    amount = amount,
                    modifyType = modifyMethod
                }
            }).ToArray<StatModifier>();
        }
    }
}