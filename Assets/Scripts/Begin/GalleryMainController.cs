using UnityEngine;
using UnityEngine.UI;

public class GalleryMainController : MonoBehaviour
{
    [Header("按钮")]
    public Button maskGalleryButton;      // 面具图鉴按钮
    public Button monsterGalleryButton;   // 怪物图鉴按钮
    public Button closeGalleryButton;     // 关闭按钮（回主页）

    void Start()
    {
        SetupButtonEvents();
    }

    void SetupButtonEvents()
    {
        // 面具图鉴按钮
        if (maskGalleryButton != null)
        {
            maskGalleryButton.onClick.RemoveAllListeners();
            maskGalleryButton.onClick.AddListener(OnMaskGalleryClicked);
        }

        // 怪物图鉴按钮
        if (monsterGalleryButton != null)
        {
            monsterGalleryButton.onClick.RemoveAllListeners();
            monsterGalleryButton.onClick.AddListener(OnMonsterGalleryClicked);
        }

        // 关闭按钮
        if (closeGalleryButton != null)
        {
            closeGalleryButton.onClick.RemoveAllListeners();
            closeGalleryButton.onClick.AddListener(OnCloseGalleryClicked);
        }
    }

    // 打开面具图鉴
    void OnMaskGalleryClicked()
    {
        if (GalleryManager.Instance != null)
            GalleryManager.Instance.OpenMaskPanel();
    }

    // 打开怪物图鉴
    void OnMonsterGalleryClicked()
    {
        if (GalleryManager.Instance != null)
            GalleryManager.Instance.OpenMonsterPanel();
    }

    // 关闭图鉴（回主页）
    void OnCloseGalleryClicked()
    {
        if (GalleryManager.Instance != null)
            GalleryManager.Instance.CloseAllToHome();
    }
}