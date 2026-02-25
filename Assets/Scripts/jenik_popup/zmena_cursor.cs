using UnityEngine;

public class zmena_cursor : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public Texture2D[] sprites;
    public Texture2D[] clicked_scprites;
    public int cursor_on;
    public Vector2 hotSpot = Vector2.zero;

    public static zmena_cursor instance;

    void Awake()
    {
        if (instance == null)
        {
            // Pokud jsem první svého druhu, pøežiju a uložím se do instance
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            // Pokud už jeden takový existuje, tenhle nový se okamžitì smaže
            Destroy(gameObject);
        }
    }
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetMouseButton(0))
        {
            Cursor.SetCursor(clicked_scprites[cursor_on], hotSpot, CursorMode.Auto);
        }
        else
            { Cursor.SetCursor(sprites[cursor_on], hotSpot, CursorMode.Auto); }
    }
    public static void SetCursor(int cursorIndex)
    {
        zmena_cursor instance = FindAnyObjectByType<zmena_cursor>();
        instance.cursor_on = cursorIndex;
    }
    public static int seted_curzor()
    {
        zmena_cursor instance = FindAnyObjectByType<zmena_cursor>();
        return instance.cursor_on;
    }
}
