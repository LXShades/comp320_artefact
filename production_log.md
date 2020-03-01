# 28/01/2020
## Overview
Testing today, the impostor system appears to be working mostly as planned in the mushroom environment. They update at the given rate and at the right resolution. However, there are a couple of caveats.

## Issues
* Depth issues, particularly when looking from above, cause several of the objects to vanish as the plane crosses the ground.
* The frame rate seems to unavoidably spike during impostor re-renders. It would be useful to test this in a complete environment.
* The height of the impostor does not appear to be accurate and expands to extremely large sizes while the camera is pointed up or down.

Furthermore, the impostor system needs to surround the player, but it currently only updates at the front.

## Next steps
The top priority is to find a scene to test the system in. A realistic scenario would be a highly detailed scene that threatens the frame rate.

The next priority depends on which of the above issues is most visually noticeable. We'll need to fix them in that order. Some of the bugs might prove too difficult in which case it may be worth continuing with the actual game's development to avoid stalls.

# 29/01/2020
## Overview
In an Agile fashion, it was felt that the features most important to the project would be the game as this is what the end user would experience. This provides the necessary context for refining the impostor system, which was taking a while to develop due to a lack focus.

We found and downloaded a scene for the game yesterday to begin this context. To get the game up and running, the character movement and slingshot ability were implemented, but collision wasn't finished.

We initially tried to create acceleration and friction curves for character movement. This required an InverseEvaluate function for curves. Unfortunately, after much time and unit testing (UnitTests.cs) the reliability of these functions proved too low. Furthermore, the behaviour of an acceleration curve when accelerating in the opposite direction was quite undefined in concept. Eventually, we didn't achieve properly-functioning curve-based movement and decided to return to a standard linear acceleration/friction model, noting the time investment to returns ratio.

## Next steps
The next priority is to fix the collision and add the balloon enemies. Once the game is playable, we still need to add the end states and questionnaires.

The impostor system will take massive amounts of refinement to get right, and we feel it would be best to commit this early time period to the game and surveys as we are able to finish those in a comparatively short time span.

# 04/02/2020
## Overview
A test of the game in the main level shows that the collision system is functional enough to continue to the next stage of the game development. We can reliably move through the environment and while in special cases we will fall through the floor, we are in a good position to iterate on the core game. However, these bugs are highly problematic - sometimes falling through the floor - and once the game is done some of the remaining time will be committed to fixing these.

## Today
We created the balloon enemies, spawners, spawn triggers and basic editor tools to bring this all together.

The UI was also implemented with minimal issues. There is now a timer and balloon tally.

We added the data file recording functionality today. To ensure the data formatting was functioning properly, we iterated with a unit test in UnitTests.cs. We checked the file data manually as well to confirm it did indeed contain comma-separated values.

Surveying functionality was also added, but needs some tweaks to be ready.

## Next steps
Finish surveying and round sequence. Update the shooting to be accurate to the animation.

# 05/02/2020
## Overview
A presentation of the game shows it to be in a buggy state. This is partly because the survey system was under testing in the last iteration.

- The balloons didn't spawn
- The survey showed immedately (for testing)
- The impostor system was not implemented.

The impostor system is being put on hold while we address the requirements of the game. Once the game has been outlined, it would then be sensible to continue the impostor system.

## Next steps
Finish the survey system, making sure it restarts the game at the next impostor configuration. Keep the game time limit customisable, so we can test the survey system by letting the game end immediately after beginning. Ensure the balloon spawn correctly and the end of the level functions.

# 11/02/2020
## Current stage: Game (and a bit of data collection for unit test refactoring)
## Overview
This week's session brought our attention to testing and bugtracking. As the game grows, it has become necessary to test more elements than were initially expected. We have written a specific requirements and stress test plan in test_plan.md to address this.

Unity has its own unit test runner, which we'll be using here onwards for our unit tests. We migrated the DataFile test code along the way (result: looking good!).

Other than that, the focus remains on the gameplay. We still have some requirements that are not quite satisfied; for example, being able to reliably shoot balloons. We aim to complete the requirements today.

## Next steps
- Clarify the test plan
- Complete core game loop MVP
- Conclude the day with a requirements test as specified in the Game stage of the test plan

# 12/02/2020
Yesterday's requirements test found everything working except the player was not stopped when the game ended. However, this is not a priority for next week's playtest.

The core game loop is there and functioning fairly well. Provisionary stress testing shows issues with the player clipping through objects. However, the biggest missing piece is the lack of a finished level. Finishing the level has become the next largest priority that should be absolutely achievable today. After that, it will be useful to return to the data collection phase so we can complete that as fast as possible, before using the remainder of development time on stress testing and impostor system refinement.

## Next steps
- Complete the level

# 18/02/2020
A requirements test of the previously completed version found the game to be complete with only the following areas missing:

* Is the player stopped when the game ends?

Following the correction of this small detail, the Game stage is complete asides from the minor bugs recorded in the GitHub Issue Tracker.

The Data collection stage needs to be fully requirements-tested. A provisional test showed that the data wasn't being recorded. The unit tests were still working. Thankfully, this issue was found to be the result of a minor oversight where the file path was missing. The recorded data now works, but a requirements test would do good to assure that the recorded data is valid.

Following this requirements test, the impostor system will be applied to multiple map objects. We will then run an ad-hoc stress test, playing the game with the impostor system in various random configurations to identify the most prominently visible flaws in the current iteration. The notes and bugs recorded from this test will form the groundwork for the Impostor Stage's next iteration. The expectation is that the systsem may be slow and inaccurate in a large scale, but this requires deeper observation to ascertain.

## Next steps
* Fix player being able to move after the game
* Verify data collection system
* Stress-test the impostor system

## A morsel of pain, suffering and humiliation
An early test of the entirely impostorised scene revealed a critical problem. A massive refactor of the system is required.

The current system works as follows

* Impostify components allow objects and subobjects to turn into impostors when dictated by the Impostor Manager.
* Impostors are centered in the group of objects
* All relevant meshes and sub-meshes are collected and managed by this system
* Long frame times are spent generating these impostors at once.

The ad-hoc test revealed the following concerns, ordered by highest severity:

* Rendering large chunks of the level manually takes a considerable amount of CPU time to the point that frame hitches are a compounding issue. Progressive rendering may be the best option.
* The impostor is not guaranteed to be in front of the player. For large chunks of the level to be impostorised, the real placement of the impostor needs to be configurable so it can be placed ahead of the camera.
* The resolution of the impostor is extremely low due to the massive area covered. The impostor should clamp its size to the size of the view frustum, or slightly larger, rather than the entire scene.

It should be noted that the experiment is still valid even if the frame rate drops. The research question is focused on identifying the tradeoff, but if we can instead identify that players are unlikely to notice impostors independent of framerate, that leaves room for further research on optimising the frame rate. Furthermore there remains a commercial interest in providing a deployable, reusable system that offers optimisation potential with minimal setup.

The crucial errors are around the sheer impracticality of the impostor system as it stands. It does not consider areas which the player stands within and produces a pixellated blur encompassing a much larger area than necessary.

The following subgoals are proposed to address the aforementioned problems:
* The decoupled ability to frame the impostor camera matrix to cover a specific area and specific centre point
* The decoupled ability to place this rendered frame seamlessly into the environment.

# 19/02/2020
Another ad-hoc test evaluation revealed the width and height of the impostors were not being set correctly when the impostor centre was changed. As this can cause extreme pixellation, this is a priority to fix.

## Reflection
A lot of time was spent attempting to work out how to slot the impostor into the correct segment of the screen. This was almost done before, but did not allow a custom distance and , which is useful for verifying the best distance to place each layer (e.g. maximal, minimal, centered).

Tunnel-vision occurred and we eventually decided to focus efforts on drawing debug shapes into the scene to visualise every variable that was being calculated, including impostor distance, boundaries and screen space positions. This was provided a critical advantage compraed to e.g. drawing graphs on Paint and we will use this method in the future if tunnel-vision occurs.

# 23/02/2020
Sunday work woo.

A refactor of FrameArea into FrameLayer now enables us to create 'windows' into parts of the level. Crucially they can be created at any distance, so layering is now a possibility.

Unfortunately, actual layering using native Unity functionality such as masks seems to be fundamentally slow, so we created a single-layer version where the player can look around and see mobile objects at a normal frame rate, but with the background rendering at a lower framerate.

Furthermore, the background can now be rendered continuously and progressively on a double-buffer texture to further stabilise the frame rate.

## Next steps
We still don't have a multi-layered system and it is reaching a point where re-designing the experiment is under consideration, investigating the effects of splitting the frame rate between the static background and interactive foreground. If this is shown to improve the player experience compared to a lower overall frame rate, this could theoretically be easily deployed into a commercial game.

## Reflection
Some of the impostor work lately proved fundamentally useless. During development of those features, namely the ability to group objects together, we realised we were aiming high for something that isn't within the requirements of the project.

Oops. We thought it'd be easy and allow for more expandability in the future, but it became something of a blocker.

The guilty function is FrameArea which could hypothetically frame an axis-aligned bounding box into the render texture. This would have been useful for e.g. splitting impostor workload across impostors, but since it was not in the design specification it was not useful to pursue this feature for as long as it took (several hours).

## Next steps
We'll take one shot at layering the impostors using a custom render pipeline in Unity. Yolo.

# 24/02/2020
## Overview
The custom render pipeline was successful, until we built it. Then everything turned black. It turns out this keeps happening and there is no documentation on how to avoid this happening. It strikes us as a lighting issue that we simply don't know how to fix.

Efforts will be dedicated to returning to the original rendering structure and finding ways to create multiple layers from it. A frame analysis revealed that objects outside the minimum and maximum cull distance are culled and tend to improve the frame rate, so there is merit in creating multiple impostor cameras - one for each layer - and assigning rendering tasks to each of them.

# 25/02/2020
Creating multiple layered impostors was successful. It was felt that the next priority would be to allow the main camera to render as well. Unfortunately this resulted in layering issues between the objects and impostors, inspiring a dive into depth impostors.

We have successfully created a shader that simulates this depth. This has two useful effects: firstly, non-impostor objects now interact and occlude correctly with the impostor environment. Second, the main camera can render objects that overlap with the impostors without causing problems.

This area needs some tweaking but once it's done, we can begin implementing the multi-impostor configuration setup for the main experiment.

## Testing
Due to the exploratory nature of this process, we have a new set of test conditions to meet including:

* Do the impostors correctly occlude interactive objects and each other?
* Do the depth impostors render quickly enough compared to regular impostors?

The first condition has not been met as there are visual artefacts in a small area between the main camera contents and the close layer contents. This wil be investigated by testing in a simplified scene with a mixture of impostor and non-impostor objects placed near to each other.

The second condition is uncertain. This needs testing on the laptop hardware. This will be done once the first condition is cleared.

## Next steps
* Investigate and fix aforementioned layering issues
* Profile the development build on laptop hardware
* Implement experiment impostor configurations

# 26/02/2020
## Overview
Panic! There is no time! We summarise the remaining goals for the week.

### Replanning:
* Revise the questionnaire
* Revise the choice of data collection

### Development
* Make final optimisations to try and gain frame rate boost.
* Add non-relative questions using a red marker instead to show previous answer

### Polish
* Run a stress test and report to buglist
* Fix all known bugs (rocket bosting)

## On questionnaire:
The questionnaire's objective is to determine whether a mixed frame rate would be a favourable option for players compared to a low frame rate overall.

Some tidbits from the Game Experience Questionnaire
* I felt skilful
* I thought it was fun
* I felt competent
* It was aesthetically pleasing
* I felt good
* I was fast at reaching the game's targets
* I felt pressured
* I felt challenged
* I felt frustrated

Some additional ones to look at
* The gameplay felt smooth

## On data collection
We currently collect FPS and player movement. But that may not be indicative of player performance.

Claypool's 2007 study measured major differences in the number of times a bot was shot.

## Stress test
Game stress test reveals the player can escape the level. This calls for more INVISIBLE WALLS.

# 27/02/2020
## Overview
Playtest time. We'll add the requirements above to the test conditions and continue shortly. This is a basic requirements test to reevaluate current priorities.

Asides from those, tasks today will likely include:
* Add accuracy to gameplay data collection
* Fix FPS in data collection
* Adjust map to place balloons in front of buildings, etc
* Add the red tick to the questionnaire responses
* Reduce number of questions
* Optimise, optimise, optimise

# 28/02/2020
Adding in the red tick.

Then finalising some example configurations

# 1/03/2020
A final requirements test and stress test is conducted before pilot testing. See readme.

The results showed satisfactory functionality across all domains with a few exceptions. The main exceptions include layers containing duplicate objects (where objects are rendered into the foreground layer as well as the first impostor layer) and some errors with the depth buffer dump.

The game itself is felt satisfactory for pilot testing. We feel it would be useful to include a 'base' imposotor configuration as per the original design, so this has been included.