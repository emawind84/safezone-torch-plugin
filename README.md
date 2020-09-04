# safezone-torch-plugin
Protects grids near a large ship or station using a Safe Zone block

## How to use

1. Place the zip into the `Plugins` folder of Torch

2. When starting Torch the first time, a configuration file will be generated, you can change some settings there, there is no GUI at the moment. The file will be created in the `Instance` folder under the name `SafeZoneConfig.cfg`.

3. In the game build a Safe Block, it doesn't need to have zone chips.

4. In the custom data of the block, insert the tag `[safezone]`, after a few seconds the name of the block will change telling you that the safe zone is enabled, switching off the safe zone will change the status on the name.

After the safe zone is enabled the grid will not take damage and will keep being like this until you remove the safe zone or remove the tag from the custom data of the block.

If a ship get close to the grid with the safe block enabled, a message will show up to the player piloting that ship telling that the damage protection is active. Leaving the safe zone will be notified as well.
