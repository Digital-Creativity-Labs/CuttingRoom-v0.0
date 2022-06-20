using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace CuttingRoom.Editor
{
    public class EditorBlackboard : Blackboard
    {
        public EditorBlackboard()
        {
            subTitle = "Settings";

            // Move away from the edge of the window.
            SetPosition(new Rect(10, 50, 200, 300));
        }
    }
}