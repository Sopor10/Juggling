# Passing Siteswaps - Interface p,p,s,s,p,p

## Übersicht

- **Periode:** 6
- **Personen:** 2
- **Objekte:** 6 (Ground State 111111)
- **Interface:** p,p,s,s,p,p (Pass-Pass-Self-Self-Pass-Pass)
- **Lokales Interface:** p,s,p (Pass-Self-Pass) für beide Jongleure
- **Pässe pro Person:** 2

## Analysierte Globale Siteswaps

| Global | A lokal | A Keulen | B lokal | B Keulen | Start A/B |
|--------|---------|----------|---------|----------|-----------|
| 996831 | 4.5, 3, 1.5 | 3.0 | 4.5, 4, 0.5 | 3.0 | 3/3 |
| 996471 | 4.5, 3, 3.5 | 3.67 | 4.5, 2, 0.5 | 2.33 | 4/2 |
| 996417 | 4.5, 3, 0.5 | 2.67 | 4.5, 2, 3.5 | 3.33 | 3/3 |
| 996237 | 4.5, 3, 1.5 | 3.0 | 4.5, 1, 3.5 | 3.0 | 3/3 |
| 978831 | 4.5, 4, 1.5 | 3.33 | 3.5, 4, 0.5 | 2.67 | 3/3 |
| 978471 | 4.5, 4, 3.5 | 4.0 | 3.5, 2, 0.5 | 2.0 | 4/2 |
| 978417 | 4.5, 4, 0.5 | 3.0 | 3.5, 2, 3.5 | 3.0 | 3/3 |
| 978237 | 4.5, 4, 1.5 | 3.33 | 3.5, 1, 3.5 | 2.67 | 3/3 |

---

## Liste A - Lokale Siteswaps für Jongleur A

Alle rotiert als **p,s,p** (Pass-Self-Pass):

| # | Lokal (p,s,p) | Ø Keulen | Start | Passend zu B mit... |
|---|---------------|----------|-------|---------------------|
| 1 | 4.5, 3, 0.5 | 2.67 | 3 | 3.33 Keulen |
| 2 | 4.5, 3, 1.5 | 3.0 | 3 | 3.0 Keulen |
| 3 | 4.5, 3, 3.5 | 3.67 | 4 | 2.33 Keulen |
| 4 | 4.5, 4, 0.5 | 3.0 | 3 | 3.0 Keulen |
| 5 | 4.5, 4, 1.5 | 3.33 | 3 | 2.67 Keulen |
| 6 | 4.5, 4, 3.5 | 4.0 | 4 | 2.0 Keulen |

### Schwierigkeits-Sortierung (leicht → schwer):

1. **4.5, 3, 0.5** (2.67) - Zwei Pässe, ein kurzer Self
2. **4.5, 3, 1.5** (3.0) - Klassisch, ausgewogen
3. **4.5, 4, 0.5** (3.0) - Mit hohem Self
4. **4.5, 4, 1.5** (3.33) - Anspruchsvoller
5. **4.5, 3, 3.5** (3.67) - Hoher zweiter Pass
6. **4.5, 4, 3.5** (4.0) - Am schwersten

---

## Liste B - Lokale Siteswaps für Jongleur B

Alle rotiert als **p,s,p** (Pass-Self-Pass):

| # | Lokal (p,s,p) | Ø Keulen | Start | Passend zu A mit... |
|---|---------------|----------|-------|---------------------|
| 1 | 3.5, 2, 0.5 | 2.0 | 2 | 4.0 Keulen |
| 2 | 4.5, 2, 0.5 | 2.33 | 2 | 3.67 Keulen |
| 3 | 3.5, 4, 0.5 | 2.67 | 3 | 3.33 Keulen |
| 4 | 3.5, 1, 3.5 | 2.67 | 3 | 3.33 Keulen |
| 5 | 4.5, 4, 0.5 | 3.0 | 3 | 3.0 Keulen |
| 6 | 4.5, 1, 3.5 | 3.0 | 3 | 3.0 Keulen |
| 7 | 3.5, 2, 3.5 | 3.0 | 3 | 3.0 Keulen |
| 8 | 4.5, 2, 3.5 | 3.33 | 3 | 2.67 Keulen |

### Schwierigkeits-Sortierung (leicht → schwer):

1. **3.5, 2, 0.5** (2.0) - Am leichtesten, wenige Objekte
2. **4.5, 2, 0.5** (2.33) - Leicht
3. **3.5, 4, 0.5** (2.67) - Mit hohem Self
4. **3.5, 1, 3.5** (2.67) - Schneller Zip-Self
5. **3.5, 2, 3.5** (3.0) - Ausgewogen
6. **4.5, 1, 3.5** (3.0) - Schneller Zip
7. **4.5, 4, 0.5** (3.0) - Hoher Self
8. **4.5, 2, 3.5** (3.33) - Am schwersten

---

## Kombinationsregeln

**Wichtig:** Die Summe der Keulen von A + B muss immer **6** ergeben!

### Gültige Kombinationen nach Keulenverteilung:

| A Keulen | B Keulen | Start | Gültige Kombinationen |
|----------|----------|-------|----------------------|
| 4.0 | 2.0 | 4/2 | A6 × B1 |
| 3.67 | 2.33 | 4/2 | A3 × B2 |
| 3.33 | 2.67 | 3/3 | A5 × (B3, B4) |
| 3.0 | 3.0 | 3/3 | (A2, A4) × (B5, B6, B7) |
| 2.67 | 3.33 | 3/3 | A1 × B8 |

---

## Frei kombinierbare Gruppen

### Gruppe 1: Symmetrischer Start (3/3)

**A mit 3.0 Keulen:**
- 4.5, 3, 1.5
- 4.5, 4, 0.5

**B mit 3.0 Keulen:**
- 4.5, 4, 0.5
- 4.5, 1, 3.5
- 3.5, 2, 3.5

→ **6 mögliche Kombinationen**, alle mit 3/3 Start

### Gruppe 2: Asymmetrischer Start (4/2)

**A mit 4.0 Keulen:**
- 4.5, 4, 3.5

**B mit 2.0 Keulen:**
- 3.5, 2, 0.5

→ **1 Kombination** mit 4/2 Start

### Gruppe 3: Asymmetrisch (3.33/2.67)

**A mit 3.33 Keulen:**
- 4.5, 4, 1.5

**B mit 2.67 Keulen:**
- 3.5, 4, 0.5
- 3.5, 1, 3.5

→ **2 Kombinationen** mit 3/3 Start (gerundet)

---

## Notation Erklärung

| Lokal | Global | Beschreibung |
|-------|--------|--------------|
| 4.5 | 9 | Hoher Pass zum Partner |
| 3.5 | 7 | Mittlerer Pass (Single) |
| 2.5 | 5 | Kurzer Pass (Zip-Pass) |
| 4 | 8 | Hoher Self |
| 3 | 6 | Normaler Self |
| 2 | 4 | Zip/Hold |
| 1 | 2 | Schneller Zip |
| 0.5 | 1 | Flip-Pass (sehr kurz) |
| 0 | 0 | Leere Hand |

---

## Beispiel-Kombinationen

### Einfach (3/3 Start):
- **A:** 4.5, 3, 1.5 + **B:** 3.5, 2, 3.5 = **978417**
- Beschreibung: A macht hohen Pass, Self, niedrigen Pass. B macht Single, Zip, Single.

### Mittel (3/3 Start):
- **A:** 4.5, 4, 0.5 + **B:** 4.5, 1, 3.5 = Neue Kombination
- Beschreibung: A macht hohen Pass, hohen Self, Flip. B macht hohen Pass, Zip, Single.

### Schwer (4/2 Start):
- **A:** 4.5, 4, 3.5 + **B:** 3.5, 2, 0.5 = **978471**
- Beschreibung: A hat 4 Keulen und macht anspruchsvolles Muster. B hat nur 2 und macht leichtes Muster.

---

*Generiert mit dem Siteswap Generator MCP Server*

