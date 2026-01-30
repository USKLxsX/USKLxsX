using UnityEngine;
using UnityEngine.UI;

public class MonsterController : MonoBehaviour
{
    [Header("面板引用")]
    public GameObject monsterPanel;        
    public GameObject monsterDetailPanel;  

    [Header("怪物按钮")]
    public Button[] monsterButtons;        

    [Header("详情页按钮")]
    public Button closeMonsterDetailButton;  // 关闭按钮（回主页）
    public Button backMonsterDetailButton;   // 返回按钮（回怪物列表）

    [Header("怪物图鉴面板的返回按钮")]
    public Button backToGalleryFromMonster;  // 返回图鉴主面板

    [Header("详情页UI组件")]
    public Image monsterImage;
    public Text monsterName;
    public Text monsterDescription;
    public Text monsterHP;
    public Text monsterAttack;
    public Text monsterSkills;

    void Start()
    {
        SetupButtonEvents();

        if (monsterDetailPanel != null)
            monsterDetailPanel.SetActive(false);
    }

    void SetupButtonEvents()
    {
        // 怪物按钮
        if (monsterButtons != null)
        {
            for (int i = 0; i < monsterButtons.Length; i++)
            {
                int index = i;
                if (monsterButtons[i] != null)
                {
                    monsterButtons[i].onClick.RemoveAllListeners();
                    monsterButtons[i].onClick.AddListener(() => OnMonsterButtonClicked(index));
                }
            }
        }

        // 详情页按钮
        if (closeMonsterDetailButton != null)
        {
            closeMonsterDetailButton.onClick.RemoveAllListeners();
            closeMonsterDetailButton.onClick.AddListener(OnCloseMonsterDetail);
        }

        if (backMonsterDetailButton != null)
        {
            backMonsterDetailButton.onClick.RemoveAllListeners();
            backMonsterDetailButton.onClick.AddListener(OnBackMonsterDetail);
        }

        // 怪物图鉴面板的返回按钮
        if (backToGalleryFromMonster != null)
        {
            backToGalleryFromMonster.onClick.RemoveAllListeners();
            backToGalleryFromMonster.onClick.AddListener(OnBackToGalleryFromMonster);
        }
    }

    // 怪物按钮点击
    void OnMonsterButtonClicked(int index)
    {
        // 这里填充怪物数据
        // FillMonsterDetail(index);

        // 切换到详情页
        if (monsterPanel != null) monsterPanel.SetActive(false);
        if (monsterDetailPanel != null) monsterDetailPanel.SetActive(true);
    }

    // 关闭怪物详情（回主页）
    void OnCloseMonsterDetail()
    {
        if (GalleryManager.Instance != null)
            GalleryManager.Instance.CloseAllToHome();
    }

    // 返回怪物列表
    void OnBackMonsterDetail()
    {
        if (monsterDetailPanel != null) monsterDetailPanel.SetActive(false);
        if (monsterPanel != null) monsterPanel.SetActive(true);
    }

    // 返回图鉴主面板
    void OnBackToGalleryFromMonster()
    {
        if (GalleryManager.Instance != null)
            GalleryManager.Instance.BackToGalleryMain();
    }
}