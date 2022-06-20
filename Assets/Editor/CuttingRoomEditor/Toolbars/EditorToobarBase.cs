using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace CuttingRoom.Editor
{
    public abstract class EditorToobarBase : Toolbar
    {
        /// <summary>
        /// Add a button to the toolbar.
        /// </summary>
        /// <param name="onClick"></param>
        /// <param name="text"></param>
        protected Button AddButton(Action onClick, string text)
        {
            Button button = new Button(() =>
            {
                onClick?.Invoke();
            });

            button.text = text;

            Add(button);

            return button;
        }
    }
}