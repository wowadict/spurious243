﻿Imports System.ComponentModel
Imports System.Collections.Generic

Public Enum ExpansionLevel As Byte
    NORMAL = 0          'WoW
    EXPANSION_1 = 1     'WoW: Burning Crusade
End Enum
Public Enum AccessLevel As Byte
    Trial = 0
    Player = 1
    PlayerVip = 2
    GameMaster = 3
    Admin = 4
    Developer = 5
End Enum

Public Interface ICluster

    <Description("Signal realm server for new world server.")> _
    Function Connect(ByVal URI As String, ByVal Maps As ICollection) As Boolean
    <Description("Signal realm server for disconected world server.")> _
    Sub Disconnect(ByVal URI As String, ByVal Maps As ICollection)

    <Description("Signal realm server for new voice server.")> _
    Sub VoiceConnect(ByVal URI As String, ByVal Host As UInteger, ByVal Port As UShort, ByVal Key() As Byte)
    <Description("Signal realm server for disconected voice server.")> _
    Sub VoiceDisconnect()

    <Description("Send data packet to client.")> _
    Sub ClientSend(ByVal ID As UInteger, ByVal Data As Byte())
    <Description("Notify client drop.")> _
    Sub ClientDrop(ByVal ID As UInteger)
    <Description("Notify client transfer.")> _
    Sub ClientTransfer(ByVal ID As UInteger, ByVal posX As Single, ByVal posY As Single, ByVal posZ As Single, ByVal ori As Single, ByVal map As UInteger)
    <Description("Notify client update.")> _
    Sub ClientUpdate(ByVal ID As UInteger, ByVal Zone As UInteger, ByVal Level As Byte)
    <Description("Set client chat flag.")> _
    Sub ClientSetChatFlag(ByVal ID As UInteger, ByVal Flag As Byte)

    Function BattlegroundList(ByVal Type As Byte) As List(Of Integer)

    <Description("Send data packet to all clients online.")> _
    Sub Broadcast(ByVal Data() As Byte)
    <Description("Send data packet to all clients in specified client's group.")> _
    Sub BroadcastGroup(ByVal GroupID As Long, ByVal Data() As Byte)
    <Description("Send data packet to all clients in specified client's raid.")> _
    Sub BroadcastRaid(ByVal GroupID As Long, ByVal Data() As Byte)
    <Description("Send data packet to all clients in specified client's guild.")> _
    Sub BroadcastGuild(ByVal GuildID As Long, ByVal Data() As Byte)
    <Description("Send data packet to all clients in specified client's guild officers.")> _
    Sub BroadcastGuildOfficers(ByVal GuildID As Long, ByVal Data() As Byte)

    <Description("Send update for the requested group.")> _
    Sub GroupRequestUpdate(ByVal ID As UInteger)

End Interface
Public Interface IWorld

    <Description("Initialize client object.")> _
    Sub ClientConnect(ByVal ID As UInteger, ByVal Client As ClientInfo)
    <Description("Destroy client object.")> _
    Sub ClientDisconnect(ByVal ID As UInteger)
    <Description("Assing particular client to this world server (Use client ID).")> _
    Sub ClientLogin(ByVal ID As UInteger, ByVal GUID As ULong)
    <Description("Remove particular client from this world server (Use client ID).")> _
    Sub ClientLogout(ByVal ID As UInteger)
    <Description("Transfer packet from Realm to World using client's ID.")> _
    Sub ClientPacket(ByVal ID As UInteger, ByVal Data() As Byte)

    <Description("Create CharacterObject.")> _
    Function ClientCreateCharacter(ByVal Account As String, ByVal Name As String, ByVal Race As Byte, ByVal Classe As Byte, ByVal Gender As Byte, ByVal Skin As Byte, _
                                   ByVal Face As Byte, ByVal HairStyle As Byte, ByVal HairColor As Byte, ByVal FacialHair As Byte, ByVal OutfitID As Byte) As Integer

    <Description("Respond to world server if still alive.")> _
    Function Ping(ByVal Timestamp As Integer) As Integer

    <Description("Tell the cluster about your CPU & Memory Usage")> _
    Sub ServerInfo(ByRef CPUUsage As Single, ByRef MemoryUsage As ULong)

    <Description("Make world create specific map.")> _
    Sub InstanceCreate(ByVal Map As UInteger)
    <Description("Make world destroy specific map.")> _
    Sub InstanceDestroy(ByVal Map As UInteger)
    <Description("Check world configuration.")> _
    Function InstanceCanCreate(ByVal Type As Integer) As Boolean

    <Description("Set client's group.")> _
    Sub ClientSetGroup(ByVal ID As UInteger, ByVal GroupID As Long)
    <Description("Update group information.")> _
    Sub GroupUpdate(ByVal GroupID As Long, ByVal GroupType As Byte, ByVal GroupLeader As ULong, ByVal Members() As ULong)
    <Description("Update group information about looting.")> _
    Sub GroupUpdateLoot(ByVal GroupID As Long, ByVal Difficulty As Byte, ByVal Method As Byte, ByVal Threshold As Byte, ByVal Master As ULong)

    <Description("Request party member stats.")> _
    Function GroupMemberStats(ByVal GUID As ULong, ByVal Flag As Integer) As Byte()

End Interface
Public Interface IVoice

    <Description("Create new channel, returns the channel ID")> _
    Function ChannelCreate(ByVal Type As Byte, ByVal Name As String) As UShort
    <Description("Destroy channel by ID.")> _
    Sub ChannelDestroy(ByVal ChannelID As UShort)

    <Description("Add a client to slot on server, returns slot ID")> _
    Function ClientConnect(ByVal ChannelID As UShort) As Byte
    <Description("Remove client from slot on server")> _
    Sub ClientDisconnect(ByVal ChannelID As UShort, ByVal Slot As Byte)

    <Description("Respond to world server if still alive.")> _
    Function Ping(ByVal Timestamp As Integer) As Integer

End Interface

<Serializable()> _
Public Class ClientInfo
    Public Index As UInteger
    Public IP As Net.IPAddress
    Public Port As UInteger
    Public Account As String
    Public Access As AccessLevel = AccessLevel.Player
    Public Expansion As ExpansionLevel = ExpansionLevel.NORMAL
End Class