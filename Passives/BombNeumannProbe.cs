using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Alexandria.ItemAPI;

namespace GunRev
{
    public class BombNeumannProbe : PassiveItem
    {
        public static void Init()
        {
            string itemName = "Bomb Neumann Probe";

            string resourceName = "GunRev/Resources/Passives/bombneumann";

            GameObject obj = new GameObject(itemName);

            var item = obj.AddComponent<BombNeumannProbe>();

            ItemBuilder.AddSpriteToObject(itemName, resourceName, obj);

            string shortDesc = "For The Swarm";
            string longDesc = "Self-replicates when an enemy is defeated by a probe.\n\n";

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