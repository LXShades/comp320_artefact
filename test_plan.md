# Stage: Impostor Engine
## Requirements testing
* Does the impostor system visibly divide the scene into layers when viewed from an alternative angle?
* Are the layers correctly ordered with no overlaps/duplicates of objects?
* Do the layers adequately frame the objects within them?
* Does the impostor system advance when the player completes a level?
* Does the impostor system exhaustively advance to every state?

## Stress testing
* Does the impostor system perform above or at least on par with regular rendering?

# Stage: Game
## Requirements testing
* Can the player complete the level?
* Can the player shoot all balloons?
* Can the player play all rounds?
* Do the balloons spawn when the player enters the trigger area?
* Does the player's slingshot shoot reliably?
* Does the score count increase as the player shoots balloons?
* Does the next round begin when the player completes a level?
* Is the player stopped when the game ends?

## Stress testing
* Do spawners reliably activate if the player attempts to jump around the invisible triggers?
* Can the player climb walls?
* Can the player escape the level?
* Can the player break the slingshot functionality by spamming Shoot?
* Can the player for once have mercy on my sensitive programmer soul

# Stage: Data collection
## Requirements testing
* Do the questions all appear in sequence after each round?
* Does the balloon lifetime data appear to match the gameplay?
* Is the data associated with the active impostor configuration correctly?
* Does the questionnaire data match the user's responses?

## Stress testing
* Does the log file continue to collate the correct data when the game is run two times or more?

## Unit tests
See Scripts/Test_PlayMode/PlayModeTests source code for descriptively commented unit tests.