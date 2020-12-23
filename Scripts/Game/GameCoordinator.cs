using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using InvocationFlow;
using Assets.Scripts.CodeUtilities;

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
    public GameObject sceneLoadCanvas;

    void Init()
    {
        DontDestroyOnLoad(this);
        audioSource = gameObject.AddComponent<AudioSource>();

        const string gameDataName = "GameData";
        data = Resources.Load<GameData>(gameDataName);
        if (data == null)
            throw new System.NullReferenceException($"{nameof(GameCoordinator)} {transform.name} - scriptableObject type {nameof(GameData)} with name {gameDataName} is missing.");
        data = Instantiate(data); // don't change the asset object.
        sceneLoadCanvas = Instantiate(data.fadeCanvas);
        DontDestroyOnLoad(sceneLoadCanvas);

        SceneFadeIn();

        SceneInit();
        initialized = true;
#if UNITY_WEBGL && !UNITY_EDITOR
        //SendMessageToBrowser("Hello from the other side!");
        WebGLStartGame();
#endif
    }
    
    public SceneType currentSceneType;
    void SceneChangedFadeIn(AsyncOperation _)
    {
        Application.backgroundLoadingPriority = ThreadPriority.Normal;

        SceneInit();

        if (currentSceneType == SceneType.None)
        {
            SceneFadeIn();
        }
        else if (currentSceneType == SceneType.Shop)
        {
            // note: let init shop control event  - maybe a little cleaner to make a onShopInit event and bind to that.
        }
        else if (currentSceneType == SceneType.Level)
        {
            // todo :: wait for some event to make sure level has loaded in completely? Levels will probably be a bit heavier than other scenes.
            SceneFadeIn();
        }
    }
    public void SceneFadeOut(AsyncOperation asyncLoad, Action onComplete = null)
    {
        var img = sceneLoadCanvas.GetComponentInChildren<Image>();
        sceneLoadCanvas.SetActive(true);
        this.TimeLerpValueThen(0.7f, 0, 1.2f,
        (value) =>
        {
            img.color = img.color.CopyWithAlpha(Mathf.Clamp01(value) * Mathf.Clamp01(value));
        },
        () =>
        {
            if (asyncLoad.progress <= 0.5f)
            {
                var slider = sceneLoadCanvas.GetComponentInChildren<Slider>(true);
                slider.gameObject.SetActive(true);
                slider.value = 0;
                var animationSpin = sceneLoadCanvas.GetComponentInChildren<Animator>(true);
                animationSpin.gameObject.SetActive(true);
                this.InvokeWhileThen(
                () =>
                {
                    slider.value = asyncLoad.progress.RemapRange(0, 0.9f, 0, 1.0f);
                },
                () => asyncLoad.progress != 0.9f,
                () =>
                {
                    slider.value = 1.0f;
                    asyncLoad.allowSceneActivation = true;
                });
            }
            else
            {
                asyncLoad.allowSceneActivation = true;
            }

        });
    }

    public void SceneFadeIn(Action onComplete = null)
    {
        var animationSpin = sceneLoadCanvas.GetComponentInChildren<Animator>(true);
        animationSpin.gameObject.SetActive(false);
        var slider = sceneLoadCanvas.GetComponentInChildren<Slider>(true);
        slider.gameObject.SetActive(false);
        var img = sceneLoadCanvas.GetComponentInChildren<Image>();
        this.TimeLerpValueThen(0.7f, 1, -0.2f, (value) =>
        {
            img.color = img.color.CopyWithAlpha(Mathf.Sqrt(Mathf.Clamp01(value)));
        },
        () =>
        {
            sceneLoadCanvas.SetActive(false);
            onComplete?.Invoke();
        });
    }

    public void LoadSceneFadedOut(int currentLevelIndex)
    {
        //Application.backgroundLoadingPriority = ThreadPriority.Low;

        string sceneName;
        if (currentSceneType == SceneType.Shop)
        {
            sceneName = $"Level{currentLevelIndex}";
        }
        else
        {
            sceneName = "Scene_UIShop";
        }

        //var async = Addressables.LoadSceneAsync(adressableSceneName, activateOnLoad:false);

        if (Application.CanStreamedLevelBeLoaded(sceneName) == false)
        {
            sceneName = "MainMenu";
            currentLevelIndex = 1;
        }
        var async = SceneManager.LoadSceneAsync(sceneName);
        async.allowSceneActivation = false;
        async.completed += SceneChangedFadeIn;
        SceneFadeOut(async);
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
        SceneFadeIn();
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
        LoadSceneFadedOut(0);
    }

    public int currentLevelIndex { get; set; } = 0;
    // temp :: lazy level loading. Should return to shop area then continue on level 2.
    public void StartLevel(int? overrideLevel = null)
    {
        // todo :: handle logic between switching between shop and level automatically. Shop should not need to know which level it is not, just send startlevel to coordinator and correct level loads.
        _weaponShop = null;
        _previewArea = null;

        if (currentSceneType == SceneType.Shop)
        {
            if (overrideLevel.HasValue)
            {
                currentLevelIndex = overrideLevel.Value;
            }
            else
            {
                currentLevelIndex++;
            }
        }
        LoadSceneFadedOut(currentLevelIndex);

    }

    [Obsolete]
    public void RegisterPrimaryWeapon(PrimaryWeapon primaryWeapon)
    {
        data.equippedPrimaryWeapon = primaryWeapon;      
    }
    // todo :: change of state flow -> pause game while shop, during shop change weapon and after shop continue game.
}
