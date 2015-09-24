==== Foreword ====
Welcome to guide editing. This file explains how different elements in the content files work.
You don't necessarily have to read this all. Mimicking previous content should be more than enough for most situations.

=== Basics ===
You don't have to touch any html files. To keep editing simple, I have created a content preprocessor which converts raw content files to web pages.
This program is called DataCreator.exe and its source code is located in DataCreator folder. Feel free to check it out and fix bugs.
I suggest using DataCreator even if you don't do anything with the web pages. It has many error checks to verify your changes.

=== Folder structure ===
The base folder has this file, DataCreator.exe, its configuration files and folders.
AvailableTactics.txt: List of tactics which DataCreator recognizes. If you need any encounter specific tactics, feel free to add them.
AvailableTips.txt: List of profession/consumable/race tips which DataCreator recognizes. Editing this file requires changes to .css files and images.
DataCreator.exe: Preprocessor which converts raw content text files to web pages.
MediaSizes.txt: List of used images and their sizes. DataCreator updates this automatically with option "back".
SpecialCharacters.txt: List of special characters (like äåö) and their html representation. Web page files can't have special characters on them.
ValidatedUrls.txt: List of urls which are known to exist. DataCreator updates this automatically with option "url".
DataCreator\: Folder with DataCreator's source code.
Planning\: Folder with planning material.
Raw\: Folder with the raw content.
Raw\Dungeons\: Folder with encounter content files.
Raw\Enemies\: Folder with enemy content files.
Raw\Other\: Folder with other files such as static pages, javascript, css, images and so on.
Research\: Folder with research data.

=== DataCreator ===
The program can be run in 4 different modes. You should mostly use the default mode because it's the fastest and contains most error checks.
Other modes require a working Internet connection. Mode 1 tries to verify every link found in the content files. Keep in mind that it fails to verify some links.
Checking all links takes about 15 minutes. To reduce this time, ValidatedUrls.txt keeps list of already verified links.
Mode 2 makes a backup of every used image or video. This will download about 2GB of data so running this first time takes a while.
Downloaded files are used to update MediaSizes.txt. Technically you should run the backup whenever adding new images but it's not needed since I can do it.
Last mode combines both modes. When you have run both modes at least once it shouldn't take very long.

=== Content files ===
Content files are built using tags. Tags are used to control how DataCreator reads the file.
There are no closing tags, a new part starts when a new tag is used again.
Tags and data are separated with "=" (used because of readability). Entries within the data are separated with "|". Separate data is separated with ":".
If you want to put a comment start the line with "#".

&nbsp; means a non-breaking space. This means that your browser avoids splitting the line at this location.

########################################################################################################################################################################
########################################################################################################################################################################
		

==== Encounters ====
Each file contains all encounter for that dungeon. This is needed because some encounters are shared between different paths.
At the start of every encounter file there is a metadata section for the paths. Each path gets an own entry.
syntax:
	init="path id"|"dungeon name"|"long path name (main page)"|"short path name (navigation bar)"
example:
	init=AC1|Ascalonian Catacombs|Hodgins's Plan (path&nbsp;1)|Path 1


== Paths ==
Paths are built one by one. When building a path, any other path information is ignored. This means you can mix different paths which is required when paths share encounters.
The path information is remembered so you only have to set it when it changes.

syntax:
	path="path id"|"path id2"|"path idN"
example:
	path=AC1|AC2|AC3
	path=AC1

	
== Encounter ==
Each encounter starts with a title. Encounters end automatically when starting a new one (or when the file ends).
First title for each path is a path title which will be bigger than the others.

syntax:
	name="title"
example:
	name=Ascalonian Catacombs Story
	
== Image ==
Image/video link for the encounter. Adds a small icon after the encounter title. Size is checked from MediaSizes.txt which is generated during backup.
Multiple images or videos can be linked to one encounter by using image= multiple times. Using media= on the link adds media/dungeonimages/ to the link so it can be used for local media.

syntax:
	image="link"
example:
	image=http://wiki.guildwars2.com/images/a/a6/Hafdan_the_Black.jpg

=== Tactics and tips===
Each encounter is split to different tactics and tips. Logic is similar to paths that lines must be in order, you can mix different tactics and lines can be in multiple tactics.
If no tactic or tip is set, lines will be included in "normal" tactic. Tactic resets for every encounter so remember to set it properly.
Tactic tab will only show if you have used/defined more than one tactic.
By default new line makes a new paragraph. If you want to continue on same line add "|" to front of the line. This is useful when some parts are in multiple tactics.

syntax:
	tactic="name1"|"name2"|"nameN"
example:
	tactic=melee|ranged
	Enemies are quite dangerous.
	tactic=ranged
	| Keep some distance.
	tactic=melee
	| Stay at max melee range.


== Valid tactics ==
Possible tactics are based on play styles. Point is to try please everyone.
For boss fights you most of the time only need "ranged", "melee" and "pro" and for the rest, "normal", "skip" and "pro". Get creative when needed.
"normal" is the default fighting tactic. Use it if nothing else fits. Can be used for non-fighting with good reason (like ACP1 2nd scepter).
"alternative" can be used for a different tactic.
"skip" is naturally used for skipping tactics. It should be used with "normal".
"ranged" is meant for ranged tactics in pugs (particularly for boss fights). Can be used to replace "normal".
"melee" same as above but for melee.
"coordinated" is meant for group-based tactics. Can be used with "normal" or "ranged".
"exploit": Cheating. Any bug or exploit which gives player advantage.
"optional": Not really a tactic but explains optional parts for the dungeon. For example achievements and secret events.
"fight": Probably not needed but can be used if running is the normal tactic for people who want full clear.
"solo" For solo specific stuff.


== Valid tips==
"guardian"
"elementalist"
"engineer"
"mesmer"
"necromancer"
"ranger"
"revenant"
"thief"
"warrior"
"consumable"
"asura"
"charr"
"human"
"norn"
"sylvari"

If you need anything else just add them to AvailableTactics.txt or AvailableTips.txt near DataCreator.exe




== Enemy links ==
Enemies can be linked so that clicking opens their info on detail box. To create a link write "enemy=" and then enemy name, category and level separared with ':*.
Category and level are optional. DataCreator attempts to find a single enemy from enemy data. If successful, it will fill the missing data. If not, warning will be given (either enemy missing or too many matches).

You can link multiple enemies to one link. These enemies will appear at the same time on the detail box. Separate enemies with '|'.

By default link will have the first enemy's name as its shown text. If you want to change it just add "text:" with wanted text as a one enemy.

syntax
	enemy="enemy_name":"enemy_category":"enemy_level"|"enemy2_name":"enemy2_category":"enemy2_level"|"enemy3_name":"enemy3_category":"enemy3_level"
	eneme=text:"text"|"enemy_name":"enemy_category":"enemy_level"|"enemy2_name":"enemy2_category":"enemy2_level"
	
example
	enemy=lupicus
	enemy=text:ghosts|elementalist:elite|mesmer:elite|monk:elite|necromancer:elite|ranger:elite|warrior:elite
	enemy=gunner:elite|gunner:elite:77

########################################################################################################################################################################
########################################################################################################################################################################


===== Enemy data =====
Enemy data exists in Raw/Enemies/"dungeonname"_e.txt


=== Start ===
You should start with defining enemy types and potion usage connected to them. The idea is to make damage calculation as realistic as possible (not assume people use potion against some thrash).
Type can be anything, it just has to match what is actually used in enemy data.
Usage can be either "main", "side" or "none". If that type of potion is usually used (like when whole dungeon is filled with that enemy type) then use "main".

syntax:
	potion="enemy type"|"potion usage"
example:
	potion=dredge|main

	
==== Enemies ====
Enemies get sorted alphabetically so order doesn't matter.
Like with encounters, all enemies start with a name. However, none of the settings remain.

syntax:
	name="name"
example:
	name=Ascalonian Mesmer

	
== Alternative names ==
When linking enemies from encounters the name isn't always enough. You may also need to use plural or may want to use a shorter version.
You can add alternative names to make linking work properly.
If there is exactly same enemy but with completely different name you should use "copy" instead (explained later). Copying makes new enemy so it can be found from search.

syntax:
	alt="name1"|"name2"|"name2"
example:
	alt=Ascalonian Mesmers|Mesmer|Mesmers

	
== Identifiers ==
Along with the name you need to add path and category to identify and distinguish enemy properly.
Valid categories: "normal", "veteran", "elite", "champion", "legendary", "structure", "trap"

syntax:
	path="path1"|"path2"|"pathN"
example:
	path=AC1|AC2|AC3
	
syntax:
	category="category"
example:
	category="elite"

	
== Type ==
Type is needed for damage calculation with potions. It can be anything (even empty) but if you want it to work on damage calculation it must match potion data.
syntax:
	type="type"
example:
	type="dredge"

== Scaling ==
Fractals enemies scale based on fractal level. However different enemies scale different which must be defined. Valid types are "normal", "champion", "legendary", "constant" and "level".
"Normal" is for normal/veteran enemies, "champion" for champions and "legendary" for legendaries. Special type "constant" should be used for non-scaling (neither scale or level) enemies and "level" for half-scaling enemies (enemy level scales).
Additionally if you aren't getting your values from scale 1 enemies you have to include fractal scale. If target enemy is above level 80 that must also be included.
All values get scaled down to fractal scale 0 (better multipliers) so don't be surprised if there are different numbers in html files.
Note: You may have to specify this multiples times if different values (health, armor, damage) are taken from different levels!

syntax:
	scaling="type"|"fractal scale"|"enemy level"
example:
	scaling=level
	scaling=normal
	scaling=legendary|49
	scaling=normal|45|83

	
== Atttributes ==
Enemies have about same attributes as players. Most are currently hard or impossible to acquire but there is on-going data mining research about this.

Health can be acquired with two ways. You can kill the enemy and sum up all damage dealt from combat log.
Alternatively you can use a memory reader such as gw2esp (or some other dps tool) to read it from client memory (risk of getting banned).

Power is needed to calculate retaliation damage. Currently retaliation formula isn't known so this needs testing.
File "\Research\Conversion.ods" has a calculator for this. Without retaliation this can be acquired if enemy can get might.

Healing power is needed to calculate regeneration healing. Healing per regeneration tick can be acquired same way as the enemy health.
Then calculate healing power from regeneration formula. File "\Research\Conversion.ods" has a calculator for this.

Condition damage is needed to calculate damage from conditions. This can be easily acquired from damage ticks with condition formulas.
File "\Research\Conversion.ods" has a calculator for this.

Armor is calculcated from defense and toughness. Every level has a specific base defense and toughness values. Every enemy has a toughness multiplier which can be acquired from data mined information.
DataCreator automatilly calculates the armor from these values. Existing armor values can be converted to multiplier with file "\Research\DataMining\DataMining.xlsx".

syntax:
	health="number"
	power="number"
	healing="number"
	condition="number"
	toughness="multiplier"


== Image ==
Image/video link for the enemy or its attack. Adds a small icon after the enemy name on detail view. Size is checked from MediaSizes.txt which is generated during backup.
Multiple images or videos can be linked to one enemy by using image= multiple times. Using media= on the link adds media/dungeonimages/ to the link so it can be used for local media.

syntax:
	image="link"
example:
	image=http://wiki.guildwars2.com/images/a/a6/Hafdan_the_Black.jpg
	
== Level ==
By default enemies will be same level as the dungeon they are in. However some enemies have a higher or lower level which has to be manually specified.
Level of fractal enemies is based on scaling so it doesn't need to be specified.

syntax:
	level="number"
example:
	level=82
	
=== Attacks ===
Enemies have multiple attacks. Attack ends when new one starts.
Attack chains should have "(X)" after their name where X is number (starting from 1).

syntax:
	attack="name"
example:
	attack=Shoot
example:
	attack=Swing (1)
example:
	attack=Swung (2)

	
== Attack Info ==
Attacks have info like cooldown, animation and extra information.

syntax:
	cooldown="number"
example:
	cooldown=3

Animation is used to describe how enemy attacks look like.
After effect tells how the attack actually looks like (projectile color, etc).

syntax:
	animation="casting effect"|"casting time (number)"|"after effect"
example:
	animation=Raises left arm|1|shoots a blue ball

Additional is used for any extra info.

syntax:
	additional="text"
example:
	additional=Can't be blocked.
example:
	additional=Used only on distant targets. Minimum-range (stack).

	
== Parts and effects ==
Each attack contains one or multiple parts. You can write anything you like but try look for examples form previous data to keep some consistency.

syntax:
	part="text"
example:
	part=projectile

Use hit counter for attack parts which hit multiple times. This makes DataCreator automatically generate total damage values.
Use length for attacks where multiple hits come over time. This makes DataCreator automatically add "over X seconds" to damage values.

syntax:
	count="number"
	length="number"
example:
	count=5
	length=1.5

Every attack part contains one or multiple effects. You should split effects to multiple lines for clarity. But it's possible to combine both effects and text together.
	
syntax:
	effect="effect 1"
example:
	effect=damage:1000
	effect=constant:5000
	effect=percent:50
	effect=vulnerability:5:2
	effect=burning:10
	effect=launch:2.3
	
Actual effects have different syntax based on their type.
Normal damage: damage:"raw amount"
Healing: healing:"healing"
Armor-ignoring damage: constant:"amount"
Percentile damage: percent:"amount"
Conditions/boons: "name":"duration":"stacks"
Agony: agony:"duration":"stacks"
Buff (not boons): buff:"name":"duration":"stacks"

When getting damage values try to get the maximum damage. Convert the damage by multiplying with your armor value and any other damage modifiers (like WvW, multiply by 1 + Defender/100). WvW bonus is currently slightly bugged so all values (except 1%) are actually 1%-unit less (5% becomes 4% and so on).
When getting condition values keep in mind that you get reduced damage from partial ticks. Damage for conditions is calculated from enemy's condition damage attribute.
Similarly healing power is used for regeneration and power for retaliation.

==== Copy ====
You can copy data of previously introduced enemy (within same dungeon). Category and path are optional. Use them to clarify selection if there are enemies with same name.
This will copy everything except name and alternative names. You can overwrite/add stuff as usually.
This should be used when exactly same enemies appear with different names or to quickly add filler enemies (like normal/champion versions from elite enemies).

syntax:
	copy="name"|"category"|"path1":"path2:"pathN"
example:
	copy=wolf|elite
	
	
########################################################################################################################################################################
########################################################################################################################################################################
	
	
=== Globals ===


== Links ==
You can link anything. Different types are available for convenience. Use "_" instead of spacebar to combine words.
If you don't put any text it will automatically use the one on address. This is useful for wiki links.
If shown text starts with '(' then the text will be put after link text with ' ' between them. For example "Portal|(Utility)" becomes "Portal (Utility)".
local: Use for internal resources.
link: Use for external resources. Automatically adds "http://" if needed.
media: Use for internal images. Assumes that images are located in "media\dungeonimages".
youtube: Use for youtube. Automatically adds "http://youtu.be/" if needed.
wiki: Use for gw2wiki. Automatically adds "http://" or "http://wiki.guildwars2.com/wiki/" if needed.

syntax:
	"linktype"="address"|"shown text"
example:
	local=media\img\temp.png|Nice_image
	link=google.com|Google
	youtube=JDWDlwzYnbY|AC_Boss_solo
	media=dungeon_ac.jpg|Ascalonian_Catacombs
	wiki=Portal|Portal
	wiki=Portal
	wiki=Phantasmal_Warden|Phantasmal_Warden_(Focus)
	wiki=Phantasmal_Warden|(Focus)
	
== Special texts ==
Link system is also used to generate specific html/javascript structures.
record: Gets videos of current restricted record for given path. Path is a database ID. At the momemnt you have to guess those IDs.

syntax:
	"type"="data"
example:
	record=2
	
########################################################################################################################################################################
########################################################################################################################################################################

== Special characters ==
DataCreator converts special characters to html equivalent strings. However you have to tell how to convert them so if you use any non-English characters you should update SpecialCharacters.txt.
Also make sure your files use UTF-8 encoding (use for example Notepad++) so that DataCreator understands characters properly!

	


