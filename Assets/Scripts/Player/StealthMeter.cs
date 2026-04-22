using UnityEngine;
using UnityEngine.UI;

public class StealthMeter : MonoBehaviour
{
    [Header("Stealth")]
    [SerializeField] private float maxStealth = 100f;
    [SerializeField] private float currentStealth;

    [Header("Rates")]
    [SerializeField] private float increaseRate = 60f;
    [SerializeField] private float decreaseRate = 15f;

    [Header("UI")]
    [SerializeField] private Slider stealthSlider;

    void Start()
    {
        currentStealth = 0f;
        if (stealthSlider == null)
        {
            stealthSlider.maxValue = maxStealth;
        }
    }

    void Update()
    {
        //passive decay when not detected
        currentStealth -= decreaseRate * Time.deltaTime;
        currentStealth = Mathf.Clamp(currentStealth, 0f, maxStealth);
        
        if (stealthSlider != null)
        {
             stealthSlider.value = Mathf.Lerp
             (
                stealthSlider.value,
                currentStealth,
                10f * Time.deltaTime
             );
        }
    }

    public void AddDetection(float amount)
    {
        currentStealth += amount;
        currentStealth = Mathf.Clamp(currentStealth, 0f, maxStealth);

        if (stealthSlider != null)
        {
            stealthSlider.value = currentStealth;
        }
    }

    public float GetStealth01()
    {
        return currentStealth / maxStealth;
    }

    public bool IsFullyDetected()
    {
        return currentStealth >= maxStealth;
    }
}