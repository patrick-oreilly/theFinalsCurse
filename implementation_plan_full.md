# The Finals Curse - 5-Day Implementation Plan

**Goal:** Complete the vertical slice of "The Finals Curse" by Sunday midnight, featuring a single polished level, narrative elements, and a complete game loop.

## **Schedule Overview**

| Day | Focus | Key Deliverables |
| :--- | :--- | :--- |
| **Wed (Today)** | **Core Mechanics + Enemies** | Basic Enemy AI, Player Health/Damage, Collectible Logic (Purification). |
| **Thu** | **Level Design & Loop** | Level Layout, Win Condition (Purify -> Hoop), Win Screen Transition. |
| **Fri** | **UI & Polish** | Main Menu, Pause Menu, HUD (Health/Coins), Audio (SFX/Music). |
| **Sat** | **Narrative & Intro** | Intro Slideshow (Sepia Stills), Dialogue System (LeBron Hints), End Screen. |
| **Sun** | **Testing & Bug Fixes** | Playtesting, Physics Tweaks, Build Creation. |

---

## **Detailed Daily Breakdown**

### **Wednesday: Core Mechanics + Enemies (Today)**
*   **Enemy AI (Patrol):** Simple enemy that walks back and forth on a platform.
*   **Combat:**
    *   Enemy damages Player on touch (knockback + health loss).
    *   Basketball damages Enemy on hit (Enemies have HP).
*   **Purification Logic:**
    *   Track "Cursed Items" (Coins) collected.
    *   **Golden Basketball:** Ball transforms (turns Gold + emits Light) when all items are collected.
    *   **The Hoop:** Golden Ball must pass *through* the rim (Trigger) to complete level.

### **Thursday: Level Design & Game Loop**
*   **Level Design (High School Gym):**
    *   Build the tilemap (Floor, Walls, Platforms).
    *   Place Enemies and Coins.
    *   Add "Anti-Dash" zones (slow movement areas).
*   **Game Loop:**
    *   Start -> Collect Coins -> Unlock Golden Ball -> Dunk in Hoop -> Win Screen.
    *   Death -> Respawn at Checkpoint (3 Lives Total -> Game Over).

### **Friday: UI, Menus & Audio**
*   **HUD:**
    *   Health Bar (Basketball icons).
    *   Coin Counter / Purification Meter.
*   **Menus:**
    *   Main Menu (Play, Quit).
    *   Pause Menu.
    *   Game Over Screen.
*   **Audio:**
    *   Jump, Shoot, Hit, Collect SFX.
    *   Background Ambience (Crowd noise, distorted chanting).

### **Saturday: Narrative & Advanced Features**
*   **Intro Sequence:**
    *   Simple scene with static images (Slideshow) and text.
    *   "LeBron James Missing" news ticker.
*   **Dialogue System:**
    *   Trigger zones that show text from "Spectral LeBron".
    *   **Death Hints:** LeBron gives a unique hint every time the player dies.


### **Sunday: Testing & Submission**
*   **Playtesting:** Adjust jump heights, enemy speeds, and coin placements.
*   **Bug Fixing:** Resolve collision issues, UI glitches.
*   **Build:** Create the final executable for submission.

---

## **Immediate Next Steps (Wednesday)**
1.  **Create Enemy Prefab:** Sprite, Collider, Rigidbody, Patrol Script.
2.  **Implement Damage:** Player takes damage, Enemy takes damage.
3.  **Implement Collection Logic:** Count coins, unlock "Golden State".
