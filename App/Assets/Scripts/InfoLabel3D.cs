using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InfoLabel3D : MonoBehaviour
{
    [Header("Label Settings")]
    public float fadeInDuration = 0.3f;
    public float fadeOutDuration = 0.3f;
    public float floatHeight = 0.1f;
    public float floatSpeed = 1f;
    
    private TextMesh textMesh;
    private Vector3 originalPosition;
    private bool isFloating = true;
    
    void Start()
    {
        textMesh = GetComponent<TextMesh>();
        originalPosition = transform.position;
        
        // Set default text properties
        if (textMesh != null)
        {
            textMesh.fontSize = 20;
            textMesh.anchor = TextAnchor.MiddleCenter;
            textMesh.alignment = TextAlignment.Center;
        }
    }
    
    void Update()
    {
        // Gentle floating animation
        if (isFloating)
        {
            float newY = originalPosition.y + Mathf.Sin(Time.time * floatSpeed) * floatHeight;
            transform.position = new Vector3(originalPosition.x, newY, originalPosition.z);
        }
        
        // Always face the camera
        if (Camera.main != null)
        {
            transform.LookAt(Camera.main.transform);
            transform.Rotate(0, 180, 0); // Flip to face camera properly
        }
    }
    
    public void SetText(string text)
    {
        if (textMesh != null)
        {
            textMesh.text = text;
        }
    }
    
    public void SetColor(Color color)
    {
        if (textMesh != null)
        {
            textMesh.color = color;
        }
    }
    
    public void StopFloating()
    {
        isFloating = false;
    }
    
    public void StartFloating()
    {
        isFloating = true;
        originalPosition = transform.position;
    }
}
