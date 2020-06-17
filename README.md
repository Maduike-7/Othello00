# O-hello

This is part of my game clone series, where I take a video game and recreate it in Unity, challenging myself to write all the code and create all the graphical assets (except fonts) from scratch.

This is a clone of the popular board game Othello.

---

## Objectives

### Primary objective
* learn how to create an AI with different difficulty settings

### Secondary objective(s)
* learn Blender basics
* learn about Actions

---

## Notes + other details

The AI logic is the same for all difficulties:

1. find locations of all possible valid cells to place a disc on, and stuff them into a list
2. order the list by whether or not the cell is a corner space, then by whether or not it's an edge space, then all the other normal spaces follow afterwards
3. pick a cell to place the disc on, based on a weighted random selection

The only difference is the weights across all difficulties (weighted towards a worse move on Easy, and towards a better move on Hard).
