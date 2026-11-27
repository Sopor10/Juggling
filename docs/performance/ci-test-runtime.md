# CI-Testlaufzeit

## Ziel und Messrahmen

Ziel ist, die Laufzeit der PR-Testpipeline in GitHub Actions um mindestens 50 % zu senken. Referenzwert ist die mediane Dauer erfolgreicher `PR Validation`-Läufe auf `ubuntu-latest`: 290 s (18 Läufe, 238–815 s). Der Median ist ein historischer Richtwert; der Lauf vom 20. November 2026 liefert die detaillierte Phasenbaseline.

Die lokale VM wurde nicht für Laufzeitoptimierungen oder Performance-Messungen verwendet. Alle Laufzeitvergleiche stammen aus GitHub-hosted Actions. Gemessen werden Gesamtdauer, Schrittzeiten, TRX-Testzeiten und bei Bedarf der MSBuild-Binlog. Binlog-Läufe dienen der Diagnostik und werden nicht als normale Laufzeitmessungen gewertet.

## Baseline: GitHub Actions

Lauf `44829121788` (`ubuntu-latest`, erfolgreich, 20. November 2026): 267 s vom Runnerstart bis Jobende. Der Composite-Workflow benötigte rund 263 s.

| Phase | Dauer | Beobachtung |
| --- | ---: | --- |
| .NET Setup | 0.9 s | SDK 11.0.100 |
| Tool Restore | 2.5 s | |
| CSharpier | 0.9 s | |
| wasm-tools installieren | 31.2 s | Emscripten-Packs stellen den Großteil der Installationszeit |
| E2E bauen und Playwright installieren | 44.9 s | Build: 30.6 s; Browser und Systemabhängigkeiten: ca. 14 s |
| `dotnet test` | 182.3 s | E2E-Assembly: 121 s; übrige Assemblies melden je 1–2 s Testzeit |

Im Lauf bestanden 875 Tests, drei wurden übersprungen. Der E2E-Testlauf ist der größte einzeln ausgewiesene Testanteil. Der Baseline-Commit auf `main` hatte serielle Test-Collections, um Chromium-Abstürze auf der 8-GB-Entwicklungs-VM zu vermeiden. Der aktuelle Arbeitszweig dieses Gesprächs hat bereits `maxParallelThreads=4`; der Remote-Test zeigte, dass diese Einstellung allein kaum Durchsatz bringt.

## Änderungen im aktuellen Versuch

| Änderung | Zweck | Messung / Status |
| --- | --- | --- |
| NuGet-Paketcache in `PR Validation` | Wiederholte Paketdownloads und Restore-Arbeit reduzieren | Nicht übernommen: Restore-Target spart 5.7 s (7.0 → 1.2 s), Cache-Laden kostet 11.9–13.7 s pro Lauf |
| xUnit-Runsettings nur in CI: parallele Collections, maximal 4 Threads | Die CI-Runner-Ressourcen nutzen, ohne die konservative lokale Runner-Konfiguration zu ändern | Verworfen: Runsettings-Override änderte die geladene Runner-Konfiguration nicht; TRX zeigte maximale Testüberlappung 1 |
| Separate CI-xUnit-Konfiguration mit parallelen Collections, maximal 4 Threads | E2E-Durchsatz auf CI erhöhen, während die konservative lokale Runner-Konfiguration seriell bleibt | Nicht übernommen: Lauf 44926606209 erreichte maximal 4 parallele Tests, E2E-Fenster ca. 117 s statt 121 s; TRX-Summe stieg auf 420.8 s |
| TRX-Zeiten bei CI-Läufen als kurzlebiges Actions-Artefakt; Binlog nur im Diagnoseversuch | Testdauer pro Fall nachverfolgen und Build-/Targetkosten bei Bedarf auswerten | Kurzlebige TRX-Artefakte sind in der PR- und Main-Branch-Testausführung aktiviert. MSBuild-Binlog wurde separat mit `baronfel.binlog.cli` und `binlog-insights` ausgewertet. Binlog wird nicht routinemäßig hochgeladen, da es keinen Performancegewinn bringt |
| Release-Konfiguration für Build und Test | WebAssembly-App und Testhost mit optimiertem Build ausführen; mögliche Verringerung der wiederholten Browser-App-Startzeit messen | Nicht übernommen: Lauf 44927901496 dauerte 341 s; E2E-Assembly 139 s und `dotnet test` 212 s |

### Ergebnis des ersten Versuchs

Lauf `44925744594` auf GitHub Actions war erfolgreich und dauerte 324 s (5:24), also 57 s länger als der detailliert ausgewertete 267-s-Baseline-Lauf. Die E2E-Assembly dauerte 134 s statt 121 s. Die Runsettings-Werte wurden vom Testadapter nicht wirksam: Die 50 TRX-Fälle überlappten zeitlich nicht. Ein Cache-Miss und 11.5 s Cache-Speicherung gehören ebenfalls zu diesem Kaltlauf; daraus lässt sich noch kein Cache-Hit-Gewinn ableiten. Dieser Lauf belegt keine Optimierung und wird nicht als Erfolg gewertet.

Die TRX-Zeiten lieferten zugleich eine Messbasis für den nächsten Versuch: E2E-Fälle dauern zusammen 130.1 s. Die zehn längsten Fälle liegen zwischen 2.9 und 5.4 s. Der Binlog und die TRX-Dateien wurden aus dem Actions-Artefakt `test-diagnostics` ausgewertet.

Die Binlog-Analyse schlüsselt den Build/Test-Engpass weiter auf: MSBuild-Ziele summierten sich auf 190.5 s; das E2E-`VSTest`-Ziel belegte 180.6 s. Danach folgten der E2E-Build mit 42 s, Restore mit 7 s und `Csc` mit 5.6 s. Das bestätigt, dass Build-Compilerarbeit allein den Zielwert nicht annähernd erklärt; die E2E-Testphase ist der zentrale Hebel.

Ein zweiter Lauf mit maximal vier parallelen E2E-Tests dauerte 316 s als Job. Die TRX-Dauern summierten sich auf 420.8 s bei 50 Tests; das belegte Testzeitfenster betrug rund 113 s und maximal vier Tests überlappten. Die Serial-Baseline hatte 130.1 s summierte Testdauer. Parallelität erhöhte damit die pro-Test-Latenz stark und verkürzte das E2E-Fenster nur gering. Der CI-spezifische Parallelmodus wurde entfernt.

Ein dritter Lauf im Release-Modus dauerte 341 s als Job. Die E2E-Assembly brauchte 139 s; der `dotnet test`-Schritt 212 s. Release wird deshalb ebenfalls verworfen.

Ein CI-Lauf gilt als Nachweis für das 50-%-Ziel, wenn die gemessene PR-Jobdauer höchstens 145 s beträgt und alle im PR-Workflow vorgesehenen Tests erfolgreich sind. Der historische Median von 290 s ist die Vergleichsbasis.

### Shared-Browser-/HTTP-Cache-Experiment

Lauf `44931008259` auf GitHub Actions testete einen Umbau des E2E-Testhosts: statt Playwright-Proxy wurde Kestrel direkt gestartet; E2E-Tests verwendeten einen serialisierten, gemeinsamen Browser-Kontext, damit Chromium den HTTP-Cache über Testseiten hinweg wiederverwenden kann. Der Lauf war erfolgreich (875 bestanden, 3 übersprungen).

| Messwert | Baseline `44829121788` | Kandidat `44931008259` | Änderung |
| --- | ---: | ---: | ---: |
| Jobdauer | 267 s | 279 s | +4,5 % |
| `dotnet test` | 182,3 s | 176,1 s | −3,4 % |
| E2E-Assembly | 121 s | 102 s | −15,7 % |
| E2E-Fälle | 50 | 50 | gleich |

Das E2E-Fenster wurde kürzer, die gesamte Teststufe aber nur um rund 6 s. Der Job wurde gegenüber der detaillierten Baseline 12 s langsamer; außerdem dauerte der vorgeschaltete Build-/Playwright-Schritt im Kandidaten 56,7 s statt 44,9 s. Der umfangreiche Fixture-Umbau wird daher nicht übernommen: Der gemessene Gesamtjob verbessert sich nicht und verfehlt das Ziel deutlich. Die TRX-Instrumentierung bleibt als Diagnosehilfe mit drei Tagen Aufbewahrung bestehen.

### E2E-Sharding-Experiment

Lauf `44932605993` verteilte alle 50 E2E-Fälle auf drei zusätzliche Runner und führte die vier Nicht-E2E-Projekte parallel aus. Alle Testprojekte bestanden; die Workflowdauer betrug rund 210 s. Dieser Lauf verfehlte das 145-s-Ziel und verbrauchte deutlich mehr Actions-Minuten durch das dreifach wiederholte WebAssembly-, Build- und Browser-Setup. Der Lauf wird verworfen. Ein temporärer Fehler in der nachgelagerten TRX-Abdeckungsprüfung machte den Workflowstatus rot, obwohl die TRX-Dateien 875 bestandene und drei übersprungene Fälle belegen.

### Vollständige Tests in PR und Main

Das PR-Gate und der Main-Workflow führen beide die vollständige Testmenge aus `Juggling.slnx` aus. Zusätzlich zu `Juggling.slnx` existierten zeitweise `Juggling.UnitTests.slnx` und `Juggling.PrUnitTests.slnx`. Auf Wunsch des Nutzers wurden beide entfernt; `Juggling.slnx` ist die einzige Solution im Repository. Der PR-Workflow wählt keine Tests anhand einer verkleinerten Suite aus. Der explizite `wasm-tools`-Installationsschritt wurde nach einer vollständigen Remote-Validierung entfernt; Playwright nutzt System-Chrome.

Lauf `44934207637` validierte den damaligen PR-Ablauf erfolgreich auf `ubuntu-latest`; er enthielt einen E2E-Smoke-Test, aber nicht alle E2E-Fälle:

| Messwert | Detaillierte Baseline `44829121788` | Historischer Median | Kandidat `44934207637` |
| --- | ---: | ---: | ---: |
| PR-Workflowdauer | 267 s | 290 s | 113 s |
| Beschleunigung | 57,7 % | 61,0 % | — |
| Formatprüfung | 0,9 s | — | 20 s |
| Unit-/Design-Tests | im `dotnet test`-Schritt | — | 87 s |
| E2E-Job | vollständige Suite | — | 109 s |
| Bestandene / übersprungene Fälle im PR-Lauf | 875 / 3 | — | 826 / 3 |

Der Kandidat liegt 32 s unter dem 145-s-Ziel und ist auch gegenüber dem einzelnen 267-s-Baseline-Lauf um mehr als 50 % schneller. Die Runner-Zeiten summieren sich im Kandidaten auf rund 216 s (Format, Unit/Design und Smoke), verglichen mit 267 s für den Baseline-Job; der PR-Lauf ist damit auch beim Actions-Verbrauch nicht teurer. Die E2E-Tests werden nicht auf der lokalen VM gemessen oder ausgeführt.

Die damalige Verbesserung verschob 49 E2E-Fälle aus der PR-Prüfung in den Main-Branch-Test. Diese Abdeckungsreduktion wurde nach der Nutzer-Klarstellung zurückgenommen: Alle E2E-Fälle müssen auch auf dem Feature-Branch laufen.

## Messdaten und Reproduktion

- Workflow: `.github/workflows/PR.yml` und `.github/workflows/steps/action.yml`
- Testkonfiguration: `Siteswaps.E2ETests/xunit.runner.json` (Baseline auf `main`: seriell; aktueller Arbeitszweig: 4 Threads)
- Historischer Actions-Lauf: <https://github.com/Sopor10/Juggling/actions/runs/44829121788>
- Shared-Browser-/Cache-Messung: <https://github.com/Sopor10/Juggling/actions/runs/44931008259>
- PR-Smoke- und Unit-Solution-Messung: <https://github.com/Sopor10/Juggling/actions/runs/44934207637>
- Ein VSTest-Runsettings-Override wurde erprobt, aber vom Testadapter nicht angewendet; ein separates CI-Runner-JSON wirkte, brachte jedoch keinen ausreichenden Gewinn.

## Folgeversuch: weitere Halbierung des 113-s-PR-Laufs

Ursprünglich wurde der 113-s-PR-Lauf als Referenz für die nächste Halbierung verwendet. Das war nicht vergleichbar, weil dieser Lauf weniger E2E-Fälle ausführte. Für gleiche Testabdeckung ist der vollständige 267-s-Lauf `44829121788` die passendere Referenz; eine Halbierung entspricht höchstens 133,5 s. Die vollständige Suite bleibt auf PR und `main` aktiv. Alle Laufzeitmessungen stammen aus GitHub Actions; auf der lokalen VM wurden keine Tests, Builds oder Performance-Messungen ausgeführt.

### Gemessene Engpässe

| PR-Job | Wandzeit im Lauf `44934207637` | Befund für das 56,5-s-Ziel |
| --- | ---: | --- |
| Formatting | 20 s | bereits unter dem Zielwert |
| Unit und Design | 87 s | eigenständiger kritischer Pfad über dem Zielwert |
| E2E-Smoke | 109 s | eigenständiger kritischer Pfad über dem Zielwert |

Die PR-Gesamtdauer wird durch den längsten parallelen Job bestimmt. Daher müssen Unit/Design und E2E-Smoke beide unter 56,5 s fallen. Beim E2E-Job entfallen rund 35 s auf die Installation von `wasm-tools`, 39,5 s auf den E2E-Build, etwa 20 s auf Playwright-Browser/Systempakete und rund 8–10 s auf Test und Abschluss. Workload-Installation und Build allein summieren sich damit auf ungefähr 74,5 s. Der Unit-/Design-Job benötigt rund 14 s für Restore, 22 s für Kompilierung und etwa 25 s für den Design-Testhost; seine Gesamtzeit beträgt 87 s.

### Bewertete Wege

| Ansatz | Erwarteter Effekt | Kosten und Einschränkung | Entscheidung |
| --- | --- | --- | --- |
| Playwright-System-Chrome (`Channel=chrome`) statt Browserdownload | Spart den Playwright-Browserdownload und dessen Systempakete | Vollsuite-Lauf `44959271646` bestand mit Chrome 155 trotz Playwright-Revision Chromium 159: alle 50 E2E-Tests erfolgreich. Der E2E-TRX-Zeitbetrag stieg gegenüber dem vorherigen Probe-Lauf um rund 5 %, der Gesamtjob fiel dennoch von 286 auf 261 s. | Übernommen; volle Browserabdeckung remote validiert |
| Browser- oder Workload-Cache | Warm-Cache könnte Setup sparen | Browserpayload ca. 325 MiB; Playwright rät vom Browsercache in CI ab, da Restore ungefähr so lange dauern kann wie der Download. Ein Workload-Cache müsste Packs, SDK-Manifeste und Installationsmetadaten zusammen sichern; die Runner-Logs zeigen dafür 6,7 s Emscripten-Cache und 20,5 s Emscripten-SDK innerhalb der 35-s-Installation. Die beiden größten Emscripten-Pakete sind bereits komprimiert ca. 260 MB; weitere Workload-Pakete bringen die bekannten Downloads auf mindestens ca. 360 MB. Selbst bei kostenloser Workload-Wiederherstellung und ohne Playwright-Installation verbleiben beim E2E-Job ungefähr 50,5 s für Setup, Build und Smoke, noch vor Cache-Restore. Der unabhängige Unit-/Design-Job braucht weiterhin 87 s. | Kein Weg zum Gesamtziel allein; Cache-Archivgröße und Restore-Zeit sind ungemessen und müssten remote erprobt werden |
| E2E-Sharding | Kann Testzeit aufteilen | Repliziert Workload-, Build- und Browser-Setup; früheres Drei-Shard-Experiment brauchte ca. 210 s Workflowzeit und erhöhte Runnerkosten deutlich | Verworfen |
| Größerer GitHub-hosted Runner | Mehr CPU könnte Build- und Testzeiten senken | Dieses Repository gehört einem persönlichen GitHub-Konto. Größere GitHub-hosted Runner sind für Organisationen und Enterprises auf Team-/Enterprise-Cloud-Plänen verfügbar, nicht für persönliche Konten ([GitHub-Dokumentation](https://docs.github.com/en/actions/concepts/runners/larger-runners)) | Für dieses Repository nicht verfügbar |
| Assets aus PR-Preview wiederverwenden | Könnte WASM-Build doppelt vermeiden | Preview ist ein anderer Workflow/Runner; Artefakte müssten SHA-sicher gekoppelt werden. Der E2E-Host soll außerdem die PR-Serverassembly und deren WebAssembly-Projekt testen | Kein belastbarer direkter Wiederverwendungspfad im aktuellen Workflowgraph |
| Gemeinsamer Browser-/HTTP-Cache | Reduzierte E2E-Testfensterzeit | Gemessener Gesamtjob wurde mit 279 s gegenüber 267 s Baseline langsamer; Testumgebungsisolation ist zusätzlich zu erhalten | Bereits gemessen und verworfen |
| Testprojekte weiter aufteilen | Könnte einzelne Unit-Jobs verkürzen | Erzeugt zusätzliche Checkout-, Restore- und Buildläufe; hilft nicht gegen den parallelen E2E-kritischen Pfad und kann den Actions-Verbrauch erhöhen | Kein ausreichender Gesamtpfad belegt |

Die TRX-Artefakte des Laufs `44934207637` wurden zusätzlich aus Actions heruntergeladen und ausgewertet (ohne lokale Testausführung): MCP-Tests liefen in ca. 1,7 s, `Siteswaps.Test` in 1,0 s, Generator-Tests in 2,9 s und Design-Tests in 25,7 s. Damit liegt der Großteil der 87-s-Jobzeit außerhalb der eigentlichen TRX-Testfenster; ein bloßes Aufteilen nach Testprojekten beseitigt Restore-/Buildkosten nicht.

Die Workload-Cache-Einschätzung basiert auf dem Actions-Log des E2E-Jobs sowie den dokumentierten .NET-Workload-Pfaden ([Workload-Installation](https://learn.microsoft.com/dotnet/core/tools/dotnet-workload-install), [Installationsdesign](https://github.com/dotnet/designs/blob/main/accepted/2021/workloads/workload-installation.md)). Playwright empfiehlt, Browser-Binaries in CI nicht zu cachen, weil das Wiederherstellen ungefähr so lange wie der Download dauern kann ([Playwright CI](https://playwright.dev/dotnet/docs/ci)).

### Messungen des Folgeversuchs

| Actions-Lauf | PR-Workflow | Jobs | PR-Testumfang | Runner-Sekunden (ca.) | Ergebnis |
| --- | ---: | --- | --- | ---: | --- |
| `44943807099` | 64 s | Formatting 9 s, Unit 57 s, Design 60 s, E2E 51 s | 826 bestanden, 3 übersprungen | 177 | 43,4 % schneller, Ziel verfehlt |
| `44944146629` | 73 s | Graph-Build: Unit 70 s, Design 64 s, E2E 57 s, Formatting 23 s | alle Jobs erfolgreich | 214 | Graph-Build verworfen, langsamer |
| `44944949597` | 60 s | Formatting 14 s, Unit 56 s | 815 bestanden, 3 übersprungen | 70 | weniger Tests als vollständige Suite |
| `44945260744` | 61 s | Formatting 14 s, Unit 57 s | 815 bestanden, 3 übersprungen | 71 | weniger Tests als vollständige Suite |
| `44945442555` | 39 s | Formatting 10 s, PR-Unit 36 s | 325 bestanden, 0 übersprungen | 46 | **nicht akzeptiert:** Testumfang reduziert |

Die Läufe mit 815 und 325 Tests wurden als Laufzeitexperimente ausgewertet, aber wegen der reduzierten Abdeckung nicht als Lösung übernommen. Das Ziel von 56,5 s gegen den 113-s-Lauf ist daher ebenfalls nicht mehr gültig.

### Ergebnis

Mit unverändert vollständiger Testabdeckung ist das 50-%-Ziel noch nicht erreicht. Der 39-s-Lauf `44945442555` wird nicht als Erfolg gezählt, weil er nur 325 statt 875 Tests ausführte.

### Prüfung weiterer Einsparungen

Eine Nachanalyse des Actions-Logs `44945442555` zeigte, dass der verkürzte Unit-Job rund 11 s mit Restore, 16 s mit Build und 2 s mit Tests verbrachte. Diese Messung bleibt nützlich für den Buildkosten-Anteil, ist aber wegen des reduzierten Umfangs kein Zielwert für die vollständige Suite.

| Kandidat | Messgrundlage | Einschätzung |
| --- | --- | --- |
| TRX-Artefakt weglassen | Upload im aktuellen Lauf unter 1 s | Kleiner Gewinn, dafür keine kurzlebigen Testergebnisse zur Fehleranalyse; nicht übernommen |
| SDK-Setup im Formatting-Job entfernen | SDK 11.0.100 war auf dem Runner bereits installiert; Setup-Schritt unter 1 s | Zu wenig Laufzeitgewinn für den Verlust der expliziten SDK-Auswahl; nicht übernommen |
| NuGet-Cache | Frühere Actions-Läufe: Restore-Ersparnis ca. 5,7 s, Cache-Wiederherstellung 11,9–13,7 s | Bereits langsamer als Restore ohne Cache; nicht übernommen |
| Testprojekte auf zusätzliche Runner verteilen | Beide PR-Testprojekte teilen Komponenten-, Analyzer- und Serverbuilds | Könnte die Wandzeit senken, wiederholt aber Build- und Restore-Arbeit und erhöht Actions-Minuten; mit Kostenrahmen verworfen |

Für die vollständige Suite ist `-m` aktiv; statische Graph-Restore-Auswertung, direkte NuGet-Audits und `RunAnalyzers=false` bleiben auf die frühere fokussierte PR-Variante beschränkt. Der aktuelle PR-Gate-Testumfang wurde nicht reduziert. Das 50-%-Ziel ist mit der vollständigen Suite noch offen.

### Vollständige Pipeline: MSBuild-Projektparallelität

Die PR-Messung erfasst nicht die komplette Zeit bis zum Publish. Deshalb wurde zusätzlich die vollständige Testsequenz auf genau einem GitHub-Runner mit und ohne MSBuild-Max-CPU (`-m`) ausgeführt. Beide Kandidaten liefen mit demselben Quellstand und derselben xUnit-Konfiguration; nur `dotnet test` unterschied sich.

| Remote-Lauf | `dotnet test` | Jobzeit inkl. Setup/Upload | Testfälle | VSTest-Ziel laut Binlog |
| --- | --- | ---: | --- | ---: |
| `44949756912` | `dotnet test Juggling.slnx` | 295 s | 875 bestanden, 3 übersprungen | 178,6 s |
| `44949012996` | `dotnet test Juggling.slnx -m` | 286 s | 875 bestanden, 3 übersprungen | 152,2 s |
| `44958479491` | Einmal `dotnet build Juggling.slnx -m`, danach fünf Testprojekte parallel mit `--no-build --no-restore` | 280 s | 875 bestanden, 3 übersprungen | Teststep 128 s |
| `44959271646` | `dotnet test Juggling.slnx -m`, System-Chrome, kein Browserdownload | 261 s | 875 bestanden, 3 übersprungen | E2E: 50 bestanden |
| `44966854855` | `dotnet test Juggling.slnx -m`, keine installierten Workloads, kein Binlog | 164 s | 875 bestanden, 3 übersprungen | E2E: 50 bestanden |

Mit `-m` fiel der gemessene Testschritt von rund 191 auf 164 s (−14 %); der solutionweite VSTest-Zielpfad im Binlog sank um 14,8 %. Die Jobzeit einschließlich Setup sank in diesem einzelnen A/B-Paar nur um 9 s (−3 %), weil die Setupdauer zwischen den Läufen variierte. Es entstanden keine zusätzlichen Runner. Das belegt einen messbaren, kostengünstigen Gewinn für den vollständigen Testschritt, aber keine Halbierung der Main-bis-Publish-Zeit. `-m` ist in PR und Main aktiviert; alle Tests und die Publish-Sperre bleiben erhalten. Das ursprüngliche 50-%-Ziel mit voller PR-Abdeckung ist damit noch nicht erreicht.

Der Probe-Lauf deckte außerdem auf, dass mehrere Root-Solutions `dotnet test` ohne Argument mehrdeutig machen (MSB1011). Die Workflows adressieren deshalb explizit `Juggling.slnx`; damit bleibt die vollständige Suite eindeutig ausgewählt.

Die Probe lief auf einem Feature-Branch ohne Publish-Schritt. Dieselbe vollständige Testkommandozeile wird nun im PR- und Main-Workflow verwendet. Probe-Artefakte/Binlogs wurden nach der Auswertung nicht in die normale Pipeline übernommen.

Das Projekt-Parallel-Experiment führte alle fünf Testassemblies gleichzeitig aus und bestand die komplette Suite. Der Teststep sank auf 128 s, aber der vollständige Build vor dem Testlauf hob die Jobzeit auf 280 s; gegenüber `dotnet test Juggling.slnx -m` waren das nur 6 s Wandzeitgewinn. Dieser Umbau wird deshalb nicht übernommen. Die aktuelle PR-Pipeline behält den einfacheren vollständigen Solution-Aufruf mit `-m`.

Der System-Chrome-Lauf entfernte den vorherigen E2E-Vorbuild nur, soweit er für den Playwright-Download benötigt wurde, sowie den Browserdownload. `dotnet test` baut die vollständige Solution selbst; die Testauswahl bleibt gleich. Trotz des 5 % längeren E2E-Testfensters sparte der vollständige Job 25 s gegenüber dem Probe-Lauf mit gebündeltem Chromium. Dieser Schritt ist in PR und Main übernommen. Die Formatprüfung bleibt in einem separaten, parallelen PR-Job.

### Prüfung von Test Impact Analysis

`dotnet-tia` 0.10.0 wurde isoliert installiert und mit `analyze` gegen `Juggling.slnx` geprüft. Es wurden keine Tests ausgeführt. Ein CSS-only-Diff in `Siteswaps.Components/CausalDiagram/CausalDiagramEditor.razor.css` sowie die offenen Feature-Änderungen an Komponentenlogik, JavaScript und E2E-Test wurden untersucht. Für den CSS-only-Diff meldete TIA einen vollständigen Lauf mit 683/683 Tests; für den Feature-Diff 686/686. Beide Berichte fielen wegen Kompilierungsfehlern im TIA-Workspace auf „Full run“ zurück. Das ist fail-safe, belegt aber weder die gezielte CSS-Weiterleitung zu Screenshot-/Browser-Tests noch einen Laufzeitgewinn.

Ursache ist die Toolchain: Das Repository pinnt .NET SDK 11.0.100 und zielt auf `net11.0`, TIA 0.10.0 lud hier jedoch MSBuild 10.0.103. Dadurch fehlten im Analysegraph Razor-/Generator-Ausgaben und TIA meldete vier Projekte als nicht kompilierbar. Direkte Builds der betroffenen Generator- und WebAssembly-Projekte mit dem gepinnten SDK 11 waren erfolgreich. Wenn nur SDK 11 sichtbar war, fand TIA kein ladbares MSBuild. Die Analyse ist deshalb für dieses Repository derzeit nicht verwertbar.

Entscheidung: TIA wird nicht in `run`- oder `shadow`-Schritte der Pipeline aufgenommen, bis eine Version die .NET-11/MSBuild-19-Workspaceanalyse unterstützt und ein Shadow-Lauf mit explizit geprüfter Auswahl bestanden ist. Der Feature-Branch behält bis dahin den vollständigen Testsatz. Die getestete Installations- und Analysefolge sowie die Projektbeschreibung stehen im [dotnet-tia-Repository](https://github.com/SebHenn/dotnet-tia).

#### Test Impact Analysis bei vollständiger Feature-Branch-Abdeckung

TIA ist für den gewünschten Zweck derzeit keine getestete Beschleunigung: es meldet wegen der nicht unterstützten SDK-/MSBuild-Kombination jeweils den kompletten Testsatz. Selbst wenn ein künftiger Shadow-Lauf die Auswahl korrekt ermittelt, würde eine TIA-`run`-Auswahl allein Tests auslassen und damit die vom Nutzer geforderte vollständige Feature-Branch-Abdeckung verletzen. Eine sichere spätere Verwendung wäre höchstens als Priorisierung: zuerst die betroffenen Tests ausführen, parallel oder danach weiterhin alle Tests ausführen und die PR erst nach dem vollständigen Lauf freigeben. Für CSS-Änderungen muss der Impact-Graph ausdrücklich die zugehörigen Browser-/Screenshot-Tests enthalten; diese Zuordnung konnte TIA 0.10.0 nicht nachweisen.

### Folgeversuch: vollständige Suite mit Test-Shards und Laufzeit-Artefakt

Um die unveränderte Testmenge auf Feature-Branches zu erhalten und den E2E-Testpfad auf mehrere Runner zu verteilen, wurde ein temporärer Workflow auf `codex/full-suite-shard-probe` erprobt. Er zählte die Testinventare zur Laufzeit und sollte die TRX-Fallzahlen gegen die vollständige Anzahl prüfen (50 E2E und 828 Nicht-E2E im Probelauf). Unit-Tests liefen vollständig im eigenen Job; E2E wurde nach Test-Namensräumen auf zwei Runner verteilt.

| Actions-Lauf | Ergebnis | Beobachtung |
| --- | --- | --- |
| `44962021292` | verworfen | Fehlende Playwright-Node-Runtime im Artefakt; E2E wurde nicht ausgeführt |
| `44962383894` | verworfen | Artefakttransport verlor das Ausführungsbit der Playwright-Node-Runtime |
| `44962683705` | verworfen | Nach Wiederherstellung des Bits fehlten Runtime-Assets der WebAssembly-App; Browserläufe meldeten 404 und `Blazor is not defined` |
| `44962932192` | verworfen | Vollständigeres Build-Artefakt beseitigte die fehlenden Asset-Root-Pfade nicht; E2E blieb auf den 404-Fehlern hängen |
| `44963774479` | abgebrochen | Ein E2E-Projekt-Build-Artefakt mit Static-Web-Asset-Manifests reichte nicht; zwei Shards lieferten keine Ergebnisse innerhalb des brauchbaren Messfensters |
| `44964453773` | abgebrochen | Unit-Job bestand; beide E2E-Shards blieben ohne TRX. Die Manifeste listen zusätzliche Content-Roots aus mehreren `obj`-Ordnern und NuGet-Paketen, die nicht im Artefakt enthalten waren. Der Coverage-Job schlug erwartungsgemäß fehl. |

Die Übergabe eines ausführbaren E2E-Builds zwischen Runnern ist damit nicht zuverlässig nachgewiesen. Das E2E-Hostmodell bindet zur Laufzeit Static-Web-Asset-Manifeste mit vielen außerhalb der gebündelten Ausgabe liegenden Content-Roots ein. Diese Fehlerläufe belegen keinen Zeitgewinn; sie sind keine Grundlage, den Workflow zu übernehmen. Ein erneuter Versuch müsste zuerst ein selbstenthaltenes, reproduzierbar startendes Host-Artefakt liefern und dann einen erfolgreichen Lauf mit genau einmal ausgeführten 878 Tests sowie vollständigen TRX-Coverage-Prüfungen zeigen. Auch dann ist die zusätzliche Runner- und Setupzeit gegen den Actions-Minutenverbrauch zu bewerten.

Der aktuelle Probe-Lauf `44964453773` wurde beendet, nachdem beide E2E-Shards über vier Minuten ohne Ergebnis waren. Es wurden keine lokalen Tests oder Laufzeitmessungen ausgeführt. Der temporäre Workflow und Branch bleiben aus der Produktpipeline ausgeschlossen.

### Folgeversuch: vier balancierte E2E-Shards

Zwei weitere Remote-Proben verteilten die 50 E2E-Fälle anhand der vorherigen TRX-Klassensummen auf vier Gruppen. Die Tests wurden nicht aus dem Feature-Branch entfernt: Der Nicht-E2E-Job führte alle vier Testprojekte aus; die vier E2E-Filter deckten die Klassen der vollständigen E2E-Inventarliste ab.

| Actions-Lauf | Gesamtzeit | Ausgeführte Testfälle | Runner-Zeit, ca. | Befund |
| --- | ---: | ---: | ---: | --- |
| `44965448090` | 192 s | 50 E2E + 828 Nicht-E2E; alle Test-Jobs grün | 746 s | Coverage-Validator meldete fälschlich doppelte Fälle anhand des Test-Anzeigenamens. Mehrere Generator-Theory-Fälle haben denselben Anzeigenamen, aber verschiedene Test-IDs. |
| `44965977535` | 205 s | 50 E2E + 828 Nicht-E2E; alle Test-Jobs grün | ca. 540 s | Einmal bauen und Artefakt weitergeben funktionierte für die Browser-Tests, brauchte aber ca. 391,9 MB und ließ die Laufzeit über 200 s steigen. Der Coverage-Job lud das Inventar-Artefakt nicht und schlug deshalb fehl. |

Der erste Lauf reduzierte die Wandzeit gegenüber der 267-s-Baseline nur um ca. 28 %, weit unter den geforderten 50 %. Eigenständiger Build pro Shard wiederholte Workload- und Build-Arbeit und erhöhte den Actions-Minutenverbrauch auf etwa das 2,8-Fache der einzelnen Baseline-Ausführung. Beim zweiten Lauf reduzierte die einmalige Kompilierung zwar wiederholte Builds, doch der 391,9-MB-Transfer und die serielle Abhängigkeit der Test-Shards führten zu einer längeren kritischen Laufzeit. Auch dieser Aufbau wird nicht übernommen.

Die Result-Logs belegen 50 bestandene E2E-Fälle sowie 825 bestandene und drei übersprungene Nicht-E2E-Fälle. Die Workflowruns blieben wegen der Coverage-Prüfungsfehler rot; das sind keine akzeptierten vollständigen Pipeline-Erfolge. Die Artefakte und Ergebnisse wurden ausschließlich von GitHub Actions abgerufen; lokal wurden keine Tests ausgeführt. Der temporäre Workflow und Branch `codex/e2e-balanced-shards-probe` sind nur Proben und gehören nicht zur Produktpipeline.

### Bewertung von `dotnet-tia` für Feature-Branches

Die CSS-only-Analyse meldete 683 von 683 Tests als vollständigen Lauf; die Analyse des offenen Logik-, JavaScript- und E2E-Änderungssatzes meldete 686 von 686. TIA 0.10.0 belegte damit weder für CSS-/Screenshot-Tests noch für E2E- oder normale Logiktests eine präzise Auswahl. Der gemeinsame Grund war der MSBuild-Workspacefehler unter .NET 11. Das Tool ist für diese Repository-Toolchain vorerst kein Laufzeithebel.

Ein künftiger Shadow-Modus kann nach einer .NET-11/MSBuild-19-kompatiblen TIA-Version protokollieren, welche Kategorien betroffen wären. CSS, Razor, JavaScript, static web assets, Screenshots, Test-Fixtures und unbekannte Dateitypen müssen dabei konservativ Browser-, Design- und betroffene Logiktests markieren. Für die vom Nutzer geforderte vollständige Feature-Branch-Abdeckung müsste TIA höchstens ausgewählte Tests zuerst zur schnelleren Rückmeldung starten; der vollständige Testsatz bleibt anschließend weiterhin verpflichtend. Das verbessert die Zeit bis zum ersten Testergebnis, verkürzt aber den gesamten Gate-Lauf nicht automatisch.

### Aktueller Zielstatus

### Weglassen der WebAssembly-Workload-Installation

Auf GitHub Actions wurde Lauf `44966854855` ohne `dotnet workload install wasm-tools` ausgeführt. `dotnet workload list` zeigte vor dem Build keine installierten Workloads. Trotzdem bestand `dotnet test Juggling.slnx -m`: 197 MCP-, 128 Core-, 490 Generator-, 10 Design- und alle 50 E2E-Tests bestanden; drei Generatorfälle waren übersprungen. Der vollständige Job benötigte 164 s. Ein separater Remote-Lauf `44967347978` veröffentlichte außerdem `Webassembly/Webassembly.csproj` mit denselben Release-/GHPages-Parametern wie der Main-Publish ohne installierte Workloads erfolgreich.

Der frühere vollständige Lauf `44959271646` maß 42,8 s für die Workload-Installation. Das Weglassen dieses Schritts spart diese klar abgegrenzte Zeit und vermeidet mindestens rund 260 MB Emscripten-Pakete. Der 164-s-Lauf lag 37 % unter jenem 261-s-Lauf, aber der frühere Lauf schrieb einen MSBuild-Binlog und die Runnerzeiten schwanken. Diese Differenz wird daher nicht vollständig der Workload-Änderung zugeschrieben. Ein Halbierungserfolg ist nicht belegt: 164 s verfehlen den 145-s-Zielwert. Der Workload-Schritt ist aus PR-, Main- und Preview-Workflows entfernt; vollständiger Test und Publish wurden auf GitHub Actions ohne ihn remote validiert.

Nach dieser Messung enthielt die offene Feature-Arbeitskopie zusätzliche zwei Core-Tests und einen neuen E2E-Test. Remote-Lauf `44967803232` führte alle 881 Fälle ohne Workload-Installation aus: 827 Nicht-E2E-Tests bestanden, drei wurden übersprungen, und es gab zwei E2E-Fehler. Der neue Arrow-Drag-Test konnte `localhost:7021` nicht erreichen; der bestehende Mobile-Touch-Test sah nach einer festen 500-ms-Wartezeit keine URL-Änderung. Probe `44968379378` startete dafür testweise Aspire und ließ den Test die Auswahlbedingung abwarten. Der Arrow-Drag-Test bestand danach; der Mobile-Touch-Test meldete weiterhin unveränderte URL. Insgesamt liefen 880/881 Fälle erfolgreich (3 übersprungen), der Testlauf blieb rot und dauerte 279 s, einschließlich 60+ s Aspire-Start. Diese Läufe weisen keinen Workload-abhängigen Fehler nach; sie zeigen jedoch, dass die aktuelle offene Feature-Änderung noch keine grüne vollständige Pipeline liefert. Der Aspire-Start ist nicht in Produktworkflows übernommen.

Der zuvor beste grüne Ein-Runner-Lauf (`44966854855`) dauerte 164 s. Die vollständigen Shard-Aufbauten lagen zunächst bei 192 s und 205 s. Ein späterer Drei-Shard-Lauf ist darunter dokumentiert.

### Navigation bis `DOMContentLoaded`

Ein Remote-Probe-Lauf (`44969573495`) änderte die drei E2E-Navigationen von `Load` auf `DOMContentLoaded`. Die Aufrufer warten danach weiterhin auf sichtbare, app-spezifische Elemente. Der erste Probelauf stoppte vor den E2E-Tests wegen SOP002: Ein neuer Kommentar war im Repository nicht zulässig. Nach Entfernen des Kommentars lief die vollständige Suite erfolgreich: 875 Tests bestanden, drei übersprungen, darunter alle 50 E2E-Tests.

Der korrigierte Lauf benötigte 254 s; die E2E-Assembly meldete 129 s. Der grüne Referenzlauf ohne Workload-Installation (`44966854855`) benötigte 164 s bei 94 s E2E-Testdauer. Die Probe zeigt keinen Zeitgewinn und wird nicht übernommen. Da es sich um je einen Lauf auf wechselnd ausgelasteten GitHub-hosted Runnern handelt, ist die Differenz allein kein kausaler Nachweis, dass `DOMContentLoaded` langsamer ist. Sie reicht aber aus, den Kandidaten ohne wiederholte kontrollierte A/B-Messungen nicht in die Pipeline zu übernehmen.

### MSBuild Static-Graph-Restore

Lauf `44970103219` führte erneut die vollständige Solution-Suite aus, diesmal mit `-p:RestoreUseStaticGraphEvaluation=true`. Alle 875 Tests bestanden, drei wurden übersprungen, einschließlich aller 50 E2E-Fälle. Der Job benötigte 264 s; die E2E-Assembly 136 s. Das ist kein gemessener Laufzeitgewinn gegenüber dem 164-s-Referenzlauf ohne Workload-Installation. Die statische Graph-Auswertung wird deshalb nicht in den Workflow übernommen; der gesamte Job bleibt maßgeblich und die beobachtete Differenz kann Runner-Schwankungen enthalten.

### Vollständige PR-Suite in drei E2E-Shards

Remote-Probe-Lauf `44972678458` prüfte den PR-Pipeline-Kandidaten: alle Nicht-E2E-Tests kamen weiterhin aus `dotnet test Juggling.slnx`; die E2E-Tests liefen parallel in drei Gruppen. `Functional` und `Ux` sind eigene Namensräume; die dritte Gruppe umfasst den Rest des `Siteswaps.E2ETests`-Namensraums (aktuell `Design`). So wird auch eine neue E2E-Kategorie außerhalb dieser beiden Namensräume einem Shard zugewiesen. Die Matrix verteilte die 50 Browserfälle auf 16, 19 und 15 Tests. Der Coverage-Job prüfte alle fünf TRX-Dateien der Testassemblies, die erwarteten Inventargrößen und eindeutige E2E-Test-IDs.

| Messwert | Lauf `44972678458` |
| --- | ---: |
| Gesamter PR-Pipeline-Kandidatenlauf | 132 s |
| Nicht-E2E-Job | 100 s |
| Längster E2E-Shard | 120 s |
| Coverage-Prüfung | 7 s |
| Runnerzeit über alle Jobs | 458 s |
| Testergebnis | 875 bestanden, 3 übersprungen; 50/50 E2E |

Die Laufzeit liegt 54,5 % unter dem PR-Median von 290 s und 50,6 % unter dem detaillierten 267-s-Vergleichslauf. Damit erreicht dieser Remote-Lauf das 145-s-Ziel bei vollständiger Abdeckung und einer Solution. Der Gesamtverbrauch steigt gegenüber einem einzelnen Runner: Die parallele Pipeline benötigte 458 Runner-Sekunden. Ein früherer Drei-Shard-Lauf mit engeren Namensraumfiltern (`44972243176`) war 121 s / 416 Runner-Sekunden; der hier gemessene Fallback-Filter deckt zusätzliche E2E-Namensräume ab und bleibt ebenfalls über 50 % schneller. Die Vier-Shard-Variante (`44971474202`) benötigte 135 s und 534 Runner-Sekunden.

Die Coverage-Inventare sind absichtlich explizit und fail-closed. Wenn Tests hinzugefügt oder entfernt werden, müssen Assembly- bzw. Namespace-Zahlen aktualisiert werden; die Tests selbst laufen durch die Solution- oder Namespace-Filter weiter vollständig. Neue `Functional`-Tests werden durch den Namespace-Filter automatisch einem Shard zugeordnet.

Die aktuell offenen Feature-Änderungen fügen zwei Core-Tests und einen Functional-E2E-Test hinzu. Der Namespace-Filter führt den neuen Browserfall aus; die PR-Coverage-Erwartungen sind inzwischen auf 130 Core-Tests und 17 Functional-E2E-Tests aktualisiert. Die Feature-Proben `44967803232` und `44968379378` blieben wegen E2E-Fehlern rot.

Ein weiterer Remote-Lauf (`44973255824`) verwendete dieselbe Drei-Shard-Topologie auf dem vollständigen offenen Feature-Arbeitsstand. Die Coverage-Prüfung bestätigte **830 Nicht-E2E- und 51 E2E-Fälle**; damit fehlte kein Test. Der Lauf benötigte 136 s und 441 Runner-Sekunden, also 53,1 % weniger Wandzeit als der 290-s-Median. Die Nicht-E2E-Suite bestand (827 bestanden, drei übersprungen); zwei Functional-E2E-Fälle schlugen fehl: der neue Arrow-Drag-Test erreichte den nicht gestarteten Aspire-Host nicht, und der Mobile-Touch-Test beobachtete keine URL-Änderung. Der Workflow endete daher rot, die Testauswahl und Coverage-Prüfung waren aber vollständig. Der Formatter meldete außerdem die neue Arrow-Drag-Testdatei; sie wurde danach lokal formatiert. Es wurde kein vollständiger erneuter Actions-Lauf nach dieser reinen Formatkorrektur ausgeführt.

### Grüner vollständiger Feature-Lauf und E2E-Korrekturen

Der bereinigte Remote-Lauf [`44975960641`](https://github.com/Sopor10/Juggling/actions/runs/44975960641) bestand mit der vollständigen offenen Feature-Änderung, einschließlich der beiden neuen Core-Tests und des Arrow-Drag-Tests. Alle 830 Nicht-E2E- und 51 E2E-Tests liefen; drei Nicht-E2E-Tests waren erwartungsgemäß übersprungen. Formatprüfung und fail-closed Coverage-Gate bestanden.

| Messwert | Lauf `44975960641` |
| --- | ---: |
| Gesamter PR-Workflow | 136 s |
| Nicht-E2E-Job | 105 s |
| Längster E2E-Shard | 126 s |
| Formatprüfung und Coverage-Gate zusammen | 24 s |
| Runnerzeit über alle Jobs | 489 s |
| Testergebnis | 878 bestanden, 3 übersprungen; 51/51 E2E |

Die Wandzeit ist 53,1 % kürzer als der 290-s-PR-Median und erreicht damit das festgelegte 145-s-Ziel. Gegenüber dem detaillierten 267-s-Ein-Runner-Vergleich beträgt die Reduktion in diesem Lauf 49,1 %; der vorherige vollständige grüne Drei-Shard-Lauf `44972678458` lag bei 132 s (50,6 % darunter). Die parallele Ausführung senkt die PR-Wartezeit, benötigt aber mehr summierte Runnerzeit als ein einzelner Job.

Die beiden zuvor roten E2E-Fälle wurden ohne Skip oder Filterkorrektur behoben. `CausalDiagramArrowDragTests` verwendet jetzt `SharedBlazorFixture`/`WizardBrowserSession`, den für die anderen Browsertests üblichen Testhost, statt einen separat gestarteten Aspire-Host auf `localhost:7021` vorauszusetzen. Der Test erzeugt seine beiden Pfeile über die Editoroberfläche und wählt anschließend anhand der dekodierten Graph-ID gezielt den zu bewegenden Pfeil. Beim mobilen Drag lagen Selbstschleifen-Griff und Wurfknoten zuvor am selben Punkt; die Selbstschleifen-Geometrie platziert den Griff jetzt am sichtbaren Pfeilende außerhalb des Touch-Ziels. Der Mobile-Test und der Arrow-Drag-Test bestanden danach im vollständigen Functional-Shard.

Für Feature-Branches bleibt die Testauswahl vollständig: die Solution führt alle Nicht-E2E-Tests aus, alle E2E-Namensräume werden genau einem der drei Shards zugeordnet, und der Coverage-Job prüft Testinventar sowie eindeutige IDs. `dotnet-tia` 0.10.0 wird nicht als selektiver Gate-Schritt eingesetzt: die dokumentierten CSS-, Logik- und E2E-Analysen fielen wegen der inkompatiblen .NET-11-/MSBuild-Workspace-Unterstützung auf den vollständigen Testsatz zurück. Eine spätere TIA-Nutzung müsste zunächst CSS-/Screenshot- und Browser-Impact-Zuordnungen in einem Shadow-Lauf belegen; sie dürfte die vollständige Feature-Branch-Suite nicht ersetzen.

### Abgleich mit dem aktuellen Main-Stand

Der erfolgreiche Main-Run [`45837149474`](https://github.com/Sopor10/Juggling/actions/runs/45837149474) vom 26. November 2026 enthält inzwischen 197 MCP-, 136 Core-, 510 Generator- (davon 3 übersprungen), 10 Design- und 53 E2E-Testfälle. Der aktualisierte PR-Workflow prüft deshalb 853 Nicht-E2E- und 53 E2E-Fälle. Die zuvor gemessene 136-s-Drei-Shard-Ausführung deckte 51 E2E-Fälle ab; sie belegt die aktuelle Testinventar-Abdeckung nicht. Für die aktualisierte Pipeline muss ein grüner Remote-Lauf mit allen 906 Fällen abgewartet werden.
