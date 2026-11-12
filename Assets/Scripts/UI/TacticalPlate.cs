using UnityEngine;
using UnityEngine.UI;

namespace Redline.UI
{
    /// <summary>Placa angular da interface, desenhada na resolução do Canvas.</summary>
    [RequireComponent(typeof(CanvasRenderer))]
    public sealed class TacticalPlate : MaskableGraphic
    {
        public float Cut = 12f;
        protected override void OnPopulateMesh(VertexHelper vh)
        {
            vh.Clear();
            Rect r = rectTransform.rect;
            float c = Mathf.Min(Cut, Mathf.Min(r.width, r.height) * 0.3f);
            Vector2[] p = {
                new Vector2(r.xMin + c, r.yMin), new Vector2(r.xMax - c, r.yMin),
                new Vector2(r.xMax, r.yMin + c), new Vector2(r.xMax, r.yMax - c),
                new Vector2(r.xMax - c, r.yMax), new Vector2(r.xMin + c, r.yMax),
                new Vector2(r.xMin, r.yMax - c), new Vector2(r.xMin, r.yMin + c)
            };
            vh.AddVert(r.center, color, Vector2.zero);
            foreach (Vector2 point in p) vh.AddVert(point, color, Vector2.zero);
            for (int i = 0; i < p.Length; i++) vh.AddTriangle(0, i + 1, (i + 1) % p.Length + 1);
        }
    }
}

