using UnityEngine;
using Farming;
using System.Collections.Generic;

namespace Character
{
    public class SphereSelector : TileSelector
    {
        [Header("Sphere Detection Settings")]
        [SerializeField] float detectionRadius = 3f;
        [SerializeField] LayerMask tileLayerMask = -1;
        [SerializeField] Transform detectionCenter;
        [SerializeField] float updateRate = 10f; // Times per second to check
        
        [Header("Selection Preference")]
        [SerializeField] bool preferForwardTiles = true; // Prefer tiles in front of player
        [SerializeField] float forwardBias = 2f; // How much to prefer forward tiles
        
        private float lastUpdateTime;
        private List<FarmTile> tilesInRange = new List<FarmTile>();

        void Start()
        {
            // Use this GameObject as detection center if not assigned
            if (!detectionCenter)
                detectionCenter = transform;
        }

        void Update()
        {
            // Update at specified rate to save performance
            if (Time.time - lastUpdateTime > 1f / updateRate)
            {
                lastUpdateTime = Time.time;
                FindNearestTile();
            }
        }

        void FindNearestTile()
        {
            // Clear previous results
            tilesInRange.Clear();
            
            // Find all colliders within sphere
            Collider[] colliders = Physics.OverlapSphere(detectionCenter.position, detectionRadius, tileLayerMask);
            
            // Filter for FarmTiles and add to list
            foreach (Collider col in colliders)
            {
                if (col.TryGetComponent<FarmTile>(out FarmTile tile))
                {
                    tilesInRange.Add(tile);
                }
            }

            // Find best tile based on distance and forward preference
            FarmTile bestTile = null;
            float bestScore = Mathf.Infinity;

            foreach (FarmTile tile in tilesInRange)
            {
                Vector3 directionToTile = tile.transform.position - detectionCenter.position;
                float distance = directionToTile.magnitude;
                float score = distance;

                if (preferForwardTiles)
                {
                    // Calculate how much tile is in front of the detection center
                    Vector3 forward = detectionCenter.forward;
                    float forwardDot = Vector3.Dot(directionToTile.normalized, forward);
                    
                    // Negative dot means behind, positive means in front
                    // Reduce score (make it better) for tiles in front
                    score -= forwardDot * forwardBias;
                }

                if (score < bestScore)
                {
                    bestScore = score;
                    bestTile = tile;
                }
            }

            SetActiveTile(bestTile);
        }

        void OnDrawGizmosSelected()
        {
            if (!detectionCenter) return;

            // Draw detection sphere
            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(detectionCenter.position, detectionRadius);
            
            // Draw forward direction
            if (preferForwardTiles)
            {
                Gizmos.color = Color.red;
                Gizmos.DrawLine(detectionCenter.position, 
                               detectionCenter.position + detectionCenter.forward * detectionRadius * 0.5f);
            }

            // Draw lines to tiles in range (only in play mode)
            if (Application.isPlaying)
            {
                Gizmos.color = Color.yellow;
                foreach (FarmTile tile in tilesInRange)
                {
                    if (tile != null)
                        Gizmos.DrawLine(detectionCenter.position, tile.transform.position);
                }

                // Highlight selected tile
                if (activeTile != null)
                {
                    Gizmos.color = Color.green;
                    Gizmos.DrawWireCube(activeTile.transform.position, Vector3.one * 1.2f);
                }
            }
        }
    }
}