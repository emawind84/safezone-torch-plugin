# safezone-torch-plugin
Protects grids near a large ship or station using a Safe Zone block

## How to use

1. Place the zip into the `Plugins` folder of Torch

2. When starting Torch the first time, a configuration file will be generated, you can change some settings there, there is no GUI at the moment. The file will be created in the `Instance` folder under the name `SafeZone.cfg`.

3. In the game, build a Safe Block on the grid where you want to activate the protection, the block doesn't need zone chips to works.

4. In the custom data of the block, insert the tag `[safezone]`, after a few seconds the name of the block will change telling you that the safe zone is enabled, switching off the safe zone will change the status on the name.

After the safe zone is enabled, the grid with the block will not takes any damage until the safe zone block is removed or the tag from the custom data is removed. Also all the ships that approach the main grid will have damage disabled.

If a ship get close to the grid with the safe block enabled, a message will show up to the player piloting that ship telling that the damage protection is active. Leaving the safe zone will be notified as well and the damage will be re-enabled for that ship.
