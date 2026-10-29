# SolTempo Mobile-App

## Was die App macht

Sie hört deinem Instrument oder deiner Stimme in Echtzeit zu und erkennt die Note, die du spielst oder singst. Wenn du 7 oder sogar 14 Noten hintereinander richtig singst, feuert dich die App mit Effekten an.

Im zweiten Modul erkennt sie außerdem das Tempo der Musik in Schlägen pro Minute (BPM).

## Kurztipps

- Mit der Menütaste unten links wechselst du zwischen Noten- und BPM-Erkennung.

- Halte dein Instrument nah ans Mikrofon des Geräts, dann misst die App am genauesten. Schalte bei Bedarf die Verstärkung in den Einstellungen ein.

- Tippe auf die Anzeigekarte, um die Erkennung zurückzusetzen und neu zuzuhören.

---

## Tonhöhenerkennung

Ideal zum Solfège-Üben und zum Stimmen von Instrumenten.  
Die App hört das Audiosignal ab und erkennt die Tonhöhe des gespielten oder gesungenen Tons. Sie zeigt die erkannte Note an und macht sichtbar, wie nah du an der Zieltonhöhe bist.  
Einige Optionen, etwa die Verstärkung, kannst du in den Einstellungen anpassen.  
Für die besten Ergebnisse sollte keine Hintergrundmusik laufen, während die App deine Stimme oder dein Instrument erkennt.

### Notenmodus

Zeigt die erkannte Tonhöhe als Notennamen an, dazu eine Stimmanzeige: Sie zeigt, wie weit du über oder unter dem nächsten Halbton liegst.

Unter **Notation** wählst du, wie die Noten benannt werden:

- **Buchstaben** — A B C D..
- **Europäisch** — Sol La Si
- **Amerikanisch** — Sol La Ti
- **Kyrillisch** — До Ре Ми..
- **Zahlen** — 1 2 3..

### Halbtöne

Schalte diese Option ein, um erhöhte und erniedrigte Halbtöne (C#, Eb usw.) als eigene Noten anzuzeigen, statt sie ihren Nachbarn zuzuordnen.


## BPM-Erkennung

Erkennt das Tempo von Musik oder Metronom in Schlägen pro Minute (BPM) im Bereich von 40 bis 260.  
Zeigt den BPM-Wert und eine grafische Darstellung.  
Bei langsamen Tempi erzielst du die besten Ergebnisse, wenn du in den Einstellungen die Verstärkung einschaltest.  
Beachte, dass sich das Tempo innerhalb eines Stücks ändern kann.  
Wirkt der erkannte BPM-Wert ungenau, tippe auf die Anzeigekarte, um die Erkennung neu zu starten.

## Audio

### Quelle

Wähle das Mikrofon oder Audio-Eingabegerät. Mit **Systemstandard** entscheidet das Betriebssystem.

### Verstärkung

Verstärkt das Eingangssignal vor der Analyse um ×2.

---

## Mitwirkende

SolTempo ist ein plattformübergreifendes Open-Source-Projekt unter MIT-Lizenz. Den vollständigen Quellcode findest du auf [GitHub](https://github.com/taublast/SolTempo).

Mit [DrawnUI](https://drawnui.com) für .NET MAUI entwickelt von [Nick Kovalsky](http://taublast.github.io).

Die gesamte App wird auf einer einzigen [SkiaSharp](https://github.com/mono/SkiaSharp)-Zeichenfläche gezeichnet.

Hintergrundbild von [John Matychuk](https://unsplash.com/@john_matychuk).
