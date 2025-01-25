using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class RoomManager : Singleton<RoomManager>
{
    [SerializeField] private Room[] _rooms;
    private Dictionary<GameZone, Room> _roomsDictionnary = new Dictionary<GameZone, Room>();

    #region MOBO CALLBACKS
    protected override void Awake()
    {
        base.Awake();
        _roomsDictionnary = _rooms.ToDictionary(x => x.ElectricityRoom, x => x);
    }

    #endregion

    #region ROOMS FUNCS
    public Room GetRoom(GameZone target)
    {
        return _roomsDictionnary[target];
    }
    #endregion

}
