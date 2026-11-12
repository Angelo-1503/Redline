# HUD do operador — 12/11/2026

Modelo 3 aprovado: retrato, placa marinho angular, barra contínua vermelha e vida atual/máxima. HealthBarUI mantém o GUID e as referências já serializadas nas seis fases. Oculta somente HealthBarBG antigo; não modifica Canvas/GameOver. Novo Canvas responsivo 1920x1080, margem de área segura, sem interceptar mouse, abaixo de aberturas/pausa/créditos. Até 25% de vida, moldura e números ficam vermelhos; cura remove o alerta.

Chefe encontrado inclusive inativo; cartão no canto superior direito só aparece quando ele está ativo/vivo. Usa recorte da cabeça/torso dos sprites reais e o nome de cada chefão. Jogador usa arte derivada do conceito aprovado, Resources/UI/OperatorPortrait.png. Retrato gerado com ImageGen integrado, com a prancha aprovada como referência; arquivo original preservado.

TacticalPlate desenha placas angulares por geometria de UI. Nenhuma alteração nas regras de vida, dano, dificuldade, áudio ou cenas/prefabs.

Compilação independente com referências locais do Unity aprovada. Conferência visual/Play Mode ainda necessária na versão 6000.3.27f1 do projeto. Sem commit/push.
