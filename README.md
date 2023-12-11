# Fairy Finder

See **develop** branch for current work.

## What is Fairy Finder?
Fairy Finder is a casual multiplayer tech demo game made to showcase Niantic's new Lighship ARDK 3.0.

In this demo, players will be able to host and join rooms over their local network to play a competitive game.

Houses will be shown in the shared space in the same location for all players which each have a chance of having a "fairy" hiding inside them.

The goal of the game is to get points over a series of rounds in which each player will take choose which house they think a fairy is hiding in.

At the end of the game, whoever has the most points will win and receive a fairy-themed celebration! 

## Current Development

For this project, we are specifically focused on utilizing Lighthship's shared AR space tech using image tracking colocalization and navigation mesh management.

We are currently experimenting in different scenes to get each portion working to understand the basics of getting this setup.

We currently have basic object placement of our "fairies" utilizing the nav mesh management system and a reticle to indicate where the player is attempting to place them.

We also have a scene in which players can choose to host or join a room to see each other's avatars move in sync with their real devices in a shared space.

We separately have started setup for the types of interactions and animations we plan on showing throughout the coarse of the game.

Our next steps are to unify these experiments to get the base gameplay working in a scene so we can start playtesting.