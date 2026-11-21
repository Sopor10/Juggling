# CI-Testlaufzeit

## Ziel und Messrahmen

Ziel ist, die Laufzeit der PR-Testpipeline in GitHub Actions um mindestens 50 % zu senken. Referenzwert ist die mediane Dauer erfolgreicher `PR Validation`-Läufe auf `ubuntu-latest`: 290 s (18 Läufe, 238–815 s). Der Median ist ein historischer Richtwert; der Lauf vom 20. November 2026 liefert die detaillierte Phasenbaseline.

Die VM dient als günstiger Screening-Punkt. Aussagen über den CI-Gewinn werden anhand von GitHub-hosted Actions-Läufen getroffen. Für die Messung werden Gesamtdauer, Schrittzeiten, TRX-Testzeiten und bei Bedarf der MSBuild-Binlog betrachtet. Laufzeitmessungen mit aktiviertem Binlog werden als Diagnostik markiert, da sie leicht von einem normalen Lauf abweichen können.

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

Im Lauf bestanden 875 Tests, drei wurden übersprungen. Der E2E-Testlauf ist der größte einzeln ausgewiesene Testanteil. `Siteswaps.E2ETests/xunit.runner.json` begrenzt ihn absichtlich auf serielle Test-Collections, um Chromium-Abstürze auf der 8-GB-Entwicklungs-VM zu vermeiden.

## Änderungen im aktuellen Versuch

| Änderung | Zweck | Messung / Status |
| --- | --- | --- |
| NuGet-Paketcache in `PR Validation` | Wiederholte Paketdownloads und Restore-Arbeit reduzieren | GitHub Actions-Lauf ausstehend; erster Lauf ist ein Cache-Miss |
| xUnit-Runsettings nur in CI: parallele Collections, maximal 4 Threads | Die CI-Runner-Ressourcen nutzen, ohne die konservative lokale Runner-Konfiguration zu ändern | GitHub Actions-Lauf ausstehend; TRX wird zur Prüfung der Testzeiten erzeugt |
| TRX und MSBuild-Binlog als kurzlebiges Actions-Artefakt | Testdauer pro Fall und Build-/Targetkosten nachvollziehbar machen | Eingeführt; Aufbewahrung 3 Tage |

Der erste Vergleich wird gegen den historischen Median und die bekannte serielle E2E-Dauer gestellt. Ein CI-Lauf gilt erst dann als Nachweis für das 50-%-Ziel, wenn die gemessene PR-Jobdauer höchstens 145 s beträgt und alle Tests erfolgreich sind. Falls der erste Lauf das Ziel verfehlt, werden die TRX-Zeiten und der Binlog zur Auswahl eines gezielten nächsten Versuchs verwendet; es werden keine breit angelegten zusätzlichen Actions-Läufe gestartet.

## Messdaten und Reproduktion

- Workflow: `.github/workflows/PR.yml` und `.github/workflows/steps/action.yml`
- Testkonfiguration: `Siteswaps.E2ETests/xunit.runner.json`
- Historischer Actions-Lauf: <https://github.com/Sopor10/Juggling/actions/runs/44829121788>
- CI-Runsettings werden über den VSTest-Aufruf gesetzt; die versionierte lokale Einstellung bleibt dabei unverändert.
