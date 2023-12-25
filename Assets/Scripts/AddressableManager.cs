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
    TMPro.TMP_Text logger;

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
                debugLogger.text += "Addressables Initialization failed\n";
                return;
            }

            HashSet<object> _keys = new HashSet<object>(obj.Result.Keys);
            debugLogger.text += "_keys.Count: " + _keys.Count + "\n";

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

    //private void OnDestroy()
    //{
    //    playerArmatureReference.ReleaseInstance(playerController);
    //    logo.ReleaseAsset();
    //}

    public void LoadPlayer()
    {
        logger.text = "Loading player";
        if (playerArmatureReference.Asset != null)
        {
            logger.text = "Player already loaded\n";
            return;
        }
        playerArmatureReference.LoadAssetAsync<GameObject>().Completed += (playerArmatureAsset) =>
        {
            playerArmatureReference.InstantiateAsync().Completed += (playerArmatureGO) =>
            {
                if (playerArmatureGO.Status != AsyncOperationStatus.Succeeded)
                {
                    logger.text = "Could not load player Armature\n";
                }
                else
                {
                    playerController = playerArmatureGO.Result;                    
                    cinemachineVirtualCamera.Follow = playerController.transform.Find("PlayerCameraRoot");
                    logger.text = "Player loaded!";
                }
            };
        };
    }

    public void LoadIcon()
    {
        logger.text = "Loading logo";
        if (logo.Asset != null)
        {
            logger.text = "Logo already loaded\n";
            return;
        }
        logo.LoadAssetAsync<Texture2D>().Completed += (texture) =>
        {
            if (texture.Status == AsyncOperationStatus.Succeeded)
            {
                rawImageLogo.texture = texture.Result;
                Color currentColor = rawImageLogo.color;
                currentColor.a = 1.0f;
                rawImageLogo.color = currentColor;
                logger.text = "Logo loaded\n";
            }
            else
            {
                logger.text = "Could not load Logo";                
            }
        };
    }
    private bool LoadingComplete()
    {
        return (playerArmatureReference.Asset != null && musicAssetReference.Asset != null && logo.Asset != null);
    }
}
