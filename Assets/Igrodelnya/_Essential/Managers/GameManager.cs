using System;
using UnityEngine;
public class GameManager : MonoBehaviour
{
    [SerializeField] private PlayerManager _player;
    [SerializeField] private Transform _playerSpawnPoint;

    public PlayerManager Player { get { return _player; } }

    public bool isEndGame = false;

    public Action GameStart;
    public Action<int> GameResume;

    private bool _pause;

    private void Awake()
    {
        if (G.Game == null)
        {
            G.Game = this;
            isEndGame = false;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
    private void OnDestroy()
    {
        isEndGame = true;
    }
    public void Init()
    {
       
    }

}
