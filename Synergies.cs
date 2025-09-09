using Alexandria.ItemAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GunRev
{
    public class Synergies
    {
        public static CustomSynergyType moxModuleSynergyType = ETGModCompatibility.ExtendEnum<CustomSynergyType>(Module.GUID, "MOX_MODULE");
        public static void Add()
        {
            // Nanobot Cluster Cannon
            CustomSynergies.Add("Shredder Swarm", new List<string>{"ai:nanobot_cluster_cannon"}, new List<string>{"katana_bullets", "knife_shield", "vorpal_bullets", "vorpal_gun", "excaliber"});
            CustomSynergies.Add("Self-Replication", new List<string> {"ai:nanobot_cluster_cannon"}, new List<string> {"nanomachines", "singularity", "double_vision"});
            CustomSynergies.Add("Molecular Magnets", new List<string> {"ai:nanobot_cluster_cannon"}, new List<string> {"relodestone", "big_iron"});

            // Energy Cell
            AdvancedSynergyEntry moxModuleSynergy = CustomSynergies.Add("Mox Module", new List<string> {"ai:energy_cell"}, new List<string> {"blue_guon_stone", "green_guon_stone", "red_guon_stone"});
            moxModuleSynergy.bonusSynergies.Add(moxModuleSynergyType);
        }
    }
}
