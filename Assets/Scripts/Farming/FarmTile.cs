using System.Collections.Generic;
using UnityEngine;
using Environment;

namespace Farming 
{
    public class FarmTile : MonoBehaviour
    {
        public enum Condition { Grass, Tilled, Watered /*, Planted */ }

        [SerializeField] private Condition tileCondition = Condition.Grass; 

        [Header("Visuals")]
        [SerializeField] private Material grassMaterial;
        [SerializeField] private Material tilledMaterial;
        [SerializeField] private Material wateredMaterial;
        // [SerializeField] private Material plantedMaterial; // Commented out - using grass material for now
        MeshRenderer tileRenderer;

        [Header("Growth & Regrowth")]
        [SerializeField] private float regrowthTime = 15f; // Seconds before watered tile grows back into grass
        private float waterTimer = 0f;
        private bool isRegrowing = false;

        [Header("Audio")]
        [SerializeField] private AudioSource stepAudio;
        [SerializeField] private AudioSource tillAudio;
        [SerializeField] private AudioSource waterAudio;

        List<Material> materials = new List<Material>();

        private int daysSinceLastInteraction = 0;
        public FarmTile.Condition GetCondition { get { return tileCondition; } }

        void Start()
        {
            tileRenderer = GetComponent<MeshRenderer>();
            Debug.Assert(tileRenderer, "FarmTile requires a MeshRenderer");

            foreach (Transform edge in transform)
            {
                materials.Add(edge.gameObject.GetComponent<MeshRenderer>().material);
            }
        }

        void Update()
        {
            // Regrowth timer: watered tiles grow back into grass after time
            if (isRegrowing && tileCondition == Condition.Watered)
            {
                waterTimer += Time.deltaTime;
                if (waterTimer >= regrowthTime)
                {
                    RegrowGrass();
                }
            }
        }

        /// <summary>
        /// Interact without water check (tilling only).
        /// For watering, use InteractWithWater() instead.
        /// </summary>
        public void Interact()
        {
            switch(tileCondition)
            {
                case FarmTile.Condition.Grass: Till(); break;
                case FarmTile.Condition.Tilled:
                    // Need water to irrigate tilled land
                    break;
                case FarmTile.Condition.Watered:
                    // Already watered - plants are growing
                    break;
                // case FarmTile.Condition.Planted:
                //     Harvest();
                //     break;
            }
            daysSinceLastInteraction = 0;
        }

        /// <summary>
        /// Interact with water resource check. Consumes water only when watering tilled land.
        /// </summary>
        public bool InteractWithWater(Character.WaterResource waterResource)
        {
            switch(tileCondition)
            {
                case FarmTile.Condition.Grass:
                    Till(); // Tilling doesn't require water
                    daysSinceLastInteraction = 0;
                    return true;
                    
                case FarmTile.Condition.Tilled:
                    if (waterResource != null && waterResource.TryConsumeWater())
                    {
                        Water();
                        daysSinceLastInteraction = 0;
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                    
                case FarmTile.Condition.Watered:
                    return false;
                    
                // case FarmTile.Condition.Planted:
                //     Harvest();
                //     return true;
            }
            return false;
        }

        public void Till()
        {
            Condition previousCondition = tileCondition;
            tileCondition = FarmTile.Condition.Tilled;
            UpdateVisual();
            tillAudio?.Play();
            
            // Fire farming event for progress tracking
            FarmingEvents.TileFarmed(this, previousCondition, tileCondition);
        }

        public void Water()
        {
            Condition previousCondition = tileCondition;
            tileCondition = FarmTile.Condition.Watered;
            UpdateVisual();
            waterAudio?.Play();
            
            // Start regrowth timer - watered tiles will grow back into grass
            waterTimer = 0f;
            isRegrowing = true;
            
            // Fire farming event for progress tracking
            FarmingEvents.TileFarmed(this, previousCondition, tileCondition);
        }

        /// <summary>
        /// Called automatically when the regrowth timer completes.
        /// Watered tile grows back into grass.
        /// </summary>
        void RegrowGrass()
        {
            Condition previousCondition = tileCondition;
            tileCondition = Condition.Grass;
            isRegrowing = false;
            waterTimer = 0f;
            UpdateVisual();
            
            // Fire farming event for progress tracking
            FarmingEvents.TileFarmed(this, previousCondition, tileCondition);
        }

        /// <summary>
        /// Called automatically when the growth timer completes.
        /// Watered tile becomes a planted tile with crops.
        /// Currently disabled - planted state uses grass material.
        /// </summary>
        // void GrowPlant()
        // {
        //     Condition previousCondition = tileCondition;
        //     tileCondition = Condition.Planted;
        //     isGrowing = false;
        //     waterTimer = 0f;
        //     UpdateVisual();
        //     Debug.Log($"[FarmTile] {gameObject.name} has grown into a plant!");
        //     FarmingEvents.TilePlanted(this);
        // }

        private void UpdateVisual()
        {
            if(tileRenderer == null) return;
            switch(tileCondition)
            {
                case FarmTile.Condition.Grass: tileRenderer.material = grassMaterial; break;
                case FarmTile.Condition.Tilled: tileRenderer.material = tilledMaterial; break;
                case FarmTile.Condition.Watered: tileRenderer.material = wateredMaterial; break;
                // case FarmTile.Condition.Planted: 
                //     tileRenderer.material = plantedMaterial != null ? plantedMaterial : grassMaterial;
                //     break;
            }
        }

        public void SetHighlight(bool active)
        {
            foreach (Material m in materials)
            {
                if (active)
                {
                    m.EnableKeyword("_EMISSION");
                } 
                else 
                {
                    m.DisableKeyword("_EMISSION");
                }
            }
            if (active) stepAudio.Play();
        }

        public void OnDayPassed()
        {
            daysSinceLastInteraction++;
            if(daysSinceLastInteraction >= 2)
            {
                // if(tileCondition == FarmTile.Condition.Planted) tileCondition = FarmTile.Condition.Grass;
                if(tileCondition == FarmTile.Condition.Watered) tileCondition = FarmTile.Condition.Tilled;
                else if(tileCondition == FarmTile.Condition.Tilled) tileCondition = FarmTile.Condition.Grass;
                isRegrowing = false;
                waterTimer = 0f;
            }
            UpdateVisual();
        }

        /// <summary>
        /// Harvest a planted tile. Resets to grass and fires harvest event.
        /// Currently disabled - planted state commented out.
        /// </summary>
        // public void Harvest()
        // {
        //     if (tileCondition != Condition.Planted) return;
        //     Debug.Log($"[FarmTile] Harvested {gameObject.name}!");
        //     tileCondition = Condition.Grass;
        //     isGrowing = false;
        //     waterTimer = 0f;
        //     UpdateVisual();
        //     FarmingEvents.TileHarvested(this);
        // }
    }
}