using System.Collections.Generic;
using UnityEngine;

namespace RobloxBasicProject.Games.KickLuckyCube
{
    [DisallowMultipleComponent]
    public sealed class KickLuckyCubeTrainingAnimationAuthoring : MonoBehaviour
    {
        [SerializeField] private Animator animator;
        [SerializeField] private AnimationClip previewClip;
        [SerializeField] private Transform toolRoot;
        [SerializeField] private Transform leftGrip;
        [SerializeField] private Transform rightGrip;
        [SerializeField] private Transform leftHand;
        [SerializeField] private Transform rightHand;
        [SerializeField] private Transform leftFoot;
        [SerializeField] private Transform rightFoot;
        [SerializeField, Min(0.05f)] private float referenceGripWidth = 0.72f;
        [SerializeField, Range(0f, 1f)] private float previewNormalizedTime;

        public Animator Animator => animator;
        public AnimationClip PreviewClip => previewClip;
        public Transform ToolRoot => toolRoot;
        public Transform LeftGrip => leftGrip;
        public Transform RightGrip => rightGrip;
        public Transform LeftHand => leftHand;
        public Transform RightHand => rightHand;
        public Transform LeftFoot => leftFoot;
        public Transform RightFoot => rightFoot;
        public float ReferenceGripWidth => Mathf.Max(0.05f, referenceGripWidth);
        public float PreviewNormalizedTime => Mathf.Clamp01(previewNormalizedTime);
        public float LeftGripError => leftGrip != null && leftHand != null
            ? Vector3.Distance(leftGrip.position, leftHand.position)
            : float.PositiveInfinity;
        public float RightGripError => rightGrip != null && rightHand != null
            ? Vector3.Distance(rightGrip.position, rightHand.position)
            : float.PositiveInfinity;

        public void AutoBind()
        {
            animator ??= GetComponentInChildren<Animator>(true);
            toolRoot ??= FindChild("Tool") ?? FindChild("Bar") ?? FindChild("Weight");
            leftGrip ??= FindChild("LeftGrip");
            rightGrip ??= FindChild("RightGrip");
            leftHand ??= FindChild("LeftHand") ?? FindChild("Hand_L");
            rightHand ??= FindChild("RightHand") ?? FindChild("Hand_R");
            leftFoot ??= FindChild("LeftFoot") ?? FindChild("Foot_L");
            rightFoot ??= FindChild("RightFoot") ?? FindChild("Foot_R");
        }

        public void SetPreviewNormalizedTime(float value)
        {
            previewNormalizedTime = Mathf.Clamp01(value);
        }

        public bool AlignToolToHands()
        {
            if (toolRoot == null || leftGrip == null || rightGrip == null || leftHand == null || rightHand == null)
                return false;

            var gripDirection = rightGrip.position - leftGrip.position;
            var handDirection = rightHand.position - leftHand.position;
            if (gripDirection.sqrMagnitude <= 0.000001f || handDirection.sqrMagnitude <= 0.000001f)
                return false;

            toolRoot.rotation = Quaternion.FromToRotation(gripDirection, handDirection) * toolRoot.rotation;
            var gripMidpoint = (leftGrip.position + rightGrip.position) * 0.5f;
            var handMidpoint = (leftHand.position + rightHand.position) * 0.5f;
            toolRoot.position += handMidpoint - gripMidpoint;
            return true;
        }

        public void CollectValidationIssues(ICollection<string> issues)
        {
            if (issues == null) return;
            if (animator == null) issues.Add($"{name}: Animator is not assigned.");
            if (previewClip == null) issues.Add($"{name}: preview clip is not assigned.");
            if (toolRoot == null) issues.Add($"{name}: tool root is not assigned.");
            if (leftGrip == null || rightGrip == null) issues.Add($"{name}: both tool grip anchors are required.");
            if (leftHand == null || rightHand == null) issues.Add($"{name}: both hand transforms are required.");
            if (leftFoot == null || rightFoot == null) issues.Add($"{name}: both foot transforms are required.");
        }

        private Transform FindChild(string partialName)
        {
            foreach (var child in GetComponentsInChildren<Transform>(true))
                if (child != transform && child.name.Contains(partialName, System.StringComparison.OrdinalIgnoreCase)) return child;
            return null;
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = new Color(1f, 0.5f, 0.15f, 0.85f);
            if (leftGrip != null) Gizmos.DrawWireSphere(leftGrip.position, 0.06f);
            if (rightGrip != null) Gizmos.DrawWireSphere(rightGrip.position, 0.06f);
            if (leftGrip != null && rightGrip != null) Gizmos.DrawLine(leftGrip.position, rightGrip.position);
            Gizmos.color = new Color(0.25f, 0.9f, 1f, 0.8f);
            if (leftFoot != null) Gizmos.DrawWireSphere(leftFoot.position, 0.08f);
            if (rightFoot != null) Gizmos.DrawWireSphere(rightFoot.position, 0.08f);
        }
    }
}
