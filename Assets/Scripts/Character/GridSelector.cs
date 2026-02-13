using UnityEngine;
using Farming;
using System.Collections.Generic;

namespace Character
{
    public class GridSelector : TileSelector
    {
        [Header("Grid Selection Settings")]
        [SerializeField] Vector2Int gridSize = new Vector2Int(4, 4);
        [SerializeField] Vector2Int currentGridPosition = new Vector2Int(0, 0);
        [SerializeField] float selectionCooldown = 0.2f;
        [SerializeField] bool wrapAroundGrid = true;
        
        [Header("Grid Detection")]
        [SerializeField] LayerMask tileLayerMask = -1;
        [SerializeField] float gridSpacing = 1.1f;
        [SerializeField] Transform gridOrigin;
        
        [Header("Input")]
        [SerializeField] KeyCode moveUpKey = KeyCode.UpArrow;
        [SerializeField] KeyCode moveDownKey = KeyCode.DownArrow;
        [SerializeField] KeyCode moveLeftKey = KeyCode.LeftArrow;
        [SerializeField] KeyCode moveRightKey = KeyCode.RightArrow;
        
        private float lastMoveTime;
        private FarmTile[,] tileGrid;
        private bool gridInitialized = false;

        void Start()
        {
            if (!gridOrigin)
                gridOrigin = transform;
                
            InitializeGrid();
        }

        void Update()
        {
            HandleGridInput();
            UpdateSelectedTile();
        }

        void InitializeGrid()
        {
            tileGrid = new FarmTile[gridSize.x, gridSize.y];
            
            // Scan for tiles in a grid pattern
            for (int x = 0; x < gridSize.x; x++)
            {
                for (int y = 0; y < gridSize.y; y++)
                {
                    Vector3 checkPosition = gridOrigin.position + 
                                          new Vector3(x * gridSpacing, 0, y * gridSpacing);
                    
                    // Look for a tile at this grid position
                    Collider[] colliders = Physics.OverlapSphere(checkPosition, gridSpacing * 0.4f, tileLayerMask);
                    
                    foreach (Collider col in colliders)
                    {
                        if (col.TryGetComponent<FarmTile>(out FarmTile tile))
                        {
                            tileGrid[x, y] = tile;
                            break; // Take the first tile found
                        }
                    }
                }
            }
            
            // Find a valid starting position
            FindValidStartPosition();
            gridInitialized = true;
            
            Debug.Log($"Grid initialized: {gridSize.x}x{gridSize.y}, Starting at: {currentGridPosition}");
        }

        void FindValidStartPosition()
        {
            // Try to find a tile at current position, otherwise find any valid tile
            if (!IsValidGridPosition(currentGridPosition) || !GetTileAtPosition(currentGridPosition))
            {
                for (int x = 0; x < gridSize.x; x++)
                {
                    for (int y = 0; y < gridSize.y; y++)
                    {
                        if (tileGrid[x, y] != null)
                        {
                            currentGridPosition = new Vector2Int(x, y);
                            return;
                        }
                    }
                }
            }
        }

        void HandleGridInput()
        {
            if (Time.time - lastMoveTime < selectionCooldown) return;

            Vector2Int inputDirection = Vector2Int.zero;

            if (Input.GetKey(moveUpKey))    inputDirection.y = 1;
            if (Input.GetKey(moveDownKey))  inputDirection.y = -1;
            if (Input.GetKey(moveLeftKey))  inputDirection.x = -1;
            if (Input.GetKey(moveRightKey)) inputDirection.x = 1;

            if (inputDirection != Vector2Int.zero)
            {
                MoveGridSelection(inputDirection);
                lastMoveTime = Time.time;
            }
        }

        void MoveGridSelection(Vector2Int direction)
        {
            Vector2Int newPosition = currentGridPosition + direction;

            if (wrapAroundGrid)
            {
                // Wrap around grid boundaries
                newPosition.x = (newPosition.x + gridSize.x) % gridSize.x;
                newPosition.y = (newPosition.y + gridSize.y) % gridSize.y;
            }
            else
            {
                // Clamp to grid boundaries
                newPosition.x = Mathf.Clamp(newPosition.x, 0, gridSize.x - 1);
                newPosition.y = Mathf.Clamp(newPosition.y, 0, gridSize.y - 1);
            }

            // Only move if there's a tile at the new position (or we're wrapping)
            if (wrapAroundGrid || GetTileAtPosition(newPosition) != null)
            {
                currentGridPosition = newPosition;
            }
        }

        void UpdateSelectedTile()
        {
            if (!gridInitialized) return;
            
            FarmTile selectedTile = GetTileAtPosition(currentGridPosition);
            SetActiveTile(selectedTile);
        }

        bool IsValidGridPosition(Vector2Int position)
        {
            return position.x >= 0 && position.x < gridSize.x && 
                   position.y >= 0 && position.y < gridSize.y;
        }

        FarmTile GetTileAtPosition(Vector2Int position)
        {
            if (!IsValidGridPosition(position)) return null;
            return tileGrid[position.x, position.y];
        }

        // Public methods for external control
        public void SetGridPosition(Vector2Int position)
        {
            if (IsValidGridPosition(position))
            {
                currentGridPosition = position;
            }
        }

        public Vector2Int GetGridPosition()
        {
            return currentGridPosition;
        }

        void OnDrawGizmosSelected()
        {
            if (!gridOrigin) return;

            // Draw grid layout
            Gizmos.color = Color.cyan;
            for (int x = 0; x < gridSize.x; x++)
            {
                for (int y = 0; y < gridSize.y; y++)
                {
                    Vector3 gridPos = gridOrigin.position + new Vector3(x * gridSpacing, 0, y * gridSpacing);
                    
                    // Different color for current selection
                    if (x == currentGridPosition.x && y == currentGridPosition.y)
                    {
                        Gizmos.color = Color.yellow;
                        Gizmos.DrawWireCube(gridPos, Vector3.one * 0.8f);
                        Gizmos.color = Color.cyan;
                    }
                    else
                    {
                        Gizmos.DrawWireCube(gridPos, Vector3.one * 0.5f);
                    }
                }
            }
            
            // Draw grid origin
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(gridOrigin.position, 0.3f);
        }
    }
}