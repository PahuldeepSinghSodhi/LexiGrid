# LexiGrid 🧩

**LexiGrid** is a mobile-optimized 2D word puzzle game built entirely in Unity (C#). This project serves as a comprehensive showcase of clean software architecture, performant custom data structures, robust UI state management, and rigorous quality assurance practices.

## 🚀 Overview

Players connect adjacent letter tiles (horizontally, vertically, or diagonally) to form valid words within a time limit. The game features dynamic board generation ensuring at least one solvable word, configurable grid sizes (3x3, 4x4, 5x5), customizable timers, and a persistent local leaderboard.

## 🏗️ Technical Architecture

LexiGrid was engineered with scalability and performance in mind, avoiding Unity's heavy monobehavior overhead where possible in favor of pure C# data structures.

### Performant Data Structures
* **Trie (Prefix Tree):** The entire 350,000+ word dictionary is parsed and loaded into a custom Trie. This allows for lightning-fast `O(L)` validation for both exact word matches and valid prefix checking while the player drags their finger across the board.
* **Min-Heap:** The top 10 high scores are managed using a custom Min-Heap implementation, ensuring `O(log N)` insertions and avoiding the overhead of sorting full lists every time a score is submitted.
* **Weighted Letter Distribution:** The board generator utilizes a standard English letter frequency distribution algorithm rather than pure random generation, ensuring balanced and playable boards.

### UI & State Management
* **State Machine Pattern:** Game states (Splash, Main Menu, Playing, Game Over) are decoupled from the core logic and managed via a central `GameManager`.
* **Event-Driven Input:** Touch/Mouse input is handled natively through Unity's EventSystems (`IPointerDownHandler`, `IDragHandler`, `IPointerUpHandler`), providing seamless native mobile touch support out of the box.
* **UI Transitions:** Screens are swapped using CanvasGroups, allowing for smooth alpha-fade transitions without the performance hit of instantiating/destroying UI elements at runtime.
* **Data Persistence:** Player preferences (Grid Size, Timer) and Leaderboard JSON serialization are securely saved using local application persistent data paths.

## 🧪 Quality Assurance & Testing

A strong emphasis was placed on testability and QA documentation throughout development:
* **Automated Unit Tests:** The project includes a comprehensive suite of Unity Test Framework scripts covering the Trie, MinHeap, Scoring Engine, and Board Generator logic.
* **QA Artifacts:** Detailed testing documentation is included to demonstrate professional QA processes:
  * **Test Plan:** Covering boundary values, state transitions, and edge cases.
  * **Bug Log:** Tracking resolved integration and logic errors.
  * **Regression Reports:** Ensuring stability across major refactors.

## 🛠️ Getting Started

### Prerequisites
* **Unity Editor:** Version 6000.5.10f1 (Unity 6.5) or newer.
* **Modules:** Windows/Mac Standalone Build Support (or Android/iOS Build Support for mobile deployment).

### Running the Project
1. Clone this repository: `git clone https://github.com/PahuldeepSinghSodhi/LexiGrid.git`
2. Add the project folder in Unity Hub and open it.
3. Open the main scene located at `Assets/Scenes/GameScene.unity`.
4. Ensure your Game View is set to a Portrait aspect ratio (e.g., `1080x1920`).
5. Press **Play** in the editor.

## 📱 Mobile Deployment
While currently built for the standalone target, the codebase is 100% mobile-ready. The responsive UI anchors and EventSystem drag handlers require zero code modifications to compile and run smoothly on Android or iOS. Simply switch the Build Target in Unity's Build Settings and export the APK/Xcode project.
