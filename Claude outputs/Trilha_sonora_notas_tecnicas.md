# Trilha sonora

- Fases 1 a 5 e percurso da Fase 6: Dark Electronic, de BaoshunMusic (Pixabay).
- Arena do FalleN na Fase 6: Darkling, de Kevin MacLeod (incompetech.com), CC BY 4.0. A troca ocorre pelo evento OnBossActivated do BossSpawner da cena.
- Os arquivos completos estão em `Assets/Audio/Music`, com importação estéreo, streaming e compressão Vorbis. Nenhum download é necessário durante o jogo.
- Referências explícitas nas seis cenas. Música 2D em loop, volume base de 0,32, multiplicado pelo volume de música das configurações. O volume geral continua no AudioListener.
- Pausa e Game Over continuam usando o bloqueio de áudio já existente.
- Ao iniciar o final após a morte do chefão na Fase 6, a música some em dois segundos e para. O multiplicador de fade é preservado quando o usuário altera o volume, evitando que a música reapareça durante o desarme. Os efeitos e a C4 mantêm o fluxo existente.
- Menu inicial: Decisions, de Kevin MacLeod, CC BY 4.0, com volume base de 0,25. Usa o mesmo controle de música das configurações. O menu de pausa não inicia a trilha do menu inicial. Não há música sobre o texto final nem sobre os créditos finais.
- Créditos das três músicas incluídos no recurso `RedlineCredits.txt`. Fontes, licenças e hashes dos MP3 registrados em `Assets/Audio/Music/MusicLicenses.txt`. Incluir esse aviso na distribuição Windows junto aos demais avisos de terceiros; não redistribuir a faixa do Pixabay isoladamente como biblioteca de áudio.

## Validação

Teste de integração em uma cópia isolada do projeto no Unity 6000.3.27f1: RESULT PASS, sem erros de compilação ou execução no teste. Relatório em `Logs/music-results.txt` e log em `Logs/music-unity.log`. A validação automatizada confere importação, referências das cenas, execução em loop, configurações de volume, pausa e transição para a C4. O equilíbrio sonoro por audição deve ser conferido em uma partida pelo usuário.

A build Windows não foi gerada nesta etapa. Nenhum commit ou push realizado.

## Revisão: menu e remoção da voz

A voz de abertura foi removida das seis fases, do menu, do gerador de cena e das referências nas cenas de recuperação. O MP3 e seu metadado foram removidos. O botão de prévia passou a se chamar “Testar efeitos” e usa um sinal curto sintetizado pelo próprio código, controlado pelo volume de efeitos. Decisions fica em um MusicManager separado da fonte da prévia, na câmera da cena do menu. O gerador de menu reproduz essa configuração.

Validação desta revisão: `Logs/menu-music-results.txt` e `Logs/menu-music-unity.log`.

Resultado da revisão: PASS. Verificados reprodução de Decisions, alteração/mute/cancelamento e persistência do volume, sinal de teste independente, troca para música da fase, configurações durante a pausa e retorno ao menu com o volume salvo. Sem erros de compilação ou execução no teste.
