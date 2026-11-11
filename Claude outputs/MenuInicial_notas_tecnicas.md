# Menu inicial do Redline — 11/11/2026

Cena: `Assets/Scenes/MenuInicial.unity`. Abra e pressione Play. A interface é montada em tempo de execução por `MainMenuController`; a cena contém a câmera e o controlador. A cena é a primeira do Build Settings.

## Comportamento

- Novo jogo abre Fase01, preservando as preferências de áudio e teclas.
- Configurações: volume geral, música, efeitos/vozes, prévia com Okay Lets Go, teclas principal e alternativa por ação, incluindo botões do mouse.
- Aplicar salva em PlayerPrefs. Cancelar restaura as preferências anteriores, inclusive o áudio usado na prévia. Restaurar padrões só é persistido depois de Aplicar.
- Esc cancela uma captura de tecla ou fecha Configurações. Backspace limpa um atalho quando há outro disponível para a ação. Teclas duplicadas são recusadas.
- Defaults: A/D/W/S e setas; Espaço para pular; Mouse 1 ou Ctrl esquerdo para atirar; C para descer; baixo + pulo também funciona.
- O áudio da bomba permanece exclusivo da Fase06. O som de passos continua removido. Não foram adicionadas músicas novas; o slider Música já controla o MusicManager quando houver um clipe atribuído.

## Implementação e validação

Interface própria com imagens, tipografia, botões, cores e layout construídos por código, sem prefabs/modelos prontos de interface. Arte original salva em `Assets/Art/Menu/RedlineMenuBackground.png`. Fonte já incluída no projeto: LiberationSans.

Preferências em `Assets/Scripts/Settings/GameSettings.cs`; leitura das teclas em GameInput.cs, integrada a PlayerController e PlayerShooting. Áudio geral via AudioListener e volumes por categoria integrados aos scripts existentes.

Compilação e testes em cópia isolada do Unity 6000.3.27f1: navegação, abertura de Configurações, volumes, prévia, Cancelar, remapeamento de teclado/mouse, conflito de teclas, proteção contra ação sem atalho, persistência de ambos os atalhos (incluindo reutilização de teclas padrão), Restaurar padrões e Novo jogo → Fase01 aprovados. Preferências de teste isoladas sob CodexValidation/RedlineMenuValidation. Relatório: `Logs/menu-results.txt`.

Prévia nativa renderizada: `Logs/menu-home.png`, `Logs/menu-settings.png`, `Logs/menu-settings-4x3.png`, `Logs/menu-home-ultrawide.png`. Conferidos 16:9, 4:3 e ultrawide; arte alinhada no topo para preservar o rosto em telas largas.

## Arte — ferramenta e prompts

Ferramenta utilizada: ImageGen integrado, sem CLI/API externa. Duas etapas: geração original e edição do rosto conforme referência enviada pelo usuário. A foto original não foi copiada para o repositório.

### Prompt da geração

Use case: stylized-concept. Asset type: original main-menu background illustration for the existing 2D tactical run-and-gun game REDLINE. The two input files are reference images only: first is the game's actual protagonist, second shows the level art and environment. Create a NEW cinematic widescreen 16:9 game title-screen painting, intended to be displayed at 1920x1080. Preserve the protagonist identity: brown/tan skin, short dark hair, dark beard, dark protective sunglasses, navy blue tactical armor, black tactical gloves and equipment, black rifle. Reimagine him as a beautifully detailed large pixel-art / hand-painted pixel illustration, standing in a relaxed but alert low-ready pose in the RIGHT half, three-quarter portrait down to thighs, looking slightly left toward the viewer, recognizable from reference. Background is a ruined sandstone city street reminiscent of the reference game's dusty arches, antennas, distant dome and palm, with atmospheric smoke and sunset glints. Sophisticated palette of near-black charcoal, desaturated navy, sandy ochre and restrained crimson accents. Warm rim lighting outlines the protagonist against smoky ruins. Keep the entire LEFT 48 percent deliberately dark, uncluttered and low contrast as usable negative space for a separately drawn game logo and buttons. Art on right should be rich and expressive, calm before battle, with visible deliberate pixel clusters and painterly depth, not a stock military photo and not cute cartoon. Strong silhouette, elegant cinematic composition. Background full bleed; no borders, no text, no typography, no logos, no UI buttons, no watermark, no bombs, no explosions, no other characters. Keep head and weapon away from outer frame edges to allow slight crop on different screen ratios.

### Prompt final da edição de rosto

Use case: identity-preserve / precise-object-edit. Image 1 is the EDIT TARGET, an original REDLINE game-menu background. Image 2 is a FACE REFERENCE only, supplied by the user. Edit ONLY the protagonist's face/head in image 1 to closely resemble the man in image 2, interpreted in the SAME detailed hand-painted pixel-art illustration style of image 1. Match the reference man's broader and softer face shape, higher/receding hairline with short dark hair, beard outline and moustache, fuller cheeks, black squared sunglasses with subtle golden green reflections, and recognizable friendly subtle smile. Preserve the head angle and appropriate warm sunset lighting of image 1. Make him recognizably closer to the reference without turning the head into a pasted photograph. Keep the existing navy tactical uniform, pose, rifle, hands, body, background street, smoke, sunset, colors, composition and especially the dark empty left half EXACTLY as they are. Do not copy the yellow shirt, headset, logos, skull emoji, or microphone from the reference. No text, no UI, no watermark. Output the same widescreen full-bleed image as image 1; change only the face and hair.

Sem commit nem push. Créditos, músicas/SFX que ainda faltam, testes acadêmicos e build Windows continuam etapas separadas.
