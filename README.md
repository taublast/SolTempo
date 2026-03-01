# SolTempo

Support: [ask your question here](https://github.com/taublast/SolTempo/discussions)

<img width="707px" height="auto" alt="banner" src="https://github.com/user-attachments/assets/a77d8f9c-f39e-4a6e-ac20-57351c4888f5" />

---

Open-source .NET app for iOS, Mac Catalyst, Android and Windows.

Real-time note pitch + BPM detection for voice and instruments. Selectable note notations, streak achievements. Audio gain boost, on-device processing.

---

SolTempo listens to your instrument or voice in real time and shows the musical note (pitch) you’re playing or singing. 

Switch to the secondary module to detect music tempo (BPM) and get an instant read with an appealing visual display.

Designed for solfeggio practice, ear training, and tuning, SolTempo also adds encouraging effects when you hit 8 and even 14 consecutive correct notes.

Features
- Real-time note pitch detection for voice and instruments  
- Tuning indicator shows how sharp/flat you are relative to the nearest semitone  
- Multiple note notations: Letters (A B C…), European (Sol La Si), American (Sol La Ti), Cyrillic (До Ре Ми…), Numbers (1 2 3…)  
- Sharp/flat semitones option (C#, Eb, etc.)  
- BPM / tempo detection (approx. 40–260 BPM) for music or metronome  
- Tap to reset the current module and start listening from scratch  
- Audio settings: choose input device (or System Default) and enable Gain (+5) for low signals

Tips
- Keep your instrument close to the microphone for best accuracy.  
- For voice pitch detection, avoid background music.

Privacy
SolTempo does not collect, store, or share personal data. Audio analysis happens locally on your device and all data stays on it.

---

Please star ⭐ if you like it!

Read the blog article - COMING SOON 👈

### Latest Changes

* Initial release

### Install

* AppStore - IN REVIEW

* Google Play - IN REVIEW

* Run on Windows and MacCatalyst compiling from source code

<img width="707px" height="auto" alt="banner" src="https://github.com/user-attachments/assets/c236479f-80ec-4279-849c-6a8ae3aaaf24" />

### Development Insights:

* Using `SkiaCamera` audio monitoring and audio transform features
* Applying backdrop liquid glass-like SKSL shader in realtime
* Using shaders for animating appear/exit of popups
* Uisng animated shader for achievement effect
* Built-in shader live editor when running on Windows
* Confetti and equalizer SkiaSharp drawing helpers
* App drawn on a single DrawnUI `Canvas`.

 ### .NET MAUI Libs Stack

* [SkiaSharp](https://github.com/mono/SkiaSharp)
* [DrawnUi for .NET MAUI](https://github.com/taublast/DrawnUi)
* [Plugin.Maui.AppRating](https://github.com/taublast/FastPopups)

### Contributing

Contributing to repository is very welcome. 

### Credits

* **Background image** - by [John Matychuk](https://unsplash.com/@john_matychuk).

### Related

* Realtime SKSL shaders: [Filters Camera](https://github.com/taublast/ShadersCamera)

* Transition shaders: [ShadersCarousel](https://github.com/taublast/ShadersCarousel)

---

Made with [DrawnUI for .NET MAUI](https://drawnui.net)

---


