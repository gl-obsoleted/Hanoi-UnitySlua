using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;

public delegate void HanoiNodeAction(HanoiNode n);

public class HanoiUtil 
{
    public static void ForeachInParentChain(HanoiNode n, HanoiNodeAction act)
    {
        HanoiNode target = n;
        while (target.Parent != null)
        {
            if (act != null)
                act(target);

            target = target.Parent;            
        }
    }

    static public int DrawingCounts = 0;
    static Dictionary<int, Color> m_colors = new Dictionary<int, Color>();
    public static void DrawRecursively(HanoiNode n, float stackHeight, int maxStackLevel)
    {
        int hash = n.GetHashCode();
        Color c;
        if (!m_colors.TryGetValue(hash, out c))
        {
            m_colors[hash] = c = n.GetNodeColor();
        }

        if (!n.HasValidRect())
        {
            n.renderRect = new Rect((float)n.beginTime, stackHeight * (maxStackLevel - n.stackLevel - 1), (float)n.timeConsuming, stackHeight);
        }

        Handles.DrawSolidRectangleWithOutline(n.renderRect, c, n.highlighted ? Color.white : c);

        DrawingCounts++;

        for (int i = 0; i < n.Children.Count; i++)
        {
            DrawRecursively(n.Children[i], stackHeight, maxStackLevel);
        }
    }

    public static void DrawLabelsRecursively(HanoiNode n, float stackHeight, int maxStackLevel, float textBackgroundWidth)
    {
        if (n.highlighted)
        {
            Rect r = n.renderRect;

            r.width = textBackgroundWidth;
            r.height = 45;
            Color bg = Color.black;
            bg.a = 0.5f;
            Handles.DrawSolidRectangleWithOutline(r, bg, bg);

            GUI.color = Color.white;
            Handles.Label(new Vector3(n.renderRect.xMin, n.renderRect.yMin), n.funcName);
            Handles.Label(new Vector3(n.renderRect.xMin, n.renderRect.yMin + 15), n.moduleName);
            Handles.Label(new Vector3(n.renderRect.xMin, n.renderRect.yMin + 30), string.Format("Time: {0:0.000}", n.timeConsuming));
        }

        for (int i = 0; i < n.Children.Count; i++)
        {
            DrawLabelsRecursively(n.Children[i], stackHeight, maxStackLevel, textBackgroundWidth);
        }
    }
}
