# Fairy Finder

See **develop** branch for current work.

## What is Fairy Finder?
Fairy Finder is a casual multiplayer tech demo game made to showcase Niantic's new Lighship ARDK 3.0.

In this demo, players will be able to host and join rooms over their local network to play a competitive game.

Houses will be shown in the shared space in the same location for all players which each have a chance of having a "fairy" hiding inside them.

The goal of the game is to get points over a series of rounds in which each player will take choose which house they think a fairy is hiding in.

At the end of the game, whoever has the most points will win and receive a fairy-themed celebration! 

## Current Development

For this project, we are specifically focused on utilizing Lightship's shared AR space tech using image tracking colocalization and navigation mesh management.

We are currently experimenting in different scenes to get each portion working to understand the basics of getting this setup.

We currently have basic object placement of our "fairies" utilizing the nav mesh management system and a reticle to indicate where the player is attempting to place them. (SampleScene)

We also have a scene in which players can choose to host or join a room to see each other's avatars move in sync with their real devices in a shared space. (SharedARTest)

We separately have started setup for the types of interactions and animations we plan on showing throughout the coarse of the game.

Our next steps are to unify these experiments to get the base gameplay working in a scene so we can start playtesting.


### Current Issues
- iPhone 15 Pro Max
	- Nav Mesh Management not working
	- need to reach out to Niantic about why/if it's known/expected

### Findings
- From experimenting with nav meshes and utilizing image tracking for the shared AR space, it seems that it might be too difficult to get everyone's devices on the same page.
	- Plan is for now to use nav meshes only locally to have some per client environment setup/local side AR agents to fill the room.
	- The only gameobjects that will be synced are the houses that the player chooses during their turn which is the main focus of the game.