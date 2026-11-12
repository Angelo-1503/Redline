# Dificuldade — 12/11/2026

Novo jogo abre seleção Fácil / Normal / Difícil, com confirmação Iniciar campanha e Voltar/Esc. Normal pré-selecionado e mantém os valores aprovados. Pausa mostra o modo atual.

Fácil: dano recebido pelo jogador x0.65; dano recebido por inimigos/chefões x1.25.
Difícil: dano recebido pelo jogador x1.35; dano recebido por inimigos/chefões x0.85.
Inteiros arredondados por Mathf.RoundToInt, mínimo 1 para dano positivo. Dano zero/negativo e invulnerabilidade permanecem respeitados. Health identifica Player pela tag e inimigos pelos componentes EnemyBasic/EnemyShooter/BossEnemy. Outros objetos não recebem ajuste.

GameDifficulty guarda a escolha em memória por campanha: mantém nas trocas de fase e Restart; abrir diretamente no Editor inicia em Normal. Escolha confirmada apenas ao iniciar campanha; Voltar não altera a campanha. Sem alterações nos assets de cenas/prefabs, movimentos, quantidade de inimigos, vida máxima ou áudio.

Compilação independente com referências locais do Unity; testes de regras de dano e persistência em processo separado. Pendente Play Mode/validação visual e ajuste humano de Fácil/Difícil. Sem commit/push.
