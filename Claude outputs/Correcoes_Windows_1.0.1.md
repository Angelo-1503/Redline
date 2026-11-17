# Correções da versão Windows 1.0.1

- Tela cheia: janela redimensionável/maximizável, modo padrão sem bordas e controle persistido pelo próprio jogo. F11, Alt+Enter e botão nas configurações alternam entre janela e tela cheia. O controle automático do Unity para Alt+Enter foi desligado para evitar duas alternâncias no mesmo frame. A escolha fica em Redline.Display.Fullscreen; preferências antigas de janela do Unity não impedem o novo padrão.
- Mirage: limite inferior opcional na CameraFollow, ligado apenas na Fase03 com minimumViewY=-10.35. A posição desejada e a posição após a suavização respeitam a borda inferior da arte. Mantém a descida que enquadra a van e a transição existente para a arena.
- Inferno: Dark Electronic durante o percurso, Darkling ao receber OnBossActivated do BossSpawner da Fase06. O fade de dois segundos continua encerrando a música antes do desarme da C4.
- HUD: etiqueta do jogador renomeada para BadFallen.
- Marca: arte original RedlineIcon.png em Assets/Art/Branding, com R, personagem de óculos/barba e traço vermelho. Importação Texture2D sem mipmaps, usada nos ícones padrão e de Windows.

## Validação

Teste de integração no Unity 6000.3.27f1, em cópia isolada com preferências separadas: PASS. Cobertura: propriedades de vídeo, importação e referências dos ícones, câmera perto da van em 16:9, 4:3 e 21:9, nome no HUD, música antes/depois do gatilho da arena, volume, fade final e botão de tela cheia. Capturas da Mirage e configurações inspecionadas visualmente.

Relatório: Logs/release-fixes-results.txt. Capturas: Logs/mirage-van-1600.png, Logs/mirage-van-1200.png, Logs/mirage-van-2100.png e Logs/release-settings.png.

Build atualizada: Builds/Windows-1.0.1. ZIP: Builds/Redline-Windows-x64-1.0.1.zip. A versão anterior foi preservada porque estava em execução. Não houve commit nem push.

## Resultado da build

BuildPipeline concluído com sucesso, zero avisos e 366.479.965 bytes no resumo. O executável possui nove recursos de ícone diferentes da versão anterior com o ícone Unity. O log `Logs/windows-1.0.1-player.log` registra FullScreenWindow 1920x1080, Windowed e MaximizedWindow, sem exceções no período observado. A automação de entrada foi interrompida por atividade do usuário e a captura da janela não respondeu; as alternâncias de vídeo foram confirmadas pelo log, sem atribuir os comandos à automação. Não houve nova partida completa até o final nesta build.

Arquivo ZIP verificado por CRC; manifesto com tamanho e SHA-256 em `Logs/windows-1.0.1-package.json`.
