# MCP Siteswap Server - Gefundene Fehler

## 🔴 Kritische Fehler

### 1. `calculate_transitions` - Internal Server Error
**Tool:** `calculate_transitions`  
**Fehler:** `MCP error -32603: An error occurred.`  
**Testfälle:**
- Von: `bb666bb6668` zu `ffa00` mit `maxLength: 10`
- Von: `ffa00` zu `bb666bb6668` mit `maxLength: 10`

**Status:** Unbekannte Exception im Server

---

### 2. `calculate_transitions` - Performance Problem
**Tool:** `calculate_transitions`  
**Fehler:** User cancelled (Timeout/zu lange Ausführungszeit)  
**Testfall:**
- Von: `531` zu `ffa00` mit `maxLength: 15`

**Status:** Dauert zu lange, mögliches Performance-Problem

---

## 🟡 Parameter-Validierungsfehler

### 3. `generate_siteswaps` - numberOfPasses Parameter
**Tool:** `generate_siteswaps`  
**Fehler:** `Parameter 'numberOfPasses' must be of type integer,null, got number`  
**Details:** Passing-Parameter werden nicht korrekt als nullable integer behandelt

---

### 4. `generate_siteswaps` - numberOfJugglers Parameter
**Tool:** `generate_siteswaps`  
**Fehler:** `Parameter 'numberOfJugglers' must be of type integer,null, got number`  
**Details:** Passing-Parameter werden nicht korrekt als nullable integer behandelt

---

### 5. `swap_positions` - Position außerhalb des Bereichs
**Tool:** `swap_positions`  
**Testfall:** `siteswap: "531", position1: 0, position2: 100`  
**Fehler:** `An error occurred invoking 'swap_positions'.`  
**Details:** Keine sinnvolle Fehlermeldung bei ungültigen Positionen

---

## ℹ️ Erwartetes Verhalten (kein Fehler)

### 6. Ungültige Siteswaps
**Tools:** `get_local_siteswap`, `analyze_siteswap`, `generate_state_graph`  
**Testfälle:**
- `7772` (ungültig)
- `96` (ungültig)

**Status:** Diese Siteswaps sind ungültig, daher ist das Fehlschlagen der Tools erwartetes Verhalten. Möglicherweise könnten die Fehlermeldungen aber spezifischer sein.

---

## ✅ Erfolgreich getestete Tools

Die folgenden Tools haben in allen Tests korrekt funktioniert:

- ✅ `validate_siteswap` - Validierung funktioniert einwandfrei
- ✅ `analyze_siteswap` - Funktioniert mit gültigen Siteswaps
- ✅ `normalize_siteswap` - Normalisierung funktioniert
- ✅ `simulate_throw` - Simulation funktioniert
- ✅ `generate_state_graph` - Graph-Generierung funktioniert (mit gültigen Siteswaps)
- ✅ `generate_causal_diagram` - Diagramm-Generierung funktioniert
- ✅ `generate_transition_graph` - Funktioniert mit kurzen maxLength
- ✅ `get_local_siteswap` - Funktioniert mit gültigen Passing-Siteswaps
- ✅ `generate_siteswaps` - Funktioniert grundsätzlich gut

---

## 📋 To-Do Liste

### Priorität Hoch
- [ ] Bug #1 beheben: `calculate_transitions` Internal Server Error
- [ ] Bug #2 untersuchen: Performance-Problem bei langen Transitionen

### Priorität Mittel
- [ ] Bug #3 & #4 beheben: Parameter-Validierung für nullable integers korrigieren
- [ ] Bug #5 beheben: Bessere Fehlerbehandlung bei ungültigen Positionen in `swap_positions`

### Priorität Niedrig
- [ ] Erwägung: Spezifischere Fehlermeldungen bei ungültigen Siteswaps

---

## 📊 Test-Zusammenfassung

**Getestete Tools:** 13 von 13  
**Erfolgreiche Tests:** ~45 von ~50  
**Gefundene Bugs:** 5 echte Fehler  
**Datum:** 2. Dezember 2025



