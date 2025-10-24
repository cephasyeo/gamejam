using UnityEngine;

[CreateAssetMenu(fileName = "New Orb Stats", menuName = "Game/Orb Stats")]
public class OrbStats : ScriptableObject
{
    [Header("Orb Visual Settings")]
    [Tooltip("The color of this orb")]
    public Color orbColor = Color.white;
    
    [Tooltip("The name of this orb type")]
    public string orbName = "Default Orb";
    
    [Header("Powerup Settings")]
    [Tooltip("The ability this orb grants")]
    public OrbAbility ability = OrbAbility.Jump;
    
    [Tooltip("Base power value for this ability")]
    public float basePower = 1f;
    
    [Tooltip("How much the power increases per stack")]
    public float powerIncreasePerStack = 1f;
    
    [Tooltip("Maximum number of stacks allowed")]
    public int maxStacks = 5;
    
    [Header("Jump Specific Settings")]
    [Tooltip("Jump power multiplier per stack")]
    public float jumpPowerMultiplier = 1.2f;
    
    [Header("Dash Specific Settings")]
    [Tooltip("Dash distance per stack")]
    public float dashDistance = 5f;
    
    [Tooltip("Dash speed")]
    public float dashSpeed = 20f;
    
    [Tooltip("Dash cooldown in seconds")]
    public float dashCooldown = 0.5f;
}

public enum OrbAbility
{
    Jump,
    Dash,
    // Future abilities can be added here
}

