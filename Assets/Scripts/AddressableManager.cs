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
            logger.text = "Exception Occured in Initializing addressables " + e.Message;
        }
    }

    private void AddressableManager_Completed(UnityEngine.ResourceManagement.AsyncOperations.AsyncOperationHandle<UnityEngine.AddressableAssets.ResourceLocators.IResourceLocator> obj)
    {
        if (obj.Status != AsyncOperationStatus.Succeeded)
        {
            logger.text = "Addressables Initialization failed";
            return;
        }
        try
        {
            logger.text = "Addressables Initialized!";
            musicAssetReference.LoadAssetAsync<AudioClip>().Completed += (clip) =>
            {
                if (clip.Status != AsyncOperationStatus.Succeeded)
                {
                    debugLogger.text += "Could not load music" + clip.Status.ToString();
                }
                else
                {
                    debugLogger.text = "Music loaded";
                    AudioSource.PlayClipAtPoint(clip.Result, Vector3.zero);
                }
            }; 
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
