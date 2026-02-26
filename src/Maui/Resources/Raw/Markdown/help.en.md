# SolTempo Mobile App

## What It Does

Listens to your instrument or voice in real time and identifies the musical note you are playing or singing. Shows encouraging effects when correctly singing 7 or even 14 consecutive notes in a row.

Will also detect the beats per minute (BPM) tempo of the music being played if switched to a secondary module.

## Quick Tips

- Switch notes/bpm detection modules by tapping on the lower left menu button.

- Hold your instrument close to the device microphone for the most accurate readings. You can also turn ON audio gain in settings.

- When you tap on display card the module will reset the detection and start listening from the start.

---

## Note Pitch Detection

Best used for solfeggio training or instrument tuning.  
Listens to the audio input and detects the pitch of the sound being played or sung. It displays the detected pitch as a musical note name, along with a visual representation of how close you are to the target pitch.  
Some options like using audio gain etc can be customised inside settings.  
For best results please make sure no background music is playing while detecting voice/intrument pitch.

### Notes Mode

Displays the detected pitch as a musical note name with a tuning indicator showing how sharp or flat you are relative to the nearest semitone.

Switch **Notation** to choose how notes are labelled:

- **Letters** — A B C D..
- **European** — Sol La Si
- **American** — Sol La Ti
- **Cyrillic** — До Ре Ми..
- **Numbers** — 1 2 3..

### Sharp notes

Enable to display sharp and flat semitones (C#, Eb, etc.) as distinct note names rather than grouping them with their neighbours.


## BPM Detection

Designed for beats per minute (BPM) detection of music or metronome inside a 40-260 range.  
Displays BPM and a visual representation.  
For slower tempos you can get best results by turning on Gain inside settings.
Please note that music tempo can vary over time inside same composition.  
If detected BPM seems inaccurate, tap on the display card to reset the listener.

## Audio Settings

### Audio Source

Select the microphone or audio input device to use. Choose **System Default** to let the OS decide.

### Use Gain

Apply +5 gain to boost input signal before analysis.

---

## Credits

This is a MIT-licenced open-source crossplatform project, find full source code on [GitHub](https://github.com/taublast/SolTempo).

Crafted with [DrawnUI](https://drawnui.com) for .NET MAUI by [Nick Kovalsky](http://taublast.github.io).

App drawn on a single [SkiaSharp](https://github.com/mono/SkiaSharp) canvas.

Background image by [John Matychuk](https://unsplash.com/@john_matychuk).

