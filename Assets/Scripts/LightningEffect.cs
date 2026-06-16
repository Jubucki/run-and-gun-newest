using UnityEngine;

public class LightningEffect : MonoBehaviour
{
    public Camera mainCamera;
    public Color flashColor = new Color(0.7f, 0.8f, 1f, 1f);
    
    float nextFlash;
    bool isFlashing;
    float flashTimer;
    float flashDuration;
    Color originalColor;

    void Start()
    {
        originalColor = mainCamera.backgroundColor;
        ScheduleNextFlash();
    }

    void Update()
    {
        if (isFlashing)
        {
            flashTimer -= Time.deltaTime;
            
            // Doppelblitz-Effekt
            float t = flashTimer / flashDuration;
            mainCamera.backgroundColor = Color.Lerp(originalColor, flashColor, t);
            
            if (flashTimer <= 0)
            {
                mainCamera.backgroundColor = originalColor;
                isFlashing = false;
                ScheduleNextFlash();
            }
        }
        else
        {
            nextFlash -= Time.deltaTime;
            if (nextFlash <= 0)
                DoFlash();
        }
    }

    void DoFlash()
    {
        isFlashing = true;
        flashDuration = Random.Range(0.05f, 0.15f);
        flashTimer = flashDuration;
        
        // Manchmal Doppelblitz
        if (Random.value > 0.5f)
            Invoke(nameof(DoFlash), Random.Range(0.1f, 0.3f));
    }

    void ScheduleNextFlash()
    {
        nextFlash = Random.Range(2f, 8f);
    }
}
