# App móvil SolTempo

## Qué hace

Escucha tu instrumento o tu voz en tiempo real e identifica la nota que tocas o cantas. Cuando cantas bien 7 o incluso 14 notas seguidas, te anima con efectos visuales.

En el segundo módulo también detecta el tempo de la música en pulsaciones por minuto (BPM).

## Consejos rápidos

- Cambia entre la detección de notas y la de BPM con el botón de menú de abajo a la izquierda.

- Acerca tu instrumento al micrófono del dispositivo para obtener lecturas más precisas. Si hace falta, activa la ganancia en los ajustes.

- Toca la tarjeta de la pantalla para reiniciar la detección y volver a escuchar desde el principio.

---

## Detección de notas

Ideal para practicar solfeo o afinar un instrumento.  
Escucha la entrada de audio y detecta la altura del sonido que tocas o cantas. Muestra el nombre de la nota y te indica visualmente cuánto te acercas a la altura correcta.  
Algunas opciones, como la ganancia, se pueden cambiar en los ajustes.  
Para obtener mejores resultados, asegúrate de que no suene música de fondo mientras se detecta la afinación de tu voz o instrumento.

### Modo notas

Muestra la altura detectada como nombre de nota, con un indicador de afinación que te dice cuánto estás por encima o por debajo del semitono más cercano.

Cambia **Notación** para elegir cómo se nombran las notas:

- **Letras** — A B C D..
- **Europea** — Sol La Si
- **Americana** — Sol La Ti
- **Cirílica** — До Ре Ми..
- **Números** — 1 2 3..

### Sostenidos

Actívalo para mostrar los sostenidos y bemoles (C#, Eb, etc.) como notas propias en lugar de agruparlos con sus vecinas.


## Detección de BPM

Pensado para detectar el tempo de la música o de un metrónomo en pulsaciones por minuto (BPM), entre 40 y 260.  
Muestra el BPM y una representación visual.  
Con tempos lentos obtendrás mejores resultados si activas la ganancia en los ajustes.  
Ten en cuenta que el tempo puede cambiar dentro de una misma canción.  
Si el BPM detectado no parece correcto, toca la tarjeta de la pantalla para reiniciar la escucha.

## Audio

### Fuente

Elige el micrófono o el dispositivo de entrada de audio. Elige **Predeterminado del sistema** para que decida el sistema operativo.

### Ganancia

Aplica una ganancia de +5 para reforzar la señal de entrada antes del análisis.

---

## Créditos

Este es un proyecto multiplataforma de código abierto con licencia MIT. Encuentra el código fuente completo en [GitHub](https://github.com/taublast/SolTempo).

Creado con [DrawnUI](https://drawnui.com) para .NET MAUI por [Nick Kovalsky](http://taublast.github.io).

Toda la app se dibuja en un único lienzo de [SkiaSharp](https://github.com/mono/SkiaSharp).

Imagen de fondo de [John Matychuk](https://unsplash.com/@john_matychuk).
