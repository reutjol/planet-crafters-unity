# Unity Inspector Setup Guide
## שינויים נדרשים באינספקטור לאחר הניקיון והאופטימיזציה

---

## 📋 סיכום השינויים שבוצעו

### קבצים שנוצרו:
1. ✅ `Assets/Scripts/Core/GameConfig.cs` - ScriptableObject חדש להגדרות משחק
2. ✅ `Assets/Scripts/Networking/BaseApiClient.cs` - מחלקת בסיס לכל ה-API Clients

### קבצים שנמחקו:
1. ❌ `Assets/Scripts/Data/DTO/PlanetStageStateDto.cs` - מוזג ל-PlanetDtos.cs
2. ❌ `Assets/Resources/Models/test planet12-16.fbx` - קבצי test מיותרים

### תיקיות ששמן שונה:
1. 📁 `Assets/Resources/Prefarbs/` → `Assets/Resources/Prefabs/`

### קבצים שעודכנו:
1. 🔄 `Assets/Scripts/Data/DTO/PlanetDtos.cs` - הורחב לכלול את כל ה-DTOs
2. 🔄 `Assets/Scripts/Networking/AuthApiClient.cs` - משתמש ב-BaseApiClient
3. 🔄 `Assets/Scripts/Networking/PlanetApiClient.cs` - משתמש ב-BaseApiClient
4. 🔄 `Assets/Scripts/Networking/PlanetStateApiClient.cs` - משתמש ב-BaseApiClient
5. 🔄 `Assets/Scripts/Networking/StageStateApiClient.cs` - משתמש ב-BaseApiClient

---

## 🔧 שינויים נדרשים באינספקטור של Unity

### שלב 1: יצירת GameConfig Asset

1. **פתחי את Unity Editor**

2. **צרי תיקייה חדשה** (אם לא קיימת):
   - `Assets/Resources/`

3. **צרי את ה-GameConfig asset**:
   - לחצי לחצן ימני על `Assets/Resources/`
   - בחרי: `Create > Game > Game Config`
   - שמי את הקובץ: `GameConfig`

4. **הגדירי את ה-GameConfig** (אופציונלי - יש ערכי ברירת מחדל):
   - בחרי את `GameConfig.asset` שיצרת
   - באינספקטור תראי את כל ההגדרות:
     - **Server Base URL**: `https://planet-crafters-server.onrender.com` (כבר מוגדר)
     - **Scene Indices**: 0-7 (כבר מוגדרים)
     - **Gameplay Settings**: ערכי ברירת מחדל (30 deck size, 3 hand size, וכו')

5. **שמרי את הפרויקט** (Ctrl+S)

---

### שלב 2: עדכון API Clients בסצנות

Unity צריך לטעון מחדש את ה-meta files של הקבצים שהשתנו. אם יש שגיאות קומפילציה:

1. **פתחי את Unity ותני לו לקמפל מחדש**
2. **אם יש שגיאות**:
   - סגרי את Unity
   - מחקי את תיקיית `Library/` (Unity תיצור אותה מחדש)
   - פתחי את Unity שוב

---

### שלב 3: בדיקת GameObjects בסצנות

עברי על הסצנות הבאות ובדקי שה-API Clients עובדים:

#### **BootScene** (`Assets/Scenes/BootScene.unity`):
1. פתחי את הסצנה
2. מצאי את ה-GameObject עם `AuthApiClient`
3. באינספקטור תראי שדה חדש: **Game Config** (SerializeField)
4. אם השדה ריק - זה בסדר! הקוד יטען אותו אוטומטית מ-Resources
5. **אופציונלי**: אפשר לגרור את `GameConfig.asset` לשדה הזה

חזרי על זה עבור:
- `PlanetApiClient` (אם קיים בסצנה)
- `PlanetStateApiClient` (אם קיים בסצנה)
- `StageStateApiClient` (אם קיים בסצנה)

#### **סצנות נוספות לבדיקה**:
- `SignInScene.unity` - בדקי `AuthApiClient`
- `SignUpScene.unity` - בדקי `AuthApiClient`
- `PlanetScene.unity` - בדקי `PlanetApiClient`
- `GamePlayScene.unity` - בדקי `PlanetStateApiClient`, `StageStateApiClient`

---

### שלב 4: בדיקת Prefabs שהועברו

1. **פתחי את** `Assets/Resources/Prefabs/`
2. **ודאי שכל ה-Prefabs שם**:
   - `planets/` - 8-9 planet prefabs
   - `Hextile.prefab`
   - `HexOpen.prefab`, `HexClosed.prefab`
   - `Tile.prefab`
   - `Button signUp.prefab`
   - אנימציות

3. **אם יש missing references**:
   - פתחי כל Prefab
   - תקני את ה-references הפגומים

---

## 🧪 בדיקות שצריך לבצע

### בדיקה 1: GameConfig נטען כראוי
1. הפעילי את המשחק
2. פתחי את Console
3. חפשי אזהרה: `"GameConfig not found in Resources"`
   - **אם יש אזהרה**: GameConfig לא נמצא ב-Resources, צרי אותו (שלב 1)
   - **אם אין אזהרה**: הכל עובד מצוין!

### בדיקה 2: API Calls עובדים
1. הפעילי את המשחק
2. נסי להתחבר (Sign In)
3. בדקי ב-Console שאין שגיאות חדשות
4. הקריאות ל-API צריכות להופיע כך:
   ```
   [AuthApiClient] POST /api/auth succeeded (200)
   [PlanetApiClient] GET /api/planets/active succeeded (200)
   ```

### בדיקה 3: Prefabs עובדים
1. פתחי את `PlanetScene`
2. הפעילי Play Mode
3. ודאי שה-Planets מופיעים (נטענים מ-Prefabs)
4. אם יש שגיאות של missing prefabs - תקני את הנתיבים

---

## 🚨 פתרון בעיות

### בעיה: "GameConfig not found in Resources"
**פתרון**:
- ודאי ש-GameConfig.asset נמצא ב-`Assets/Resources/GameConfig.asset`
- אם הוא במיקום אחר, העברי אותו לשם
- או: גררי את ה-asset ידנית לשדה "Game Config" בכל API Client

### בעיה: "NullReferenceException in BaseApiClient"
**פתרון**:
- ה-GameConfig לא נטען - עיני בפתרון למעלה
- או: לחצי על כל API Client באינספקטור וגררי את GameConfig.asset לשדה

### בעיה: "Missing references in Prefabs"
**פתרון**:
- Unity לא עדכן את ה-GUIDs לאחר שינוי שם התיקייה
- פתחי כל Prefab שבור ותקני את ה-references ידנית
- או: החזרי את שם התיקייה ל-Prefarbs, אז Unity יתקן אוטומטית, ואז שני את השם שוב

### בעיה: קומפילציה נכשלת
**פתרון**:
1. סגרי את Unity
2. מחקי את `Library/ScriptAssemblies/`
3. פתחי את Unity שוב
4. אם עדיין לא עובד - מחקי את כל `Library/`

---

## 📊 סטטוס הקוד

### ✅ מה עובד עכשיו:
- כל ה-API Clients משתמשים במחלקת בסיס משותפת (BaseApiClient)
- אין יותר כפילויות בקוד
- ה-URL של השרת מרוכז ב-GameConfig
- DTOs מאוחדים בקובץ אחד
- תיקיות ומודלים מנוקים

### ⚠️ מה עלול להישבר:
- אם GameConfig לא נטען, ה-API Clients ישתמשו ב-URL קשיח (fallback)
- Prefabs שהיו תחת `Prefarbs/` עשויים להיות missing עד שUnity יטען מחדש

---

## 📝 הערות נוספות

1. **לא צריך לשנות קוד** - כל השינויים כבר בוצעו
2. **אם משהו לא עובד** - הקוד נשאר backwards compatible עם fallback ל-URLs קשיחים
3. **אחרי הבדיקות** - אפשר למחוק את הקובץ הזה

---

**תאריך**: 2026-01-19
**גרסה**: 1.0
**סטטוס**: ✅ מוכן לשימוש
