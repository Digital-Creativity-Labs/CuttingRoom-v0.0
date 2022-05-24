using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace CuttingRoom.Editor
{
    public abstract class CuttingRoomEditorToolbarBase
    {
        /// <summary>
        /// Toolbar ui element for each toolbar.
        /// </summary>
        public Toolbar Toolbar { get; private set; } = new Toolbar();

        /// <summary>
        /// Add a button to the toolbar.
        /// </summary>
        /// <param name="onClick"></param>
        /// <param name="text"></param>
        protected void AddButton(Action onClick, string text)
        {
            Button button = new Button(() => { onClick?.Invoke(); });
            button.text = text;

            Toolbar.Add(button);
        }
    }
}