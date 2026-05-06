using UnityEngine;

[CreateAssetMenu(fileName = "MineSweeperConfig_", menuName = "LightHouse/Computer/Apps/MineSweeper/New Config")]
public class MineSweeperConfig : ScriptableObject
{
    [Header("--- FIXED VALUES --- ")]
    public int NumberOfColumns = 9;
    public int NumberOfRows = 9;
    public int Mines = 10;

    [Header("--- RANDOM VALUES --- ")]
    [Header("Mines")]
    public bool EnableRandomMines = false;
    public int MinMinesNumber = 7;
    public int MaxMinesNumber = 10;

    [Header("Columns")]
    public bool EnableRandomColumns = false;
    public int MinNumberOfColumns = 9;
    public int MaxNumberOfColumns = 24;

    [Header("Rows")]
    public bool EnableRandomRows = false;
    public int MinNumberOfRows = 9;
    public int MaxNumberOfRows = 24;
}