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
'


Imports System.Threading
Imports System.Net.Sockets
Imports System.Xml.Serialization
Imports System.IO
Imports System.Net
Imports System.Reflection
Imports System.Runtime.CompilerServices
Imports mangosVB.Common.BaseWriter
Imports mangosVB.Common


Public Module WC_Handlers_Social


#Region "Framework"


    Public Sub LoadIgnoreList(ByRef c As CharacterObject)
        'DONE: Query DB
        Dim q As New DataTable
        Database.Query(String.Format("SELECT * FROM characters_social WHERE char_guid = {0} AND flags = {1};", c.GUID, CType(SocialFlag.SOCIAL_FLAG_IGNORED, Byte)), q)

        'DONE: Add to list
        For Each r As DataRow In q.Rows
            c.IgnoreList.Add(CType(r.Item("guid"), ULong))
        Next
    End Sub

    Public Enum FriendStatus As Byte
        FRIEND_STATUS_OFFLINE = 0
        FRIEND_STATUS_ONLINE = 1
        FRIEND_STATUS_AFK = 2
        FRIEND_STATUS_UNK3 = 3
        FRIEND_STATUS_DND = 4
    End Enum
    Public Enum FriendResult As Byte
        FRIEND_DB_ERROR = &H0
        FRIEND_LIST_FULL = &H1
        FRIEND_ONLINE = &H2
        FRIEND_OFFLINE = &H3
        FRIEND_NOT_FOUND = &H4
        FRIEND_REMOVED = &H5
        FRIEND_ADDED_ONLINE = &H6
        FRIEND_ADDED_OFFLINE = &H7
        FRIEND_ALREADY = &H8
        FRIEND_SELF = &H9
        FRIEND_ENEMY = &HA
        FRIEND_IGNORE_FULL = &HB
        FRIEND_IGNORE_SELF = &HC
        FRIEND_IGNORE_NOT_FOUND = &HD
        FRIEND_IGNORE_ALREADY = &HE
        FRIEND_IGNORE_ADDED = &HF
        FRIEND_IGNORE_REMOVED = &H10
    End Enum
    Public Enum SocialFlag As Byte
        SOCIAL_FLAG_FRIEND = &H1
        SOCIAL_FLAG_IGNORED = &H2
        SOCIAL_FLAG_MUTED = &H4
    End Enum
    Public Sub SendContactList(ByRef Client As ClientClass, ByRef Character As CharacterObject)
        'DONE: Query DB
        Dim q As New DataTable
        Database.Query(String.Format("SELECT * FROM characters_social WHERE char_guid = {0};", Character.GUID), q)


        'DONE: Make the packet
        Dim SMSG_FRIEND_LIST As New PacketClass(OPCODES.SMSG_CONTACT_LIST)
        SMSG_FRIEND_LIST.AddInt32(&H7)                              'Flag (0x1 = List, 0x2 = FriendList, 0x4 = IgnoreList)
        If q.Rows.Count > 0 Then
            SMSG_FRIEND_LIST.AddInt32(q.Rows.Count)

            For Each r As DataRow In q.Rows
                Dim GUID As ULong = r.Item("guid")
                SMSG_FRIEND_LIST.AddUInt64(GUID)                    'Player GUID
                SMSG_FRIEND_LIST.AddInt32(r.Item("flags"))          'Player Flag (0x1-friend?, 0x2-ignored?, 0x4-muted?)
                SMSG_FRIEND_LIST.AddString(r.Item("note"))          'Player Note
                If CInt(r.Item("flags")) = SocialFlag.SOCIAL_FLAG_FRIEND Then
                    If CHARACTERs.ContainsKey(GUID) AndAlso CHARACTERs(GUID).IsInWorld Then
                        'If CType(CHARACTERs(guid), CharacterObject).DND Then
                        '    SMSG_FRIEND_LIST.AddInt8(FriendStatus.FRIEND_STATUS_DND)
                        'ElseIf CType(CHARACTERs(guid), CharacterObject).AFK Then
                        '    SMSG_FRIEND_LIST.AddInt8(FriendStatus.FRIEND_STATUS_AFK)
                        'Else
                        SMSG_FRIEND_LIST.AddInt8(FriendStatus.FRIEND_STATUS_ONLINE)
                        'End If
                        SMSG_FRIEND_LIST.AddInt32(CType(CHARACTERs(GUID), CharacterObject).Zone)    'Area
                        SMSG_FRIEND_LIST.AddInt32(CType(CHARACTERs(GUID), CharacterObject).Level)   'Level
                        SMSG_FRIEND_LIST.AddInt32(CType(CHARACTERs(GUID), CharacterObject).Classe)  'Class
                    Else
                        SMSG_FRIEND_LIST.AddInt8(FriendStatus.FRIEND_STATUS_OFFLINE)
                    End If
                End If
            Next
        Else
            SMSG_FRIEND_LIST.AddInt32(0)
        End If

        Client.Send(SMSG_FRIEND_LIST)
        SMSG_FRIEND_LIST.Dispose()

        Log.WriteLine(LogType.DEBUG, "[{0}:{1}] SMSG_CONTACT_LIST", Client.IP, Client.Port)
    End Sub
    Public Sub NotifyFriendStatus(ByRef c As CharacterObject, ByVal s As FriendStatus)
        Dim q As New DataTable
        Database.Query(String.Format("SELECT char_guid FROM characters_social WHERE guid = {0};", c.GUID), q)

        'DONE: Send "Friend offline/online"
        Dim friendpacket As New PacketClass(OPCODES.SMSG_FRIEND_STATUS)
        friendpacket.AddInt8(s)
        friendpacket.AddUInt64(c.GUID)
        For Each r As DataRow In q.Rows
            Dim GUID As ULong = r.Item("char_guid")
            If CHARACTERs.ContainsKey(GUID) AndAlso CHARACTERs(GUID).Client IsNot Nothing Then
                CHARACTERs(GUID).Client.SendMultiplyPackets(friendpacket)
            End If
        Next
        friendpacket.Dispose()
    End Sub


#End Region
#Region "Handlers"


    Public Sub On_CMSG_WHO(ByRef packet As PacketClass, ByRef Client As ClientClass)
        Log.WriteLine(LogType.DEBUG, "[{0}:{1}] CMSG_WHO", Client.IP, Client.Port)

        packet.GetInt16()
        Dim LevelMinimum As UInteger = packet.GetUInt32()       '0
        Dim LevelMaximum As UInteger = packet.GetUInt32()       '100
        Dim NamePlayer As String = EscapeString(packet.GetString())
        Dim NameGuild As String = EscapeString(packet.GetString())
        Dim MaskRace As UInteger = packet.GetUInt32()
        Dim MaskClass As UInteger = packet.GetUInt32()
        Dim ZonesCount As UInteger = packet.GetUInt32()         'Limited to 10
        Dim Zones As New List(Of UInteger)
        For i As Integer = 1 To ZonesCount
            Zones.Add(packet.GetUInt32)
        Next
        Dim StringsCount As UInteger = packet.GetUInt32         'Limited to 4

        'NOTE: We are reading only the first string
        Dim Strings As String = ""
        If StringsCount > 0 Then
            Strings = UCase(EscapeString(packet.GetString()))
        End If



        Dim results As New List(Of ULong)
        CHARACTERs_Lock.AcquireReaderLock(DEFAULT_LOCK_TIMEOUT)
        For Each c As KeyValuePair(Of ULong, CharacterObject) In CHARACTERs
            If Not c.Value.IsInWorld Then Continue For
            If (GetCharacterSide(c.Value.Race) <> GetCharacterSide(Client.Character.Race)) AndAlso Client.Character.Access < AccessLevel.GameMaster Then Continue For
            If c.Value.Name.IndexOf(NamePlayer) = -1 Then Continue For
            If c.Value.Level < LevelMinimum Then Continue For
            If c.Value.Level > LevelMaximum Then Continue For

            If UCase(c.Value.Name).IndexOf(Strings) = -1 Then Continue For

            'DONE: List first 49 characters (like original)
            If results.Count > 49 Then Exit For

            results.Add(c.Value.GUID)
        Next
        CHARACTERs_Lock.ReleaseReaderLock()


        Dim response As New PacketClass(OPCODES.SMSG_WHO)
        response.AddInt32(results.Count)
        response.AddInt32(results.Count)
        For Each GUID As ULong In results
            response.AddString(CHARACTERs(GUID).Name)   'Name
            response.AddString("")                      'Guild Name
            response.AddInt32(CHARACTERs(GUID).Level)   'Level
            response.AddInt32(CHARACTERs(GUID).Classe)  'Class
            response.AddInt32(CHARACTERs(GUID).Race)    'Race
            response.AddInt8(0)                         'new in 2.4.x
            response.AddInt32(CHARACTERs(GUID).Zone)    'Zone ID
        Next
        Client.Send(response)
        response.Dispose()

        Log.WriteLine(LogType.DEBUG, "[{0}:{1}] SMSG_WHO [P:'{2}' G:'{3}' L:{4}-{5} C:{6:X} R:{7:X}]", Client.IP, Client.Port, NamePlayer, NameGuild, LevelMinimum, LevelMaximum, MaskClass, MaskRace)
    End Sub


    Public Sub On_CMSG_ADD_FRIEND(ByRef packet As PacketClass, ByRef Client As ClientClass)
        If (packet.Data.Length - 1) < 6 Then Exit Sub
        packet.GetInt16()

        Dim response As New PacketClass(OPCODES.SMSG_FRIEND_STATUS)
        Dim name As String = packet.GetString()
        If (packet.Data.Length - 1) < (6 + name.Length + 1) Then Exit Sub
        Dim note As String = packet.GetString()
        Dim GUID As ULong = 0
        Log.WriteLine(LogType.DEBUG, "[{0}:{1}] CMSG_ADD_FRIEND [{2}]", Client.IP, Client.Port, name)

        'DONE: Get GUID from DB
        Dim q As New DataTable
        Database.Query(String.Format("SELECT char_guid, char_race FROM characters WHERE char_name = ""{0}"";", name), q)

        If q.Rows.Count > 0 Then
            GUID = CType(q.Rows(0).Item("char_guid"), Long)
            Dim FriendSide As Boolean = GetCharacterSide(CType(q.Rows(0).Item("char_race"), Byte))

            q.Clear()
            Database.Query(String.Format("SELECT flags FROM characters_social WHERE flags = {0}", CType(SocialFlag.SOCIAL_FLAG_FRIEND, Byte)), q)
            Dim NumberOfFriends As Integer = q.Rows.Count
            q.Clear()
            Database.Query(String.Format("SELECT flags FROM characters_social WHERE char_guid = {0} AND guid = {1} AND flags = {2};", Client.Character.GUID, GUID, CType(SocialFlag.SOCIAL_FLAG_FRIEND, Byte)), q)

            If GUID = Client.Character.GUID Then
                response.AddInt8(FriendResult.FRIEND_SELF)
                response.AddUInt64(GUID)
            ElseIf q.Rows.Count > 0 Then
                response.AddInt8(FriendResult.FRIEND_ALREADY)
                response.AddUInt64(GUID)
            ElseIf NumberOfFriends >= MAX_FRIENDS_ON_LIST Then
                response.AddInt8(FriendResult.FRIEND_LIST_FULL)
                response.AddUInt64(GUID)
            ElseIf GetCharacterSide(Client.Character.Race) <> FriendSide Then
                response.AddInt8(FriendResult.FRIEND_ENEMY)
                response.AddUInt64(GUID)
            ElseIf CHARACTERs.ContainsKey(GUID) Then
                response.AddInt8(FriendResult.FRIEND_ADDED_ONLINE)
                response.AddUInt64(GUID)
                response.AddString(name)
                'If CType(CHARACTERs(GUID), CharacterObject).DND Then
                '    response.AddInt8(FriendStatus.FRIEND_STATUS_DND)
                'ElseIf CType(CHARACTERs(GUID), CharacterObject).AFK Then
                '    response.AddInt8(FriendStatus.FRIEND_STATUS_AFK)
                'Else
                response.AddInt8(FriendStatus.FRIEND_STATUS_ONLINE)
                'End If
                response.AddInt32(CType(CHARACTERs(GUID), CharacterObject).Zone)
                response.AddInt32(CType(CHARACTERs(GUID), CharacterObject).Level)
                response.AddInt32(CType(CHARACTERs(GUID), CharacterObject).Classe)
                Database.Update(String.Format("INSERT INTO characters_social (char_guid, guid, note, flags) VALUES ({0}, {1}, ""{2}"", {3});", Client.Character.GUID, GUID, note, CType(SocialFlag.SOCIAL_FLAG_FRIEND, Byte)))
            Else
                response.AddInt8(FriendResult.FRIEND_ADDED_OFFLINE)
                response.AddUInt64(GUID)
                response.AddString(name)
                Database.Update(String.Format("INSERT INTO characters_social (char_guid, guid, note, flags) VALUES ({0}, {1}, ""{2}"", {3});", Client.Character.GUID, GUID, note, CType(SocialFlag.SOCIAL_FLAG_FRIEND, Byte)))
            End If
        Else
            response.AddInt8(FriendResult.FRIEND_NOT_FOUND)
            response.AddUInt64(GUID)
        End If

        Client.Send(response)
        response.Dispose()
        q.Dispose()
        Log.WriteLine(LogType.DEBUG, "[{0}:{1}] SMSG_FRIEND_STATUS", Client.IP, Client.Port)
    End Sub
    Public Sub On_CMSG_ADD_IGNORE(ByRef packet As PacketClass, ByRef Client As ClientClass)
        If (packet.Data.Length - 1) < 6 Then Exit Sub
        packet.GetInt16()
        Dim response As New PacketClass(OPCODES.SMSG_FRIEND_STATUS)
        Dim name As String = packet.GetString()
        Dim GUID As ULong = 0
        Log.WriteLine(LogType.DEBUG, "[{0}:{1}] CMSG_ADD_IGNORE [{2}]", Client.IP, Client.Port, name)

        'DONE: Get GUID from DB
        Dim q As New DataTable
        Database.Query(String.Format("SELECT char_guid FROM characters WHERE char_name = ""{0}"";", name), q)

        If q.Rows.Count > 0 Then
            GUID = CType(q.Rows(0).Item("char_guid"), Long)
            q.Clear()
            Database.Query(String.Format("SELECT flags FROM characters_social WHERE flags = {0}", CType(SocialFlag.SOCIAL_FLAG_IGNORED, Byte)), q)
            Dim NumberOfFriends As Integer = q.Rows.Count
            q.Clear()
            Database.Query(String.Format("SELECT * FROM characters_social WHERE char_guid = {0} AND guid = {1} AND flags = {2};", Client.Character.GUID, GUID, CType(SocialFlag.SOCIAL_FLAG_IGNORED, Byte)), q)

            If GUID = Client.Character.GUID Then
                response.AddInt8(FriendResult.FRIEND_IGNORE_SELF)
                response.AddUInt64(GUID)
            ElseIf q.Rows.Count > 0 Then
                response.AddInt8(FriendResult.FRIEND_IGNORE_ALREADY)
                response.AddUInt64(GUID)
            ElseIf NumberOfFriends >= MAX_IGNORES_ON_LIST Then
                response.AddInt8(FriendResult.FRIEND_IGNORE_ALREADY)
                response.AddUInt64(GUID)
            Else
                response.AddInt8(FriendResult.FRIEND_IGNORE_ADDED)
                response.AddUInt64(GUID)

                Database.Update(String.Format("INSERT INTO characters_social (char_guid, guid, note, flags) VALUES ({0}, {1}, ""{2}"", {3});", Client.Character.GUID, GUID, "", CType(SocialFlag.SOCIAL_FLAG_IGNORED, Byte)))
                Client.Character.IgnoreList.Add(GUID)
            End If
        Else
            response.AddInt8(FriendResult.FRIEND_IGNORE_NOT_FOUND)
            response.AddUInt64(GUID)
        End If

        Client.Send(response)
        response.Dispose()
        q.Dispose()
        Log.WriteLine(LogType.DEBUG, "[{0}:{1}] SMSG_FRIEND_STATUS", Client.IP, Client.Port)
    End Sub
    Public Sub On_CMSG_DEL_FRIEND(ByRef packet As PacketClass, ByRef Client As ClientClass)
        Log.WriteLine(LogType.DEBUG, "[{0}:{1}] CMSG_DEL_FRIEND", Client.IP, Client.Port)
        If (packet.Data.Length - 1) < 13 Then Exit Sub
        packet.GetInt16()
        Dim response As New PacketClass(OPCODES.SMSG_FRIEND_STATUS)
        Dim GUID As ULong = packet.GetUInt64()

        Try

            Database.Update(String.Format("DELETE FROM characters_social WHERE guid = {1} AND char_guid = {0};", Client.Character.GUID, GUID))
            response.AddInt8(FriendResult.FRIEND_REMOVED)
        Catch
            response.AddInt8(FriendResult.FRIEND_DB_ERROR)
        End Try

        response.AddUInt64(GUID)

        Client.Send(response)
        response.Dispose()
        Log.WriteLine(LogType.DEBUG, "[{0}:{1}] SMSG_FRIEND_STATUS", Client.IP, Client.Port)
    End Sub
    Public Sub On_CMSG_DEL_IGNORE(ByRef packet As PacketClass, ByRef Client As ClientClass)
        Log.WriteLine(LogType.DEBUG, "[{0}:{1}] CMSG_DEL_IGNORE", Client.IP, Client.Port)
        If (packet.Data.Length - 1) < 13 Then Exit Sub
        packet.GetInt16()
        Dim response As New PacketClass(OPCODES.SMSG_FRIEND_STATUS)
        Dim GUID As ULong = packet.GetUInt64()

        Try
            Database.Update(String.Format("DELETE FROM characters_social WHERE guid = {1} AND char_guid = {0};", Client.Character.GUID, GUID))
            response.AddInt8(FriendResult.FRIEND_IGNORE_REMOVED)
            Client.Character.IgnoreList.Remove(GUID)
        Catch
            response.AddInt8(FriendResult.FRIEND_DB_ERROR)
        End Try
        response.AddUInt64(GUID)

        Client.Send(response)
        response.Dispose()
        Log.WriteLine(LogType.DEBUG, "[{0}:{1}] SMSG_FRIEND_STATUS", Client.IP, Client.Port)
    End Sub
    Public Sub On_CMSG_SET_CONTACT_NOTES(ByRef packet As PacketClass, ByRef Client As ClientClass)
        If (packet.Data.Length - 1) < 14 Then Exit Sub
        Log.WriteLine(LogType.DEBUG, "[{0}:{1}] CMSG_SET_CONTACT_NOTES", Client.IP, Client.Port)
        packet.GetInt16()
        Dim GUID As ULong = packet.GetUInt64()
        Dim note As String = packet.GetString()

        Database.Update(String.Format("UPDATE characters_social SET note = ""{0}"" WHERE char_guid = {2} AND guid = {1};", note, GUID, Client.Character.GUID))
    End Sub
    Public Sub On_CMSG_CONTACT_LIST(ByRef packet As PacketClass, ByRef Client As ClientClass)
        Log.WriteLine(LogType.DEBUG, "[{0}:{1}] CMSG_CONTACT_LIST", Client.IP, Client.Port)
        SendContactList(Client, Client.Character)
    End Sub


#End Region

    





End Module