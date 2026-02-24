# SolTempo  Mobile App

## What It Does

Music Notes listens to your instrument or voice in real time and identifies the musical note you are playing or singing.

On top of that another module can detect the tempo of the music being played, showing the beats per minute (BPM).

## Quick Tips

- Switch notes/bpm detection modules by tapping on the lower left menu button.

- Hold your instrument close to the device microphone for the most accurate readings. You can also turn ON audio gain in settings.

- When you tap on display card the module will reset the detection and start listening from the start.

---

## Pitch Detection Module

best used for solfeggio traiing or instrument tuning.  
This module listens to the audio input and detects the pitch of the sound being played or sung. It displays the detected pitch as a musical note name, along with a visual representation of how close you are to the target pitch.  
Some options can be customised inside settings.

### Notes Mode

Displays the detected pitch as a musical note name with a tuning indicator showing how sharp or flat you are relative to the nearest semitone.

Switch **Notation** to choose how notes are labelled:

- **Letters** — A B C D..
- **European** — Sol La Si
- **American** — Sol La Ti
- **Cyrillic** — До Ре Ми..
- **Numbers** — 1 2 3..

### Detection

Choose between two frequency ranges for pitch detection:

- **Instruments 60–1600 Hz** — suited for most melodic instruments
- **Voice 80–1100 Hz** — optimised for singing

### Sharp notes

Enable to display sharp and flat semitones (C#, Eb, etc.) as distinct note names rather than grouping them with their neighbours.


## BPM Detection Module

Best used for BPM detection of a music or a metronome.  
This module analyses the audio signal to detect the tempo of the music being played. 
It displays the beats per minute (BPM) and a visual representation of the detected tempo.  
Note that tempo of the music can change over time inside same musical composition.
This module gives best results with audio gain turned off.

## Audio Settings

### Audio Source

Select the microphone or audio input device to use. Choose **System Default** to let the OS decide.

### Use Gain

Apply automatic gain to boost quiet input signals before analysis.

---

## Credits

This is a MIT-licenced open-source crossplatform project, find full source code on [GitHub](https://github.com/taublast/AudioNotes).

Crafted with [DrawnUI](https://drawnui.com) for .NET MAUI by [Nick Kovalsky](http://taublast.github.io).

App drawn enterely on a single [SkiaSharp](https://github.com/mono/SkiaSharp) canvas.

Background image by [John Matychuk](https://unsplash.com/@john_matychuk).

