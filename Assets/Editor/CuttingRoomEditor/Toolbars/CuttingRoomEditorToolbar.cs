using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace CuttingRoom.Editor
{
    public class CuttingRoomEditorToolbar : CuttingRoomEditorToolbarBase
    {
        /// <summary>
        /// Invoked when the dev toolbar button is clicked.
        /// </summary>
        public event Action OnClickToggleDevToolbar;

        /// <summary>
        /// Invoked when the Add Atomic Narrative Object Node button is clicked.
        /// </summary>
        public event Action OnClickAddAtomicNarrativeObjectNode;

        /// <summary>
        /// Invoked when the Add Graph Narrative Object Node button is clicked.
        /// </summary>
        public event Action OnClickAddGraphNarrativeObjectNode;

		/// Invoked when the Add Group Narrative Object Node button is clicked.
        /// </summary>
        public event Action OnClickAddGroupNarrativeObjectNode;

        /// <summary>
        /// The button which creates atomic objects when clicked.
        /// </summary>
        public Button CreateAtomicNarrativeObjectButton { get; private set; } = null;

        /// <summary>
        /// The button which creates atomic objects when clicked.
        /// </summary>
        public Button CreateGraphNarrativeObjectButton { get; private set; } = null;

        public CuttingRoomEditorToolbar()
        {
            //AddButton(InvokeOnClickToggleDevToolbar, "Show Dev Toolbar");
            CreateAtomicNarrativeObjectButton = AddButton(InvokeOnClickAddAtomicNarrativeObjectNode, "Atomic");
            CreateGraphNarrativeObjectButton = AddButton(InvokeOnClickAddGraphNarrativeObjectNode, "Graph");
			AddButton(InvokeOnClickAddGroupNarrativeObjectNode, "Group");
        }

        private void InvokeOnClickToggleDevToolbar()
        {
            OnClickToggleDevToolbar?.Invoke();
        }

        private void InvokeOnClickAddAtomicNarrativeObjectNode()
        {
            OnClickAddAtomicNarrativeObjectNode?.Invoke();
        }

        private void InvokeOnClickAddGraphNarrativeObjectNode()
        {
            OnClickAddGraphNarrativeObjectNode?.Invoke();
        }

        private void InvokeOnClickAddGroupNarrativeObjectNode()
        {
            OnClickAddGroupNarrativeObjectNode?.Invoke();
        }
    }
}