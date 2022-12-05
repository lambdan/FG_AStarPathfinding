# FG_AStarPathfinding

Unity version: 2021.3.8f1

- Two scenes available: Grid Based and No Grid. 
  - The A Star algorithm is basically the same for both.
    - No Grid uses Vector2s and checks bounds of obstacles to calculate a path.
    - The grid version uses a grid generated at runtime
 
 # Grid version
 
- You can toggle between allowing diagonals or not. 
- The blocked squares are random. Only square that is guaranteed open is where the player is.
  - You can specify how big the chance is for a square to be blocked in the Unity Editor
- You cannot randomize the grid while the player is moving

![2022-12-05_10 32 10 739_Unity](https://user-images.githubusercontent.com/1690265/205603061-ab59da46-e704-4365-8f7a-0fc3826a9b60.png)

 # No Grid version
- You can specifiy precision. The default value is 0.5
  - Lower values = slower = more precise corners
- Feel free to add/remove obstacles. Just make sure they're in the Obstacles game object.

![2022-12-05_10 31 33 444_Unity](https://user-images.githubusercontent.com/1690265/205603031-f7605baf-e999-4ad7-94e8-91996eb124b2.png)
