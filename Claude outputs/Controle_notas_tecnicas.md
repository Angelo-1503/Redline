# Suporte a controle Xbox

## Comandos

| Entrada | Ação |
| --- | --- |
| Analógico esquerdo ou direcional | Mover; cima/baixo também orientam a mira |
| Analógico direito | Mirar em oito direções, inclusive enquanto se move no sentido oposto |
| A | Pular / confirmar nos menus |
| RT ou X | Atirar |
| B | Descer da plataforma / voltar nos menus |
| Menu/Start | Pausar e retomar |
| Analógico direito nos créditos | Rolar o texto |

O controle pode ser conectado durante a partida. Teclado e mouse continuam disponíveis com os atalhos configuráveis existentes. O mapeamento do controle é fixo nesta versão; a tela de configurações mostra os comandos Xbox.

## Implementação

- `ControllerInput` utiliza o pacote Input System já instalado, com zona morta nos analógicos e detecção de desconexão.
- `GameInput` combina teclado/mouse e controle respeitando o bloqueio durante a pausa.
- `ControllerUIInput` integra o gamepad ao `StandaloneInputModule` existente e recupera o foco quando necessário. Os sliders mantêm o foco ao ajustar o volume para esquerda/direita.
- Menus principal, configurações, dificuldade, créditos, pausa e Game Over aceitam navegação pelo controle.
- A pausa ignora a entrada que a abriu no mesmo frame, evitando fechamento imediato. A retomada mantém o bloqueio de ações no frame de fechamento.
- Nenhuma cena, configuração de teclas salva ou mecânica de projéteis foi alterada para adicionar o controle.

## Validação

Executado no Unity 6000.3.27f1 em uma cópia isolada do projeto, com Gamepad virtual do Input System e preferências separadas das do usuário. Resultado: PASS.

Cobertura: movimento analógico, direcional e diagonais, oito direções de mira, pulo, criação de projétil, menus, volume, cancelamento da captura de teclas, pausa/retomada, desconexão/reconexão, reinício após morte preservando dificuldade, volta ao menu e rolagem dos créditos. A imagem das configurações foi inspecionada visualmente.

Artefatos locais ignorados pelo Git: `Logs/controller-results.txt`, `Logs/controller-unity.log` e `Logs/controller-settings.png`.

Ainda precisa de teste manual com controle Xbox físico, inclusive no executável Windows. Nenhum commit ou push foi realizado.
