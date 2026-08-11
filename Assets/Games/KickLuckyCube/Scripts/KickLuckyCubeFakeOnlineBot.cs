using System;
using UnityEngine;

namespace RobloxBasicProject.Games.KickLuckyCube
{
    public sealed class KickLuckyCubeFakeOnlineBot : MonoBehaviour
    {
        private enum FakeBotActivity
        {
            Patrol,
            Train,
            Kick
        }

        [SerializeField] private string botName = "Player";
        [SerializeField] private Transform avatarRoot;
        [SerializeField] private Transform toolRoot;
        [SerializeField] private Transform trainingPoint;
        [SerializeField] private Transform kickPoint;
        [SerializeField] private Transform kickCube;
        [SerializeField] private Transform kickStart;
        [SerializeField] private Transform kickEnd;
        [SerializeField] private Transform[] patrolPoints = Array.Empty<Transform>();
        [SerializeField] private TextMesh statusLabel;
        [SerializeField, Min(0.1f)] private float moveSpeed = 2.4f;
        [SerializeField, Min(0.1f)] private float cycleSeconds = 15f;
        [SerializeField, Min(0f)] private float phaseOffset;
        [SerializeField, Min(0.1f)] private float kickFlightSeconds = 1.35f;
        [SerializeField, Min(0f)] private float kickArcHeight = 2.1f;

        private Quaternion toolRestRotation;
        private Vector3 avatarRestLocalPosition;
        private int patrolIndex;
        private FakeBotActivity lastActivity;

        public string BotName => botName;

        public void Configure(string displayName, KickLuckyCubeBotActivityAnchors anchors, float activityPhaseOffset)
        {
            botName = string.IsNullOrWhiteSpace(displayName) ? "Player" : displayName;
            phaseOffset = Mathf.Max(0f, activityPhaseOffset);
            if (anchors != null)
            {
                trainingPoint = anchors.TrainingPoint;
                kickPoint = anchors.KickPoint;
                kickStart = anchors.KickStart;
                kickEnd = anchors.KickEnd;
                patrolPoints = anchors.PatrolPoints;
                transform.SetPositionAndRotation(anchors.SpawnPoint.position, anchors.SpawnPoint.rotation);
            }

            CacheRestState();
            ResetKickCube();
        }

        private void Awake()
        {
            CacheRestState();
        }

        private void OnEnable()
        {
            CacheRestState();
            lastActivity = FakeBotActivity.Patrol;
        }

        private void Update()
        {
            if (!Application.isPlaying)
            {
                return;
            }

            var activity = ResolveActivity();
            if (activity != lastActivity)
            {
                lastActivity = activity;
                if (activity != FakeBotActivity.Kick)
                {
                    ResetKickCube();
                }
            }

            switch (activity)
            {
                case FakeBotActivity.Train:
                    UpdateTraining();
                    break;
                case FakeBotActivity.Kick:
                    UpdateKicking();
                    break;
                default:
                    UpdatePatrol();
                    break;
            }

            UpdateStatusLabel(activity);
        }

        private void CacheRestState()
        {
            if (avatarRoot != null)
            {
                avatarRestLocalPosition = avatarRoot.localPosition;
            }

            if (toolRoot != null)
            {
                toolRestRotation = toolRoot.localRotation;
            }
        }

        private FakeBotActivity ResolveActivity()
        {
            var t = Mathf.Repeat(Time.time + phaseOffset, cycleSeconds) / cycleSeconds;
            if (t < 0.4f)
            {
                return FakeBotActivity.Patrol;
            }

            return t < 0.7f ? FakeBotActivity.Train : FakeBotActivity.Kick;
        }

        private void UpdatePatrol()
        {
            ResetKickCube();

            if (patrolPoints == null || patrolPoints.Length == 0)
            {
                AnimateIdle();
                return;
            }

            var point = patrolPoints[Mathf.Clamp(patrolIndex, 0, patrolPoints.Length - 1)];
            if (MoveTo(point.position))
            {
                patrolIndex = (patrolIndex + 1) % patrolPoints.Length;
            }

            AnimateWalk();
        }

        private void UpdateTraining()
        {
            ResetKickCube();

            if (trainingPoint != null && !MoveTo(trainingPoint.position))
            {
                AnimateWalk();
                return;
            }

            AnimateTraining();
        }

        private void UpdateKicking()
        {
            if (kickPoint != null && !MoveTo(kickPoint.position))
            {
                ResetKickCube();
                AnimateWalk();
                return;
            }

            AnimateKick();
        }

        private bool MoveTo(Vector3 target)
        {
            var current = transform.position;
            target.y = current.y;
            var delta = target - current;
            if (delta.sqrMagnitude <= 0.05f)
            {
                return true;
            }

            var direction = delta.normalized;
            transform.position = Vector3.MoveTowards(current, target, moveSpeed * Time.deltaTime);
            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                Quaternion.LookRotation(direction, Vector3.up),
                1f - Mathf.Exp(-12f * Time.deltaTime));
            return false;
        }

        private void AnimateIdle()
        {
            if (avatarRoot == null)
            {
                return;
            }

            avatarRoot.localPosition = avatarRestLocalPosition + Vector3.up * (Mathf.Sin((Time.time + phaseOffset) * 3f) * 0.03f);
            if (toolRoot != null)
            {
                toolRoot.localRotation = Quaternion.Slerp(toolRoot.localRotation, toolRestRotation, 10f * Time.deltaTime);
            }
        }

        private void AnimateWalk()
        {
            if (avatarRoot == null)
            {
                return;
            }

            avatarRoot.localPosition = avatarRestLocalPosition + Vector3.up * (Mathf.Abs(Mathf.Sin((Time.time + phaseOffset) * 8f)) * 0.08f);
            if (toolRoot != null)
            {
                toolRoot.localRotation = toolRestRotation * Quaternion.Euler(Mathf.Sin((Time.time + phaseOffset) * 8f) * 12f, 0f, 0f);
            }
        }

        private void AnimateTraining()
        {
            if (avatarRoot != null)
            {
                avatarRoot.localPosition = avatarRestLocalPosition + Vector3.up * (Mathf.Abs(Mathf.Sin((Time.time + phaseOffset) * 9f)) * 0.07f);
            }

            if (toolRoot != null)
            {
                var swing = Mathf.Sin((Time.time + phaseOffset) * 8.5f);
                toolRoot.localRotation = toolRestRotation * Quaternion.Euler(-54f + swing * 34f, 0f, swing * 10f);
            }
        }

        private void AnimateKick()
        {
            if (avatarRoot != null)
            {
                avatarRoot.localPosition = avatarRestLocalPosition + Vector3.up * (Mathf.Sin((Time.time + phaseOffset) * 10f) * 0.05f);
            }

            if (toolRoot != null)
            {
                toolRoot.localRotation = toolRestRotation * Quaternion.Euler(-18f, 0f, Mathf.Sin((Time.time + phaseOffset) * 11f) * 18f);
            }

            if (kickCube == null || kickStart == null || kickEnd == null)
            {
                return;
            }

            var loopSeconds = kickFlightSeconds + 0.8f;
            var loop = Mathf.Repeat(Time.time + phaseOffset, loopSeconds);
            if (loop > kickFlightSeconds)
            {
                ResetKickCube();
                return;
            }

            var t = Mathf.Clamp01(loop / kickFlightSeconds);
            var eased = Mathf.SmoothStep(0f, 1f, t);
            kickCube.position = Vector3.Lerp(kickStart.position, kickEnd.position, eased)
                + Vector3.up * (Mathf.Sin(eased * Mathf.PI) * kickArcHeight);
            kickCube.Rotate(180f * Time.deltaTime, 240f * Time.deltaTime, 120f * Time.deltaTime, Space.World);
        }

        private void ResetKickCube()
        {
            if (kickCube == null || kickStart == null)
            {
                return;
            }

            kickCube.SetPositionAndRotation(kickStart.position, kickStart.rotation);
        }

        private void UpdateStatusLabel(FakeBotActivity activity)
        {
            if (statusLabel == null)
            {
                return;
            }

            var action = activity switch
            {
                FakeBotActivity.Train => "training",
                FakeBotActivity.Kick => "kicking cube",
                _ => "running"
            };

            statusLabel.text = $"{botName}\n{action}";
            statusLabel.transform.rotation = Quaternion.Euler(60f, 0f, 0f);
        }
    }
}
