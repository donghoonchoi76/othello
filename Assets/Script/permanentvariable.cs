using UnityEngine;
using System.Collections;

public static class permanentvariable {
    public static readonly float _displayWidth = 4.8f;
    public static readonly float _displayHeight = 8.0f;
    public static readonly float _stoneSize = 1.2f;

    public static UnityEngine.Rect rtCameraBorder;

	public static int[] MAX_DEPTH = new int[] { 1, 7, 9 };
	public static int MAX_DEPTH_IDX = 0;
    
	public static void ToggleVisibility(Transform obj, bool bShow)
    {
        if (obj.renderer != null)
            obj.renderer.enabled = bShow;

        if (obj.guiTexture != null)
            obj.guiTexture.enabled = bShow;

        if (obj.guiText != null)
            obj.guiText.enabled = bShow;

        for (int i = 0; i < obj.childCount; i++)
        {
            if (obj.GetChild(i).renderer != null)
                obj.GetChild(i).renderer.enabled = bShow;

            if (obj.GetChild(i).guiTexture != null)
                obj.GetChild(i).guiTexture.enabled = bShow;

            if (obj.GetChild(i).guiText != null)
                obj.GetChild(i).guiText.enabled = bShow;
            
            if (obj.GetChild(i).childCount > 0)
            {
                ToggleVisibility(obj.GetChild(i), bShow);
            }
        }
    }

    
	
};
