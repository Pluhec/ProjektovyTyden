using UnityEngine;

public class popup_spawn : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public GameObject popup_prefab;

    public void spawn(int x_pozice, int y_pozice)
    {
        GameObject popup = Instantiate(popup_prefab, new Vector3(x_pozice, y_pozice, 0), Quaternion.identity);
    }
}
