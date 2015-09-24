-- phpMyAdmin SQL Dump
-- version 4.0.10.6
-- http://www.phpmyadmin.net
--
-- Palvelin: localhost
-- Luontiaika: 11.02.2015 klo 15:30
-- Palvelimen versio: 5.5.41-cll-lve
-- PHP:n versio: 5.4.23

SET SQL_MODE = "NO_AUTO_VALUE_ON_ZERO";
SET time_zone = "+00:00";


/*!40101 SET @OLD_CHARACTER_SET_CLIENT=@@CHARACTER_SET_CLIENT */;
/*!40101 SET @OLD_CHARACTER_SET_RESULTS=@@CHARACTER_SET_RESULTS */;
/*!40101 SET @OLD_COLLATION_CONNECTION=@@COLLATION_CONNECTION */;
/*!40101 SET NAMES utf8 */;

--
-- Tietokanta: `RecordDB`
--

-- --------------------------------------------------------

--
-- Rakenne taululle `Categories`
--

CREATE TABLE IF NOT EXISTS `Categories` (
  `ID` int(11) NOT NULL AUTO_INCREMENT,
  `name` text NOT NULL,
  PRIMARY KEY (`ID`)
) ENGINE=InnoDB  DEFAULT CHARSET=latin1 AUTO_INCREMENT=4 ;

--
-- Vedos taulusta `Categories`
--

INSERT INTO `Categories` (`ID`, `name`) VALUES
(1, 'Restricted'),
(2, 'Unrestricted'),
(3, 'Solo');

-- --------------------------------------------------------

--
-- Rakenne taululle `Characters`
--

CREATE TABLE IF NOT EXISTS `Characters` (
  `ID` int(11) NOT NULL AUTO_INCREMENT,
  `name` text NOT NULL,
  `profession` int(11) NOT NULL,
  `player` int(11) NOT NULL,
  PRIMARY KEY (`ID`),
  KEY `player` (`player`),
  KEY `profession` (`profession`)
) ENGINE=InnoDB  DEFAULT CHARSET=latin1 AUTO_INCREMENT=131 ;

--
-- Vedos taulusta `Characters`
--

INSERT INTO `Characters` (`ID`, `name`, `profession`, `player`) VALUES
(1, 'Yujisa', 8, 1),
(2, 'Fontys', 4, 2),
(3, 'Derpy Saurus', 1, 3),
(4, 'Lumen Phoenix', 1, 4),
(5, 'Mia Swift', 7, 5),
(7, 'Wethospu Braveheart', 8, 6),
(8, 'MaliciÃ¡', 7, 12),
(9, 'Arcane Spoj', 1, 28),
(10, 'Lena Arrow', 8, 9),
(11, 'Sierra Trinity', 1, 27),
(12, 'Jerem The Illusion', 4, 8),
(13, 'Casual Elite Koptev', 1, 10),
(14, 'Black Eternity', 4, 31),
(15, 'Small Hater', 8, 32),
(16, 'Little Toasty', 1, 33),
(17, 'Mango Samba', 1, 35),
(18, 'Little Play Bunny', 4, 37),
(19, 'Diana Moonlord', 4, 1),
(20, 'Siria Palis', 8, 5),
(21, 'Spoj Of Torment', 4, 28),
(22, 'Unknown Warrior', 8, 44),
(23, 'Unknown Thief', 7, 44),
(24, 'Unknown Ranger', 6, 44),
(25, 'Unknown Necromancer', 5, 44),
(26, 'Unknown Mesmer', 4, 44),
(27, 'Unknown Guardian', 3, 44),
(28, 'Unknown Engineer', 2, 44),
(29, 'Unknown Elementalist', 1, 44),
(30, 'Particlar', 1, 45),
(31, 'Goku En', 1, 19),
(32, 'Brandstorm', 1, 78),
(33, 'Ghostaloempah', 1, 11),
(34, 'Arcolithe Zolt', 4, 9),
(35, 'Yerissy ', 7, 80),
(37, 'Sam AjestÃ©', 1, 49),
(38, 'Encinia', 1, 12),
(41, 'Jerem The Great', 1, 8),
(42, 'Jerem The Light', 3, 8),
(43, 'Aurora KÃ¶nigslicht', 8, 27),
(44, 'Elsee Shinigami', 4, 29),
(45, 'Mighty Breke', 1, 81),
(46, 'Alathaer', 8, 82),
(47, 'LÃ©ks', 1, 79),
(48, 'Elements of Death', 1, 21),
(49, 'Axias', 8, 20),
(50, 'Blaze It Abe', 1, 50),
(51, 'Samantha Martinez', 3, 11),
(52, 'Goku Destroys U', 8, 19),
(53, 'Candy L Assassine', 7, 57),
(54, 'Zara Blaze', 1, 51),
(55, 'Wind Ghosti', 6, 11),
(56, 'Selene Arvine', 8, 15),
(57, 'Nimmi Van Fay', 1, 16),
(58, 'Meadra Die Heldin', 7, 39),
(59, 'Kira Ceyil', 3, 39),
(60, 'Elijah Windflower', 1, 39),
(61, 'Shatter Your Dreams', 4, 51),
(62, 'Eara Ignigena', 1, 33),
(63, 'Serica Flarix', 1, 59),
(64, 'Vrias', 7, 75),
(65, 'Selena Heartseeker', 7, 22),
(66, 'Mystic Ysu', 1, 25),
(67, 'Thul Za Wrong', 3, 26),
(68, 'Shunra Vandella', 1, 24),
(69, 'Fee Nomenale', 8, 23),
(70, 'Fabiana Velvet', 4, 43),
(71, 'Sam Wuvs Lb', 8, 49),
(72, 'Mizuq The Yeti', 1, 52),
(73, 'Brreke', 8, 81),
(74, 'Cptn Obvious', 7, 52),
(75, 'Lets Try To Die', 8, 53),
(76, 'Great Hater', 3, 32),
(77, 'Deathly Eternity', 7, 31),
(78, 'Negative Dps', 6, 3),
(79, 'I Sweet N Sassy I', 7, 37),
(80, 'Ryorco Marm', 1, 54),
(81, 'Casual Sarah', 3, 55),
(82, 'Syl Sy', 8, 56),
(83, 'Braia Eiswind', 8, 33),
(84, 'Candy Stacks Retal', 1, 57),
(85, 'Deathly Yuno', 1, 31),
(86, 'Divine Fervor', 3, 76),
(87, 'Lucernal', 1, 76),
(88, 'Quaggan Of Shadows', 7, 58),
(89, 'Kylar Qurinus', 8, 59),
(90, 'Aeie', 1, 60),
(91, 'Double Pistol Op', 7, 51),
(92, 'Bati', 7, 41),
(93, 'Maghilia', 1, 30),
(94, 'Juelz Deluxe', 1, 41),
(95, 'Thielaya', 3, 61),
(96, 'Grimkram', 8, 41),
(97, 'Blast My Lightfield', 3, 41),
(98, 'Glory Peach', 1, 41),
(99, 'Staff Guard', 3, 62),
(100, 'Inspector Boom Boom', 2, 63),
(101, 'Animus Infernim', 8, 64),
(102, 'Leon Paroyxsm', 1, 65),
(103, 'SubÃ¯', 1, 42),
(104, 'Darkside Candy', 7, 57),
(105, 'Grand Hater', 1, 32),
(106, 'Thilanka', 7, 67),
(107, 'Deadmuffy', 1, 68),
(108, 'Rumbo The Keeper', 8, 77),
(109, 'Mhenlo Dk', 3, 69),
(110, 'Spoj', 5, 28),
(111, 'Kottis Renatus', 4, 60),
(112, 'Dual Ez', 1, 70),
(113, 'Mini Black Eternity', 4, 31),
(114, 'Deathlyhearts', 8, 31),
(115, 'Deathly Sinon', 3, 31),
(116, 'Deathly Asuna', 7, 31),
(117, 'Lina Heartfilia', 7, 74),
(118, 'Vansebros', 8, 73),
(119, 'Mila Wii', 1, 71),
(120, 'Alenka Ayra', 1, 72),
(121, 'Hatred Ez', 3, 70),
(122, 'Obito Ez', 8, 70),
(123, 'Dual Assa', 7, 70),
(124, 'Faith Ez', 4, 70),
(125, 'Arc Z', 3, 75),
(126, 'Braiz', 8, 75),
(127, 'Sesshi No Sora', 1, 56),
(128, 'LaxÃ ', 7, 20),
(129, 'Mika Morgenstern', 3, 87),
(130, 'Banner Of Defense', 8, 21);

-- --------------------------------------------------------

--
-- Rakenne taululle `GameBuilds`
--

CREATE TABLE IF NOT EXISTS `GameBuilds` (
  `ID` int(11) NOT NULL AUTO_INCREMENT,
  `date` date NOT NULL,
  `build` int(11) NOT NULL,
  PRIMARY KEY (`ID`)
) ENGINE=InnoDB  DEFAULT CHARSET=latin1 AUTO_INCREMENT=41 ;

--
-- Vedos taulusta `GameBuilds`
--

INSERT INTO `GameBuilds` (`ID`, `date`, `build`) VALUES
(5, '2015-01-15', 44039),
(6, '2015-01-14', 43970),
(7, '2015-01-13', 43910),
(8, '2015-01-13', 43886),
(9, '2014-12-23', 43467),
(10, '2014-12-22', 43440),
(11, '2014-09-30', 39660),
(12, '2014-05-20', 34002),
(14, '2014-10-22', 40756),
(15, '2014-04-25', 32998),
(16, '2014-09-22', 39327),
(17, '2014-09-12', 38871),
(18, '2014-02-05', 29529),
(19, '2014-11-18', 42115),
(21, '2014-08-14', 37556),
(22, '2014-10-27', 41000),
(23, '2014-10-24', 40880),
(25, '2014-06-18', 35212),
(26, '2014-08-06', 37160),
(27, '2014-03-11', 31017),
(28, '2014-07-17', 36321),
(30, '2014-09-17', 39135),
(31, '2014-07-05', 35867),
(33, '2014-06-03', 34604),
(34, '2014-04-01', 31991),
(35, '2014-08-25', 38057),
(36, '2015-01-27', 44353),
(37, '2015-01-29', 44501),
(40, '2015-02-10', 44986);

-- --------------------------------------------------------

--
-- Rakenne taululle `Groups`
--

CREATE TABLE IF NOT EXISTS `Groups` (
  `ID` int(11) NOT NULL AUTO_INCREMENT,
  `name` text NOT NULL,
  `tag` text NOT NULL,
  `website` text NOT NULL,
  PRIMARY KEY (`ID`)
) ENGINE=InnoDB  DEFAULT CHARSET=latin1 AUTO_INCREMENT=28 ;

--
-- Vedos taulusta `Groups`
--

INSERT INTO `Groups` (`ID`, `name`, `tag`, `website`) VALUES
(1, 'Snow Crows', '[SC]', 'http://snowcrows.enjin.com/'),
(2, 'Quantify', '[qT]', 'http://qtfy.enjin.com/'),
(4, 'Oh My Gosh Virtual Squirrels', '[Vs]', 'http://www.omygosh.fr/'),
(5, 'Intricacy', '[iV]', 'http://www.intricacy-iv.enjin.com/'),
(6, 'Shaolin Monkeys ', '[SM]', ''),
(7, 'Kreios', '[KRS]', ' '),
(8, 'Legion of Doom', '[LoD]', 'http://lodgw2.com/'),
(9, 'Lupi Stole My Bike ', '[LuPi]', 'http://www.lupi.enjin.com/'),
(10, 'Retaliate', '[rT]', ''),
(19, 'Almost Sophisticated', '[geek]', ''),
(20, 'Death and Taxes', '[DnT]', 'http://www.dtguilds.com/'),
(22, 'Immortal Kingdom', '[KING]', ''),
(23, 'Slayers of Lupicus', '[SOLO]', ''),
(24, 'DO NOT REMOVE', '[]', ''),
(26, 'Guys I Dont Have Feedback', '[NoFB]', ''),
(27, 'Is It Swamp', '[Yet]', '');

-- --------------------------------------------------------

--
-- Rakenne taululle `Instances`
--

CREATE TABLE IF NOT EXISTS `Instances` (
  `ID` int(11) NOT NULL AUTO_INCREMENT,
  `name` text NOT NULL,
  `alt` text NOT NULL,
  `image` text NOT NULL,
  `type` int(11) NOT NULL,
  `ordering` int(11) NOT NULL,
  PRIMARY KEY (`ID`),
  KEY `type` (`type`)
) ENGINE=InnoDB  DEFAULT CHARSET=latin1 AUTO_INCREMENT=10 ;

--
-- Vedos taulusta `Instances`
--

INSERT INTO `Instances` (`ID`, `name`, `alt`, `image`, `type`, `ordering`) VALUES
(1, 'Ascalonian Catacombs', 'AC', 'icon_ac.png', 1, 1),
(2, 'Caudecus''s Manor', 'CM', 'icon_cm.png', 1, 2),
(3, 'Twilight Arbor', 'TA', 'icon_ta.png', 1, 3),
(4, 'Sorrow''s Embrace', 'SE', 'icon_se.png', 1, 4),
(5, 'Citadel of Flame', 'CoF', 'icon_cof.png', 1, 5),
(6, 'Honor of the Waves', 'HotW', 'icon_hotw.png', 1, 6),
(7, 'Crucible of Eternity', 'CoE', 'icon_coe.png', 1, 7),
(8, 'The Ruined City of Arah', 'Arah', 'icon_arah.png', 1, 8),
(9, 'Fractals of the Mists', 'FotM', 'icon_fotm.png', 2, 9);

-- --------------------------------------------------------

--
-- Rakenne taululle `InstanceTypes`
--

CREATE TABLE IF NOT EXISTS `InstanceTypes` (
  `ID` int(11) NOT NULL AUTO_INCREMENT,
  `name` text NOT NULL,
  PRIMARY KEY (`ID`)
) ENGINE=InnoDB  DEFAULT CHARSET=latin1 AUTO_INCREMENT=5 ;

--
-- Vedos taulusta `InstanceTypes`
--

INSERT INTO `InstanceTypes` (`ID`, `name`) VALUES
(1, 'dungeon'),
(2, 'fractal');

-- --------------------------------------------------------

--
-- Rakenne taululle `Paths`
--

CREATE TABLE IF NOT EXISTS `Paths` (
  `ID` int(11) NOT NULL AUTO_INCREMENT,
  `instance` int(11) NOT NULL,
  `name` text NOT NULL,
  `description` text NOT NULL,
  `ordering` int(11) NOT NULL,
  `validity` tinyint(1) NOT NULL,
  PRIMARY KEY (`ID`),
  KEY `instance` (`instance`)
) ENGINE=InnoDB  DEFAULT CHARSET=latin1 AUTO_INCREMENT=52 ;

--
-- Vedos taulusta `Paths`
--

INSERT INTO `Paths` (`ID`, `instance`, `name`, `description`, `ordering`, `validity`) VALUES
(1, 1, 'Story', '', 1, 1),
(2, 1, 'Path 1', '', 2, 1),
(3, 1, 'Path 2', '', 3, 1),
(4, 1, 'Path 3', '', 4, 1),
(5, 2, 'Story', '', 1, 1),
(6, 2, 'Path 1', '', 2, 1),
(7, 2, 'Path 2', '', 3, 1),
(8, 2, 'Path 3', '', 4, 1),
(9, 3, 'Story', '', 1, 1),
(10, 3, 'Forward', '', 2, 1),
(11, 3, 'Up', '', 3, 1),
(12, 3, 'Aetherpath', '', 4, 1),
(14, 4, 'Story', '', 1, 1),
(15, 4, 'Path 1', '', 2, 1),
(16, 4, 'Path 2', '', 3, 1),
(17, 4, 'Path 3', '', 4, 1),
(18, 5, 'Story', '', 1, 1),
(19, 5, 'Path 1', '', 2, 1),
(20, 5, 'Path 2', '', 3, 1),
(21, 5, 'Path 3', '', 4, 1),
(22, 6, 'Story', '', 1, 1),
(23, 6, 'Path 1', '', 2, 1),
(24, 6, 'Path 2', '', 3, 1),
(25, 6, 'Path 3', '', 4, 1),
(26, 7, 'Story', '', 1, 1),
(27, 7, 'Path 1', '', 2, 1),
(28, 7, 'Path 2', '', 3, 1),
(29, 7, 'Path 3', '', 4, 1),
(30, 8, 'Story', '', 1, 1),
(31, 8, 'Path 1', '', 2, 1),
(32, 8, 'Path 2', '', 3, 1),
(33, 8, 'Path 3', '', 4, 1),
(34, 8, 'Path 4', '', 5, 1),
(35, 9, 'Aetherblade', '', 1, 1),
(36, 9, 'Aquatic Ruins (Dolphin)', 'Dolphin', 2, 1),
(37, 9, 'Aquatic Ruins (Krait)', 'Krait', 3, 1),
(38, 9, 'Cliffside', '', 4, 1),
(39, 9, 'Molten Furnace', '', 5, 1),
(40, 9, 'Captain Mai Trin', '', 6, 1),
(41, 9, 'Molten Boss', '', 7, 1),
(42, 9, 'Snowblind', '', 8, 1),
(43, 9, 'Solid Ocean', '', 9, 1),
(44, 9, 'Swampland (Bloomhunger)', 'Bloomhunger', 10, 1),
(45, 9, 'Swampland (Mossman)', 'Mossman', 11, 1),
(46, 9, 'Thaumanova Reactor', '', 12, 1),
(47, 9, 'Uncategorized', '', 13, 1),
(48, 9, 'Underground Facility (Bombs)', 'Bombs', 14, 1),
(49, 9, 'Underground Facility (Turret)', 'Turrets', 15, 1),
(50, 9, 'Urban Battlegrounds', '', 16, 1),
(51, 9, 'Volcanic', '', 17, 1);

-- --------------------------------------------------------

--
-- Rakenne taululle `PlayerGroups`
--

CREATE TABLE IF NOT EXISTS `PlayerGroups` (
  `ID` int(11) NOT NULL AUTO_INCREMENT,
  `groupID` int(11) NOT NULL,
  `player` int(11) NOT NULL,
  PRIMARY KEY (`ID`),
  KEY `groupID` (`groupID`,`player`),
  KEY `player` (`player`)
) ENGINE=InnoDB  DEFAULT CHARSET=latin1 AUTO_INCREMENT=142 ;

--
-- Vedos taulusta `PlayerGroups`
--

INSERT INTO `PlayerGroups` (`ID`, `groupID`, `player`) VALUES
(1, 1, 8),
(2, 1, 9),
(3, 1, 10),
(4, 1, 11),
(5, 1, 12),
(6, 1, 19),
(7, 1, 20),
(8, 1, 21),
(9, 1, 27),
(10, 1, 29),
(11, 1, 47),
(12, 1, 49),
(13, 1, 50),
(14, 1, 68),
(15, 1, 69),
(16, 1, 77),
(17, 1, 78),
(18, 1, 79),
(19, 1, 80),
(20, 1, 81),
(21, 1, 82),
(22, 2, 31),
(23, 2, 32),
(24, 2, 33),
(25, 2, 40),
(26, 2, 51),
(27, 2, 57),
(28, 2, 58),
(29, 2, 59),
(30, 2, 60),
(31, 2, 67),
(32, 2, 70),
(33, 2, 75),
(34, 2, 76),
(35, 4, 22),
(36, 4, 23),
(37, 4, 24),
(38, 4, 25),
(39, 4, 26),
(40, 5, 1),
(41, 5, 2),
(42, 5, 3),
(43, 5, 4),
(44, 5, 5),
(45, 5, 37),
(46, 5, 43),
(47, 5, 54),
(48, 5, 55),
(49, 5, 56),
(50, 5, 71),
(51, 5, 72),
(52, 5, 73),
(53, 5, 74),
(54, 6, 52),
(55, 7, 30),
(56, 7, 41),
(57, 7, 61),
(58, 8, 34),
(59, 8, 35),
(60, 8, 36),
(61, 8, 42),
(62, 8, 62),
(63, 8, 63),
(64, 8, 64),
(65, 8, 65),
(66, 9, 53),
(67, 10, 28),
(68, 10, 46),
(134, 10, 87),
(69, 19, 13),
(70, 19, 14),
(71, 19, 15),
(72, 19, 16),
(73, 19, 39),
(74, 20, 48),
(75, 22, 45),
(76, 23, 6),
(77, 24, 44),
(136, 26, 31),
(139, 26, 71),
(140, 26, 72),
(138, 26, 73),
(137, 26, 74),
(130, 27, 20),
(141, 27, 21),
(129, 27, 54),
(128, 27, 56),
(131, 27, 83),
(135, 27, 87);

-- --------------------------------------------------------

--
-- Rakenne taululle `Players`
--

CREATE TABLE IF NOT EXISTS `Players` (
  `ID` int(11) NOT NULL AUTO_INCREMENT,
  `name` text NOT NULL,
  PRIMARY KEY (`ID`)
) ENGINE=InnoDB  DEFAULT CHARSET=latin1 AUTO_INCREMENT=88 ;

--
-- Vedos taulusta `Players`
--

INSERT INTO `Players` (`ID`, `name`) VALUES
(1, 'Kirasia'),
(2, 'Fontys'),
(3, 'Derpy Moa'),
(4, 'Fennec'),
(5, 'Cheezy'),
(6, 'Wethospu'),
(8, 'Jerem'),
(9, 'Ameziz'),
(10, 'Casual'),
(11, 'Ghosti'),
(12, 'Nexus'),
(13, 'Lelani'),
(14, 'Van Gherwen'),
(15, 'Selene Arvine'),
(16, 'Nimmi'),
(19, 'Goku'),
(20, 'Axias'),
(21, 'Perry'),
(22, 'Rhonk'),
(23, 'Kishlin'),
(24, 'Lou'),
(25, 'Ysuran'),
(26, 'Wrong'),
(27, 'NeoVaris'),
(28, 'spoj'),
(29, 'Jing'),
(30, 'Skywalker'),
(31, 'Deathlyhearts'),
(32, 'Hater'),
(33, 'Toast'),
(34, 'Animus Infernim'),
(35, 'Ather'),
(36, 'Subii'),
(37, 'Debby'),
(39, 'Unknown [geek]'),
(40, 'Unknown [qT]'),
(41, 'Unknown [KRS]'),
(42, 'Unknown [LoD]'),
(43, 'Unknown [iV]'),
(44, 'Unknown'),
(45, 'Particlar'),
(46, 'Unknown [rT]'),
(47, 'Unknown [SC]'),
(48, 'Unknown [DnT]'),
(49, 'Sam'),
(50, 'Abe'),
(51, 'Louis'),
(52, 'Unknown [SM]'),
(53, 'Antonio'),
(54, 'Ryo'),
(55, 'Menno'),
(56, 'Sesshi'),
(57, 'Candy'),
(58, 'Quaggan'),
(59, 'Ricie'),
(60, 'Magnus'),
(61, 'Thilaya'),
(62, 'Ted'),
(63, 'David'),
(64, 'Fay'),
(65, 'Leon'),
(67, 'Seb'),
(68, 'Arkaiza'),
(69, 'Mhenlo'),
(70, 'Kevin'),
(71, 'Edd'),
(72, 'Doc'),
(73, 'Daniel'),
(74, 'Colin'),
(75, 'Sebi'),
(76, 'Alex'),
(77, 'Rumpo'),
(78, 'Brandstorm'),
(79, 'Leks'),
(80, 'Atmadra'),
(81, 'Brreke'),
(82, 'Andi'),
(83, 'Unknown [Yet]'),
(87, 'Mika');

-- --------------------------------------------------------

--
-- Rakenne taululle `Professions`
--

CREATE TABLE IF NOT EXISTS `Professions` (
  `ID` int(11) NOT NULL AUTO_INCREMENT,
  `name` text NOT NULL,
  `image` text NOT NULL,
  PRIMARY KEY (`ID`)
) ENGINE=InnoDB  DEFAULT CHARSET=latin1 AUTO_INCREMENT=9 ;

--
-- Vedos taulusta `Professions`
--

INSERT INTO `Professions` (`ID`, `name`, `image`) VALUES
(1, 'elementalist', 'tip_elementalist.png'),
(2, 'engineer', 'tip_engineer.png'),
(3, 'guardian', 'tip_guardian.png'),
(4, 'mesmer', 'tip_mesmer.png'),
(5, 'necromancer', 'tip_necromancer.png'),
(6, 'ranger', 'tip_ranger.png'),
(7, 'thief', 'tip_thief.png'),
(8, 'warrior', 'tip_warrior.png');

-- --------------------------------------------------------

--
-- Rakenne taululle `RecordGroups`
--

CREATE TABLE IF NOT EXISTS `RecordGroups` (
  `ID` int(11) NOT NULL AUTO_INCREMENT,
  `record` int(11) NOT NULL,
  `groupID` int(11) NOT NULL,
  PRIMARY KEY (`ID`),
  KEY `record` (`record`),
  KEY `guild` (`groupID`)
) ENGINE=InnoDB  DEFAULT CHARSET=latin1 AUTO_INCREMENT=63 ;

--
-- Vedos taulusta `RecordGroups`
--

INSERT INTO `RecordGroups` (`ID`, `record`, `groupID`) VALUES
(1, 1, 5),
(3, 4, 5),
(8, 7, 1),
(9, 8, 1),
(10, 9, 1),
(11, 10, 1),
(12, 11, 1),
(13, 12, 1),
(14, 13, 1),
(15, 14, 1),
(16, 15, 1),
(17, 16, 1),
(18, 17, 1),
(19, 18, 1),
(20, 19, 1),
(21, 20, 2),
(22, 21, 1),
(23, 22, 19),
(24, 23, 2),
(25, 24, 4),
(26, 25, 5),
(27, 26, 1),
(28, 27, 1),
(29, 27, 6),
(30, 28, 9),
(32, 29, 5),
(33, 30, 2),
(34, 31, 2),
(35, 32, 2),
(36, 33, 2),
(37, 34, 7),
(38, 35, 7),
(39, 36, 7),
(41, 38, 8),
(42, 39, 8),
(43, 40, 5),
(44, 41, 2),
(45, 42, 2),
(46, 43, 1),
(47, 44, 1),
(48, 45, 26),
(49, 46, 1),
(50, 47, 1),
(51, 48, 1),
(52, 49, 1),
(53, 50, 1),
(54, 51, 1),
(55, 52, 1),
(56, 53, 1),
(57, 54, 1),
(58, 55, 1),
(59, 56, 1),
(60, 57, 27),
(61, 58, 1),
(62, 59, 1);

-- --------------------------------------------------------

--
-- Rakenne taululle `Records`
--

CREATE TABLE IF NOT EXISTS `Records` (
  `ID` int(6) NOT NULL AUTO_INCREMENT,
  `time` int(8) NOT NULL,
  `date` date NOT NULL,
  `path` int(11) NOT NULL,
  `validity` tinyint(1) NOT NULL,
  `topiclink` text NOT NULL,
  `category` int(1) NOT NULL,
  PRIMARY KEY (`ID`),
  KEY `instance` (`path`),
  KEY `category` (`category`)
) ENGINE=InnoDB  DEFAULT CHARSET=latin1 AUTO_INCREMENT=60 ;

--
-- Vedos taulusta `Records`
--

INSERT INTO `Records` (`ID`, `time`, `date`, `path`, `validity`, `topiclink`, `category`) VALUES
(1, 1135000, '2015-01-10', 34, 0, 'iV-The-Ruined-City-of-Arah-Path-4-18-55', 1),
(4, 532000, '2015-01-04', 20, 0, 'http://www.dtguilds.com/forum/m/6563292/viewthread/18328355-iv-cof-p2-852-restricted', 1),
(7, 153000, '2014-10-21', 1, 0, 'https://www.reddit.com/r/Guildwars2/related/2jxbkq/ac_story_record_233/', 1),
(8, 252000, '2014-10-12', 2, 0, 'SC-Ascalonian-Catacombs-Speedclear-Series', 1),
(9, 423000, '2014-10-18', 3, 0, 'SC-Ascalonian-Catacombs-Speedclear-Series', 1),
(10, 546000, '2014-10-24', 4, 0, 'SC-Ascalonian-Catacombs-Speedclear-Series', 1),
(11, 207000, '2014-11-17', 6, 0, 'http://gwscr.com/forum/viewtopic.php?f=6&t=773', 1),
(12, 169000, '2014-11-26', 7, 0, 'http://gwscr.com/forum/viewtopic.php?f=6&t=792', 1),
(13, 157000, '2014-11-15', 8, 0, 'http://gwscr.com/forum/viewtopic.php?f=6&t=771', 1),
(14, 360000, '2014-11-22', 10, 0, 'http://gwscr.com/forum/viewtopic.php?f=6&t=785', 1),
(15, 307000, '2015-01-10', 11, 0, 'http://www.reddit.com/r/Guildwars2/comments/2rynky/sc_twilight_arbor_up_507/', 1),
(16, 556000, '2014-11-02', 14, 0, 'http://gwscr.com/forum/viewtopic.php?f=6&t=759', 1),
(17, 178000, '2014-11-23', 15, 0, 'SC-SE-P1-2-58', 1),
(18, 298000, '2014-11-02', 18, 0, 'http://gwscr.com/forum/viewtopic.php?f=6&t=760', 1),
(19, 434000, '2014-10-25', 33, 0, 'SC-The-Ruined-City-of-Arah-Path-3-7-14', 1),
(20, 484000, '2014-12-22', 31, 0, 'The-Ruined-City-of-Arah-P1-8-04-by-qT', 1),
(21, 349000, '2014-10-24', 9, 0, 'http://gwscr.com/forum/viewtopic.php?f=6&t=752', 1),
(22, 1073000, '2014-11-20', 12, 0, 'http://gwscr.com/forum/viewtopic.php?f=6&t=778', 1),
(23, 212000, '2014-11-04', 5, 0, 'http://gwscr.com/forum/viewtopic.php?f=6&t=762', 1),
(24, 573000, '2014-12-07', 16, 0, 'http://gwscr.com/forum/viewtopic.php?f=6&t=807', 1),
(25, 227000, '2014-12-07', 17, 0, 'http://gwscr.com/forum/viewtopic.php?f=6&t=1596', 1),
(26, 460000, '2014-09-28', 21, 0, 'http://gwscr.com/forum/viewtopic.php?f=6&t=721', 1),
(27, 297000, '2014-10-25', 19, 0, 'http://gwscr.com/forum/viewtopic.php?f=6&t=755', 1),
(28, 615000, '2014-09-21', 32, 0, 'http://gwscr.com/forum/viewtopic.php?f=6&t=710', 1),
(29, 1931000, '2014-10-07', 30, 0, 'http://gwscr.com/forum/viewtopic.php?f=6&t=732', 1),
(30, 610000, '2014-12-05', 26, 0, 'http://gwscr.com/forum/viewtopic.php?f=6&t=801', 1),
(31, 430000, '2014-12-05', 27, 0, 'http://gwscr.com/forum/viewtopic.php?f=6&t=802', 1),
(32, 520000, '2014-12-05', 28, 0, 'http://gwscr.com/forum/viewtopic.php?f=6&t=802', 1),
(33, 518000, '2014-12-05', 29, 0, 'http://gwscr.com/forum/viewtopic.php?f=6&t=783', 1),
(34, 437000, '2014-11-23', 22, 0, 'http://gwscr.com/forum/viewtopic.php?f=6&t=787', 1),
(35, 343000, '2015-01-20', 24, 0, 'http://www.dtguilds.com/forum/m/6563292/viewthread/17902026-krs-hotw-path-2-543min/post/', 1),
(36, 279000, '2014-11-23', 23, 0, 'http://www.dtguilds.com/forum/m/6563292/viewthread/18040381-krs-hotw-path-1-439-restricted', 1),
(38, 330000, '2015-01-20', 24, 0, 'GWSCR-initiative-Content/page/2#post4742636', 1),
(39, 335000, '2015-01-20', 25, 0, 'GWSCR-initiative-Content/page/2#post4742636', 1),
(40, 386000, '2014-12-21', 21, 0, 'GWSCR-initiative-Content/page/2#post4742636', 1),
(41, 328000, '2014-11-11', 9, 0, 'http://gwscr.com/forum/viewtopic.php?f=6&t=767', 1),
(42, 209000, '2015-02-01', 5, 1, 'https://forum-en.guildwars2.com/forum/game/dungeons/qT-Cm-Story-3-29-min-Restricted/', 1),
(43, 248000, '2014-03-17', 23, 0, 'http://www.reddit.com/r/Guildwars2/comments/20mwjk/snow_crowssc_hotw_p1_speed_clear/', 1),
(44, 74000, '2014-04-04', 45, 0, 'Snow-Crows-SC-having-fun-with-Mossman', 1),
(45, 498000, '2015-02-03', 16, 1, 'https://forum-en.guildwars2.com/forum/game/dungeons/NoFB-SE-P2-8-18-min-Restricted/', 1),
(46, 1042000, '2015-02-05', 10, 1, 'https://forum-en.guildwars2.com/forum/game/dungeons/Solo-TA-Foward-17-22/', 3),
(47, 368000, '2015-02-05', 1, 1, 'https://forum-en.guildwars2.com/forum/game/dungeons/Solo-TA-Foward-17-22/', 3),
(48, 557000, '2015-01-28', 2, 1, 'https://forum-en.guildwars2.com/forum/game/dungeons/Solo-TA-Foward-17-22/', 3),
(49, 807000, '2015-01-29', 4, 1, 'https://forum-en.guildwars2.com/forum/game/dungeons/Solo-TA-Foward-17-22/', 3),
(50, 578000, '2014-06-06', 1, 0, 'https://forum-en.guildwars2.com/forum/game/dungeons/Solo-TA-Foward-17-22/', 3),
(51, 538000, '2014-11-29', 2, 0, 'https://forum-en.guildwars2.com/forum/game/dungeons/Solo-TA-Foward-17-22/', 3),
(52, 956000, '2014-09-20', 4, 0, 'https://forum-en.guildwars2.com/forum/game/dungeons/Solo-TA-Foward-17-22/', 3),
(53, 829000, '2014-12-29', 6, 0, 'https://forum-en.guildwars2.com/forum/game/dungeons/Solo-TA-Foward-17-22/', 3),
(54, 578000, '2015-01-11', 7, 0, 'https://forum-en.guildwars2.com/forum/game/dungeons/Solo-TA-Foward-17-22/', 3),
(55, 487000, '2014-12-09', 8, 0, 'https://forum-en.guildwars2.com/forum/game/dungeons/Solo-TA-Foward-17-22/', 3),
(56, 1039000, '2014-10-28', 18, 0, 'https://forum-en.guildwars2.com/forum/game/dungeons/Solo-TA-Foward-17-22/', 3),
(57, 250000, '2015-02-09', 47, 1, 'Yet-Frac-50-Uncategorized-4-10-Restricted', 1),
(58, 536000, '2015-02-11', 5, 1, '', 3),
(59, 922000, '2015-02-11', 9, 1, '', 3);

-- --------------------------------------------------------

--
-- Rakenne taululle `Rules`
--

CREATE TABLE IF NOT EXISTS `Rules` (
  `ID` int(11) NOT NULL AUTO_INCREMENT,
  `ruleset` int(11) NOT NULL,
  `date` date NOT NULL,
  PRIMARY KEY (`ID`)
) ENGINE=InnoDB  DEFAULT CHARSET=latin1 AUTO_INCREMENT=4 ;

--
-- Vedos taulusta `Rules`
--

INSERT INTO `Rules` (`ID`, `ruleset`, `date`) VALUES
(1, 0, '2014-01-01'),
(2, 1, '2014-10-13'),
(3, 2, '2015-01-31');

-- --------------------------------------------------------

--
-- Rakenne taululle `Videos`
--

CREATE TABLE IF NOT EXISTS `Videos` (
  `ID` int(11) NOT NULL AUTO_INCREMENT,
  `link` text NOT NULL,
  `record` int(11) NOT NULL,
  `characterID` int(11) NOT NULL,
  PRIMARY KEY (`ID`),
  KEY `record` (`record`),
  KEY `character` (`characterID`)
) ENGINE=InnoDB  DEFAULT CHARSET=latin1 AUTO_INCREMENT=234 ;

--
-- Vedos taulusta `Videos`
--

INSERT INTO `Videos` (`ID`, `link`, `record`, `characterID`) VALUES
(1, 'aap1LeSm5Fg', 1, 1),
(2, '7BoW84XguJ4 ', 1, 2),
(3, 'ltv2xxBQgwM ', 1, 3),
(4, 'uuIjb9rEkn8 ', 1, 4),
(5, 'YIdi1lLUO_8', 1, 5),
(8, '0OiNYbWFIAg', 4, 3),
(9, 'fvjL9QjMT9Q', 4, 18),
(10, '4p7hBQwZ8MA', 4, 4),
(11, 'RlXtAWIDlHs', 4, 20),
(12, '2_t3bM9W6ig', 4, 19),
(13, 'P5Y_qJnfAV0', 7, 31),
(18, 'AsgTLfyzPbc', 7, 32),
(19, 'R0FhpBomT2k', 7, 33),
(20, 'UklyJEXHQro', 7, 8),
(21, '2b_7RolWyOc', 7, 34),
(22, '141p7RHYDDM', 8, 37),
(23, 'O9Q1qBIFcCk', 8, 31),
(24, '', 8, 32),
(25, 'A2SamoHD8rg', 8, 34),
(26, 'ifC6WcftIgY', 8, 35),
(27, 'qeMDGC9yflc', 9, 37),
(28, '88GIqtXypX4', 9, 32),
(29, 'dyvS1wZw2KU', 9, 11),
(30, 'prJHoP0OZtg', 9, 34),
(31, '1buwkultoCE', 9, 35),
(32, 'VlT4P5gu6CA', 10, 37),
(33, 'F8rVuXwdOF4', 10, 31),
(34, '', 10, 38),
(35, 'rwnAxfQmdmg', 10, 34),
(36, 'okAGfmYzG6k', 10, 35),
(37, 'Zp6p4JjiKT4', 11, 8),
(38, 'h43MsN8PrMc', 11, 13),
(39, 'QK29VQd085A', 11, 41),
(40, 'AtuFO46LyP0', 11, 34),
(41, 'Z341VhYk1tk', 11, 43),
(42, 'Tn35p972ncA', 12, 12),
(43, 'M76rSl_wGFY', 12, 34),
(44, 'I8o_8A0rIQI', 12, 13),
(45, '5VD8ec0xWsU', 12, 33),
(46, 'yggs6ICCLM0', 12, 8),
(47, 'http://youtu.be/zrCow66DD-s', 13, 8),
(52, 'od3r-V5iaT8', 14, 8),
(53, '2ba5cntqcXU', 14, 44),
(54, '', 14, 47),
(55, '', 14, 45),
(56, '', 14, 46),
(57, 'l-NOwbvYbV0', 15, 8),
(58, '318WD9oOsCE', 15, 44),
(59, 'bRMi5MOi9sQ', 15, 43),
(60, '', 15, 47),
(61, '', 15, 45),
(62, '8IrZ0E1I4S8', 16, 12),
(63, 'c5ZzBLv5tHg', 16, 34),
(64, 'qZ8YXginEI0', 16, 8),
(65, 'BbkCJeehwjM', 16, 13),
(66, 'fdX_bZqa1ms', 16, 33),
(67, 'AC-WLNIxXGY', 17, 12),
(68, 'uoGtdCwYoH4 ', 17, 34),
(69, 'eLm31pBwCpw', 17, 31),
(70, 'CCsEwZg4gSs', 17, 48),
(71, 'tIkMGOxqY9o', 17, 49),
(72, '6ZVAltsxr9o', 18, 50),
(73, 'v7Dgn7c9bXQ', 18, 41),
(74, '24etVvr1slY', 18, 51),
(75, 'MyCKTFELqt0', 18, 8),
(76, 'UzrMYaeBSPU', 18, 52),
(77, 'X0zSEYyoj7M', 19, 12),
(78, 'BDrYUgZu-V8', 19, 8),
(79, 'cZxHIxn6yvw ', 19, 52),
(80, 'uZtFkV2dzrM', 19, 50),
(81, 'mmuNijC4BFg', 19, 33),
(82, '6zbAAATHz7s', 20, 15),
(83, 'seloanM0y_k', 20, 14),
(84, '', 20, 53),
(85, '9JbiMLN0fUY', 20, 54),
(86, 'QuKuoawdC60', 20, 16),
(87, 'zMM8Z_-Cesc', 21, 55),
(88, '_EKPDCYpupA', 21, 31),
(89, 'EWHAvar8nHY', 21, 41),
(90, '2tmEeIpkIJo', 21, 11),
(91, 'AsjRM3q3stY', 21, 8),
(92, 'http://www.twitch.tv/selene_arvine/c/5535047', 22, 56),
(93, 'SpYNTQwT-3M', 22, 57),
(94, '', 22, 58),
(95, '', 22, 59),
(96, '', 22, 60),
(97, 'cyVYU07Qrg0', 23, 14),
(98, '', 23, 61),
(99, 'obq9otwmnxU', 23, 62),
(100, '', 23, 63),
(101, '', 23, 64),
(102, 'WvPJSx5T3O0', 24, 69),
(103, 'Wy_nSprHbEo', 24, 67),
(104, '', 24, 65),
(105, '', 24, 66),
(106, 'anXjmnvFL4w', 24, 68),
(107, 'm6kIXhQglCY', 25, 19),
(108, 'VF7QbnDkyFg', 25, 3),
(109, 'VvVgWZ-GoAg', 25, 20),
(110, '', 25, 70),
(111, '', 25, 4),
(112, '2DjM-x7jULA', 26, 50),
(113, 'fZi2xB_2Kl8', 26, 12),
(114, '', 26, 8),
(115, '', 26, 51),
(116, '', 26, 71),
(117, '', 27, 47),
(118, '-difpkXnw0I', 27, 44),
(119, 'CrdNwx4Uyg8', 27, 72),
(120, '', 27, 73),
(121, '', 27, 74),
(122, 'n_2fDefVO_g', 28, 75),
(123, 'DijmioEeg_s', 28, 76),
(124, '', 28, 77),
(125, '', 28, 54),
(126, 'Id1KvHjwA8E', 28, 62),
(127, 'KIRAZs7LM9o', 29, 78),
(128, 'Y_rBQK_4xxY', 29, 79),
(129, '', 29, 80),
(130, '', 29, 81),
(131, '', 29, 82),
(132, 'V4edlBZJZY0', 30, 83),
(133, '', 30, 86),
(134, 'J2k5DJdMh_U', 30, 116),
(135, '', 30, 54),
(136, '', 30, 84),
(137, 'SgSYhlVubH0', 31, 88),
(138, '6Ezq90lKZHc', 31, 15),
(139, '2y3W4A6hbrs', 31, 14),
(140, '', 31, 87),
(141, '', 31, 84),
(142, 'PPlI1d3S3KA', 32, 14),
(143, 'TXxWsJ0a6Zo', 32, 88),
(144, 'RaJ6UL238IY', 32, 89),
(145, 'tMtOpnQ1n7o', 32, 90),
(146, '', 32, 54),
(147, 'qLsaB3KPX14', 33, 14),
(148, 'llgrlypiuSQ', 33, 15),
(149, '', 33, 91),
(150, '', 33, 84),
(151, 'w2jZkjI0Bxs', 33, 62),
(152, 'u-lC7IWjYGc', 34, 92),
(153, 'P8z40Ki7PtU', 34, 93),
(154, '', 34, 94),
(155, '', 34, 95),
(156, '', 34, 96),
(157, 'a_bBBqp476M', 35, 93),
(158, '', 35, 95),
(159, 'NDa4bwkTjqM', 35, 92),
(160, '', 35, 97),
(161, '', 35, 96),
(162, 'cC1Cmserl_U', 36, 93),
(163, '', 36, 98),
(164, 'Pn9ji1EoDEY', 36, 92),
(165, '', 36, 96),
(166, 'lTIVwC0druw', 36, 95),
(167, 'http://youtu.be/7coyLAP1e-4', 13, 12),
(168, 'http://youtu.be/Ha5Q6XiqR5k', 13, 13),
(169, 'http://youtu.be/O22Q32BrMcE', 13, 11),
(170, 'http://youtu.be/AC6Fk6feu4M', 13, 10),
(176, 'szqB_I-Wc04', 38, 99),
(177, '', 38, 100),
(178, '', 38, 101),
(179, '', 38, 17),
(180, 'B83lo07JcfA', 38, 102),
(181, '', 39, 99),
(182, 'daiKfBHMOR0', 39, 101),
(183, '', 39, 100),
(184, 'S1AG4801s8c', 39, 17),
(185, '', 39, 103),
(186, 'bmc9IhxrXxw', 40, 18),
(187, 'ZTXRdA1CCLE', 40, 3),
(188, '', 40, 4),
(189, 'GMeVVqGIgmQ', 40, 5),
(190, '1irFiV0Fytk', 40, 1),
(191, 'L5j7hiG4qD8', 41, 62),
(192, '', 41, 63),
(193, 'N1UKqJUhkvY', 41, 14),
(194, 'QlXxe73IIjs', 41, 15),
(195, '', 41, 104),
(196, 'nzJCZsXW-YA', 42, 14),
(197, 'y_Gyy9BzJ7A', 42, 61),
(198, 'BJ35Zd8PH0A', 42, 106),
(199, 'xEmcx7eUW_o', 42, 105),
(200, '5yeUnZ2HSkM', 42, 62),
(201, 'OzH66kU5iao', 43, 107),
(202, 'V--FQ9dSmvE', 43, 50),
(203, 'cLiIoMiYL1s', 43, 8),
(204, 'Yc8YxWWE9s4', 43, 42),
(205, '', 43, 108),
(206, 'Ll55RA5kU3o', 44, 42),
(207, 'PtyJVDvWOU0', 44, 109),
(208, 'UJCBVlngvXI', 44, 11),
(209, '5pzabiked7k', 44, 50),
(210, 'IqUuucaasuA', 44, 38),
(211, 'eobIFCELA2o', 45, 115),
(212, '9N2g0wOuc3g', 45, 117),
(213, '', 45, 118),
(214, '', 45, 119),
(215, 'loJCU1sAyWU', 45, 120),
(216, 'SCPvucL_e0A', 46, 31),
(217, 'rRXBMiHxSqM', 47, 31),
(218, 'YAfoSz_vMbk', 48, 31),
(219, 'F1xxCi1wVEo', 49, 31),
(220, '9vzNtUOy3CA', 50, 52),
(221, '2gMSucIhDTs', 51, 31),
(222, 'vmYf0Y9fqDU', 52, 52),
(223, 'tL-c5Pc64-k', 53, 52),
(224, 'XJZ_Tn6-J0U', 54, 31),
(225, 'msw6-Fhor8A', 55, 52),
(226, 'LH83E91Z3aY', 56, 52),
(227, 'bvk0iMIrNXo', 57, 127),
(228, '', 57, 80),
(229, 'IJSZFKpCQMQ', 57, 128),
(230, '', 57, 129),
(231, '', 57, 130),
(232, 'kx2Cu3xAgQg', 58, 31),
(233, 'rXLCBfbiu_A', 59, 31);

--
-- Rajoitteet vedostauluille
--

--
-- Rajoitteet taululle `Characters`
--
ALTER TABLE `Characters`
  ADD CONSTRAINT `Characters_ibfk_1` FOREIGN KEY (`player`) REFERENCES `Players` (`ID`),
  ADD CONSTRAINT `Characters_ibfk_2` FOREIGN KEY (`profession`) REFERENCES `Professions` (`ID`);

--
-- Rajoitteet taululle `Instances`
--
ALTER TABLE `Instances`
  ADD CONSTRAINT `Instances_ibfk_1` FOREIGN KEY (`type`) REFERENCES `InstanceTypes` (`ID`);

--
-- Rajoitteet taululle `Paths`
--
ALTER TABLE `Paths`
  ADD CONSTRAINT `Paths_ibfk_1` FOREIGN KEY (`instance`) REFERENCES `Instances` (`ID`);

--
-- Rajoitteet taululle `PlayerGroups`
--
ALTER TABLE `PlayerGroups`
  ADD CONSTRAINT `PlayerGroups_ibfk_2` FOREIGN KEY (`player`) REFERENCES `Players` (`ID`) ON DELETE CASCADE ON UPDATE CASCADE,
  ADD CONSTRAINT `PlayerGroups_ibfk_1` FOREIGN KEY (`groupID`) REFERENCES `Groups` (`ID`);

--
-- Rajoitteet taululle `RecordGroups`
--
ALTER TABLE `RecordGroups`
  ADD CONSTRAINT `RecordGroups_ibfk_2` FOREIGN KEY (`groupID`) REFERENCES `Groups` (`ID`),
  ADD CONSTRAINT `RecordGroups_ibfk_1` FOREIGN KEY (`record`) REFERENCES `Records` (`ID`) ON DELETE CASCADE ON UPDATE CASCADE;

--
-- Rajoitteet taululle `Records`
--
ALTER TABLE `Records`
  ADD CONSTRAINT `Records_ibfk_1` FOREIGN KEY (`path`) REFERENCES `Paths` (`ID`),
  ADD CONSTRAINT `Records_ibfk_2` FOREIGN KEY (`category`) REFERENCES `Categories` (`ID`);

--
-- Rajoitteet taululle `Videos`
--
ALTER TABLE `Videos`
  ADD CONSTRAINT `Videos_ibfk_1` FOREIGN KEY (`record`) REFERENCES `Records` (`ID`) ON DELETE CASCADE ON UPDATE CASCADE,
  ADD CONSTRAINT `Videos_ibfk_2` FOREIGN KEY (`characterID`) REFERENCES `Characters` (`ID`);

/*!40101 SET CHARACTER_SET_CLIENT=@OLD_CHARACTER_SET_CLIENT */;
/*!40101 SET CHARACTER_SET_RESULTS=@OLD_CHARACTER_SET_RESULTS */;
/*!40101 SET COLLATION_CONNECTION=@OLD_COLLATION_CONNECTION */;
