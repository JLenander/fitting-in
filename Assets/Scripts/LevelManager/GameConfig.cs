/// <summary>
/// A class holding the Levels and other configuration data for the game. (Kept separate so changes are tracked accordingly)
/// The Levels array contains the scene configuration and can be edited to change the relationship between and the existing levels in the level select.
/// </summary>
public class GameConfig
{
    // For some reason when I serialize this it curses my unity
    // Holds the level configuration for the game.
    public static readonly Level[] Levels = {
        new("Tutorial", "Level0", LevelStatus.Unlocked, levelArtSpriteName: "TutorialLevel"),
        new("Cafe", "Cafe", LevelStatus.Unlocked, levelArtSpriteName: "CafeLevel"),
        // new("Carnival", "Carnival", LevelStatus.Unlocked, levelArtSpriteName: "default_level"),
    };
}