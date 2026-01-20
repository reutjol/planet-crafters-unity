# Fix: API References בControllers

## הבעיה
SignInController ו-SignUpController צריכים reference ל-AuthApiClient, אבל הוא נמצא ב-BootScene עם DontDestroyOnLoad.

## הפתרון
עדכנתי את שני הControllers למצוא את ה-API אוטומטית אם הוא לא מוקצה באינספקטור.

## השינויים

### SignInController.cs & SignUpController.cs

**נוסף**:
```csharp
private void Start()
{
    // If api is not assigned in Inspector, try to find it
    if (api == null)
    {
        api = FindObjectOfType<AuthApiClient>();
        if (api == null)
        {
            Debug.LogError("AuthApiClient not found! Make sure BootScene is loaded first.");
        }
    }
}
```

**עכשיו זה עובד גם אם**:
1. AuthApiClient לא מוקצה באינספקטור
2. BootScene נטען ראשון (והוא אמור!)
3. AuthApiClient עם DontDestroyOnLoad נמצא אוטומטית

## איך לוודא שזה עובד

### אופציה 1: BootScene ראשון (מומלץ)
1. `File > Build Settings`
2. ודאי ש-BootScene הוא Index 0
3. SignInScene הוא Index 3
4. הכל יעבוד אוטומטית!

### אופציה 2: בדיקות ידניות
1. פתחי SignInScene
2. השדה `Api` יכול להישאר ריק
3. Run → הקוד ימצא את AuthApiClient אוטומטית
4. אם יש שגיאה - BootScene לא נטען ראשון

## תאריך
2026-01-19
