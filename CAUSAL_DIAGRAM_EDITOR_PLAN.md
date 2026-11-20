# Causal Diagram Editor: Implementierungsplan

## Ziel

Einen mobil und am Rechner bedienbaren Editor für kausale Jonglierdiagramme bauen. Der Editor verwendet feste Zeitleisten pro Jongleur, zeigt Würfe als beschriftete Knoten und explizite Kausalbeziehungen als Pfeile zwischen Knoten. Der vollständige Graphzustand wird in einem teilbaren Deeplink gespeichert.

## Festgelegte Anforderungen

- Es gibt eine feste Zeitleiste pro Jongleur.
- Würfe sind Knoten auf den Zeitleisten und tragen ein Handlabel (z. B. L/R).
- Beim Hinzufügen wechseln die Handlabels standardmäßig ab.
- Das Handlabel eines einzelnen Wurfs ist änderbar. Diese Änderung verändert keine anderen vorhandenen Labels.
- Mehrere Jongleure können unterschiedliche Wurffrequenzen haben, z. B. 7 Würfe auf einer Spur und 5 auf einer anderen (Polyrhythmus).
- Knoten können mit aktivierter Einrasthilfe verschoben werden; freies Verschieben muss ebenfalls möglich sein. Die Einrasthilfe ist ein- und ausschaltbar.
- Beim Verschieben ändert sich nur die Position dieses Wurfknotens. Andere Knoten bleiben stehen; Pfeile bleiben an ihren explizit verbundenen Zielknoten verankert.
- Die Zahl der dargestellten Wiederholungen ist einstellbar und gilt für das Diagramm als Ganzes.
- Der Deeplink enthält den vollständigen bearbeitbaren Zustand: Wurfknoten mit Hand und Positionen, kausale Zielverbindungen, Einrastmodus und Wiederholungszahl. Die Höhe in Schlägen wird schreibgeschützt aus Zielknoten, Rhythmus und Positionen abgeleitet.
- Es gibt einen Katalog gebräuchlicher Siteswap- und Polyrhythmus-Presets. Weil Siteswap- und Polyrhythmusmuster keine endliche vollständige Presetmenge bilden, können zusätzlich beliebige gültige Siteswaps und benutzerdefinierte Polyrhythmus-Zeilenmuster geladen werden. Alle geladenen Muster bleiben bearbeitbar.
- Nullwürfe (`0`) sind gültige Siteswap-Leerbeats und werden als Beatknoten ohne Höhe/Pfeilverbindung dargestellt. Der Siteswap-Importer akzeptiert nur Ziffern und Kleinbuchstaben als Höhenzeichen.
- Die Hand eines Wurfknotens ist editierbar. Im anfänglichen Nicht-Multiplex-Modus hat jeder Knoten höchstens eine ausgehende und eine eingehende Verbindung. Das Ziel wird direkt über ein Feld geändert; das Verschieben eines Knotens lässt die Kante am gleichen Zielknoten.
- Unterschiedliche Polyrhythmus-Zeilen behalten ihren eigenen Abstand im gemeinsamen Zyklus. Für 7:5 liegen sieben Würfe im Abstand 5 und fünf Würfe im Abstand 7 innerhalb des 35-Zeit-Einheiten-Zyklus. Es gibt zwölf Knoten, die individuellen Zeitleistenabstände bleiben sichtbar.
- Die Auswahl, für welchen Jongleur der nächste Wurf angelegt wird, gehört zum geteilten Zustand.
- Die Bedienung und Darstellung funktionieren auf Touchgeräten und mit Maus/Tastatur auf Desktop.
- Unter dem Editor erscheint die bestehende PassistAnimation. Editorzustände werden in das Domain-Diagramm-Modell projiziert und ausschließlich über `JifMapper.Map(DiagramSet)` nach JIF gemappt. Unvollständige oder vom Mapper nicht unterstützte Diagramme zeigen eine kurze Verfügbarkeitsmeldung.

## Interaktions- und Datenmodell

1. Zeitleisten werden nach Jongleur gruppiert. Jede Spur teilt sich die gemeinsame Diagrammzeit; die Würfe pro Jongleur dürfen unterschiedliche Anzahl bzw. Abstände haben.
2. Ein Wurfknoten hat mindestens: stabile ID, Jongleur/Zeitleiste, Hand und Startzeit. Eine kausale Kante referenziert explizit Start- und Zielknoten sowie den Zyklusversatz. Sie beschreibt, welcher Wurf einen anderen erzwingt; sie ist keine Flug- oder Landebahn. Die Höhe in Schlägen wird aus der Kante und den rhythmischen Handrückkehrzeiten berechnet und nie unabhängig editiert.
3. Handalternation ist ein Standard beim Erzeugen einer Folge, keine globale Invariante. Manuelle Labels bleiben lokal und werden beim späteren Editieren nicht automatisch umgeschrieben.
4. Einrasten ist eine Platzierungshilfe, keine Beschränkung. UI stellt den Modus sichtbar und touchfreundlich um.
5. Wiederholungszahl steuert die dargestellten Wiederholungen des Gesamtmusters, ohne die zugrundeliegenden Wurfdefinitionen zu duplizieren.
6. Importierte Presets werden in dasselbe editierbare Modell überführt wie manuell erstellte Diagramme.

## Umsetzungsschritte

1. Bestehendes CausalDiagram-Domainmodell, Renderer und Blazor-Komponenten prüfen und die Editorlogik passend in den vorhandenen Schichten ergänzen.
2. Ein serialisierbares Editorzustandsmodell mit Versionierung erstellen. Es muss Spuren, Wurfknoten mit Hand/Position und explizite kausale Kanten mit Zyklusversatz sowie Wiederholungszahl und Snap-Einstellung ausdrücken. Gespeicherte Höhen verbundener Knoten werden beim Lesen und Schreiben mit den Kantenendpunkten synchronisiert; Höhen unverknüpfter Knoten bleiben intern für Muster-Migration erhalten, erscheinen aber nicht in der UI. Der Decoder migriert j2-Graphen deterministisch, indem er bei Mehrfachkanten Ausgangs- und Zielknoten nach erster gespeicherter Kante priorisiert; so bleiben Diagramme lesbar, ohne das Nicht-Multiplex-Modell zu verletzen.
3. Preset-Auswahl als Katalog gebräuchlicher Siteswaps (u. a. 3, 4, 5, 531, 441, 423) und Polyrhythmen (u. a. 3:3, 5:3, 7:5) ergänzen. Siteswap-Kanten folgen der Builder-Regel `height - numberOfHands` mit periodischem Zielknoten. Bei Polyrhythmen ist die nominale Zielzeit `start + (height - 2) * sourceBeatDuration`; sie wird dem nächsten periodischen Ereignis auf der adressierten Zielzeitleiste zugeordnet. Zusätzlich bleibt der manuelle Graph frei editierbar.
4. Responsive Passing-Zone-Oberfläche im vorhandenen violetten Schema umsetzen. Zeitleisten zeigen Wurfknoten mit Höhe und Hand sowie kausale Abhängigkeiten als Pfeile zwischen Ereignissen, nie als Objektflugbahnen.
5. Drag-Interaktion für Maus und Touch implementieren. Andere Knoten bleiben beim Verschieben stationär; eine Kante endet weiterhin an ihrem verknüpften Knoten. Snap-Modus quantisiert nur die neue Knotenposition.
6. Versioniertes Deeplink-Encoding/-Decoding integrieren und Browsernavigation berücksichtigen. Neue Links enthalten explizite Kanten; bisherige `j1`- und JSON-Links werden in Graphzustände migriert. Fehlerhafte oder unbekannte Versionen führen zu einem verständlichen Fallback statt Absturz.
7. Für mobile Ansichten mindestens 44 CSS-Pixel große transparente Wurf-Touchziele, horizontal scrollbare Zeitleisten und einen sichtbaren Tastaturfokus sicherstellen.
   Die SVG-Timeline wächst mit Musterlänge und Wiederholungen bei konstantem Pixelabstand pro Zeiteinheit; Touchziele bleiben kleiner als der Abstand benachbarter Beats, damit auch späte Wiederholungen einzeln auswählbar bleiben.
8. Browserverhalten mit E2E-Verifikation abdecken: Preset/Muster laden, URL kopieren, den kopierten Deeplink in einem frischen Seitenkontext öffnen, Zustand und Einstellungen vergleichen sowie mobile Viewport-, Scroll- und Touchzielgeometrie prüfen.
9. Den Editorzustand in `DiagramSet` überführen und den vorhandenen `JifMapper.Map(DiagramSet)`-Pfad für die PassistAnimation verwenden. Unterschiedliche Polyrhythmus-Zeilen nutzen ihr kleinsten gemeinsames Zeitintervall, ihre Ereignisse werden dabei nicht auf gleiche Abstände expandiert; JIF-Perioden schließen zusätzlich die Hand- und Prop-Orbits. Gemeinsame ganzzahlige Zeitskalierung unterstützt rationale Ereigniszeiten; unvollständige Graphen und Werte außerhalb der begrenzten Skala bleiben verständlich als nicht verfügbar markiert.

## Abnahmekriterien

- Ein Diagramm mit mindestens zwei Jongleuren und ungleichen Wurfzahlen (insbesondere 7:5) lässt sich darstellen und bearbeiten.
- Das Ändern eines Handlabels betrifft ausschließlich den ausgewählten Wurf; neu angelegte Folgen starten mit alternierenden Labels.
- Ein Knoten lässt sich mit Snap-Hilfe und frei verschieben. Andere Knoten bleiben an ihrer Position und kausale Pfeile bleiben mit ihrem Zielknoten verbunden.
- Die globale Wiederholungszahl kann geändert werden und wird korrekt dargestellt.
- Katalog-Presets und benutzerdefinierte Siteswap-/Polyrhythmus-Muster erzeugen editierbare Diagramme.
- Ein Deeplink rekonstruiert den vollständigen Zustand einschließlich aller Einstellungen und benutzerdefinierten Änderungen.
- Kernaktionen sind per Maus, Touch und Tastatur bedienbar; auf schmalen Ansichten bleiben Zeitleisten horizontal scrollbar, Wurfziele mindestens 44 CSS-Pixel groß und Tastaturfokus sichtbar.
- Der aktive Jongleur für neue Knoten, Handlabels und kausale Ziele sowie die abgeleitete Höhe bleiben nach Deeplink-Öffnen erhalten.
- Zielwechsel bieten nur freie Knoten an und lehnen belegte Eingänge ab; je Knoten gibt es höchstens eine ausgehende Verbindung. Polyrhythmus-Vorlagen behalten die zeilenspezifischen Zeitabstände über das gemeinsame Periodenintervall bei.
- Browser-E2E-Prüfungen belegen Copy/Open-Flows und realistische Linkgrößen.
- Browser-E2E prüft eine echte CDP-Touch-Swipe-Geste auf leerer Canvasfläche sowie die Positions- und URL-Änderung durch ArrowRight. Große benutzerdefinierte Zustände werden zusätzlich im Zustands-/Codec-Test dekodiert; der Browser-Copy/Open-Test deckt einen üblichen kurzen Deeplink ab.
- Ein mobiler E2E-Fall mit 20 Wiederholungen prüft Timelinebreite, Mindestabstand der Touchziele und die Fokussierbarkeit eines späten Wurfs nach horizontalem Scrollen.
- Bestehende Kausaldiagramm-Ansichten und bestehende Siteswap-/Polyrhythmus-Darstellungen funktionieren weiter.
- Die PassistAnimation erscheint unter dem Editor für unterstützte Muster, darunter 7:5 und Siteswap 531; Presetwechsel aktualisieren die Animation, unvollständige Graphen zeigen den Fallback.

## Zu verifizieren

- Bestehende Domänenklassen und Generatoren nicht unnötig duplizieren; Editor-spezifische Positionen/Zustände sollen in einer passenden UI-/Anwendungsabstraktion liegen.
- Deeplink-Größe sowohl für typische Presets als auch für größere benutzerdefinierte Muster prüfen; kompakte serialisierte Nutzlast, Größenlimit und sichere Validierung beibehalten.
- Uncommittete Änderungen im Arbeitsverzeichnis erhalten und nicht überschreiben.
