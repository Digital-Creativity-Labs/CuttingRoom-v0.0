using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace CuttingRoom.Editor
{
	public class DecisionPointNode : BaseNode
	{
		/// <summary>
		/// The candidate nodes which are linked to this object.
		/// </summary>
		public List<NarrativeObjectNode> candidateNodes { get; private set; } = new List<NarrativeObjectNode>();

		/// <summary>
		/// Decision point represented by this node.
		/// </summary>
		private DecisionPoint decisionPoint = null;

		public void Init(NarrativeSpaceEditor narrativeSpaceEditor, DecisionPoint decisionPoint)
		{
			base.Init(narrativeSpaceEditor);

			this.decisionPoint = decisionPoint;
		}

		public override void DrawWindow(GUIRenderingUtilities.RenderSettings renderSettings)
		{
			base.DrawWindow(renderSettings);
		}

		public void SetCandidateNodes(List<NarrativeObjectNode> candidateNodes)
		{
			this.candidateNodes = candidateNodes;
		}

		public void AddCandidate(NarrativeObjectNode narrativeObjectNode)
		{
			candidateNodes.Add(narrativeObjectNode);

			Undo.RecordObject(decisionPoint, "Adding candidate to decision point");

			decisionPoint.AddCandidate(narrativeObjectNode.narrativeObject.gameObject);
		}

		public void RemoveCandidate(NarrativeObjectNode narrativeObjectNode)
		{
			if (candidateNodes.Contains(narrativeObjectNode))
			{
				candidateNodes.Remove(narrativeObjectNode);

				Undo.RecordObject(decisionPoint, "Removing candidate from decision point");

				decisionPoint.RemoveCandidate(narrativeObjectNode.narrativeObject.gameObject);
			}
		}
	}
}
