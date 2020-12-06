Discord ACTbot
============

A Discord bot and corresponding ACT(Advanced Combat Tracker) plugin to post ACT output to discord.

Usage
------------

### Using an existing Discord Bot ###

If you're using an existing bot, you need the following from the bot owner/maintainer/admin:

  * Bot server address
  * Authentication password

Now DM the bot on discord with the authentication password in the form:
~~~
!auth <authPassword>
~~~
(If the bot has been configured to use a different symbol, a different character might be necessary before auth)

The bot should then respond with a message something like:
~~~
Your Discord username: xxxxx
Your Discord ID: 0000000
Registration complete. ACT key: xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx
~~~
This ACT key will be required by the plugin.

Now you can install the plugin from this repo to ACT. (ACTBOT_plugin.cs)

Once this has been installed, there should be a new tab in the plugin section titled ACTBOT_plugin.cs.
Under this heading add the Bot server address (from the admin) and the ACT key (from the Bot's DM).

The final step is to change ACT to use your character name instead of 'YOU':
This setting can be found in Options Tab>Data Correction>Miscellaneous>Default character name if not defined...
Click Apply.

If everything has worked, you should now be able to use the (hopefully self-explanatory) commands:
~~~
!parse
~~~
and
~~~
!endparse
~~~
to start and stop the parse in any text channel the bot has access to. (Again, if the symbol has been changed by the bot owner, the character used might not be !)

### Setting up a Discord Bot ###

(TODO)