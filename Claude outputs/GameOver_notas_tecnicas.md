# Game Over — 12/11/2026

Tela original no estilo do menu: arte própria do operador abatido à direita, área escura para leitura à esquerda, grafite/marinho/vermelho, título GAME OVER, fase/mapa/dificuldade, botões angulares Tentar novamente e Voltar ao menu inicial. Interface construída por código, arte sem texto; sem modelos prontos de UI.

GameOverUI mantém o GUID existente e a referência gameOverPanel das seis cenas. O painel antigo permanece oculto. Ao morrer, congela a simulação, libera cursor, abre um Canvas responsivo acima do HUD/pausa (ordem 3000), faz fade de 0.7 s em tempo não escalado e suspende o áudio da fase depois de reduzir seu volume. Foco inicial no botão Tentar novamente; navegação por mouse/teclado via EventSystem. Botões bloqueados durante entrada/carregamento; cliques repetidos não duplicam carregamento. Falta de cena no build mostra mensagem.

Reinício restaura Time.timeScale/AudioListener e mantém GameDifficulty. Retorno carrega MenuInicial com cursor livre. Canvas/evento limpos na saída. Não altera dano, fases, efeitos existentes nem adiciona música.

Arte: Assets/Resources/UI/RedlineGameOverBackground.png, criada por ImageGen integrado a partir de Assets/Art/Menu/RedlineMenuBackground.png como referência de identidade, vestimenta, estilo e cenário. Cena nova: operador sentado exausto em ruínas, cabeça baixa, arma apoiada, pôr do sol discreto e fumaça; esquerda escura para interface; sem texto, UI, sangue ou marcas d'água. Original gerado preservado em .codex/generated_images.

TacticalPlate: adicionada a dependência explícita CanvasRenderer para desenhar corretamente as placas angulares, também utilizada pelo HUD. Nenhum GUID de script alterado.

VALIDAÇÃO: Unity 6000.3.27f1, projeto isolado gameover_validation, preferências isoladas CodexValidation/RedlineGameOverValidation. 17 verificações Play Mode aprovadas: morte, congelamento, conclusão do fade com timeScale zero, pausa de áudio, arte carregada, botões/CanvasRenderer, foco inicial, reinício com vida cheia e dificuldade preservada, nova morte após reinício, retorno ao menu, ausência de Canvas residual e cursor livre. Botões acionados pelo teste via onClick; não houve interação física de teclado/mouse. Compilação aprovada. Renderizações nativas em Results/game-over-16x9.png, game-over-4x3.png e game-over-ultrawide.png conferidas visualmente; screenshots usam câmera offscreen com escala equivalente ao CanvasScaler. Não é uma nova validação completa das seis fases.

Sem commit/push.
