using UnityEngine;
using System;

namespace Farming
{
    /// <summary>
    /// Static event system for farming-related events
    /// </summary>
    public static class FarmingEvents
    {
        // Event fired when a tile is farmed (tilled, watered, etc.)
        public static event Action<FarmTile, FarmTile.Condition, FarmTile.Condition> OnTileFarmed;
        
        // Event fired when a tile is planted (future functionality)
        public static event Action<FarmTile> OnTilePlanted;
        
        // Event fired when a crop is harvested (future functionality)  
        public static event Action<FarmTile> OnTileHarvested;

        /// <summary>
        /// Call this when a tile's condition changes due to farming
        /// </summary>
        public static void TileFarmed(FarmTile tile, FarmTile.Condition previousCondition, FarmTile.Condition newCondition)
        {
            OnTileFarmed?.Invoke(tile, previousCondition, newCondition);
        }

        /// <summary>
        /// Call this when a tile is planted
        /// </summary>
        public static void TilePlanted(FarmTile tile)
        {
            OnTilePlanted?.Invoke(tile);
        }

        /// <summary>
        /// Call this when a crop is harvested
        /// </summary>
        public static void TileHarvested(FarmTile tile)
        {
            OnTileHarvested?.Invoke(tile);
        }
    }
}