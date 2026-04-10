using UnityEngine;

public class HandAnchor : MonoBehaviour
{
    public Camera handCamera; // גררי לכאן את מצלמת היד
    [Range(0, 1)] public float horizontalAnchor = 0.65f; // 1 זה הכי ימין
    [Range(0, 1)] public float verticalAnchor = 0.7f;   // 0 זה הכי למטה
    public float distanceToCamera = 0.6f;                // כמה היד רחוקה מהעדשה

    void Update()
    {
        if (handCamera == null) return;

        // חישוב הנקודה היחסית במסך
        Vector3 viewportPoint = new Vector3(horizontalAnchor, verticalAnchor, distanceToCamera);
        
        // המרה למיקום בעולם ביחס למצלמה הספציפית הזו
        Vector3 worldPos = handCamera.ViewportToWorldPoint(viewportPoint);
        
        transform.position = worldPos;

        // שמירה על רוטציה יחסית למצלמה (שהיד תמיד תפנה קדימה מהמבט)
transform.localEulerAngles = new Vector3(0, 0, 0);    }
}