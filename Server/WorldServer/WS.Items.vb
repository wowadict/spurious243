'
' Copyright (C) 2013 getMaNGOS <http://www.getMangos.co.uk>
'
' This program is free software; you can redistribute it and/or modify
' it under the terms of the GNU General Public License as published by
' the Free Software Foundation; either version 2 of the License, or
' (at your option) any later version.
'
' This program is distributed in the hope that it will be useful,
' but WITHOUT ANY WARRANTY; without even the implied warranty of
' MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
' GNU General Public License for more details.
'
' You should have received a copy of the GNU General Public License
' along with this program; if not, write to the Free Software
' Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA  02111-1307  USA
'

Imports System.Threading
Imports System.Runtime.CompilerServices
Imports mangosVB.Common.BaseWriter

Public Module WS_Items

#Region "WS.Items.Constants"
    Enum InventoryChangeFailure As Byte
        EQUIP_ERR_OK = 0
        EQUIP_ERR_CANT_EQUIP_LEVEL_I = 1
        EQUIP_ERR_ERR_CANT_EQUIP_SKILL = 2
        EQUIP_ERR_ITEM_DOESNT_GO_TO_SLOT = 3
        EQUIP_ERR_BAG_FULL = 4
        EQUIP_ERR_NONEMPTY_BAG_OVER_OTHER_BAG = 5
        EQUIP_ERR_CANT_TRADE_EQUIP_BAGS = 6
        EQUIP_ERR_ONLY_AMMO_CAN_GO_HERE = 7
        EQUIP_ERR_NO_REQUIRED_PROFICIENCY = 8
        EQUIP_ERR_NO_EQUIPMENT_SLOT_AVAILABLE = 9
        EQUIP_ERR_YOU_CAN_NEVER_USE_THAT_ITEM = 10
        EQUIP_ERR_YOU_CAN_NEVER_USE_THAT_ITEM2 = 11
        EQUIP_ERR_NO_EQUIPMENT_SLOT_AVAILABLE2 = 12
        EQUIP_ERR_CANT_EQUIP_WITH_TWOHANDED = 13
        EQUIP_ERR_CANT_DUAL_WIELD = 14
        EQUIP_ERR_ITEM_DOESNT_GO_INTO_BAG = 15
        EQUIP_ERR_ITEM_DOESNT_GO_INTO_BAG2 = 16
        EQUIP_ERR_CANT_CARRY_MORE_OF_THIS = 17
        EQUIP_ERR_NO_EQUIPMENT_SLOT_AVAILABLE3 = 18
        EQUIP_ERR_ITEM_CANT_STACK = 19
        EQUIP_ERR_ITEM_CANT_BE_EQUIPPED = 20
        EQUIP_ERR_ITEMS_CANT_BE_SWAPPED = 21
        EQUIP_ERR_SLOT_IS_EMPTY = 22
        EQUIP_ERR_ITEM_NOT_FOUND = 23
        EQUIP_ERR_CANT_DROP_SOULBOUND = 24
        EQUIP_ERR_OUT_OF_RANGE = 25
        EQUIP_ERR_TRIED_TO_SPLIT_MORE_THAN_COUNT = 26
        EQUIP_ERR_COULDNT_SPLIT_ITEMS = 27
        EQUIP_ERR_MISSING_REAGENT = 28
        EQUIP_ERR_NOT_ENOUGH_MONEY = 29
        EQUIP_ERR_NOT_A_BAG = 30
        EQUIP_ERR_CAN_ONLY_DO_WITH_EMPTY_BAGS = 31
        EQUIP_ERR_DONT_OWN_THAT_ITEM = 32
        EQUIP_ERR_CAN_EQUIP_ONLY1_QUIVER = 33
        EQUIP_ERR_MUST_PURCHASE_THAT_BAG_SLOT = 34
        EQUIP_ERR_TOO_FAR_AWAY_FROM_BANK = 35
        EQUIP_ERR_ITEM_LOCKED = 36
        EQUIP_ERR_YOU_ARE_STUNNED = 37
        EQUIP_ERR_YOU_ARE_DEAD = 38
        EQUIP_ERR_CANT_DO_RIGHT_NOW = 39
        EQUIP_ERR_INT_BAG_ERROR = 40
        EQUIP_ERR_CAN_EQUIP_ONLY1_QUIVER2 = 41
        EQUIP_ERR_CAN_EQUIP_ONLY1_AMMOPOUCH = 42
        EQUIP_ERR_STACKABLE_CANT_BE_WRAPPED = 43
        EQUIP_ERR_EQUIPPED_CANT_BE_WRAPPED = 44
        EQUIP_ERR_WRAPPED_CANT_BE_WRAPPED = 45
        EQUIP_ERR_BOUND_CANT_BE_WRAPPED = 46
        EQUIP_ERR_UNIQUE_CANT_BE_WRAPPED = 47
        EQUIP_ERR_BAGS_CANT_BE_WRAPPED = 48
        EQUIP_ERR_ALREADY_LOOTED = 49
        EQUIP_ERR_INVENTORY_FULL = 50
        EQUIP_ERR_BANK_FULL = 51
        EQUIP_ERR_ITEM_IS_CURRENTLY_SOLD_OUT = 52
        EQUIP_ERR_BAG_FULL3 = 53
        EQUIP_ERR_ITEM_NOT_FOUND2 = 54
        EQUIP_ERR_ITEM_CANT_STACK2 = 55
        EQUIP_ERR_BAG_FULL4 = 56
        EQUIP_ERR_ITEM_SOLD_OUT = 57
        EQUIP_ERR_OBJECT_IS_BUSY = 58
        EQUIP_ERR_NONE = 59
        EQUIP_ERR_NOT_IN_COMBAT = 60
        EQUIP_ERR_NOT_WHILE_DISARMED = 61
        EQUIP_ERR_BAG_FULL6 = 62
        EQUIP_ERR_CANT_EQUIP_RANK = 63
        EQUIP_ERR_CANT_EQUIP_REPUTATION = 64
        EQUIP_ERR_TOO_MANY_SPECIAL_BAGS = 65
        EQUIP_ERR_LOOT_CANT_LOOT_THAT_NOW = 66
        EQUIP_ERR_ITEM_UNIQUE_EQUIPABLE = 67
        EQUIP_ERR_VENDOR_MISSING_TURNINS = 68
        EQUIP_ERR_NOT_ENOUGH_HONOR_POINTS = 69
        EQUIP_ERR_NOT_ENOUGH_ARENA_POINTS = 70
        EQUIP_ERR_ITEM_MAX_COUNT_SOCKETED = 71
        EQUIP_ERR_MAIL_BOUND_ITEM = 72
        EQUIP_ERR_NO_SPLIT_WHILE_PROSPECTING = 73
        EQUIP_ERR_ITEM_MAX_COUNT_EQUIPPED_SOCKETED = 75
        EQUIP_ERR_ITEM_UNIQUE_EQUIPPABLE_SOCKETED = 76
        EQUIP_ERR_TOO_MUCH_GOLD = 77
        EQUIP_ERR_NOT_DURING_ARENA_MATCH = 78
        EQUIP_ERR_CANNOT_TRADE_THAT = 79
        EQUIP_ERR_PERSONAL_ARENA_RATING_TOO_LOW = 80
    End Enum

    Public Enum ITEM_DAMAGE_TYPE As Byte
        NORMAL_DAMAGE = 0
        HOLY_DAMAGE = 1
        FIRE_DAMAGE = 2
        NATURE_DAMAGE = 3
        FROST_DAMAGE = 4
        SHADOW_DAMAGE = 5
        ARCANE_DAMAGE = 6
    End Enum
    Public Enum ITEM_QUALITY_NAMES As Byte
        ITEM_QUALITY_POOR_GREY = 0
        ITEM_QUALITY_NORMAL_WHITE = 1
        ITEM_QUALITY_UNCOMMON_GREEN = 2
        ITEM_QUALITY_RARE_BLUE = 3
        ITEM_QUALITY_EPIC_PURPLE = 4
        ITEM_QUALITY_LEGENDARY_ORANGE = 5
        ITEM_QUALITY_ARTIFACT_LIGHT_YELLOW = 6
    End Enum
    Public Enum ITEM_STAT_TYPE As Byte
        HEALTH = 1
        UNKNOWN = 2
        AGILITY = 3
        STRENGTH = 4
        INTELLECT = 5
        SPIRIT = 6
        STAMINA = 7
        DEFENCE = 12
        DODGE = 13
        PARRY = 14
        BLOCK = 15
        MELEEHITRATING = 16
        RANGEDHITRATING = 17
        SPELLHITRATING = 18
        MELEECRITRATING = 19
        RANGEDCRITRATING = 20
        SPELLCRITRATING = 21
        MELEEHITAVOIDANCE = 22
        RANGEDHITAVOIDANCE = 23
        SPELLHITAVOIDANCE = 24
        MELEECRITAVOIDANCE = 25
        RANGEDCRITAVOIDANCE = 26
        SPELLCRITAVOIDANCE = 26
        MELEEHASTERATING = 28
        RANGEDHASTERATING = 29
        SPELLHASTERATING = 30
        HITRATING = 31
        HITCRITRATING = 32
        HITAVOIDANCE = 33
        HITCRITAVOIDANCE = 34
        RESILIENCE = 35
        HITHASTERATING = 36
    End Enum
    Public Enum ITEM_SPELLTRIGGER_TYPE As Byte
        USE = 0
        ON_EQUIP = 1
        CHANCE_ON_HIT = 2
        SOULSTONE = 4
        NO_DELAY_USE = 5
        LEARN_SPELL = 6
    End Enum
    Public Enum ITEM_BONDING_TYPE As Byte
        NO_BIND = 0
        BIND_WHEN_PICKED_UP = 1
        BIND_WHEN_EQUIPED = 2
        BIND_WHEN_USED = 3
        BIND_UNK_QUESTITEM1 = 4
        BIND_UNK_QUESTITEM2 = 5
    End Enum
    Public Enum SHEATHE_TYPE As Byte
        SHEATHETYPE_NONE = 0
        SHEATHETYPE_MAINHAND = 1
        SHEATHETYPE_OFFHAND = 2
        SHEATHETYPE_LARGEWEAPONLEFT = 3
        SHEATHETYPE_LARGEWEAPONRIGHT = 4
        SHEATHETYPE_HIPWEAPONLEFT = 5
        SHEATHETYPE_HIPWEAPONRIGHT = 6
        SHEATHETYPE_SHIELD = 7
    End Enum
    Public Enum SHEATHE_SLOT As Byte
        SHEATHE_NONE = 0
        SHEATHE_WEAPON = 1
        SHEATHE_RANGED = 2
    End Enum
    Public Enum INVENTORY_TYPES As Byte
        INVTYPE_NON_EQUIP = &H0
        INVTYPE_HEAD = &H1
        INVTYPE_NECK = &H2
        INVTYPE_SHOULDERS = &H3
        INVTYPE_BODY = &H4           ' cloth robes only
        INVTYPE_CHEST = &H5
        INVTYPE_WAIST = &H6
        INVTYPE_LEGS = &H7
        INVTYPE_FEET = &H8
        INVTYPE_WRISTS = &H9
        INVTYPE_HANDS = &HA
        INVTYPE_FINGER = &HB
        INVTYPE_TRINKET = &HC
        INVTYPE_WEAPON = &HD
        INVTYPE_SHIELD = &HE
        INVTYPE_RANGED = &HF
        INVTYPE_CLOAK = &H10
        INVTYPE_TWOHAND_WEAPON = &H11
        INVTYPE_BAG = &H12
        INVTYPE_TABARD = &H13
        INVTYPE_ROBE = &H14
        INVTYPE_WEAPONMAINHAND = &H15
        INVTYPE_WEAPONOFFHAND = &H16
        INVTYPE_HOLDABLE = &H17
        INVTYPE_AMMO = &H18
        INVTYPE_THROWN = &H19
        INVTYPE_RANGEDRIGHT = &H1A
        INVTYPE_SLOT_ITEM = &H1B
        INVTYPE_RELIC = &H1C
        NUM_INVENTORY_TYPES = &H1D
    End Enum

    'Got them from ItemSubClass.dbc
    Public Enum ITEM_CLASS As Byte
        ITEM_CLASS_CONSUMABLE = 0
        ITEM_CLASS_CONTAINER = 1
        ITEM_CLASS_WEAPON = 2
        ITEM_CLASS_JEWELRY = 3
        ITEM_CLASS_ARMOR = 4
        ITEM_CLASS_REAGENT = 5
        ITEM_CLASS_PROJECTILE = 6
        ITEM_CLASS_TRADE_GOODS = 7
        ITEM_CLASS_GENERIC = 8
        ITEM_CLASS_BOOK = 9
        ITEM_CLASS_MONEY = 10
        ITEM_CLASS_QUIVER = 11
        ITEM_CLASS_QUEST = 12
        ITEM_CLASS_KEY = 13
        ITEM_CLASS_PERMANENT = 14
        ITEM_CLASS_JUNK = 15
    End Enum
    Public Enum ITEM_SUBCLASS As Byte
        ' Consumable
        ITEM_SUBCLASS_CONSUMABLE = 0
        ITEM_SUBCLASS_FOOD = 1
        ITEM_SUBCLASS_LIQUID = 2
        ITEM_SUBCLASS_POTION = 3
        ITEM_SUBCLASS_SCROLL = 4
        ITEM_SUBCLASS_BANDAGE = 5
        ITEM_SUBCLASS_HEALTHSTONE = 6
        ITEM_SUBCLASS_COMBAT_EFFECT = 7

        ' Container
        ITEM_SUBCLASS_BAG = 0
        ITEM_SUBCLASS_SOUL_BAG = 1
        ITEM_SUBCLASS_HERB_BAG = 2
        ITEM_SUBCLASS_ENCHANTING_BAG = 3
        ITEM_SUBCLASS_ENGINEERING_BAG = 4
        ITEM_SUBCLASS_GEM_BAG = 5
        ITEM_SUBCLASS_MINNING_BAG = 6
        ITEM_SUBCLASS_LEATHERWORKING_BAG = 7

        ' Weapon
        ITEM_SUBCLASS_AXE = 0
        ITEM_SUBCLASS_TWOHAND_AXE = 1
        ITEM_SUBCLASS_BOW = 2
        ITEM_SUBCLASS_GUN = 3
        ITEM_SUBCLASS_MACE = 4
        ITEM_SUBCLASS_TWOHAND_MACE = 5
        ITEM_SUBCLASS_POLEARM = 6
        ITEM_SUBCLASS_SWORD = 7
        ITEM_SUBCLASS_TWOHAND_SWORD = 8
        ITEM_SUBCLASS_WEAPON_obsolete = 9
        ITEM_SUBCLASS_STAFF = 10
        ITEM_SUBCLASS_WEAPON_EXOTIC = 11
        ITEM_SUBCLASS_WEAPON_EXOTIC2 = 12
        ITEM_SUBCLASS_FIST_WEAPON = 13
        ITEM_SUBCLASS_MISC_WEAPON = 14
        ITEM_SUBCLASS_DAGGER = 15
        ITEM_SUBCLASS_THROWN = 16
        ITEM_SUBCLASS_SPEAR = 17
        ITEM_SUBCLASS_CROSSBOW = 18
        ITEM_SUBCLASS_WAND = 19
        ITEM_SUBCLASS_FISHING_POLE = 20

        ' Gem
        ITEM_SUBCLASS_RED = 0
        ITEM_SUBCLASS_BLUE = 1
        ITEM_SUBCLASS_YELLOW = 2
        ITEM_SUBCLASS_PURPLE = 3
        ITEM_SUBCLASS_GREEN = 4
        ITEM_SUBCLASS_ORANGE = 5
        ITEM_SUBCLASS_META = 6
        ITEM_SUBCLASS_SIMPLE = 7
        ITEM_SUBCLASS_PRISMATIC = 8

        ' Armor
        ITEM_SUBCLASS_MISC = 0
        ITEM_SUBCLASS_CLOTH = 1
        ITEM_SUBCLASS_LEATHER = 2
        ITEM_SUBCLASS_MAIL = 3
        ITEM_SUBCLASS_PLATE = 4
        ITEM_SUBCLASS_BUCKLER = 5
        ITEM_SUBCLASS_SHIELD = 6
        ITEM_SUBCLASS_LIBRAM = 7
        ITEM_SUBCLASS_IDOL = 8
        ITEM_SUBCLASS_TOTEM = 9

        ' Projectile
        ITEM_SUBCLASS_WAND_obslete = 0
        ITEM_SUBCLASS_BOLT_obslete = 1
        ITEM_SUBCLASS_ARROW = 2
        ITEM_SUBCLASS_BULLET = 3
        ITEM_SUBCLASS_THROWN_obslete = 4

        ' Trade goods
        ITEM_SUBCLASS_TRADE_GOODS = 0
        ITEM_SUBCLASS_PARTS = 1
        ITEM_SUBCLASS_EXPLOSIVES = 2
        ITEM_SUBCLASS_DEVICES = 3
        ITEM_SUBCLASS_GEMS = 4
        ITEM_SUBCLASS_CLOTHS = 5
        ITEM_SUBCLASS_LEATHERS = 6
        ITEM_SUBCLASS_METAL_AND_STONE = 7
        ITEM_SUBCLASS_MEAT = 8
        ITEM_SUBCLASS_HERB = 9
        ITEM_SUBCLASS_ELEMENTAL = 10
        ITEM_SUBCLASS_OTHERS = 11
        ITEM_SUBCLASS_ENCHANTANTS = 12
        ITEM_SUBCLASS_MATERIALS = 13

        ' Recipe
        ITEM_SUBCLASS_BOOK = 0
        ITEM_SUBCLASS_LEATHERWORKING = 1
        ITEM_SUBCLASS_TAILORING = 2
        ITEM_SUBCLASS_ENGINEERING = 3
        ITEM_SUBCLASS_BLACKSMITHING = 4
        ITEM_SUBCLASS_COOKING = 5
        ITEM_SUBCLASS_ALCHEMY = 6
        ITEM_SUBCLASS_FIRST_AID = 7
        ITEM_SUBCLASS_ENCHANTING = 8
        ITEM_SUBCLASS_FISNING = 9
        ITEM_SUBCLASS_JEWELCRAFTING = 10

        ' Quiver
        ITEM_SUBCLASS_QUIVER0_obslete = 0
        ITEM_SUBCLASS_QUIVER1_obslete = 1
        ITEM_SUBCLASS_QUIVER = 2
        ITEM_SUBCLASS_AMMO_POUCH = 3

        ' Keys
        ITEM_SUBCLASS_KEY = 0
        ITEM_SUBCLASS_LOCKPICK = 1

        ' Misc
        ITEM_SUBCLASS_JUNK = 0
        ITEM_SUBCLASS_REAGENT = 1
        ITEM_SUBCLASS_PET = 2
        ITEM_SUBCLASS_HOLIDAY = 3
        ITEM_SUBCLASS_OTHER = 4
        ITEM_SUBCLASS_MOUNT = 5
    End Enum
    Public Enum ITEM_FLAGS As Byte
        ITEM_FLAGS_BINDED = 1
    End Enum
    Public Enum ITEM_BAG As Integer
        NONE = 0
        ARROW = 1
        BULLET = 2
        SOUL_SHARD = 3
        HERB = 6
        ENCHANTING = 7
        ENGINEERING = 8
        KEYRING = 9
        JEWELCRAFTING = 10
        MINNING = 11
    End Enum
    Public Enum ITEM_GEMS As Byte
        COLOR_META = 1
        COLOR_RED = 2
        COLOR_YELLOW = 4
        COLOR_BLUE = 8
    End Enum

    Public item_weapon_skills() As Integer = New Integer() {SKILL_IDs.SKILL_AXES, _
                                                            SKILL_IDs.SKILL_TWO_HANDED_AXES, _
                                                            SKILL_IDs.SKILL_BOWS, _
                                                            SKILL_IDs.SKILL_GUNS, _
                                                            SKILL_IDs.SKILL_MACES, _
                                                            SKILL_IDs.SKILL_TWO_HANDED_MACES, _
                                                            SKILL_IDs.SKILL_POLEARMS, _
                                                            SKILL_IDs.SKILL_SWORDS, _
                                                            SKILL_IDs.SKILL_TWO_HANDED_SWORDS, 0, _
                                                            SKILL_IDs.SKILL_STAVES, 0, 0, 0, 0, _
                                                            SKILL_IDs.SKILL_DAGGERS, _
                                                            SKILL_IDs.SKILL_THROWN, _
                                                            SKILL_IDs.SKILL_SPEARS, _
                                                            SKILL_IDs.SKILL_CROSSBOWS, _
                                                            SKILL_IDs.SKILL_WANDS, _
                                                            SKILL_IDs.SKILL_FISHING}
    Public item_armor_skills() As Integer = New Integer() {0, SKILL_IDs.SKILL_CLOTH, SKILL_IDs.SKILL_LEATHER, SKILL_IDs.SKILL_MAIL, SKILL_IDs.SKILL_PLATE_MAIL, 0, SKILL_IDs.SKILL_SHIELD, 0, 0, 0}
#End Region
#Region "WS.Items.TypeDef"

    'WARNING: Use only with ITEMDatabase()
    Public Class ItemInfo
        Implements IDisposable
        Private found_ As Boolean = False
        Public Sub New()
            Damage(0) = New TDamage
            Damage(1) = New TDamage
            Damage(2) = New TDamage
            Damage(3) = New TDamage
            Damage(4) = New TDamage
            Spells(0) = New TItemSpellInfo
            Spells(1) = New TItemSpellInfo
            Spells(2) = New TItemSpellInfo
            Spells(3) = New TItemSpellInfo
            Spells(4) = New TItemSpellInfo
            Sockets(0) = New TItemSocket
            Sockets(1) = New TItemSocket
            Sockets(2) = New TItemSocket
        End Sub
        Public Sub New(ByVal ItemId As Integer)
            Me.New()

            'DONE: Load Item Data from MySQL
            Dim MySQLQuery As New DataTable
            Database.Query(String.Format("SELECT * FROM items WHERE entry = {0};", ItemId), MySQLQuery)
            If MySQLQuery.Rows.Count = 0 Then
                Log.WriteLine(LogType.FAILED, "ItemID {0} not found in SQL database! Loading default ""Unknown Item"" info.", ItemId)
                found_ = False
                Id = ItemId
                ITEMDatabase.Add(Id, Me)
                Exit Sub
            End If
            found_ = True

            Model = MySQLQuery.Rows(0).Item("displayid")
            Name = MySQLQuery.Rows(0).Item("name1")
            Quality = MySQLQuery.Rows(0).Item("quality")               '0=Grey-Poor 1=White-Common 2=Green-Uncommon 3=Blue-Rare 4=Purple-Epic 5=Orange-Legendary 6=Red-Artifact
            Material = MySQLQuery.Rows(0).Item("lock_material")             '-1=Consumables 1=Metal 2=Wood 3=Liquid 4=Jewelry 5=Chain 6=Plate 7=Cloth 8=Leather
            Durability = MySQLQuery.Rows(0).Item("MaxDurability")
            MaxCount = MySQLQuery.Rows(0).Item("maxcount")
            Sheath = MySQLQuery.Rows(0).Item("sheathID")
            Bonding = MySQLQuery.Rows(0).Item("bonding")
            BuyPrice = MySQLQuery.Rows(0).Item("buyprice")
            SellPrice = MySQLQuery.Rows(0).Item("sellprice")

            'Item's Characteristics
            Id = MySQLQuery.Rows(0).Item("entry")
            Flags = MySQLQuery.Rows(0).Item("flags")
            ObjectClass = MySQLQuery.Rows(0).Item("class")
            SubClass = MySQLQuery.Rows(0).Item("subclass")
            InventoryType = MySQLQuery.Rows(0).Item("inventorytype")
            Level = MySQLQuery.Rows(0).Item("itemlevel")

            AvailableClasses = BitConverter.ToUInt32(BitConverter.GetBytes(MySQLQuery.Rows(0).Item("allowableclass")), 0)
            AvailableRaces = BitConverter.ToUInt32(BitConverter.GetBytes(MySQLQuery.Rows(0).Item("allowablerace")), 0)
            ReqLevel = MySQLQuery.Rows(0).Item("requiredlevel")
            ReqSkill = MySQLQuery.Rows(0).Item("RequiredSkill")
            ReqSkillRank = MySQLQuery.Rows(0).Item("RequiredSkillRank")
            ReqSkillSubRank = MySQLQuery.Rows(0).Item("RequiredSkillSubRank")
            'ReqSpell = MySQLQuery.Rows(0).Item("requiredSpell")
            ReqFaction = MySQLQuery.Rows(0).Item("RequiredFaction")
            ReqFactionLevel = MySQLQuery.Rows(0).Item("RequiredFactionStanding")
            ReqHonorRank = MySQLQuery.Rows(0).Item("RequiredPlayerRank1")
            ReqHonorRank2 = MySQLQuery.Rows(0).Item("RequiredPlayerRank2")

            'Special items
            AmmoType = MySQLQuery.Rows(0).Item("ammo_type")
            PageText = MySQLQuery.Rows(0).Item("page_id")
            Stackable = MySQLQuery.Rows(0).Item("maxcount")
            Unique = MySQLQuery.Rows(0).Item("Unique")
            Description = MySQLQuery.Rows(0).Item("description")
            Block = MySQLQuery.Rows(0).Item("block")
            ItemSet = MySQLQuery.Rows(0).Item("itemset")
            PageMaterial = MySQLQuery.Rows(0).Item("page_material")     'The background of the page window: 1=Parchment 2=Stone 3=Marble 4=Silver 5=Bronze
            StartQuest = MySQLQuery.Rows(0).Item("quest_id")
            ContainerSlots = MySQLQuery.Rows(0).Item("ContainerSlots")
            LanguageID = MySQLQuery.Rows(0).Item("page_language")
            BagFamily = MySQLQuery.Rows(0).Item("bagfamily")

            Delay = MySQLQuery.Rows(0).Item("delay")
            Range = MySQLQuery.Rows(0).Item("range")

            Damage(0).Minimum = MySQLQuery.Rows(0).Item("dmg_min1")
            Damage(0).Maximum = MySQLQuery.Rows(0).Item("dmg_max1")
            Damage(0).Type = MySQLQuery.Rows(0).Item("dmg_type1")
            Damage(1).Minimum = MySQLQuery.Rows(0).Item("dmg_min2")
            Damage(1).Maximum = MySQLQuery.Rows(0).Item("dmg_max2")
            Damage(1).Type = MySQLQuery.Rows(0).Item("dmg_type2")
            Damage(2).Minimum = MySQLQuery.Rows(0).Item("dmg_min3")
            Damage(2).Maximum = MySQLQuery.Rows(0).Item("dmg_max3")
            Damage(2).Type = MySQLQuery.Rows(0).Item("dmg_type3")
            Damage(3).Minimum = MySQLQuery.Rows(0).Item("dmg_min4")
            Damage(3).Maximum = MySQLQuery.Rows(0).Item("dmg_max4")
            Damage(3).Type = MySQLQuery.Rows(0).Item("dmg_type4")
            Damage(4).Minimum = MySQLQuery.Rows(0).Item("dmg_min5")
            Damage(4).Maximum = MySQLQuery.Rows(0).Item("dmg_max5")
            Damage(4).Type = MySQLQuery.Rows(0).Item("dmg_type5")

            Resistances(DamageTypes.DMG_PHYSICAL) = MySQLQuery.Rows(0).Item("armor")        'Armor
            Resistances(DamageTypes.DMG_HOLY) = MySQLQuery.Rows(0).Item("holy_res")          'Holy
            Resistances(DamageTypes.DMG_FIRE) = MySQLQuery.Rows(0).Item("fire_res")          'Fire
            Resistances(DamageTypes.DMG_NATURE) = MySQLQuery.Rows(0).Item("nature_res")      'Nature
            Resistances(DamageTypes.DMG_FROST) = MySQLQuery.Rows(0).Item("frost_res")        'Frost
            Resistances(DamageTypes.DMG_SHADOW) = MySQLQuery.Rows(0).Item("shadow_res")      'Shadow
            Resistances(DamageTypes.DMG_ARCANE) = MySQLQuery.Rows(0).Item("arcane_res")      'Arcane

            Spells(0).SpellID = MySQLQuery.Rows(0).Item("spellid_1")
            Spells(0).SpellTrigger = MySQLQuery.Rows(0).Item("spelltrigger_1")    '0="Use:" 1="Equip:" 2="Chance on Hit:"
            Spells(0).SpellCharges = MySQLQuery.Rows(0).Item("spellcharges_1")    '0=Doesn't disappear after use -1=Disappears after use
            Spells(0).SpellCooldown = MySQLQuery.Rows(0).Item("spellcooldown_1")
            Spells(0).SpellCategory = MySQLQuery.Rows(0).Item("spellcategory_1")
            Spells(0).SpellCategoryCooldown = MySQLQuery.Rows(0).Item("spellcategorycooldown_1")
            Spells(1).SpellID = MySQLQuery.Rows(0).Item("spellid_2")
            Spells(1).SpellTrigger = MySQLQuery.Rows(0).Item("spelltrigger_2")    '0="Use:" 1="Equip:" 2="Chance on Hit:"
            Spells(1).SpellCharges = MySQLQuery.Rows(0).Item("spellcharges_2")    '0=Doesn't disappear after use -1=Disappears after use
            Spells(1).SpellCooldown = MySQLQuery.Rows(0).Item("spellcooldown_2")
            Spells(1).SpellCategory = MySQLQuery.Rows(0).Item("spellcategory_2")
            Spells(1).SpellCategoryCooldown = MySQLQuery.Rows(0).Item("spellcategorycooldown_2")
            Spells(2).SpellID = MySQLQuery.Rows(0).Item("spellid_3")
            Spells(2).SpellTrigger = MySQLQuery.Rows(0).Item("spelltrigger_3")    '0="Use:" 1="Equip:" 2="Chance on Hit:"
            Spells(2).SpellCharges = MySQLQuery.Rows(0).Item("spellcharges_3")    '0=Doesn't disappear after use -1=Disappears after use
            Spells(2).SpellCooldown = MySQLQuery.Rows(0).Item("spellcooldown_3")
            Spells(2).SpellCategory = MySQLQuery.Rows(0).Item("spellcategory_3")
            Spells(2).SpellCategoryCooldown = MySQLQuery.Rows(0).Item("spellcategorycooldown_3")
            Spells(3).SpellID = MySQLQuery.Rows(0).Item("spellid_4")
            Spells(3).SpellTrigger = MySQLQuery.Rows(0).Item("spelltrigger_4")    '0="Use:" 1="Equip:" 2="Chance on Hit:"
            Spells(3).SpellCharges = MySQLQuery.Rows(0).Item("spellcharges_4")    '0=Doesn't disappear after use -1=Disappears after use
            Spells(3).SpellCooldown = MySQLQuery.Rows(0).Item("spellcooldown_4")
            Spells(3).SpellCategory = MySQLQuery.Rows(0).Item("spellcategory_4")
            Spells(3).SpellCategoryCooldown = MySQLQuery.Rows(0).Item("spellcategorycooldown_4")
            Spells(4).SpellID = MySQLQuery.Rows(0).Item("spellid_5")
            Spells(4).SpellTrigger = MySQLQuery.Rows(0).Item("spelltrigger_5")    '0="Use:" 1="Equip:" 2="Chance on Hit:"
            Spells(4).SpellCharges = MySQLQuery.Rows(0).Item("spellcharges_5")    '0=Doesn't disappear after use -1=Disappears after use
            Spells(4).SpellCooldown = MySQLQuery.Rows(0).Item("spellcooldown_5")
            Spells(4).SpellCategory = MySQLQuery.Rows(0).Item("spellcategory_5")
            Spells(4).SpellCategoryCooldown = MySQLQuery.Rows(0).Item("spellcategorycooldown_5")

            'Unknown
            LockID = MySQLQuery.Rows(0).Item("lock_id")
            'Extra = MySQLQuery.Rows(0).Item("item_extra")

            ItemBonusStatType(0) = MySQLQuery.Rows(0).Item("stat_type1")
            ItemBonusStatValue(0) = MySQLQuery.Rows(0).Item("stat_value1")
            ItemBonusStatType(1) = MySQLQuery.Rows(0).Item("stat_type2")
            ItemBonusStatValue(1) = MySQLQuery.Rows(0).Item("stat_value2")
            ItemBonusStatType(2) = MySQLQuery.Rows(0).Item("stat_type3")
            ItemBonusStatValue(2) = MySQLQuery.Rows(0).Item("stat_value3")
            ItemBonusStatType(3) = MySQLQuery.Rows(0).Item("stat_type4")
            ItemBonusStatValue(3) = MySQLQuery.Rows(0).Item("stat_value4")
            ItemBonusStatType(4) = MySQLQuery.Rows(0).Item("stat_type5")
            ItemBonusStatValue(4) = MySQLQuery.Rows(0).Item("stat_value5")
            ItemBonusStatType(5) = MySQLQuery.Rows(0).Item("stat_type6")
            ItemBonusStatValue(5) = MySQLQuery.Rows(0).Item("stat_value6")
            ItemBonusStatType(6) = MySQLQuery.Rows(0).Item("stat_type7")
            ItemBonusStatValue(6) = MySQLQuery.Rows(0).Item("stat_value7")
            ItemBonusStatType(7) = MySQLQuery.Rows(0).Item("stat_type8")
            ItemBonusStatValue(7) = MySQLQuery.Rows(0).Item("stat_value8")
            ItemBonusStatType(8) = MySQLQuery.Rows(0).Item("stat_type9")
            ItemBonusStatValue(8) = MySQLQuery.Rows(0).Item("stat_value9")
            ItemBonusStatType(9) = MySQLQuery.Rows(0).Item("stat_type10")
            ItemBonusStatValue(9) = MySQLQuery.Rows(0).Item("stat_value10")

            Field4 = MySQLQuery.Rows(0).Item("field4")
            Name2 = MySQLQuery.Rows(0).Item("name2")
            Name3 = MySQLQuery.Rows(0).Item("name3")
            Name4 = MySQLQuery.Rows(0).Item("name4")
            RandomProp = MySQLQuery.Rows(0).Item("randomprop")
            RandomSuffix = MySQLQuery.Rows(0).Item("randomsuffix") ' Not sure about this one
            ZoneNameID = MySQLQuery.Rows(0).Item("ZoneNameID")
            MapID = MySQLQuery.Rows(0).Item("mapid")
            TotemCategory = MySQLQuery.Rows(0).Item("TotemCategory")
            Sockets(0).Color = MySQLQuery.Rows(0).Item("socket_color_1")
            Sockets(0).Content = MySQLQuery.Rows(0).Item("socket_content_1")
            Sockets(1).Color = MySQLQuery.Rows(0).Item("socket_color_2")
            Sockets(1).Content = MySQLQuery.Rows(0).Item("socket_content_2")
            Sockets(2).Color = MySQLQuery.Rows(0).Item("socket_color_3")
            Sockets(2).Content = MySQLQuery.Rows(0).Item("socket_content_3")
            SocketBonus = MySQLQuery.Rows(0).Item("socket_bonus")
            GemProperties = MySQLQuery.Rows(0).Item("GemProperties")
            ReqDisenchantSkill = MySQLQuery.Rows(0).Item("ReqDisenchantSkill")
            ArmorDamageModifier = MySQLQuery.Rows(0).Item("armorDamageModifier")
            ExistingDuration = MySQLQuery.Rows(0).Item("ExistingDuration")

            'DONE: Internal database fixers
            If Stackable = 0 Then Stackable = 1

            Id = ItemId
            ITEMDatabase.Add(Id, Me)
        End Sub
        Public Sub Dispose() Implements System.IDisposable.Dispose
            ITEMDatabase.Remove(Id)
        End Sub

        'Item's visuals
        Public Model As Integer = 0
        Public Name As String = "Unknown Item"
        Public Name2 As String = ""
        Public Name3 As String = ""
        Public Name4 As String = ""
        Public Quality As Integer = 0
        Public Material As Integer = 0
        Public Durability As Integer = 0
        Public MaxCount As Integer = 0
        Public Sheath As SHEATHE_TYPE = 0
        Public Bonding As Integer = 0
        Public BuyPrice As Integer = 0
        Public SellPrice As Integer = 0

        'Item's Characteristics
        Public Id As Integer = 0
        Public Flags As Integer = 0
        Public ObjectClass As ITEM_CLASS = 0
        Public SubClass As ITEM_SUBCLASS = 0
        Public InventoryType As INVENTORY_TYPES = 0
        Public Level As Integer = 0

        Public AvailableClasses As UInteger = 0
        Public AvailableRaces As UInteger = 0
        Public ReqLevel As Integer = 0
        Public ReqSkill As Integer = 0
        Public ReqSkillRank As Integer = 0
        Public ReqSkillSubRank As Integer
        Public ReqSpell As Integer = 0
        Public ReqFaction As Integer = 0
        Public ReqFactionLevel As Integer = 0
        Public ReqHonorRank As Integer = 0
        Public ReqHonorRank2 As Integer = 0

        'Special items
        Public AmmoType As Integer = 0
        Public PageText As Integer = 0
        Public Stackable As Integer = 1
        Public Unique As Integer = 0
        Public Description As String = ""
        Public Block As Integer = 0
        Public ItemSet As Integer = 0
        Public PageMaterial As Integer = 0
        Public StartQuest As Integer = 0
        Public ContainerSlots As Integer = 0
        Public LanguageID As Integer = 0
        Public BagFamily As ITEM_BAG = 0
        Public GemProperties As Integer = 0

        'Item's bonuses
        Public Delay As Integer = 0
        Public Range As Single = 0
        Public Damage(4) As TDamage
        Public Resistances() As Integer = {0, 0, 0, 0, 0, 0, 0}
        Public ItemBonusStatType() As Integer = {0, 0, 0, 0, 0, 0, 0, 0, 0, 0}
        Public ItemBonusStatValue() As Integer = {0, 0, 0, 0, 0, 0, 0, 0, 0, 0}

        'Item's Spells
        Public Spells(4) As TItemSpellInfo
        Public Sockets(2) As TItemSocket
        Public SocketBonus As Integer = 0
        Public ReqDisenchantSkill As Integer = -1
        Public ArmorDamageModifier As Single = 0
        Public ExistingDuration As Integer = 0

        'Other
        Public Field4 As Integer = -1  ' Unknown_BC
        Public Unk2 As Integer = 0
        Public LockID As Integer = 0
        Public Extra As Integer = 0
        Public Area As Integer = 0
        Public ZoneNameID As Integer = 0
        Public MapID As Integer = 0
        Public TotemCategory As Integer = 0
        Public RandomProp As Integer = 0
        Public RandomSuffix As Integer = 0

        Public ReadOnly Property IsContainer() As Boolean
            Get
                If ContainerSlots > 0 Then Return True Else Return False
            End Get
        End Property
        Public ReadOnly Property GetSlots() As Byte()
            Get
                Select Case InventoryType
                    Case INVENTORY_TYPES.INVTYPE_HEAD
                        Return New Byte() {EQUIPMENT_SLOT_HEAD}
                        Exit Select
                    Case INVENTORY_TYPES.INVTYPE_NECK
                        Return New Byte() {EQUIPMENT_SLOT_NECK}
                        Exit Select
                    Case INVENTORY_TYPES.INVTYPE_SHOULDERS
                        Return New Byte() {EQUIPMENT_SLOT_SHOULDERS}
                        Exit Select
                    Case INVENTORY_TYPES.INVTYPE_BODY
                        Return New Byte() {EQUIPMENT_SLOT_BODY}
                        Exit Select
                    Case INVENTORY_TYPES.INVTYPE_CHEST
                        Return New Byte() {EQUIPMENT_SLOT_CHEST}
                        Exit Select
                    Case INVENTORY_TYPES.INVTYPE_ROBE
                        Return New Byte() {EQUIPMENT_SLOT_CHEST}
                        Exit Select
                    Case INVENTORY_TYPES.INVTYPE_WAIST
                        Return New Byte() {EQUIPMENT_SLOT_WAIST}
                        Exit Select
                    Case INVENTORY_TYPES.INVTYPE_LEGS
                        Return New Byte() {EQUIPMENT_SLOT_LEGS}
                        Exit Select
                    Case INVENTORY_TYPES.INVTYPE_FEET
                        Return New Byte() {EQUIPMENT_SLOT_FEET}
                        Exit Select
                    Case INVENTORY_TYPES.INVTYPE_WRISTS
                        Return New Byte() {EQUIPMENT_SLOT_WRISTS}
                        Exit Select
                    Case INVENTORY_TYPES.INVTYPE_HANDS
                        Return New Byte() {EQUIPMENT_SLOT_HANDS}
                        Exit Select
                    Case INVENTORY_TYPES.INVTYPE_FINGER
                        Return New Byte() {EQUIPMENT_SLOT_FINGER1, EQUIPMENT_SLOT_FINGER2}
                        Exit Select
                    Case INVENTORY_TYPES.INVTYPE_TRINKET
                        Return New Byte() {EQUIPMENT_SLOT_TRINKET1, EQUIPMENT_SLOT_TRINKET2}
                        Exit Select
                    Case INVENTORY_TYPES.INVTYPE_CLOAK
                        Return New Byte() {EQUIPMENT_SLOT_BACK}
                        Exit Select
                    Case INVENTORY_TYPES.INVTYPE_WEAPON
                        Return New Byte() {EQUIPMENT_SLOT_MAINHAND, EQUIPMENT_SLOT_OFFHAND}
                        Exit Select
                    Case INVENTORY_TYPES.INVTYPE_SHIELD
                        Return New Byte() {EQUIPMENT_SLOT_OFFHAND}
                        Exit Select
                    Case INVENTORY_TYPES.INVTYPE_RANGED
                        Return New Byte() {EQUIPMENT_SLOT_RANGED}
                        Exit Select
                    Case INVENTORY_TYPES.INVTYPE_TWOHAND_WEAPON
                        Return New Byte() {EQUIPMENT_SLOT_MAINHAND}
                        Exit Select
                    Case INVENTORY_TYPES.INVTYPE_TABARD
                        Return New Byte() {EQUIPMENT_SLOT_TABARD}
                        Exit Select
                    Case INVENTORY_TYPES.INVTYPE_WEAPONMAINHAND
                        Return New Byte() {EQUIPMENT_SLOT_MAINHAND}
                        Exit Select
                    Case INVENTORY_TYPES.INVTYPE_WEAPONOFFHAND
                        Return New Byte() {EQUIPMENT_SLOT_OFFHAND}
                        Exit Select
                    Case INVENTORY_TYPES.INVTYPE_HOLDABLE
                        Return New Byte() {EQUIPMENT_SLOT_OFFHAND}
                        Exit Select
                    Case INVENTORY_TYPES.INVTYPE_THROWN
                        Return New Byte() {EQUIPMENT_SLOT_RANGED}
                        Exit Select
                    Case INVENTORY_TYPES.INVTYPE_RANGEDRIGHT
                        Return New Byte() {EQUIPMENT_SLOT_RANGED}
                        Exit Select
                    Case INVENTORY_TYPES.INVTYPE_BAG
                        Return New Byte() {INVENTORY_SLOT_BAG_1, INVENTORY_SLOT_BAG_2, INVENTORY_SLOT_BAG_3, INVENTORY_SLOT_BAG_4}
                        Exit Select
                    Case INVENTORY_TYPES.INVTYPE_RELIC
                        Return New Byte() {EQUIPMENT_SLOT_RANGED}
                        Exit Select
                    Case Else
                        Return New Byte() {}
                End Select
            End Get
        End Property
        Public ReadOnly Property GetReqSkill() As Integer
            Get
                If ObjectClass = ITEM_CLASS.ITEM_CLASS_WEAPON Then Return item_weapon_skills(SubClass)
                If ObjectClass = ITEM_CLASS.ITEM_CLASS_ARMOR Then Return item_armor_skills(SubClass)
                Return 0
            End Get
        End Property
        Public ReadOnly Property GetReqSpell() As Short
            Get
                Select Case ObjectClass
                    Case ITEM_CLASS.ITEM_CLASS_WEAPON
                        Select Case SubClass
                            Case ITEM_SUBCLASS.ITEM_SUBCLASS_AXE
                                Return 196
                            Case ITEM_SUBCLASS.ITEM_SUBCLASS_TWOHAND_AXE
                                Return 197
                            Case ITEM_SUBCLASS.ITEM_SUBCLASS_BOW
                                Return 264
                            Case ITEM_SUBCLASS.ITEM_SUBCLASS_GUN
                                Return 266
                            Case ITEM_SUBCLASS.ITEM_SUBCLASS_MACE
                                Return 198
                            Case ITEM_SUBCLASS.ITEM_SUBCLASS_TWOHAND_MACE
                                Return 199
                            Case ITEM_SUBCLASS.ITEM_SUBCLASS_POLEARM
                                Return 200
                            Case ITEM_SUBCLASS.ITEM_SUBCLASS_SWORD
                                Return 201
                            Case ITEM_SUBCLASS.ITEM_SUBCLASS_TWOHAND_SWORD
                                Return 202
                            Case ITEM_SUBCLASS.ITEM_SUBCLASS_STAFF
                                Return 227
                            Case ITEM_SUBCLASS.ITEM_SUBCLASS_DAGGER
                                Return 1180
                            Case ITEM_SUBCLASS.ITEM_SUBCLASS_THROWN
                                Return 2567
                            Case ITEM_SUBCLASS.ITEM_SUBCLASS_SPEAR
                                Return 3386
                            Case ITEM_SUBCLASS.ITEM_SUBCLASS_CROSSBOW
                                Return 5011
                            Case ITEM_SUBCLASS.ITEM_SUBCLASS_WAND
                                Return 5009
                        End Select
                    Case ITEM_CLASS.ITEM_CLASS_ARMOR
                        Select Case SubClass
                            Case ITEM_SUBCLASS.ITEM_SUBCLASS_CLOTH
                                Return 9078
                            Case ITEM_SUBCLASS.ITEM_SUBCLASS_LEATHER
                                Return 9077
                            Case ITEM_SUBCLASS.ITEM_SUBCLASS_MAIL
                                Return 8737
                            Case ITEM_SUBCLASS.ITEM_SUBCLASS_PLATE
                                Return 750
                            Case ITEM_SUBCLASS.ITEM_SUBCLASS_SHIELD
                                Return 9116
                        End Select
                    Case Else
                        Return 0
                        Exit Select
                End Select
            End Get
        End Property
    End Class

    'WARNING: Use only with ITEMs()
    Public Class ItemObject
        Implements IDisposable

        Public ReadOnly Property ItemInfo() As ItemInfo
            Get
                Return ITEMDatabase(ItemEntry)
            End Get
        End Property

        Public ItemEntry As Integer
        Public GUID As ULong
        Public OwnerGUID As ULong
        Public GiftCreatorGUID As ULong = 0
        Public CreatorGUID As ULong

        Public StackCount As Byte = 1
        Public Durability As Integer = 1
        Public ChargesLeft As Integer = 0
        Public Flags As Integer = 0
        Public Items As Dictionary(Of Byte, ItemObject) = Nothing
        Public RandomProperties As Integer = 0
        Public SuffixFactor As Integer = 0
        Public Enchantments() As TEnchantmentInfo = {Nothing, Nothing, Nothing, Nothing, Nothing, Nothing, Nothing}

        'WARNING: Containers cannot hold itemText value
        Public ItemText As Integer = 0

        Public Sub FillAllUpdateFlags(ByRef Update As UpdateClass)
            If ItemInfo.ContainerSlots > 0 Then
                Update.SetUpdateFlag(EObjectFields.OBJECT_FIELD_GUID, GUID)
                Update.SetUpdateFlag(EObjectFields.OBJECT_FIELD_TYPE, CType(ObjectType.TYPE_CONTAINER + ObjectType.TYPE_OBJECT, Integer))
                Update.SetUpdateFlag(EObjectFields.OBJECT_FIELD_ENTRY, ItemEntry)
                Update.SetUpdateFlag(EObjectFields.OBJECT_FIELD_SCALE_X, 1.0F)

                Update.SetUpdateFlag(EItemFields.ITEM_FIELD_OWNER, OwnerGUID)
                Update.SetUpdateFlag(EItemFields.ITEM_FIELD_CONTAINED, OwnerGUID)
                If CreatorGUID > 0 Then Update.SetUpdateFlag(EItemFields.ITEM_FIELD_CREATOR, CreatorGUID)
                Update.SetUpdateFlag(EItemFields.ITEM_FIELD_GIFTCREATOR, GiftCreatorGUID)
                Update.SetUpdateFlag(EItemFields.ITEM_FIELD_STACK_COUNT, StackCount)
                'Update.SetUpdateFlag(EItemFields.ITEM_FIELD_DURATION, 0)
                Update.SetUpdateFlag(EItemFields.ITEM_FIELD_FLAGS, Flags)
                'Update.SetUpdateFlag(EItemFields.ITEM_FIELD_ITEM_TEXT_ID, ItemText)

                Update.SetUpdateFlag(EContainerFields.CONTAINER_FIELD_NUM_SLOTS, ItemInfo.ContainerSlots)
                'DONE: Here list in bag items
                Dim i As Byte
                For i = 0 To 35
                    If Items.ContainsKey(i) Then
                        Update.SetUpdateFlag(EContainerFields.CONTAINER_FIELD_SLOT_1 + i * 2, CType(Items(i).GUID, Long))
                    Else
                        Update.SetUpdateFlag(EContainerFields.CONTAINER_FIELD_SLOT_1 + i * 2, 0)
                    End If
                Next
            Else
                Update.SetUpdateFlag(EObjectFields.OBJECT_FIELD_GUID, GUID)
                Update.SetUpdateFlag(EObjectFields.OBJECT_FIELD_TYPE, CType(ObjectType.TYPE_ITEM + ObjectType.TYPE_OBJECT, Integer))
                Update.SetUpdateFlag(EObjectFields.OBJECT_FIELD_ENTRY, ItemEntry)
                Update.SetUpdateFlag(EObjectFields.OBJECT_FIELD_SCALE_X, 1.0F)

                Update.SetUpdateFlag(EItemFields.ITEM_FIELD_OWNER, OwnerGUID)
                Update.SetUpdateFlag(EItemFields.ITEM_FIELD_CONTAINED, OwnerGUID)
                If CreatorGUID > 0 Then Update.SetUpdateFlag(EItemFields.ITEM_FIELD_CREATOR, CreatorGUID)
                Update.SetUpdateFlag(EItemFields.ITEM_FIELD_GIFTCREATOR, GiftCreatorGUID)
                Update.SetUpdateFlag(EItemFields.ITEM_FIELD_STACK_COUNT, StackCount)
                'Update.SetUpdateFlag(EItemFields.ITEM_FIELD_DURATION, 0)
                Update.SetUpdateFlag(EItemFields.ITEM_FIELD_SPELL_CHARGES, ChargesLeft)         'NOTE: There are other 4 unused charges fields
                Update.SetUpdateFlag(EItemFields.ITEM_FIELD_FLAGS, Flags)

                'Update.SetUpdateFlag(EItemFields.ITEM_FIELD_PROPERTY_SEED, 0)
                Update.SetUpdateFlag(EItemFields.ITEM_FIELD_RANDOM_PROPERTIES_ID, RandomProperties)

                'Update.SetUpdateFlag(EItemFields.ITEM_FIELD_ENCHANTMENT, 0)        'Enchantment ID
                'Update.SetUpdateFlag(EItemFields.ITEM_FIELD_ENCHANTMENT + 1, 0)    'Enchantment Duration
                'Update.SetUpdateFlag(EItemFields.ITEM_FIELD_ENCHANTMENT + 2, 0)    'Enchantment Charges
                For i As Integer = 0 To Enchantments.Length - 1
                    If Not Enchantments(i) Is Nothing Then
                        Update.SetUpdateFlag(EItemFields.ITEM_FIELD_ENCHANTMENT + i * 3, Enchantments(i).SpellID)
                        Update.SetUpdateFlag(EItemFields.ITEM_FIELD_ENCHANTMENT + i * 3 + 1, Enchantments(i).Duration)
                        Update.SetUpdateFlag(EItemFields.ITEM_FIELD_ENCHANTMENT + i * 3 + 2, Enchantments(i).Charges)
                    End If
                Next

                Update.SetUpdateFlag(EItemFields.ITEM_FIELD_ITEM_TEXT_ID, ItemText)
                Update.SetUpdateFlag(EItemFields.ITEM_FIELD_DURABILITY, Durability)
                Update.SetUpdateFlag(EItemFields.ITEM_FIELD_MAXDURABILITY, ITEMDatabase(ItemEntry).Durability)
            End If
        End Sub
        Public Sub SendContainedItemsUpdate(ByRef Client As ClientClass, Optional ByVal UPDATETYPE As Integer = ObjectUpdateType.UPDATETYPE_CREATE_OBJECT)
            Dim packet As New PacketClass(OPCODES.SMSG_UPDATE_OBJECT)
            packet.AddInt32(Items.Count)      'Operations.Count
            packet.AddInt8(0)

            For Each Item As KeyValuePair(Of Byte, ItemObject) In Items
                Dim tmpUpdate As New UpdateClass(FIELD_MASK_SIZE_ITEM)
                Item.Value.FillAllUpdateFlags(tmpUpdate)
                tmpUpdate.AddToPacket(packet, UPDATETYPE, CType(Item.Value, ItemObject), 0)
                tmpUpdate.Dispose()
            Next

            Client.Send(packet)
        End Sub

        Public Sub InitializeBag()
            If ITEMDatabase(ItemEntry).IsContainer Then
                Items = New Dictionary(Of Byte, ItemObject)
            Else
                Items = Nothing
            End If
        End Sub
        Public ReadOnly Property IsFree() As Boolean
            Get
                If Items.Count > 0 Then Return False Else Return True
            End Get
        End Property
        Public ReadOnly Property IsFull() As Boolean
            Get
                If Items.Count = ITEMDatabase(ItemEntry).ContainerSlots Then Return True Else Return False
            End Get
        End Property

        Public ReadOnly Property GetBagSlot() As Byte
            Get
                If CHARACTERs.ContainsKey(OwnerGUID) = False Then Return 255

                With CHARACTERs(OwnerGUID)
                    For i As Byte = INVENTORY_SLOT_BAG_1 To INVENTORY_SLOT_BAG_END - 1
                        If .Items.ContainsKey(i) Then
                            For j As Byte = 0 To .Items(i).ItemInfo.ContainerSlots - 1
                                If .Items(i).Items.ContainsKey(j) Then
                                    If .Items(i).Items(j) Is Me Then Return i
                                End If
                            Next
                        End If
                    Next
                End With

                Return 255
            End Get
        End Property

        Public ReadOnly Property GetSlot() As Integer
            Get
                If CHARACTERs.ContainsKey(OwnerGUID) = False Then Return -1

                With CHARACTERs(OwnerGUID)
                    For i As Byte = EQUIPMENT_SLOT_START To INVENTORY_SLOT_ITEM_END - 1
                        If .Items.ContainsKey(i) Then
                            If .Items(i) Is Me Then Return i
                        End If
                    Next

                    For i As Byte = KEYRING_SLOT_START To KEYRING_SLOT_END - 1
                        If .Items.ContainsKey(i) Then
                            If .Items(i) Is Me Then Return i
                        End If
                    Next

                    For i As Byte = INVENTORY_SLOT_BAG_1 To INVENTORY_SLOT_BAG_END - 1
                        If .Items.ContainsKey(i) Then
                            For j As Byte = 0 To .Items(i).ItemInfo.ContainerSlots - 1
                                If .Items(i).Items.ContainsKey(j) Then
                                    If .Items(i).Items(j) Is Me Then Return j
                                End If
                            Next
                        End If
                    Next
                End With

                Return -1
            End Get
        End Property

        Public Sub New(ByVal GUIDVal As ULong)
            'DONE: Get from SQLDB
            Dim MySQLQuery As New DataTable
            Database.Query(String.Format("SELECT * FROM characters_inventory WHERE item_guid = ""{0}"";", GUIDVal), MySQLQuery)
            If MySQLQuery.Rows.Count = 0 Then Err.Raise(1, "ItemObject.New", String.Format("ItemGUID {0} not found in SQL database!", GUIDVal))

            GUID = MySQLQuery.Rows(0).Item("item_guid") + GUID_ITEM
            CreatorGUID = MySQLQuery.Rows(0).Item("item_creator")
            OwnerGUID = MySQLQuery.Rows(0).Item("item_owner")
            GiftCreatorGUID = MySQLQuery.Rows(0).Item("item_giftCreator")
            StackCount = MySQLQuery.Rows(0).Item("item_stackCount")
            Durability = MySQLQuery.Rows(0).Item("item_durability")
            ChargesLeft = MySQLQuery.Rows(0).Item("item_chargesLeft")
            RandomProperties = MySQLQuery.Rows(0).Item("item_random_properties")
            ItemEntry = MySQLQuery.Rows(0).Item("item_id")
            Flags = MySQLQuery.Rows(0).Item("item_flags")
            ItemText = MySQLQuery.Rows(0).Item("item_textId")

            'DONE: Intitialize enchantments - Saved as STRING like "SpellID1:Duration:Charges SpellID2:Duration:Charges SpellID3:Duration:Charges"
            Dim tmp() As String = Split(CType(MySQLQuery.Rows(0).Item("item_enchantment"), String), " ")
            Dim i As Integer
            If tmp.Length > 0 Then
                For i = 0 To tmp.Length - 1
                    If Trim(tmp(i)) <> "" Then
                        Dim tmp2() As String
                        tmp2 = Split(tmp(i), ":")
                        Enchantments(i) = New TEnchantmentInfo(tmp2(0), tmp2(1), tmp2(1))
                    End If
                Next i
            End If

            'DONE: Load ItemID in cashe if not loaded
            If ITEMDatabase.ContainsKey(ItemEntry) = False Then
                Dim tmpItem As New ItemInfo(ItemEntry)
            End If

            InitializeBag()

            'DONE: Get Items
            MySQLQuery.Clear()
            Database.Query(String.Format("SELECT * FROM characters_inventory WHERE item_bag = {0};", GUID), MySQLQuery)
            For Each row As DataRow In MySQLQuery.Rows
                If row.Item("item_slot") <> ITEM_SLOT_NULL Then
                    Dim tmpItem As New ItemObject(CType(row.Item("item_guid"), Long))
                    Items(CType(row.Item("item_slot"), Byte)) = tmpItem
                End If
            Next

            WORLD_ITEMs.Add(GUID, Me)
        End Sub
        Public Sub New(ByVal ItemId As Integer, ByVal Owner As ULong)
            'DONE: Load ItemID in cashe if not loaded
            If ITEMDatabase.ContainsKey(ItemId) = False Then
                Dim tmpItem As New ItemInfo(ItemId)
            End If
            ItemEntry = ItemId
            OwnerGUID = Owner
            Durability = ITEMDatabase(ItemEntry).Durability

            'DONE: Create new GUID
            GUID = GetNewGUID()
            InitializeBag()
            SaveAsNew()

            WORLD_ITEMs.Add(GUID, Me)
        End Sub
        Public Sub SaveAsNew()
            'DONE: Save to SQL
            Dim tmpCMD As String = "INSERT INTO characters_inventory (item_guid"
            Dim tmpValues As String = " VALUES (" & GUID - GUID_ITEM
            tmpCMD = tmpCMD & ", item_owner"
            tmpValues = tmpValues & ", """ & OwnerGUID & """"
            tmpCMD = tmpCMD & ", item_creator"
            tmpValues = tmpValues & ", " & CreatorGUID
            tmpCMD = tmpCMD & ", item_giftCreator"
            tmpValues = tmpValues & ", " & GiftCreatorGUID
            tmpCMD = tmpCMD & ", item_stackCount"
            tmpValues = tmpValues & ", " & StackCount
            tmpCMD = tmpCMD & ", item_durability"
            tmpValues = tmpValues & ", " & Durability
            tmpCMD = tmpCMD & ", item_chargesLeft"
            tmpValues = tmpValues & ", " & ChargesLeft
            tmpCMD = tmpCMD & ", item_random_properties"
            tmpValues = tmpValues & ", " & RandomProperties
            tmpCMD = tmpCMD & ", item_id"
            tmpValues = tmpValues & ", " & ItemEntry
            tmpCMD = tmpCMD & ", item_flags"
            tmpValues = tmpValues & ", " & Flags

            'DONE: Saving enchanments
            Dim temp As New ArrayList
            For Each Enchantment As TEnchantmentInfo In Enchantments
                If Not Enchantment Is Nothing Then temp.Add(String.Format("{0}:{1}:{2}", Enchantment.SpellID, Enchantment.Duration, Enchantment.Charges))
            Next
            tmpCMD = tmpCMD & ", item_enchantment"
            tmpValues = tmpValues & ", '" & Join(temp.ToArray, " ") & "'"
            tmpCMD = tmpCMD & ", item_textId"
            tmpValues = tmpValues & ", " & ItemText

            tmpCMD = tmpCMD & ") " & tmpValues & ");"
            Database.Update(tmpCMD)
        End Sub
        Public Sub Save(Optional ByVal saveAll As Boolean = True)
            Dim tmp As String = "UPDATE characters_inventory SET"

            tmp = tmp & " item_owner=""" & OwnerGUID & """"
            tmp = tmp & ", item_creator=" & CreatorGUID
            tmp = tmp & ", item_giftCreator=" & GiftCreatorGUID
            tmp = tmp & ", item_stackCount=" & StackCount
            tmp = tmp & ", item_durability=" & Durability
            tmp = tmp & ", item_chargesLeft=" & ChargesLeft
            tmp = tmp & ", item_random_properties=" & RandomProperties
            tmp = tmp & ", item_flags=" & Flags

            'DONE: Saving enchanments
            Dim temp As New ArrayList
            For Each Enchantment As TEnchantmentInfo In Enchantments
                If Not Enchantment Is Nothing Then temp.Add(String.Format("{0}:{1}:{2}", Enchantment.SpellID, Enchantment.Duration, Enchantment.Charges))
            Next
            tmp = tmp & ", item_enchantment=""" & Join(temp.ToArray, " ") & """"
            tmp = tmp & ", item_textId=" & ItemText

            tmp = tmp & " WHERE item_guid = """ & (GUID - GUID_ITEM) & """;"

            Database.Update(tmp)

            If ITEMDatabase(ItemEntry).IsContainer() AndAlso saveAll Then
                For Each Item As KeyValuePair(Of Byte, ItemObject) In Items
                    Item.Value.Save()
                Next
            End If
        End Sub
        Public Sub Delete()
            'DONE: Check if item is petition
            If ItemEntry = PETITION_GUILD OrElse ItemEntry = PETITION_2v2 OrElse ItemEntry = PETITION_3v3 OrElse ItemEntry = PETITION_5v5 Then Database.Update("DELETE FROM petitions WHERE petition_itemGuid = " & GUID - GUID_ITEM & ";")

            Database.Update(String.Format("DELETE FROM characters_inventory WHERE item_guid = {0}", GUID - GUID_ITEM))

            If ITEMDatabase(ItemEntry).IsContainer() Then
                For Each Item As KeyValuePair(Of Byte, ItemObject) In Items
                    Item.Value.Delete()
                Next
            End If
            Me.Dispose()
        End Sub
        Public Sub Dispose() Implements System.IDisposable.Dispose
            WORLD_ITEMs.Remove(GUID)

            If ITEMDatabase(ItemEntry).IsContainer() Then
                For Each Item As KeyValuePair(Of Byte, ItemObject) In Items
                    Item.Value.Dispose()
                Next
            End If
        End Sub

        Public Function IsBroken() As Boolean
            Return (Durability = 0) AndAlso (ItemInfo.Durability > 0)
        End Function
        Public Sub ModifyDurability(ByVal percent As Single, ByRef Client As ClientClass)
            If ITEMDatabase(ItemEntry).Durability > 0 Then
                Durability -= Fix(ITEMDatabase(ItemEntry).Durability * percent)
                If Durability < 0 Then Durability = 0
                If Durability > ITEMDatabase(ItemEntry).Durability Then Durability = ITEMDatabase(ItemEntry).Durability

                Dim packet As New PacketClass(OPCODES.SMSG_UPDATE_OBJECT)
                packet.AddInt32(1)      'Operations.Count
                packet.AddInt8(0)

                Dim tmpUpdate As New UpdateClass(FIELD_MASK_SIZE_ITEM)
                tmpUpdate.SetUpdateFlag(EItemFields.ITEM_FIELD_DURABILITY, Durability)
                tmpUpdate.AddToPacket(packet, ObjectUpdateType.UPDATETYPE_VALUES, Me, 0)
                tmpUpdate.Dispose()

                Client.Send(packet)
            End If
        End Sub
        Public ReadOnly Property GetDurabulityCost() As Byte
            Get
                If ITEMDatabase(ItemEntry).Durability - Durability > DurabilityCosts_MAX Then
                    Log.WriteLine(LogType.DEBUG, "Durability Cost: {0}", DurabilityCosts(DurabilityCosts_MAX, ITEMDatabase(ItemEntry).InventoryType))
                    Return DurabilityCosts(DurabilityCosts_MAX, ITEMDatabase(ItemEntry).InventoryType)
                Else
                    Log.WriteLine(LogType.DEBUG, "Durability Cost: {0}", DurabilityCosts(ITEMDatabase(ItemEntry).Durability - Durability, ITEMDatabase(ItemEntry).InventoryType))
                    Return DurabilityCosts(ITEMDatabase(ItemEntry).Durability - Durability, ITEMDatabase(ItemEntry).InventoryType)
                End If
            End Get
        End Property

        Public Sub AddEnchantment(ByVal SpellID As Integer, Optional ByVal Duration As Integer = 0, Optional ByVal Charges As Integer = 0, Optional ByVal Slot As Byte = 255)
            'DONE: Find free slot
            If Slot = 255 Then
                For i As Integer = 0 To Enchantments.Length - 1
                    If Enchantments(i) Is Nothing Then
                        Slot = i
                        Exit For
                    End If
                Next
            End If

            Enchantments(Slot) = New TEnchantmentInfo(SpellID, Duration, Charges)
        End Sub
        Public Sub RemoveEnchantment(ByVal Slot As Byte)
            Enchantments(Slot) = Nothing
        End Sub

        Public Sub SoulbindItem(Optional ByRef Client As ClientClass = Nothing)
            If (Flags And ITEM_FLAGS.ITEM_FLAGS_BINDED) = ITEM_FLAGS.ITEM_FLAGS_BINDED Then Exit Sub

            'DONE: Setting the flag
            Flags += ITEM_FLAGS.ITEM_FLAGS_BINDED
            Me.Save()

            'DONE: Sending update to character
            If Not Client Is Nothing Then
                Dim packet As New PacketClass(OPCODES.SMSG_UPDATE_OBJECT)
                packet.AddInt32(1)      'Operations.Count
                packet.AddInt8(0)

                Dim tmpUpdate As New UpdateClass(FIELD_MASK_SIZE_ITEM)
                tmpUpdate.SetUpdateFlag(EItemFields.ITEM_FIELD_FLAGS, Flags)
                tmpUpdate.AddToPacket(packet, ObjectUpdateType.UPDATETYPE_VALUES, Me, 0)

                Client.Send(packet)
                packet.Dispose()
                tmpUpdate.Dispose()
            End If
        End Sub
        Public ReadOnly Property IsSoulBound() As Boolean
            Get
                Return ((Flags And ITEM_FLAGS.ITEM_FLAGS_BINDED) = ITEM_FLAGS.ITEM_FLAGS_BINDED)
            End Get
        End Property
    End Class
    Public Class TDamage
        Public Minimum As Single = 0
        Public Maximum As Single = 0
        Public Type As Integer = DamageTypes.DMG_PHYSICAL
    End Class
    Public Class TEnchantmentInfo
        Public SpellID As Integer = 0
        Public Duration As Integer = 0
        Public Charges As Integer = 0

        Public Sub New(ByVal SpellID_ As Integer, Optional ByVal Duration_ As Integer = 0, Optional ByVal Charges_ As Integer = 0)
            SpellID = SpellID_
            Duration = Duration_
            Charges = Charges_
        End Sub
    End Class
    Public Class TItemSpellInfo
        Public SpellID As Integer = 0
        Public SpellTrigger As ITEM_SPELLTRIGGER_TYPE = 0
        Public SpellCharges As Integer = -1
        Public SpellCooldown As Integer = 0
        Public SpellCategory As Integer = 0
        Public SpellCategoryCooldown As Integer = 0
    End Class
    Public Class TItemSocket
        Public Color As ITEM_GEMS
        Public Content As UInteger = 0
    End Class

#End Region
#Region "WS.Items.Handlers"

    <MethodImplAttribute(MethodImplOptions.Synchronized)> _
    Private Function GetNewGUID() As ULong
        ItemGUIDCounter += 1
        GetNewGUID = ItemGUIDCounter
    End Function
    Public Function LoadItemByGUID(ByVal GUID As ULong) As ItemObject
        If WORLD_ITEMs.ContainsKey(GUID + GUID_ITEM) Then
            Return WORLD_ITEMs(GUID + GUID_ITEM)
        End If

        Dim tmpItem As New ItemObject(GUID)
        Return tmpItem
    End Function
    Public Sub SendItemInfo(ByRef Client As ClientClass, ByVal ItemID As Integer)
        Dim response As New PacketClass(OPCODES.SMSG_ITEM_QUERY_SINGLE_RESPONSE)

        Dim Item As ItemInfo

        If ITEMDatabase.ContainsKey(ItemID) = False Then
            Item = New ItemInfo(ItemID)
        Else
            Item = ITEMDatabase(ItemID)
        End If
        Dim i As Integer

        response.AddInt32(Item.Id)
        response.AddInt32(Item.ObjectClass)
        response.AddInt32(Item.SubClass)
        response.AddInt32(-1)
        response.AddString(Item.Name)
        response.AddInt8(0)     'Item.Name2
        response.AddInt8(0)     'Item.Name3
        response.AddInt8(0)     'Item.Name4

        response.AddInt32(Item.Model)
        response.AddInt32(Item.Quality)
        response.AddInt32(Item.Flags)
        response.AddInt32(Item.BuyPrice)
        response.AddInt32(Item.SellPrice)
        response.AddInt32(Item.InventoryType)
        response.AddUInt32(Item.AvailableClasses)
        response.AddUInt32(Item.AvailableRaces)
        response.AddInt32(Item.Level)
        response.AddInt32(Item.ReqLevel)
        response.AddInt32(Item.ReqSkill)
        response.AddInt32(Item.ReqSkillRank)
        response.AddInt32(Item.ReqSkillSubRank) ' Item.ReqSpell
        response.AddInt32(Item.ReqHonorRank)
        response.AddInt32(Item.ReqHonorRank2)                        'RequiredCityRank           [1 - Protector of Stormwind, 2 - Overlord of Orgrimmar, 3 - Thane of Ironforge, 4 - High Sentinel of Darnassus, 5 - Deathlord of the Undercity, 6 - Chieftan of Thunderbluff, 7 - Avenger of Gnomeregan, 8 - Voodoo Boss of Senjin]
        response.AddInt32(Item.ReqFaction)          'RequiredReputationFaction
        response.AddInt32(Item.ReqFactionLevel)     'RequiredRaputationRank
        response.AddInt32(Item.Unique) ' Was stackable
        response.AddInt32(Item.MaxCount)
        response.AddInt32(Item.ContainerSlots)

        For i = 0 To 9
            response.AddInt32(Item.ItemBonusStatType(i))
            response.AddInt32(Item.ItemBonusStatValue(i))
        Next
        For i = 0 To 4
            response.AddSingle(Item.Damage(i).Minimum)
            response.AddSingle(Item.Damage(i).Maximum)
            response.AddInt32(Item.Damage(i).Type)
        Next
        For i = 0 To 6
            response.AddInt32(Item.Resistances(i))
        Next

        response.AddInt32(Item.Delay)
        response.AddInt32(Item.AmmoType)
        response.AddSingle(Item.Range)          'itemRangeModifier (Ranged Weapons = 100.0, Fishing Poles = 3.0)

        For i = 0 To 4
            response.AddInt32(Item.Spells(i).SpellID)
            response.AddInt32(Item.Spells(i).SpellTrigger)
            response.AddInt32(Item.Spells(i).SpellCharges)
            response.AddInt32(Item.Spells(i).SpellCooldown)
            response.AddInt32(Item.Spells(i).SpellCategory)
            response.AddInt32(Item.Spells(i).SpellCategoryCooldown)
        Next

        response.AddInt32(Item.Bonding)
        response.AddString(Item.Description)
        response.AddInt32(Item.PageText)
        response.AddInt32(Item.LanguageID)
        response.AddInt32(Item.PageMaterial)
        response.AddInt32(Item.StartQuest)
        response.AddInt32(Item.LockID)
        response.AddInt32(Item.Material)
        response.AddInt32(Item.Sheath)
        response.AddInt32(Item.RandomProp)
        response.AddInt32(Item.RandomSuffix)
        response.AddInt32(Item.Block)
        response.AddInt32(Item.ItemSet)
        response.AddInt32(Item.Durability)
        response.AddInt32(Item.ZoneNameID)
        response.AddInt32(Item.MapID)
        response.AddInt32(Item.BagFamily)

        response.AddInt32(Item.TotemCategory)
        response.AddInt32(Item.Sockets(0).Color)
        response.AddInt32(Item.Sockets(0).Content)
        response.AddInt32(Item.Sockets(1).Color)
        response.AddInt32(Item.Sockets(1).Content)
        response.AddInt32(Item.Sockets(2).Color)
        response.AddInt32(Item.Sockets(2).Content)
        response.AddInt32(Item.SocketBonus)
        response.AddInt32(Item.GemProperties) 'itemGemID
        response.AddInt32(Item.ReqDisenchantSkill)
        response.AddInt32(Item.ArmorDamageModifier)
        response.AddInt32(Item.ExistingDuration)

        Client.Send(response)
        response.Dispose()
    End Sub
    Public Sub On_CMSG_ITEM_QUERY_SINGLE(ByRef packet As PacketClass, ByRef Client As ClientClass)
        If (packet.Data.Length - 1) < 9 Then Exit Sub
        packet.GetInt16()
        Dim ItemID As Integer = packet.GetInt32

        SendItemInfo(Client, ItemID)
    End Sub
    Public Sub On_CMSG_ITEM_NAME_QUERY(ByRef packet As PacketClass, ByRef Client As ClientClass)
        If (packet.Data.Length - 1) < 9 Then Exit Sub
        packet.GetInt16()
        Dim ItemID As Integer = packet.GetInt32

        Dim Item As ItemInfo

        If ITEMDatabase.ContainsKey(ItemID) = False Then
            Item = New ItemInfo(ItemID)
        Else
            Item = ITEMDatabase(ItemID)
        End If

        Dim response As New PacketClass(OPCODES.SMSG_ITEM_NAME_QUERY_RESPONSE)
        response.AddInt32(ItemID)
        response.AddString(Item.Name)
        Client.Send(response)
        response.Dispose()
    End Sub
    Public Sub On_CMSG_SWAP_INV_ITEM(ByRef packet As PacketClass, ByRef Client As ClientClass)
        If (packet.Data.Length - 1) < 7 Then Exit Sub
        packet.GetInt16()
        Dim srcSlot As Byte = packet.GetInt8
        Dim dstSlot As Byte = packet.GetInt8
        Log.WriteLine(LogType.DEBUG, "[{0}:{1}] CMSG_SWAP_INV_ITEM [srcSlot=0:{2}, dstSlot=0:{3}]", Client.IP, Client.Port, srcSlot, dstSlot)

        Client.Character.ItemSWAP(0, srcSlot, 0, dstSlot)
    End Sub
    Public Sub On_CMSG_AUTOEQUIP_ITEM(ByRef packet As PacketClass, ByRef Client As ClientClass)
        If (packet.Data.Length - 1) < 7 Then Exit Sub
        Try
            packet.GetInt16()
            Dim srcBag As Byte = packet.GetInt8
            Dim srcSlot As Byte = packet.GetInt8
            If srcBag = 255 Then srcBag = 0
            Log.WriteLine(LogType.DEBUG, "[{0}:{1}] CMSG_AUTOEQUIP_ITEM [srcSlot={3}:{2}]", Client.IP, Client.Port, srcSlot, srcBag)

            Dim errCode As Byte = InventoryChangeFailure.EQUIP_ERR_ITEM_CANT_BE_EQUIPPED

            'DONE: Check owner of the item
            If Client.Character.ItemGET(srcBag, srcSlot).OwnerGUID <> Client.Character.GUID Then
                errCode = InventoryChangeFailure.EQUIP_ERR_DONT_OWN_THAT_ITEM
            Else

                If srcBag = 0 AndAlso Client.Character.Items.ContainsKey(srcSlot) Then
                    Dim Slots() As Byte = Client.Character.Items(srcSlot).ItemInfo.GetSlots
                    For Each tmpSlot As Byte In Slots
                        If Not Client.Character.Items.ContainsKey(tmpSlot) Then
                            Client.Character.ItemSWAP(srcBag, srcSlot, 0, tmpSlot)
                            errCode = InventoryChangeFailure.EQUIP_ERR_OK
                            Exit For
                        Else
                            errCode = InventoryChangeFailure.EQUIP_ERR_NO_EQUIPMENT_SLOT_AVAILABLE
                        End If
                    Next
                    If errCode = InventoryChangeFailure.EQUIP_ERR_NO_EQUIPMENT_SLOT_AVAILABLE Then
                        For Each tmpSlot As Byte In Slots
                            Client.Character.ItemSWAP(srcBag, srcSlot, 0, tmpSlot)
                            errCode = InventoryChangeFailure.EQUIP_ERR_OK
                            Exit For
                        Next
                    End If
                ElseIf srcBag > 0 Then
                    Dim Slots() As Byte = Client.Character.Items(srcBag).Items(srcSlot).ItemInfo.GetSlots
                    For Each tmpSlot As Byte In Slots
                        If Not Client.Character.Items.ContainsKey(tmpSlot) Then
                            Client.Character.ItemSWAP(srcBag, srcSlot, 0, tmpSlot)
                            errCode = InventoryChangeFailure.EQUIP_ERR_OK
                            Exit For
                        Else
                            errCode = InventoryChangeFailure.EQUIP_ERR_NO_EQUIPMENT_SLOT_AVAILABLE
                        End If
                    Next
                    If errCode = InventoryChangeFailure.EQUIP_ERR_NO_EQUIPMENT_SLOT_AVAILABLE Then
                        For Each tmpSlot As Byte In Slots
                            Client.Character.ItemSWAP(srcBag, srcSlot, 0, tmpSlot)
                            errCode = InventoryChangeFailure.EQUIP_ERR_OK
                            Exit For
                        Next
                    End If
                Else
                    errCode = InventoryChangeFailure.EQUIP_ERR_ITEM_NOT_FOUND
                End If

            End If

            If errCode <> InventoryChangeFailure.EQUIP_ERR_OK Then
                Dim response As New PacketClass(OPCODES.SMSG_INVENTORY_CHANGE_FAILURE)
                response.AddInt8(errCode)
                response.AddUInt64(Client.Character.ItemGetGUID(srcBag, srcSlot))
                response.AddUInt64(0)
                response.AddInt8(0)
                Client.Send(response)
                response.Dispose()
            End If
        Catch err As Exception
            Log.WriteLine(LogType.FAILED, "[{0}:{1}] Unable to equip item. {2}{3}", Client.IP, Client.Port, vbNewLine, err.ToString)
        End Try
    End Sub
    Public Sub On_CMSG_AUTOSTORE_BAG_ITEM(ByRef packet As PacketClass, ByRef Client As ClientClass)
        If (packet.Data.Length - 1) < 8 Then Exit Sub
        packet.GetInt16()
        Dim srcBag As Byte = packet.GetInt8
        Dim srcSlot As Byte = packet.GetInt8
        Dim dstBag As Byte = packet.GetInt8
        If srcBag = 255 Then srcBag = 0
        If dstBag = 255 Then dstBag = 0
        Log.WriteLine(LogType.DEBUG, "[{0}:{1}] CMSG_AUTOSTORE_BAG_ITEM [srcSlot={3}:{2}, dstBag={4}]", Client.IP, Client.Port, srcSlot, srcBag, dstBag)

        If Client.Character.ItemADD_AutoBag(WORLD_ITEMs(Client.Character.ItemGetGUID(srcBag, srcSlot)), dstBag) Then
            Client.Character.ItemREMOVE(srcBag, srcSlot, False, True)
            Dim response As New PacketClass(OPCODES.SMSG_INVENTORY_CHANGE_FAILURE)
            response.AddInt8(InventoryChangeFailure.EQUIP_ERR_OK)
            response.AddUInt64(Client.Character.ItemGetGUID(srcBag, srcSlot))
            response.AddUInt64(0)
            response.AddInt8(0)
            Client.Send(response)
            response.Dispose()
        End If
    End Sub
    Public Sub On_CMSG_SWAP_ITEM(ByRef packet As PacketClass, ByRef Client As ClientClass)
        If (packet.Data.Length - 1) < 9 Then Exit Sub
        packet.GetInt16()
        Dim dstBag As Byte = packet.GetInt8
        Dim dstSlot As Byte = packet.GetInt8
        Dim srcBag As Byte = packet.GetInt8
        Dim srcSlot As Byte = packet.GetInt8
        If dstBag = 255 Then dstBag = 0
        If srcBag = 255 Then srcBag = 0

        Log.WriteLine(LogType.DEBUG, "[{0}:{1}] CMSG_SWAP_ITEM [srcSlot={4}:{2}, dstSlot={5}:{3}]", Client.IP, Client.Port, srcSlot, dstSlot, srcBag, dstBag)
        Client.Character.ItemSWAP(srcBag, srcSlot, dstBag, dstSlot)
    End Sub
    Public Sub On_CMSG_SPLIT_ITEM(ByRef packet As PacketClass, ByRef Client As ClientClass)
        If (packet.Data.Length - 1) < 10 Then Exit Sub
        packet.GetInt16()
        Dim srcBag As Byte = packet.GetInt8
        Dim srcSlot As Byte = packet.GetInt8
        Dim dstBag As Byte = packet.GetInt8
        Dim dstSlot As Byte = packet.GetInt8
        Dim count As Byte = packet.GetInt8
        If dstBag = 255 Then dstBag = 0
        If srcBag = 255 Then srcBag = 0

        Log.WriteLine(LogType.DEBUG, "[{0}:{1}] CMSG_SPLIT_ITEM [srcSlot={3}:{2}, dstBag={5}:{4}, count={6}]", Client.IP, Client.Port, srcSlot, srcBag, dstSlot, dstBag, count)
        If srcBag = dstBag AndAlso srcSlot = dstSlot Then Return
        If count > 0 Then Client.Character.ItemSPLIT(srcBag, srcSlot, dstBag, dstSlot, count)
    End Sub
    Public Sub On_CMSG_READ_ITEM(ByRef packet As PacketClass, ByRef Client As ClientClass)
        If (packet.Data.Length - 1) < 7 Then Exit Sub
        packet.GetInt16()
        Dim srcBag As Byte = packet.GetInt8
        Dim srcSlot As Byte = packet.GetInt8
        If srcBag = 255 Then srcBag = 0
        Log.WriteLine(LogType.DEBUG, "[{0}:{1}] CMSG_READ_ITEM [srcSlot={3}:{2}]", Client.IP, Client.Port, srcSlot, srcBag)

        'TODO: If InCombat/Dead
        Dim Opcode As Short = OPCODES.SMSG_READ_ITEM_FAILED
        Dim GUID As ULong = 0

        If srcBag = 0 Then
            If Client.Character.Items.ContainsKey(srcSlot) Then
                Opcode = OPCODES.SMSG_READ_ITEM_OK
                If Client.Character.Items(srcSlot).ItemInfo.PageText > 0 Then GUID = Client.Character.Items(srcSlot).GUID
            End If
        Else
            If Client.Character.Items.ContainsKey(srcBag) Then
                If Client.Character.Items(srcBag).Items.ContainsKey(srcSlot) Then
                    Opcode = OPCODES.SMSG_READ_ITEM_OK
                    If Client.Character.Items(srcBag).Items(srcSlot).ItemInfo.PageText > 0 Then GUID = Client.Character.Items(srcBag).Items(srcSlot).GUID
                End If
            End If
        End If

        If GUID <> 0 Then
            Dim response As New PacketClass(Opcode)
            response.AddUInt64(GUID)
            Client.Send(response)
            response.Dispose()
        End If
    End Sub
    Public Sub On_CMSG_PAGE_TEXT_QUERY(ByRef packet As PacketClass, ByRef Client As ClientClass)
        If (packet.Data.Length - 1) < 17 Then Exit Sub
        packet.GetInt16()
        Dim pageID As Integer = packet.GetInt32
        Dim itemGUID As ULong = packet.GetUInt64
        Log.WriteLine(LogType.DEBUG, "[{0}:{1}] CMSG_PAGE_TEXT_QUERY [pageID={2}, itemGUID={3:X}]", Client.IP, Client.Port, pageID, itemGUID)

        Dim MySQLQuery As New DataTable
        Database.Query(String.Format("SELECT * FROM itempages WHERE entry = ""{0}"";", pageID), MySQLQuery)

        Dim response As New PacketClass(OPCODES.SMSG_PAGE_TEXT_QUERY_RESPONSE)
        response.AddInt32(pageID)
        If MySQLQuery.Rows.Count <> 0 Then response.AddString(MySQLQuery.Rows(0).Item("text")) Else response.AddString("Page " & pageID & " not found! Please report this to database devs.")
        If MySQLQuery.Rows.Count <> 0 Then response.AddInt32(MySQLQuery.Rows(0).Item("next_page")) Else response.AddInt32(0)
        Client.Send(response)
        response.Dispose()
    End Sub
    Public Sub On_CMSG_WRAP_ITEM(ByRef packet As PacketClass, ByRef Client As ClientClass)
        If (packet.Data.Length - 1) < 9 Then Exit Sub
        packet.GetInt16()
        Dim giftBag As Byte = packet.GetInt8
        Dim giftSlot As Byte = packet.GetInt8
        Dim itemBag As Byte = packet.GetInt8
        Dim itemSlot As Byte = packet.GetInt8
        If giftBag = 255 Then giftBag = 0
        If itemBag = 255 Then itemBag = 0

        Log.WriteLine(LogType.DEBUG, "[{0}:{1}] CMSG_WRAP_ITEM [{2}:{3} -> {4}{5}]", Client.IP, Client.Port, giftBag, giftSlot, itemBag, itemSlot)

        Dim Gift As ItemObject = Client.Character.ItemGET(giftBag, giftSlot)
        Dim Item As ItemObject = Client.Character.ItemGET(itemBag, itemSlot)

        If Gift Is Nothing Or Item Is Nothing Then
            SendInventoryChangeFailure(Client.Character, InventoryChangeFailure.EQUIP_ERR_ITEM_NOT_FOUND, 0, 0)
        End If

        'if(item==gift)                                          // not possable with pacjket from real client
        '{
        '    _player->SendEquipError( EQUIP_ERR_WRAPPED_CANT_BE_WRAPPED, item, NULL );
        '    return;
        '}

        'if(item->IsEquipped())
        '{
        '    _player->SendEquipError( EQUIP_ERR_EQUIPPED_CANT_BE_WRAPPED, item, NULL );
        '    return;
        '}

        'if(item->GetUInt64Value(ITEM_FIELD_GIFTCREATOR)) // HasFlag(ITEM_FIELD_FLAGS, 8);
        '{
        '    _player->SendEquipError( EQUIP_ERR_WRAPPED_CANT_BE_WRAPPED, item, NULL );
        '    return;
        '}

        'if(item->IsBag())
        '{
        '    _player->SendEquipError( EQUIP_ERR_BAGS_CANT_BE_WRAPPED, item, NULL );
        '    return;
        '}

        'if(item->IsSoulBound() || item->GetProto()->Class == ITEM_CLASS_QUEST)
        '{
        '    _player->SendEquipError( EQUIP_ERR_BOUND_CANT_BE_WRAPPED, item, NULL );
        '    return;
        '}

        'if(item->GetMaxStackCount() != 1)
        '{
        '    _player->SendEquipError( EQUIP_ERR_STACKABLE_CANT_BE_WRAPPED, item, NULL );
        '    return;
        '}

        '//if(item->IsUnique()) // need figure out unique item flags...
        '//{
        '//    _player->SendEquipError( EQUIP_ERR_UNIQUE_CANT_BE_WRAPPED, item, NULL );
        '//    return;
        '//}

        'sDatabase.BeginTransaction();
        'sDatabase.PExecute("INSERT INTO `character_gifts` VALUES ('%u', '%u', '%u', '%u')", GUID_LOPART(item->GetOwnerGUID()), item->GetGUIDLow(), item->GetEntry(), item->GetUInt32Value(ITEM_FIELD_FLAGS));
        'item->SetUInt32Value(OBJECT_FIELD_ENTRY, gift->GetUInt32Value(OBJECT_FIELD_ENTRY));
        'item->SetUInt64Value(ITEM_FIELD_GIFTCREATOR, _player->GetGUID());
        'item->SetUInt32Value(ITEM_FIELD_FLAGS, 8); // wrapped ?
        'item->SetState(ITEM_CHANGED, _player);

        'if(item->GetState()==ITEM_NEW)                          // save new item, to have alway for `character_gifts` record in `item_template`
        '    item->SaveToDB();
        'sDatabase.CommitTransaction();

        'uint32 count = 1;
        '_player->DestroyItemCount(gift, count, true);
    End Sub

    Public Sub On_CMSG_DESTROYITEM(ByRef packet As PacketClass, ByRef Client As ClientClass)
        If (packet.Data.Length - 1) < 8 Then Exit Sub
        Try
            packet.GetInt16()
            Dim srcBag As Byte = CType(packet.GetInt8, Byte)
            Dim srcSlot As Byte = packet.GetInt8
            Dim Count As Byte = packet.GetInt8
            If srcBag = 255 Then srcBag = 0
            Log.WriteLine(LogType.DEBUG, "[{0}:{1}] CMSG_DESTROYITEM [srcSlot={3}:{2}  count={4}]", Client.IP, Client.Port, srcSlot, srcBag, Count)

            If srcBag = 0 Then
                'DONE: Fire quest event to check for if this item is required for quest
                'NOTE: Not only quest items are needed for quests
                OnQuestItemRemove(Client.Character, Client.Character.Items(srcSlot).ItemEntry, Count)

                If Count = 0 Or Count >= Client.Character.Items(srcSlot).StackCount Then
                    If srcSlot < EQUIPMENT_SLOT_END Then Client.Character.UpdateRemoveItemStats(Client.Character.Items(srcSlot), srcSlot)
                    Client.Character.ItemREMOVE(srcBag, srcSlot, True, True)
                Else
                    Client.Character.Items(srcSlot).StackCount -= Count
                    Client.Character.SendItemUpdate(Client.Character.Items(srcSlot))
                    Client.Character.Items(srcSlot).Save()
                End If

            Else
                'DONE: Fire quest event to check for if this item is required for quest
                'NOTE: Not only quest items are needed for quests
                OnQuestItemRemove(Client.Character, Client.Character.Items(srcSlot).ItemEntry, Count)

                If Count = 0 Or Count >= Client.Character.Items(srcBag).Items(srcSlot).StackCount Then
                    Client.Character.ItemREMOVE(srcBag, srcSlot, True, True)
                Else
                    Client.Character.Items(srcBag).Items(srcSlot).StackCount -= Count
                    Client.Character.SendItemUpdate(Client.Character.Items(srcBag).Items(srcSlot))
                    Client.Character.Items(srcBag).Items(srcSlot).Save()
                End If
            End If

        Catch e As Exception
            Log.WriteLine(LogType.DEBUG, "Error destroying item.{0}", vbNewLine & e.ToString)
        End Try
    End Sub
    Public Sub SetVirtualItemInfo(ByVal c As CharacterObject, ByVal Slot As Byte, ByRef Item As ItemObject)
        If Item Is Nothing Then
            c.SetUpdateFlag(EUnitFields.UNIT_VIRTUAL_ITEM_INFO + Slot * 2, 0)
            c.SetUpdateFlag(EUnitFields.UNIT_VIRTUAL_ITEM_INFO_2 + Slot * 2, 0)
            c.SetUpdateFlag(EUnitFields.UNIT_VIRTUAL_ITEM_SLOT_DISPLAY + Slot, 0)
        Else
            c.SetUpdateFlag(EUnitFields.UNIT_VIRTUAL_ITEM_INFO + Slot * 2, CType(Item.GUID << 32 >> 32, Integer))
            c.SetUpdateFlag(EUnitFields.UNIT_VIRTUAL_ITEM_INFO_2 + Slot * 2, Item.ItemInfo.Sheath)
            c.SetUpdateFlag(EUnitFields.UNIT_VIRTUAL_ITEM_SLOT_DISPLAY + Slot, Item.ItemInfo.Model)
        End If
    End Sub
    Public Sub On_CMSG_SETSHEATHED(ByRef packet As PacketClass, ByRef Client As ClientClass)
        If (packet.Data.Length - 1) < 9 Then Exit Sub
        packet.GetInt16()
        Dim sheathed As SHEATHE_SLOT = packet.GetInt32
        Log.WriteLine(LogType.DEBUG, "[{0}:{1}] CMSG_SETSHEATHED [{2}]", Client.IP, Client.Port, sheathed)

        Client.Character.attackSheathState = sheathed
        Client.Character.combatCanDualWield = False

        Client.Character.cBytes2 = ((Client.Character.cBytes2 And (Not &HFF)) Or sheathed)
        Client.Character.SetUpdateFlag(EUnitFields.UNIT_FIELD_BYTES_2, Client.Character.cBytes2)

        Select Case sheathed
            Case SHEATHE_SLOT.SHEATHE_NONE
                SetVirtualItemInfo(Client.Character, 0, Nothing)
                SetVirtualItemInfo(Client.Character, 1, Nothing)
                SetVirtualItemInfo(Client.Character, 2, Nothing)

            Case SHEATHE_SLOT.SHEATHE_WEAPON
                If Client.Character.Items.ContainsKey(EQUIPMENT_SLOT_MAINHAND) AndAlso (Not Client.Character.Items(EQUIPMENT_SLOT_MAINHAND).IsBroken) Then
                    SetVirtualItemInfo(Client.Character, 0, Client.Character.Items(EQUIPMENT_SLOT_MAINHAND))
                Else
                    SetVirtualItemInfo(Client.Character, 0, Nothing)
                    Client.Character.attackSheathState = SHEATHE_SLOT.SHEATHE_NONE
                End If
                If Client.Character.Items.ContainsKey(EQUIPMENT_SLOT_OFFHAND) AndAlso (Not Client.Character.Items(EQUIPMENT_SLOT_OFFHAND).IsBroken) Then
                    SetVirtualItemInfo(Client.Character, 1, Client.Character.Items(EQUIPMENT_SLOT_OFFHAND))
                    'DONE: Must be applyed SPELL_EFFECT_DUAL_WIELD and weapon in offhand
                    If Client.Character.spellCanDualWeild AndAlso CType(Client.Character.Items(EQUIPMENT_SLOT_OFFHAND), ItemObject).ItemInfo.ObjectClass = ITEM_CLASS.ITEM_CLASS_WEAPON Then Client.Character.combatCanDualWield = True
                Else
                    SetVirtualItemInfo(Client.Character, 1, Nothing)
                End If
                SetVirtualItemInfo(Client.Character, 2, Nothing)

            Case SHEATHE_SLOT.SHEATHE_RANGED
                SetVirtualItemInfo(Client.Character, 0, Nothing)
                SetVirtualItemInfo(Client.Character, 1, Nothing)
                If Client.Character.Items.ContainsKey(EQUIPMENT_SLOT_RANGED) AndAlso (Not Client.Character.Items(EQUIPMENT_SLOT_RANGED).IsBroken) Then
                    SetVirtualItemInfo(Client.Character, 2, Client.Character.Items(EQUIPMENT_SLOT_RANGED))
                Else
                    SetVirtualItemInfo(Client.Character, 2, Nothing)
                    Client.Character.attackSheathState = SHEATHE_SLOT.SHEATHE_NONE
                End If

            Case Else
                Log.WriteLine(LogType.WARNING, "Unhandled sheathe state [{0}]", sheathed)
        End Select

        Client.Character.SendCharacterUpdate(True)
    End Sub

    Public Sub On_CMSG_USE_ITEM(ByRef packet As PacketClass, ByRef Client As ClientClass)
        Try
            If (packet.Data.Length - 1) < 17 Then Exit Sub
            packet.GetInt16()
            Dim bag As Byte = packet.GetInt8
            If bag = 255 Then bag = 0
            Dim slot As Byte = packet.GetInt8
            Dim spell As Byte = packet.GetInt8
            Dim cn As Byte = packet.GetInt8
            Dim ItemGUID As ULong = packet.GetUInt64
            Log.WriteLine(LogType.DEBUG, "[{0}:{1}] CMSG_USE_ITEM [guid={5} bag={2} slot={3} spell={4}]", Client.IP, Client.Port, bag, slot, spell, ItemGUID)
            If (Client.Character.cUnitFlags And UnitFlags.UNIT_FLAG_TAXI_FLIGHT) Then Exit Sub

            Dim tmpGUID As ULong = Client.Character.ItemGetGUID(bag, slot)
            If WORLD_ITEMs.ContainsKey(ItemGUID) = False Then
                SendInventoryChangeFailure(Client.Character, InventoryChangeFailure.EQUIP_ERR_ITEM_NOT_FOUND, 0, 0)
                Exit Sub
            End If
            Dim itemInfo As ItemInfo = ITEMDatabase(WORLD_ITEMs(ItemGUID).ItemEntry)

            'DONE: Check if the character isn't trying to use item that isn't his own
            If tmpGUID <> ItemGUID Then
                SendInventoryChangeFailure(Client.Character, InventoryChangeFailure.EQUIP_ERR_ITEM_NOT_FOUND, 0, 0)
                Exit Sub
            End If

            'DONE: Check if the item can be used in combat
            Dim InstantCast As Boolean = False

            For i As Byte = 0 To 4
                If SPELLs.ContainsKey(itemInfo.Spells(i).SpellID) Then
                    If ((Client.Character.cUnitFlags And UnitFlags.UNIT_FLAG_IN_COMBAT) = UnitFlags.UNIT_FLAG_IN_COMBAT) Then
                        If (CType(SPELLs(itemInfo.Spells(i).SpellID), SpellInfo).Attributes And SpellAttributes.SPELL_CANT_USED_IN_COMBAT) Then
                            SendInventoryChangeFailure(Client.Character, InventoryChangeFailure.EQUIP_ERR_NOT_IN_COMBAT, ItemGUID, 0)
                            Exit Sub
                        End If
                    End If
                    For j As Byte = 0 To 2
                        If SPELLs(itemInfo.Spells(i).SpellID).SpellEffects(j) IsNot Nothing AndAlso SPELLs(itemInfo.Spells(i).SpellID).SpellEffects(j).ID = SpellEffects_Names.SPELL_EFFECT_SUMMON Then
                            If SPELLs(itemInfo.Spells(i).SpellID).SpellEffects(j).MiscValueB = SummonType.SUMMON_TYPE_CRITTER OrElse SPELLs(itemInfo.Spells(i).SpellID).SpellEffects(j).MiscValueB = SummonType.SUMMON_TYPE_CRITTER2 Then
                                If Client.Character.OldCritter IsNot Nothing Then InstantCast = True
                            End If
                        End If
                    Next
                End If
            Next

            If Client.Character.DEAD = True Then
                SendInventoryChangeFailure(Client.Character, InventoryChangeFailure.EQUIP_ERR_YOU_ARE_DEAD, ItemGUID, 0)
                Exit Sub
            End If

            If itemInfo.ObjectClass = ITEM_CLASS.ITEM_CLASS_CONSUMABLE Then
                'DONE: Consume the item
                CType(WORLD_ITEMs(ItemGUID), ItemObject).StackCount -= 1
                If CType(WORLD_ITEMs(ItemGUID), ItemObject).StackCount = 0 Then
                    Client.Character.ItemREMOVE(bag, slot, True, True)
                Else
                    Client.Character.SendItemUpdate(WORLD_ITEMs(ItemGUID))
                End If
            Else
                'DONE: Bind item to player
                If CType(WORLD_ITEMs(ItemGUID), ItemObject).ItemInfo.Bonding = ITEM_BONDING_TYPE.BIND_WHEN_USED AndAlso CType(WORLD_ITEMs(ItemGUID), ItemObject).IsSoulBound = False Then CType(WORLD_ITEMs(ItemGUID), ItemObject).SoulbindItem()
            End If

            For i As Byte = 0 To 4
                If itemInfo.Spells(i).SpellID > 0 AndAlso (itemInfo.Spells(i).SpellTrigger = ITEM_SPELLTRIGGER_TYPE.USE OrElse itemInfo.Spells(i).SpellTrigger = ITEM_SPELLTRIGGER_TYPE.NO_DELAY_USE) Then
                    If SPELLs.ContainsKey(itemInfo.Spells(i).SpellID) Then
                        'DONE: Read spell targets
                        Dim Targets As New SpellTargets
                        Targets.ReadTargets(packet, CType(Client.Character, CharacterObject))
                        Dim tmpSpell As New CastSpellParameters
                        tmpSpell.tmpTargets = Targets
                        tmpSpell.tmpCaster = Client.Character
                        tmpSpell.tmpSpellID = itemInfo.Spells(i).SpellID
                        tmpSpell.tmpCN = cn
                        tmpSpell.tmpInstant = InstantCast
                        If WORLD_ITEMs.ContainsKey(ItemGUID) Then tmpSpell.tmpItem = WORLD_ITEMs(ItemGUID)

                        Dim castResult As Byte = SpellFailedReason.CAST_NO_ERROR
                        Try
                            castResult = CType(SPELLs(itemInfo.Spells(i).SpellID), SpellInfo).CanCast(Client.Character, tmpSpell.tmpTargets)

                            'Only instant cast send ERR_OK for cast result?
                            If castResult = SpellFailedReason.CAST_NO_ERROR Then

                                'DONE: Enqueue spell casting function
                                ThreadPool.QueueUserWorkItem(New WaitCallback(AddressOf tmpSpell.Cast))
                            Else
                                SendCastResult(castResult, Client, itemInfo.Spells(i).SpellID, cn)
                            End If

                        Catch e As Exception
                            Log.WriteLine(LogType.DEBUG, "Error casting spell {0}.{1}", itemInfo.Spells(i).SpellID, vbNewLine & e.ToString)
                            SendCastResult(castResult, Client, itemInfo.Spells(i).SpellID, cn)
                        End Try
                    End If
                End If
            Next

        Catch ex As Exception
            Log.WriteLine(LogType.CRITICAL, "Error while using a item.{0}", vbNewLine & ex.ToString)
        End Try
    End Sub
    Public Sub On_CMSG_OPEN_ITEM(ByRef packet As PacketClass, ByRef Client As ClientClass)
        If (packet.Data.Length - 1) < 7 Then Exit Sub
        packet.GetInt16()
        Dim bag As Byte = packet.GetInt8
        If bag = 255 Then bag = 0
        Dim slot As Byte = packet.GetInt8

        Log.WriteLine(LogType.DEBUG, "[{0}:{1}] CMSG_OPEN_ITEM [bag={2} slot={3}]", Client.IP, Client.Port, bag, slot)

        Dim itemGUID As ULong = 0
        If bag = 0 Then
            itemGUID = Client.Character.Items(slot).GUID
        Else
            itemGUID = Client.Character.Items(bag).Items(slot).GUID
        End If

        If itemGUID <> 0 Then
            If GenerateLoot(Client.Character, itemGUID, WS_Loot.LootType.LOOTTYPE_CORPSE) Then
                CType(LootTable(itemGUID), LootObject).SendLoot(Client)
                Exit Sub
            End If
        End If

        SendEmptyLoot(itemGUID, WS_Loot.LootType.LOOTTYPE_CORPSE, Client)
    End Sub

    Public Sub SendInventoryChangeFailure(ByRef c As CharacterObject, ByVal ErrorCode As InventoryChangeFailure, ByVal GUID1 As ULong, ByVal GUID2 As ULong)
        Dim packet As New PacketClass(OPCODES.SMSG_INVENTORY_CHANGE_FAILURE)
        packet.AddInt8(ErrorCode)

        If ErrorCode = InventoryChangeFailure.EQUIP_ERR_CANT_EQUIP_LEVEL_I Then
            packet.AddInt32(CType(WORLD_ITEMs(GUID1), ItemObject).ItemInfo.ReqLevel)
        End If

        packet.AddUInt64(GUID1)
        packet.AddUInt64(GUID2)
        packet.AddInt8(0)
        c.Client.Send(packet)
        packet.Dispose()
    End Sub
    Public Sub SendEnchantmentLog(ByRef c As CharacterObject, ByVal iGUID As ULong, ByVal iEntry As Integer, ByVal iSpellID As Integer)
        Dim packet As New PacketClass(OPCODES.SMSG_ENCHANTMENTLOG)
        packet.AddUInt64(iGUID)
        packet.AddUInt64(c.GUID)
        packet.AddInt32(iEntry)
        packet.AddInt32(iSpellID)
        packet.AddInt8(0)
        c.Client.Send(packet)
        packet.Dispose()
    End Sub
    Public Sub SendEnchantmentTimeUpdate(ByRef c As CharacterObject, ByVal iGUID As ULong, ByVal iSlot As Integer, ByVal iTime As Integer)
        Dim packet As New PacketClass(OPCODES.SMSG_ITEM_ENCHANT_TIME_UPDATE)
        packet.AddUInt64(iGUID)
        packet.AddInt32(iSlot)
        packet.AddInt32(iTime)
        c.Client.Send(packet)
        packet.Dispose()
    End Sub

#End Region

End Module