using System;
using Alexandria.ItemAPI;
using UnityEngine;
using Dungeonator;
using System.Collections;


//Thanks to Skilotar_ and Some Bunny for helping with this effect
namespace GunRev
{
    public class SlowingBulletsEffect : MonoBehaviour
    {

        public SlowingBulletsEffect()
        {

        }
        private void Start()
        {
            try
            {
                this.m_projectile = base.GetComponent<Projectile>();

                this.m_projectile.UpdateSpeed();
                this.speedMultiplierPerFrame = UnityEngine.Random.Range(94f, 96f) / 100f;
                this.SizeMultPerFrame = UnityEngine.Random.Range(0.95f, 1f);

                this.m_projectile.RuntimeUpdateScale(UnityEngine.Random.Range(1f, 1.5f));
                shouldSpeedModify = true;

            }
            catch (Exception e)
            {
                ETGModConsole.Log(e.Message);
                ETGModConsole.Log(e.StackTrace);
            }
        }
        private Projectile m_projectile;
        private float speedMultiplierPerFrame;
        private bool shouldSpeedModify;
        public bool HasSynergyHunterSpores;
        public float SizeMultPerFrame;

        private void FixedUpdate()
        {
            if (shouldSpeedModify)
            {
                if (m_projectile.baseData.speed > 0.01f)
                {
                    m_projectile.baseData.speed *= speedMultiplierPerFrame;
                    m_projectile.RuntimeUpdateScale(SizeMultPerFrame);
                    m_projectile.UpdateSpeed();

                }
                else
                {
                    StartCoroutine(FloatHandler());
                    shouldSpeedModify = false;
                }
            }
        }
        private IEnumerator FloatHandler()
        {
            yield return new WaitForSeconds(3);
            m_projectile.DieInAir(true);
        }

    }
}