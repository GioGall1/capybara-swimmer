using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Banana : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player")) // Убедись, что у капибары стоит тег Player
        {
            // Добавляем очки
            ScoreManager.Instance.AddScore(10);
            
            // Уничтожаем банан
            Destroy(gameObject);
        }
    }
}