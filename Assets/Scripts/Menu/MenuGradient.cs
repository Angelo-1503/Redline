using UnityEngine;
using UnityEngine.UI;
namespace Redline.Menu
{
    public class MenuGradient : MaskableGraphic
    {
        protected override void OnPopulateMesh(VertexHelper vh)
        {
            vh.Clear(); var rect = rectTransform.rect;
            Color dark = new Color(0.015f,0.02f,0.03f,0.52f), clear = new Color(0.015f,0.02f,0.03f,0f);
            vh.AddVert(new Vector3(rect.xMin,rect.yMin),dark,Vector2.zero);
            vh.AddVert(new Vector3(rect.xMin,rect.yMax),dark,Vector2.zero);
            vh.AddVert(new Vector3(rect.xMax*0.25f,rect.yMax),clear,Vector2.zero);
            vh.AddVert(new Vector3(rect.xMax*0.25f,rect.yMin),clear,Vector2.zero);
            vh.AddTriangle(0,1,2);vh.AddTriangle(2,3,0);
        }
    }
}
