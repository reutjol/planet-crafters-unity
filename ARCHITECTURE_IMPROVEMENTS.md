# Architecture Improvements - Game State Management
## שינויים בארכיטקטורה של ניהול המשחק

תאריך: 2026-01-19

---

## 🎯 הבעיה שנפתרה

לפני התיקון היו **שתי מערכות נפרדות** שניהלו את ה-State:

### ❌ לפני (Before):
```
BootScene
  └── GameManager
       └── משתמש ב-StageStateApiClient
       └── טוען StageStateDto
       └── שומר ב-AppSession

GamePlayScene
  └── GameBootstrap
       └── משתמש ב-PlanetStateApiClient
       └── טוען PlanetStageStateDto שוב!
       └── לא יודע מה GameManager טען
```

**בעיות**:
1. ✗ Stage State נטען **פעמיים** (duplicate API calls)
2. ✗ שני API Clients שונים לאותה מטרה
3. ✗ אין סנכרון בין GameManager ל-GameBootstrap
4. ✗ בזבוז זמן וcalls לשרת
5. ✗ קשה לתחזק

---

## ✅ הפתרון החדש

### ✅ אחרי (After):
```
BootScene
  └── GameManager (Singleton - נשאר לכל המשחק)
       ├── משתמש רק ב-PlanetStateApiClient
       ├── טוען PlanetStageStateDto פעם אחת
       ├── שומר ב-cache פנימי
       └── מספק API מרכזי לכל המערכת

GamePlayScene
  └── GameBootstrap
       ├── מקבל State מ-GameManager (מהcache)
       ├── לא עושה API calls ישירות
       └── רק מאתחל את ה-UI עם הנתונים

StageStateAutoSave
  └── משתמש ב-GameManager.SavePlanetStageState()

RestartStageButton
  └── משתמש ב-GameManager.ResetCurrentStage()
```

**יתרונות**:
1. ✓ State נטען **פעם אחת** בלבד
2. ✓ API Client אחד מרכזי (PlanetStateApiClient)
3. ✓ GameManager הוא Single Source of Truth
4. ✓ Cache מונע טעינות מיותרות
5. ✓ קל לתחזק ולהבין

---

## 📋 שינויים בקבצים

### 1. GameManager.cs - הורחב משמעותית

**לפני**: 152 שורות
**אחרי**: 329 שורות

#### תוספות חדשות:

```csharp
// Cache למניעת טעינות כפולות
private PlanetStageStateDto currentPlanetStageState;
private string currentStageId;

// API מרכזי לטעינת Stage State
public void RequestPlanetStageState(bool forceRefresh = false)

// API לשמירת State
public void SavePlanetStageState(SaveStageStateRequestDto stateDto, ...)

// API לאיפוס Stage
public void ResetCurrentStage(Action onSuccess = null, ...)

// קבלת State מה-cache
public PlanetStageStateDto GetCachedPlanetStageState()

// ניקוי Cache
public void ClearCache()
```

#### שינויים:
- ✅ הוחלף `StageStateApiClient` ב-`PlanetStateApiClient`
- ✅ נוסף מנגנון Cache חכם
- ✅ כל הפעולות עוברות דרך GameManager
- ✅ Event חדש: `OnPlanetStageStateLoaded`

---

### 2. GameBootstrap.cs - שונה לחלוטין

**לפני**: 82 שורות (קורא ישירות מה-API)
**אחרי**: 160 שורות (מקבל מ-GameManager)

#### שינויים מרכזיים:

```csharp
// ✅ לא צריך יותר PlanetStateApiClient
// ✅ לא צריך deckSize

// תהליך חדש:
1. בדיקה אם יש cache ב-GameManager
2. אם יש cache → שימוש ישיר (מהיר!)
3. אם אין cache → בקשה מ-GameManager
4. המתנה לטעינה עם timeout
5. אתחול ה-UI עם ה-State
```

**יתרון עצום**: אם משתמש חוזר למשחק, ה-State כבר טעון!

---

### 3. StageStateAutoSave.cs - משתמש ב-GameManager

**לפני**: שמר ישירות דרך `PlanetStateApiClient`
**אחרי**: שומר דרך `GameManager.SavePlanetStageState()`

```csharp
// ✅ לא צריך יותר PlanetStateApiClient בשדה
// ✅ משתמש ב-GameManager.Instance

GameManager.Instance.SavePlanetStageState(
    stateDto,
    onSuccess: () => { ... },
    onError: (err) => { ... }
);
```

**יתרון**: כל השמירות מנוהלות במקום אחד, קל לעקוב ולדבג.

---

### 4. RestartStageButton.cs - משתמש ב-GameManager

**לפני**: איפס ישירות דרך `PlanetStateApiClient`
**אחרי**: מאפס דרך `GameManager.ResetCurrentStage()`

```csharp
// ✅ לא צריך יותר PlanetStateApiClient בשדה
// ✅ משתמש ב-GameManager.Instance

GameManager.Instance.ResetCurrentStage(
    onSuccess: () => { ... },
    onError: (err) => { ... }
);
```

---

## 🔄 זרימת הנתונים החדשה

### טעינת משחק (Stage Loading):

```
1. משתמש בוחר Stage במפה
   └→ MapStageController קורא ל-GameManager.SelectStage(stageId)

2. GameManager מנקה cache ישן
   └→ currentStageId = null

3. משתמש עובר ל-GamePlayScene

4. GameBootstrap.Start() מתחיל:
   └→ בודק אם יש cached state ב-GameManager
       ├─ אם כן: משתמש בו מיד (מהיר!)
       └─ אם לא: קורא ל-GameManager.RequestPlanetStageState()
           └→ GameManager טוען מהשרת
           └→ שומר ב-cache
           └→ מפעיל event OnPlanetStageStateLoaded
           └→ GameBootstrap מקבל את ה-State

5. GameBootstrap מאתחל את ה-UI:
   └→ MapController.LoadPlacedTilesFromServer()
   └→ HandController.LoadFromServer()
   └→ StageStateAutoSave.SetReady(true)
```

### שמירה אוטומטית (Auto-Save):

```
1. משתמש מניח tile על המפה
   └→ MapController.OnMapStateChanged event

2. StageStateAutoSave מקבל את ה-event
   └→ מתחיל debounce timer (0.25 שניות)

3. אחרי debounce:
   └→ בונה SaveStageStateRequestDto
   └→ קורא ל-GameManager.SavePlanetStageState()
       └→ GameManager שולח ל-PlanetStateApiClient
           └→ PlanetStateApiClient שולח PUT request לשרת
```

### איפוס Stage (Reset):

```
1. משתמש לוחץ על כפתור Restart
   └→ RestartStageButton.OnRestartClicked()

2. קורא ל-GameManager.ResetCurrentStage()
   └→ GameManager שולח POST request לשרת
   └→ מנקה cache (currentStageId = null)

3. SceneLoader טוען מחדש את GamePlayScene

4. GameBootstrap רואה שאין cache
   └→ טוען State חדש מהשרת
   └→ Stage מתחיל מהתחלה
```

---

## 🎁 יתרונות נוספים

### 1. קל להוסיף Multiplayer בעתיד
עכשיו כל ה-State עובר דרך GameManager, אז קל להוסיף:
- Real-time sync
- State notifications
- Conflict resolution

### 2. קל לדבג
- כל API call עובר דרך GameManager
- Logs מרכזיים עם `[GameManager]` prefix
- קל לעקוב אחרי זרימת הנתונים

### 3. Performance
- Cache מונע API calls מיותרים
- אם חוזרים לאותו Stage - טעינה מיידית
- פחות עומס על השרת

### 4. Testability
- GameManager יכול להיות mocked בקלות
- כל הלוגיקה במקום אחד
- קל לכתוב unit tests

---

## 🚨 שינויים נדרשים באינספקטור

### GameManager (BootScene):

```
לפני:
  - planetApi: PlanetApiClient
  - stageStateApi: StageStateApiClient ❌

אחרי:
  - planetApi: PlanetApiClient
  - planetStateApi: PlanetStateApiClient ✅
```

**פעולה נדרשת**:
1. פתחי את BootScene
2. בחרי את GameManager GameObject
3. במקום StageStateApiClient, גררי את PlanetStateApiClient

---

### GameBootstrap (GamePlayScene):

```
לפני:
  - planetStateApi: PlanetStateApiClient ❌
  - deckSize: 30 ❌

אחרי:
  (שדות אלו הוסרו - לא נדרשים יותר!)
```

**פעולה נדרשת**:
1. פתחי את GamePlayScene
2. בחרי את GameBootstrap GameObject
3. תראי ש-`planetStateApi` ו-`deckSize` **לא מופיעים יותר** - זה תקין!

---

### StageStateAutoSave (GamePlayScene):

```
לפני:
  - planetStateApi: PlanetStateApiClient ❌

אחרי:
  (שדה זה הוסר - לא נדרש יותר!)
```

**פעולה נדרשת**: אין - השדה יעלם אוטומטית

---

### RestartStageButton (GamePlayScene):

```
לפני:
  - planetStateApi: PlanetStateApiClient ❌

אחרי:
  (שדה זה הוסר - לא נדרש יותר!)
```

**פעולה נדרשת**: אין - השדה יעלם אוטומטית

---

## 📊 סטטיסטיקות

| מדד | לפני | אחרי | שיפור |
|-----|------|------|-------|
| API Calls בטעינת Stage | 2 | 1 | 50% פחות |
| Files עם API Client references | 4 | 1 | 75% פחות |
| Coupling בין Scenes | גבוה | נמוך | ✓ |
| Single Source of Truth | ✗ | ✓ GameManager | ✓ |
| Cache support | ✗ | ✓ | ✓ |

---

## 🧪 איך לבדוק שהכל עובד

### Test 1: טעינת Stage רגילה
1. הפעילי את המשחק
2. התחברי
3. בחרי Planet
4. בחרי Stage
5. **בדקי ב-Console**:
   ```
   [GameManager] Loading PlanetStageState for planet=..., stage=...
   [GameManager] PlanetStageState loaded successfully
   [GameBootstrap] Using cached state from GameManager
   [GameBootstrap] Gameplay initialized successfully
   ```

### Test 2: Auto-Save
1. בתוך המשחק, הנחי tile
2. **בדקי ב-Console** (אחרי 0.25 שניות):
   ```
   [AutoSave] Saving state...
   [GameManager] Stage state saved successfully
   [AutoSave] State saved successfully
   ```

### Test 3: Reset Stage
1. לחצי על כפתור Restart
2. **בדקי ב-Console**:
   ```
   [Restart] Resetting stage...
   [GameManager] Stage reset successfully
   [Restart] Stage reset successfully, reloading scene...
   ```

---

## 🎓 מה למדנו

1. **Centralized State Management** - כל ה-State במקום אחד
2. **Single Responsibility** - כל מחלקה עושה דבר אחד
3. **Caching Strategy** - שמירת נתונים למניעת טעינות מיותרות
4. **Event-Driven Architecture** - תקשורת דרך Events
5. **Separation of Concerns** - UI מופרד מ-Business Logic

---

## 💡 המלצות לעתיד

### 1. הוספת State Versioning
```csharp
public class PlanetStageStateDto
{
    public int version; // למניעת overwrites
    public long timestamp;
    // ...
}
```

### 2. Offline Support
```csharp
// שמירה מקומית ל-fallback
PlayerPrefs.SetString("last_stage_state", JsonUtility.ToJson(state));
```

### 3. State History (Undo/Redo)
```csharp
private Stack<PlanetStageStateDto> stateHistory;
public void Undo() { ... }
```

---

**סיכום**: עכשיו המשחק מנוהל בצורה מסודרת, ממוקדת, ויעילה. GameManager הוא המנהל המרכזי והיחיד, וכל השאר פשוט משתמשים בו! 🎉

