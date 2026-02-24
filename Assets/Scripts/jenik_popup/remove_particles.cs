using System.Collections;
using UnityEngine;

public class remove_particles : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        StartCoroutine(WaitForSeconds());
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    IEnumerator WaitForSeconds()
    {
        yield return new WaitForSeconds(3f);
        Destroy(gameObject);
    }
}
