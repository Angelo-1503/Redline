using UnityEngine;

namespace Redline.Effects
{
    /// <summary>
    /// Configura e dispara um Particle System via código assim que o objeto é
    /// instanciado — usado pelas partículas de tiro, impacto e morte (mesmo
    /// componente reaproveitado três vezes, só muda os valores no Inspector,
    /// igual fizemos com Health e Bullet). O Particle System em si pode ficar
    /// com as configurações padrão da Unity: este script sobrescreve tudo que
    /// importa (cor, tamanho, velocidade, duração, formato do jato) no Awake().
    /// O próprio Particle System se autodestrói ao terminar (Stop Action).
    /// O componente ParticleSystem é adicionado dinamicamente em código (em vez
    /// de já vir montado no prefab) — assim o prefab fica só com este script,
    /// sem depender de configuração manual de módulos no Inspector.
    /// </summary>
    public class SimpleBurstEffect : MonoBehaviour
    {
        [SerializeField] private Color startColor = Color.white;
        [SerializeField] private float startSize = 0.15f;
        [SerializeField] private float startSpeed = 3f;
        [SerializeField] private float startLifetime = 0.2f;
        [SerializeField] private int burstCount = 8;
        [SerializeField] private float coneAngle = 15f;
        [SerializeField] private float coneRadius = 0.05f;
        [SerializeField] private bool useGravity = false;
        [Tooltip("Material usado pelo renderer da partícula. Use o mesmo material dos sprites do projeto (Sprites-Default) para garantir compatibilidade com a URP.")]
        [SerializeField] private Material particleMaterial;

        private void Awake()
        {
            ParticleSystem ps = GetComponent<ParticleSystem>();
            if (ps == null)
            {
                ps = gameObject.AddComponent<ParticleSystem>();
            }

            ParticleSystemRenderer psRenderer = GetComponent<ParticleSystemRenderer>();
            if (psRenderer != null && particleMaterial != null)
            {
                psRenderer.material = particleMaterial;
            }

            ParticleSystem.MainModule main = ps.main;
            main.duration = Mathf.Max(0.1f, startLifetime);
            main.loop = false;
            main.startLifetime = startLifetime;
            main.startSpeed = startSpeed;
            main.startSize = startSize;
            main.startColor = startColor;
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            main.stopAction = ParticleSystemStopAction.Destroy;
            main.gravityModifier = useGravity ? 1f : 0f;

            ParticleSystem.EmissionModule emission = ps.emission;
            emission.rateOverTime = 0;
            emission.SetBursts(new[] { new ParticleSystem.Burst(0f, (short)burstCount) });

            ParticleSystem.ShapeModule shape = ps.shape;
            shape.shapeType = ParticleSystemShapeType.Cone;
            shape.angle = coneAngle;
            shape.radius = coneRadius;

            ParticleSystem.ColorOverLifetimeModule colorOverLifetime = ps.colorOverLifetime;
            colorOverLifetime.enabled = true;
            Gradient gradient = new Gradient();
            gradient.SetKeys(
                new[] { new GradientColorKey(startColor, 0f), new GradientColorKey(startColor, 1f) },
                new[] { new GradientAlphaKey(1f, 0f), new GradientAlphaKey(0f, 1f) }
            );
            colorOverLifetime.color = gradient;

            ps.Play();
        }
    }
}
