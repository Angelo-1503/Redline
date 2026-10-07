namespace Redline.Factions
{
    /// <summary>
    /// Os dois lados do conflito. A cada fase o jogador assume um lado e
    /// enfrenta apenas inimigos do lado oposto:
    /// fases ímpares → jogador Blue (Vanguarda) contra Reds (Insurgência);
    /// fases pares   → jogador Red (Insurgência) contra Blues (Vanguarda).
    /// </summary>
    public enum Faction
    {
        Blue,
        Red
    }

    /// <summary>
    /// Função do inimigo na onda. As ondas pedem uma função e o
    /// LevelFaction devolve o prefab da facção inimiga correspondente.
    /// </summary>
    public enum EnemyRole
    {
        Melee,
        Shooter
    }

    public static class FactionExtensions
    {
        public static Faction Opposite(this Faction faction)
        {
            return faction == Faction.Blue ? Faction.Red : Faction.Blue;
        }
    }
}
