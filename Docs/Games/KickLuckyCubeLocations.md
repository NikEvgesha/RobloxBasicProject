# Kick Lucky Cube: Location Bible

Status: canonical world-design brief for the 30-location corridor. All 30 locations have a connected first-pass voxel ProBuilder prefab; final art refinement remains in progress.

This document defines what every location represents, which large landmarks and small props belong in it, which ambient effects distinguish it, and which animals the current runtime catalog can award there. Use it when creating the biome prefabs under `Assets/Games/KickLuckyCube/Prefabs/World/Biomes`.

## Shared Rules

- The corridor has 30 locations. Location 1 begins at kick distance `7m`; location centers are spaced by `48.4m`; every prefab keeps that fixed total length.
- Prefab slots `01-30` and their Edit Mode preview instances are populated. Locations `06-30` can be rebuilt in batches with `Assets/Games/KickLuckyCube/Editor/KickLuckyCubeBiomeBatchBuilder.cs`; rebuilding replaces those prefab assets and must not be run after a designer starts manual refinement without first preserving that work.
- Use approximately the first `35.8m` for the dry biome and the final `12.6m` of the same prefab for its river transition. Never increase location spacing to make the river wider.
- `KLC_CorridorGameplayData_DoNotDelete` owns the corridor's world position. `FirstLocationAnchor`, `AuthoringBiomeInstances`, and every biome preview must stay on local `Y=0`; never compensate for the corridor position by raising prefab roots or individual instances.
- Every biome prefab owns a `Walls` group. `BiomeWall_Left` and `BiomeWall_Right` are editable ProBuilder blocks, start at local floor height, are `22m` tall, cover the full `48.4m` location length, and use that biome's wall material. Keep their inside faces aligned with the floor edges at `X=-39` and `X=39`.
- Do not reserve an empty center road. Stagger gameplay obstacles across the width so the runner must change direction, while preserving at least two readable routes around every obstacle group.
- Every river is authored with three long bridge choices. All three remain visible in Edit Mode; `KickLuckyCubeRiverTraversalZone` randomly leaves exactly two active in Play Mode.
- Water outside an active bridge applies `0.9x` animal movement speed and a `0.22m` visual sink. Active bridges use normal movement and normal ground height.
- Every biome prefab needs `KickLuckyCubeBiomeAuthoring`, `GameplayFloor`, `StartAnchor`, `EndAnchor`, `Landmarks`, `Details`, and `VFX` references.
- Only the assigned gameplay floor and deliberate traversal objects should collide with the runner. Grass, flowers, particles, signs, and distant landmarks should normally be non-colliding.
- The objects listed below are environmental props, not collectible inventory items. The only location reward currently implemented is one animal from the listed pool.
- Every third location should read as a stronger visual milestone because wave-speed messaging changes in three-location groups.
- Avoid repeating the same silhouette in adjacent locations. At least one large landmark must make each biome identifiable from the launch line and during the cube flight.
- `KLC_ExtendedCorridorFloor_30Zones` remains a temporary fallback collider for compatibility. All 30 biome gameplay floors now pass the automated contract audit, but the fallback should stay until cube landing, camera, and runner traversal have been manually approved across the complete route.

## Animal Progression Rules

- The pool comes from `KickLuckyCubeAnimalCatalog.CreateLocationOptions(locationIndex)` and is authoritative for runtime behavior.
- Income is linear: `30 + progressionIndex * 6` soft currency per second.
- Sell value is currently `18` seconds of that animal's base income.
- Grades are `Normal`, `Golden`, `Diamond`, and `Fire`. Grades change tint, size, glow, and VFX; income is already encoded by progression index and is not multiplied again.
- The three elite shop animals and the rating gift animal never appear in corridor pools.

## Locations 01-10: Natural World

### 01. Start Meadow

- Prefab: `Assets/Games/KickLuckyCube/Prefabs/World/Biomes/KLC_Biome_01_StartMeadow.prefab` (`VOXEL OBSTACLE PASS`, connected to corridor slot 01).
- Identity: welcoming bright-green meadow that teaches the player the corridor language without visual noise.
- Large landmarks: one broad oak, a low stone arch, and a distant windmill silhouette.
- Details: short grass clumps, daisies, hay bales, fence fragments, smooth stones, and a few directional flags.
- Traversal: there is no reserved center road; hedges, fences, hay, rocks, and stumps form staggered obstacles with several side-to-side routes.
- Art rule: visible static geometry uses editable ProBuilder cubes in a low-cost voxel style; rounded primitives are intentionally avoided.
- VFX: butterflies, light pollen, and soft wind streaks.
- Animals: `Normal Cat` (`30/s`, sell `540`), `Normal Dog` (`36/s`, sell `648`), `Normal Chicken` (`42/s`, sell `756`).

### 02. Pine Forest

- Prefab: `Assets/Games/KickLuckyCube/Prefabs/World/Biomes/KLC_Biome_02_PineForest.prefab` (`FIRST VOXEL PROBUILDER PASS`, connected to corridor slot 02).
- Identity: cool evergreen forest with a narrower, taller silhouette than the meadow.
- Large landmarks: groups of tall pines, a fallen trunk crossing the background, and a stump cluster.
- Details: pine cones, ferns, needles, mossy rocks, small logs, and wooden trail signs.
- Traversal: fallen trunks, log barriers, rocks, and stump clusters alternate across the full playable width instead of leaving a center road.
- Art rule: pines and props are assembled from editable ProBuilder blocks; collision is limited to gameplay obstacles and trunks.
- VFX: low morning mist and drifting needles.
- Animals: `Normal Chicken` (`42/s`, sell `756`), `Normal Bee` (`48/s`, sell `864`), `Normal Pudding` (`54/s`, sell `972`).

### 03. Flower Grove

- Prefab: `Assets/Games/KickLuckyCube/Prefabs/World/Biomes/KLC_Biome_03_FlowerGrove.prefab` (`FIRST VOXEL PROBUILDER PASS`, connected to corridor slot 03).
- Identity: saturated flower garden and the first strong color milestone.
- Large landmarks: one giant blocky flowering tree, stepped hedge walls, and a flower-covered arch.
- Details: dense flower patches, trimmed bushes, planters, baskets, petals, and stepping stones.
- Traversal: planters, stepped hedges, giant flowers, stones, and baskets alternate across the full width without a reserved center road.
- Art rule: visible static geometry uses editable ProBuilder cubes; the bright milestone palette changes without introducing rounded high-segment primitives.
- VFX: falling petals, pollen sparkles, and small butterfly groups.
- Animals: `Normal Pudding` (`54/s`, sell `972`), `Normal Finn` (`60/s`, sell `1080`), `Normal Bessie` (`66/s`, sell `1188`).

### 04. Desert

- Prefab: `Assets/Games/KickLuckyCube/Prefabs/World/Biomes/KLC_Biome_04_Desert.prefab` (`FIRST VOXEL PROBUILDER PASS`, connected to corridor slot 04).
- Identity: open yellow sand with strong negative space after the dense grove.
- Large landmarks: sandstone mesas, one branching cactus, and a half-buried stone gate.
- Details: blocky sand ridges, small cacti, animal bones, clay pots, dry grass, rocks, and voxel tumbleweed.
- Traversal: ridges, cactus clusters, pots, bones, rocks, and tumbleweed stagger across the dry section; the final `12.6m` remains clear for the river and three bridge choices.
- Art rule: all visible static geometry uses editable low-segment ProBuilder blocks without rounded high-poly primitives.
- VFX: ground-level dust, heat shimmer, and occasional sand gusts.
- Animals: `Normal Bessie` (`66/s`, sell `1188`), `Normal Paca` (`72/s`, sell `1296`), `Normal Champ` (`78/s`, sell `1404`).

### 05. Canyon

- Prefab: `Assets/Games/KickLuckyCube/Prefabs/World/Biomes/KLC_Biome_05_Canyon.prefab` (`FIRST VOXEL PROBUILDER PASS`, connected to corridor slot 05).
- Identity: red-rock passage framed by high vertical shapes.
- Large landmarks: themed corridor walls, layered canyon buttresses, a natural stone arch, and two tall hoodoos.
- Details: angular boulders, cracked plates, dry shrubs, a broken cart, rope posts, and scattered pebbles.
- Traversal: rocks, plates, cart pieces, ropes, and shrubs stagger across the dry section; the river section stays clear except for its three bridge choices.
- Art rule: Canyon walls and all visible static props are editable ProBuilder blocks using the Canyon material family.
- VFX: reddish dust and pebbles falling from cliff edges.
- Animals: `Normal Champ` (`78/s`, sell `1404`), `Normal Svinina` (`84/s`, sell `1512`), `Normal Chill` (`90/s`, sell `1620`).

### 06. Oasis

- Identity: blue-green relief zone and the second three-location milestone.
- Large landmarks: palm island, clear pool, and a rock formation with a small waterfall.
- Details: reeds, lily pads, flowering bushes, clay jars, woven mats, and wet stones.
- VFX: water sparkle, drifting droplets, and palm leaves moving in the wind.
- Animals: `Normal Chill` (`90/s`, sell `1620`), `Normal Wolfle` (`96/s`, sell `1728`), `Normal Bailey` (`102/s`, sell `1836`).

### 07. Swamp

- Identity: humid green swamp with twisted silhouettes and shallow water.
- Large landmarks: giant cypress tree, exposed root bridge, and a leaning wooden shack.
- Details: reeds, lily pads, logs, moss, mud bubbles, cattails, and small mushrooms.
- VFX: green fog, fireflies, water ripples, and occasional bubbles.
- Animals: `Normal Bailey` (`102/s`, sell `1836`), `Normal Stripey` (`108/s`, sell `1944`), `Normal Gigi` (`114/s`, sell `2052`).

### 08. Dark Marsh

- Identity: darker continuation of the swamp where the first Golden animals begin appearing.
- Large landmarks: dead trees, a crooked ruin, and a black-water basin.
- Details: pale mushrooms, broken lanterns, roots, skull-shaped stones, thorn bushes, and tar puddles.
- VFX: dark ground fog, violet wisps, and dim floating spores.
- Animals: `Normal Gigi` (`114/s`, sell `2052`), `Golden Cat` (`120/s`, sell `2160`), `Golden Dog` (`126/s`, sell `2268`).

### 09. Jungle

- Identity: dense tropical growth with strong vertical layering and the first Golden milestone.
- Large landmarks: giant kapok tree, vine-covered temple wall, and broad canopy masses.
- Details: vines, broad leaves, ferns, fallen logs, carved stones, fruit, and root clusters.
- VFX: humid mist, insects, falling leaves, and shafts of green light.
- Animals: `Golden Dog` (`126/s`, sell `2268`), `Golden Chicken` (`132/s`, sell `2376`), `Golden Bee` (`138/s`, sell `2484`).

### 10. Bamboo Valley

- Identity: ordered green valley with repeating bamboo rhythm and cleaner geometry.
- Large landmarks: tall bamboo walls, a wooden torii-like gate, and a raised footbridge in the background.
- Details: bamboo shoots, cut poles, flat stones, paper lanterns, raked gravel, and low shrubs.
- VFX: drifting bamboo leaves, light wind, and warm lantern motes.
- Animals: `Golden Bee` (`138/s`, sell `2484`), `Golden Pudding` (`144/s`, sell `2592`), `Golden Finn` (`150/s`, sell `2700`).

## Locations 11-20: Extreme World

### 11. Snowfield

- Identity: broad white field with simple dark-green and brown accents.
- Large landmarks: snow-covered firs, a small cabin, and high snowbanks.
- Details: snowmen, sled parts, chopped logs, footprints, ice patches, and buried fence posts.
- VFX: steady snowfall, breath-like mist, and powder blown across the floor.
- Animals: `Golden Finn` (`150/s`, sell `2700`), `Golden Bessie` (`156/s`, sell `2808`), `Golden Paca` (`162/s`, sell `2916`).

### 12. Ice Cliffs

- Identity: sharp blue ice corridor and a strong cold milestone.
- Large landmarks: high ice cliffs, a frozen arch, and several giant ice spires.
- Details: ice shards, frozen puddles, snow drifts, icicles, cracked ice blocks, and rope markers.
- VFX: cold mist, tiny ice crystals, and wind streaks.
- Animals: `Golden Paca` (`162/s`, sell `2916`), `Golden Champ` (`168/s`, sell `3024`), `Golden Svinina` (`174/s`, sell `3132`).

### 13. Crystal Cave

- Identity: enclosed violet-blue cave with emissive mineral shapes.
- Large landmarks: cave-mouth ribs, two giant crystal clusters, and a crystal bridge silhouette.
- Details: gem fragments, stalagmites, mining carts, rails, lanterns, and small crystal veins.
- VFX: crystal sparkles, slow glowing dust, and reflected light pulses.
- Animals: `Golden Svinina` (`174/s`, sell `3132`), `Golden Chill` (`180/s`, sell `3240`), `Golden Wolfle` (`186/s`, sell `3348`).

### 14. Mushroom Forest

- Identity: oversized fantasy forest with rounded silhouettes and magenta accents.
- Large landmarks: giant cap mushrooms, hollow mushroom trunk, and a fungal arch.
- Details: tiny mushroom rings, mossy logs, spores, curled plants, dew drops, and soft stones.
- VFX: floating spores, cap glow, and occasional puff clouds.
- Animals: `Golden Wolfle` (`186/s`, sell `3348`), `Golden Bailey` (`192/s`, sell `3456`), `Golden Stripey` (`198/s`, sell `3564`).

### 15. Autumn Grove

- Identity: orange-red woodland marking the transition from Golden to Diamond animals.
- Large landmarks: giant maple, covered wooden bridge, and a small gazebo.
- Details: leaf piles, pumpkins, acorns, baskets, benches, logs, and low stone borders.
- VFX: falling leaves, warm dust motes, and gusts through leaf piles.
- Animals: `Golden Stripey` (`198/s`, sell `3564`), `Golden Gigi` (`204/s`, sell `3672`), `Diamond Cat` (`210/s`, sell `3780`).

### 16. Savanna

- Identity: warm open grassland with long horizontal visibility.
- Large landmarks: wide acacia trees, termite mound, and layered rock outcrop.
- Details: dry grass, bones, low shrubs, cracked earth, seed pods, and small stones.
- VFX: heat shimmer, grass waves, and light dust.
- Animals: `Diamond Cat` (`210/s`, sell `3780`), `Diamond Dog` (`216/s`, sell `3888`), `Diamond Chicken` (`222/s`, sell `3996`).

### 17. Badlands

- Identity: harsher striped-earth landscape with little vegetation.
- Large landmarks: stepped mesas, narrow hoodoos, and an abandoned mining scaffold.
- Details: ore rocks, dry bushes, rails, mine carts, broken planks, and warning posts.
- VFX: sand sheets, small rock falls, and dry wind.
- Animals: `Diamond Chicken` (`222/s`, sell `3996`), `Diamond Bee` (`228/s`, sell `4104`), `Diamond Pudding` (`234/s`, sell `4212`).

### 18. Volcano

- Identity: black basalt and bright orange lava, forming a major danger milestone.
- Large landmarks: volcano cone, basalt gate, and smoking rock towers.
- Details: obsidian shards, lava cracks, black boulders, warning signs, and scorched debris.
- VFX: embers, smoke columns, heat distortion, and intermittent lava glow.
- Animals: `Diamond Pudding` (`234/s`, sell `4212`), `Diamond Finn` (`240/s`, sell `4320`), `Diamond Bessie` (`246/s`, sell `4428`).

### 19. Lava River

- Identity: molten river system where black traversal shapes contrast against moving lava.
- Large landmarks: wide lava channel, basalt bridge, and a lavafall wall.
- Details: basalt columns, magma rocks, chains, charred posts, obsidian plates, and cooled crust.
- VFX: lava bubbles, sparks, embers, and rising heat waves.
- Animals: `Diamond Bessie` (`246/s`, sell `4428`), `Diamond Paca` (`252/s`, sell `4536`), `Diamond Champ` (`258/s`, sell `4644`).

### 20. Ash Wastes

- Identity: desaturated aftermath of the volcano with strong black silhouettes.
- Large landmarks: charred giant tree, ruined furnace, and collapsed stone tower.
- Details: ash piles, burnt logs, broken metal, cracked statues, chains, and dead shrubs.
- VFX: falling ash, low smoke, and rare red embers.
- Animals: `Diamond Champ` (`258/s`, sell `4644`), `Diamond Svinina` (`264/s`, sell `4752`), `Diamond Chill` (`270/s`, sell `4860`).

## Locations 21-30: Fantastical World

### 21. Beach

- Identity: bright reset after the dark wastes and a readable turquoise milestone.
- Large landmarks: leaning palms, lighthouse silhouette, and a coastal rock arch.
- Details: shells, umbrellas, driftwood, towels, buckets, sandcastles, and coral pieces.
- VFX: sea foam, water sparkle, wind in palms, and light spray.
- Animals: `Diamond Chill` (`270/s`, sell `4860`), `Diamond Wolfle` (`276/s`, sell `4968`), `Diamond Bailey` (`282/s`, sell `5076`).

### 22. Pirate Cove

- Identity: adventurous cove with shipwreck shapes and a darker teal palette.
- Large landmarks: wrecked ship, skull-shaped cave entrance, and broken mast.
- Details: treasure chests, barrels, crates, cannons, anchors, ropes, coins, and torn flags.
- VFX: sea mist, drifting map scraps, lantern motes, and occasional cannon smoke.
- Animals: `Diamond Bailey` (`282/s`, sell `5076`), `Diamond Stripey` (`288/s`, sell `5184`), `Diamond Gigi` (`294/s`, sell `5292`).

### 23. Candy Land

- Identity: first Fire-grade transition, using edible shapes and high color contrast.
- Large landmarks: candy castle, giant lollipop grove, and chocolate waterfall.
- Details: gumdrops, wafer fences, candy canes, frosting rocks, wrapped sweets, and cookie signs.
- VFX: sugar sparkles, floating sprinkles, and soft candy-cloud puffs.
- Animals: `Diamond Gigi` (`294/s`, sell `5292`), `Fire Cat` (`300/s`, sell `5400`), `Fire Dog` (`306/s`, sell `5508`).

### 24. Toy City

- Identity: oversized playroom city and a strong Fire milestone.
- Large landmarks: block skyscrapers, toy train, and a giant wind-up robot silhouette.
- Details: building blocks, cones, springs, balls, dominoes, puzzle pieces, and toy road signs.
- VFX: confetti, wind-up sparks, and bouncing dust puffs.
- Animals: `Fire Dog` (`306/s`, sell `5508`), `Fire Chicken` (`312/s`, sell `5616`), `Fire Bee` (`318/s`, sell `5724`).

### 25. Neon City

- Identity: dark urban floor with cyan, magenta, and yellow emissive accents.
- Large landmarks: voxel skyscrapers, holographic gate, and elevated rail silhouette.
- Details: neon signs, vents, cables, crates, street barriers, screens, and glowing road markings.
- VFX: hologram scan lines, light rain, steam vents, and animated sign flicker.
- Animals: `Fire Bee` (`318/s`, sell `5724`), `Fire Pudding` (`324/s`, sell `5832`), `Fire Finn` (`330/s`, sell `5940`).

### 26. Sky Isles

- Identity: bright clouds and floating land masses with a strong sense of height.
- Large landmarks: floating islands, cloud arch, and a waterfall falling into the void.
- Details: cloud rocks, windmills, suspended bridges, banners, feathers, and small floating stones.
- VFX: cloud wisps, wind trails, distant birds, and drifting light particles.
- Animals: `Fire Finn` (`330/s`, sell `5940`), `Fire Bessie` (`336/s`, sell `6048`), `Fire Paca` (`342/s`, sell `6156`).

### 27. Magic Ruins

- Identity: ancient purple-green sanctuary and the final major earthly milestone.
- Large landmarks: ruined temple, rune pillars, and broken circular portal.
- Details: cracked columns, books, candles, crystal fragments, chains, vines, and carved floor tiles.
- VFX: floating runes, magical wisps, candle motes, and portal pulses.
- Animals: `Fire Paca` (`342/s`, sell `6156`), `Fire Champ` (`348/s`, sell `6264`), `Fire Svinina` (`354/s`, sell `6372`).

### 28. Moon Crater

- Identity: pale lunar surface with black sky and sparse man-made accents.
- Large landmarks: crater rim, lunar module, and a tall alien monolith.
- Details: moon rocks, flags, satellite dishes, footprints, sample crates, and broken antenna parts.
- VFX: slow dust, small floating debris, star glints, and subtle low-gravity motion.
- Animals: `Fire Svinina` (`354/s`, sell `6372`), `Fire Chill` (`360/s`, sell `6480`), `Fire Wolfle` (`366/s`, sell `6588`).

### 29. Cosmic Rift

- Identity: unstable space tear with floating geometry and the highest danger read.
- Large landmarks: giant rift portal, asteroid arch, and fragmented floating platform silhouettes.
- Details: crystal shards, meteor fragments, broken rings, star stones, cables, and dark-energy cracks.
- VFX: distortion, star particles, pulsing lightning, and slow orbiting debris.
- Animals: `Fire Wolfle` (`366/s`, sell `6588`), `Fire Bailey` (`372/s`, sell `6696`), `Fire Stripey` (`378/s`, sell `6804`).

### 30. Divine Garden

- Identity: final aspirational location combining white stone, gold, saturated greenery, and celestial light.
- Large landmarks: enormous celestial tree, marble temple, and open golden gates framing the endpoint.
- Details: fountains, hedges, white flowers, statues, gold-trimmed benches, floating steps, and ceremonial banners.
- VFX: god rays, gold petals, fountain mist, soft aura rings, and slow ascending sparkles.
- Animals: `Fire Bailey` (`372/s`, sell `6696`), `Fire Stripey` (`378/s`, sell `6804`), `Fire Gigi` (`384/s`, sell `6912`).

## Prefab Naming And Build Order

Use these prefab names so the corridor sequence remains sortable:

```text
KLC_Biome_01_StartMeadow.prefab
KLC_Biome_02_PineForest.prefab
...
KLC_Biome_30_DivineGarden.prefab
```

The first-pass geometry is complete for all milestone groups. Use the same groups for manual visual refinement and regression testing:

1. Locations 01-03: establish scale, lane width, material language, and river transition.
2. Locations 04-06: validate distant readability and the first faster-wave milestone.
3. Locations 07-15: complete Normal-to-Golden progression and the first grade transition.
4. Locations 16-24: complete Diamond progression and the Fire transition.
5. Locations 25-30: finish the high-value fantasy endpoint.

After editing a prefab, keep it assigned to the matching slot on `KLC_CorridorGameplayData_DoNotDelete`, rebuild only the non-destructive preview if needed, run `Validate All`, and test cube landing plus runner return across both neighboring transitions. Do not move the prefab root vertically; the corridor data root owns the world height.
