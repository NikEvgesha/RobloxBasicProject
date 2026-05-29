using RobloxBasicProject.GameKit.Interaction;
using UnityEngine;

namespace RobloxBasicProject.Games.MechanicsTestbed
{
    public sealed class MechanicsTestbedCarryController : MonoBehaviour
    {
        [SerializeField] private Transform carryAnchor;
        [SerializeField] private Transform leftArm;
        [SerializeField] private Transform rightArm;

        private GameObject carriedObject;

        public bool HasCarriedObject => carriedObject != null;

        private void Awake()
        {
            if (carryAnchor == null)
            {
                var anchor = new GameObject("CarryAnchor");
                anchor.transform.SetParent(transform, false);
                anchor.transform.localPosition = new Vector3(0f, 2.35f, 0.45f);
                carryAnchor = anchor.transform;
            }

            leftArm ??= transform.Find("LeftArm");
            rightArm ??= transform.Find("RightArm");
        }

        private void LateUpdate()
        {
            if (!HasCarriedObject)
            {
                return;
            }

            if (leftArm != null)
            {
                leftArm.localEulerAngles = new Vector3(0f, 0f, 145f);
            }

            if (rightArm != null)
            {
                rightArm.localEulerAngles = new Vector3(0f, 0f, -145f);
            }
        }

        public bool TryPickup(GameObject item)
        {
            if (carriedObject != null || item == null)
            {
                return false;
            }

            carriedObject = item;
            SetCarriedObjectPhysics(item, false);

            var target = item.GetComponent<GameKitInteractionTarget>();
            target?.SetInteractable(false);

            item.transform.SetParent(carryAnchor, false);
            item.transform.localPosition = Vector3.zero;
            item.transform.localRotation = Quaternion.identity;
            return true;
        }

        public bool TryDrop(Transform dropAnchor)
        {
            if (carriedObject == null || dropAnchor == null)
            {
                return false;
            }

            var item = carriedObject;
            carriedObject = null;
            item.transform.SetParent(null, true);
            item.transform.SetPositionAndRotation(dropAnchor.position, dropAnchor.rotation);
            SetCarriedObjectPhysics(item, true);

            var target = item.GetComponent<GameKitInteractionTarget>();
            target?.SetInteractable(true);
            return true;
        }

        private static void SetCarriedObjectPhysics(GameObject item, bool enabled)
        {
            foreach (var itemCollider in item.GetComponentsInChildren<Collider>())
            {
                itemCollider.enabled = enabled;
            }

            var rigidbody = item.GetComponent<Rigidbody>();
            if (rigidbody != null)
            {
                rigidbody.isKinematic = !enabled;
                rigidbody.useGravity = enabled;
            }
        }
    }
}
