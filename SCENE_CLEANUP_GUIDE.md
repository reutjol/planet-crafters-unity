# Scene Cleanup Guide - הסרת כפילויות API Clients
## מדריך לניקוי הסצנות ביוניטי

---

## 🔴 הבעיה

יש **כפילויות** של API Clients במספר סצנות, מה שגורם ל:
- בזבוז זיכרון
- בלבול (איזה מופע בשימוש?)
- התנהגות לא צפויה

---

## ✅ הפתרון הנכון

**כלל זהב**: כל API Client צריך להיות **רק ב-BootScene** ולהישאר לכל המשחק עם `DontDestroyOnLoad`.

### המבנה הנכון:

```
BootScene (נטען ראשון, נשאר לכל המשחק):
  ├─ GameManager
  ├─ AppSession
  ├─ SceneLoader
  ├─ AuthApiClient          ← רק פעם אחת!
  ├─ PlanetApiClient        ← רק פעם אחת!
  └─ PlanetStateApiClient   ← רק פעם אחת!

כל הסצנות האחרות:
  ← לא צריכות API Clients! הן משתמשות במה שב-BootScene
```

---

## 🗑️ מה למחוק מכל סצנה

### 1. SignInScene - מסך התחברות
**מה למחוק**:
- ❌ **AuthApiClient GameObject** (אם קיים)

**למה**: SignInController משתמש ב-AuthApiClient שכבר ב-BootScene.

**איך לבדוק**:
1. פתחי את SignInScene
2. חפשי GameObject בשם `AuthApiClient` (או דומה)
3. מחקי אותו
4. ודאי ש-SignInController מצביע ל-AuthApiClient שב-BootScene

---

### 2. SignUpScene - מסך הרשמה
**מה למחוק**:
- ❌ **AuthApiClient GameObject** (אם קיים)

**למה**: SignUpController משתמש ב-AuthApiClient שכבר ב-BootScene.

**איך לבדוק**:
1. פתחי את SignUpScene
2. חפשי GameObject בשם `AuthApiClient` (או דומה)
3. מחקי אותו
4. ודאי ש-SignUpController מצביע ל-AuthApiClient שב-BootScene

---

### 3. PlanetScene - מסך בחירת פלנטה
**מה למחוק**:
- ❌ **PlanetApiClient GameObject** (אם קיים)

**למה**: GameManager משתמש ב-PlanetApiClient שכבר ב-BootScene.

**איך לבדוק**:
1. פתחי את PlanetScene
2. חפשי GameObject בשם `PlanetApiClient` (או דומה)
3. מחקי אותו
4. GameManager ימצא אותו אוטומטית עם `FindObjectOfType`

---

### 4. GamePlayScene - מסך המשחק
**מה למחוק**:
- ❌ **PlanetStateApiClient GameObject** (אם קיים)
- ❌ **StageStateApiClient GameObject** (אם קיים) - לא צריכים אותו יותר!

**למה**:
- GameManager משתמש ב-PlanetStateApiClient שב-BootScene
- StageStateApiClient לא בשימוש יותר (עברנו רק ל-PlanetStateApiClient)

**איך לבדוק**:
1. פתחי את GamePlayScene
2. חפשי GameObjects: `PlanetStateApiClient`, `StageStateApiClient`
3. מחקי את שניהם
4. GameBootstrap ו-StageStateAutoSave משתמשים ב-GameManager (לא צריכים API ישירות)

---

## ✅ מה להשאיר בכל סצנה

### BootScene (הכל נשאר):
```
GameManager (עם references ל):
  ├─ PlanetApiClient      ✓
  └─ PlanetStateApiClient ✓

AppSession                ✓
SceneLoader               ✓
AuthApiClient             ✓
PlanetApiClient           ✓
PlanetStateApiClient      ✓
```

### SignInScene:
```
SignInController (עם reference ל):
  └─ AuthApiClient (מ-BootScene)  ✓
```

### SignUpScene:
```
SignUpController (עם reference ל):
  └─ AuthApiClient (מ-BootScene)  ✓
```

### PlanetScene:
```
PlanetScreenController (אם צריך)
PlanetSpawner (אם צריך)
← אין API Clients!
```

### GamePlayScene:
```
GameBootstrap (אין API Clients!)  ✓
StageStateAutoSave (אין API Clients!)  ✓
MapController  ✓
HandController  ✓
← אין API Clients!
```

---

## 🔍 איך לוודא ש-References עובדים

### בBootScene:

1. בחרי **GameManager**
2. בדקי שיש references:
   - `Planet Api` → PlanetApiClient (באותה סצנה)
   - `Planet State Api` → PlanetStateApiClient (באותה סצנה)

### בSignInScene / SignUpScene:

1. בחרי **SignInController** או **SignUpController**
2. בדקי שיש reference:
   - `Api` → AuthApiClient
3. **אם ה-Reference ריק**:
   - גררי את AuthApiClient מה-Hierarchy (הוא יופיע אם פתחת קודם BootScene)
   - או: שמרי את הסצנה, פתחי את BootScene, ואז פתחי שוב את SignInScene

---

## 🎯 תהליך הניקוי המלא

### שלב 1: פתחי את BootScene
```
File > Open Scene > BootScene.unity
```

**ודאי שיש**:
- ✓ GameManager
- ✓ AppSession
- ✓ SceneLoader
- ✓ AuthApiClient (GameObject נפרד)
- ✓ PlanetApiClient (GameObject נפרד)
- ✓ PlanetStateApiClient (GameObject נפרד)

כל אחד מהם צריך להיות GameObject נפרד עם `DontDestroyOnLoad` בקוד.

---

### שלב 2: נקי את SignInScene
```
File > Open Scene > SignInScene.unity
```

1. חפשי ב-Hierarchy אם יש `AuthApiClient`
2. אם כן - **מחקי אותו**
3. בחרי את `SignInController`
4. ודאי שהשדה `Api` מצביע ל-AuthApiClient (אפשר לראות את הנתיב)

---

### שלב 3: נקי את SignUpScene
```
File > Open Scene > SignUpScene.unity
```

1. חפשי ב-Hierarchy אם יש `AuthApiClient`
2. אם כן - **מחקי אותו**
3. בחרי את `SignUpController`
4. ודאי שהשדה `Api` מצביע ל-AuthApiClient

---

### שלב 4: נקי את PlanetScene
```
File > Open Scene > PlanetScene.unity
```

1. חפשי ב-Hierarchy אם יש `PlanetApiClient`
2. אם כן - **מחקי אותו**

---

### שלב 5: נקי את GamePlayScene
```
File > Open Scene > GamePlayScene.unity
```

1. חפשי ב-Hierarchy אם יש:
   - `PlanetStateApiClient` → **מחקי**
   - `StageStateApiClient` → **מחקי**
2. בחרי את `GameBootstrap`:
   - ודאי שאין שדות `planetStateApi` או `deckSize` (הוסרו בקוד)
3. בחרי את `StageStateAutoSave`:
   - ודאי שאין שדה `planetStateApi` (הוסר בקוד)

---

## 🧪 איך לבדוק שהכל עובד

### Test 1: בדיקת Singleton
1. הפעילי את המשחק מ-BootScene
2. פתחי את Console
3. חפשי שגיאות על Singletons duplicates
   - **אם יש** → יש עוד כפילויות שלא מחקת
   - **אם אין** → מצוין! ✓

### Test 2: בדיקת התחברות
1. עברי למסך Sign In
2. התחברי
3. ודאי שעובר ל-Planet Selection
4. **אם עובד** → AuthApiClient עובד נכון! ✓

### Test 3: בדיקת משחק
1. בחרי Planet
2. בחרי Stage
3. ודאי שהמשחק נטען
4. נסי להניח tile
5. בדקי ב-Console שיש `[AutoSave] State saved successfully`
6. **אם עובד** → כל ה-API Clients עובדים! ✓

---

## ⚠️ אזהרות

### אזהרה 1: "Missing Reference"
אם אחרי המחיקה יש `Missing Reference` בשדות:

**פתרון**:
1. תוודאי ש-BootScene נטען ראשון
2. פתחי את BootScene
3. הפעילי Play Mode
4. עצרי
5. עכשיו פתחי את הסצנה הבעייתית
6. גררי את ה-API Client הנכון מה-Hierarchy (הוא יופיע מ-BootScene)

### אזהרה 2: "Object not found"
אם בזמן ריצה יש שגיאה `Object not found`:

**פתרון**:
- GameManager משתמש ב-`FindObjectOfType` - זה אמור לעבוד אוטומטית
- ודאי שב-BootScene יש את ה-API Client
- ודאי שיש לו `DontDestroyOnLoad` בקוד

---

## 📊 סיכום - לפני ואחרי

### לפני הניקוי (בעייתי):
```
BootScene:     AuthApiClient, PlanetApiClient, PlanetStateApiClient
SignInScene:   AuthApiClient (כפול! ❌)
SignUpScene:   AuthApiClient (כפול! ❌)
PlanetScene:   PlanetApiClient (כפול! ❌)
GamePlayScene: PlanetStateApiClient (כפול! ❌), StageStateApiClient (מיותר! ❌)
```

### אחרי הניקוי (נכון):
```
BootScene:     AuthApiClient, PlanetApiClient, PlanetStateApiClient ✓
SignInScene:   (ריק - משתמש בBootScene) ✓
SignUpScene:   (ריק - משתמש בBootScene) ✓
PlanetScene:   (ריק - משתמש בBootScene) ✓
GamePlayScene: (ריק - משתמש בBootScene) ✓
```

---

## 💡 למה זה חשוב?

1. **Performance** - פחות GameObjects, פחות עומס
2. **בהירות** - ברור איפה כל דבר נמצא
3. **תחזוקה** - קל לשנות משהו במקום אחד
4. **Bugs** - מונע התנהגויות מוזרות עקב כפילויות
5. **Multiplayer** - בעתיד, מנגנון מרוכז חיוני

---

**תאריך**: 2026-01-19
**גרסה**: 1.0
**סטטוס**: ✅ מוכן לשימוש

אחרי הניקוי, המשחק יהיה נקי, מהיר, ומאורגן! 🎉
