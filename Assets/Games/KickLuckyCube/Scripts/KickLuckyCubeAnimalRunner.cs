using System;
using UnityEngine;

#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace RobloxBasicProject.Games.KickLuckyCube
{
    public sealed class KickLuckyCubeAnimalRunner : MonoBehaviour
    {
        [SerializeField] private KickLuckyCubeSpawnedAnimal animal;
        [SerializeField] private KickLuckyCubeMobileInput mobileInput;
        [SerializeField, Min(0f)] private float speed = 7f;
        [SerializeField] private bool controlEnabled;
        [SerializeField] private float returnLineZ;
        [SerializeField, Min(0f)] private float returnTolerance = 0.8f;

        public event Action<KickLuckyCubeAnimalRunner> ReturnedToLine;

        public KickLuckyCubeSpawnedAnimal Animal => animal;
        public bool ControlEnabled => controlEnabled;
        public float Speed => speed;

        private void Awake()
        {
            mobileInput ??= FindFirstObjectByType<KickLuckyCubeMobileInput>(FindObjectsInactive.Include);
        }

        public void Configure(KickLuckyCubeSpawnedAnimal spawnedAnimal, float runnerSpeed)
        {
            animal = spawnedAnimal;
            speed = Mathf.Max(0f, runnerSpeed);
        }

        public void BeginRun(float targetReturnLineZ)
        {
            returnLineZ = targetReturnLineZ;
            controlEnabled = true;
        }

        public void StopRun()
        {
            controlEnabled = false;
        }

        public void ForceReturnForPrototype()
        {
            transform.position = new Vector3(transform.position.x, transform.position.y, returnLineZ);
            ReturnedToLine?.Invoke(this);
        }

        private void Update()
        {
            if (!controlEnabled)
            {
                return;
            }

            var input = ReadMoveInput();
            if (mobileInput != null && mobileInput.MoveInput.sqrMagnitude > input.sqrMagnitude)
            {
                input = mobileInput.MoveInput;
            }
            var movement = new Vector3(input.x, 0f, input.y);
            if (movement.sqrMagnitude > 1f)
            {
                movement.Normalize();
            }

            if (movement.sqrMagnitude > 0.0001f)
            {
                transform.position += movement * (speed * Time.deltaTime);
                transform.rotation = Quaternion.Slerp(
                    transform.rotation,
                    Quaternion.LookRotation(movement, Vector3.up),
                    1f - Mathf.Exp(-16f * Time.deltaTime));
            }

            if (transform.position.z <= returnLineZ + returnTolerance)
            {
                controlEnabled = false;
                ReturnedToLine?.Invoke(this);
            }
        }

        private static Vector2 ReadMoveInput()
        {
#if ENABLE_INPUT_SYSTEM
            var keyboard = Keyboard.current;
            if (keyboard == null)
            {
                return Vector2.zero;
            }

            var input = Vector2.zero;
            if (keyboard.aKey.isPressed || keyboard.leftArrowKey.isPressed)
            {
                input.x -= 1f;
            }

            if (keyboard.dKey.isPressed || keyboard.rightArrowKey.isPressed)
            {
                input.x += 1f;
            }

            if (keyboard.sKey.isPressed || keyboard.downArrowKey.isPressed)
            {
                input.y -= 1f;
            }

            if (keyboard.wKey.isPressed || keyboard.upArrowKey.isPressed)
            {
                input.y += 1f;
            }

            return input;
#else
            return new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
#endif
        }
    }
}
