using UnityEngine;
using UnityEngine.UI;

public class TutorialPanelController : MonoBehaviour
{
    [Header("教程界面")]
    [SerializeField] private GameObject tutorialPanel;
    [SerializeField] private Button closeButton;
    [SerializeField] private Button nextPageButton;
    [SerializeField] private Button prevPageButton;
    [SerializeField] private Text pageNumberText;

    [Header("内容页面")]
    [SerializeField] private GameObject[] contentPages;

    private int currentPageIndex = 0;

    void Start()
    {
        // 初始化按钮监听
        if (closeButton != null)
        {
            closeButton.onClick.AddListener(CloseTutorial);
        }

        if (nextPageButton != null)
        {
            nextPageButton.onClick.AddListener(GoToNextPage);
        }

        if (prevPageButton != null)
        {
            prevPageButton.onClick.AddListener(GoToPreviousPage);
        }

        // 确保初始状态
        if (tutorialPanel != null)
        {
            tutorialPanel.SetActive(false);
        }
    }

    // 打开教程页面（从主菜单调用）
    public void OpenTutorial()
    {
        if (tutorialPanel == null) return;

        tutorialPanel.SetActive(true);
        currentPageIndex = 0;
        UpdatePageDisplay();
    }

    // 关闭教程页面
    void CloseTutorial()
    {
        if (tutorialPanel != null)
        {
            tutorialPanel.SetActive(false);

            // 通知主菜单恢复交互
            MainMenuController menuController = FindObjectOfType<MainMenuController>();
            if (menuController != null)
            {
                menuController.OnTutorialClosed();
            }
        }
    }

    // 下一页
    void GoToNextPage()
    {
        if (contentPages == null || contentPages.Length == 0) return;

        if (currentPageIndex < contentPages.Length - 1)
        {
            currentPageIndex++;
            UpdatePageDisplay();
        }
    }

    // 上一页
    void GoToPreviousPage()
    {
        if (contentPages == null || contentPages.Length == 0) return;

        if (currentPageIndex > 0)
        {
            currentPageIndex--;
            UpdatePageDisplay();
        }
    }

    // 更新页面显示
    void UpdatePageDisplay()
    {
        if (contentPages == null) return;

        // 隐藏所有页面，显示当前页面
        for (int i = 0; i < contentPages.Length; i++)
        {
            if (contentPages[i] != null)
            {
                contentPages[i].SetActive(i == currentPageIndex);
            }
        }

        // 更新页码显示
        if (pageNumberText != null)
        {
            pageNumberText.text = $"第 {currentPageIndex + 1} 页 / 共 {contentPages.Length} 页";
        }

        // 更新翻页按钮状态
        if (prevPageButton != null)
        {
            prevPageButton.interactable = currentPageIndex > 0;
        }

        if (nextPageButton != null)
        {
            nextPageButton.interactable = currentPageIndex < contentPages.Length - 1;
        }
    }
}