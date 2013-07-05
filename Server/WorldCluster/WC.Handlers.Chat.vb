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
Imports mangosVB.Common
Imports mangosVB.Common.BaseWriter

Public Module WC_Handlers_Chat

    Public Sub On_CMSG_CHAT_IGNORED(ByRef packet As PacketClass, ByRef Client As ClientClass)
        packet.GetInt16()

        Dim GUID As ULong = packet.GetUInt64
        Log.WriteLine(LogType.DEBUG, "[{0}:{1}] CMSG_CHAT_IGNORED [0x{2}]", Client.IP, Client.Port, GUID)

        If CHARACTERs.ContainsKey(GUID) Then
            Dim response As PacketClass = BuildChatMessage(Client.Character.GUID, "", ChatMsg.CHAT_MSG_IGNORED, LANGUAGES.LANG_UNIVERSAL, 0, "")
            CHARACTERs(GUID).Client.Send(response)
            response.Dispose()
        End If
    End Sub
    Public Sub On_CMSG_MESSAGECHAT(ByRef packet As PacketClass, ByRef Client As ClientClass)
        If (packet.Data.Length - 1) < 14 Then Exit Sub
        packet.GetInt16()

        Dim msgType As ChatMsg = packet.GetInt32()
        Dim msgLanguage As LANGUAGES = packet.GetInt32()
        Log.WriteLine(LogType.DEBUG, "[{0}:{1}] CMSG_MESSAGECHAT [{2}:{3}]", Client.IP, Client.Port, msgType, msgLanguage)

        Select Case msgType

            Case ChatMsg.CHAT_MSG_CHANNEL
                Dim Channel As String = packet.GetString()
                If (packet.Data.Length - 1) < (14 + Channel.Length) Then Exit Sub
                Dim Message As String = packet.GetString()

                'DONE: Broadcast to all
                If CHAT_CHANNELs.ContainsKey(Channel.ToUpper) Then
                    CHAT_CHANNELs(Channel.ToUpper).Say(Message, msgLanguage, Client.Character)
                End If
                Exit Select

            Case ChatMsg.CHAT_MSG_WHISPER
                Dim ToUser As String = UCase(packet.GetString())
                If (packet.Data.Length - 1) < (14 + ToUser.Length) Then Exit Sub
                Dim Message As String = packet.GetString()

                'DONE: Handle admin/gm commands
                If ToUser = "WARDEN" AndAlso Client.Character.Access > 0 Then
                    Client.Character.GetWorld.ClientPacket(Client.Index, packet.Data)
                    Exit Sub
                End If

                'DONE: Send whisper MSG to receiver
                Dim GUID As ULong = 0
                CHARACTERs_Lock.AcquireReaderLock(DEFAULT_LOCK_TIMEOUT)
                For Each Character As KeyValuePair(Of ULong, CharacterObject) In CHARACTERs
                    If UCase(Character.Value.Name) = ToUser Then
                        GUID = Character.Value.GUID
                        Exit For
                    End If
                Next
                CHARACTERs_Lock.ReleaseReaderLock()

                If GUID > 0 Then
                    'DONE: Check if ignoring
                    If CHARACTERs(GUID).IgnoreList.Contains(Client.Character.GUID) Then
                        'Client.Character.SystemMessage(String.Format("{0} is ignoring you.", ToUser))
                        Client.Character.SendChatMessage(GUID, "", ChatMsg.CHAT_MSG_IGNORED, LANGUAGES.LANG_UNIVERSAL)
                    Else
                        'From [gaddas]: Hello
                        CHARACTERs(GUID).SendChatMessage(Client.Character.GUID, Message, ChatMsg.CHAT_MSG_WHISPER, msgLanguage)
                        'To [gaddas]: Hello
                        Client.Character.SendChatMessage(GUID, Message, ChatMsg.CHAT_MSG_REPLY, msgLanguage)
                    End If
                Else
                    Dim SMSG_CHAT_PLAYER_NOT_FOUND As New PacketClass(OPCODES.SMSG_CHAT_PLAYER_NOT_FOUND)
                    SMSG_CHAT_PLAYER_NOT_FOUND.AddString(ToUser)
                    Client.Send(SMSG_CHAT_PLAYER_NOT_FOUND)
                    SMSG_CHAT_PLAYER_NOT_FOUND.Dispose()
                End If
                Exit Select

            Case ChatMsg.CHAT_MSG_PARTY, ChatMsg.CHAT_MSG_RAID, ChatMsg.CHAT_MSG_RAID_LEADER, ChatMsg.CHAT_MSG_RAID_WARNING, ChatMsg.CHAT_MSG_BATTLEGROUND_LEADER, ChatMsg.CHAT_MSG_BATTLEGROUND
                Dim Message As String = packet.GetString()

                'DONE: Check in group
                If Not Client.Character.IsInGroup Then
                    SendPartyResult(Client, "", PartyCommandResult.INVITE_NOT_IN_PARTY)
                    Exit Select
                End If

                'DONE: Broadcast to party
                Client.Character.Group.SendChatMessage(Client.Character, Message, msgLanguage, msgType)
                Exit Select

            Case ChatMsg.CHAT_MSG_AFK, ChatMsg.CHAT_MSG_DND
                Client.Character.GetWorld.ClientPacket(Client.Index, packet.Data)
                Exit Select

            Case ChatMsg.CHAT_MSG_SAY, ChatMsg.CHAT_MSG_YELL, ChatMsg.CHAT_MSG_EMOTE
                Client.Character.GetWorld.ClientPacket(Client.Index, packet.Data)
                Exit Select

            Case ChatMsg.CHAT_MSG_OFFICER, ChatMsg.CHAT_MSG_GUILD
                'ToBe Moved to Cluster
                Client.Character.GetWorld.ClientPacket(Client.Index, packet.Data)
                Exit Select

            Case Else
                Log.WriteLine(LogType.FAILED, "[{0}:{1}] Unknown chat message [msgType={2}, msgLanguage={3}]", Client.IP, Client.Port, msgType, msgLanguage)
                DumpPacket(packet.Data, Client)
        End Select

    End Sub

    Public Sub On_CMSG_JOIN_CHANNEL(ByRef packet As PacketClass, ByRef Client As ClientClass)
        If (packet.Data.Length - 1) < 12 Then Exit Sub
        packet.GetInt16()
        Dim ChannelIndex As Integer = packet.GetInt32()
        Dim ChannelOperation As Short = packet.GetInt16()
        Dim ChannelName As String = packet.GetString()
        Dim Password As String = packet.GetString()

        Log.WriteLine(LogType.DEBUG, "[{0}:{1}] CMSG_JOIN_CHANNEL [{3}:{2}]", Client.IP, Client.Port, ChannelName, ChannelIndex)

        If Not CHAT_CHANNELs.ContainsKey(ChannelName.ToUpper) Then
            'Dim NewChannel As New ChatChannelClass(ChannelName)
            Dim NewChannel As New VoiceChatChannelClass(ChannelName, ChannelIndex)
        End If

        CHAT_CHANNELs(ChannelName.ToUpper).Join(Client.Character, Password, ChannelIndex)
    End Sub
    Public Sub On_CMSG_LEAVE_CHANNEL(ByRef packet As PacketClass, ByRef Client As ClientClass)
        If (packet.Data.Length - 1) < 10 Then Exit Sub
        packet.GetInt16()
        Dim ChannelIndex As String = packet.GetInt32
        Dim ChannelName As String = packet.GetString

        Log.WriteLine(LogType.DEBUG, "[{0}:{1}] CMSG_LEAVE_CHANNEL [{3}:{2}]", Client.IP, Client.Port, ChannelName, ChannelIndex)

        ChannelName = ChannelName.ToUpper
        If CHAT_CHANNELs.ContainsKey(ChannelName) Then
            CHAT_CHANNELs(ChannelName).Part(Client.Character)
        End If
    End Sub
    Public Sub On_CMSG_CHANNEL_LIST(ByRef packet As PacketClass, ByRef Client As ClientClass)
        If (packet.Data.Length - 1) < 6 Then Exit Sub
        packet.GetInt16()
        Dim ChannelName As String = packet.GetString()

        Log.WriteLine(LogType.DEBUG, "[{0}:{1}] CMSG_CHANNEL_LIST [{2}]", Client.IP, Client.Port, ChannelName)

        ChannelName = ChannelName.ToUpper
        If CHAT_CHANNELs.ContainsKey(ChannelName) Then
            CHAT_CHANNELs(ChannelName).List(Client.Character)
        End If
    End Sub

    Public Sub On_CMSG_CHANNEL_PASSWORD(ByRef packet As PacketClass, ByRef Client As ClientClass)
        packet.GetInt16()
        Dim ChannelName As String = packet.GetString
        Dim ChannelNewPassword As String = packet.GetString

        Log.WriteLine(LogType.DEBUG, "[{0}:{1}] CMSG_CHANNEL_PASSWORD [{2}, {3}]", Client.IP, Client.Port, ChannelName, ChannelNewPassword)

        ChannelName = ChannelName.ToUpper
        If CHAT_CHANNELs.ContainsKey(ChannelName) Then
            CHAT_CHANNELs(ChannelName).SetPassword(Client.Character, ChannelNewPassword)
        End If
    End Sub
    Public Sub On_CMSG_CHANNEL_SET_OWNER(ByRef packet As PacketClass, ByRef Client As ClientClass)
        packet.GetInt16()
        Dim ChannelName As String = packet.GetString
        Dim ChannelNewOwner As String = packet.GetString

        Log.WriteLine(LogType.DEBUG, "[{0}:{1}] CMSG_CHANNEL_SET_OWNER [{2}, {3}]", Client.IP, Client.Port, ChannelName, ChannelNewOwner)

        ChannelName = ChannelName.ToUpper
        If CHAT_CHANNELs.ContainsKey(ChannelName) Then
            If CHAT_CHANNELs(ChannelName).CanSetOwner(Client.Character, ChannelNewOwner) Then
                For Each GUID As ULong In CHAT_CHANNELs(ChannelName).Joined.ToArray
                    If CHARACTERs(GUID).Name.ToUpper = ChannelNewOwner.ToUpper Then
                        CHAT_CHANNELs(ChannelName).SetOwner(CHARACTERs(GUID))
                        Exit For
                    End If
                Next
            End If
        End If
    End Sub
    Public Sub On_CMSG_CHANNEL_OWNER(ByRef packet As PacketClass, ByRef Client As ClientClass)
        packet.GetInt16()
        Dim ChannelName As String = packet.GetString

        Log.WriteLine(LogType.DEBUG, "[{0}:{1}] CMSG_CHANNEL_OWNER [{2}]", Client.IP, Client.Port, ChannelName)

        ChannelName = ChannelName.ToUpper
        If CHAT_CHANNELs.ContainsKey(ChannelName) Then
            CHAT_CHANNELs(ChannelName).GetOwner(Client.Character)
        End If
    End Sub
    Public Sub On_CMSG_CHANNEL_MODERATOR(ByRef packet As PacketClass, ByRef Client As ClientClass)
        packet.GetInt16()
        Dim ChannelName As String = packet.GetString
        Dim ChannelUser As String = packet.GetString

        Log.WriteLine(LogType.DEBUG, "[{0}:{1}] CMSG_CHANNEL_MODERATOR [{2}, {3}]", Client.IP, Client.Port, ChannelName, ChannelUser)

        ChannelName = ChannelName.ToUpper
        If CHAT_CHANNELs.ContainsKey(ChannelName) Then
            CHAT_CHANNELs(ChannelName).SetModerator(Client.Character, ChannelUser)
        End If
    End Sub
    Public Sub On_CMSG_CHANNEL_UNMODERATOR(ByRef packet As PacketClass, ByRef Client As ClientClass)
        packet.GetInt16()
        Dim ChannelName As String = packet.GetString
        Dim ChannelUser As String = packet.GetString

        Log.WriteLine(LogType.DEBUG, "[{0}:{1}] CMSG_CHANNEL_UNMODERATOR [{2}, {3}]", Client.IP, Client.Port, ChannelName, ChannelUser)

        ChannelName = ChannelName.ToUpper
        If CHAT_CHANNELs.ContainsKey(ChannelName) Then
            CHAT_CHANNELs(ChannelName).SetUnModerator(Client.Character, ChannelUser)
        End If
    End Sub
    Public Sub On_CMSG_CHANNEL_MUTE(ByRef packet As PacketClass, ByRef Client As ClientClass)
        packet.GetInt16()
        Dim ChannelName As String = packet.GetString
        Dim ChannelUser As String = packet.GetString

        Log.WriteLine(LogType.DEBUG, "[{0}:{1}] CMSG_CHANNEL_MUTE [{2}, {3}]", Client.IP, Client.Port, ChannelName, ChannelUser)

        ChannelName = ChannelName.ToUpper
        If CHAT_CHANNELs.ContainsKey(ChannelName) Then
            CHAT_CHANNELs(ChannelName).SetMute(Client.Character, ChannelUser)
        End If
    End Sub
    Public Sub On_CMSG_CHANNEL_UNMUTE(ByRef packet As PacketClass, ByRef Client As ClientClass)
        packet.GetInt16()
        Dim ChannelName As String = packet.GetString
        Dim ChannelUser As String = packet.GetString

        Log.WriteLine(LogType.DEBUG, "[{0}:{1}] CMSG_CHANNEL_UNMUTE [{2}, {3}]", Client.IP, Client.Port, ChannelName, ChannelUser)

        ChannelName = ChannelName.ToUpper
        If CHAT_CHANNELs.ContainsKey(ChannelName) Then
            CHAT_CHANNELs(ChannelName).SetUnMute(Client.Character, ChannelUser)
        End If
    End Sub
    Public Sub On_CMSG_CHANNEL_INVITE(ByRef packet As PacketClass, ByRef Client As ClientClass)
        If (packet.Data.Length - 1) < 6 Then Exit Sub
        packet.GetInt16()
        Dim ChannelName As String = packet.GetString
        If (packet.Data.Length - 1) < 6 + ChannelName.Length + 1 Then Exit Sub
        Dim PlayerName As String = CapitalizeName(packet.GetString)

        Log.WriteLine(LogType.DEBUG, "[{0}:{1}] CMSG_CHANNEL_INVITE [{2}, {3}]", Client.IP, Client.Port, ChannelName, PlayerName)

        ChannelName = ChannelName.ToUpper
        If CHAT_CHANNELs.ContainsKey(ChannelName) Then
            CHAT_CHANNELs(ChannelName).Invite(Client.Character, PlayerName)
        End If
    End Sub
    Public Sub On_CMSG_CHANNEL_KICK(ByRef packet As PacketClass, ByRef Client As ClientClass)
        If (packet.Data.Length - 1) < 6 Then Exit Sub
        packet.GetInt16()
        Dim ChannelName As String = packet.GetString
        If (packet.Data.Length - 1) < 6 + ChannelName.Length + 1 Then Exit Sub
        Dim PlayerName As String = CapitalizeName(packet.GetString)

        Log.WriteLine(LogType.DEBUG, "[{0}:{1}] CMSG_CHANNEL_KICK [{2}, {3}]", Client.IP, Client.Port, ChannelName, PlayerName)

        ChannelName = ChannelName.ToUpper
        If CHAT_CHANNELs.ContainsKey(ChannelName) Then
            CHAT_CHANNELs(ChannelName).Kick(Client.Character, PlayerName)
        End If
    End Sub
    Public Sub On_CMSG_CHANNEL_ANNOUNCEMENTS(ByRef packet As PacketClass, ByRef Client As ClientClass)
        packet.GetInt16()
        Dim ChannelName As String = packet.GetString

        Log.WriteLine(LogType.DEBUG, "[{0}:{1}] CMSG_CHANNEL_ANNOUNCEMENTS [{2}]", Client.IP, Client.Port, ChannelName)

        ChannelName = ChannelName.ToUpper
        If CHAT_CHANNELs.ContainsKey(ChannelName) Then
            CHAT_CHANNELs(ChannelName).SetAnnouncements(Client.Character)
        End If
    End Sub
    Public Sub On_CMSG_CHANNEL_BAN(ByRef packet As PacketClass, ByRef Client As ClientClass)
        If (packet.Data.Length - 1) < 6 Then Exit Sub
        packet.GetInt16()
        Dim ChannelName As String = packet.GetString
        If (packet.Data.Length - 1) < 6 + ChannelName.Length + 1 Then Exit Sub
        Dim PlayerName As String = CapitalizeName(packet.GetString)

        Log.WriteLine(LogType.DEBUG, "[{0}:{1}] CMSG_CHANNEL_BAN [{2}, {3}]", Client.IP, Client.Port, ChannelName, PlayerName)

        ChannelName = ChannelName.ToUpper
        If CHAT_CHANNELs.ContainsKey(ChannelName) Then
            CHAT_CHANNELs(ChannelName).Ban(Client.Character, PlayerName)
        End If
    End Sub
    Public Sub On_CMSG_CHANNEL_UNBAN(ByRef packet As PacketClass, ByRef Client As ClientClass)
        If (packet.Data.Length - 1) < 6 Then Exit Sub
        packet.GetInt16()
        Dim ChannelName As String = packet.GetString
        If (packet.Data.Length - 1) < 6 + ChannelName.Length + 1 Then Exit Sub
        Dim PlayerName As String = CapitalizeName(packet.GetString)

        Log.WriteLine(LogType.DEBUG, "[{0}:{1}] CMSG_CHANNEL_UNBAN [{2}, {3}]", Client.IP, Client.Port, ChannelName, PlayerName)

        ChannelName = ChannelName.ToUpper
        If CHAT_CHANNELs.ContainsKey(ChannelName) Then
            CHAT_CHANNELs(ChannelName).UnBan(Client.Character, PlayerName)
        End If
    End Sub
    Public Sub On_CMSG_CHANNEL_MODERATE(ByRef packet As PacketClass, ByRef Client As ClientClass)
        packet.GetInt16()
        Dim ChannelName As String = packet.GetString

        Log.WriteLine(LogType.DEBUG, "[{0}:{1}] CMSG_CHANNEL_MODERATE [{2}]", Client.IP, Client.Port, ChannelName)

        ChannelName = ChannelName.ToUpper
        If CHAT_CHANNELs.ContainsKey(ChannelName) Then
            CHAT_CHANNELs(ChannelName).SetModeration(Client.Character)
        End If
    End Sub
    Public Sub On_CMSG_CHANNEL_DISPLAY_LIST(ByRef packet As PacketClass, ByRef Client As ClientClass)
        packet.GetInt16()
        Dim ChannelName As String = packet.GetString

        Log.WriteLine(LogType.DEBUG, "[{0}:{1}] CMSG_CHANNEL_DISPLAY_LIST [{2}]", Client.IP, Client.Port, ChannelName)

        ChannelName = ChannelName.ToUpper
        If CHAT_CHANNELs.ContainsKey(ChannelName) Then
            CHAT_CHANNELs(ChannelName).List(Client.Character)
        End If
    End Sub

    Public Sub On_CMSG_GET_CHANNEL_MEMBER_COUNT(ByRef packet As PacketClass, ByRef Client As ClientClass)
        packet.GetInt16()
        Dim ChannelName As String = packet.GetString

        Log.WriteLine(LogType.DEBUG, "[{0}:{1}] CMSG_GET_CHANNEL_MEMBER_COUNT [{2}]", Client.IP, Client.Port, ChannelName)

        ChannelName = ChannelName.ToUpper
        If CHAT_CHANNELs.ContainsKey(ChannelName) Then
            With CHAT_CHANNELs(ChannelName)
                Dim response As New PacketClass(OPCODES.SMSG_CHANNEL_MEMBER_COUNT)
                response.AddString(.ChannelName)
                response.AddInt8(.ChannelFlags)
                response.AddUInt32(.Joined.Count)
                Client.Send(response)
                response.Dispose()
            End With
        End If
    End Sub
    Public Sub On_CMSG_SET_CHANNEL_WATCH(ByRef packet As PacketClass, ByRef Client As ClientClass)
        packet.GetInt16()
        Dim ChannelName As String = packet.GetString

        Log.WriteLine(LogType.DEBUG, "[{0}:{1}] CMSG_SET_CHANNEL_WATCH [{2}]", Client.IP, Client.Port, ChannelName)

        ChannelName = ChannelName.ToUpper
        If CHAT_CHANNELs.ContainsKey(ChannelName) Then
            'SMSG_USERLIST_ADD
            'SMSG_USERLIST_UPDATE
            '       Join GUID
            '       Join Flags
            '       Channel Flags
            '       Channel Count
            '       Channel Name
            'SMSG_USERLIST_REMOVE
            '       Join GUID
            '       Channel Flags
            '       Channel Count
            '       Channel Name
        End If
    End Sub
    Public Sub On_CMSG_CLEAR_CHANNEL_WATCH(ByRef packet As PacketClass, ByRef Client As ClientClass)
        packet.GetInt16()
        Dim ChannelName As String = packet.GetString

        Log.WriteLine(LogType.DEBUG, "[{0}:{1}] CMSG_CLEAR_CHANNEL_WATCH [{2}]", Client.IP, Client.Port, ChannelName)
    End Sub

End Module