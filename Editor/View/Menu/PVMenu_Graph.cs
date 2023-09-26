// smidgens @ github

namespace Smidgenomics.Unity.ProjectView.Editor
{
	using System;
	using System.Collections.Generic;
	using UnityEditor;
	using UnityEngine;

	/// <summary>
	/// Menu graph asset
	/// </summary>
	[CreateAssetMenu(menuName = Constants.CreateAssetMenu.ROOT + "Menu/Graph")]
	internal partial class PVMenu_Graph : PVMenu
	{
		/// <summary>
		/// Starting node
		/// </summary>
		public MenuNode RootNode => _rootNode;

		/// <summary>
		/// Total number of nodes in graph
		/// </summary>
		public int NodeCount => _nodes.Length;

		/// <summary>
		/// Total number of edges in graph
		/// </summary>
		public int EdgeCount => _edges.Length;

		/// <summary>
		/// Returns edge at given index
		/// </summary>
		public IEdge GetEdgeAt(int i) => _edges.IsOutOfBounds(i) ? null : _edges[i];

		/// <summary>
		/// Returns node at index
		/// </summary>
		public MenuNode GetNodeAt(int i) => _nodes.IsOutOfBounds(i) ? null : _nodes[i];

		public override GenericMenu GetMenu()
		{
			var m = new GenericMenu();

			m.allowDuplicateNames = true;

			if (!string.IsNullOrEmpty(_menuTitle))
			{
				m.AddDisabledItem(new GUIContent($"• {_menuTitle} •"));
				m.AddSeparator("");
			}

			var items = new List<Tuple<GUIContent, Action>>();

			if (!RootNode)
			{
				m.AddDisabledItem(new GUIContent("Empty"));
				return m;
			}

			var visited = new Dictionary<MenuNode, bool>();

			Action<MenuNode, string> traverse = null;

			traverse = (node, path) =>
			{
				if (visited.ContainsKey(node)) { return; } // tf u duin

				visited[node] = true;

				var handler = node.GetInvokeHandler();

				if (node.Outputs == 0)
				{
					items.Add(new Tuple<GUIContent, Action>(new GUIContent(path), handler));
				}

				// outgoing nodes
				foreach (var exitNode in GetExits(node))
				{
					traverse(exitNode, path + "/" + GetNodeText(exitNode));
				}
			};

			visited[RootNode] = true;

			foreach (var e in GetExits(RootNode))
			{
				traverse(e, GetNodeText(e));
			}

			foreach (var l in items)
			{
				if (l.Item2 != null)
				{
					m.AddItem(l.Item1, false, () => l.Item2.Invoke());
				}
				else { m.AddDisabledItem(l.Item1); }
			}
			return m;
		}

		public bool AreNodesConnected(MenuNode n1, MenuNode n2)
		{
			return Array.FindIndex(_edges, x => x.HasNodes(n1, n2)) > -1;
		}

		public MenuNode GetLeftMostNode()
		{
			var x = float.MaxValue;
			MenuNode node = null;
			foreach (var n in _nodes)
			{
				if (n.Position.x < x) { node = n; }
			}
			return node;
		}

		public int IndexOfNode(MenuNode n)
		{
			return Array.FindIndex(_nodes, x => x == n);
		}

		public bool HasNodes(params MenuNode[] nodes)
		{
			foreach (var n in nodes)
			{
				if (!n || IndexOfNode(n) < 0) { return false; }
			}
			return true;
		}

		[SerializeField, HideInInspector] private MenuNode _rootNode = null;
		[SerializeField] private NEdge[] _edges = { };
		[SerializeField, HideInInspector] private MenuNode[] _nodes = { };
		[SerializeField, HideInInspector] private int _lastModified = -1;

		// finds possible exit nodes
		private List<MenuNode> GetExits(MenuNode node)
		{
			var exits = new List<MenuNode>();
			if (node.Outputs == 0) { return exits; }
			for (var i = 0; i < _edges.Length; i++)
			{
				var e = _edges[i];
				if (e.N1 != node || !e.N2) { continue; }
				exits.Add(e.N2);
			}
			exits.Sort((a, b) =>
			{
				var w1 = a.SortWeight;
				var w2 = b.SortWeight;
				if (w1 == w2) { return 0; }
				return w1 > w2 ? 1 : -1;
			});

			return exits;

		}

		private static string GetNodeText(MenuNode node)
		{
			if (node.GetType() == typeof(MenuSeparator)) { return ""; }
			return node.Label;
		}


	}
}

namespace Smidgenomics.Unity.ProjectView.Editor
{
	using System;
	using UnityEngine;

	partial class PVMenu_Graph
	{
		public interface IEdge
		{
			public MenuNode N1 { get; }
			public MenuNode N2 { get; }
			public int P1 { get; }
			public int P2 { get; }
			public bool IsValid { get; }
		}

		[Serializable]
		private class NEdge : IEdge
		{
			public MenuNode N1 => _n1;
			public MenuNode N2 => _n2;
			public int P1 => _p1;
			public int P2 => _p2;

			public bool IsValid => !(!_n1 || !_n2 || _p1 == 0 || _p2 == 0);

			public NEdge(MenuNode n1, MenuNode n2, int p1, int p2)
			{
				_n1 = n1;
				_n2 = n2;
				_p1 = p1;
				_p2 = p2;
			}

			/// <summary>
			/// Check if edge connects given nodes (in any order)
			/// </summary>
			public bool HasNodes(MenuNode n1, MenuNode n2)
			{
				if (!n1 || !n2) { return false; }
				return (n1 == _n1 && n2 == _n2) || (n1 == _n2 && n2 == _n1);
			}

			public bool HasNode(MenuNode n)
			{
				if (!n) { return false; }
				return _n1 == n || _n2 == n;
			}

			[SerializeField] private MenuNode _n1, _n2;
			[SerializeField] private int _p1, _p2; // input,output
		}
	}
}


namespace Smidgenomics.Unity.ProjectView.Editor
{
	using System;
	using System.Linq;
	using UnityEditor;
	using UnityEngine;

	partial class PVMenu_Graph
	{
		/// <summary>
		/// Editing API with Undo tracking
		/// </summary>
		public static class WithUndo
		{
			public const string NODES_FIELD = nameof(_nodes);
			public const string MODIFIED_FIELD = nameof(_lastModified);

			/// <summary>
			/// Sets root/start node
			/// </summary>
			public static bool SetRootNode(PVMenu_Graph g, MenuNode n)
			{
				Undo.RecordObject(g, nameof(SetRootNode));
				g._rootNode = n;
				MarkChanges(g);
				return true;
			}

			/// <summary>
			/// Adds a new node to the graph
			/// </summary>
			public static bool AddNode<T>(PVMenu_Graph g, Vector2 pos) where T : MenuNode
			{
				T node = g.CreateNested<T>(hide: true, name: "");
				if (!node) { return false; }

				// apply position
				MenuNode.WithUndo.SetPosition(node, pos);
				// apply changes
				Undo.RecordObject(g, nameof(AddNode));
				var nodes = g._nodes.ToList();
				nodes.Add(node);
				g._nodes = nodes.ToArray();
				MarkChanges(g);
				return true;
			}

			/// <summary>
			/// Deletes a node and all edges using it
			/// </summary>
			public static bool DeleteNode(PVMenu_Graph g, MenuNode n)
			{
				// node isn't in graph
				if (!g.HasNodes(n)) { return false; }
				// remove nodes
				var nodes = g._nodes.ToList();
				nodes.Remove(n);
				// remove edges
				var edges = g._edges.ToList();
				var removedEdges = edges.RemoveAll(x => x.HasNode(n));
				// apply changes
				Undo.RecordObject(g, nameof(DeleteNode));
				g._nodes = nodes.ToArray();
				if (removedEdges > 0) { g._edges = edges.ToArray(); }
				Undo.DestroyObjectImmediate(n);
				MarkChanges(g);
				return true;
			}

			/// <summary>
			/// Connects two nodes
			/// </summary>
			public static bool AddEdge(PVMenu_Graph g, MenuNode n1, MenuNode n2)
			{
				// connect to self?
				if (n1 == n2) { return false; }
				// nodes not found in menu
				if (!g.HasNodes(n1, n2)) { return false; }
				// are nodes already connected
				if (g.AreNodesConnected(n1, n2)) { return false; }

				// apply changes
				Undo.RecordObject(g, nameof(AddEdge));
				var edges = g._edges.ToList();
				var e = new NEdge(n1, n2, -1, 1);
				edges.Add(e);
				g._edges = edges.ToArray();
				MarkChanges(g);
				//AssetDatabase.SaveAssetIfDirty(g);
				return true;
			}

			/// <summary>
			/// Deletes all edges in graph
			/// </summary>
			public static bool DeleteAllEdges(PVMenu_Graph g)
			{
				// nothing to delete
				if (g._edges.Length == 0) { return false; }
				// clear edges and apply
				Undo.RecordObject(g, nameof(DeleteAllEdges));
				g._edges = new NEdge[0];
				MarkChanges(g);
				return true;
			}

			/// <summary>
			/// Deletes edge at specific index
			/// </summary>
			public static bool DeleteEdge(PVMenu_Graph g, int i)
			{
				if (g._edges.IsOutOfBounds(i)) { return false; }

				// apply changes
				Undo.RecordObject(g, nameof(DeleteEdge));

				var edges = g._edges.ToList();
				edges.RemoveAt(i);
				g._edges = edges.ToArray();

				MarkChanges(g);

				return true;
			}

			/// <summary>
			/// Delete all edges between nodes
			/// </summary>
			public static bool DeleteEdges(PVMenu_Graph g, MenuNode n1, MenuNode n2)
			{
				// remove all matching edges
				var edges = g._edges
				.Where(x => !(!x.IsValid || (x.N1 == n1 && x.N2 == n2))) // filter
				.ToArray();

				if (edges.Length == g._edges.Length)
				{
					return false;
				}
				Undo.RecordObject(g, nameof(DeleteEdge));
				g._edges = edges;
				MarkChanges(g);
				return true;
			}

			public static int DeleteInvalidEdges(PVMenu_Graph g)
			{
				var edges = g._edges
				.Where(x => x.IsValid) // retain valid edges
				.ToArray();
				var count = g._edges.Length - edges.Length;
				// if no invalids were found
				if (count == 0) { return 0; }
				// overwrite edges
				Undo.RecordObject(g, nameof(DeleteInvalidEdges));
				g._edges = edges;
				MarkChanges(g);
				return count;
			}

			/// <summary>
			/// fix issues with node references if any exist
			/// </summary>
			public static bool FixNodeRefs(PVMenu_Graph g)
			{
				// load assets afresh
				var nodes = g.LoadNestedAssets<MenuNode>();
				// update node list
				if (!nodes.IsSameAs(g._nodes))
				{
					var so = new SerializedObject(g);

					// mark time of change
					so.FindProperty(MODIFIED_FIELD).intValue = GetTimestamp();

					var narr = so.FindProperty(NODES_FIELD);
					narr.arraySize = nodes.Length;
					for (var i = 0; i < nodes.Length; i++)
					{
						narr.GetArrayElementAtIndex(i).objectReferenceValue = nodes[i];
					}
					so.ApplyModifiedPropertiesWithoutUndo();
					return true;
				}
				return false;
			}

			/// <summary>
			/// Set modified timestamp and mark dirty
			/// </summary>
			private static void MarkChanges(PVMenu_Graph g)
			{
				g._lastModified = GetTimestamp();
				EditorUtility.SetDirty(g);
			}

			private static int GetTimestamp()
			{
				return (int)DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1)).TotalSeconds;
			}

		}
	}
}

namespace Smidgenomics.Unity.ProjectView.Editor
{
	using UnityEditor;
	using UnityEngine;
	using System.Collections.Generic;
	using System.Linq;

	[CustomEditor(typeof(PVMenu_Graph))]
	internal class _PVMenu_Graph : Editor
	{
		public override void OnInspectorGUI()
		{
			serializedObject.UpdateIfRequiredOrScript();

			GUILayout.Space(1f);
			foreach (var p in _defaultProperties)
			{
				EditorGUILayout.PropertyField(p);
			}

			serializedObject.ApplyModifiedProperties();

			GUILayout.Space(5f);

			DisplayNodes();

			GUILayout.Space(10f);


			if (_buttons.Count > 0)
			{
				using (new GUILayout.HorizontalScope())
				{
					foreach (var item in _buttons)
					{
						if (GUILayout.Button(item.Key)) { item.Value.Invoke(); }
					}
				}
			}
		}

		private UnityEditorInternal.ReorderableList _nodes = null;

		private bool _foldout = false;

		private PVMenu_Graph _target = null;

		private Dictionary<string, System.Action> _buttons = null;

		private IEnumerable<SerializedProperty> _defaultProperties = null;

		private static string[] _defaultPropertyNames =
		{
			nameof(PVMenu._menuTitle)
		};

		private void OnEnable()
		{

			_target = (PVMenu_Graph)target;

			_defaultProperties = _defaultPropertyNames.Select(pn => serializedObject.FindProperty(pn));

			var nodes = serializedObject.FindProperty(PVMenu_Graph.WithUndo.NODES_FIELD);
			_nodes = new UnityEditorInternal.ReorderableList(serializedObject, nodes, false, false, false, false);

			_nodes.headerHeight = 0f;

			_buttons = new Dictionary<string, System.Action>()
			{
				{ "Fix References", FixNodeRefs },
				{ "Clear Invalid Edges", ClearInvalidEdges },
			};

			_nodes.drawElementCallback = (pos, i, a, f) =>
			{
				var n = _target.GetNodeAt(i);
				if (!n)
				{
					EditorGUI.DrawRect(pos, Color.red);
					EditorGUI.LabelField(pos, "<missing>");
					return;
				}
				EditorGUI.LabelField(pos, n.Label);
			};
		}


		private void DisplayNodes()
		{
			using (new GUILayout.VerticalScope(GUI.skin.box))
			{
				using (new EditorGUI.IndentLevelScope(1))
				{
					_foldout = EditorGUILayout.Foldout(_foldout, _nodes.serializedProperty.displayName, true);
				}

				if (_foldout)
				{
					GUILayout.Space(5f);
					_nodes.DoLayoutList();
				}
			}
		}

		private void ClearInvalidEdges()
		{
			PVMenu_Graph.WithUndo.DeleteInvalidEdges(_target);
		}

		private void FixNodeRefs()
		{
			PVMenu_Graph.WithUndo.FixNodeRefs(_target);
		}
	}
}