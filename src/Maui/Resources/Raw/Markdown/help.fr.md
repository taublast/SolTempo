# Application mobile SolTempo

## Ce que fait l'application

Elle écoute votre instrument ou votre voix en temps réel et reconnaît la note que vous jouez ou chantez. Des effets vous encouragent quand vous chantez juste 7, voire 14 notes d'affilée.

Son second module détecte aussi le tempo de la musique en battements par minute (BPM).

## Astuces

- Passez de la détection des notes à celle du BPM avec le bouton de menu en bas à gauche.

- Pour une mesure plus précise, rapprochez votre instrument du micro de l'appareil. Activez l'amplification dans les réglages si besoin.

- Touchez la carte d'affichage pour réinitialiser la détection et reprendre l'écoute depuis le début.

---

## Détection de la hauteur des notes

Idéal pour travailler le solfège ou accorder un instrument.  
L'application écoute l'entrée audio et détecte la hauteur du son joué ou chanté. Elle affiche le nom de la note et montre visuellement à quel point vous êtes proche de la hauteur visée.  
Certaines options, comme l'amplification, sont disponibles dans les réglages.  
Pour de meilleurs résultats, évitez toute musique de fond pendant la détection de la voix ou de l'instrument.

### Mode notes

Affiche la hauteur détectée sous forme de nom de note, avec un indicateur d'accordage qui montre de combien vous êtes trop haut ou trop bas par rapport au demi-ton le plus proche.

Changez **Notation** pour choisir comment les notes sont nommées :

- **Lettres** — A B C D..
- **Européenne** — Sol La Si
- **Américaine** — Sol La Ti
- **Cyrillique** — До Ре Ми..
- **Chiffres** — 1 2 3..

### Dièses

Activez cette option pour afficher les dièses et les bémols (C#, Eb, etc.) comme des notes distinctes au lieu de les regrouper avec leurs voisines.


## Détection du BPM

Conçu pour détecter le tempo d'une musique ou d'un métronome en battements par minute (BPM), entre 40 et 260.  
Affiche le BPM et une représentation visuelle.  
Pour les tempos lents, activez l'amplification dans les réglages : vous obtiendrez de meilleurs résultats.  
Notez que le tempo peut varier au cours d'un même morceau.  
Si le BPM détecté semble faux, touchez la carte d'affichage pour relancer l'écoute.

## Audio

### Source

Choisissez le micro ou le périphérique d'entrée audio à utiliser. Choisissez **Micro du système** pour laisser le système décider.

### Amplification

Applique un gain de ×2 pour renforcer le signal d'entrée avant l'analyse.

---

## Crédits

Ce projet multiplateforme est open source, sous licence MIT. Le code source complet est sur [GitHub](https://github.com/taublast/SolTempo).

Conçu avec [DrawnUI](https://drawnui.com) pour .NET MAUI par [Nick Kovalsky](http://taublast.github.io).

L'application entière est dessinée sur un seul canevas [SkiaSharp](https://github.com/mono/SkiaSharp).

Image de fond : [John Matychuk](https://unsplash.com/@john_matychuk).
