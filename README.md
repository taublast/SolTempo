# SolTempo

Support: [ask your question here](https://github.com/taublast/SolTempo/discussions)

<img width="707px" height="auto" alt="banner" src="https://github.com/user-attachments/assets/a77d8f9c-f39e-4a6e-ac20-57351c4888f5" />

---

Open-source .NET app for iOS, Mac Catalyst, Android and Windows.

Real-time note pitch + BPM detection for voice and instruments. Selectable note notations, streak achievements. Audio gain boost, on-device processing.

[Read the blog article](https://taublast.github.io/posts/SolTempo/) 👈

---

### Install

* [AppStore](https://apps.apple.com/app/soltempo/id6759321593) - install for iPhone and iPad

* [Google Play](https://play.google.com/store/apps/details?id=com.appomobi.soltempo) - install on Android

* Run on Windows and MacCatalyst compiling from source code

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
- Audio settings: choose input device (or System Default) and enable Gain (×2) for low signals  
- App in 8 languages: English, Русский, Français, Deutsch, Español, Italiano, Português, Srpski. Follows the phone language, change it in Settings  
- Works with screen readers: buttons and settings are labelled, the held note and the settled tempo are spoken

<img width="707px" height="auto" alt="banner" src="https://github.com/user-attachments/assets/afea3e16-37cd-4a6e-8783-d050d5ccd13c" />

Tips
- Keep your instrument close to the microphone for best accuracy.  
- For voice pitch detection, avoid background music.

Privacy
SolTempo does not collect, store, or share personal data. Audio analysis happens locally on your device and all data stays on it.

---

Please star ⭐ if you like it!

### Latest Changes

**1.2**

* App in 8 languages: English, Russian, French, German, Spanish, Italian, Portuguese, Serbian
* Follows the phone language, change it in Settings
* Works with screen readers: labelled buttons and settings, the held note and the settled tempo are spoken

**1.1**

* Ready for Android 16
* Requires Android 7.0 or newer
* Back button closes an open panel first
* Glass panels show the live equalizer behind them

**1.0**

* Initial release

### Development Insights:

* Using `SkiaCamera` audio monitoring and audio transform features
* Applying backdrop liquid glass-like SKSL shader in real-time
* Using shaders for animating appear/exit of popups
* Using animated shader for achievement effect
* Built-in shader live editor when running on Windows
* Confetti and equalizer SkiaSharp drawing helpers
* App drawn on a single DrawnUI `Canvas`.
* UI in 8 languages from resx files; switching language rebuilds only the drawn tree inside the same canvas
* Screen reader support with DrawnUI accessibility: labelled controls, live note and tempo announcements

 ### .NET MAUI Libs Stack

* .NET 10 / MAUI 10
* [SkiaSharp](https://github.com/mono/SkiaSharp)
* [DrawnUi for .NET MAUI](https://github.com/taublast/DrawnUi)
* [Plugin.Maui.AppRating](https://github.com/FabriBertani/Plugin.Maui.AppRating)

### Contributing

Contributing to repository is very welcome. 

### Credits

* **Background image** - by [John Matychuk](https://unsplash.com/@john_matychuk).

### Related

* Real-time SKSL shaders: [Filters Camera](https://github.com/taublast/ShadersCamera)

* Transition shaders: [ShadersCarousel](https://github.com/taublast/ShadersCarousel)

---

Made with [DrawnUI for .NET MAUI](https://drawnui.net)

---

## Language-switch restart audit

Checked 2026-10-29 per ~/.claude/skills/language-switch-restart-audit. Result: in-place rebuild variant, designed in: a language change keeps the canvas and replaces only its drawn tree (AudioPage.RebuildForLanguage), stops the old tree's audio, confetti timer and banner fade first, and reopens Settings after the new tree's first frame. Verified on Android (BV8800, Debug and Release).
