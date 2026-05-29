# Hold Interaction Test Notes

The first draft is validated through `Assets/Games/MechanicsTestbed/Scenes/MechanicsTestbed.unity`.

Manual runtime checks:

1. Look at `MT_InteractionPickupCube`; prompt appears.
2. Hold `E`; progress fills and the cube attaches to `MT_Player/CarryAnchor`.
3. Step into `MT_InteractionDropZone`; prompt appears only while carrying.
4. Hold `E`; cube is placed on the drop zone and its collider is restored.
5. Approach `MT_InteractionShopKiosk`; hold `E`; the shop panel opens.

Automated coverage should be added once the interaction driver has a prefab or test scene that can run without game-specific objects.
