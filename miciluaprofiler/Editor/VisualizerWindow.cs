 using UnityEngine;
 using UnityEditor;
 using System.Collections.Generic;
 
     public class VisualizerWindow : EditorWindow
     {
         [SerializeField]
         internal Vector2 m_Scale = new Vector2(1, 1);
         [SerializeField]
         internal Vector2 m_Translation = new Vector2(30, 0);

         HanoiData m_data = new HanoiData();

         [MenuItem("Window/VisualizerWindow")]
         static void Create()
         {
             // Get existing open window or if none, make a new one:
             VisualizerWindow window = (VisualizerWindow)EditorWindow.GetWindow(typeof(VisualizerWindow));
             window.Show();
         }

         public VisualizerWindow()
         {
             m_data.Load("Assets/luaprofiler_jx3pocket.json");
         }

         public void OnGUI()
         {
             Handles.BeginGUI();
             Handles.matrix = Matrix4x4.TRS(m_Translation, Quaternion.identity, new Vector3(m_Scale.x, m_Scale.y, 1));
             Handles.color = Color.green;
             Handles.DrawLine(new Vector3(0, 0), new Vector3(300, 300));
             Handles.DrawSolidRectangleWithOutline(new Rect(new Vector2(100, 100), new Vector2(50, 50)), Color.green, Color.red);

             Debug.LogFormat("time: {0}, window: {1}", Time.time, this.position.ToString());

             Handles.EndGUI();
         }
     }
 