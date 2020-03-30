Exilania written by Greg Billings 1/7/2013. Build version 13-04-09 (0.04) 1733
from http://exilania.com

How to play:
1. Make sure to extract this directory somewhere (desktop or documents works just fine. Just as long as you know where that is.)
2. Go to this website: http://www.microsoft.com/en-us/download/details.aspx?id=20914 and download the "XNA 4.0 redistributable." Install that to your computer.
3. To run the game, run Exilania.exe.
4. Main screen- currently only options are single player, multiplayer join and multiplayer host. single player is probably what you want (unless you want to try multiplayer on your home network... more on that later.)
5. choose your game-mode from above and you will then be taken to the character selection screen... create a new character. to give your character a name, just start typing and it will show up, then click the create character text at the bottom.
6. you have just spawned in your world! this world will save it's state each time you exit (as will your character)

How to play:
-at the top you see 12 boxes. 10 of them are labelled 1-9 and 0, the number keys correspond to these 10 boxes.
-Your Left hand and Right hand can each hold an item. to change the left hand's items, just press the corresponding key number. to change the right hand's items, hold down shift and press the key number.
-part of the dev environment lets you spawn items to make it easy to test the game. Press F1 to pause the game, you will notice your items bar changes to a lot of useful items. press F1 again to unpause. (you know you are paused because you can't move and the clouds don't move.)
-Your pickaxe head is in slot 9, I usually put that in my right hand.
-The game world consists of 3 layers:
	-foreground
		This is the collision layer, blocks placed in the foreground will prevent passing through the block (unless it is the platform block) to work with this layer, use normal clicks
	-furniture
		this layer this layer can be a collision layer (like doors and draw bridges) press 'Q' when hovering over furniture to learn about it. furniture is placed the same way as the foreground layer is placed. Lots of furniture is interactive, try clicking on it!
	-background
		this layer is the backing you see on the ground when you dig out blocks. some block-types cannot be placed in the background. most can. (dirt and mettite cannot be, nor can wood platforms.) to place a block in the background layer, hold down the shift key while clicking (left or right depending on what you want to do.)

		e.g. if I had a pickaxe in my right hand and a red brick in my left hand, shift-left click results in placing the red brick in the background, but holding down shift right click would try to remove a block from where the mouse is.

Other controls:
	-WASD to move around in the world
	-spacebar to jump
	-F11 to toggle between fullscreen and windowed mode
	-F12 to take a screenshot and save it to the "screenshot" folder in the main folder of the game
	-F1 to pause/unpause/get resources
	-Esc to move backward through the game (including exiting)
	-I to toggle inventory
	-M to toggle messages
	-T to toggle minimap
	-Enter to type a message/finish typing a message, ESC to cancel typing a message
	-X to use the "laser drill (dev feature, cuts through all blocks between you and the mouse"
	-CTRL to pan the camera around with the mouse, double tap to return to normal view
	-S in water to swim down
	+,-, pageup,pagedown to scroll through the messages (when the message screen is open 'm')

Developer Cheat:
	in the chat box, type /give:<item>[:quantity]
		<item> - name of a physical item named within one of the following files: (blocks.txt, craft_materials.txt, furniture.txt, item_pieces.txt)
		[:quantity] - optional parameter, leaving this off will give you 10 of whatever.
		E.X. if I wanted 11 pieces of sand, I could write: /give:Sand:11.
			if I wanted 10 batteries, I could write: /give:mark 10 battery

I'm excited to see what you think!

Those are all the basics. Enjoy!
also, the numbers you see in the bottom right-hand corner are the performance measurements. they tell you how much time is being spent (on average) doing what. I would find it helpful if you told me those numbers if the game runs slowly.

