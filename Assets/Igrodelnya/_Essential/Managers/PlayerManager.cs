using UnityEngine;

public class PlayerManager : MonoBehaviour
{
    [SerializeField] private Transform _getPoint;
    [SerializeField] private Transform _handPoint;
    //[SerializeField] private TPCameraController _camera;
    [SerializeField] private Transform _cameraPivot;

   // private TPPlayerController _tPPlayer;
    private CharacterController _controller;

    private void Awake()
    {
        if (G.Player == null)
        {
            G.Player = this;
            _controller = GetComponent<CharacterController>();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void Init(Transform spawnPos)
    {

    }
}
