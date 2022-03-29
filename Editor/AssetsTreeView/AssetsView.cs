﻿using AwtterSDK.Editor.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEditor.TreeViewExamples;
using UnityEngine;
using UnityEngine.Networking;

namespace AwtterSDK.Editor.AssetsTreeView
{
	internal class AssetsView : TreeViewWithTreeModel<AssetElement>
	{
		const float kRowHeights = 20f;
		const float kToggleWidth = 18f;
		public bool showControls = true;


		enum MyColumns
		{
			Icon,
			Name,
			Status,
			Version
		}

		public static void TreeToList(TreeViewItem root, IList<TreeViewItem> result)
		{
			if (root == null)
				throw new NullReferenceException("root");
			if (result == null)
				throw new NullReferenceException("result");

			result.Clear();

			if (root.children == null)
				return;

			Stack<TreeViewItem> stack = new Stack<TreeViewItem>();
			for (int i = root.children.Count - 1; i >= 0; i--)
				stack.Push(root.children[i]);

			while (stack.Count > 0)
			{
				TreeViewItem current = stack.Pop();
				result.Add(current);

				if (current.hasChildren && current.children[0] != null)
				{
					for (int i = current.children.Count - 1; i >= 0; i--)
					{
						stack.Push(current.children[i]);
					}
				}
			}
		}

		public AssetsView(TreeViewState state, MultiColumnHeader multicolumnHeader, TreeModel<AssetElement> model) : base(state, multicolumnHeader, model)
		{
			// Custom setup
			extraSpaceBeforeIconAndLabel = 0f;
			rowHeight = kRowHeights;
			columnIndexForTreeFoldouts = 2;
			showAlternatingRowBackgrounds = true;
			customFoldoutYOffset = (kRowHeights - EditorGUIUtility.singleLineHeight) * 0.5f; // center foldout in the row since we also center content. See RowGUI
			showBorder = true;
			Reload();
		}


        protected override bool CanStartDrag(CanStartDragArgs args) => false;

        // Note we We only build the visible rows, only the backend has the full tree information. 
        // The treeview only creates info for the row list.
        protected override IList<TreeViewItem> BuildRows(TreeViewItem root)
		{
			var rows = base.BuildRows(root);
			return rows;
		}

		int GetIcon1Index(TreeViewItem<AssetElement> item) => 0;

		protected override void RowGUI(RowGUIArgs args)
		{
			var item = (TreeViewItem<AssetElement>)args.item;

			for (int i = 0; i < args.GetNumVisibleColumns(); ++i)
			{
				CellGUI(args.GetCellRect(i), item, (MyColumns)args.GetColumn(i), ref args);
			}
		}


		void CellGUI(Rect cellRect, TreeViewItem<AssetElement> item, MyColumns column, ref RowGUIArgs args)
		{
			CenterRectUsingSingleLineHeight(ref cellRect);
			switch (column)
			{
				case MyColumns.Icon:
					{
						GUI.DrawTexture(cellRect, AwtterSdkInstaller.GetTextureOrDownload(item.data.Icon), ScaleMode.ScaleToFit);
					}
					break;
				case MyColumns.Name:
					{
						args.label = item.data.name;
						args.rowRect = cellRect;
						base.RowGUI(args);
					}
					break;
				case MyColumns.Status:
					{
						args.rowRect = cellRect;
						var defColor = GUI.color;
						Rect statusRect = cellRect;
						cellRect.x += 25f;

						switch (item.data.Status)
                        {
							case PackageStatus.None:
							case PackageStatus.NotInstalled:
								statusRect.width = 25f;
								GUI.color = Color.red;
								EditorGUI.LabelField(statusRect, "❌");
								GUI.color = defColor;
								EditorGUI.LabelField(cellRect, "        Not Installed");
								break;
							case PackageStatus.Installed:
								statusRect.width = 25f;
								GUI.color = Color.green;
								EditorGUI.LabelField(statusRect, "✔️");
								GUI.color = defColor;
								EditorGUI.LabelField(cellRect, "        Installed");
								break;
                        }
					}
					break;
				case MyColumns.Version:
					{
						EditorGUI.LabelField(cellRect, item.data.Version);
					}
					break;
			}
		}

		public static MultiColumnHeaderState CreateDefaultMultiColumnHeaderState(float treeViewWidth)
		{
			var columns = new[]
			{
				new MultiColumnHeaderState.Column
				{
					headerContent = new GUIContent(EditorGUIUtility.FindTexture("FilterByLabel"), "Icon"),
					contextMenuText = "Icon",
					headerTextAlignment = TextAlignment.Center,
					sortedAscending = true,
					sortingArrowAlignment = TextAlignment.Right,
					width = 30,
					minWidth = 30,
					maxWidth = 30,
					autoResize = false,
					allowToggleVisibility = true,
					canSort = false,
				},
				new MultiColumnHeaderState.Column
				{
					headerContent = new GUIContent("Name"),
					headerTextAlignment = TextAlignment.Left,
					sortedAscending = true,
					sortingArrowAlignment = TextAlignment.Center,
					width = 150, 
					minWidth = 60,
					autoResize = false,
					allowToggleVisibility = false
				},
				new MultiColumnHeaderState.Column
				{
					headerContent = new GUIContent("Status"),
					headerTextAlignment = TextAlignment.Left,
					sortedAscending = true,
					sortingArrowAlignment = TextAlignment.Left,
					width = 150,
					minWidth = 60,
					autoResize = false,
					allowToggleVisibility = false,
					canSort = false,
				},
				new MultiColumnHeaderState.Column
				{
					headerContent = new GUIContent("Version"),
					headerTextAlignment = TextAlignment.Left,
					sortedAscending = true,
					sortingArrowAlignment = TextAlignment.Left,
					width = 150,
					minWidth = 60,
					autoResize = false,
					allowToggleVisibility = false,
					canSort = false,
				},
			};

			var state = new MultiColumnHeaderState(columns);
			return state;
		}
	}
}
