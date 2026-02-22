using System.Collections.Generic;
using UnityEngine;

public class CheckpointManager : MonoBehaviour
{
    public static CheckpointManager Instance { get; private set; }

    // Live set: enemies killed since level start
    private readonly HashSet<string> deadEnemyLive = new HashSet<string>();

    // Snapshot set: enemies that are "saved" at last checkpoint
    private HashSet<string> deadEnemySnapshot = new HashSet<string>();

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    // Call when an enemy dies
    public void RegisterEnemyDeath(string enemyId)
    {
        if (!string.IsNullOrEmpty(enemyId))
            deadEnemyLive.Add(enemyId);
    }

    // Call when a photo checkpoint is created
    public void SaveCheckpointSnapshot()
    {
        deadEnemySnapshot = new HashSet<string>(deadEnemyLive);
    }

    // Call when respawning
    public void RestoreToCheckpointSnapshot()
    {
        // Reset live to snapshot (kills after checkpoint are undone)
        deadEnemyLive.Clear();
        foreach (var id in deadEnemySnapshot)
            deadEnemyLive.Add(id);

        // Restore all enemies to correct state
        var all = FindObjectsByType<EnemyID>(
        FindObjectsInactive.Include,
        FindObjectsSortMode.None);
        
        foreach (var e in all)
        {
            var eh = e.GetComponent<EnemyHealth>();
            if (eh == null) continue;

            if (deadEnemySnapshot.Contains(e.id))
                eh.ForceDeadState();   // should stay dead
            else
                eh.ResetEnemy();       // should come back
        }
    }
}