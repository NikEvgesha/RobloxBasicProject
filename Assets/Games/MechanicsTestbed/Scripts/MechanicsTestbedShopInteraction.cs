using UnityEngine;

namespace RobloxBasicProject.Games.MechanicsTestbed
{
    public sealed class MechanicsTestbedShopInteraction : MonoBehaviour
    {
        [SerializeField] private MechanicsTestbedHud hud;

        private void Awake()
        {
            hud ??= FindFirstObjectByType<MechanicsTestbedHud>();
        }

        public void OpenShop()
        {
            hud?.ShowShopPanel();
        }
    }
}
