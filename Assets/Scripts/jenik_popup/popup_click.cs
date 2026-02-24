using System.Collections;
using UnityEngine;

public class popup_click : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public GameObject particles;
    public GameObject ukazatel;
    public Gradient timerGradient;

    private Vector2 mousePos;
    void Start()
    {
        StartCoroutine(WaitForSeconds());
    }

    // Update is called once per frame
    void Update()
    {
        mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        if(Input.GetMouseButtonDown(0))
        {
            RaycastHit2D hit = Physics2D.Raycast(mousePos, Vector2.zero);
            if (hit.collider != null && hit.collider.gameObject == gameObject)
            {
                StopAllCoroutines();
                GameObject particl = Instantiate(particles, transform.position, Quaternion.identity);
                particl.GetComponent<ParticleSystem>().startColor = Color.white;
                Destroy(gameObject);
            }
        }
    }
    IEnumerator WaitForSeconds()
    {
        for(int i = 0; i < 15; i++)
        {
            ukazatel.transform.localScale += new Vector3(0, -0.02f, 0);
            ukazatel.GetComponent<SpriteRenderer>().color = timerGradient.Evaluate((float)i/15f);
            yield return new WaitForSeconds(1f);
        }
        GameObject partikl = Instantiate(particles, transform.position, Quaternion.identity);
        partikl.GetComponent<ParticleSystem>().startColor = Color.black;
        Destroy(gameObject);
    }
}
