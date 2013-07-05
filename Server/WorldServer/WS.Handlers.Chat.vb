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
Imports System.Collections.Generic
Imports mangosVB.Common.BaseWriter

Public Module WS_Handlers_Chat

    Public Function GetChatFlag(ByVal c As CharacterObject) As Byte
        If c.GM Then
            Return 4
        ElseIf c.AFK Then
            Return 1
        ElseIf c.DND Then
            Return 3
        Else
            Return 0
        End If
    End Function
    Public Sub On_CMSG_MESSAGECHAT(ByRef packet As PacketClass, ByRef Client As ClientClass)
        Log.WriteLine(LogType.DEBUG, "[{0}:{1}] CMSG_MESSAGECHAT", Client.IP, Client.Port)
        If (packet.Data.Length - 1) < 14 Then Exit Sub
        packet.GetInt16()

        Dim msgType As ChatMsg = packet.GetInt32()
        Dim msgLanguage As LANGUAGES = packet.GetInt32()
        Log.WriteLine(LogType.DEBUG, "[{0}:{1}] CMSG_MESSAGECHAT [{2}:{3}]", Client.IP, Client.Port, msgType, msgLanguage)

        'DONE: Changing language
        If Client.Character.Spell_Language <> -1 Then msgLanguage = Client.Character.Spell_Language

        Select Case msgType
            Case ChatMsg.CHAT_MSG_SAY, ChatMsg.CHAT_MSG_YELL, ChatMsg.CHAT_MSG_EMOTE
                Dim Message As String = packet.GetString()
                Client.Character.SendChatMessage(Client.Character, Message, msgType, msgLanguage, "", True)
                Exit Select

            Case ChatMsg.CHAT_MSG_GUILD
                Dim Message As String = packet.GetString()

                'DONE: Broadcast to guild
                BroadcastChatMessageGuild(Client.Character, Message, msgLanguage, Client.Character.GuildID)
                Exit Select
            Case ChatMsg.CHAT_MSG_OFFICER
                Dim Message As String = packet.GetString()

                'DONE: Broadcast to officer chat
                BroadcastChatMessageOfficer(Client.Character, Message, msgLanguage, Client.Character.GuildID)
                Exit Select

            Case ChatMsg.CHAT_MSG_AFK
                Dim Message As String = packet.GetString()
                If Message <> "" Then
                    Client.Character.AFK = True
                Else
                    Client.Character.AFK = False
                End If
                Client.Character.SetUpdateFlag(EPlayerFields.PLAYER_FLAGS, Client.Character.cPlayerFlags)
                Client.Character.SendCharacterUpdate()
                Exit Select

            Case ChatMsg.CHAT_MSG_DND
                Dim Message As String = packet.GetString()
                If Message <> "" Then
                    Client.Character.DND = True
                Else
                    Client.Character.DND = False
                End If
                Client.Character.SetUpdateFlag(EPlayerFields.PLAYER_FLAGS, Client.Character.cPlayerFlags)
                Client.Character.SendCharacterUpdate()
                Exit Select

            Case ChatMsg.CHAT_MSG_WHISPER
                Dim ToUser As String = UCase(packet.GetString())
                If (packet.Data.Length - 1) < (14 + ToUser.Length) Then Exit Sub
                Dim Message As String = packet.GetString()

                'DONE: Handle admin/gm commands
                If ToUser = "SERVER" AndAlso Client.Character.Access > 0 Then
                    Dim toServer As PacketClass = BuildChatMessage(WardenGUID, Message, ChatMsg.CHAT_MSG_REPLY, LANGUAGES.LANG_UNIVERSAL)
                    Client.Send(toServer)
                    toServer.Dispose()

                    OnCommand(Client, Message)
                    Exit Sub
                Else
                    Log.WriteLine(LogType.WARNING, "This chat message type should not be here!")
                End If
                Exit Select

            Case ChatMsg.CHAT_MSG_CHANNEL, ChatMsg.CHAT_MSG_PARTY, ChatMsg.CHAT_MSG_RAID, ChatMsg.CHAT_MSG_RAID_WARNING, ChatMsg.CHAT_MSG_RAID_LEADER
                Log.WriteLine(LogType.WARNING, "This chat message type should not be here!")
                Exit Select

            Case Else
                Log.WriteLine(LogType.FAILED, "[{0}:{1}] Unknown chat message [msgType={2}, msgLanguage={3}]", Client.IP, Client.Port, msgType, msgLanguage)
                DumpPacket(packet.Data, Client)
        End Select

    End Sub

End Module