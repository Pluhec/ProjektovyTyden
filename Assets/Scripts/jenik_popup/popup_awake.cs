using UnityEngine;
using UnityEngine.U2D;

public class popup_awake : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if(transform.GetComponent<SpriteRenderer>()!= null)
        {if (transform.GetComponent<SpriteRenderer>().color.a < 1)
            {
                transform.GetComponent<SpriteRenderer>().color += new Color(0, 0, 0, Time.deltaTime);
            }
        }
        else
        {
            if (transform.GetComponent<SpriteShapeRenderer>().color.a < 1)
            {
                transform.GetComponent<SpriteShapeRenderer>().color += new Color(0, 0, 0, Time.deltaTime);
            }
        }
    }
}
