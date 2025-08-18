# Recipes
Recipes are ways in which items can be used to make other items.

| Property | Type | Description | Default |
| - | - | - | - |
| result | string | The ID of the item this recipe produces. |
| ingredients | array | An array of the items used to craft this item. (See #Recipes for a further explanation.)
| method | string | The way this item is crafted (see below for a list). |
| intellect | int | How much intellect the player needs to be able to use this recipe (between 0 and 100). | 0 |

## Ingredient
> Recipes can use a maximum of 3 ingredients when using the crafting menu, or 1 if using a tile or object. Recipes must specify either an item or a tag.

| Property | Type | Description | Default |
| - | - | - | - |
| item | string | The ID of the item to use. | |
| tag | string | The item tag this recipe uses. | |
| consumed | bool | This item is consumed (i.e. destroyed) when this recipe is used (like with keys). | true |  

### Crafting Methods
| Workstation | Description |
| - | - |
| menu | The player's crafting menu. |
| tile | Recipe occurs by clicking on a tile (like a wall). |
| workstation | Recipe occurs by clicking on an object (like an oven). |

## Additional Fields
Some workstations provide additional fields for configuring the recipe.

### If method is 'tile'
| Property | Type | Description | Default |
| - | - | - | - |
| tile_type | string | The ID of the tile type this object can be crafted at. (See: Tile Types) |
| time | float | How long it takes for the item to be crafted in seconds. | 0 |

> Tile-based recipes require the player to interact with the tile for the amount of time the recipe takes; this is an illegal activity.

### If method is 'workstation'
| Property | Type | Description | Default |
| - | - | - | - |
| workstation_type | string | The ID of the workstation type this item is crafted at. |  |
| time | float | How long it takes for the item to be crafted in seconds. | 0 |

> Workstation-based recipes don't require players to interact with the object for the recipe duration; the object spends that time producing the item itself and presents it for collection.

> Objects specify what kind of workstation they are (if any), the oven for example uses the 'oven' workstation. This system means recipes can work on different objects that are marked as the same workstation.

# Examples

Crafting Menu (Sock Mace)
---
```json
"recipe": {
    "method": "menu",
    "ingredients": [
        {"item": "soap"}, 
        {"item": "sock"}
    ],
    "result": "sock_mace",
    "intellect": 30
}
```

Crafting Menu, Not Consumed (Cell Key Mold)
---
```json
"recipe": {
    "method": "menu",
    "ingredients": [
        {"item": "putty"}, 
        {
            "item": "cell_key",
            "consumed": false
        }
    ],
    "result": "cell_key_mold",
    "intellect": 50
}
```

Crafting Menu, Tags (Infirmary Outfit)
---
```json
"recipe": {
    "method": "menu",
    "ingredients": [
        {"item": "bleach"}, 
        {"tag": "inmate_outfit"}
    ],
    "result": "infirmary_outfit",
    "intellect": 50
}
```

Workstation (Cooked Food)
---
```json
"recipe": {
    "method": "workstation",
    "workstation_type": "oven",
    "ingredients": [{"item": "frozen_food"}],
    "result": "cooked_food",
    "time": 2
}
```

Tile (Comb Shiv)
---
```json
"recipe": {
    "method": "tile",
    "tile_type": "wall",
    "ingredients": [{"item": "comb"}],
    "result": "comb_shiv"
}
```