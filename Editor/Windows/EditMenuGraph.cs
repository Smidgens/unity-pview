// smidgens @ github

namespace Smidgenomics.Unity.ProjectView.Editor
{
	using UnityEngine;
	using UnityEditor;
	using System;

	using EGU = UnityEditor.EditorGUIUtility;
	using LZTex = System.Lazy<UnityEngine.Texture>;
	using UnityEditor.Experimental;

	/// <summary>
	/// Mouse button constants
	/// </summary>
	internal static class MouseButton
	{
		public const int
		LEFT = 0,
		RIGHT = 1,
		MIDDLE = 2;
	}

	internal partial class EditMenuGraph : EditorWindow
	{
		public const string
		WINDOW_TITLE = "Edit Menu (Graph)",
		WINDOW_TITLE_DIRTY = WINDOW_TITLE + "*";

		public const float LINE_THICKNESS = 3f;

		// scroll zoom
		public const float
		ZOOM_MIN = 0.25f,
		ZOOM_MAX = 2f,
		ZOOM_STEP = 0.125f,
		ZOOM_DEFAULT = 1f;

		public const float
		DELETE_BTN_SIZE = 13f;

		//public const float CANVAS_SIZE = 10000;

		// node drag
		public const float
		GRID_SNAP = 10;

		public static readonly Vector2
		NODE_SIZE = new Vector2(100f, 20f);

		// 
		public const float
		NODE_WIDTH = 100f,
		NODE_HEIGHT = 20f,
		PORT_SIZE = 10f,
		PORT_OFFSET = 2f,
		NODE_COLOR_WIDTH = 3f;

		public static void Open(PVMenu_Graph p)
		{
			var w = GetWindow<EditMenuGraph>(typeof(SceneView));
			w._Graph = p;
			w.Show();
		}

		private PVMenu_Graph _Graph
		{
			get => _graph;
			set
			{
				if (value == _graph) { return; }
				_graph = value;
				ResetLayout();
			}
		}

		[SerializeField] private PVMenu_Graph _graph = default;

		// node layout rects
		private Rect[] _nodeRects = { };

		//private int _draggedIndex = -1; // node index being dragged
		private Connector _connector = null;
		//private WindowResources _resources = null;
		private ViewportTransform _transform = new ViewportTransform();
		// last node to be pressed
		private MenuNode _pressedNode = null;

		private Tuple<int, MenuNode> _draggedNode = null;


		// transformation per user movement (pan, zoom)
		private class ViewportTransform
		{
			public float rotation { get; set; } = 0f;
			public Vector2 drag { get; set; } = Vector2.zero;
			public float scale { get; private set; } = ZOOM_DEFAULT;

			public Matrix4x4 GetMatrix()
			{
				var s = Vector3.one * scale;
				var r = Quaternion.AngleAxis(rotation, new Vector3(0, 0, 1));
				return Matrix4x4.TRS(drag, r, s);
			}

			public void Zoom(int sign)
			{
				var z = scale + (-sign * ZOOM_STEP);
				scale = Mathf.Clamp(z, ZOOM_MIN, ZOOM_MAX);
			}

			public void Reset()
			{
				drag = Vector2.zero;
				scale = ZOOM_DEFAULT;
				rotation = 0f;
			}
		}

		// connect
		private class Connector
		{
			public MenuNode node;
			public int port;
			public int index;
		}

		private Texture _gridTex = null;

		private void OnEnable()
		{
			//_resources = new WindowResources();
			titleContent.text = WINDOW_TITLE;
			//titleContent.CopyFrom(_resources.TitleContent.Value);
			Undo.undoRedoPerformed += OnUndo;
			ResetLayout();
		}

		private void OnDisable()
		{
			if (_gridTex)
			{
				Resources.UnloadAsset(_gridTex);
				_gridTex = null;
			}

			_draggedNode = null;
			Undo.undoRedoPerformed -= OnUndo;
			_nodeRects = new Rect[0];
		}

		private void OnUndo()
		{
			_nodeRects = new Rect[0];
			EditorApplication.delayCall += Repaint;
		}

		public void Update()
		{
			if (NeedsRepaint()) { Repaint(); }
		}

		// TODO (plz): clean up this horribleness
		private void DrawBackground()
		{
			if (!_gridTex)
			{
				_gridTex = Resources.Load<Texture>(Constants.TEX_PATH_GRID);
			}

			if (!_gridTex) { return; }

			var r = position;
			r.position = new Vector2(0f, 20f); // 20 = header height

			var z = (_transform.scale - ZOOM_MIN) / (ZOOM_MAX - ZOOM_MIN);

			var opacity = Mathf.Lerp(0.1f, 0.2f, z);
			var bgScale = Mathf.Lerp(ZOOM_MAX, ZOOM_MIN, z);
	
			using (new GUI.ClipScope(r))
			{
				var bgRect = r;
				bgRect.position = default;
				bgRect.height = bgRect.width;

				var bgWidth = bgRect.width * bgScale;

				var cellWidth = 50f;

				//var repeat = 2f;
				var repeat = bgWidth / cellWidth;

				var coords = new Rect(0f, 0f, repeat, repeat);

				var dragX = _transform.drag.x;
				var dragY = _transform.drag.y;

				var dragScaleX = -(dragX / bgRect.width) * repeat;
				var dragScaleY = (dragY / bgRect.width) * repeat;

				var constOffset = _transform.scale * cellWidth;

				var cpos = new Vector2(dragScaleX, dragScaleY);
				//coords.position = cpos + Vector2.one * constOffset;
				coords.position = cpos;

				var tColor = GUI.color;
				GUI.color = Color.black * opacity;

				Graphics.DrawTexture(bgRect, _gridTex, coords, 0, 0, 0, 0, Color.black * opacity);
				GUI.color = tColor;
			}


		}

		private void OnGUI()
		{
			if (!_Graph) { return; }

			// refresh
			RefreshTitle();
			// init
			ComputeLayoutRects();
			// pre-events
			ProcessCanvasDrag(Event.current);
			ProcessZoomEvents(Event.current);
			ProcessNodeDrag(Event.current);

			using (new GUIScope.Unclip(GUI.matrix))
			{
				DrawBackground();

				// scale etc
				ApplyTransformMatrix();

				DrawEdges();
				using (new GUIScope.Windows(this))
				{
					DrawNodes();
				}

				DrawConnector();
				ProcessBackgroundEvents(Event.current);
			}

			DrawOverlayWidgets();
		}

		private void OnInspectorUpdate()
		{
			Repaint();
		}


		/// <summary>
		/// Too
		/// </summary>
		private void DrawOverlayWidgets()
		{
			var area = position;
			area.position = Vector2.zero;

			//GUI.Button()
		}

		/// <summary>
		/// Clear layout state
		/// </summary>
		private void ResetLayout()
		{
			_nodeRects = new Rect[0];
			_transform.Reset();
			CenterOnNodes();
		}

		/// <summary>
		/// 
		/// </summary>
		private void RefreshTitle()
		{
			var modified = IsModified();
			//var modified = _Graph ? EditorUtility.IsDirty(_Graph) : false;
			titleContent.text = modified ? WINDOW_TITLE_DIRTY : WINDOW_TITLE;
		}

		private bool IsModified()
		{
			if (!_graph) { return false; }

			if (EditorUtility.IsDirty(_graph)) { return true; }

			for (var i = 0; i < _graph.NodeCount; i++)
			{
				var n = _graph.GetNodeAt(i);
				if (!n) { continue; }

				if (EditorUtility.IsDirty(n)) { return true; }

			}
			return false;
		}

		private void Select(MenuNode node)
		{
			Selection.activeObject = node;
		}

		private void CenterOnNodes()
		{
			_transform.drag = GetPositionOfNodes();
		}

		private Vector2 GetPositionOfNodes()
		{
			if (!_graph || _graph.NodeCount == 0) { return Vector2.zero; }
			var center = position.size * 0.5f;
			var nodePos = Vector2.zero;
			var nodeOffset = NODE_SIZE * 0.5f;
			// node to center on
			var node = _graph.RootNode ?? _graph.GetLeftMostNode();
			if (node) { nodePos = -node.Position; }
			return center + nodePos - nodeOffset;
		}

		private void ProcessNodeDrag(Event e)
		{

			if (_draggedNode == null) { return; }

			var index = _draggedNode.Item1;

			// apply if...
			var applyChanges =
			e.IsMouseUp(MouseButton.LEFT) // mouse released
			|| !this.IsCurrentlyMoused(); // canvas lost focus

			if (applyChanges)
			{
				var i = index;
				//_draggedIndex = -1;
				_draggedNode = null;
				ApplyPositionChangeAt(i);
			}
		}

		private void ProcessCanvasDrag(Event e)
		{
			// mmb needs to be held
			if (e.button != MouseButton.MIDDLE) { return; }

			// should add drag
			var isDragging =
			e.type == EventType.MouseDrag
			|| e.type == EventType.MouseDown;

			// nothing
			if (!isDragging) { return; }

			// apply drag to canvas
			e.Use();
			_transform.drag += e.delta;
		}

		private void ProcessZoomEvents(Event e)
		{
			if (!e.isScrollWheel) { return; }
			if (Mathf.Approximately(0, e.delta.y)) { return; }
			e.Use();
			_transform.Zoom(sign: e.delta.y > 0 ? 1 : -1);
		}

		private void ProcessBackgroundEvents(Event e)
		{
			// is currently dragging node
			//if(_draggedIndex > -1) { return; } // dragging node
			if (_draggedNode != null) { return; } // busy dragging node


			// show dropdown for adding nodes
			var showAddContext =
			_connector == null // shouldn't be connecting node
			&& e.type == EventType.ContextClick; // right clicked

			if (showAddContext)
			{
				e.Use();
				GetContextMenu().ShowAsContext();
			}

			// should selection be cleared?
			var clearSelection =
			e.type == EventType.MouseDown
			&& e.button != MouseButton.MIDDLE;

			if (clearSelection)
			{
				e.Use();
				ClearConnector();
				Selection.activeObject = null;
			}
		}

		private GenericMenu GetContextMenu()
		{
			var m = new GenericMenu();
			var e = Event.current;
			// add node at position
			var pos = e.mousePosition;
			m.AddDisabledItem(new GUIContent("Add"));
			m.AddSeparator("");
			m.AddItem(new GUIContent("Group"), false, () => AddNode<MenuGroup>(pos));
			m.AddItem(new GUIContent("Action"), false, () => AddNode<MenuAction>(pos));
			m.AddItem(new GUIContent("Separator"), false, () => AddNode<MenuSeparator>(pos));
			return m;
		}

		private void AddNode<T>(Vector2 pos) where T : MenuNode
		{
			PVMenu_Graph.WithUndo.AddNode<T>(_graph, pos);
		}

		private void OnPortClicked(MenuNode n, int port)
		{
			if (port == 0) { return; } // interpret as empty

			if (_connector == null)
			{
				_connector = new Connector();
				_connector.node = n;
				_connector.port = port;
				_connector.index = _graph.IndexOfNode(n);
			}
			else
			{
				var sameType = Mathf.Sign(port) == Mathf.Sign(_connector.port);
				if (sameType) { return; } // no homo
				ConnectNodes(_connector.node, n, _connector.port, port);
				ClearConnector();
			}
		}

		private void ConnectNodes(MenuNode n1, MenuNode n2, int p1, int p2)
		{
			// make sure output (<0) -> input (>0)
			if (p2 < p1)
			{
				Swap(ref n1, ref n2);
				Swap(ref p1, ref p2);
			}
			PVMenu_Graph.WithUndo.AddEdge(_graph, n1, n2);
		}

		private void DrawConnector()
		{
			// nothing is being dragged
			if (_connector == null) { return; }

			// compute 
			var nodeRect = _nodeRects[_connector.index];
			var p1 = ComputePortCenter(nodeRect, port: _connector.port);
			var p2 = Event.current.mousePosition;
			GraphGizmos.DrawDragConnector(p1, p2, Color.white);
		}

		// when returning true, the gui should be refreshed
		private bool NeedsRepaint()
		{
			if (_connector != null) { return true; }
			return false;
		}

		private void ClearConnector() => _connector = null;

		private void ApplyTransformMatrix() => GUI.matrix = _transform.GetMatrix();

		private void ApplyPositionChangeAt(int i)
		{
			var node = _graph.GetNodeAt(i);
			if (!node) { return; }
			var pos = _nodeRects[i].position;
			MenuNode.WithUndo.SetPosition(node, pos);
		}

		private void ComputeLayoutRects()
		{
			if (_nodeRects.Length == _graph.NodeCount) { return; }
			_nodeRects = new Rect[_graph.NodeCount];
			for (var i = 0; i < _nodeRects.Length; i++)
			{
				var n = _graph.GetNodeAt(i);
				_nodeRects[i].size = NODE_SIZE;
				_nodeRects[i].position = n.Position;
			}
		}

		private void DrawNodes()
		{
			var c = _graph.NodeCount;
			try { for (var i = 0; i < c; i++) { DrawNodeAt(i); } }
			catch (Exception e) { Debug.Log(e.Message); }
		}

		private void DrawEdges()
		{
			var c = _graph.EdgeCount;
			try { for (var i = 0; i < c; i++) { DrawEdgeAt(i); } }
			finally { }
		}

		private void DrawNodeAt(int i)
		{
			var e = Event.current;
			if (e == null) { return; }

			var node = _graph.GetNodeAt(i);
			var nodeRect = _nodeRects[i];
			var oldPosition = nodeRect.position;

			DrawNodePorts(nodeRect, node);

			ProcessNodeClickRight(nodeRect, e, node);

			var wid = GUIUtility.GetControlID(FocusType.Passive);
			_nodeRects[i] = GUI.Window(wid, _nodeRects[i], _ =>
			{
				var draggable =
				this.IsCurrentlyMoused()
				&& e.button == MouseButton.LEFT
				&& _connector == null;

				ProcessNodeClick(e, node);

				if (draggable)
				{
					GUI.DragWindow();
				}
				var innerArea = _nodeRects[i];
				innerArea.position = Vector2.zero;
				DrawNodeArea(node, innerArea);



				if (_graph.RootNode == node)
				{
					DrawRootNodeGUI(_nodeRects[i]);
				}
			}, "");



			var moved = _nodeRects[i].position != oldPosition;

			if (moved)
			{
				SnapRectToGrid(ref _nodeRects[i]);
				_draggedNode = new Tuple<int, MenuNode>(i, node);
			}
		}

		private void DrawRootNodeGUI(in Rect nodeRect)
		{
			var ico = GraphIcons.ROOT_NODE.Value;
			GraphGizmos.DrawIcon(nodeRect.size * 0.5f, ico, 20f);
		}

		private void DrawEdgeAt(int i)
		{
			var e = _graph.GetEdgeAt(i);
			if (e == null) { return; }

			if (!e.IsValid) { return; }

			var ni1 = _graph.IndexOfNode(e.N1);
			var ni2 = _graph.IndexOfNode(e.N2);
			if (ni1 < 0 || ni2 < 0) { return; }
			var nr1 = _nodeRects[ni1];
			var nr2 = _nodeRects[ni2];
			var p1 = ComputePortCenter(nr1, -1);
			var p2 = ComputePortCenter(nr2, 1);
			var mid = (p1 + p2) * 0.5f;
			GraphGizmos.DrawConnector(p1, p2, Color.white);

			if (DrawDeleteButton(mid))
			{
				DeleteEdgeAt(i);
			}
		}

		private static bool DrawDeleteButton(in Vector2 pos)
		{
			var rect = new Rect(0, 0, DELETE_BTN_SIZE, DELETE_BTN_SIZE);
			rect.center = pos;
			var v = GUI.Button(rect, "");
			GraphGizmos.DrawIcon(rect, GraphIcons.DELETE.Value);
			return v;
		}

		private void DeleteEdgeAt(int i)
		{
			PVMenu_Graph.WithUndo.DeleteEdge(_graph, i);
		}

		private static void SnapRectToGrid(ref Rect r)
		{
			r.position = r.position.SnapToGrid(GRID_SNAP);
		}

		private void DrawNodeArea(MenuNode n, in Rect r)
		{
			var isRoot = _graph.RootNode == n;
			var label = isRoot ? "" : n.Label;
			var color = isRoot ? Color.clear : n.Color;

			DrawNodeColor(r, color);
			DrawNodeLabel(r, label);
		}

		private void DrawNodeColor(in Rect nodeRect, in Color c)
		{
			var area = nodeRect;
			{
				area.width = NODE_COLOR_WIDTH;
				var pos = area.position;
				area.position = pos;
			};
			EditorGUI.DrawRect(area, c);
		}

		private void ProcessNodeClickRight(in Rect rect, Event e, MenuNode node)
		{
			if (e.type == EventType.ContextClick && rect.Contains(e.mousePosition))
			{
				e.Use();
				GetContextMenu(node).ShowAsContext();
			}
		}

		private void ProcessNodeClick(Event e, MenuNode node)
		{
			if (e.button != MouseButton.LEFT) { return; }
			if (e.type == EventType.MouseDown) { _pressedNode = node; }
			else if (e.type == EventType.MouseUp && _pressedNode == node)
			{
				_pressedNode = null;
				Select(node);
			}
		}

		private GenericMenu GetContextMenu(MenuNode node)
		{
			var m = new GenericMenu();
			if (_graph.RootNode != node)
			{
				m.AddItem(new GUIContent("Set Root"), false, () => SetRootMode(node));
				m.AddSeparator("");
			}
			m.AddItem(new GUIContent("Delete"), false, () => DeleteNode(node));
			return m;
		}

		private void SetRootMode(MenuNode n)
		{
			PVMenu_Graph.WithUndo.SetRootNode(_graph, n);
		}

		private void DeleteNode(MenuNode n)
		{
			PVMenu_Graph.WithUndo.DeleteNode(_graph, n);
		}

		private static Lazy<GUIStyle> _NLABEL_STYLE = new Lazy<GUIStyle>(() =>
		{
			var s = new GUIStyle(EditorStyles.boldLabel);
			//s.fontSize = 10;
			//Debug.Log(s.fontSize);
			return s;
		});

		private static void DrawNodeLabel(in Rect r, string v)
		{

			if (v.Length > 10) { v = v.Substring(0, 10) + "..."; }

			var style = _NLABEL_STYLE.Value;
			var l = new GUIContent(v);
			var size = style.CalcSize(l);
			var pos = r.center;
			pos.x -= size.x * 0.5f;
			pos.y -= EGU.singleLineHeight * 0.5f;
			pos.y -= 2f;
			var area = r;
			area.position = pos;
			EditorGUI.LabelField(area, l, style);
		}

		private void DrawNodePorts(in Rect pos, MenuNode node)
		{
			var hasInputs = node.Inputs > 0 && _graph.RootNode != node;
			if (hasInputs)
			{
				for (var i = 0; i < node.Inputs; i++) { DrawNodePort(pos, node, i + 1); }
			}
			if (node.Outputs > 0)
			{
				for (var i = 0; i < node.Outputs; i++) { DrawNodePort(pos, node, -i - 1); }
			}
		}

		private void DrawNodePort(in Rect nodeRect, MenuNode n, in int port)
		{
			var center = ComputePortCenter(nodeRect, port);
			var box = GetCenteredBox(center, PORT_SIZE);
			var pressed = GUI.Button(box, "");

			GUI.DrawTexture(box, GraphIcons.CIRCLE_DOTTED.Value);

			if (pressed)
			{
				Event.current.Use();
				OnPortClicked(n, port);
			}
		}

		private Vector2 ComputePortCenter(in Rect area, in int port)
		{
			var flip = port < 0;
			var pos = area.center;
			var ox = (area.width * 0.5f) + PORT_OFFSET + (PORT_SIZE * 0.5f);
			var oy = 0;
			var offset = new Vector2(ox, oy);
			if (!flip) { offset.x *= -1f; }
			pos += offset;
			return pos;
		}

		private static Rect GetCenteredBox(in Vector2 center, in float size)
		{
			var r = new Rect(0, 0, size, size);
			r.center = center;
			return r;
		}

		private static void Swap<T>(ref T a, ref T b)
		{
			var t = a;
			a = b;
			b = t;
		}

		private static class GraphGizmos
		{
			public static void DrawIcon(in Vector2 pos, Texture t, float size)
			{
				var r = new Rect(0, 0, size, size);
				r.center = pos;
				GUI.DrawTexture(r, t);
			}

			public static void DrawIcon(in Rect r, Texture t)
			{
				GUI.DrawTexture(r, t);
			}

			public static void DrawConnector(in Vector2 a, in Vector2 b, in Color c)
			{
				Handles.color = c;
				Handles.DrawAAPolyLine(LINE_THICKNESS, a, b);
				Handles.DrawLine(a, b);
				DrawIcon(a, GraphIcons.DOT_YELLOW.Value, 10f);
				DrawIcon(b, GraphIcons.DOT_YELLOW.Value, 10f);
			}


			public static void DrawDragConnector(in Vector2 a, in Vector2 b, in Color c)
			{
				Handles.color = c;
				Handles.DrawAAPolyLine(LINE_THICKNESS, a, b);
				DrawIcon(a, GraphIcons.DOT_YELLOW.Value, 8f);
			}
		}

		internal static class GraphIcons
		{
			public static readonly LZTex
			CIRCLE = new LZTex(() => EGU.IconContent("AvatarInspector/DotFrame").image),
			CIRCLE_DOTTED = new LZTex(() => EGU.IconContent("AvatarInspector/DotFrameDotted").image),
			DOT_YELLOW = new LZTex(() => EGU.IconContent("sv_icon_dot4_pix16_gizmo").image),
			DELETE = new LZTex(() => EGU.IconContent("P4_DeletedRemote").image),
			ROOT_NODE = new LZTex(() => EGU.IconContent("Favorite Icon").image);
		}

	}


}