using UnityEngine;

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
        
    }
}
