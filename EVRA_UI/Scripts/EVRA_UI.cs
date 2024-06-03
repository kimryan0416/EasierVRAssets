using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EVRA_UI : MonoBehaviour
{
    public EVRA_UI_Selector[] selectors;

    public Vector2 ui_area = Vector2.one;
    public Color ui_area_color = Color.yellow;

    private void OnDrawGizmos() {
        if (Application.isPlaying) return;
        // Draw a box to represent the UI area based on 
        Gizmos.color = ui_area_color;
        Gizmos.DrawWireCube(transform.position, new Vector3(ui_area.x, ui_area.y, 0f));
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
