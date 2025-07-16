using UnityEngine;

// Bu script, eklendiği herhangi bir nesneyi kendi etrafında döndürür.
public class Rotator : MonoBehaviour
{
    // Hızı Inspector'dan ayarlayabilmek için public bir değişken.
    public float rotationSpeed = 50f;

    void Update()
    {
        // Nesneyi Z ekseni etrafında, belirtilen hızda döndür.
        transform.Rotate(0f, 0f, rotationSpeed * Time.deltaTime);
    }
}