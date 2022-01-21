using UnityEngine;
using UnityEngine.Networking;

public class Player : NetworkBehaviour
{
    [SerializeField]
    private GameObject _playerPrefab;
    private GameObject _playerCharacter;

    private void Start()
    {
        SpawnCharacter();
    }

    private void SpawnCharacter()
    {
        if (!isServer)
            return;

        _playerCharacter = Instantiate(_playerPrefab, transform);
        NetworkServer.SpawnWithClientAuthority(_playerCharacter, connectionToClient);
    }
}
