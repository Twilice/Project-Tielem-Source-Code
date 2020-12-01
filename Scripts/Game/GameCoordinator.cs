using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameCoordinator : MonoBehaviour
{
    [Serializable]
    public enum SceneType
    {
        None,
        Level,
        Shop
    }

    // *** global construct *** 
    #region construct
    public GameData data;
    public static GameCoordinator instance;
    public static bool initialized = false;

    // todo :: coordinator needs mayor overhaul to be able to move back and forth between states.

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
    #endregion 

#if UNITY_WEBGL && !UNITY_EDITOR
    [DllImport("__Internal")]
    private static extern void WebGLStartGame();
    [DllImport("__Internal")]
    private static extern void SendMessageToBrowser(string message);
#endif

    public PlayerShip playerShip;
    public GameField gameField = null;
    public int currencyValue { get; set; } = 10000;

    public AudioSource audioSource;

    void Init()
    {
        DontDestroyOnLoad(this);
        audioSource = gameObject.AddComponent<AudioSource>();

        const string gameDataName = "GameData";
        data = Resources.Load<GameData>(gameDataName);
        if (data == null)
            throw new System.NullReferenceException($"{nameof(GameCoordinator)} {transform.name} - scriptableObject type {nameof(GameData)} with name {gameDataName} is missing.");
        data = Instantiate(data); // don't change the asset object.
        SceneManager.sceneLoaded += (s, m) => SceneChanged(s,m);
        SceneInit();
        initialized = true;
#if UNITY_WEBGL && !UNITY_EDITOR
        //SendMessageToBrowser("Hello from the other side!");
        WebGLStartGame();
#endif
    }
    public SceneType currentSceneType;
    void SceneChanged(Scene scene, LoadSceneMode loadMode)
    {
      

        SceneInit();
    }

    void SceneInit()
    {
        playerShip = FindObjectOfType<PlayerShip>();
        gameField = FindObjectOfType<GameField>();

        currentSceneType = DetermineSceneType();

        switch (currentSceneType)
        {
            case SceneType.None:
                break;
            case SceneType.Level:
                InitLevel();
                break;
            case SceneType.Shop:
                //InitShopUI(); // note : we wait until resources have loaded, then init.
                break;
            default:
                break;
        }
    }

    SceneType DetermineSceneType()
    {
        if (playerShip != null && gameField != null)
            return SceneType.Level;

        if (FindObjectOfType<UI_WeaponShop>() != null)
            return SceneType.Shop;

        return SceneType.None;
    }

    #region UI_SHOP
    private UI_WeaponShop _weaponShop;
    public UI_WeaponShop weaponShop
    {
        get => _weaponShop;
        set {
            _weaponShop = value;
            if (_previewArea != null)
            {
                AsyncCreatePortraitRenderers();
            }
        }
    }
    private UI_PreviewPortrait_Area _previewArea;
    public UI_PreviewPortrait_Area previewArea
    {
        get => _previewArea;
        set
        {
            _previewArea = value;
            if (_weaponShop != null)
            {
                AsyncCreatePortraitRenderers();
            }
        }
    }

    // note :: PortraitRenderers are asynscronous loaded scenes, but objects are dependant. So we have to wait for loading to continue.
    void AsyncCreatePortraitRenderers()
    {
        previewArea.CreatePortraitRenderers(instance.weaponShop.weapons, InitShopUI);
    }

    void InitShopUI()
    {
        weaponShop.Init();
    }
    #endregion

    #region LEVEL
    void InitLevel()
    {
#if DEBUG
        var _playerShips = FindObjectsOfType<PlayerShip>();
        if(_playerShips.Length > 1)
            throw new System.ArgumentOutOfRangeException($"{nameof(GameCoordinator)} \"{transform.name}\" - Multiple playerShips found {_playerShips.JoinStrings((ship) => ship.name)}.");
        else if(_playerShips.Length < 1)
            Debug.LogWarning($"{nameof(GameCoordinator)} {transform.name} - Playership not found.");

        var _gameFields = FindObjectsOfType<GameField>();
        if (_gameFields.Length > 1)
            throw new System.ArgumentOutOfRangeException($"{nameof(GameCoordinator)} \"{transform.name}\" - Multiple gameFields found {_gameFields.JoinStrings((field) => field.name)}.");
        else if (_gameFields.Length < 1)
            Debug.LogWarning($"{nameof(GameCoordinator)} {transform.name} - gameField not found.");
#endif

        var primaryWeapon = Instantiate(data.equippedPrimaryWeaponPrefab);
        data.equippedPrimaryWeapon = primaryWeapon;
        primaryWeapon.gameObject.SetActive(false);
        primaryWeapon.ApplyLevel(data.primaryWeaponLevel);

        playerShip.RegisterPrimaryWeapon(primaryWeapon);
    }
    #endregion

    public static bool PlayOneShot(AudioClip audioClip)
    {
        if(audioClip != null)
        {
            instance.audioSource.PlayOneShot(audioClip);
            return true;
        }
        return false;
    }

    public void RestartGame()
    {
        currentLevelIndex = 0;
        SceneManager.LoadScene(0);
    }

    public int currentLevelIndex = 0;
    // temp :: lazy level loading. Should return to shop area then continue on level 2.
    public void StartLevel()
    {
        _weaponShop = null;
        _previewArea = null;
        currentLevelIndex++;
        SceneManager.LoadScene(currentLevelIndex);
    }

    [Obsolete]
    public void RegisterPrimaryWeapon(PrimaryWeapon primaryWeapon)
    {
        data.equippedPrimaryWeapon = primaryWeapon;      
    }
    // todo :: change of state flow -> pause game while shop, during shop change weapon and after shop continue game.
}
