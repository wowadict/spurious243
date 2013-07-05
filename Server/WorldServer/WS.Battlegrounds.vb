﻿' 
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

Public Module WS_Battlegrounds

#Region "WS.Battlegrounds.Constants"
    Public Enum ArenaTeamCommandTypes As Byte
        ERR_ARENA_TEAM_CREATE_S = &H0
        ERR_ARENA_TEAM_INVITE_SS = &H1
        ERR_ARENA_TEAM_QUIT_S = &H3
        ERR_ARENA_TEAM_FOUNDER_S = &HC
    End Enum

    Public Enum ArenaTeamCommandErrors As Byte
        ERR_ARENA_TEAM_INTERNAL = &H1
        ERR_ALREADY_IN_ARENA_TEAM = &H2
        ERR_ALREADY_IN_ARENA_TEAM_S = &H3
        ERR_INVITED_TO_ARENA_TEAM = &H4
        ERR_ALREADY_INVITED_TO_ARENA_TEAM_S = &H5
        ERR_ARENA_TEAM_NAME_INVALID = &H6
        ERR_ARENA_TEAM_NAME_EXISTS_S = &H7
        ERR_ARENA_TEAM_LEADER_LEAVE_S = &H8
        ERR_ARENA_TEAM_PERMISSIONS = &H8
        ERR_ARENA_TEAM_PLAYER_NOT_IN_TEAM = &H9
        ERR_ARENA_TEAM_PLAYER_NOT_IN_TEAM_SS = &HA
        ERR_ARENA_TEAM_PLAYER_NOT_FOUND_S = &HB
        ERR_ARENA_TEAM_NOT_ALLIED = &HC
    End Enum
#End Region

#Region "WS.Battlegrounds.Handlers"
    Public Sub On_CMSG_ARENA_TEAM_QUERY(ByRef packet As PacketClass, ByRef Client As ClientClass)
        If (packet.Data.Length - 1) < 9 Then Exit Sub
        packet.GetInt16()
        Dim ArenaTeamID As UInteger = packet.GetUInt32

        SendArenaQueryResponse(Client, ArenaTeamID)
        SendArenaStats(Client, ArenaTeamID)
    End Sub

    Public Sub On_CMSG_ARENA_TEAM_ROSTER(ByRef packet As PacketClass, ByRef Client As ClientClass)
        If (packet.Data.Length - 1) < 9 Then Exit Sub
        packet.GetInt16()
        Dim ArenaTeamID As UInteger = packet.GetUInt32

        SendArenaRoster(Client, ArenaTeamID)
    End Sub

    Public Sub On_MSG_INSPECT_ARENA_TEAMS(ByRef packet As PacketClass, ByRef Client As ClientClass)
        If (packet.Data.Length - 1) < 13 Then Exit Sub
        packet.GetInt16()
        Dim GUID As ULong = packet.GetUInt64
        If CHARACTERs.ContainsKey(GUID) = False Then Exit Sub

        'TODO: Read it from the DB
        For i As Byte = 0 To 2
            If CHARACTERs(GUID).ArenaTeamID(i) <> 0 Then
                Dim q As New DataTable
                Database.Query(String.Format("SELECT * FROM arena_teams WHERE arena_id = {0}", CHARACTERs(GUID).ArenaTeamID(i)), q)
                If q.Rows.Count = 0 Then Exit Sub

                Dim response As New PacketClass(OPCODES.MSG_INSPECT_ARENA_TEAMS)
                response.AddUInt64(GUID)
                response.AddInt8(i)
                response.AddUInt32(CHARACTERs(GUID).ArenaTeamID(i))
                response.AddUInt32(q.Rows(0).Item("arena_rating")) 'Rating
                response.AddUInt32(q.Rows(0).Item("arena_weekgames")) 'Games played this week
                response.AddUInt32(q.Rows(0).Item("arena_weekwins")) 'Wins
                response.AddUInt32(q.Rows(0).Item("arena_seasongames")) 'Games this season
                response.AddUInt32(0) 'Personal rating?
                Client.Send(response)
                response.Dispose()
            End If
        Next i
    End Sub

    Public Sub On_CMSG_BATTLEMASTER_HELLO(ByRef packet As PacketClass, ByRef Client As ClientClass)
        If (packet.Data.Length - 1) < 13 Then Exit Sub
        packet.GetInt16()
        Dim GUID As ULong = packet.GetUInt64
        If WORLD_CREATUREs.ContainsKey(GUID) = False Then Exit Sub
        If (WORLD_CREATUREs(GUID).CreatureInfo.cNpcFlags And NPCFlags.UNIT_NPC_FLAG_BATTLEFIELDPERSON) = 0 Then Exit Sub
        If Battlemasters.ContainsKey(WORLD_CREATUREs(GUID).ID) = False Then Exit Sub

        Dim BGType As Byte = Battlemasters(WORLD_CREATUREs(GUID).ID)
        If Battlegrounds.ContainsKey(BGType) = False Then Exit Sub

        If Battlegrounds(BGType).MinLevel > Client.Character.Level OrElse Battlegrounds(BGType).MaxLevel < Client.Character.Level Then
            SendMessageNotification(Client, "You don't meet Battleground level requirements")
            Exit Sub
        End If

        'DONE: Send list
        Dim response As New PacketClass(OPCODES.SMSG_BATTLEFIELD_LIST)
        response.AddUInt64(GUID)
        response.AddInt32(BGType)

        If BGType = 6 Then 'Arenas
            response.AddInt8(5) 'Unk
            response.AddInt32(0) 'Unk
        Else
            Dim Battlegrounds As List(Of Integer) = WS.GetBattlegroundInstances(BGType)
            response.AddInt8(0) 'Unk
            response.AddInt32(Battlegrounds.Count) 'Number of BG Instances

            For Each Instance As Integer In Battlegrounds
                response.AddInt32(Instance)
            Next
        End If

        Client.Send(response)
        response.Dispose()
    End Sub
#End Region

#Region "WS.Battlegrounds.Functions"
    Public Sub SendArenaCommandResult(ByRef Client As ClientClass, ByVal ErrorType As ArenaTeamCommandTypes, ByVal String1 As String, ByVal String2 As String, ByVal ErrorCode As ArenaTeamCommandErrors)
        Dim packet As New PacketClass(OPCODES.SMSG_ARENA_TEAM_COMMAND_RESULT)
        packet.AddInt32(ErrorType)
        packet.AddString(String1)
        packet.AddString(String1)
        packet.AddInt32(ErrorCode)
        Client.Send(packet)
        packet.Dispose()
    End Sub

    Public Sub SendArenaRoster(ByRef Client As ClientClass, ByVal ArenaTeamID As UInteger)
        Dim q As New DataTable
        Database.Query(String.Format("SELECT * FROM arena_members WHERE member_team = {0}", ArenaTeamID), q)
        If q.Rows.Count = 0 Then Exit Sub
        Dim q2 As New DataTable
        Database.Query(String.Format("SELECT * FROM arena_teams WHERE arena_id = {0}", ArenaTeamID), q2)
        If q2.Rows.Count = 0 Then Exit Sub

        Dim response As New PacketClass(OPCODES.SMSG_ARENA_TEAM_ROSTER)
        response.AddUInt32(ArenaTeamID)
        response.AddUInt32(q.Rows.Count)
        response.AddUInt32(q2.Rows(0).Item("arena_type"))

        For i As Byte = 0 To q.Rows.Count - 1
            If CHARACTERs.ContainsKey(CULng(q.Rows(i).Item("member_id"))) Then
                response.AddUInt64(q.Rows(i).Item("member_id"))
                response.AddInt8(1) 'Online
                response.AddString(CHARACTERs(CULng(q.Rows(i).Item("member_id"))).Name)
                If CULng(q2.Rows(0).Item("arena_captain")) = CULng(q.Rows(i).Item("member_id")) Then
                    response.AddInt32(1) 'Is leader
                Else
                    response.AddInt32(0) 'Normal member
                End If
                response.AddInt8(CHARACTERs(CULng(q.Rows(i).Item("member_id"))).Level) 'Level
                response.AddInt8(CHARACTERs(CULng(q.Rows(i).Item("member_id"))).Classe) 'Class
                response.AddUInt32(q.Rows(i).Item("member_playedweek")) 'Played this week
                response.AddUInt32(q.Rows(i).Item("member_wonsweek")) 'Wins this week
                response.AddUInt32(q.Rows(i).Item("member_playedseason")) 'Played season
                response.AddUInt32(q.Rows(i).Item("member_wonsseason")) 'Wins season
                response.AddUInt32(q.Rows(i).Item("member_personalrating")) 'Personal rating?
            Else
                Dim q3 As New DataTable
                Database.Query(String.Format("SELECT char_name, char_level, char_class FROM characters WHERE char_guid = {0}", q.Rows(i).Item("member_id")), q3)
                If q3.Rows.Count = 0 Then GoTo NextMember

                response.AddUInt64(q.Rows(i).Item("member_id"))
                response.AddInt8(0) 'Offline
                response.AddString(q3.Rows(0).Item("char_name"))
                If CULng(q2.Rows(0).Item("arena_captain")) = CULng(q.Rows(i).Item("member_id")) Then
                    response.AddInt32(1) 'Is leader
                Else
                    response.AddInt32(0) 'Normal member
                End If
                response.AddInt8(q3.Rows(0).Item("char_level")) 'Level
                response.AddInt8(q3.Rows(0).Item("char_class")) 'Class
                response.AddUInt32(q.Rows(i).Item("member_playedweek")) 'Played this week
                response.AddUInt32(q.Rows(i).Item("member_wonsweek")) 'Wins this week
                response.AddUInt32(q.Rows(i).Item("member_playedseason")) 'Played season
                response.AddUInt32(q.Rows(i).Item("member_wonsseason")) 'Wins season
                response.AddUInt32(q.Rows(i).Item("member_personalrating")) 'Personal rating?
            End If
NextMember:
        Next

        Client.Send(response)
        response.Dispose()
    End Sub

    Public Sub SendArenaQueryResponse(ByRef Client As ClientClass, ByVal ArenaTeamID As UInteger)
        Dim q As New DataTable
        Database.Query(String.Format("SELECT * FROM arena_teams WHERE arena_id = {0}", ArenaTeamID), q)
        If q.Rows.Count = 0 Then Exit Sub

        Dim response As New PacketClass(OPCODES.SMSG_ARENA_TEAM_QUERY_RESPONSE)
        response.AddUInt32(ArenaTeamID)
        response.AddString(q.Rows(0).Item("arena_name"))
        response.AddUInt32(q.Rows(0).Item("arena_type"))
        response.AddUInt32(q.Rows(0).Item("arena_emblemstyle"))
        response.AddUInt32(q.Rows(0).Item("arena_emblemcolor"))
        response.AddUInt32(q.Rows(0).Item("arena_borderstyle"))
        response.AddUInt32(q.Rows(0).Item("arena_bordercolor"))
        response.AddUInt32(q.Rows(0).Item("arena_background"))
        Client.Send(response)
        response.Dispose()
    End Sub

    Public Sub SendArenaStats(ByRef Client As ClientClass, ByVal ArenaTeamID As UInteger)
        Dim q As New DataTable
        Database.Query(String.Format("SELECT * FROM arena_teams WHERE arena_id = {0}", ArenaTeamID), q)
        If q.Rows.Count = 0 Then Exit Sub

        'DONE: Read it from the DB
        Dim response As New PacketClass(OPCODES.SMSG_ARENA_TEAM_STATS)
        response.AddUInt32(ArenaTeamID)
        response.AddUInt32(q.Rows(0).Item("arena_rating")) 'Rating
        response.AddUInt32(q.Rows(0).Item("arena_weekgames")) 'Games this week?
        response.AddUInt32(q.Rows(0).Item("arena_weekwins")) 'Week wins?
        response.AddUInt32(q.Rows(0).Item("arena_seasongames")) 'Games this season?
        response.AddUInt32(q.Rows(0).Item("arena_seasonwins")) 'Season wins?
        response.AddUInt32(q.Rows(0).Item("arena_rank")) 'Rank
        Client.Send(response)
        response.Dispose()
    End Sub
#End Region

End Module
