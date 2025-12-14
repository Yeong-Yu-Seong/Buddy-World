# Buddy World

## Description

Buddy World is an AR pet care game where players can adopt, feed, and interact with virtual pets in an augmented reality environment. Players can choose from available pets, including a clownfish and a dachshund dog, and take care of them by feeding, petting, and learning fun facts about each pet.

Pets are spawned using physical AR tracking cards, creating an interactive experience where the real world and digital pets merge.

Walkthrough video:
[ITD_Asg1_BuddyWorld_Video](https://www.youtube.com/watch?v=hNWiz99mZb8)

---

## Platforms / Hardware Requirements

- **Operating System:** iOS 12+ / Android 8+
- **Device Requirements:** Smartphone or tablet with AR capabilities
- **Other Requirements:**
  - Internet connection may be needed for updates or fetching pet data
  - Physical AR tracking cards to spawn pets

---

## Installation / How to Run

1. Open Unity and build an **.apk** (Android) or **.xcodeproj** (iOS) file.
2. Transfer the file to your mobile device.
3. Launch the application on your AR-compatible device.
4. Click on "How To Use" button to view instructions.
5. Click on "Play" button.
6. Interact with your pet by feeding, playing, renaming, or viewing information.

---

## Key Controls

| Action        | Gesture / Input                          |
| ------------- | ---------------------------------------- |
| Feed Pet      | Tap on feed button                       |
| Play with Pet | Tap on play button → Hover pet           |
| Rename Pet    | Tap rename → Enter new name              |
| View Pet Info | Tap on pet                               |
| Spawn Pet     | Scan AR tracking card with device camera |

All interactions are touch-based on the mobile device screen.

---

## Pet Info / Fun Facts

### **Clownfish**

- **Info:**  
  Clownfish are small marine fish that live in warm shallow waters, especially in coral reefs. They have a mutualistic relationship with sea anemones - clownfish gain protection from the anemone’s stinging tentacles, while the anemone benefits from cleaning and food particles brought by the fish. Clownfish communicate through soft popping or clicking sounds.

- **Fun Fact:**  
  Clownfish can change sex, and the dominant individual in a group becomes the breeding female.

---

### **Dachshund Dog**

- **Info:**  
  Dachshunds are a breed of domestic dog known for their long bodies and short legs. Originally bred in Germany to hunt burrowing animals, they are intelligent, alert, and capable problem-solvers. Their compact size and keen senses make them adaptable to many environments.

- **Fun Fact:**  
  Their long, low body shape is specifically suited for entering tunnels while tracking prey.

---

## Bugs / Limitations

- Only two pets are currently available: clownfish and dachshund.
- Limited AR interactions: pets do not perform complex animations.
- Animations may appear delayed.
- Only one pet can be spawned at a time.
- Some older devices may experience reduced AR performance.
- Physical AR cards are required to spawn pets.
- New users may need to restart the app or re-login for data to load after signing up.
- Cooldown timer may be bugged, current solution is that I reset the timer whenever you stop viewing the pet.
- Hunger and happiness bar not loaded properly, need to rescan the card.

---

## Credits / References

**3D Models:** Sketchfab

- [Dachshund Dog](https://sketchfab.com/3d-models/dashshund-dog-cb0abb276cc247f095c0c527db987276)
- [Dog Treat](https://sketchfab.com/3d-models/dog-treat-7555ca546cb841bea3489967c13bb6a7)
- [Clownfish](https://sketchfab.com/3d-models/clown-fish-low-poly-animated-af7ba2aa41d2413098a59b21cfda79c2)

**Textures / Materials:** Sketchfab

- [Dachshund Dog](https://sketchfab.com/3d-models/dashshund-dog-cb0abb276cc247f095c0c527db987276)
- [Dog Treat](https://sketchfab.com/3d-models/dog-treat-7555ca546cb841bea3489967c13bb6a7)
- [Clownfish](https://sketchfab.com/3d-models/clown-fish-low-poly-animated-af7ba2aa41d2413098a59b21cfda79c2)
- [Grasspatch](https://static.vecteezy.com/system/resources/previews/014/572/097/original/background-of-green-grass-field-cartoon-drawing-free-vector.jpg)
- [Water](https://cdn.vectorstock.com/i/preview-1x/26/80/turquoise-water-surface-in-swimming-pool-perfect-vector-25862680.jpg)

**Audio / Music:** [Pixabay](https://pixabay.com)  
**Original Assets:** AR tracking cards, UI graphics

---

## Notes

- Ensure your device supports AR for the best experience.
- More pets and features are planned for future updates.
