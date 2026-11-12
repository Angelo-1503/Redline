# Pausa — 12/11/2026

Esc abre pausa nas seis fases; Continuar/Esc retoma. Configurações reaproveitam MainMenuController: áudio, remapeamento, Aplicar/Cancelar/Restaurar. Esc dentro das configurações fecha apenas as configurações; durante captura cancela apenas a captura.

Voltar ao menu pede confirmação avisando perda do progresso da fase. Time.timeScale e AudioListener.pause congelam jogo e áudio; prévia de configurações ignora a pausa do listener. Entrada de gameplay bloqueada durante pausa e no frame de retomada. Cursor restaurado ao continuar.

Instalação automática por sceneLoaded; sem editar cenas. Não abre durante morte, abertura, transições ou final protegidos por invulnerabilidade/controle desabilitado. Não adiciona música.

Validação: compilação independente com referências locais do Unity. Pendente teste visual/Play Mode na versão do projeto (6000.3.27f1). Sem commit/push.
