using UnityEngine;

public static class GameplayMetrics
{

    public const int StacksPerArmy = 7;

    public const KeyCode SplitKeyCode = KeyCode.LeftShift;

    public static string[] ResourceNames = { "Gold", "Wood", "Ore" };

    public static string[] BuildingNames = { "Sawmill", "Obelisk", "Gold Mine" };

    public static string[] TroopNames = { "Pikeman", "Archer", "Knight", "Griffin" };

    public static Texture2D[] TroopIcons;

    public static Sprite[] TroopSprites75;

    public static Sprite GetTroopSprite75(int index)
    {

        if (TroopSprites75 == null)
        {
            TroopSprites75 = new Sprite[TroopIcons.Length];
        }

        if (TroopSprites75[index] == null)
        {
            TroopSprites75[index] = Sprite.Create(
                TroopIcons[index],
                new Rect(Vector2.zero, Vector2.one * 75),
                Vector2.one * 0.5f
            );
        }

        return TroopSprites75[index];
    }

}
