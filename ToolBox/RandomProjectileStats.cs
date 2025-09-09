using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace GunRev
{
    public class RandomProjectileStatsComponent : MonoBehaviour
    {
        public RandomProjectileStatsComponent()
        {
            this.randomDamage = false;
            this.highDMGPercent = 200;
            this.lowDMGPercent = 10;

            this.randomSpeed = false;
            this.highSpeedPercent = 200;
            this.lowSpeedPercent = 10;

            this.randomKnockback = false;
            this.highKnockbackPercent = 200;
            this.lowKnockbackPercent = 10;

            this.randomRange = false;
            this.highRangePercent = 200;
            this.lowRangePercent = 10;

            this.randomScale = false;
            this.highScalePercent = 200;
            this.lowScalePercent = 10;

            this.randomJammedDMG = false;
            this.highJammedDMGPercent = 200;
            this.lowJammedDMGPercent = 10;

            this.randomBossDMG = false;
            this.highBossDMGPercent = 200;
            this.lowBossDMGPercent = 10;

            this.scaleBasedOnDamage = false;
        }
        private void Start()
        {
            this.m_projectile = base.GetComponent<Projectile>();

            if (randomSpeed) this.m_projectile.baseData.speed *= (UnityEngine.Random.Range(lowSpeedPercent, highSpeedPercent + 1)) / 100f;
            if (randomKnockback) this.m_projectile.baseData.force *= (UnityEngine.Random.Range(lowKnockbackPercent, highKnockbackPercent + 1)) / 100f;
            if (randomRange) this.m_projectile.baseData.range *= (UnityEngine.Random.Range(lowRangePercent, highRangePercent + 1)) / 100f;
            if (randomBossDMG) this.m_projectile.BossDamageMultiplier *= (UnityEngine.Random.Range(lowBossDMGPercent, highBossDMGPercent + 1)) / 100f;
            if (randomJammedDMG) this.m_projectile.BlackPhantomDamageMultiplier *= (UnityEngine.Random.Range(lowJammedDMGPercent, highJammedDMGPercent + 1)) / 100f;
            float damageMult = (UnityEngine.Random.Range(lowDMGPercent, highDMGPercent + 1)) / 100f;
            if (randomDamage) this.m_projectile.baseData.damage *= damageMult;
            if (randomScale) this.m_projectile.RuntimeUpdateScale((UnityEngine.Random.Range(lowScalePercent, highScalePercent + 1)) / 100f);
            if (scaleBasedOnDamage) this.m_projectile.RuntimeUpdateScale(damageMult);
            this.m_projectile.UpdateSpeed();
        }
        private Projectile m_projectile;

        public bool randomDamage;
        public int highDMGPercent;
        public int lowDMGPercent;

        public bool randomSpeed;
        public int highSpeedPercent;
        public int lowSpeedPercent;

        public bool randomKnockback;
        public int highKnockbackPercent;
        public int lowKnockbackPercent;

        public bool randomRange;
        public int highRangePercent;
        public int lowRangePercent;

        public bool randomScale;
        public int highScalePercent;
        public int lowScalePercent;

        public bool randomBossDMG;
        public int highBossDMGPercent;
        public int lowBossDMGPercent;

        public bool randomJammedDMG;
        public int highJammedDMGPercent;
        public int lowJammedDMGPercent;

        public bool scaleBasedOnDamage;
    }
}
