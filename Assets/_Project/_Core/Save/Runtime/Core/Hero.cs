using UnityEngine;

namespace LightHouse.Core.Save.Sample
{
    public class Hero : MonoBehaviour, IBind<PlayerData>
    {
        [field: SerializeField] public SerializableGuid Id { get; set; } = SerializableGuid.NewGuid();
        [SerializeField] private PlayerData _data;

        private void Start()
        {
            Debug.Log(Id.ToString());
        }

        public void Bind(PlayerData data)
        {
            this._data = data;
            this._data.Id = Id;
            transform.position = data.position;
            transform.rotation = data.rotation;
        }

        private void Update()
        {
            _data.position = transform.position;
            _data.rotation = transform.rotation;
        }
    }

    [System.Serializable]
    public class PlayerData : ISaveable
    {
        [field : SerializeField] public SerializableGuid Id { get; set; }
        public Vector3 position;
        public Quaternion rotation;
    }
}
