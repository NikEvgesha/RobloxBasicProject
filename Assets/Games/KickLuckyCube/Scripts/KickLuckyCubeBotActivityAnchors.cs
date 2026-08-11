using System;
using UnityEngine;

namespace RobloxBasicProject.Games.KickLuckyCube
{
    public sealed class KickLuckyCubeBotActivityAnchors : MonoBehaviour
    {
        [SerializeField] private Transform spawnPoint;
        [SerializeField] private Transform trainingPoint;
        [SerializeField] private Transform kickPoint;
        [SerializeField] private Transform kickStart;
        [SerializeField] private Transform kickEnd;
        [SerializeField] private Transform[] patrolPoints = Array.Empty<Transform>();

        public Transform SpawnPoint => spawnPoint != null ? spawnPoint : transform;
        public Transform TrainingPoint => trainingPoint != null ? trainingPoint : transform;
        public Transform KickPoint => kickPoint != null ? kickPoint : transform;
        public Transform KickStart => kickStart != null ? kickStart : KickPoint;
        public Transform KickEnd => kickEnd != null ? kickEnd : transform;
        public Transform[] PatrolPoints => patrolPoints ?? Array.Empty<Transform>();

        public void Configure(
            Transform spawn,
            Transform training,
            Transform kick,
            Transform cubeStart,
            Transform cubeEnd,
            Transform[] patrol)
        {
            spawnPoint = spawn;
            trainingPoint = training;
            kickPoint = kick;
            kickStart = cubeStart;
            kickEnd = cubeEnd;
            patrolPoints = patrol ?? Array.Empty<Transform>();
        }
    }
}
