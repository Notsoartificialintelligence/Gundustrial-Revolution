using System.Collections.Generic;
using MonoMod.RuntimeDetour;
using UnityEngine;


namespace GunRev
{
    public class EnemyReplace : MonoBehaviour
    {
        public static void RunReplace(List<AGDEnemyReplacementTier> m_cachedReplacementTiers)
        {
            if (m_cachedReplacementTiers != null)
            {
                for (int i = 0; i < m_cachedReplacementTiers.Count; i++)
                {
                    if (m_cachedReplacementTiers[i].Name.ToLower().EndsWith("_aimines") | m_cachedReplacementTiers[i].Name.ToLower().EndsWith("_aihollow"))
                    {
                        m_cachedReplacementTiers.Remove(m_cachedReplacementTiers[i]);
                    }
                }
                InitReplacementEnemiesForMines(m_cachedReplacementTiers);
                InitReplacementEnemiesForHollow(m_cachedReplacementTiers);
            }
        }

        private static void InitReplacementEnemiesForMines(List<AGDEnemyReplacementTier> agdEnemyReplacementTiers)
        {
            GlobalDungeonData.ValidTilesets TargetTileset = GlobalDungeonData.ValidTilesets.MINEGEON;
            string nameAppend = "_aimines";
            agdEnemyReplacementTiers.Add(GenerateEnemyReplacementTier("mineletReplace" + nameAppend, new DungeonPrerequisite[0], TargetTileset, Minelet, BeamTurret, .2f));
            agdEnemyReplacementTiers.Add(GenerateEnemyReplacementTier("bulletkinReplace" + nameAppend, new DungeonPrerequisite[0], TargetTileset, BulletKin, BeamTurret, .1f));
            return;
        }
        private static void InitReplacementEnemiesForHollow(List<AGDEnemyReplacementTier> agdEnemyReplacementTiers)
        {
            GlobalDungeonData.ValidTilesets TargetTileset = GlobalDungeonData.ValidTilesets.CATACOMBGEON;
            string nameAppend = "_aihollow";
            agdEnemyReplacementTiers.Add(GenerateEnemyReplacementTier("bulletkinReplace" + nameAppend, new DungeonPrerequisite[0], TargetTileset, BulletKin, HoloKin, .2f));
            return;
        }
        public static AGDEnemyReplacementTier GenerateEnemyReplacementTier(string m_name, DungeonPrerequisite[] m_Prereqs, GlobalDungeonData.ValidTilesets m_TargetTileset, List<string> m_TargetGuids, List<string> m_ReplacementGUIDs, float m_ChanceToReplace = 1)
        {
            AGDEnemyReplacementTier m_cachedEnemyReplacementTier = new AGDEnemyReplacementTier()
            {
                Name = m_name,
                Prereqs = m_Prereqs,
                TargetTileset = m_TargetTileset,
                ChanceToReplace = m_ChanceToReplace,
                MaxPerFloor = -1,
                MaxPerRun = -1,
                TargetAllNonSignatureEnemies = false,
                TargetAllSignatureEnemies = false,
                TargetGuids = m_TargetGuids,
                ReplacementGuids = m_ReplacementGUIDs,
                RoomMustHaveColumns = false,
                RoomMinEnemyCount = -1,
                RoomMaxEnemyCount = -1,
                RoomMinSize = -1,
                RemoveAllOtherEnemies = false,
                RoomCantContain = new List<string>()
            };
            return m_cachedEnemyReplacementTier;
        }
        public static List<string> Minelet = new List<string>()
        {
            "3cadf10c489b461f9fb8814abc1a09c1"
        };
        public static List<string> BulletKin = new List<string>() {
            "01972dee89fc4404a5c408d50007dad5"
        };
        public static List<string> HoloKin = new List<string>() {
            "79711df6b2ba4dfaa49b5502dadefcac"
        };
        public static List<string> BeamTurret = new List<string>()
        {
            "fdd43f47902945bab28910c8c01b6ea5"
        };
    }
}