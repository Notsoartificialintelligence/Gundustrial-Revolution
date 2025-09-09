using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Alexandria.ItemAPI;

namespace GunRev
{
    public class EvolverBullets : PassiveItem
    {
        public static void Init()
        {
            string itemName = "Evolver Rounds";

            string resourceName = "GunRev/Resources/Passives/evolverrounds";

            GameObject obj = new GameObject(itemName);

            var item = obj.AddComponent<EvolverBullets>();

            ItemBuilder.AddSpriteToObject(itemName, resourceName, obj);

            string shortDesc = "One Step Forward";
            string longDesc = "Grants all bullets a small chance to evolve enemies. Whether they survive the process is up to natural selection.\n\nBullets that have gleaned information from future timelines. They have witnessed the rise and fall of many nations, species, universes, and trends; all valuable data in the process of self-iteration.";

            ItemBuilder.SetupItem(item, shortDesc, longDesc, "ai");

            item.quality = PickupObject.ItemQuality.A;

        }
        public void PostProcessProjectile(Projectile projectile, float f)
        {

        }

        public override void Pickup(PlayerController player)
        {
            base.Pickup(player);
            player.PostProcessProjectile += this.PostProcessProjectile;
        }

        public override DebrisObject Drop(PlayerController player)
        {
            player.PostProcessProjectile -= this.PostProcessProjectile;
            return base.Drop(player);
        }
    }
}