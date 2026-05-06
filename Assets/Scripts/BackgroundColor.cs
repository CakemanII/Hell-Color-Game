using UnityEngine;
using System.Collections;

public class BackgroundColor : MonoBehaviour
{

    public Color[] colors;
    
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Camera.main.backgroundColor = colors[Random.Range(0, colors.Length)];
    }

    // Update is called once per frame
    void Update()
    {
        Camera.main.backgroundColor = colors[Random.Range(0, colors.Length)];
    }

    /*IEnumerator Fade()
    {

    }*/
}
