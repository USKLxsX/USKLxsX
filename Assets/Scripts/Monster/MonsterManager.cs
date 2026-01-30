using UnityEngine;
using System.Collections.Generic;

public class MonsterManager : MonoBehaviour
{
    private static MonsterManager _instance;
    public static MonsterManager Instance => _instance;

    [Header("所有怪物数据")]
    public List<CreateMonster> allMonsters = new List<CreateMonster>();

    void Awake()
    {
        if (_instance != null && _instance != this)
            Destroy(gameObject);
        else
        {
            _instance = this;
            DontDestroyOnLoad(gameObject);
        }
    }

    public CreateMonster GetMonsterByIndex(int index)
    {
        if (index >= 0 && index < allMonsters.Count)
            return allMonsters[index];
        return null;
    }

    public CreateMonster GetMonsterByName(string name)
    {
        foreach (CreateMonster monster in allMonsters)
        {
            if (monster.monstername == name)
                return monster;
        }
        return null;
    }
}