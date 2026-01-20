# Quick Cleanup Checklist ✓
## רשימת בדיקה מהירה לניקוי הסצנות

---

## 📋 Checklist - סמני V כשסיימת

### BootScene ✓
- [ ] יש GameManager
- [ ] יש AppSession
- [ ] יש SceneLoader
- [ ] יש AuthApiClient (GameObject נפרד)
- [ ] יש PlanetApiClient (GameObject נפרד)
- [ ] יש PlanetStateApiClient (GameObject נפרד)
- [ ] GameManager מצביע ל-PlanetApiClient
- [ ] GameManager מצביע ל-PlanetStateApiClient

---

### SignInScene 🗑️
- [ ] **מחקתי** את AuthApiClient GameObject (אם היה)
- [ ] SignInController.Api מצביע ל-AuthApiClient מ-BootScene
- [ ] אין שגיאות Missing Reference

---

### SignUpScene 🗑️
- [ ] **מחקתי** את AuthApiClient GameObject (אם היה)
- [ ] SignUpController.Api מצביע ל-AuthApiClient מ-BootScene
- [ ] אין שגיאות Missing Reference

---

### PlanetScene 🗑️
- [ ] **מחקתי** את PlanetApiClient GameObject (אם היה)
- [ ] אין שגיאות Missing Reference

---

### GamePlayScene 🗑️
- [ ] **מחקתי** את PlanetStateApiClient GameObject (אם היה)
- [ ] **מחקתי** את StageStateApiClient GameObject (אם היה)
- [ ] GameBootstrap אין לו שדה planetStateApi
- [ ] GameBootstrap אין לו שדה deckSize
- [ ] StageStateAutoSave אין לו שדה planetStateApi
- [ ] RestartStageButton אין לו שדה planetStateApi

---

### בדיקות סופיות 🧪
- [ ] הרצתי את המשחק מ-BootScene
- [ ] אין שגיאות ב-Console על duplicates
- [ ] התחברות עובדת
- [ ] בחירת Planet עובדת
- [ ] בחירת Stage עובדת
- [ ] המשחק נטען
- [ ] הנחת tile עובדת
- [ ] AutoSave עובד (יש לוג `[AutoSave] State saved successfully`)

---

## ✅ כשסיימת הכל - מעולה!

עכשיו יש לך:
- מבנה נקי ומאורגן
- אין כפילויות
- הכל עובד דרך BootScene
- קל לתחזק ולהוסיף features

🎉 **כל הכבוד!**
