# Bugfix: תיקון שמות פרמטרים ב-API Clients

## הבעיה
לאחר עדכון BaseApiClient והAPI Clients, שיניתי את שמות הפרמטרים מ:
- `onOk` → `onSuccess`
- `onErr` → `onError`

אבל שכחתי לעדכן את הקבצים שמשתמשים ב-API Clients, מה שגרם לשגיאות קומפילציה.

## הפתרון

עדכנתי את כל הקבצים הבאים:

### 1. SignInController.cs
**לפני**:
```csharp
StartCoroutine(api.Login(email, pass,
    onOk: (resp) => { ... },
    onErr: (err) => Debug.LogError(err)
));
```

**אחרי**:
```csharp
StartCoroutine(api.Login(email, pass,
    onSuccess: (resp) => { ... },
    onError: (err) => Debug.LogError(err)
));
```

### 2. SignUpController.cs
**לפני**:
```csharp
StartCoroutine(api.Register(name, email, userName, pass,
    onOk: (resp) => { ... },
    onErr: (err) => Debug.LogError(err)
));
```

**אחרי**:
```csharp
StartCoroutine(api.Register(name, email, userName, pass,
    onSuccess: (resp) => { ... },
    onError: (err) => Debug.LogError(err)
));
```

### 3. GameManager.cs
**לפני**:
```csharp
yield return StartCoroutine(planetApi.GetActivePlanet(
    AppSession.Instance.AccessToken,
    onOk: (planet) => { ... },
    onErr: (err) => errMsg = err
));
```

**אחרי**:
```csharp
yield return StartCoroutine(planetApi.GetActivePlanet(
    AppSession.Instance.AccessToken,
    onSuccess: (planet) => { ... },
    onError: (err) => errMsg = err
));
```

## אימות
הרצתי חיפוש על כל הקבצים ב-Scripts:
- ✅ אין יותר שימוש ב-`onOk`
- ✅ אין יותר שימוש ב-`onErr`

כל הקוד עכשיו משתמש בשמות האחידים:
- ✅ `onSuccess`
- ✅ `onError`

## תאריך
2026-01-19
