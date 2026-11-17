# Executável Windows

## Gerar novamente no Unity

1. Salve as alterações das cenas e saia do modo Play.
2. Abra **Redline → Gerar executável Windows (64 bits)**.
3. Aguarde o Console confirmar que a build ficou pronta em `Builds/Windows`.

O comando gera `Redline.exe` para Windows x64, sem Development Build, usando Mono. Inclui MenuInicial e Fase01 a Fase06, nessa ordem. Copia os avisos das músicas e um LEIA-ME para a distribuição.

## Jogar e compartilhar

Abra `Builds/Windows/Redline.exe`. Para enviar a outra pessoa, compacte e envie a pasta Windows inteira. O executável precisa de `Redline_Data`, `UnityPlayer.dll`, `MonoBleedingEdge` e dos demais arquivos gerados ao lado dele. A pessoa deve extrair tudo antes de abrir o jogo; não precisa instalar o Unity.

A pasta Builds é ignorada pelo Git. O comando de build fica em `Assets/Editor/WindowsBuild.cs` para permitir reproduzir a geração. A versão entregue deve ser testada no computador de destino, incluindo áudio e controle Xbox físico.

## Primeira geração

Gerada a partir de uma cópia isolada dos Assets, Packages e ProjectSettings atuais, preservando o Unity aberto no projeto principal. Log: `Logs/windows-build-unity.log`. A cópia usa as configurações de produto do projeto, não as preferências separadas dos testes anteriores.


Resultado: build concluída com sucesso, sem avisos do Unity (366.477.352 bytes no resumo). Executável iniciado com Direct3D 12 e Windows.Gaming.Input; o log da segunda execução confirma carregamento da Fase 1 e ativação dos encontros, sem exceções registradas. A captura nativa da janela não respondeu, por isso não houve conferência visual completa nem uma partida completa nesta build. O jogo foi deixado aberto para teste pelo usuário.

Pacote de distribuição: `Builds/Redline-Windows-x64.zip`. Exclui a pasta de símbolos `Redline_BurstDebugInformation_DoNotShip`, mantendo os arquivos necessários ao jogo. Extraia o ZIP e abra `Redline-Windows/Redline.exe`.


## Versão corrigida 1.0.1

Executável em `Builds/Windows-1.0.1/Redline.exe` e pacote em `Builds/Redline-Windows-x64-1.0.1.zip`. A pasta anterior foi preservada. F11, Alt+Enter ou a opção de tela cheia nas configurações alternam o vídeo; em modo janela é possível redimensionar e maximizar.
