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
        ActivateRandomGroups(diagonalGroups, minDiagonals, maxDiagonals, isDiagonal: true);
        ActivateRandomGroups(verticalGroups, minVerticals, maxVerticals, isDiagonal: false);
    }

    void ActivateRandomGroups(List<GameObject> groups, int min, int max, bool isDiagonal)
    {
        foreach (var group in groups)
            group.SetActive(false);

        int countToActivate = Random.Range(min, max + 1);

        List<GameObject> shuffled = new List<GameObject>(groups);
        for (int i = 0; i < shuffled.Count; i++)
        {
            int randIndex = Random.Range(i, shuffled.Count);
            var temp = shuffled[i];
            shuffled[i] = shuffled[randIndex];
            shuffled[randIndex] = temp;
        }

        for (int i = 0; i < Mathf.Min(countToActivate, shuffled.Count); i++)
        {
            GameObject group = shuffled[i];
            group.SetActive(true);

            if (isDiagonal)
            {
                // Рандомно решаем, зеркалить или нет
                bool shouldFlip = Random.value > 0.5f;

                // Угол поворота по Y — можно изменить вручную, например 180f для зеркального вида
                float diagonalYRotation = shouldFlip ? 180f : 0f;

                // Сброс поворота и установка нового
                float originalZRotation = group.transform.localEulerAngles.z;
                group.transform.localRotation = Quaternion.Euler(0f, diagonalYRotation, originalZRotation);
            }
        }
    }
}