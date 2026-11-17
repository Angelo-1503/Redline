# Textura dos projéteis — 12/11/2026

Arte original criada com ImageGen integrado. Arquivo: `Assets/Art/Projectiles/RifleProjectile.png`, RGBA com transparência real. Original gerado preservado em .codex/generated_images. Não houve edição de pixels por scripts.

Aplicada a `Assets/Prefabs/Bullet.prefab` e `Assets/Prefabs/EnemyBullet.prefab`. Jogador dourado/creme, inimigo vermelho por tint do SpriteRenderer. Ordem de desenho 10. Pivô próximo da ponta, acompanhando o collider e a rotação já feita por Bullet.Init.

Importação explicitamente como Sprite 2D único (não cubemap), filtro Point, sem mipmaps, Clamp, sem compressão, maxTextureSize 512. Original 2172x724; importada em 512x171. PPU 658.5654, calculado usando a resolução original porque o Unity preserva o tamanho mundial ao reduzir a textura. Largura visível de aproximadamente 0.62 unidades e altura 0.06 unidades na escala existente de 0.25. Alpha das bordas preservado.

Não foram alterados scripts de gameplay, escala do objeto, círculo de colisão, layers, dano, velocidade, duração, máscaras ou efeitos de impacto/disparo existentes. Os prefabs receberam apenas sprite, cor e sortingOrder.

Validação no Unity 6000.3.27f1 em cópia isolada: transparência, importação, escala real, cores distintas, preservação de escala/hitbox/física/parâmetros, rotação e movimento em oito direções, colisão física e dano configurado. Relatório: Logs/bullet-texture-results.txt. Prévia nativa: Logs/bullet-texture-preview.png. Não substitui avaliação humana durante uma partida.

## Prompt utilizado

Use case: stylized-concept. Asset type: production 2D side-scrolling run-and-gun PROJECTILE SPRITE for REDLINE, a tactical pixel-art game. Create ONE isolated tiny horizontal flying rifle projectile, pointing exactly RIGHT, centered on a truly transparent background. Design as a readable 1990s high quality arcade pixel-art sprite: small bright ivory/white hot leading core, a copper-gold pointed projectile tip at the RIGHT, pale gold middle, and a SHORT tapered amber light streak fading to transparent toward the LEFT. This is the projectile in flight, NOT a cartridge or ammunition inventory icon: NO shell casing, NO brass cartridge case, NO primer, NO bullet belt. Long narrow silhouette, roughly 5:1 width-to-height, logical design about 48 by 10 pixels with deliberate sharp pixel clusters, presented enlarged with crisp nearest-neighbor pixel edges. Restrained highlights and 4-6 colors; clear dark-gold outline only around the solid tip, unobtrusive small luminous tail, no giant bloom. Sprite fits comfortably in center of canvas, occupies about 80 percent of canvas width and 15 percent of canvas height. Exactly one projectile, no variants, no sprite sheet, no UI, no text, no ground, no scene, no watermark, no motion diagonals. No smoke clouds, no stars, no explosion, no realistic photograph, no magic fireball or science-fiction laser. Fully transparent RGBA background outside the narrow sprite.

Sem commit nem push.
