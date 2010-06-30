/*
MySQL Data Transfer
Source Host: localhost
Source Database: test
Target Host: localhost
Target Database: test
Date: 7/19/2008 4:25:48 PM
*/

/*entry - entry id
type - the type of loot
	1 = prospecting
	2 = pickpocketing
	3 = gameobject
	4 = item
	5 = skinning
typeid - the entry id of the go, item, ect.
percentchance - the percent chance
heriodpercentchance - the herioc percent chance
minnum - minimum # of typeid dropped
maxnum - maximum # of typeid dropper
ffa_loot - party loot?*/

SET FOREIGN_KEY_CHECKS=0;
-- ----------------------------
-- Table structure for miscloot
-- ----------------------------
CREATE TABLE `miscloot` (
  `entry` int(11) unsigned zerofill NOT NULL auto_increment,
  `type` int(11) NOT NULL,
  `type_id` int(11) NOT NULL,
  `percentchance` int(11) NOT NULL,
  `heriocpercentchance` int(11) NOT NULL,
  `minnum` int(11) NOT NULL,
  `maxnum` int(11) NOT NULL,
  `ffa_loot` int(11) NOT NULL,
  PRIMARY KEY  (`entry`)
) ENGINE=MyISAM DEFAULT CHARSET=utf8;

-- ----------------------------
-- Records 
-- ----------------------------
