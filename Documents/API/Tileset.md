# Tileset

| Property | Type | Description | Default Value |
| - | - | - | - |
| id | String | The Content ID of this tileset. | |
| texture_path | Content Path | Where the texture to this Tileset can be found. | |
| tile_count | String | How many tiles this tileset has. | 100 |
| tile_size | Vector 2 | The width and height of each tile within this set. | 16, 16 |
| editor_groups | Dictionary | Groups sof related tiles which can be cycled through in the map editor for convenience. (I.e, all wall corners are in their own group so you can quickly tab to switch through them). | |
| tiles | Array | A list of the data for each tile. (See below) | |

## Tile Properties
| Property | Type | Description | Default Value |
| - | - | - | - |
| index | Int | The index (1 less than the ID) of this tile within the tileset. |  |
| tile_type | Tile Type | What kind of tile this is - mostly used in the editor for tooltips.
| is_inside | Bool | Whether this type of tile is considered to be inside of the prison by default. Guards will punish inmates who are on outside tiles when they aren't allowed to be. | `true` |
| casts_shadow | Bool | Does this tile cast and block shadows? | `false` |
| collision_polygon | Polygon | Area of this tile which has collision and is impassable. | |
| avoidance_polygon | Polygon | Area of this tile which NPCs will avoid stepping on, but are able to if forced. | |
| diggable | Bool | This tile can be dug through with digging tools like a spoon or trowel | `true` if tile has no collision polygon, `false` otherwise. |
| chippable | Bool | This tile can be chipped and destroyed with chipping tools like forks or pickaxes. | `false` |
| cuttable | Bool | This tile can be cut through and destroyed with a cutting tool like a knife. | `false` |
| electrified | Bool | This tile will shock and injure players that attempt to destroy it, whilst the generator is online | `false` |
| summon_object | Object ID | An object that will be summoned at the position of this tile when the map is loaded (only used by Jingle Cells, you shouldn't use this).
| custom | Dictionary | A dictionary of extra custom properties. | |

## Example
```yml
id: irongate
texture_path: Escapists|images/tiles_irongate.gif
tile_count: 100
editor_groups:
  checkered_tile: [1, 2]
  wall: [5, 6]
  corner_wall_corner: [7, 8, 10, 9]
  wall_three_way: [11, 12, 13, 14]
  fence: [19, 20]
  fence_corner: [21, 22, 24, 23]
  vent_wall: [27, 28, 30, 29]
  vent_wall_corner: [40, 41]
  roof_wall: [31, 32]
  roof_wall_corner: [36, 37, 35, 34]
  roof_wall_end: [52, 51, 53, 54]
  reinforced_wall: [62, 63]
tiles:
  - index: 0
    has_shadow: false
    tile_type: floor
```