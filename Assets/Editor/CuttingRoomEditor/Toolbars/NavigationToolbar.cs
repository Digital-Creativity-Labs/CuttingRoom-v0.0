using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine.UIElements;

namespace CuttingRoom.Editor
{
    public class NavigationToolbar : CuttingRoomEditorToolbarBase
    {
        List<Button> viewContainerButtons = new List<Button>();

        /// <summary>
        /// Invoked whenever one of the breadcrumbs is clicked.
        /// </summary>
        public event Action<ViewContainer> OnClickNavigationButton;

        public void GenerateContents(Stack<ViewContainer> viewContainerStack)
        {
            // Remove old buttons.
            foreach (Button button in viewContainerButtons)
            {
                Toolbar.Remove(button);
            }

            // Remove references to removed buttons.
            viewContainerButtons.Clear();

            // Get the view containers in the view stack.
            List<ViewContainer> viewContainers = viewContainerStack.ToList();

            // Reverse this list to put the bottom of the stack at the start of the list.
            viewContainers.Reverse();

            NarrativeObject[] narrativeObjects = UnityEngine.Object.FindObjectsOfType<NarrativeObject>();

            // Add a breadcrumb for each view container.
            foreach (ViewContainer viewContainer in viewContainers)
            {
                Button button = new Button(() =>
                {
                    OnClickNavigationButton?.Invoke(viewContainer);
                });

                if (viewContainer.narrativeObjectGuid == CuttingRoomEditorGraphView.rootViewContainerGuid)
                {
                    button.text = "Root";
                }
                else
                {
                    NarrativeObject narrativeObject = narrativeObjects.Where(narrativeObject => narrativeObject.guid == viewContainer.narrativeObjectGuid).FirstOrDefault();

                    button.text = narrativeObject.gameObject.name;
                }

                Toolbar.Add(button);

                viewContainerButtons.Add(button);
            }
        }
    }
}
