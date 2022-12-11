using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TimerModifyMesh : BaseMeshEffect
{
    public float _textspacing = 1f;

    public float spacing = 0;
    public override void ModifyMesh(VertexHelper vh)
    {
        List<UIVertex> vertexs = new List<UIVertex>();
        vh.GetUIVertexStream(vertexs);
        int vertexIndexCount = vertexs.Count;
        for(int i = 0; i < vertexIndexCount; i++)
        {
            UIVertex v = vertexs[i];
            if (i % 6 <= 1)
            {
                v.position += new Vector3(spacing * 1, 0, 0);
            }
            if (i % 6 == 5)
            {
                v.position += new Vector3(spacing * 1, 0, 0);
            }

            vertexs[i] = v;
            if (i % 6 <= 2)
            {
                vh.SetUIVertex(v, (i / 6) * 4 + i % 6);
            }
            if (i % 6 == 4)
            {
                vh.SetUIVertex(v, (i / 6) * 4 + i % 6 - 1);
            }
        }
    }
}
