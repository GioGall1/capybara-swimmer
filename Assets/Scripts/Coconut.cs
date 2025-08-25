using UnityEngine;

public class Coconut : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            var swimController = other.GetComponent<CapybaraSwimController>();
            if (swimController != null)
            {
                swimController.ApplyCoconutBoost();
            }

            Destroy(gameObject);
        }
    }
}