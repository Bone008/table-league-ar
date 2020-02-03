public static class Constants
{
    public const string PLAYER_TAG = "Player";
    public const string BALL_TAG = "GameBall";
    public const string WALL_TAG = "Walls";
    public const string TOWER_TAG = "Towers";
    public const string FLOOR_TAG = "Floor";
    public const string COLLECTABLE_TAG = "Collectable";
    public const string GOAL_TAG = "Goal";
    
    public const float freezeBallDuration = 5f;
    public const float towerJamDuration = 10f;
    public const float grappleShootDuration = 0.7f;
    public const float grapplePullDuration = 1.2f;
    public const float grappleHoldDuration = 0.5f;
    public const float timeWarning = 30f;

    public const int totalTicks = 10;

    // IMPORTANT: All constants that are somehow connected to in-game units need to be scaled (and property getters, not actually const).
    public static float scaledCeilingHeight => 1f * Scale.gameScale;
    public static float scaledGrappleTargetDistance => 0.25f * Scale.gameScale;
    public static float scaledTowerDistance => 0.35f * Scale.gameScale;

    public static float scaledDistanceFromGoal => 0.13f * Scale.gameScale;
    public static float scaledDallVelocity => 0.5f * Scale.gameScale;
}