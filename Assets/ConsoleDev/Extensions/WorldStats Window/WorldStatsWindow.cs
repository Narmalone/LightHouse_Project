using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class WorldStatsWindow : MonoBehaviour
{
    //IDEAS TO IMPLEMENT:
    //player speed (velocity)
    //health if not showed by an ui
    //playtime
    //total playtime
    [SerializeField] private ResizableConsoleWidth resizableConsole;

    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private TextMeshProUGUI m_playerCoords;
    [SerializeField] private TextMeshProUGUI m_playerRotation;
    [SerializeField] private TextMeshProUGUI m_cardinalPoints;
    [SerializeField] private TextMeshProUGUI m_timePlayed;
    [SerializeField] private TextMeshProUGUI m_currentZoneTxt;

    [SerializeField] private Button closeWindowButton;
    public bool IsShowed = false;

    [SerializeField] private Transform m_player;
    [SerializeField] private Transform m_targetToCardinalPoint;
    public enum CardinalPoint
    {
        North,
        South,
        East,
        West,
        NorthEast,
        SouthEast,
        NorthWest,
        SouthWest
    }

    public enum Zone
    {
        None,
        Kitchen,
        Bedroom,
        Control,
        Service,
        Lantern
    }

    public CardinalPoint GetFacingDirection(Transform target)
    {
        // Get the player's forward direction
        Vector3 forward = target.forward;
        forward = forward.normalized;

        // Define the cardinal directions as vectors
        Vector3 north = Vector3.forward;
        Vector3 south = Vector3.back;
        Vector3 east = Vector3.right;
        Vector3 west = Vector3.left;
        Vector3 northEast = (north + east).normalized;
        Vector3 southEast = (south + east).normalized;
        Vector3 northWest = (north + west).normalized;
        Vector3 southWest = (south + west).normalized;

        // Find the maximum dot product
        float maxDot = float.MinValue;
        CardinalPoint direction = CardinalPoint.North;

        // Compare the forward vector with each cardinal direction
        if (Vector3.Dot(forward, north) > maxDot)
        {
            maxDot = Vector3.Dot(forward, north);
            direction = CardinalPoint.North;
        }
        if (Vector3.Dot(forward, south) > maxDot)
        {
            maxDot = Vector3.Dot(forward, south);
            direction = CardinalPoint.South;
        }
        if (Vector3.Dot(forward, east) > maxDot)
        {
            maxDot = Vector3.Dot(forward, east);
            direction = CardinalPoint.East;
        }
        if (Vector3.Dot(forward, west) > maxDot)
        {
            maxDot = Vector3.Dot(forward, west);
            direction = CardinalPoint.West;
        }
        if (Vector3.Dot(forward, northEast) > maxDot)
        {
            maxDot = Vector3.Dot(forward, northEast);
            direction = CardinalPoint.NorthEast;
        }
        if (Vector3.Dot(forward, southEast) > maxDot)
        {
            maxDot = Vector3.Dot(forward, southEast);
            direction = CardinalPoint.SouthEast;
        }
        if (Vector3.Dot(forward, northWest) > maxDot)
        {
            maxDot = Vector3.Dot(forward, northWest);
            direction = CardinalPoint.NorthWest;
        }
        if (Vector3.Dot(forward, southWest) > maxDot)
        {
            maxDot = Vector3.Dot(forward, southWest);
            direction = CardinalPoint.SouthWest;
        }

        return direction;
    }

    private Zone m_currentZone;

    private void Awake()
    {
        WorldZoneTrigger.OnZoneChange += (s) =>
        {
            m_currentZone = s;
        };

        if (canvasGroup.alpha == 0f)
            Disable();
        else
            Enable();

        closeWindowButton.onClick.AddListener(() =>
        {
            Disable();
        });
    }
    public void ResetWindow()
    {
        resizableConsole.consoleRect.anchoredPosition = resizableConsole.StartPos;
        resizableConsole.consoleRect.sizeDelta = new Vector2(resizableConsole.InitialSize.x, resizableConsole.InitialSize.y);
    }

    public void Enable()
    {
        IsShowed = true;
        canvasGroup.alpha = 1.0f;
        canvasGroup.interactable = true;
        canvasGroup.blocksRaycasts = true;
    }

    public void Disable()
    {
        IsShowed = false;
        canvasGroup.alpha = 0f;
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;
    }
    private void Update()
    {
        if (canvasGroup.alpha <= 0f) return;
        m_playerCoords.text = string.Format("Pos (x: {0:F2}, y: {1:F2}, z: {2:F2})",
                                            m_player.position.x,
                                            m_player.position.y,
                                            m_player.position.z);
        m_cardinalPoints.text = "Dir: " + GetFacingDirection(m_targetToCardinalPoint).ToString();
        m_playerRotation.text = string.Format("Rot: {0:F2}, y: {1:F2}, z: {2:F2})",
                                            m_player.rotation.eulerAngles.x,
                                            m_player.rotation.eulerAngles.y,
                                            m_player.rotation.eulerAngles.z);
        m_timePlayed.text = string.Format("PlayTime: {0:00}:{1:00}:{2:00}",
                                        (int)Time.time / 3600, (int)Time.time % 3600 / 60, (int)Time.time % 60);

        m_currentZoneTxt.text = "Zone: " + GetCurrentZone();
    }

    public string GetCurrentZone()
    {
        return m_currentZone.ToString();
    }
}
