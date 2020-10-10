using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameCoordinator : MonoBehaviour
{
    public GameData data;

    public static GameCoordinator instance;
    [RuntimeInitializeOnLoadMethod]
    public static void Construct()
    {
        if (initialized)
            return;

        if (instance == null)
        {
            instance = new GameObject("GameCoordinator").AddComponent<GameCoordinator>();
        }
        instance.Init();
    }

#if UNITY_WEBGL && !UNITY_EDITOR
    [DllImport("__Internal")]
    private static extern void WebGLStartGame();
    [DllImport("__Internal")]
    private static extern void SendMessageToBrowser(string message);
#endif

    public static bool initialized = false;
    public PlayerShip playerShip;
    public GameField gameField;
    public int gold;

    void Awake()
    {
        if (this != instance && instance != null)
        {
            Destroy(gameObject);
        }

        // If scene is in buildindex we assume for now that should have a GameCoordinator and PlayerShip. Will probably change in the future if non-game scenes are added.
        InitLevel();
    }


    void Init()
    {
        DontDestroyOnLoad(this);
        Time.timeScale = 0;

        const string gameDataName = "GameData";
        data = Resources.Load<GameData>(gameDataName);
        if (data == null)
            throw new System.NullReferenceException($"{nameof(GameCoordinator)} {transform.name} - scriptableObject type {nameof(GameData)} with name {gameDataName} is missing.");
        data = Instantiate(data); // don't change the asset object.
        SceneManager.sceneLoaded += (s, m) => InitLevel();
        initialized = true;
#if UNITY_WEBGL && !UNITY_EDITOR
        //SendMessageToBrowser("Hello from the other side!");
        WebGLStartGame();
#endif
    }

    void InitLevel()
    {
        var _playerShips = FindObjectsOfType<PlayerShip>();
        if(_playerShips.Length > 1)
            throw new System.ArgumentOutOfRangeException($"{nameof(GameCoordinator)} \"{transform.name}\" - Multiple playerShips found {_playerShips.JoinStrings((ship) => ship.name)}.");
        else if(_playerShips.Length < 1)
            Debug.LogWarning($"{nameof(GameCoordinator)} {transform.name} - Playership not found.");
        else
            playerShip = _playerShips[0];

        var _gameFields = FindObjectsOfType<GameField>();
        if (_gameFields.Length > 1)
            throw new System.ArgumentOutOfRangeException($"{nameof(GameCoordinator)} \"{transform.name}\" - Multiple gameFields found {_gameFields.JoinStrings((field) => field.name)}.");
        else if (_gameFields.Length < 1)
            Debug.LogWarning($"{nameof(GameCoordinator)} {transform.name} - gameField not found.");
        else 
            gameField = _gameFields[0];
    }

    public void RestartGame()
    {
        Time.timeScale = 0;
        UI_Select_PrimaryWeapon.ui_weapons = new List<UI_Select_PrimaryWeapon>();
        SceneManager.LoadScene(0);
    }

    public void StartLevel()
    {
        var primaryWeapon = Instantiate(data.selectedPrimaryWeapon);
        primaryWeapon.gameObject.SetActive(false);
        primaryWeapon.ApplyLevel(data.primaryWeaponLevel);

        playerShip.RegisterPrimaryWeapon(primaryWeapon);

        Time.timeScale = 1;
    }

    public void RegisterPrimaryWeapon(PrimaryWeapon primaryWeapon)
    {
        data.primaryWeaponLevel = 1;
        data.selectedPrimaryWeapon = primaryWeapon;      
    }

    public void UpgradePrimaryWeapon()
    {
        data.primaryWeaponLevel++;
        if(data.primaryWeaponLevel > 5)
        {
            data.primaryWeaponLevel = 5;
        }
    }

    public void DowngradePrimaryWeapon()
    {
        data.primaryWeaponLevel--;
        if (data.primaryWeaponLevel < 1)
        {
            data.primaryWeaponLevel = 1;
        }
    }


    // todo :: change of state flow -> pause game while shop, during shop change weapon and after shop continue game.
}
