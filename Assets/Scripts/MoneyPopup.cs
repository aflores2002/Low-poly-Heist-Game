using UnityEngine;
using TMPro;

public class MoneyPopup : MonoBehaviour
{
    public TextMeshProUGUI text;
    public float lifetime = 1.5f;
    public float floatSpeed = 20f;

    private float timer;

    void Start()
    {
        timer = lifetime;
    }

    void Update()
    {
        transform.Translate(Vector3.up * floatSpeed * Time.deltaTime);
        timer -= Time.deltaTime;
        if (timer <= 0f)
        {
            Destroy(gameObject);
        }
    }

    public void SetAmount(int amount)
    {
        text.text = $"+${amount:N0}";
    }
}
