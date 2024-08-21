using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoomManager : Singleton<RoomManager>
{
    [SerializeField] private Room[] _rooms;

    private Dictionary<GameZone, Room> _roomsDictionnary = new Dictionary<GameZone, Room>();

}
