using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MaskManager : MonoBehaviour
{
    [Header("所有面具数据")]
    public List<CreateMask> allMasks = new List<CreateMask>();

    [Header("已解锁面具")]
    public List<CreateMask> unlockedMasks = new List<CreateMask>();

    [Header("当前佩戴面具")]
    public CreateMask currentMask;

    // 玩家引用（现在可以是任何类型）
    private GameObject _player;
    private GameObject player
    {
        get
        {
            if (_player == null)
            {
                _player = GameObject.FindWithTag("Player");
            }
            return _player;
        }
    }

    private static MaskManager _instance;
    public static MaskManager Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType<MaskManager>();
                if (_instance == null)
                {
                    GameObject obj = new GameObject("MaskManager");
                    _instance = obj.AddComponent<MaskManager>();
                }
            }
            return _instance;
        }
    }

    void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }

        _instance = this;
        DontDestroyOnLoad(gameObject);

        // 初始化
        LoadUnlockedMasks();
        if (unlockedMasks.Count == 0)
        {
            InitializeDefaultMasks();
        }
    }

    void InitializeDefaultMasks()
    {
        // 查找黑铁面具数据
        foreach (var mask in allMasks)
        {
            if (mask.maskName == CreateMask.MaskType.黑铁面具)
            {
                UnlockMask(mask);
                EquipMask(mask);
                break;
            }
        }
    }

    // 解锁新面具
    public void UnlockMask(CreateMask mask)
    {
        if (!unlockedMasks.Contains(mask))
        {
            unlockedMasks.Add(mask);
            Debug.Log($"解锁新面具: {mask.maskName}");

            // 触发解锁事件
            OnMaskUnlocked?.Invoke(mask);

            // 保存解锁进度
            SaveUnlockedMasks();
        }
    }

    // 佩戴面具
    public void EquipMask(CreateMask mask)
    {
        if (unlockedMasks.Contains(mask))
        {
            currentMask = mask;
            Debug.Log($"佩戴面具: {mask.maskName}");

            // 应用面具属性到玩家
            ApplyMaskEffects(mask);

            // 触发更换面具事件
            OnMaskEquipped?.Invoke(mask);
        }
    }

    // 应用面具效果到玩家
    private void ApplyMaskEffects(CreateMask mask)
    {
        if (player == null)
        {
            Debug.LogWarning("找不到玩家，无法应用面具效果");
            return;
        }

        // 这里可以根据你的实际玩家脚本结构来应用效果
        // 例如，如果玩家有 BasicControl 脚本：
        BasicControl playerControl = player.GetComponent<BasicControl>();
        if (playerControl != null)
        {
            // 应用攻击力加成
            // playerControl.attackDamage += mask.attackBonus;
            // 应用速度加成
            // playerControl.moveSpeed *= mask.speedMultiplier;
        }

        // 或者通过事件系统让玩家脚本自己处理
        // 这样就不需要知道具体的玩家脚本类型
    }

    // 根据名称获取面具
    public CreateMask GetMaskByName(CreateMask.MaskType maskType)
    {
        foreach (var mask in allMasks)
        {
            if (mask.maskName == maskType)
            {
                return mask;
            }
        }
        return null;
    }

    // 检查是否已解锁
    public bool IsMaskUnlocked(CreateMask.MaskType maskType)
    {
        foreach (var mask in unlockedMasks)
        {
            if (mask.maskName == maskType)
            {
                return true;
            }
        }
        return false;
    }

    // 获取所有已解锁面具的图标
    public List<Sprite> GetUnlockedMaskIcons()
    {
        List<Sprite> icons = new List<Sprite>();
        foreach (var mask in unlockedMasks)
        {
            if (mask.icon != null)
            {
                icons.Add(mask.icon);
            }
        }
        return icons;
    }

    // 事件
    public delegate void MaskUnlockedDelegate(CreateMask mask);
    public event MaskUnlockedDelegate OnMaskUnlocked;

    public delegate void MaskEquippedDelegate(CreateMask mask);
    public event MaskEquippedDelegate OnMaskEquipped;

    // 保存/加载解锁进度
    public void SaveUnlockedMasks()
    {
        List<string> unlockedNames = new List<string>();
        foreach (var mask in unlockedMasks)
        {
            unlockedNames.Add(mask.maskName.ToString());
        }

        string json = JsonUtility.ToJson(new StringListWrapper(unlockedNames));
        PlayerPrefs.SetString("UnlockedMasks", json);
        PlayerPrefs.Save();
    }

    public void LoadUnlockedMasks()
    {
        if (PlayerPrefs.HasKey("UnlockedMasks"))
        {
            string json = PlayerPrefs.GetString("UnlockedMasks");
            StringListWrapper wrapper = JsonUtility.FromJson<StringListWrapper>(json);

            unlockedMasks.Clear();
            foreach (string maskName in wrapper.items)
            {
                foreach (var mask in allMasks)
                {
                    if (mask.maskName.ToString() == maskName)
                    {
                        unlockedMasks.Add(mask);
                        break;
                    }
                }
            }
        }
    }

    // 编辑器工具：快速创建所有面具数据
    [ContextMenu("快速填充所有面具数据")]
    public void FillAllMaskData()
    {
        // 这里可以根据策划案快速填充数据
        // 需要你提供具体的数值
    }

    [System.Serializable]
    private class StringListWrapper
    {
        public List<string> items;
        public StringListWrapper(List<string> items) { this.items = items; }
    }
}