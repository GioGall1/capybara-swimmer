using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class DevTools : MonoBehaviour
{
    void Update()
    {
        HandleSceneRestart();
        // В будущем: HandleToggleUI(), HandleSpawnTestObject(), и т.д.
    }

    private void HandleSceneRestart()
    {
        if (Input.GetKeyDown(KeyCode.R))
        {
            Debug.Log("🔁 Перезапуск сцены...");
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
    }
}
