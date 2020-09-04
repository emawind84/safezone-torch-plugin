# safezone-torch-plugin
Protects grids near a large ship or station using a Safe Zone block

## How to use

1. Build a Safe Block, it doesn't need to have zone chips.

2. In the custom data of the block, insert the tag `[safezone]`, after a few seconds the name of the block will change telling you that the safe zone is enabled, switching off the safe zone will change the status on the name.

After the safe zone is enabled the grid will not take damage and will keep being like this until you remove the safe zone or remove the tag from the custom data of the block.

If a ship get close to the grid with the safe block enabled, a message will show up to the player piloting that ship telling that the damage protection is active. Leaving the safe zone will be notified as well.
