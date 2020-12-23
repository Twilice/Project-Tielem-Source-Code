using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using System;
#if UNITY_EDITOR
using UnityEditor;
#endif

// error :: doesn't work with skybox? Is there some renderQueue shenanigans going on?
/* bug :: renderLayerMask is ignored by renderer 
 * https://github.com/Unity-Technologies/Graphics/pull/1891 
 * https://forum.unity.com/threads/renderobjects-rendere-feature-is-missing-support-for-rendering-layer-mask.867391/#post-6508969)
 */

public abstract class PortraitRenderer : MonoBehaviour
{
#if UNITY_EDITOR
    public SceneAsset sceneAsset;
#endif
    public string sceneName;
    public Scene scene;
    public PhysicsScene physicsScene;
    public Vector3 offsetPosition;
    public bool autoPlay = true;
    public Camera foundCamera;
    public GameObject previewSceneRoot;
    public bool initialized { get; private set; }
    public bool loadFailed;
    public Action<PortraitRenderer> onLoad;

    public void Start()
    {
        if (initialized == false)
        {
            if (autoPlay)
            {
                Init();
            }
            else
            {
                gameObject.SetActive(false);
            }
        }
    }

    public void Init(string overrideSceneName = null, Action<PortraitRenderer> onLoad = null)
    {
        if (overrideSceneName != null)
            sceneName = overrideSceneName;

        if (Application.CanStreamedLevelBeLoaded(sceneName))
        {
            this.onLoad = onLoad;
            initialized = true;

            LoadSceneParameters loadParams = new LoadSceneParameters(LoadSceneMode.Additive, LocalPhysicsMode.Physics3D);
            SceneManager.sceneLoaded += SceneLoaded;
            SceneManager.LoadSceneAsync(sceneName, loadParams);
        }
        else
        {
            loadFailed = true;
            if (string.IsNullOrEmpty(sceneName))
            {
                Debug.LogWarning($"SceneName is null or empty, please select a scene to be previewed.");
            }
            else
            {
                Debug.LogWarning($"Scene {sceneName} could not be loaded, did you add scene to buildSettings?");
            }
            onLoad(this);
        }
    }

    public void SceneLoaded(Scene loadedScene, LoadSceneMode mode)
    {
        if (sceneName == loadedScene.name)
        {
            SceneManager.sceneLoaded -= SceneLoaded;
            scene = loadedScene;
            physicsScene = scene.GetPhysicsScene();
            foundCamera = null;
            AudioListener audioListener = null;
            scene.GetRootGameObjects().FirstOrDefault(go => go.TryFindComponentInChildren(ref foundCamera, true));
            scene.GetRootGameObjects().FirstOrDefault(go => go.TryFindComponentInChildren(ref audioListener, true));

            audioListener.enabled = false;

            if (foundCamera != null)
            {

                foundCamera.tag = "Untagged";
                //foundCamera.scene = scene; // note :: according to documentation this does not work. Try without layers and see what happens
                BindRenderTexture(foundCamera);
                previewSceneRoot = new GameObject("PreviewSceneRoot");

                int layer = LayerMask.NameToLayer("PortraitRenderer");
                // find not used layer
                //for (int i = 30; i >= 0; i--)
                //{
                //    layer = i;
                //    var layerName = LayerMask.LayerToName(layer);
                //    if (string.IsNullOrEmpty(layerName))
                //    {
                //        break;
                //    }
                //}

                foundCamera.cullingMask = 1 << layer;
                previewSceneRoot.layer = layer;
                // set all non-PortraidCamera to not render portraitScene
                foreach (var cam in Camera.allCameras)
                {
                    if (cam != foundCamera && cam.cullingMask != foundCamera.cullingMask)
                    {
                        cam.cullingMask &= ~(1 << layer);
                    }
                }
                // make render only show for PortraitSceneCameras without changing physics.
                uint renderingLayerMask = (uint)1 << layer;
                SceneManager.MoveGameObjectToScene(previewSceneRoot, scene);
                foreach (var go in scene.GetRootGameObjects())
                {
                    if (go != previewSceneRoot)
                    {
                        go.transform.parent = previewSceneRoot.transform;
                    }
                    foreach (var rend in go.GetComponentsInChildren<Renderer>(true))
                    {
                        rend.gameObject.layer = layer; // temp error :: until Unity solves the renderLayerMask issue...
                        rend.renderingLayerMask = renderingLayerMask;
                    }
                    foreach (var audio in go.GetComponentsInChildren<AudioSource>(true))
                    {
                        audio.enabled = false;
                    }
                }
                previewSceneRoot.transform.position = offsetPosition;

                if (autoPlay)
                    Play();
                else
                    Stop();

                onLoad?.Invoke(this);
            }
            else
            {
                loadFailed = true;
                Debug.LogError($"No camera found in scene {loadedScene.name} to use in PortraitRenderer {gameObject.name}");
            }
        }
    }

    public abstract void BindRenderTexture(Camera cam);

    public void Play()
    {
        if (initialized == false) return;

        gameObject.SetActive(true);
        foundCamera.enabled = true;
        previewSceneRoot.SetActive(true);
    }

    public void Pause()
    {
        if (initialized == false) return;

        foundCamera.enabled = false;
        previewSceneRoot.SetActive(false);
    }

    public void Stop()
    {
        if (initialized == false) return;

        gameObject.SetActive(false);
        foundCamera.enabled = false;
        previewSceneRoot.SetActive(false);
    }

    public void Destroy()
    {
        Cleanup();
    }

    protected virtual void Cleanup()
    {
        if (initialized)
            SceneManager.UnloadSceneAsync(scene);
    }

    private void OnDestroy()
    {
        Cleanup();
    }

    public IEnumerable Simulate()
    {
        // todo :: start simulate
        while(true)
        {
            yield return new WaitForFixedUpdate();
            if(physicsScene.IsValid())
                physicsScene.Simulate(Time.fixedDeltaTime);
        }
    }
}

#if UNITY_EDITOR
[CustomEditor(typeof(PortraitRenderer), editorForChildClasses: true)]
public class PortraitRendererEditor : Editor
{

    SerializedObject previousValue = null;
    SerializedProperty sceneAsset;
    private void OnEnable()
    {
        sceneAsset = serializedObject.FindProperty(nameof(PortraitRenderer.sceneAsset));
    }

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        if (previousValue != sceneAsset.serializedObject)
        {
            PortraitRenderer rend = (PortraitRenderer)target;
            var newValue = sceneAsset.objectReferenceValue as SceneAsset;
            if (newValue != null)
                rend.sceneName = newValue.name;
        }
    }
}
#endif