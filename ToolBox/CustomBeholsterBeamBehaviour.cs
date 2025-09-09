using System;
using System.Collections.Generic;
using UnityEngine;
using Dungeonator;
using FullInspector;
using GunRev;
using System.Collections;
using Alexandria.ItemAPI;
using System.Linq;

public class CustomBeholsterLaserBehavior : BasicAttackBehavior
{
	public override void Start()
	{
		base.Start();
		this.m_aiActor.healthHaver.OnDeath += (obj) =>
		{
			if (this.m_laserBeam != null)
			{
				this.m_laserBeam.DestroyBeam();
				this.m_laserBeam = null;
			}
		};
	}
	private bool ShowSpecificBeamShooter()
	{
		return this.beamSelection == ShootBeamBehavior.BeamSelection.Specify;
	}
	public Vector2 LaserFiringCenter
	{
		get
		{
			return this.m_aiActor.transform.position.XY();
		}
	}

	public bool FiringLaser
	{
		get
		{
			return this.IsfiringLaser;
		}
	}
	public bool LaserActive
	{
		get
		{
			return this.m_laserActive;
		}
	}
	public float LaserAngle
	{
		get
		{
			return this.AlaserAngle;
		}
	}
	public void SetLaserAngle(float alaserAngle)
	{
		this.AlaserAngle = alaserAngle;
		if (this.IsfiringLaser)
		{
			this.m_aiActor.aiAnimator.FacingDirection = alaserAngle;
		}
	}

	public BasicBeamController LaserBeam
	{
		get
		{
			return this.m_laserBeam;
		}
	}

	public bool m_laserActive;
	public bool IsfiringLaser;
	public float AlaserAngle;
	public BasicBeamController m_laserBeam;

	public VFXPool chargeUpVfx;
	public VFXPool chargeDownVfx;
	public ProjectileModule beamModule;
	public Transform beamTransform;

	public override void Upkeep()
	{
		base.Upkeep();
		if (this.m_aiActor.TargetRigidbody)
		{
			this.m_targetPosition = this.m_aiActor.TargetRigidbody.GetUnitCenter(ColliderType.HitBox);
			this.m_backupTarget = this.m_aiActor.TargetRigidbody;
		}
		else if (this.m_backupTarget)
		{
			this.m_targetPosition = this.m_backupTarget.GetUnitCenter(ColliderType.HitBox);
		}
	}

	public override BehaviorResult Update()
	{
		base.Update();
		BehaviorResult behaviorResult = base.Update();
		if (behaviorResult != BehaviorResult.Continue)
		{
			return behaviorResult;
		}
		if (!this.IsReady())
		{
			return BehaviorResult.Continue;
		}
		this.PrechargeFiringLaser();
		this.HasTriggeredScript = false;
		this.m_state = CustomBeholsterLaserBehavior.State.PreCharging;
		if (LockInPlaceWhileAttacking == true)
		{
			this.m_aiActor.SuppressTargetSwitch = true;
			this.m_aiActor.ClearPath();
		}
		this.m_updateEveryFrame = true;

		this.m_allBeamShooters = new List<AIBeamShooter2>(this.m_aiActor.GetComponents<AIBeamShooter2>());
		if (this.beamSelection == ShootBeamBehavior.BeamSelection.All)
		{
			List<AIBeamShooter2> beams = new List<AIBeamShooter2>() { };
			foreach (AIBeamShooter2 c in m_aiActor.gameObject.GetComponents(typeof(AIBeamShooter2)))
			{
				beams.Add(c);
				this.m_currentBeamShooters.Add(c);
			}
		}
		else if (this.beamSelection == ShootBeamBehavior.BeamSelection.Random)
		{
			this.m_currentBeamShooters.Add(BraveUtility.RandomElement<AIBeamShooter2>(this.m_allBeamShooters));
		}
		else if (this.beamSelection == ShootBeamBehavior.BeamSelection.Specify)
		{
			this.m_currentBeamShooters.Add(this.specificBeamShooter);
		}

		if (!string.IsNullOrEmpty(this.ChargeAnimation))
		{
			this.m_aiAnimator.PlayUntilCancelled(this.ChargeAnimation, true, null, -1f, false);

		}
		return BehaviorResult.RunContinuous;
	}

	public override ContinuousBehaviorResult ContinuousUpdate()
	{

		base.ContinuousUpdate();
		if (this.m_state == CustomBeholsterLaserBehavior.State.PreCharging)
		{
			if (!this.LaserActive)
			{
				this.ChargeFiringLaser(this.chargeTime);
				this.m_timer = this.chargeTime;
				this.m_state = CustomBeholsterLaserBehavior.State.Charging;
			}
		}
		else
		{
			if (this.m_state == CustomBeholsterLaserBehavior.State.Charging)
			{
				this.m_timer -= this.m_deltaTime;
				if (this.m_timer <= 0f)
				{
					this.m_state = CustomBeholsterLaserBehavior.State.Firing;
					this.StartFiringTheLaser();
					this.m_timer = this.firingTime;
				}
				return ContinuousBehaviorResult.Continue;
			}
			if (this.m_state == CustomBeholsterLaserBehavior.State.Firing)
			{
				if (this.HasTriggeredScript != true)
				{
					this.HasTriggeredScript = true;
					if (this.BulletScript != null && !this.BulletScript.IsNull)
					{
						if (!this.m_bulletSource)
						{
							this.m_bulletSource = this.ShootPoint.gameObject.GetOrAddComponent<BulletScriptSource>();
						}
						this.m_bulletSource.BulletManager = this.m_aiActor.bulletBank;
						this.m_bulletSource.BulletScript = this.BulletScript;
						this.m_bulletSource.Initialize();
					}
				}
				this.m_timer -= this.m_deltaTime;
				if (this.m_timer <= 0f || !this.FiringLaser)
				{
					if (this.m_bulletSource != null)
					{
						this.m_bulletSource.ForceStop();
					}
					return ContinuousBehaviorResult.Finished;
				}
				for (int i = 0; i < this.m_currentBeamShooters.Count; i++)
				{
					bool isFiringLaser = this.m_currentBeamShooters[0].IsFiringLaser;
					if (!isFiringLaser)
					{
						AIBeamShooter2 aibeamShooter = this.m_currentBeamShooters[i];
						Vector2 b = aibeamShooter.transform.position;
						bool flag11 = this.trackingType == CustomBeholsterLaserBehavior.TrackingType.Follow;
						float alaserAngle;
						if (flag11)
						{
							float num = Vector2.Distance(this.m_targetPosition, b);
							float num2 = (this.m_targetPosition - b).ToAngle();
							float num3 = BraveMathCollege.ClampAngle180(num2 - this.AlaserAngle);
							float f = num3 * num * 0.0174532924f;
							float num4 = this.maxTurnRate;
							float num5 = Mathf.Sign(num3);
							bool flag12 = this.m_unitOvershootTimer > 0f;
							if (flag12)
							{
								num5 = this.m_unitOvershootFixedDirection;
								this.m_unitOvershootTimer -= this.m_deltaTime;
								num4 = this.unitOvershootSpeed;
							}
							this.m_currentUnitTurnRate = Mathf.Clamp(this.m_currentUnitTurnRate + num5 * this.turnRateAcceleration * this.m_deltaTime, -num4, num4);
							float num6 = this.m_currentUnitTurnRate / num * 57.29578f;
							float num7 = 0f;
							bool flag13 = this.useDegreeCatchUp && Mathf.Abs(num3) > this.minDegreesForCatchUp;
							if (flag13)
							{
								float b2 = Mathf.InverseLerp(this.minDegreesForCatchUp, 180f, Mathf.Abs(num3)) * this.degreeCatchUpSpeed;
								num7 = Mathf.Max(num7, b2);
							}
							bool flag14 = this.useUnitCatchUp && Mathf.Abs(f) > this.minUnitForCatchUp;
							if (flag14)
							{
								float num8 = Mathf.InverseLerp(this.minUnitForCatchUp, this.maxUnitForCatchUp, Mathf.Abs(f)) * this.unitCatchUpSpeed;
								float b3 = num8 / num * 57.29578f;
								num7 = Mathf.Max(num7, b3);
							}
							bool flag15 = this.useUnitOvershoot && Mathf.Abs(f) < this.minUnitForOvershoot;
							if (flag15)
							{
								this.m_unitOvershootFixedDirection = (float)((this.m_currentUnitTurnRate <= 0f) ? -1 : 1);
								this.m_unitOvershootTimer = this.unitOvershootTime;
							}
							num7 *= Mathf.Sign(num3);
							alaserAngle = BraveMathCollege.ClampAngle360(this.AlaserAngle + (num6 + num7) * this.m_deltaTime);
						}
						else
						{
							alaserAngle = BraveMathCollege.ClampAngle360(this.AlaserAngle + this.maxTurnRate * this.m_deltaTime);
						}
						bool flag16 = this.IsfiringLaser == true;
						if (flag16)
						{
							this.AlaserAngle = alaserAngle;
						}
						return ContinuousBehaviorResult.Continue;
					}
				}
			}
		}
		return ContinuousBehaviorResult.Continue;
	}

	public override void EndContinuousUpdate()
	{
		base.EndContinuousUpdate();
		if (!string.IsNullOrEmpty(this.FireAnimation))
		{
			this.m_aiAnimator.EndAnimationIf(this.FireAnimation);
		}
		this.StopFiringLaser();
		this.m_aiAnimator.LockFacingDirection = false;
		this.m_aiActor.SuppressTargetSwitch = false;
		this.m_updateEveryFrame = false;
		this.UpdateCooldowns();
	}

	public override bool IsOverridable()
	{
		return false;
	}

	public void PrechargeFiringLaser()
	{
		this.m_aiActor.aiAnimator.LockFacingDirection = true;
		if (EnemyChargeSound != null) { AkSoundEngine.PostEvent(EnemyChargeSound, base.m_aiActor.gameObject); }
		else if (UsesBaseSounds == true) { AkSoundEngine.PostEvent("Play_ENM_beholster_charging_01", base.m_aiActor.gameObject); }
	}

	public void ChargeFiringLaser(float time)
	{
		if (BeamChargingSound != null) { AkSoundEngine.PostEvent(BeamChargingSound, base.m_aiActor.gameObject); }
		else if (UsesBaseSounds == true) { AkSoundEngine.PostEvent("Play_ENM_beholster_charging_01", base.m_aiActor.gameObject); }
		this.m_laserActive = true;
	}

	public void StartFiringTheLaser()
	{
		float facingDirection = this.m_aiActor.aiAnimator.FacingDirection;
		this.IsfiringLaser = true;
		this.SetLaserAngle(facingDirection);
		this.m_aiActor.aiAnimator.LockFacingDirection = true;

		AkSoundEngine.PostEvent("Play_ENM_deathray_shot_01", base.m_aiActor.gameObject);
		this.IsfiringLaser = true;
		base.m_aiActor.aiAnimator.LockFacingDirection = true;
		MonoBehaviour yes = this.m_aiActor.GetComponent<MonoBehaviour>();
		for (int i = 0; i < this.m_currentBeamShooters.Count; i++)
		{
			AIBeamShooter2 aibeamShooter2 = this.m_currentBeamShooters[i];

			yes.StartCoroutine(this.FireBeam(aibeamShooter2));
		}

	}


	public void StopFiringLaser()
	{
		if (!this.IsfiringLaser)
		{
			return;
		}
		if (!string.IsNullOrEmpty(this.PostFireAnimation))
		{
			this.m_aiAnimator.PlayUntilFinished(this.PostFireAnimation, true, null, -1f, false);

		}
		if (StopLaserFiringSound != null) { AkSoundEngine.PostEvent(StopLaserFiringSound, base.m_aiActor.gameObject); }
		else if (UsesBaseSounds == true) { AkSoundEngine.PostEvent("Stop_ENM_deathray_loop_01", base.m_aiActor.gameObject); }
		this.m_laserActive = false;
		this.IsfiringLaser = false;
		this.m_aiActor.aiAnimator.LockFacingDirection = false;
		this.m_currentBeamShooters.Clear();
	}

	protected IEnumerator FireBeam(AIBeamShooter2 aibeamShooter2)
	{
		GameObject beamObject = UnityEngine.Object.Instantiate<GameObject>(aibeamShooter2.beamModule.GetCurrentProjectile().gameObject);
		if (UsesBeamProjectileWithoutModule)
		{
			beamObject = UnityEngine.Object.Instantiate<GameObject>(aibeamShooter2.beamProjectile.gameObject);
		}
		BasicBeamController beamCont = beamObject.GetComponent<BasicBeamController>();

		List<AIActor> activeEnemies = base.m_aiActor.ParentRoom.GetActiveEnemies(RoomHandler.ActiveEnemyType.All);
		for (int i = 0; i < activeEnemies.Count; i++)
		{
			AIActor aiactor = activeEnemies[i];
			if (aiactor && aiactor != base.m_aiActor && aiactor.healthHaver)
			{
				beamCont.IgnoreRigidbodes.Add(aiactor.specRigidbody);
			}
		}
		beamCont.Owner = base.m_aiActor;
		beamCont.HitsPlayers = true;
		beamCont.HitsEnemies = true;

		beamCont.HeightOffset = 0f;
		beamCont.RampHeightOffset = 0;

		beamCont.ContinueBeamArtToWall = true;
		float enemyTickCooldown = 0f;
		beamCont.OverrideHitChecks = delegate (SpeculativeRigidbody hitRigidbody, Vector2 dirVec)
		{
			Projectile currentProjectile = aibeamShooter2.beamModule.GetCurrentProjectile();
			if (UsesBeamProjectileWithoutModule)
			{
				currentProjectile = aibeamShooter2.beamProjectile;
			}

			HealthHaver healthHaver = (!hitRigidbody) ? null : hitRigidbody.healthHaver;
			if (hitRigidbody && hitRigidbody.projectile && hitRigidbody.GetComponent<BeholsterBounceRocket>())
			{
				BounceProjModifier component = hitRigidbody.GetComponent<BounceProjModifier>();
				if (component)
				{
					component.numberOfBounces = 0;
				}
				hitRigidbody.projectile.DieInAir(false, true, true, false);
			}
			if (healthHaver != null)
			{
				if (healthHaver.aiActor)
				{
					if (enemyTickCooldown <= 0f)
					{
						healthHaver.ApplyDamage(ProjectileData.FixedFallbackDamageToEnemies, dirVec, this.m_aiActor.GetActorName(), currentProjectile.damageTypes, DamageCategory.Normal, false, null, false);
						enemyTickCooldown = 0.1f;
					}
				}
				else
				{
					healthHaver.ApplyDamage(currentProjectile.baseData.damage, dirVec, this.m_aiActor.GetActorName(), currentProjectile.damageTypes, DamageCategory.Normal, false, null, false);
				}
			}
			if (hitRigidbody.majorBreakable)
			{
				hitRigidbody.majorBreakable.ApplyDamage(26f * BraveTime.DeltaTime, dirVec, false, false, false);
			}
		};
		bool firstFrame = true;
		while (beamCont != null && this.IsfiringLaser)
		{
			enemyTickCooldown = Mathf.Max(enemyTickCooldown - BraveTime.DeltaTime, 0f);
			bool flag4 = this.UsesCustomAngle == true && this.FiresDirectlyTowardsPlayer == true;
			float clampedAngle;
			if (flag4)
			{
				clampedAngle = BraveMathCollege.ClampAngle360((aibeamShooter2.northAngleTolerance + this.m_aiActor.aiAnimator.FacingDirection) + AlaserAngle);
			}
			else
			{
				bool usesCustomAngle = this.UsesCustomAngle;
				if (usesCustomAngle == true)
				{
					clampedAngle = BraveMathCollege.ClampAngle360((aibeamShooter2.northAngleTolerance) + AlaserAngle);
				}
				else
				{
					bool firesDirectlyTowardsPlayer = this.FiresDirectlyTowardsPlayer;
					if (firesDirectlyTowardsPlayer)
					{
						clampedAngle = BraveMathCollege.ClampAngle360((this.m_aiActor.aiAnimator.FacingDirection) + AlaserAngle);
					}
					else
					{
						clampedAngle = BraveMathCollege.ClampAngle360(this.LaserAngle);
					}
				}
			}
			Vector3 dirVec2 = new Vector3(Mathf.Cos(clampedAngle * 0.0174532924f), Mathf.Sin(clampedAngle * 0.0174532924f), 0f) * 10f;
			Vector2 startingPoint = aibeamShooter2.beamTransform.position;
			//aibeamShooter2.gameObject.layer = 30;

			//aibeamShooter2.sprite.allowDefaultLayer = false;

			//aibeamShooter2.gameObject.GetComponent<tk2dBaseSprite>().SortingOrder = 10;
			aibeamShooter2.sprite.HeightOffGround = -20;
			//aibeamShooter2.sprite.allowDefaultLayer

			//ETGModConsole.Log("Enemy:" + m_aiActor.transform.position.x.ToString() + "," + m_aiActor.transform.position.y.ToString());
			//ETGModConsole.Log("Beam:" + aibeamShooter2.transform.position.x.ToString() + "," + aibeamShooter2.transform.position.y.ToString());
			//float tanAngle = Mathf.Tan(clampedAngle * 0.0174532924f);
			//float sign = (float)((clampedAngle <= 90f || clampedAngle >= 270f) ? 1 : -1);
			//float denominator = Mathf.Sqrt(this.firingEllipseB * this.firingEllipseB + this.firingEllipseA * this.firingEllipseA * (tanAngle * tanAngle));
			//startingPoint.x += sign * this.firingEllipseA * this.firingEllipseB / denominator;
			//startingPoint.y += sign * this.firingEllipseA * this.firingEllipseB * tanAngle / denominator;
			beamCont.Origin = startingPoint;// + this.m_aiActor.sprite.WorldCenter;
			beamCont.Direction = dirVec2;
			bool flag5 = firstFrame;
			if (flag5)
			{
				yield return null;
				firstFrame = false;
			}
			else
			{
				beamCont.LateUpdatePosition(startingPoint);
				yield return null;
				if (this.IsfiringLaser && !beamCont)
				{
					this.StopFiringLaser();
					break;
				}
				while (Time.timeScale == 0f)
				{
					yield return null;
				}
			}
		}
		if (!this.IsfiringLaser && beamCont != null)
		{
			beamCont.DestroyBeam();
			beamCont = null;
		}
		yield break;
	}




	public CustomBeholsterLaserBehavior.TrackingType trackingType;

	public float initialAimOffset;

	public float chargeTime;

	public float firingTime;

	public float maxTurnRate;

	public float turnRateAcceleration;

	public bool useDegreeCatchUp;

	public float minDegreesForCatchUp;
	public float degreeCatchUpSpeed;

	public bool useUnitCatchUp;
	public float minUnitForCatchUp;

	public float maxUnitForCatchUp;
	public float unitCatchUpSpeed;

	public bool useUnitOvershoot;
	public float minUnitForOvershoot;

	public float unitOvershootTime;

	public float unitOvershootSpeed;

	private CustomBeholsterLaserBehavior.State m_state;

	private float m_timer;

	private Vector2 m_targetPosition;

	private float m_currentUnitTurnRate;

	private float m_unitOvershootFixedDirection;

	private float m_unitOvershootTimer;

	private SpeculativeRigidbody m_backupTarget;

	private BulletScriptSource m_bulletSource;
	public bool HasTriggeredScript;

	public BulletScriptSelector BulletScript;
	public Transform ShootPoint;

	private enum State
	{
		PreCharging,
		Charging,
		Firing
	}

	public enum TrackingType
	{
		Follow,
		ConstantTurn
	}
	//=====
	public bool FiresDirectlyTowardsPlayer;
	public bool UsesCustomAngle;
	public float CustomAngleValue;
	//=====
	public string ChargeAnimation;
	public string FireAnimation;
	public string PostFireAnimation;

	public ShootBeamBehavior.BeamSelection beamSelection;
	public AIBeamShooter2 specificBeamShooter;
	private List<AIBeamShooter2> m_allBeamShooters;
	private readonly List<AIBeamShooter2> m_currentBeamShooters = new List<AIBeamShooter2>();

	public string LaserFiringSound;
	public string StopLaserFiringSound;
	public string EnemyChargeSound;
	public string BeamChargingSound;
	public bool UsesBaseSounds;

	public bool LockInPlaceWhileAttacking;

	public bool UsesBeamProjectileWithoutModule;
}