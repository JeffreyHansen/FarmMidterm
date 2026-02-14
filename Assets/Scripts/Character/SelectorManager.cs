using UnityEngine;
using Farming;
using System.Collections.Generic;

namespace Character
{
    public class SelectorManager : TileSelector
    {
        [Header("Selector Management")]
        [SerializeField] List<TileSelector> selectors = new List<TileSelector>();
        [SerializeField] int currentSelectorIndex = 0;
        [SerializeField] bool allowSwitching = true;
        
        [Header("UI Feedback")]
        [SerializeField] bool showSelectorType = true;
        [SerializeField] float displayTime = 2f;
        
        private TileSelector currentSelector;
        private float lastSwitchTime;
        
        void Start()
        {
            InitializeSelectors();
            if (selectors.Count == 0)
            {
                Debug.LogError("[SelectorManager] No selectors found! Add PointSelector, RaycastSelector, etc. to this GameObject.");
                return;
            }
            SwitchToSelector(currentSelectorIndex);
        }

        void Update()
        {
            // Input handling moved to PlayerController - remove old input polling
            UpdateCurrentTile();
        }

        void InitializeSelectors()
        {
            // Auto-find selectors if list is empty
            if (selectors.Count == 0)
            {
                selectors.AddRange(GetComponents<TileSelector>());
                // Remove this manager from the list
                selectors.Remove(this);
            }

            // Found selectors

            // Disable all selectors initially
            foreach (TileSelector selector in selectors)
            {
                if (selector != this)
                    selector.enabled = false;
            }
        }

        void UpdateCurrentTile()
        {
            // Get tile from current active selector and update our own tile reference
            if (currentSelector != null)
            {
                FarmTile newTile = currentSelector.GetSelectedTile();
                SetActiveTile(newTile);
            }
        }

        // Called from PlayerController when switch input is received
        public void OnSelectorSwitchInput()
        {
            if (allowSwitching)
            {
                SwitchToNextSelector();
            }
        }

        public void SwitchToNextSelector()
        {
            int nextIndex = (currentSelectorIndex + 1) % selectors.Count;
            SwitchToSelector(nextIndex);
        }

        public void SwitchToSelector(int index)
        {
            if (index < 0 || index >= selectors.Count)
            {
                Debug.LogWarning($"Invalid selector index: {index}");
                return;
            }

            // Disable current selector
            if (currentSelector != null)
            {
                currentSelector.enabled = false;
                // Clear highlighting from old selector
                currentSelector.SetActiveTile(null);
            }

            // Switch to new selector
            currentSelectorIndex = index;
            currentSelector = selectors[index];
            currentSelector.enabled = true;
            
            lastSwitchTime = Time.time;
        }

        public void SwitchToSelectorByType<T>() where T : TileSelector
        {
            for (int i = 0; i < selectors.Count; i++)
            {
                if (selectors[i] is T)
                {
                    Debug.Log($"[SelectorManager] Switching to {typeof(T).Name} by type request");
                    SwitchToSelector(i);
                    return;
                }
            }
            Debug.LogWarning($"[SelectorManager] No selector of type {typeof(T).Name} found. Available types: {string.Join(", ", selectors.ConvertAll(s => s.GetType().Name))}");
        }

        public string GetCurrentSelectorName()
        {
            return currentSelector != null ? currentSelector.GetType().Name : "None";
        }

        void OnGUI()
        {
            if (!showSelectorType) return;
            
            // Show current selector type for a few seconds after switching
            if (Time.time - lastSwitchTime < displayTime)
            {
                GUI.Box(new Rect(10, 10, 200, 30), 
                       $"Selector: {GetCurrentSelectorName()}");
            }
        }

        void OnDrawGizmosSelected()
        {
            // Show which selector is currently active
            if (currentSelector != null)
            {
                Gizmos.color = Color.white;
                Vector3 pos = transform.position + Vector3.up * 2f;
                Gizmos.DrawWireCube(pos, Vector3.one * 0.5f);
            }
        }
    }
}