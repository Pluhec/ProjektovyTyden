using UnityEngine;

public class Hover_cursor : MonoBehaviour
{
    public enum cursor_type
    { 
        default_cursor,
        hover_cursor,
        open_cursor,
        mechanic_cursor,
        lupa_cursor
    }

    public cursor_type typ_cursoru;
    private SpriteRenderer spriteRenderer;
    private bool jsem_on = false;

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    void Update()
    {
        // 1. Pøevod pozice myši z obrazovky (pixely) do herního svìta
        Vector2 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);

        // 2. Kontrola, jestli je myš uvnitø "obdélníku" spritu
        if(spriteRenderer != null)
        {if (spriteRenderer.bounds.Contains(mouseWorldPos))
            {
                jsem_on = true;
                int nastav_cursor;
                switch (typ_cursoru)
                {
                    case cursor_type.hover_cursor:
                        nastav_cursor = 1;
                        break;
                    case cursor_type.open_cursor:
                        nastav_cursor = 2;
                        break;
                    case cursor_type.mechanic_cursor:
                        nastav_cursor = 3;
                        break;
                    case cursor_type.lupa_cursor:
                        nastav_cursor = 4;
                        break;
                    default:
                        nastav_cursor = 0;
                        break;
                }
                zmena_cursor.SetCursor(nastav_cursor);
            }
            else
            {
                if(jsem_on)
                {   zmena_cursor.SetCursor(0);
                    jsem_on = false;
                }
            }
        }else if (transform.GetComponent<RectTransform>() != null)
        {
            // Doplnìná èást pro UI
            if (RectTransformUtility.RectangleContainsScreenPoint(GetComponent<RectTransform>(), Input.mousePosition, null))
            {
                jsem_on = true;
                int nastav_cursor;
                switch (typ_cursoru)
                {
                    case cursor_type.hover_cursor:
                        nastav_cursor = 1;
                        break;
                    case cursor_type.open_cursor:
                        nastav_cursor = 2;
                        break;
                    case cursor_type.mechanic_cursor:
                        nastav_cursor = 3;
                        break;
                    case cursor_type.lupa_cursor:
                        nastav_cursor = 4;
                        break;
                    default:
                        nastav_cursor = 0;
                        break;
                }
                zmena_cursor.SetCursor(nastav_cursor);
            }
            else
            {
                if (jsem_on)
                {
                    zmena_cursor.SetCursor(0);
                    jsem_on = false;
                }
            }
        }
    }
}