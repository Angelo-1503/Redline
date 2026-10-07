using System;
using UnityEngine;

namespace Redline.Level
{
    /// <summary>
    /// Cria as plataformas (telhados, lajes, caixotes, muros) de uma tela do
    /// cenário a partir de uma lista simples de números, em vez de montar
    /// cada colisor à mão na cena.
    ///
    /// Fica no mesmo objeto da tela (ex.: Fase01_Tela01). As coordenadas são
    /// LOCAIS à tela: x = 0 é a borda esquerda da imagem e y = 0 é a superfície
    /// da rua. Ou seja, "height" é a altura da plataforma acima da rua, em
    /// unidades (1 unidade = 64 px da arte).
    ///
    /// As plataformas são "de mão única" (PlatformEffector2D): o jogador
    /// atravessa por baixo ao pular e fica em pé ao cair por cima — igual ao
    /// Contra. Ficam na layer "Ground", então o pulo/detecção de chão do
    /// jogador funciona nelas sem mudar nada no PlayerController.
    ///
    /// RAMPAS: marque "slope" e preencha "endHeight" para a plataforma ir em
    /// linha reta de "height" (esquerda) até "endHeight" (direita), como um
    /// corrimão de escada.
    ///
    /// NO EDITOR: com a tela selecionada, cada plataforma aparece como uma
    /// linha verde. Ajuste x, height e width no Inspector até a linha ficar
    /// em cima do telhado/laje no desenho.
    /// </summary>
    public class MapPlatforms : MonoBehaviour
    {
        [Serializable]
        public struct Platform
        {
            [Tooltip("Borda esquerda da plataforma, em unidades a partir da borda esquerda da tela.")]
            public float x;
            [Tooltip("Altura do topo da plataforma acima da rua, em unidades.")]
            public float height;
            [Tooltip("Largura da plataforma, em unidades.")]
            public float width;
            [Tooltip("Marque para fazer uma rampa: a plataforma vai em linha reta da altura 'height' (lado esquerdo) até 'endHeight' (lado direito).")]
            public bool slope;
            [Tooltip("Altura do lado direito da rampa (só usado quando 'slope' está marcado).")]
            public float endHeight;
        }

        [SerializeField] private Platform[] platforms = new Platform[0];
        [SerializeField] private float thickness = 0.2f;
        [Tooltip("Ângulo da superfície que segura o jogador. 160–180 é o comum para plataformas de mão única.")]
        [SerializeField] private float surfaceArc = 160f;
        [SerializeField] private string groundLayerName = "Ground";

        private void Awake()
        {
            int layer = LayerMask.NameToLayer(groundLayerName);
            if (layer < 0)
            {
                Debug.LogWarning($"[MapPlatforms] Layer '{groundLayerName}' não existe — usando a layer da tela.");
                layer = gameObject.layer;
            }

            for (int i = 0; i < platforms.Length; i++)
            {
                Platform p = platforms[i];
                if (p.width <= 0f)
                {
                    continue;
                }

                float rightHeight = p.slope ? p.endHeight : p.height;
                Vector2 start = new Vector2(p.x, p.height);
                Vector2 end = new Vector2(p.x + p.width, rightHeight);
                Vector2 along = end - start;
                float length = along.magnitude;
                float angle = Mathf.Atan2(along.y, along.x) * Mathf.Rad2Deg;
                Vector2 down = Quaternion.Euler(0f, 0f, angle) * Vector2.down;

                GameObject go = new GameObject(p.slope ? $"Rampa_{i}" : $"Plataforma_{i}");
                go.layer = layer;
                go.transform.SetParent(transform, false);
                Vector2 center = (start + end) * 0.5f + down * (thickness * 0.5f);
                go.transform.localPosition = new Vector3(center.x, center.y, 0f);
                go.transform.localRotation = Quaternion.Euler(0f, 0f, angle);

                BoxCollider2D box = go.AddComponent<BoxCollider2D>();
                box.size = new Vector2(length, thickness);
                box.usedByEffector = true;

                PlatformEffector2D effector = go.AddComponent<PlatformEffector2D>();
                effector.useOneWay = true;
                effector.surfaceArc = surfaceArc;
            }
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = new Color(0.2f, 1f, 0.3f, 0.9f);
            foreach (Platform p in platforms)
            {
                Vector3 a = transform.TransformPoint(new Vector3(p.x, p.height, 0f));
                Vector3 b = transform.TransformPoint(new Vector3(p.x + p.width, p.slope ? p.endHeight : p.height, 0f));
                Gizmos.DrawLine(a, b);
                Gizmos.DrawLine(a, a + Vector3.down * 0.15f);
                Gizmos.DrawLine(b, b + Vector3.down * 0.15f);
            }
        }
    }
}
