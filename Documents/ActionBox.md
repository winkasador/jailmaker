# Action Box
The **Action Box** is an editor widget intend to let users quickly change something without knowing the related shortcuts or where they are supposed to change it.

You simply type what you want to happen into the box, and it will try to interpret and execute your command.

"_Change tiles to perks_" - Will set the tileset to perks.
<br>"_Grid off_" - Will hide the editor grid.
<br>"_Make island_" - Will generate an island.

This makes the editor easier to interface with and decreases the learning curve.

This is not AI, and cannot generate a map for you.

## Interpreting Commands
The action box tokenizes input, and attempts to simplify it and then extract the meaning.

Take the phrase: "_Set tileset to perks._"

We can remove the words '_to_' and '_set_', because it isn't needed to understand the intention of the command.

This leaves us with: "_tileset perks_".

We interpret tileset as referring to the Set Tileset command, and perks is the parameter which is provided.

### How are mispellings handled?
Let's say the user acidentally entered: "_Set tileset to porks._"

'_porks_' is not a recognized word, but we can figure out what it was supposed to mean via [Levenshtein distance](https://en.wikipedia.org/wiki/Levenshtein_distance "Wikipedia - Levenshtein distance").

This is where we compare the word to another word and measure how many changes would need to be made to make them the same.

We know that it's obviously not the word 'change' because it has a distance of 6, whereas 'perks' only has a distance of 1, thus we can assume that's what the user meant.

If the distance is too high (above 2), or the shortest distance is shared by multiple words, we can't be certain of intent, and abort the command.

## Commands
The following is a list of commands which can be executed through
the action box.

| Command | Parameters |
| - | - |
| Toggle Grid | On / Off |
| Toggle Shadows | On / Off |
| Set Tileset | Tileset ID |
| Set Ground | Ground ID |
| Set Music | Track ID |
| Set Prison Name | Name |
| Set Warden Name | Name |
| Set Inmate Population | Amount |
| Set Guard Population | Amount |
| Set Security Level | Minimum / Medium / Maximum / Camp |
| Set NPC Strength | Low / Medium / High |
| Set Prison Area | Inside / Outside / Either |
| Set Starting Job | Job |
| Generate Terrain | Island / Flowers / Lakes / Forest / Complex |

## Synonyms
In order to handle the different ways a command can be asked (Like "Set tiles to jungle" and "Change tiles to jungle"),
we keep a list of synonyms for a basic, universal term and swap these out during processing.

This means that both of the previous examples would simplify down to "Set tiles to jungle", which we will then process accordingly.

Below is a list of all synonyms
| Word | Synonyms |
| - | - |
| Tileset | Tiles |
| Ground | Floor |
| On | Yes, Show, Enable |
| Off | No, Hide, Disable |
| Set | Change, Switch |
| Stalag Flucht | Winter, Stalag, Snow |
| Shankton State Pen | Shankton |
| San Pancho | Sand |
| Jungle | Forest, Jungle Compound |
| Irongate | HMP Irongate |
| Black | Space, Void, Empty |