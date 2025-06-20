// FixedAspectCamera.cs → 메인카메라 + UI카메라 둘 다 처리
using UnityEngine;


#if UNITY_ANDROID || UNITY_IOS
public class FixedAspectCamera : MonoBehaviour
{
    public float targetAspect = 9f / 16f;
    public Camera uiCamera; // UI 카메라도 받아오기

    void Start()
    {
        Camera cam = GetComponent<Camera>();
        float screenAspect = (float)Screen.width / Screen.height;
        float scale = screenAspect / targetAspect;

        Rect rect;
        if (scale < 1f)
        {
            float height = scale;
            float yOffset = (1f - height) / 2f;
            rect = new Rect(0f, yOffset, 1f, height);
        }
        else
        {
            float width = 1f / scale;
            float xOffset = (1f - width) / 2f;
            rect = new Rect(xOffset, 0f, width, 1f);
        }

        cam.rect = rect;
        if (uiCamera != null)
            uiCamera.rect = rect;
    }
}
#endif