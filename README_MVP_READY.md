# Chief of Sin

**MVP-ready top-down room-based roguelite built in Unity.**

You are a ghost trapped in Hell. This is not ambition, it is survival.

**You either rule Hell, or burn in it.**

In life, you were a chief of sin. In death, Hell forces you to face what you were. The long-term design still points toward the full throne climb through the **7 Deadly Sins**, **4 Horsemen**, and **Lucifer**, but the current MVP focuses on one complete vertical slice:

```text
Explore Hell
↓
Clear rooms
↓
Build your ghost
↓
Survive challenges
↓
Buy cursed tools
↓
Collect Gluttony clues
↓
Reveal the boss coordinate
↓
Defeat Gluttony
↓
Unlock the Hunger clue
↓
Reach MVP Complete
```

This README now describes the **actual MVP project state**, not only the original blueprint.

---

## Current MVP Status

**Playable MVP route: complete.**

Chief of Sin now has:

- A top-down player controller with mouse aim.
- Attack / Defense combat with ectoplasm shooting and Disappear.
- A grid-based room world with door transitions.
- Single-room loading for performance.
- Persistent room state by coordinate.
- Combat rooms with deterministic encounter generation.
- Five enemy families.
- XP, Souls, levels, upgrade branches, and save/load.
- Campfire checkpoints, recovery, death return, and repopulation.
- Hostage ghosts that can be rescued and gathered at campfires.
- Four challenge room types.
- Trade rooms with a merchant and usable items.
- A complete Gluttony boss route.
- Feedback UI: tips, pause hub, save menu, clue menu, and Sinner's Ledger.

The original blueprint imagined the full boss ladder, but the MVP intentionally stops after **Gluttony**. That makes this build a complete vertical slice rather than an unfinished attempt at the whole game.

---

## What Changed From the Original Plan

The original README described the future full game:

- 7 Deadly Sins
- 4 Horsemen
- final Lucifer boss
- full boss map progression
- broader merchant pools
- larger endgame structure

The MVP version implements the first real boss arc instead:

```text
4 unique Gluttony Challenge wins
↓
4 coordinate clues
↓
Boss room at +44, +39
↓
Gluttony fight
↓
Hunger clue: +69
↓
MVP Complete
```

### Implemented for MVP

- Combat, room traversal, persistence, save/load.
- Difficulty rings and encounter levels.
- Enemy roster from Hellpuppy through Devil's Advocate.
- Campfires, checkpoints, recovery, repopulation.
- Hostage spirits and campfire ghost population.
- Challenge rooms: Betting, Gluttony, Sloth, Lie.
- Trade room: Under the Crossroads.
- Trade items: Chronos Spell, Bloodlust Potion, Ectoplasm Potion, Horsemen Ring.
- Gluttony boss route and victory ending.
- Pause hub, tips, clues, saves, Sinner's Ledger.

### Deferred beyond MVP

- Remaining 6 Sin bosses.
- 4 Horsemen boss fights.
- final Lucifer boss fight.
- separate Main Menu scene.
- full Options menu.
- audio / graphics sliders.
- control rebinding.
- multi-save-slot architecture.
- expanded art/audio polish.
- full Steam-ready packaging pass.

The MVP cut is intentional. It keeps the project shippable and readable instead of turning the final phase into a many-headed UI hydra.

---

## Core Gameplay Loop

1. Move through a grid of rooms using north/east/south/west doors.
2. Enter combat, campfire, challenge, trade, or boss rooms.
3. Clear enemies to earn **Souls** and **XP**.
4. Spend XP levels into upgrade branches.
5. Spend Souls at **Under the Crossroads** merchant.
6. Use trade items and temporary challenge effects to survive deeper rooms.
7. Complete unique **Gluttony Challenge** rooms to collect boss clues.
8. Reveal and enter the Gluttony boss room at **+44, +39**.
9. Defeat Gluttony.
10. Receive the **Hunger Horseman clue: +69**.
11. Reach the MVP ending state.

---

## Controls

Current MVP controls:

| Input | Action |
|---|---|
| WASD | Move |
| Mouse | Aim |
| Left Mouse | Shoot ectoplasm |
| Right Mouse / Defense input | Use Disappear / defensive mode depending on current setup |
| E | Upgrade menu / contextual interaction |
| L | Open Sinner's Ledger |
| C | Open Clue Menu |
| Esc | Pause Menu / close active menu |
| 1 | Chronos Spell |
| 2 | Bloodlust Potion |
| 3 | Ectoplasm Potion |
| 4 | Horsemen Ring slot, passive death-save item |

Some exact bindings depend on the current Unity input setup, but this is the MVP control shape.

---

## Project Structure and Runtime Philosophy

Chief of Sin is built around a **systems-first Unity architecture**.

Important principles:

- The player, UI, save systems, managers, and progression systems persist.
- Rooms are disposable runtime chunks.
- Only the current room is instantiated.
- Room state lives in data, not in the room prefab.
- Visual objects are rebuilt from saved state when entering a room.
- Save/load preserves run progress instead of relying on scene objects.

This is the main reason the project can support a large coordinate map without keeping hundreds of rooms alive at once.

---

## World and Room System

The world is a coordinate grid.

```text
Start room = (0, 0)

North = (x, y + 1)
South = (x, y - 1)
East  = (x + 1, y)
West  = (x - 1, y)
```

### Runtime room loading

When the player enters a door:

1. `RoomManager` calculates the next coordinate.
2. The old room GameObject is destroyed.
3. The new room prefab is instantiated.
4. Player spawn point is selected based on entry direction.
5. Camera clamp is rebound to the new room bounds.
6. Room type and room state decide what gameplay starts.

### Persistent room state

Every room can remember:

- coordinate
- visited / cleared status
- room type
- combat level
- encounter seed
- enemy state entries
- challenge type / completion
- boss type / boss defeated
- hostage state
- campfire stored ghost count
- clear step for repopulation
- Lie challenge progress
- other room-specific flags

This prevents rooms from becoming random soup after save/load or re-entry.

---

## Room Types

The MVP currently supports these room types:

### Combat Rooms

Combat rooms are the main source of XP and Souls.

Behavior:

- Doors lock while enemies are alive.
- Enemies spawn from deterministic encounter state.
- Killing all saved enemies clears the room.
- Cleared rooms unlock doors.
- Dead enemies stay dead until repopulation.
- Runtime-only summons do not become saved room enemies.

### Campfire Rooms

Campfires are the run's sanctuary rooms.

Behavior:

- No combat.
- Activate checkpoint on entry.
- Restore HP/AP to current max.
- Save checkpoint state.
- Death returns the player to the last activated campfire.
- Rescued hostage ghosts gather here.
- Campfire ghost HUD shows stored ghost count, up to the current cap.

### Challenge Rooms

Challenge rooms are dangerous side encounters.

Implemented challenge types:

- Betting
- Gluttony
- Sloth
- Lie / Lucifer

Challenge effects usually last until the next challenge room. Lie is the exception because it can chain forced trials and temporarily stack effects inside the same Lucifer sequence.

### Trade Rooms: Under the Crossroads

Trade rooms contain a friendly demon merchant.

Behavior:

- No combat.
- Doors stay open.
- Player approaches merchant and presses E.
- Shop opens and blocks gameplay input.
- Player buys items with Souls.
- Item counts persist through save/load.
- Owned items appear on the HUD and can be used with hotkeys.

### Boss Rooms

Boss rooms are special coordinate-driven rooms.

Current MVP boss room:

```text
Gluttony Boss Room = (+44, +39)
```

Before the boss is unlocked, that coordinate behaves like a normal room. After 4 Gluttony clues are collected, the coordinate becomes a Gluttony Boss room.

---

## Combat System

The combat foundation is built around **Attack vs Defense**.

### Attack

The player shoots ectoplasm projectiles.

- Shooting costs AP normally.
- DP controls ectoplasm damage.
- Bloodlust Potion temporarily removes AP shooting cost.

### Defense

The player uses **Disappear**.

Disappear is the key survival tool:

- Makes the player temporarily invulnerable / intangible.
- Costs AP.
- Avoids many damage sources, including Gluttony's Eating Wave.
- Duration is increased by Demon branch upgrades.
- Ectoplasm Potion temporarily adds +3 seconds to Disappear duration.

### Player stats

| Stat | Meaning |
|---|---|
| HP | Health / survivability |
| AP | Action resource for shooting and Disappear |
| DP | Ectoplasm damage power |

Base run stats:

```text
HP = 10
AP = 10
DP = 2
```

Stat floors are protected so HP/AP/DP cannot collapse below safe minimums.

---

## Enemy Roster

Combat levels 1-5 introduce a growing enemy ecosystem.

### Hellpuppy

Melee chaser.

Role:

- Basic close-range pressure.
- Runs at the player.
- Bites when in range.
- Used by normal encounters and by Gluttony's boss summons.

### Vermin

Basic ranged attacker.

Role:

- Teaches movement and spacing.
- Shoots projectiles.
- Pressures the player from distance.

### Inferno

Explosive ranged enemy.

Role:

- Area denial.
- Fires explosive projectiles.
- Punishes standing still.
- Introduces AoE attack behavior.

### Warden

Shield-state enemy.

Role:

- Punishes brainless shooting.
- Alternates between attack and defense states.
- Reflects or ricochets player shots during defense mode.
- Forces the player to read enemy state.

### Devil's Advocate

Summoner and area-control enemy.

Role:

- Casts fire zones under the player.
- Summons Hellpuppies during combat.
- Uses parallel timers so fire and summons can happen independently.
- Summoned enemies are runtime pressure and should not award XP/Souls.

---

## Encounter Generation and Difficulty Scaling

The world uses ring-based difficulty.

- Rooms near the center are easier.
- Rooms farther away become more dangerous.
- Combat level is derived from coordinate distance.
- Encounter generation uses deterministic room data so save/load does not reroll rooms.

The encounter system evolved from simple spawn counts into saved enemy identity:

```text
RoomEnemyStateEntry:
- enemy type
- spawn point index
- alive/dead state
```

This allows the game to remember exactly which enemies remain in a room.

Curated encounter templates are used for early combat levels, which makes rooms feel designed rather than purely random.

---

## Progression System

XP feeds into levels.

Leveling grants:

```text
+1 upgrade point per level
```

Points can be spent in four branches.

### Demon

Focused on offense and Disappear timing.

Effects:

- Increases DP.
- Increases Disappear duration.

### Monster

Focused on durability and resource size.

Effects:

- Increases Max HP.
- Increases Max AP.

### Fallen God

Balanced growth.

Effects:

- Small DP increase.
- Small HP increase.
- Small AP increase.

### Hellhound

High-variance room execution branch.

Effects:

- Adds a chance to instantly clear normal combat rooms on entry.
- Does not apply to boss fights.

The upgrade menu lets the player spend points during a run. Level-up feedback makes new points visible.

---

## Currency and Rewards

### Souls

Souls are the shop currency.

Sources:

- Normal enemy kills.
- Higher-tier enemies give more.
- Boss rewards are planned broader-system rewards, though the MVP boss route is mainly clue/ending focused.

Uses:

- Buying merchant items at Under the Crossroads.

### XP

XP feeds leveling.

Sources:

- Enemy kills.
- Challenge success / challenge completion flows.
- Certain special challenge outcomes.

Death no longer destroys level state. Death penalties are focused around Souls and run pressure rather than de-leveling.

---

## Challenge Rooms

Challenge rooms use a shared runtime shell:

- `ChallengeRoomController`
- `ChallengeResult`
- challenge runtimes
- tips panel
- gameplay blocking
- challenge completion save path
- challenge-effect application

Challenge rooms can reset after enough run steps, but unique Gluttony clue rewards cannot be farmed from the same coordinate.

---

### Betting Challenge

A hellhound race and AP wager challenge.

Flow:

1. Player enters Betting room.
2. Player is snapped to spectator/bettor position.
3. Camera frames the race.
4. Player assigns AP bets across 4 hellhound lanes.
5. Race runs.
6. Winning lane is revealed.
7. Effects are applied.

Reward / penalty shape:

- Winning AP wager becomes temporary Bonus HP.
- Losing AP wager becomes temporary Locked AP.
- Effects last until the next challenge room.

---

### Gluttony Challenge

A feed-exactly challenge.

Flow:

1. Player enters Gluttony room.
2. Fake monster/stage appears.
3. Player selects feed amount with UI controls.
4. Feeding increases monster fullness.
5. Exact target succeeds.
6. Overfeeding fails.
7. Safe feeds apply temporary HP pressure.

Current tuning notes:

- Minimum target: 8.
- Maximum target: 20.
- Minimum feed choice: 1.
- Maximum feed choice: 20.
- Success grants temporary HP based on entry HP snapshot.
- Overfeeding applies temporary AP loss.
- A guard prevents the challenge from reducing the player below safe HP behavior.

Unique successful Gluttony Challenge rooms award boss clues for the MVP route.

---

### Sloth Challenge

A one-shot timing challenge.

Flow:

1. Player enters Sloth room.
2. Timing bar appears.
3. Marker moves automatically.
4. Player presses STOP once.
5. Success if stopped inside the safe zone.
6. Failure triggers real death and checkpoint respawn.

Reward:

```text
Temporary DP x1.5 until next Challenge room
```

Sloth is intentionally brutal: one press, one judgment.

---

### Lie / Lucifer Challenge

Lie is the most complex challenge room.

The player chooses one route:

#### Sneak Past

- Player attempts to slip past Lucifer using Disappear.
- Sneak success is rolled deterministically.
- During the attempt, Disappear can become effectively unlimited.
- On failure, Lucifer delays the reveal, then exposes the player with palm magic.
- Failure applies Lie punishment through challenge effects.

#### Accept Trials

- Lucifer forces a sequence of embedded trials.
- Possible forced trials:
  - Betting
  - Gluttony
  - Sloth
- Forced trials do not count as new challenge-room entries.
- Their effects can stack inside the Lie chain.
- Mid-chain progress persists through save/load.
- Forced trial sequence avoids duplicate challenge types within the same chain.

Lie required special stage hosting, UI restoration, route persistence, camera handling, and save/load recovery.

---

## Challenge Effects

Challenge effects are managed as run-level temporary modifiers.

They can affect:

- Max HP
- Max AP
- DP
- AP locking
- temporary stat loss
- temporary multipliers
- Lie-chain stacking

The normal rule:

```text
Challenge effects clear when entering the next Challenge room.
```

Exception:

```text
Lie can stack forced-trial effects during one internal Lucifer sequence.
```

The Sinner's Ledger reads active challenge effects directly from the effect manager.

---

## Trade Rooms: Under the Crossroads

Under the Crossroads is the MVP merchant system.

### Merchant interaction

- Approach the merchant.
- Prompt appears: `Press E to trade`.
- Press E to open the shop.
- Gameplay input is blocked while shop is open.
- Cursor is shown through the shared cursor system.
- E conflicts with the upgrade menu are blocked near the merchant.

### Shop inventory

The shop sells four fixed items.

| Item | Cost | Stack | Hotkey | Type |
|---|---:|---:|---|---|
| Chronos Spell | 60 Souls | 5 | 1 | Active |
| Bloodlust Potion | 100 Souls | 5 | 2 | Active |
| Ectoplasm Potion | 35 Souls | 5 | 3 | Active |
| Horsemen Ring | 200 Souls | 5 | 4 | Passive |

Purchases:

- Spend exact Souls.
- Respect max stack.
- Refresh shop UI.
- Refresh HUD.
- Save item counts.

### Trade item HUD

Owned items appear in the HUD.

- Counts display as `current / 5`.
- Hotkeys are shown.
- Item use is blocked when gameplay input is blocked.
- Passive ring is displayed but not manually consumed.

---

## Trade Item Effects

### Chronos Spell

Hotkey:

```text
1
```

Effect:

- Freezes enemy behavior for 5 seconds.
- Enemies stop moving / attacking.
- Player can still damage enemies.
- Acts as a panic/control tool.

### Bloodlust Potion

Hotkey:

```text
2
```

Effect:

- For 15 seconds, shooting ectoplasm costs no AP.
- Supports burst offense.

### Ectoplasm Potion

Hotkey:

```text
3
```

Effect:

- For 30 seconds, Disappear gains +3 seconds duration.
- Supports defensive play and escape timing.

### Horsemen Ring

Hotkey slot:

```text
4
```

Effect:

- Passive death-save.
- If lethal damage would kill the player and at least one ring is owned:
  - consume 1 Ring
  - prevent death
  - restore full HP
  - flash player gold
  - continue current room/fight

If no ring is owned, normal death flow happens.

---

## Campfires and Death Flow

Campfires are the checkpoint backbone.

On entering a campfire:

- room is safe
- checkpoint activates
- HP and AP restore to current max
- popup feedback appears
- run checkpoint state saves

On death:

1. Death flow checks for Horsemen Ring.
2. If Ring is available, death is prevented.
3. If no Ring is available, player loses the relevant death penalty.
4. Player respawns at last activated campfire.
5. If no campfire has been activated, fallback is the start coordinate.
6. Room state remains save-safe.

This replaced the earlier same-room respawn model.

---

## World Repopulation

The world has a step-based repopulation clock.

- Normal room-to-room travel increases `runStepCount`.
- Initial load, save load, and checkpoint respawn do not count as normal travel.
- Cleared combat rooms store the step when cleared.
- Old cleared rooms can repopulate after enough travel.
- The oldest cleared rooms naturally come back first.
- Repopulated rooms regenerate encounter state safely.

This keeps the world from becoming permanently empty while avoiding random timer chaos.

---

## Hostage Ghost System

Some combat rooms contain trapped hostage ghosts.

### Hostage room behavior

- Hostage rooms are assigned deterministically.
- Trapped ghosts are visible before rescue.
- They appear inside a black containment box.
- Clearing the room rescues them.
- The box disappears.
- Ghosts escape toward a door.
- Rescue state persists.

### Campfire transfer

When hostages are rescued:

- their count transfers to the currently active checkpoint campfire
- that campfire stores ghost count in room state
- entering that campfire spawns ambient ghost residents around the fire

### Capacity

Campfires have a hostage capacity.

Current MVP tuning:

```text
Max stored hostage ghosts per campfire = 10
```

When the active destination campfire is full:

- new hostage-room assignment can be suppressed
- transfer respects remaining capacity
- campfire-only HUD shows occupancy

### Ghost dialogue

Ghosts have lightweight dialogue bubbles:

- rescue gratitude while escaping
- ambient campfire chatter
- question/answer exchanges between campfire ghosts
- dialogue is data-driven through a ghost dialogue database

This turns campfires into visible evidence that Hell is changing.

---

## Boss Route: Gluttony MVP Arc

The MVP objective is the Gluttony boss route.

### Step 1: Find Gluttony Challenge rooms

Successful unique Gluttony Challenge completions award clues.

Rules:

- Only Gluttony Challenge success awards Gluttony boss clues.
- Betting, Sloth, and Lie do not award Gluttony boss clues directly.
- Failing Gluttony does not award a clue.
- The same Gluttony room cannot award more than one clue.
- Four unique clue rooms unlock the boss.

### Step 2: Reveal coordinate clues

The Clue Menu opens with:

```text
C
```

Clue progression:

```text
0 clues: No clues found.
1 clue:  Gluttony Boss: +4_, __
2 clues: Gluttony Boss: +44, __
3 clues: Gluttony Boss: +44, +3_
4 clues: Gluttony Boss: +44, +39
```

### Step 3: Boss room override

After 4 Gluttony clues:

```text
Room (+44, +39) becomes RoomType.Boss / BossType.Gluttony.
```

Before unlock, the room behaves normally.

After Gluttony is defeated, the room remains defeated/open and does not restart the fight.

### Step 4: Gluttony boss fight

Final Gluttony loop:

```text
Spawn Hellhounds
↓
Eating Wave Telegraph
↓
Eating Wave Release
↓
Sleep
↓
Repeat
```

Boss rules:

- Doors lock during the fight.
- Camera zooms out for the boss arena.
- Boss health bar appears.
- Gluttony is immune while awake.
- Gluttony is vulnerable only while sleeping.
- Hellhounds are the main damage pressure.
- Eating Wave is the Max HP pressure.

### Hellhound summons

Current tuning:

```text
Hellhounds per summon = 5
Max alive Hellhounds = 15
```

Rules:

- Boss-owned Hellhounds do not persist in room state.
- They do not count as normal encounter enemies.
- They should not award XP/Souls.
- They clean up on boss death or room unload.

### Eating Wave

Eating Wave behavior:

- Spawns from Gluttony's mouth / wave spawn point.
- Travels toward the player.
- On hit, reduces Max HP by 1.
- Cannot reduce Max HP below 1.
- Disappear blocks the wave.
- Popup shows Max HP loss.
- Max HP loss is attempt-local and restored after the attempt ends.

### Victory

On defeat:

- Hellhounds clean up.
- Doors unlock.
- Health bar hides.
- Boss is marked defeated.
- Hunger clue unlocks.
- MVP ending flag is set.
- Victory menu opens.

Victory text:

```text
GLUTTONY DEFEATED

Gluttony's excessive injuries rupture his swollen body.
The hellhounds he had devoured tear him apart from within.

HUNGER CLUE RECEIVED
+69

MVP COMPLETE
The next hunger waits deeper in Hell.
```

After victory, the Clue Menu shows:

```text
Gluttony Boss: +44, +39
Hunger Horseman: +69, __
```

---

## Save and Load

The MVP uses JSON-backed run persistence.

Save/load preserves:

- player stats
- Souls and XP
- level progression
- branch points
- room states
- cleared/visited rooms
- room types
- room combat levels
- encounter seeds
- enemy entries
- challenge identity and completion
- active challenge effects
- trade item inventory
- checkpoint state
- campfire ghost counts
- hostage rooms and rescue status
- run step count
- boss progression
- Gluttony clue-awarded rooms
- boss unlock / defeat / MVP flags

Several systems save immediately after major milestones:

- campfire checkpoint activation
- challenge completion
- shop purchase
- boss progression changes
- victory menu open/close

---

## Feedback, Menus, and Player-Facing UI

The final MVP does not use a separate Main Menu scene. Instead, the in-game Pause Menu became the central MVP hub.

### Pause Hub

Open with:

```text
Esc
```

Final buttons:

```text
Resume
Save Game
Saves
Tips
Clues
Exit Game
```

Behavior:

- pauses time
- blocks gameplay
- shows cursor
- manual save works
- Saves menu supports load/delete/back
- Tips opens global tips panel
- Clues opens clue menu
- Exit quits build or stops Play Mode in Editor

### Saves Menu

The Saves menu provides:

- save preview
- load
- delete
- two-click delete confirmation
- back to Pause

This is intentionally simple and MVP-focused.

### Tips System

Global tips explain:

- start/welcome
- movement
- combat
- stats
- campfires
- hostages
- trade rooms
- challenge rooms
- boss rooms
- progression
- items

Tips are shown once by ID and remembered through PlayerPrefs, so new runs do not constantly replay basic tutorial text.

Start tips are prioritized before campfire tips because the starting room is a campfire room.

### Tips Codex

The Pause Hub's Tips button opens all global tips through the existing paged tips panel.

This avoids building a separate complicated codex UI for MVP.

### Clue Menu

Open with:

```text
C
```

Also accessible from the Pause Hub.

Shows:

- Gluttony boss clue progress.
- Hunger Horseman clue after MVP completion.

### Sinner's Ledger

Open with:

```text
L
```

The Ledger is a run-status menu.

It shows:

- ghost preview
- current stats
- active challenge effects
- owned trade items
- level and branch progression

Important architecture rule:

```text
The Ledger is a mirror, not a second brain.
```

It reads from existing systems instead of storing separate gameplay truth.

### UI Locking

The project uses a shared named-owner UI blocking system.

This prevents one menu from accidentally unblocking gameplay while another menu is still open.

Used by:

- Pause Menu
- Saves Menu
- Tips Menu
- Clue Menu
- Sinner's Ledger
- Trade Shop
- Challenge UI
- Victory Menu

---

## Camera

The regular room camera was widened for readability.

Current MVP note:

```text
Camera size = 7
```

Boss room camera can zoom farther out during the Gluttony fight while still respecting room clamp bounds.

---

## Main Implemented Systems by Phase

### Phase 0-2: Foundation

Implemented:

- Unity project setup.
- Bootstrap / RoomRuntime structure.
- Player movement.
- Mouse aim.
- Camera follow.
- Crosshair.
- HP/AP/DP.
- Ectoplasm shooting.
- Disappear.
- Basic combat HUD.

### Phase 3-4: Room and Combat Loop

Implemented:

- Room grid coordinates.
- Door transitions.
- Single-room loading.
- Camera clamp per room.
- Room state.
- Hellpuppy enemy.
- Combat room clearing.
- Door locks/unlocks.
- XP/Souls rewards.
- Early respawn and persistence.

### Phase 5: Progression and Save/Load

Implemented:

- XP to level system.
- Unspent points.
- Four upgrade branches.
- Branch effects on stats and Disappear.
- Hellhound execute chance.
- Upgrade UI.
- Level-up popup.
- Run save/load.

### Phase 6: Combat Ecosystem

Implemented:

- Enemy identity persistence.
- World difficulty rings.
- Deterministic encounter generation.
- Shared enemy framework.
- Vermin.
- Inferno.
- Warden.
- Devil's Advocate.
- Curated encounter templates.
- Enemy projectiles, AoE, shields, summons, fire zones.

### Phase 7: Run Structure

Implemented:

- RoomType foundation.
- Campfire room behavior.
- Checkpoint activation and persistence.
- Campfire full recovery.
- Checkpoint-based death flow.
- Step-based room repopulation.
- Campfire feedback popups.

### Hostages Phase

Implemented:

- Hostage room data.
- Deterministic hostage assignment.
- Trapped ghost visuals.
- Rescue on room clear.
- Escape sequence.
- Transfer to checkpoint campfire.
- Campfire ghost population.
- Capacity and campfire ghost HUD.
- Ghost dialogue bubbles.

### Phase 8: Challenge Rooms

Implemented:

- Challenge room type.
- Challenge type persistence.
- Challenge effect manager.
- Shared challenge runtime shell.
- Betting.
- Gluttony.
- Sloth.
- Lie / Lucifer.
- Challenge tips.
- Challenge save/load recovery.

### Phase 9: Trade Rooms

Implemented:

- Trade room type.
- Under the Crossroads prefab.
- Merchant interaction.
- Shop panel.
- Trade item catalog.
- Trade inventory.
- Purchases with Souls.
- Item HUD.
- Item hotkeys.
- Chronos Spell.
- Bloodlust Potion.
- Ectoplasm Potion.
- Horsemen Ring death-save.

### Phase 10: Gluttony Boss Route

Implemented:

- Boss progression state.
- Clue menu.
- Gluttony clue awarding.
- Guaranteed Gluttony clue path.
- Boss room override at +44,+39.
- Boss room prefab / entry shell.
- Gluttony boss loop.
- Sleep vulnerability.
- Boss health bar.
- Eating Wave.
- Hellhound summons.
- Victory menu.
- Hunger clue +69.
- MVP completion.
- Final audit.

### Feedback / Final MVP Phase

Implemented:

- UI lock foundation.
- Sinner's Ledger.
- Global tips.
- Tip placement.
- Pause menu.
- Saves menu.
- Pause hub polish.
- Tips routing.
- Clues routing.
- Start tip priority.
- Camera size adjustment.

---

## Known MVP Cuts and Limitations

This MVP is intentionally not the full final game.

Known cuts:

- Only Gluttony has a complete boss route.
- Remaining Sin bosses are not implemented yet.
- Horsemen are represented only by the Hunger clue.
- Lucifer exists as the Lie Challenge ruler, not as the final boss.
- Main Menu was cut in favor of Pause Hub.
- Options Menu was cut.
- Tips Codex uses the existing paged tip panel instead of a custom encyclopedia UI.
- Visuals are still mostly prototype / placeholder.
- Audio is partial.
- Balance values are still test-friendly.
- Build/publishing polish is not part of this README yet.

---

## Current MVP Acceptance Checklist

The project should be considered MVP-ready if the following are true:

- Player can move, aim, shoot, and use Disappear.
- Room transitions work in all directions.
- Combat rooms spawn and clear correctly.
- Doors lock during combat and unlock after clear.
- XP/Souls/levels/branches work.
- Save/load restores run state.
- Campfires activate checkpoints and restore HP/AP.
- Death returns to checkpoint.
- Repopulation works over travel steps.
- Hostage rooms rescue ghosts and transfer them to campfires.
- Challenge rooms play and resolve.
- Challenge effects apply and clear correctly.
- Trade room opens shop and sells items.
- Trade item hotkeys work.
- Horsemen Ring prevents lethal damage.
- Four unique Gluttony Challenge successes reveal +44,+39.
- Boss room transforms after unlock.
- Gluttony boss fight runs.
- Eating Wave and Hellhounds work.
- Gluttony dies only through sleep vulnerability.
- Victory menu opens.
- Hunger clue appears.
- MVP ending state persists.
- Pause, Saves, Tips, Clues, and Ledger do not leave stuck cursor/time/input locks.

---

## Development Notes

This project was built as a learning-heavy Unity roguelite project. Its main value is not only the final MVP route, but the system architecture learned along the way:

- data-first room state
- deterministic generation
- save/load-safe gameplay systems
- reusable UI blocking
- modular enemy families
- challenge runtime architecture
- run-level managers
- phase-by-phase implementation discipline

Chief of Sin is now a complete MVP slice: not the whole Hell throne war, but a functional cursed machine with an objective, progression, pressure, recovery, tradeoffs, and an ending.

The next major step would be either:

1. **MVP build and publishing preparation**, or
2. **Post-MVP expansion**, starting with another Sin boss route.
