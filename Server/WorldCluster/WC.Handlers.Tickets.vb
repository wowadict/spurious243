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

Public Module WC_Handlers_Tickets

    Private Enum SuggestionType As Integer
        TYPE_BUG_REPORT = 0
        TYPE_SUGGESTION = 1
    End Enum
    Public Sub On_CMSG_BUG(ByRef packet As PacketClass, ByRef Client As ClientClass)
        If (packet.Data.Length - 1) < 14 Then Exit Sub
        packet.GetInt16()
        Dim Suggestion As SuggestionType = packet.GetInt32
        Dim cLength As Integer = packet.GetInt32
        Dim cString As String = EscapeString(packet.GetString)
        If (packet.Data.Length - 1) < (14 + cString.Length + 5) Then Exit Sub
        Dim tLength As Integer = packet.GetInt32
        Dim tString As String = EscapeString(packet.GetString)

        Log.WriteLine(LogType.DEBUG, "[{0}:{1}] CMSG_BUG [2]", Client.IP, Client.Port, Suggestion)
        Log.WriteLine(LogType.INFORMATION, "Bug report [{0}:{1}] " & cString & vbNewLine & tString)
    End Sub

    'ERR_TICKET_ALREADY_EXISTS
    'ERR_TICKET_CREATE_ERROR
    'ERR_TICKET_UPDATE_ERROR
    'ERR_TICKET_DB_ERROR
    'ERR_TICKET_NO_TEXT

    Private Enum GMTicketGetResult
        GMTICKET_AVAILABLE = 6
        GMTICKET_NOTICKET = 10
    End Enum
    Public Sub On_CMSG_GMTICKET_GETTICKET(ByRef packet As PacketClass, ByRef Client As ClientClass)
        Log.WriteLine(LogType.DEBUG, "[{0}:{1}] CMSG_GMTICKET_GETTICKET", Client.IP, Client.Port)

        Dim SMSG_GMTICKET_GETTICKET As New PacketClass(OPCODES.SMSG_GMTICKET_GETTICKET)
        Dim MySQLResult As New DataTable
        'Database.Query(String.Format("SELECT * FROM characters_tickets WHERE char_guid = {0};", Client.Character.GUID), MySQLResult)
		CharacterDatabase.Query(String.Format("SELECT * FROM characters_tickets WHERE char_guid = {0};", Client.Character.GUID), MySQLResult)
        If MySQLResult.Rows.Count > 0 Then
            SMSG_GMTICKET_GETTICKET.AddInt32(GMTicketGetResult.GMTICKET_AVAILABLE)
            SMSG_GMTICKET_GETTICKET.AddString(MySQLResult.Rows(0).Item("ticket_text"))
        Else
            SMSG_GMTICKET_GETTICKET.AddInt32(GMTicketGetResult.GMTICKET_NOTICKET)
        End If
        Client.Send(SMSG_GMTICKET_GETTICKET)
        SMSG_GMTICKET_GETTICKET.Dispose()

        Dim SMSG_QUERY_TIME_RESPONSE As New PacketClass(OPCODES.SMSG_QUERY_TIME_RESPONSE)
        SMSG_QUERY_TIME_RESPONSE.AddInt32(GetTimestamp(Now))
        Client.Send(SMSG_QUERY_TIME_RESPONSE)
        SMSG_QUERY_TIME_RESPONSE.Dispose()
    End Sub

    Private Enum GMTicketCreateResult
        GMTICKET_ALREADY_HAVE = 1
        GMTICKET_CREATE_OK = 2
    End Enum
    Public Sub On_CMSG_GMTICKET_CREATE(ByRef packet As PacketClass, ByRef Client As ClientClass)
        packet.GetInt16()
        Dim ticket_map As UInteger = packet.GetUInt32()
        Dim ticket_x As Single = packet.GetFloat()
        Dim ticket_y As Single = packet.GetFloat()
        Dim ticket_z As Single = packet.GetFloat()
        Dim ticket_text As String = EscapeString(packet.GetString)

        Dim MySQLResult As New DataTable
        'Database.Query(String.Format("SELECT * FROM characters_tickets WHERE char_guid = {0};", Client.Character.GUID), MySQLResult)
		CharacterDatabase.Query(String.Format("SELECT * FROM characters_tickets WHERE char_guid = {0};", Client.Character.GUID), MySQLResult)

        Dim SMSG_GMTICKET_CREATE As New PacketClass(OPCODES.SMSG_GMTICKET_CREATE)
        If MySQLResult.Rows.Count > 0 Then
            Log.WriteLine(LogType.DEBUG, "[{0}:{1}] CMSG_GMTICKET_CREATE", Client.IP, Client.Port)
            SMSG_GMTICKET_CREATE.AddInt32(GMTicketCreateResult.GMTICKET_ALREADY_HAVE)
        Else
            Log.WriteLine(LogType.DEBUG, "[{0}:{1}] CMSG_GMTICKET_CREATE [{2}]", Client.IP, Client.Port, ticket_text)
            'Database.Update(String.Format("INSERT INTO characters_tickets (char_guid, ticket_text, ticket_x, ticket_y, ticket_z, ticket_map) VALUES ({0} , ""{1}"", {2}, {3}, {4}, {5});", Client.Character.GUID, ticket_text, Trim(Str(ticket_x)), Trim(Str(ticket_y)), Trim(Str(ticket_z)), ticket_map))
			CharacterDatabase.Update(String.Format("INSERT INTO characters_tickets (char_guid, ticket_text, ticket_x, ticket_y, ticket_z, ticket_map) VALUES ({0} , ""{1}"", {2}, {3}, {4}, {5});", Client.Character.GUID, ticket_text, Trim(Str(ticket_x)), Trim(Str(ticket_y)), Trim(Str(ticket_z)), ticket_map))
            SMSG_GMTICKET_CREATE.AddInt32(GMTicketCreateResult.GMTICKET_CREATE_OK)
        End If
        Client.Send(SMSG_GMTICKET_CREATE)
        SMSG_GMTICKET_CREATE.Dispose()
    End Sub

    Private Enum GMTicketSystemStatus
        GMTICKET_SYSTEMSTATUS_ENABLED = 1
        GMTICKET_SYSTEMSTATUS_DISABLED = 2
        GMTICKET_SYSTEMSTATUS_SURVEY = 3
    End Enum
    Public Sub On_CMSG_GMTICKET_SYSTEMSTATUS(ByRef packet As PacketClass, ByRef Client As ClientClass)
        Log.WriteLine(LogType.DEBUG, "[{0}:{1}] CMSG_GMTICKET_SYSTEMSTATUS", Client.IP, Client.Port)

        Dim SMSG_GMTICKET_SYSTEMSTATUS As New PacketClass(OPCODES.SMSG_GMTICKET_SYSTEMSTATUS)
        SMSG_GMTICKET_SYSTEMSTATUS.AddInt32(GMTicketSystemStatus.GMTICKET_SYSTEMSTATUS_SURVEY)
        Client.Send(SMSG_GMTICKET_SYSTEMSTATUS)
        SMSG_GMTICKET_SYSTEMSTATUS.Dispose()
    End Sub

    Private Enum GMTicketDeleteResult
        GMTICKET_DELETE_SUCCESS = 9
    End Enum
    Public Sub On_CMSG_GMTICKET_DELETETICKET(ByRef packet As PacketClass, ByRef Client As ClientClass)
        Log.WriteLine(LogType.DEBUG, "[{0}:{1}] CMSG_GMTICKET_DELETETICKET", Client.IP, Client.Port)
        'Database.Update(String.Format("DELETE FROM characters_tickets WHERE char_guid = {0};", Client.Character.GUID))
		CharacterDatabase.Update(String.Format("DELETE FROM characters_tickets WHERE char_guid = {0};", Client.Character.GUID))

        Dim SMSG_GMTICKET_DELETETICKET As New PacketClass(OPCODES.SMSG_GMTICKET_DELETETICKET)
        SMSG_GMTICKET_DELETETICKET.AddInt32(GMTicketDeleteResult.GMTICKET_DELETE_SUCCESS)
        Client.Send(SMSG_GMTICKET_DELETETICKET)
        SMSG_GMTICKET_DELETETICKET.Dispose()
    End Sub
    Public Sub On_CMSG_GMTICKET_UPDATETEXT(ByRef packet As PacketClass, ByRef Client As ClientClass)
        If (packet.Data.Length - 1) < 7 Then Exit Sub
        packet.GetInt16()
        Dim ticket_text As String = EscapeString(packet.GetString)
        Log.WriteLine(LogType.DEBUG, "[{0}:{1}] CMSG_GMTICKET_UPDATETEXT [{2}]", Client.IP, Client.Port, ticket_text)
        'Database.Update(String.Format("UPDATE characters_tickets SET char_guid={0}, ticket_text=""{1}"";", Client.Character.GUID, ticket_text))
		CharacterDatabase.Update(String.Format("UPDATE characters_tickets SET char_guid={0}, ticket_text=""{1}"";", Client.Character.GUID, ticket_text))
    End Sub

    Public Sub On_CMSG_GMSURVEY_SUBMIT(ByRef packet As PacketClass, ByRef Client As ClientClass)
        packet.GetInt16()

        Dim Questions As UInteger = packet.GetUInt32
        'Data from GMSurveyQuestions.dbc
        '   Dim Question As UInteger = packet.GetUInt32
        '   Dim Value As Byte = packet.GetUInt32
        '   Dim Unknown As String = packet.GetString
        'Dim Comment As String = packet.GetString

        Log.WriteLine(LogType.DEBUG, "[{0}:{1}] CMSG_GMSURVEY_SUBMIT [{2}]", Client.IP, Client.Port, Questions)

#If DEBUG Then
        DumpPacket(packet.Data, Client)
#End If
    End Sub
    Public Sub On_CMSG_WHOIS(ByRef packet As PacketClass, ByRef Client As ClientClass)
        packet.GetInt16()

        Dim Name As String = packet.GetString
        Log.WriteLine(LogType.DEBUG, "[{0}:{1}] CMSG_WHOIS [{2}]", Client.IP, Client.Port, Name)

        Dim response As New PacketClass(OPCODES.SMSG_WHOIS)
        response.AddString("This feature is not available yet.")
        Client.Send(response)
        response.Dispose()
    End Sub

    Private Enum ComplainType As Byte
        SPAM_MAIL = 0
        SPAM_CHAT = 1
    End Enum
    Public Sub On_CMSG_COMPLAIN(ByRef packet As PacketClass, ByRef Client As ClientClass)
        packet.GetInt16()

        Dim Type As ComplainType = packet.GetInt8
        Dim GUID As ULong = packet.GetUInt64
        Select Case Type
            Case ComplainType.SPAM_MAIL
                packet.GetUInt32()                              '0?
                packet.GetUInt32()                              'MailID?
                packet.GetUInt32()                              '0?

                Log.WriteLine(LogType.DEBUG, "[{0}:{1}] CMSG_COMPLAIN [MAIL]", Client.IP, Client.Port, Type)
            Case ComplainType.SPAM_CHAT
                packet.GetUInt32()                              'Language?
                packet.GetUInt32()                              'MsgType?
                packet.GetUInt32()                              'ChannelID?
                packet.GetUInt32()                              'Unk?
                Dim Description As String = packet.GetString()  'Description?

                Log.WriteLine(LogType.DEBUG, "[{0}:{1}] CMSG_COMPLAIN [CHAT: {2}]", Client.IP, Client.Port, Type, Description)
            Case Else
                Log.WriteLine(LogType.DEBUG, "[{0}:{1}] CMSG_COMPLAIN [Unknown Type {2}]", Client.IP, Client.Port, Type)
        End Select

        Dim response As New PacketClass(OPCODES.SMSG_COMPLAIN_RESULT)
        response.AddInt8(0)
        Client.Send(response)
        response.Dispose()
    End Sub

End Module