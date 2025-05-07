# PolyHeist

## Group Members & Roles

- **[Alfredo Flores]** – Player interaction system, initial level design (city layout, jewelry store placement), asset integration from the Unity Asset Store.
- **[Chris H.]** – Advanced level design (filling in missing pieces of the city to make it feel more immersive).
- **[Nick Russert]** – Enemy AI system design (police patrol, and chase behavior), early pathfinding setup.
- **[Arturo Guerzoni]** – Audio integration planning

---

## Game Description

**PolyHeist** is a first-person heist game set in a low-poly city, where the player plans and executs a robbery of a jewelry store guarded by police AI. Players must locate the target store, enter stealthily or go in guns-blazing, steal items of value, and escape with the loot before reinforcements arrive. Once the money is secured, players must locate their getaway vehicle and trigger the final escape sequence, which summarizes the amount stolen and the time taken.

Gameplay emphasizes **immersive interaction**, and **reactive NPC behavior**. Shoppers will panic and freeze in place during the robbery, while police AI will chase the player down. Interactions are initiated via prompts — for example, pressing `E` when looking at a lootable item allows the player to steal it.

We will be incorporating the following X-Factor from the topics learned in this course:
- **Multiplayer**: We plan to support 2-player cooperative gameplay
- **Variety of Enemies**: Our enemy system includes police AI with patrolling, and chase states, as well as reactive civilians who panic during the robbery.

---

## Foreseen Challenges

- **Multiplayer Support**: We would like to add two-player co-op once the core gameplay is functional, however, this will most likely be the most challenging part of the game. 
- **AI Complexity**: Getting police and civilian AI to respond intelligently may require tuning or advanced AI state machines.
