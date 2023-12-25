using Cinemachine;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.UI;

public class AddressableManager : MonoBehaviour
{
    [SerializeField]
    AssetReference playerArmatureReference;

    [SerializeField]
    AssetReference musicAssetReference;

    [SerializeField]
    AssetReferenceTexture2D logo;

    [SerializeField]
    CinemachineVirtualCamera cinemachineVirtualCamera;

    [SerializeField]
    RawImage rawImageLogo;

    [SerializeField]
    TMPro.TMP_Text debugLogger;

    [SerializeField]
    GameObject loading;

    GameObject playerController;


    // Start is called before the first frame update
    void Start()
    {
        try
        {
            Addressables.InitializeAsync().Completed += AddressableManager_Completed;
        }
        catch(Exception e)
        {
            debugLogger.text = "Exception Occured in Initializing addressables " + e.Message;
        }
    }

    private void AddressableManager_Completed(UnityEngine.ResourceManagement.AsyncOperations.AsyncOperationHandle<UnityEngine.AddressableAssets.ResourceLocators.IResourceLocator> obj)
    {
        try
        {
            if (obj.Status != AsyncOperationStatus.Succeeded)
            {
                debugLogger.text += "Addressables Initialization failed";
                return;
            }

            HashSet<object> _keys = new HashSet<object>(obj.Result.Keys);
            debugLogger.text += "\n_keys.Count: " + _keys.Count + "\n";

            musicAssetReference.LoadAssetAsync<AudioClip>().Completed += (clip) =>
            {
                if (clip.Status != AsyncOperationStatus.Succeeded)
                {
                    debugLogger.text += "Could not load music\n" + clip.Status.ToString();
                    //debugLogger.text += "clip.Result : " + clip.Result.ToString() + "\n";
                }
                else
                {
                    debugLogger.text += "Music loaded\n";
                    AudioSource.PlayClipAtPoint(clip.Result, Vector3.zero);
                }
            }; 
           
            debugLogger.text += "Addressables Initialized!\n";
        }
        catch (Exception e)
        {
            debugLogger.text = "Exception Occured in addressable callback " + e.Message;
        }
    }

    private void OnDestroy()
    {
        playerArmatureReference.ReleaseInstance(playerController);
        logo.ReleaseAsset();
    }

    public void LoadPlayer()
    {
        playerArmatureReference.LoadAssetAsync<GameObject>().Completed += (playerArmatureAsset) =>
        {
            playerArmatureReference.InstantiateAsync().Completed += (playerArmatureGO) =>
            {
                if (playerArmatureGO.Status != AsyncOperationStatus.Succeeded)
                {
                    debugLogger.text += "Could not load player Armature\n";
                }
                else
                {
                    playerController = playerArmatureGO.Result;
                    debugLogger.text += "Robot Instantiated" + playerController.name + "\n";
                    cinemachineVirtualCamera.Follow = playerController.transform.Find("PlayerCameraRoot");
                }
            };
        };
    }

    public void LoadIcon()
    {
        logo.LoadAssetAsync<Texture2D>().Completed += (texture) =>
        {
            debugLogger.text += "Logo LoadAssetAsync Texture2D Completed\n";

            if (texture.Status == AsyncOperationStatus.Succeeded)
            {
                rawImageLogo.texture = texture.Result;
                Color currentColor = rawImageLogo.color;
                currentColor.a = 1.0f;
                rawImageLogo.color = currentColor;
            }
            else
            {
                debugLogger.text += "texture.Status: " + texture.Status.ToString() + "\n";
                // debugLogger.text += "loc.Result[0].PrimaryKey: " + texture + "\n";
            }
        };
    }

    private void Update()
    {
        if (LoadingComplete())
        {
            loading.SetActive(false);
        }
    }

    private bool LoadingComplete()
    {
        return (playerArmatureReference.Asset != null && musicAssetReference.Asset != null && logo.Asset != null);
    }
}
