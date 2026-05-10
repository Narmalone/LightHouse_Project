/// <summary>
/// Class that stores all base menu paths for scriptableobjects in the projects. This is used to avoid hardcoding menu paths in the scriptableobjects and to have a single place to change them if needed.
/// </summary>
public static class GlobalAssetsMenuPaths
{
    public const string GlobalAssetsMenuPath = "LightHouse/";

    public const string ComputerAssetsMenuPath = GlobalAssetsMenuPath + "Computer/";
    public const string AudioAssetsMenuPath = GlobalAssetsMenuPath + "Audio/";
    public const string WeatherAssetsMenuPath = GlobalAssetsMenuPath + "Weather/";
    public const string BoatsAssetsMenuPath = GlobalAssetsMenuPath + "Boats/";

    #region UTILITIES
    public const string UtilitiesAssetsMenuPath = GlobalAssetsMenuPath + "Utilities/";
    public const string VectorUtilitiesAssetsMenuPath = UtilitiesAssetsMenuPath + "Vector/";
    #endregion
}