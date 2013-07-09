ALTER TABLE `creatures` ADD `creature_minlife` INT DEFAULT '0' NOT NULL AFTER `creature_size`;
ALTER TABLE `creatures` ADD `creature_maxlife` INT DEFAULT '0' NOT NULL AFTER `creature_minlife`;
ALTER TABLE `creatures` ADD `creature_minmana` INT DEFAULT '0' NOT NULL AFTER `creature_maxlife`;
ALTER TABLE `creatures` ADD `creature_maxmana` INT DEFAULT '0' NOT NULL AFTER `creature_minlife`;