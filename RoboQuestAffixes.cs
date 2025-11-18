using System;
using System.Collections;
using Gungeon;
using UnityEngine;
using System.Collections.Generic;
using Alexandria.ItemAPI;
using Alexandria.Misc;
using System.Linq;

namespace GunRev
{
    public class GunAffixBehaviour : GunBehaviour
    {
        // thanks spapi :D
        public static T CreateScriptable<T>(Action<T> configure = null) where T : ScriptableObject
        {
            var s = ScriptableObject.CreateInstance<T>();
            configure?.Invoke(s);

            return s;
        }
        public class Affix : ScriptableObject
        {
            /// <summary>
            /// The internal name of this Affix.
            /// </summary>
            public string Name;
            /// <summary>
            /// The text that appears when a gun with this Affix is picked up.
            /// </summary>
            public string Text;
            /// <summary>
            /// The rarity of this Affix. Affects font colour and roll chance.
            /// </summary>
            public string Rarity;
            /// <summary>
            /// The font colour of the text displayed from this Affix.
            /// </summary>
            public Color FontColor;
            /// <summary>
            /// An array of stat modifiers this Affix will apply.
            /// </summary>
            public List<StatModifier> StatModifiers = null;
            /// <summary>
            /// An array of bullet modifiers this Affix will apply.
            /// </summary>
            public List<Component> BulletModifiers = null;
            /// <summary>
            /// The on-reload effect for this Affix.
            /// </summary>
            public Action<PlayerController, Gun> OnReload;
        }
        public static Color ConvertRarityToColour(string rarity)
        {
            Color FontColor = rarity switch
            {
                "inherent" => new Color32(161, 161, 161, 255),
                "uncommon" => new Color32(143, 248, 67, 255),
                "superior" => new Color32(55, 81, 230, 255),
                "epic" => new Color32(165, 64, 230, 255),
                "fantastic" => new Color32(230, 174, 22, 255),
                _ => new Color32(161, 161, 161, 255)
            };
            return FontColor;
        }
        public override void Update()
        {
            player = PlayerOwner;
            if (player?.CurrentGun == gun && !StatsApplied)
            {
                StatsApplied = true;
                foreach (Affix aff in CurrentAffixes)
                {
                    if (aff.StatModifiers != null)
                    {
                        foreach (StatModifier modifier in aff.StatModifiers)
                        {
                            player.ownerlessStatModifiers.Add(modifier);
                        }
                    }
                }
            }
            else if (player?.CurrentGun != gun && StatsApplied)
            {
                StatsApplied = false;
                foreach (Affix aff in CurrentAffixes)
                {
                    if (aff.StatModifiers != null)
                    {
                        foreach (StatModifier modifier in aff.StatModifiers)
                        {
                            player.ownerlessStatModifiers.Remove(modifier);
                        }
                    }
                }
            }
            base.Update();
        }
        public override void OnReloadedPlayer(PlayerController owner, Gun gun)
        {
            if (gun.ClipShotsRemaining == 0)
            {
                foreach (Affix aff in CurrentAffixes)
                {
                    aff.OnReload?.Invoke(owner, gun);
                }
            }
            base.OnReloadedPlayer(owner, gun);
        }
        public override void OnPlayerPickup(PlayerController playerOwner)
        {
            if (!EverPickedUp)
            {
                CurrentAffixes?.Clear();
                UncommonAffixes?.Clear();
                SuperiorAffixes?.Clear();
                EpicAffixes?.Clear();
                FantasticAffixes?.Clear();
                foreach (string name in GuaranteedAffixes)
                {
                    foreach (Affix aff in Affixes)
                    {
                        if (aff.Name == name)
                        {
                            CurrentAffixes.Add(aff);
                        }
                    }
                }
                foreach (Affix aff in Affixes)
                {
                    if (CurrentAffixes.Contains(aff)) continue;

                    switch (aff.Rarity)
                    {
                        case "uncommon":
                            UncommonAffixes.Add(aff);
                            break;
                        case "superior":
                            SuperiorAffixes.Add(aff);
                            break;
                        case "epic":
                            EpicAffixes.Add(aff);
                            break;
                        case "fantastic":
                            FantasticAffixes.Add(aff);
                            break;
                    }
                }
                int coolness = (int)Math.Round(playerOwner.stats.GetStatValue(PlayerStats.StatType.Coolness));
                int numRolls = 6 + coolness;
                for (int i = 0; i < numRolls; i++)
                {
                    int roll = UnityEngine.Random.Range(0, 100);
                    if (roll <= 5)
                    {
                        if (FantasticAffixes.Count > 0)
                        {
                            Affix aff = FantasticAffixes[UnityEngine.Random.Range(0, FantasticAffixes.Count - 1)];
                            CurrentAffixes.Add(aff);
                            FantasticAffixes.Remove(aff);
                        }
                    }
                    else if (roll >= 6 && roll <= 15)
                    {
                        if (EpicAffixes.Count > 0)
                        {
                            Affix aff = EpicAffixes[UnityEngine.Random.Range(0, EpicAffixes.Count - 1)];
                            CurrentAffixes.Add(aff);
                            EpicAffixes.Remove(aff);
                        }
                    }
                    else if (roll >= 16 && roll <= 30)
                    {
                        if (SuperiorAffixes.Count > 0)
                        {
                            Affix aff = SuperiorAffixes[UnityEngine.Random.Range(0, SuperiorAffixes.Count - 1)];
                            CurrentAffixes.Add(aff);
                            SuperiorAffixes.Remove(aff);
                        }
                    }
                    else if (roll >= 31 && roll <= 75)
                    {
                        if (UncommonAffixes.Count > 0)
                        {
                            Affix aff = UncommonAffixes[UnityEngine.Random.Range(0, UncommonAffixes.Count - 1)];
                            CurrentAffixes.Add(aff);
                            UncommonAffixes.Remove(aff);
                        }
                    }
                };
            }
            gun.PostProcessProjectile += PostProcessProj;
            StartCoroutine(ShowAffixes(playerOwner));
            base.OnPlayerPickup(playerOwner);
        }
        public IEnumerator ShowAffixes(PlayerController pl)
        {
            foreach (Affix aff in CurrentAffixes.OrderBy(x => x.Rarity switch
            {
                "inherent" => 0,
                "uncommon" => 1,
                "superior" => 2,
                "epic" => 3,
                "fantastic" => 4,

                _ => 0
            }))
            {
                OtherTools.DoRisingStringFade(aff.Text, pl.CenterPosition, aff.FontColor);
                yield return new WaitForSeconds(1f);
            }
            yield break;
        }
        public void PostProcessProj(Projectile proj)
        {
            foreach (Affix aff in CurrentAffixes)
            {
                if (aff.BulletModifiers != null)
                {
                    foreach (Component mod in aff.BulletModifiers)
                    {
                        Type realType = mod.GetType();
                        Component addedComponent = proj.gameObject.AddComponent(realType);
                        addedComponent.CopyFrom(mod);
                    }
                }
            }
        }
        public static List<Affix> Affixes =
        [
            // INHERENT AFFIXES

            // UNCOMMON AFFIXES

            CreateScriptable<Affix>(a => {a.Name = "cadence";
                a.Text = "Cadence - +10% firerate.";
                a.Rarity = "uncommon";
                a.FontColor = ConvertRarityToColour(a.Rarity);
                a.StatModifiers = new List<StatModifier>() {new StatModifier {statToBoost = PlayerStats.StatType.RateOfFire, amount = 1.1f, modifyType = StatModifier.ModifyMethod.MULTIPLICATIVE} }; }),

            CreateScriptable<Affix>(a => {a.Name = "quickload";
                a.Text = "Quickload - +15% reload speed.";
                a.Rarity = "uncommon";
                a.FontColor = ConvertRarityToColour(a.Rarity);
                a.StatModifiers = new List<StatModifier>() {new StatModifier {statToBoost = PlayerStats.StatType.ReloadSpeed, amount = 0.85f, modifyType = StatModifier.ModifyMethod.MULTIPLICATIVE} }; }),

            CreateScriptable<Affix>(a => {a.Name = "neat";
                a.Text = "Neat - +20% accuracy and +20% range.";
                a.Rarity = "uncommon";
                a.FontColor = ConvertRarityToColour(a.Rarity);
                a.StatModifiers = new List<StatModifier>() {new StatModifier {statToBoost = PlayerStats.StatType.Accuracy, amount = 0.8f, modifyType = StatModifier.ModifyMethod.MULTIPLICATIVE },
                new StatModifier {statToBoost = PlayerStats.StatType.RangeMultiplier, amount = 0.2f, modifyType = StatModifier.ModifyMethod.ADDITIVE} }; }),

            CreateScriptable<Affix>(a => {a.Name = "knock";
                a.Text = "Knock - +15% projectile knockback.";
                a.Rarity = "uncommon";
                a.FontColor = ConvertRarityToColour(a.Rarity);
                a.StatModifiers = new List<StatModifier>() {new StatModifier {statToBoost = PlayerStats.StatType.KnockbackMultiplier, amount = 0.15f, modifyType = StatModifier.ModifyMethod.ADDITIVE} }; }),

            CreateScriptable<Affix>(a => {a.Name = "biggame";
                a.Text = "Big-Game - +15% damage against bosses.";
                a.Rarity = "uncommon";
                a.FontColor = ConvertRarityToColour(a.Rarity);
                a.StatModifiers = new List<StatModifier>() {new StatModifier {statToBoost = PlayerStats.StatType.DamageToBosses, amount = 1.15f, modifyType = StatModifier.ModifyMethod.MULTIPLICATIVE} }; }),

            // SUPERIOR AFFIXES

            CreateScriptable<Affix>(a => {a.Name = "bounce";
                a.Text = "Bounce - Shots bounce once.";
                a.Rarity = "superior";
                a.FontColor = ConvertRarityToColour(a.Rarity);
                a.BulletModifiers = new List<Component>() {new BounceProjModifier {numberOfBounces = 1} }; }),

            CreateScriptable<Affix>(a => {a.Name = "pierce";
                a.Text = "Pierce - Shots pierce enemies once.";
                a.Rarity = "superior";
                a.FontColor = ConvertRarityToColour(a.Rarity);
                a.BulletModifiers = new List<Component>() {new PierceProjModifier {penetration = 1} }; }),

            // EPIC AFFIXES

            CreateScriptable<Affix>(a => {a.Name = "seeker";
                a.Text = "Seeker - Shots home in on enemies.";
                a.Rarity = "epic";
                a.FontColor = ConvertRarityToColour(a.Rarity);
                a.BulletModifiers = new List<Component>() {new HomingModifier {HomingRadius = 4f, AngularVelocity = 360f} }; }),

            // FANTASTIC AFFIXES

            CreateScriptable<Affix>(a => {a.Name = "explosive";
                a.Text = "Explosive - Shots explode.";
                a.Rarity = "fantastic";
                a.FontColor = ConvertRarityToColour(a.Rarity);
                a.BulletModifiers = new List<Component>() {new ExplosiveModifier {explosionData = GameManager.Instance.Dungeon.sharedSettingsPrefab.DefaultSmallExplosionData, doDistortionWave = false, doExplosion = true, IgnoreQueues = true} }; }),

            CreateScriptable<Affix>(a => {a.Name = "grenade";
                a.Text = "Grenade - Fires an explosive grenade on reload.";
                a.Rarity = "fantastic";
                a.FontColor = ConvertRarityToColour(a.Rarity);
                a.OnReload = (PlayerController owner, Gun gun) => {
                    Projectile spawned = (PickupObjectDatabase.GetById(19) as Gun).DefaultModule.projectiles[0].InstantiateAndFireInDirection(gun.barrelOffset.position, owner.FacingDirection).GetComponent<Projectile>();
                    spawned.Owner = owner;
                    spawned.Shooter = owner.specRigidbody;
                };
            }),

        ];
        public PlayerController player;
        public List<Affix> CurrentAffixes = [];
        public List<string> GuaranteedAffixes = [];
        private bool StatsApplied;

        private List<Affix> UncommonAffixes = [];
        private List<Affix> SuperiorAffixes = [];
        private List<Affix> EpicAffixes = [];
        private List<Affix> FantasticAffixes = [];
    }
}