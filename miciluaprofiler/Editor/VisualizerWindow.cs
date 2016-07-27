 using UnityEngine;
 using UnityEditor;
 using System.Collections.Generic;
 
     public class VisualizerWindow : EditorWindow
     {
         [SerializeField]
         internal Vector2 m_Scale = new Vector2(20, 1);
         [SerializeField]
         internal Vector2 m_Translation = new Vector2(0, 0);

         float m_winWidth = 0.0f;
         float m_winHeight = 0.0f;
         float m_stackHeight = 0.0f;

         HanoiData m_data = new HanoiData();

         [MenuItem("Window/VisualizerWindow")]
         static void Create()
         {
             // Get existing open window or if none, make a new one:
             VisualizerWindow window = (VisualizerWindow)EditorWindow.GetWindow(typeof(VisualizerWindow));
             window.Show();
             window.wantsMouseMove = true;
         }

         public VisualizerWindow()
         {
             m_data.Load("Assets/luaprofiler_jx3pocket.json");
         }

         public void OnGUI()
         {
             CheckForResizing();

             CheckForInput();

             Handles.BeginGUI();
             Handles.matrix = Matrix4x4.TRS(m_Translation, Quaternion.identity, new Vector3(m_Scale.x, m_Scale.y, 1));

             //Handles.DrawLine(new Vector3(0, 0), new Vector3(300, 300));
             //Handles.DrawSolidRectangleWithOutline(new Rect(new Vector2(100, 100), new Vector2(50, 50)), Color.green, Color.green);
             //Debug.LogFormat("time: {0}, window: {1}", Time.time, this.position.ToString());

             DrawHanoiData(m_data.Root);

             Handles.EndGUI();
         }

         private void CheckForResizing()
         {
             if (Mathf.Approximately(position.width, m_winWidth) && 
                 Mathf.Approximately(position.height, m_winHeight))
                 return;

             m_winWidth = position.width;
             m_winHeight = position.height;
             m_stackHeight = (m_data.MaxStackLevel != 0) ? (m_winHeight / m_data.MaxStackLevel) : m_winHeight;
         }

         private void DrawHanoiData(HanoiRoot r)
         {
             if (r.callStats == null)
                 return;

             m_drawingCounts = 0;
             float startTime = 0.0f;
             DrawHanoiRecursively(r.callStats, startTime);
             Debug.LogFormat("time: {0}, drawingCounts: {1}", Time.time, m_drawingCounts);
         }

         int m_drawingCounts = 0;
         private void DrawHanoiRecursively(HanoiNode n, float startTime)
         {
             //if (n.stackLevel > 2)
             //    return;

             int hash = n.GetHashCode();
             Color c;
             if (!m_colors.TryGetValue(hash, out c))
             {
                 m_colors[hash] = c = Random.ColorHSV();
             }

             Handles.DrawSolidRectangleWithOutline(new Rect(startTime, m_stackHeight * (m_data.MaxStackLevel - n.stackLevel - 1), n.timeConsuming, m_stackHeight), c, c);
             m_drawingCounts++;

             float accum = startTime;
             for (int i = 0; i < n.Children.Count; i++)
             {
                 //accum += n.Children[i].interval;
                 if (i > 0)
                 {
                     //accum += n.Children[i - 1].interval;
                     accum += n.Children[i - 1].timeConsuming;
                 }

                 DrawHanoiRecursively(n.Children[i], accum);
             }
         }

         public Vector2 ViewToDrawingTransformPoint(Vector2 lhs)
         { return new Vector2((lhs.x - m_Translation.x) / m_Scale.x, (lhs.y - m_Translation.y) / m_Scale.y); }
         public Vector3 ViewToDrawingTransformPoint(Vector3 lhs)
         { return new Vector3((lhs.x - m_Translation.x) / m_Scale.x, (lhs.y - m_Translation.y) / m_Scale.y, 0); }

         public Vector2 DrawingToViewTransformVector(Vector2 lhs)
         { return new Vector2(lhs.x * m_Scale.x, lhs.y * m_Scale.y); }
         public Vector3 DrawingToViewTransformVector(Vector3 lhs)
         { return new Vector3(lhs.x * m_Scale.x, lhs.y * m_Scale.y, 0); }

         public Vector2 ViewToDrawingTransformVector(Vector2 lhs)
         { return new Vector2(lhs.x / m_Scale.x, lhs.y / m_Scale.y); }
         public Vector3 ViewToDrawingTransformVector(Vector3 lhs)
         { return new Vector3(lhs.x / m_Scale.x, lhs.y / m_Scale.y, 0); }

         public Vector2 mousePositionInDrawing
         {
             get { return ViewToDrawingTransformPoint(Event.current.mousePosition); }
         }

         private void CheckForInput()
         {
             if (Event.current.type == EventType.mouseDrag)
             {
                 if (Event.current.button == 1)
                 {
                     m_Translation.x += Event.current.delta.x;
                     Repaint();
                 }
             }

             if (Event.current.type == EventType.scrollWheel)
             {
                 float delta = Event.current.delta.x + Event.current.delta.y;
                 delta = -delta;

                 // Scale multiplier. Don't allow scale of zero or below!
                 float scale = Mathf.Max(0.03F, 1 + delta * 0.03F);

                 // Offset to make zoom centered around cursor position
                 m_Translation.x -= mousePositionInDrawing.x * (scale - 1) * m_Scale.x;

                 // Apply zooming
                 m_Scale.x *= scale;

                 Repaint();
             }
         }

         Dictionary<int, Color> m_colors = new Dictionary<int,Color>();
     }
 