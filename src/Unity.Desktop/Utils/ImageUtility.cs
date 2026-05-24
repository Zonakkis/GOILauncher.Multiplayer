using UnityEngine;
using UnityEngine.UI;

public static class ImageUtility
{   
    public static void MakeTransparent(GameObject gameObject)
    {
        Image image = gameObject.GetComponent<Image>();
        if (image == null)
            return;

        image.color = Color.clear;
        image.raycastTarget = false;
    }
}