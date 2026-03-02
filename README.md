# Chief of Sin (Working Title)
**Top-down room-based RPG / roguelite** built in Unity.

You are a ghost trapped in Hell. This is not ambition, it's survival.
**You either rule the hell, or burn in it.**

In life, you were a chief of sin. In death, Hell forces you to face what you were.
Climb the ranks, master your powers, and defeat Hell's rulers: **7 Deadly Sins**, **4 Horsemen**, then the final throne-holder: **Lucifer**.

---

## Core Gameplay Loop
1. Enter a room via one of 4 doors (N/E/S/W)
2. Survive combat or a challenge
3. Earn **Souls** (currency) + **XP**
4. Spend Souls at the merchant or push deeper
5. Level up -> invest points into a build branch
6. Defeat bosses across the map
7. After 11 bosses fall, the **Lucifer Room** is revealed -> prepare or strike immediately

---

## World & Map Structure
- The world is a **grid of rooms** connected by doors in 4 directions.
- The **Start Room** is in the **center** of the map.
- Difficulty increases outward in **rings**:
  - Near center: easier enemies (Lv 1-2), lower chance of harsh events, **no bosses**
  - Further out: tougher enemies (Lv 3-5), more danger, bosses begin appearing
- Most rooms have 4 doors. Edge rooms may have fewer.

### Loading Strategy (Performance-Friendly)
Only the **current room** (and optionally adjacent) is loaded/instantiated in Unity.
All other rooms exist as lightweight saved state (seed, type, cleared, etc.).
This supports **hundreds of rooms** without turning Unity into a slideshow.

---

## Room Types
### 1) Combat Rooms 
- Main source of XP and Souls.
- Combat difficulty levels: **1-5**
- Level 6 is reserved for **Boss Rooms** (handled as special unique encounters).

### 2) Challenge Rooms 
Exactly four challenge rooms exist:
- **Betting**
- **Gluttony**
- **Sloth**
- **Lie (Lucifer)**

Rewards are powerful and temporary, and the consequences follow you into combat.

### 3) Trade Rooms: **Under the Crossroads** 
- A merchant sells **temporary boosts**.
- Purchases are made with **Souls**.

### 4) Campfire Rooms 
- **Checkpoint** + **full regeneration** up to *current max* stats.
- Pure vibe. No extra mechanics.
- Freed spirits gather here over time (see "Hostage Spirits").

---

## Combat System: Attack vs Defense
In combat rooms the ghost can switch between two modes:

### Attack Mode
- Shoot **ectoplasm** (ranged attack).
- Costs **AP** per shot.

### Defense Mode
- Use **Disappear** to dodge enemy attacks.
- Disappear duration: **1.2 seconds** (base)
- Requires **timing**: survive by phasing through attack windows.
- Costs **AP** to activate.

**AP (Action Points)** is a shared resource for attacking and disappearing.
Running out of AP limits your options and forces smarter timing.

---

## Stats & Terminology
- **HP**: survivability
- **AP**: resource spent on shooting and Disappear
- **DP**: ectoplasm damage power (shooting strength)

### Base Stats (Start of Run)
- HP = **10**
- AP = **10**
- DP = **2**
- Floors: HP/AP/DP cannot go below **1**

---

## Progression: Four Build Branches
Gaining levels grants **1 point per level**, always.
Spend points in one of four branches (cap: **30 points each**, for now):

### Demon
- +1 DP per point  
- +0.1s Disappear duration per point  
Focused on offense and survival timing.

### Monster
- +4 HP per point  
- +4 AP per point  
Focused on durability and resource dominance.

### Fallen God
- +0.3 DP per point  
- +1 HP per point  
- +1 AP per point  
A balanced path: weaker per point, but covers everything.

### Hellhound
- +0.5% chance per point to instantly kill all enemies in a combat room on entry  
- Cap: **30 points** -> max **15%** chance  
High-variance "room execution" build (bosses handled separately).

---

## Currency & Rewards
### Souls (Currency)
- Dropped by enemies only.
- Example drops:
  - Low-level enemies: **1-5 Souls**
  - High-level enemies: **7-11 Souls**
  - Bosses: **30-40 Souls**

### XP
- Enemies: **+1-2 XP**
- Challenge success: **+15 XP**
- Lucifer's special challenge route: **+25 XP**

---

## Challenge Rooms (The Four Trials)
Challenge effects last **until entering the next Challenge room**.
They do **not** stack across separate challenge rooms.

**Exception:** In the Lie room, Lucifer may force multiple challenges in sequence, allowing those rewards to stack together temporarily.

### 1) Betting: Hellhound Racing
- 4 hellhounds race.
- Player wagers **AP** on any hellhound(s).
- Mandatory wager: **15% of AP**
- Winning:
  - AP wagered on the winning hellhound becomes **Bonus HP** until next challenge.
- Losing:
  - AP wagered on losing hellhounds becomes **Locked AP** until next challenge.

Luck influences the race outcome (see "Luck").

### 2) Gluttony
- Feed a fat monster with points until it becomes full.
- Each turn until it's full:
  - player loses **1 HP** until next challenge (punishes 1-point micro-feeding).
- If overfed:
  - monster explodes -> **50% of AP is lost** until next challenge.
- If HP would drop below 1:
  - challenge fails, player moves on with **1 HP**.
- Success:
  - player gains **double the initial HP** (snapshot at entry) until next challenge.

### 3) Sloth
- A sleeping devil awaits a single killing blow.
- Player must strike when the timing bar hits the **center zone**.
- Miss:
  - devil wakes and instantly kills the player -> respawn at nearest campfire.
- Success:
  - gain **x1.5 DP** until next challenge.
- Hellhound synergy:
  - For every 5 points in Hellhound, the timing bar moves slightly slower.

### 4) Lie: Lucifer
Lucifer appears as a roaming challenge-room ruler.

You have two choices:

**A) Sneak Past (Lie)**
- Use Disappear to attempt to walk past Lucifer.
- Success: pass and gain the normal challenge reward (+15 XP).
- Failure: **HP/AP/DP halved** until next challenge.

**B) Accept Lucifer's Trial**
- Lucifer randomly forces **1-3** of the other challenges (Betting/Gluttony/Sloth).
- These can stack if multiple are forced and all are cleared.
- Reward: **+25 XP**.

Luck affects the success chance of sneaking past Lucifer, but **does not** affect how many challenges Lucifer forces.

---

## Hidden Luck Stat
Luck is computed from the player's **base stats only**:
- Based on HP + AP + DP **without temporary bonuses**.
- Used for:
  - Hellhound race outcome influence
  - Chance to successfully lie/sneak past Lucifer

---

## Bosses: 12 Total
### Tier 1: 7 Deadly Sins (Bosses 1-7)
Unique encounters spread across the dangerous rings of Hell.

### Tier 2: 4 Horsemen (Bosses 8-11)
Stronger, rarer, deeper.

### Final Boss: Lucifer (Boss 12)
After defeating the first 11 bosses:
- A special **Lucifer Room** is revealed/marked to the player.
- The player can choose to prepare further or go immediately.
- Defeat Lucifer to beat the game.

---

## Enemy Roster (Combat Levels 1-5)
- **Hellpuppies**: simple melee
- **Vermins**: ranged attackers
- **Infernos**: explosive shots
- **Wardens**: melee + ranged, with Attack/Shield modes  
  - Shield reflects/ricochets player shots back at the player
- **Devil's Advocates**: summoners + area control  
  - summon hellpuppies  
  - "Raises Hell" creates fire zones on the floor that deal damage

---

## Room Persistence & Repopulation
Cleared rooms don't instantly refill.
The world "breathes" over time:
- Recently cleared rooms stay cleared for a while.
- If the player returns much later, enemies may repopulate.
- Campfire regions are intended to feel safer (checkpoint frustration avoided).

(Exact thresholds and repop rules will be tuned during implementation.)

---

## Hostage Spirits (Vibe System)
Some combat rooms contain **hostage spirits**.
When enemies are defeated:
- hostages break free and run toward the **nearest Campfire room**.
- later, the player can see freed spirits **chilling at campfires**.

It's a visible reminder that you're not just surviving: you're changing Hell.

---

## Tech Notes (Planned)
- Unity top-down controller + mouse aim
- Room generation/state stored by grid coordinate
- Only current/adjacent rooms instantiated to keep performance stable
- Systems-first architecture (RoomManager, RunManager, Combat, Challenges, Progression)

---

## Status
Design locked for initial production.
Boss roster, room frequency rules, and merchant item pools will be expanded during development and post-build updates.