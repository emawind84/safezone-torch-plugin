# safezone-torch-plugin
Protects grids near a large ship or station using a Safe Zone block

## How to use

Build a Safe Block, it doesn't need zone chips.
In the custom data insert the tag '[safezone]', after some seconds the name will change telling you if the safe zone is enabled or not.
After the safe zone is enabled the grid will not take damage and will keep being like this until you remove the safe zone or remove the tag from the custom data of the block.

If a ship get close to the grid with the safe block enabled, a message will show up to the player piloting that ship telling that the damage protection is active.
