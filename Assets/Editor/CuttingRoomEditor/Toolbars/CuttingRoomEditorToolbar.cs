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
        /// Invoked when the Pop View button is clicked.
        /// </summary>
        public event Action OnClickPopViewContainer;

        /// <summary>
        /// Invoked when the Add Atomic Narrative Object Node button is clicked.
        /// </summary>
        public event Action OnClickAddAtomicNarrativeObjectNode;

        /// <summary>
        /// Invoked when the Add Atomic Narrative Object Node button is clicked.
        /// </summary>
        public event Action OnClickAddGraphNarrativeObjectNode;

        public CuttingRoomEditorToolbar()
        {
            AddButton(InvokeOnClickToggleDevToolbar, "Show Dev Toolbar");
            AddButton(InvokeOnClickPopViewContainer, "Pop View");
            AddButton(InvokeOnClickAddAtomicNarrativeObjectNode, "Atomic");
            AddButton(InvokeOnClickAddGraphNarrativeObjectNode, "Graph");
        }

        private void InvokeOnClickToggleDevToolbar()
        {
            OnClickToggleDevToolbar?.Invoke();
        }

        private void InvokeOnClickPopViewContainer()
        {
            OnClickPopViewContainer?.Invoke();
        }

        private void InvokeOnClickAddAtomicNarrativeObjectNode()
        {
            OnClickAddAtomicNarrativeObjectNode?.Invoke();
        }

        private void InvokeOnClickAddGraphNarrativeObjectNode()
        {
            OnClickAddGraphNarrativeObjectNode?.Invoke();
        }
    }
}