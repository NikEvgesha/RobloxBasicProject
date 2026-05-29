using UnityEngine;

public class GameEntryPoint : MonoBehaviour
{
    //[SerializeField] private Inventory _inventory;
    [SerializeField] private PlayerManager _player;
    [SerializeField] private GameObject _ui;

    //[SerializeField] private DailyPlaytimeTrackerMB _dailyPlaytimeTracker;

    [SerializeField] private Transform _playerSpawnPoint;

    private void Start()
    {
        if (_player != null) Instantiate(_player).Init(_playerSpawnPoint);
        if (_ui != null) Instantiate(_ui);

        G.Initialized?.Invoke();
        G.GameLoader.ShowLoadingScreen(false);
    }
}
