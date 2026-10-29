# App SolTempo

## O que o app faz

Ouve seu instrumento ou sua voz em tempo real e identifica a nota que você está tocando ou cantando. Quando você canta afinado 7 ou até 14 notas seguidas, o app comemora com efeitos visuais.

No segundo módulo, também detecta o andamento da música em batidas por minuto (BPM).

## Dicas rápidas

- Alterne entre a detecção de notas e a de BPM pelo botão de menu no canto inferior esquerdo.

- Para leituras mais precisas, deixe o instrumento perto do microfone do aparelho. Se precisar, ative o ganho nas configurações.

- Toque no cartão do visor para zerar a detecção e começar a ouvir de novo.

---

## Detecção de notas

Ideal para treinar solfejo ou afinar instrumentos.  
O app ouve a entrada de áudio e detecta a altura do som tocado ou cantado. Mostra o nome da nota e indica visualmente o quanto você está perto da altura certa.  
Algumas opções, como o ganho, podem ser ajustadas nas configurações.  
Para melhores resultados, garanta que não haja música de fundo enquanto o app detecta a afinação da voz ou do instrumento.

### Modo notas

Mostra a altura detectada como nome de nota, com um indicador de afinação que mostra o quanto você está acima ou abaixo do semitom mais próximo.

Mude **Notação** para escolher como as notas são nomeadas:

- **Letras** — A B C D..
- **Europeia** — Sol La Si
- **Americana** — Sol La Ti
- **Cirílica** — До Ре Ми..
- **Números** — 1 2 3..

### Sustenidos

Ative para mostrar sustenidos e bemóis (C#, Eb etc.) como notas separadas, em vez de agrupá-los com as notas vizinhas.


## Detecção de BPM

Feito para detectar o andamento de músicas ou metrônomos em batidas por minuto (BPM), na faixa de 40 a 260.  
Mostra o BPM e uma representação visual.  
Em andamentos lentos, você obtém melhores resultados ativando o ganho nas configurações.  
Lembre-se de que o andamento pode variar ao longo de uma mesma música.  
Se o BPM detectado parecer errado, toque no cartão do visor para reiniciar a escuta.

## Áudio

### Fonte

Escolha o microfone ou dispositivo de entrada de áudio. Escolha **Padrão do sistema** para deixar o sistema decidir.

### Ganho

Aplica um ganho de +5 para reforçar o sinal de entrada antes da análise.

---

## Créditos

Este é um projeto multiplataforma de código aberto com licença MIT. O código-fonte completo está no [GitHub](https://github.com/taublast/SolTempo).

Feito com [DrawnUI](https://drawnui.com) para .NET MAUI por [Nick Kovalsky](http://taublast.github.io).

O app inteiro é desenhado em um único canvas [SkiaSharp](https://github.com/mono/SkiaSharp).

Imagem de fundo de [John Matychuk](https://unsplash.com/@john_matychuk).
