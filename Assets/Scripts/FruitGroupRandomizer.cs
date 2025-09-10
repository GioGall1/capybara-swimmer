using UnityEngine;
using System.Collections.Generic;

public class FruitGroupRandomizer : MonoBehaviour
{
    public List<GameObject> diagonalGroups;
    public List<GameObject> verticalGroups;

    [Range(0, 3)] public int minDiagonals = 1;
    [Range(0, 3)] public int maxDiagonals = 2;

    [Range(0, 3)] public int minVerticals = 1;
    [Range(0, 3)] public int maxVerticals = 2;

    void Start()
    {
        ActivateRandomGroups(diagonalGroups, minDiagonals, maxDiagonals);
        ActivateRandomGroups(verticalGroups, minVerticals, maxVerticals);
    }

    void ActivateRandomGroups(List<GameObject> groups, int min, int max)
    {
        // Сначала выключим все
        foreach (var group in groups)
            group.SetActive(false);

        // Случайное число групп для активации
        int countToActivate = Random.Range(min, max + 1);

        // Перемешаем список
        List<GameObject> shuffled = new List<GameObject>(groups);
        for (int i = 0; i < shuffled.Count; i++)
        {
            int randIndex = Random.Range(i, shuffled.Count);
            var temp = shuffled[i];
            shuffled[i] = shuffled[randIndex];
            shuffled[randIndex] = temp;
        }

        // Активируем выбранные
        for (int i = 0; i < Mathf.Min(countToActivate, shuffled.Count); i++)
            shuffled[i].SetActive(true);
    }
}