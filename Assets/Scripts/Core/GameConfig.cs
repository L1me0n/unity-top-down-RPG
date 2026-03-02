using UnityEngine;

public static class GameConfig
{
    [Header("Player Stats")]
    public const int PlayerStartHP = 10;
    public const int PlayerStartAP = 10;
    public const int PlayerStartDP = 2;
    public const float FadeDisappearSeconds = 1.2f;

    [Header("Player Movement")]
    public const float PlayerMoveSpeed = 7.0f; // units/sec
    public const float PlayerAcceleration = 60.0f; // units/sec^2
    public const float PlayerDeceleration = 80.0f; // units/sec^2

    [Header("Aiming")]
    public const float AimDeadzone = 0.05f; // world units
    
}