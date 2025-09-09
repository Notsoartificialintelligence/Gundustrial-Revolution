using Dungeonator;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace GunRev
{
    public class LightningProjectileComp : MonoBehaviour
    {
        public static GameObject RenderLaserSight(Vector2 position, float length, float angle, bool alterColour = false, Color? colour = null)
        {
            GameObject gameObject = SpawnManager.SpawnVFX(OtherTools.laserSightPrefab, position, Quaternion.Euler(0, 0, angle));

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
        public LightningProjectileComp()
        {
            initialAngle = float.NegativeInfinity;
        }
        private void Start()
        {
            self = base.GetComponent<Projectile>();
            self.OnHitEnemy += OnHitEnemy;
            laserVFX = new List<GameObject>();
            owner = self.ProjectilePlayerOwner();
            StartCoroutine(moveLightning());
        }
        private void Update()
        {
            if (self)
            {
                if (self.GetElapsedDistance() > tilesSinceLastCheck)
                {
                    tilesSinceLastCollision += (self.GetElapsedDistance() - tilesSinceLastCheck);

                    tilesSinceLastCheck = self.GetElapsedDistance();
                }
            }
            if (tilesSinceLastCollision > 3 && midEnemyZapping) midEnemyZapping = false;
        }
        private void OnHitEnemy(Projectile self, SpeculativeRigidbody enemy, bool fatal)
        {
            if (enemy && enemy.aiActor)
            {
                if (enemy.aiActor != lastHitEnemy)
                {
                    if (lastHitEnemy != null) secondToLastHitEnemy = lastHitEnemy;
                    lastHitEnemy = enemy.aiActor;
                }
                tilesSinceLastCollision = 0;

                //Check if there's another valid enemy around to arc to
                if (Vector2.Distance(enemy.UnitCenter, enemy.UnitCenter.GetPositionOfNearestEnemy(true, true, new List<AIActor>() { lastHitEnemy, secondToLastHitEnemy })) < 3)
                {
                    PierceProjModifier piercing = self.gameObject.GetComponent<PierceProjModifier>();
                    if (piercing != null)
                    {
                        piercing.penetration++;
                    }
                    else
                    {
                        self.gameObject.AddComponent<PierceProjModifier>();
                    }

                    midEnemyZapping = true;
                    float newArc = self.specRigidbody.UnitCenter.GetVectorToNearestEnemy(0, 0, self.ProjectilePlayerOwner(), new List<AIActor>() { lastHitEnemy, secondToLastHitEnemy }).ToAngle();
                    self.SendInDirection(newArc.DegreeToVector2(), true, true); //Send the projectile in the new direction

                    TriggerLightningBreak();

                }

            }
        }
        private IEnumerator moveLightning()
        {
            while (self)
            {
                yield return new WaitForSeconds(LightningTime);
                if (!midEnemyZapping)
                {

                    //Determine the new direction of the projectile
                    if (initialAngle == float.NegativeInfinity) initialAngle = self.Direction.ToAngle(); //If initial angle is not set to the placeholder, set it
                    float newArc = ProjSpawnHelper.GetAccuracyAngled(initialAngle, 80, owner); //Determine accuracy


                    if (Vector2.Distance(self.specRigidbody.UnitCenter, self.specRigidbody.UnitCenter.GetPositionOfNearestEnemy(true, true, new List<AIActor>() { lastHitEnemy, secondToLastHitEnemy })) < 3)
                    {
                        newArc = self.specRigidbody.UnitCenter.GetVectorToNearestEnemy(0, 5, self.ProjectilePlayerOwner(), new List<AIActor>() { lastHitEnemy, secondToLastHitEnemy }).ToAngle();
                    }

                    self.SendInDirection(newArc.DegreeToVector2(), true, true); //Send the projectile in the new direction


                    TriggerLightningBreak();
                }

            }
            yield break;
        }
        private void TriggerLightningBreak()
        {
            //Erase the TiledSpriteConnector from the last laser created, if the last laser exists.
            if (lastLaser != null)
            {
                if (lastLaser.GetComponent<TiledSpriteConnector>() != null) UnityEngine.Object.Destroy(lastLaser.GetComponent<TiledSpriteConnector>());
                lastLaser = null;
            }

            //Create New Laser Sight
            GameObject laserSight = RenderLaserSight(self.specRigidbody.UnitCenter, 1, self.Direction.ToAngle(), true, ExtendedColours.skyblue);

            TiledSpriteConnector connector = laserSight.AddComponent<TiledSpriteConnector>();
            connector.eraseSpriteIfTargetOrSourceNull = false;
            connector.sourceRigidbody = self.specRigidbody;
            connector.eraseComponentIfTargetOrSourceNull = true;
            connector.targetVector = self.specRigidbody.UnitCenter;
            connector.usesVector = true;

            lastLaser = laserSight;
            laserVFX.Add(laserSight);
        }
        private float LightningTime
        {
            get
            {
                //ETGModConsole.Log(BraveTime.DeltaTime.ToString());
                return (0.005f / Time.timeScale);
            }
        }
        private void OnDestroy()
        {
            if (laserVFX.Count > 0) ETGMod.StartGlobalCoroutine(deleteLasers(laserVFX, LightningTime, logDebug));
        }
        private static IEnumerator deleteLasers(List<GameObject> lasers, float delay, bool logDebug)
        {
            if (logDebug) ETGModConsole.Log("Running laser deletion code");
            yield return new WaitForSeconds(delay);

            List<GameObject> reversedList = new List<GameObject>();
            for (int i = lasers.Count - 1; i >= 0; i--)
            {
                if (logDebug) ETGModConsole.Log($"Checking laser at index ({i}) in laser List.");
                if (lasers[i] != null)
                {
                    reversedList.Add(lasers[i]);
                    if (logDebug) ETGModConsole.Log($"Laser at index ({i}) was valid, adding to reversedList at index ({reversedList.Count - 1}).");
                }
            }
            for (int i = reversedList.Count - 1; i >= 0; i--)
            {
                if (logDebug) ETGModConsole.Log($"Checking laser at index ({i}) in reversedList.");
                if (reversedList[i] != null)
                {
                    if (logDebug) ETGModConsole.Log($"Laser at index ({i}) in reversedList exists and will be destroyed.");

                    UnityEngine.Object.Destroy(reversedList[i]);
                }
                else { if (logDebug) ETGModConsole.Log($"Laser at index ({i}) was NULL in reversedList."); }
                yield return new WaitForSeconds(delay);

            }
            yield break;
        }
        //Public
        public bool logDebug;
        //Private
        private float initialAngle;
        private GameObject lastLaser;
        private List<GameObject> laserVFX;
        private Projectile self;
        private PlayerController owner;

        private AIActor lastHitEnemy;
        private AIActor secondToLastHitEnemy;
        private bool midEnemyZapping;
        private float tilesSinceLastCollision;
        private float tilesSinceLastCheck;
    }
    public class TiledSpriteConnector : MonoBehaviour
    {
        private void Start()
        {
            tiledSprite = base.GetComponent<tk2dTiledSprite>();
        }
        private void Update()
        {
            if (sourceRigidbody)
            {
                Vector2 unitCenter = sourceRigidbody.UnitCenter;
                Vector2 unitCenter2 = Vector2.zero;
                if (usesVector && targetVector != Vector2.zero) unitCenter2 = targetVector;
                else if (targetRigidbody) unitCenter2 = targetRigidbody.UnitCenter;
                if (unitCenter2 != Vector2.zero)
                {
                    tiledSprite.transform.position = unitCenter;
                    Vector2 vector = unitCenter2 - unitCenter;
                    float num = BraveMathCollege.Atan2Degrees(vector.normalized);
                    int num2 = Mathf.RoundToInt(vector.magnitude / 0.0625f);
                    tiledSprite.dimensions = new Vector2((float)num2, tiledSprite.dimensions.y);
                    tiledSprite.transform.rotation = Quaternion.Euler(0f, 0f, num);
                    tiledSprite.UpdateZDepth();

                }
                else
                {
                    if (eraseSpriteIfTargetOrSourceNull) UnityEngine.Object.Destroy(tiledSprite.gameObject);
                    else if (eraseComponentIfTargetOrSourceNull) UnityEngine.Object.Destroy(this);
                }
            }
            else
            {
                if (eraseSpriteIfTargetOrSourceNull) UnityEngine.Object.Destroy(tiledSprite.gameObject);
                else if (eraseComponentIfTargetOrSourceNull) UnityEngine.Object.Destroy(this);
            }
        }

        public SpeculativeRigidbody sourceRigidbody;
        public SpeculativeRigidbody targetRigidbody;
        public Vector2 targetVector;
        public bool usesVector;
        public bool eraseSpriteIfTargetOrSourceNull;
        public bool eraseComponentIfTargetOrSourceNull;
        private tk2dTiledSprite tiledSprite;
    }
}
