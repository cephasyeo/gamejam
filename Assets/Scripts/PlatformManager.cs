using UnityEngine;
using System.Collections.Generic;

public class PlatformManager : MonoBehaviour
{
    [Header("Platform Prefabs")]
    [SerializeField] private GameObject[] platformPrefabs = new GameObject[5]; // One for each color
    
    [Header("Platform Settings")]
    [SerializeField] private int platformCount = 5;
    [SerializeField] private float platformSpacing = 3f;
    [SerializeField] private float platformHeight = 2f;
    
    [Header("Color Change Settings")]
    [SerializeField] private float colorChangeInterval = 3f;
    [SerializeField] private bool randomizeColorsOnStart = true;
    
    [Header("Debug")]
    [SerializeField] private bool debugMode = true;
    
    private List<Platform> activePlatforms = new List<Platform>();
    private float lastColorChangeTime;
    
    private void Start()
    {
        SpawnPlatforms();
        
        if (randomizeColorsOnStart)
        {
            RandomizeAllPlatformColors();
        }
        
        lastColorChangeTime = Time.time;
    }
    
    private void Update()
    {
        // Change platform colors periodically
        if (Time.time - lastColorChangeTime >= colorChangeInterval)
        {
            RandomizeAllPlatformColors();
            lastColorChangeTime = Time.time;
        }
    }
    
    private void SpawnPlatforms()
    {
        // Clear existing platforms
        foreach (Platform platform in activePlatforms)
        {
            if (platform != null)
                DestroyImmediate(platform.gameObject);
        }
        activePlatforms.Clear();
        
        // Spawn platforms in a line
        for (int i = 0; i < platformCount; i++)
        {
            Vector3 spawnPosition = new Vector3(
                i * platformSpacing - (platformCount - 1) * platformSpacing * 0.5f,
                platformHeight,
                0
            );
            
            // Use white platform as base (index 0)
            GameObject platformObj = Instantiate(platformPrefabs[0], spawnPosition, Quaternion.identity);
            Platform platform = platformObj.GetComponent<Platform>();
            
            if (platform != null)
            {
                activePlatforms.Add(platform);
            }
            
            if (debugMode)
            {
                Debug.Log($"Spawned platform {i} at position: {spawnPosition}");
            }
        }
    }
    
    public void RandomizeAllPlatformColors()
    {
        foreach (Platform platform in activePlatforms)
        {
            if (platform != null)
            {
                platform.RandomizeColor();
            }
        }
        
        if (debugMode)
        {
            Debug.Log("Randomized all platform colors");
        }
    }
    
    public void SetAllPlatformsToColor(PlatformColor color)
    {
        foreach (Platform platform in activePlatforms)
        {
            if (platform != null)
            {
                platform.SetPlatformColor(color);
            }
        }
        
        if (debugMode)
        {
            Debug.Log($"Set all platforms to color: {color}");
        }
    }
    
    public List<Platform> GetActivePlatforms()
    {
        return activePlatforms;
    }
    
    public Platform GetPlatformAt(int index)
    {
        if (index >= 0 && index < activePlatforms.Count)
        {
            return activePlatforms[index];
        }
        return null;
    }
    
    // Method to spawn platforms in custom positions
    public void SpawnPlatformAt(Vector3 position, PlatformColor color = PlatformColor.White)
    {
        GameObject platformObj = Instantiate(platformPrefabs[0], position, Quaternion.identity);
        Platform platform = platformObj.GetComponent<Platform>();
        
        if (platform != null)
        {
            platform.SetPlatformColor(color);
            activePlatforms.Add(platform);
        }
        
        if (debugMode)
        {
            Debug.Log($"Spawned platform at {position} with color {color}");
        }
    }
}

