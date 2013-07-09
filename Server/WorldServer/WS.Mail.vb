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
Imports mangosVB.Common.BaseWriter

Public Module WS_Mail

#Region "WS.Mail.Constants"

    Public Const ITEM_MAILTEXT_ITEMID As Integer = 889

    Private Enum MailResult
        MAIL_SENT = 0
        MAIL_MONEY_REMOVED = 1
        MAIL_ITEM_REMOVED = 2
        MAIL_RETURNED = 3
        MAIL_DELETED = 4
        MAIL_MADE_PERMANENT = 5

    End Enum
    Private Enum MailSentError
        NO_ERROR = 0
        BAG_FULL = 1
        CANNOT_SEND_TO_SELF = 2
        NOT_ENOUGHT_MONEY = 3
        CHARACTER_NOT_FOUND = 4
        NOT_YOUR_ALLIANCE = 5
        RECIPIENT_CAP_REACHED = 6
        CANT_SEND_WRAPPED_COD = 7
        MAIL_AND_CHAT_SUSPENDED = 8
        INTERNAL_ERROR = 9
    End Enum

    Private Enum MailReadInfo As Byte
        Unread = 0
        Read = 1
        Auction = 4
        COD = 8
    End Enum
    Private Enum MailTypeInfo As Byte
        NORMAL = 0
        GMMAIL = 1
        AUCTION = 2
    End Enum

#End Region
#Region "WS.Mail.Handlers"

    Public Sub On_CMSG_MAIL_RETURN_TO_SENDER(ByRef packet As PacketClass, ByRef Client As ClientClass)
        If (packet.Data.Length - 1) < 17 Then Exit Sub
        packet.GetInt16()
        Dim GameObjectGUID As ULong = packet.GetUInt64
        Dim MailID As Integer = packet.GetInt32

        Log.WriteLine(LogType.DEBUG, "[{0}:{1}] CMSG_MAIL_RETURN_TO_SENDER [MailID={2}]", Client.IP, Client.Port, MailID)

        'A = 1
        'B = 2
        'A = A + B '3
        'B = A - B '3-2=1
        'A = A - B '3-1=2

        Dim MailTime As Integer = GetTimestamp(Now) + (86400 * 30) 'Set expiredate to today + 30 days
        CharacterDatabase.Update(String.Format("UPDATE characters_mail SET mail_time = {1}, mail_read = 0, mail_receiver = (mail_receiver + mail_sender), mail_sender = (mail_receiver - mail_sender), mail_receiver = (mail_receiver - mail_sender) WHERE mail_id = {0};", MailID, MailTime))

        Dim response As New PacketClass(OPCODES.SMSG_SEND_MAIL_RESULT)
        response.AddInt32(MailID)
        response.AddInt32(MailResult.MAIL_RETURNED)
        response.AddInt32(0)
        Client.Send(response)
        response.Dispose()
    End Sub
    Public Sub On_CMSG_MAIL_DELETE(ByRef packet As PacketClass, ByRef Client As ClientClass)
        If (packet.Data.Length - 1) < 17 Then Exit Sub
        packet.GetInt16()
        Dim GameObjectGUID As ULong = packet.GetUInt64
        Dim MailID As Integer = packet.GetInt32
        Dim i As Byte

        Log.WriteLine(LogType.DEBUG, "[{0}:{1}] CMSG_MAIL_DELETE [MailID={2}]", Client.IP, Client.Port, MailID)

        Dim MySQLQuery As New DataTable
        CharacterDatabase.Query(String.Format("SELECT item_guid FROM mail_items WHERE mail_id = {0};", MailID), MySQLQuery)

        If MySQLQuery.Rows.Count > 0 Then
            For i = 0 To MySQLQuery.Rows.Count - 1
                CharacterDatabase.Update(String.Format("DELETE FROM characters_inventory WHERE item_guid = {0};", MySQLQuery.Rows(i).Item("item_guid")))
            Next
        End If

        CharacterDatabase.Update(String.Format("DELETE FROM characters_mail WHERE mail_id = {0};", MailID))

        Dim response As New PacketClass(OPCODES.SMSG_SEND_MAIL_RESULT)
        response.AddInt32(MailID)
        response.AddInt32(MailResult.MAIL_DELETED)
        response.AddInt32(0)
        Client.Send(response)
        response.Dispose()
    End Sub
    Public Sub On_CMSG_MAIL_MARK_AS_READ(ByRef packet As PacketClass, ByRef Client As ClientClass)
        If (packet.Data.Length - 1) < 17 Then Exit Sub
        packet.GetInt16()
        Dim GameObjectGUID As ULong = packet.GetUInt64
        Dim MailID As Integer = packet.GetInt32

        Log.WriteLine(LogType.DEBUG, "[{0}:{1}] CMSG_MAIL_MARK_AS_READ [MailID={2}]", Client.IP, Client.Port, MailID)
        Dim MailTime As Integer = GetTimestamp(Now) + (86400 * 3) 'Set expiredate to today + 3 days
        CharacterDatabase.Update(String.Format("UPDATE characters_mail SET mail_read = 1, mail_time = {1} WHERE mail_id = {0} AND mail_read < 2;", MailID, MailTime))
    End Sub
    Public Sub On_MSG_QUERY_NEXT_MAIL_TIME(ByRef packet As PacketClass, ByRef Client As ClientClass)
        Log.WriteLine(LogType.DEBUG, "[{0}:{1}] MSG_QUERY_NEXT_MAIL_TIME", Client.IP, Client.Port)

        Dim MySQLQuery As New DataTable
        CharacterDatabase.Query(String.Format("SELECT COUNT(*) FROM characters_mail WHERE mail_read = 0 AND mail_receiver = {0} AND mail_time > {1};", Client.Character.GUID, GetTimestamp(Now)), MySQLQuery)
        If MySQLQuery.Rows(0).Item(0) > 0 Then
            Dim response As New PacketClass(OPCODES.MSG_QUERY_NEXT_MAIL_TIME)
            response.AddInt32(0)
            Client.Send(response)
            response.Dispose()
        Else
            Dim response As New PacketClass(OPCODES.MSG_QUERY_NEXT_MAIL_TIME)
            response.AddInt8(0)
            response.AddInt8(&HC0)
            response.AddInt8(&HA8)
            response.AddInt8(&HC7)
            Client.Send(response)
            response.Dispose()
        End If
    End Sub
    Public Sub On_CMSG_GET_MAIL_LIST(ByRef packet As PacketClass, ByRef Client As ClientClass)
        If (packet.Data.Length - 1) < 13 Then Exit Sub
        packet.GetInt16()
        Dim GameObjectGUID As ULong = packet.GetUInt64

        Log.WriteLine(LogType.DEBUG, "[{0}:{1}] CMSG_GET_MAIL_LIST [GUID={2:X}]", Client.IP, Client.Port, GameObjectGUID)

        Try
            'Done: Check for old mails, and delete those that have expired
            Dim MySQLQuery As New DataTable
            Dim i As Byte = 0
            CharacterDatabase.Query(String.Format("SELECT mail_id FROM characters_mail WHERE mail_time < {0};", GetTimestamp(Now)), MySQLQuery)
            If MySQLQuery.Rows.Count > 0 Then
                For i = 0 To MySQLQuery.Rows.Count - 1
                    CharacterDatabase.Update(String.Format("DELETE FROM characters_mail WHERE mail_id = {0};", MySQLQuery.Rows(i).Item("mail_id")))
                Next
            End If

            CharacterDatabase.Query(String.Format("SELECT * FROM characters_mail WHERE mail_receiver = {0};", Client.Character.GUID), MySQLQuery)

            Dim response As New PacketClass(OPCODES.SMSG_MAIL_LIST_RESULT)
            response.AddInt8(MySQLQuery.Rows.Count)

            Dim tmpItem As ItemObject
            If MySQLQuery.Rows.Count > 0 Then
                For i = 0 To MySQLQuery.Rows.Count - 1
                    response.AddInt16(&H40) 'Unknown, came with 2.3.0
                    response.AddInt32(CType(MySQLQuery.Rows(i).Item("mail_id"), Integer))
                    response.AddInt8(CType(MySQLQuery.Rows(i).Item("mail_type"), Byte))

                    Select Case CType(MySQLQuery.Rows(i).Item("mail_type"), Byte)
                        Case MailTypeInfo.NORMAL
                            response.AddUInt64(CType(MySQLQuery.Rows(i).Item("mail_sender"), Long))
                        Case MailTypeInfo.AUCTION
                            response.AddInt32(CType(MySQLQuery.Rows(i).Item("mail_sender"), Integer)) 'creature/gameobject entry, auction id
                    End Select

                    response.AddInt32(MySQLQuery.Rows(i).Item("mail_COD"))      'Money as COD

                    If MySQLQuery.Rows(i).Item("mail_body") <> "" Then
                        response.AddInt32(MySQLQuery.Rows(i).Item("mail_id")) 'ItemtextID?
                    Else
                        response.AddInt32(0)
                    End If

                    response.AddInt32(0) '2  = Gift
                    response.AddInt32(MySQLQuery.Rows(i).Item("mail_stationary")) '41/62 = Mail Background
                    response.AddInt32(MySQLQuery.Rows(i).Item("mail_money"))    'Money on delivery
                    If CType(MySQLQuery.Rows(i).Item("mail_type"), Byte) = MailTypeInfo.AUCTION Then
                        response.AddInt32(&H4)
                    Else
                        response.AddInt32(&H10)
                    End If
                    response.AddSingle(((CType(MySQLQuery.Rows(i).Item("mail_time"), Integer) - GetTimestamp(Now)) / 86400))
                    response.AddInt32(0) 'Mail template ID
                    response.AddString(MySQLQuery.Rows(i).Item("mail_subject"))

                    CharacterDatabase.Query(String.Format("SELECT item_guid FROM mail_items WHERE mail_id = {0};", MySQLQuery.Rows(i).Item("mail_id")), MySQLQuery)
                    response.AddInt8(CType(MySQLQuery.Rows.Count, Byte))
                    If MySQLQuery.Rows.Count > 0 Then
                        Dim j As Byte, k As Byte
                        For j = 0 To MySQLQuery.Rows.Count - 1
                            response.AddInt8(j)
                            tmpItem = LoadItemByGUID(CType(MySQLQuery.Rows(j).Item("item_guid"), Long) - GUID_ITEM)
                            response.AddInt32(CType(CType(MySQLQuery.Rows(j).Item("item_guid"), Long) - GUID_ITEM, Integer))
                            response.AddInt32(tmpItem.ItemEntry)

                            For k = 0 To 5
                                response.AddInt32(0) 'Enchantment Charges
                                response.AddInt32(0) 'Enchantment Duration
                                response.AddInt32(0) 'Enchantment ID
                            Next

                            response.AddInt32(tmpItem.RandomProperties)                 'Item random property
                            response.AddInt32(0)                                        'Item suffix factor
                            response.AddInt8(tmpItem.StackCount)                        'Item cout
                            response.AddInt32(tmpItem.ChargesLeft)                      'Spell Charges
                            response.AddInt32(tmpItem.Durability)                       'Durability Max
                            response.AddInt32(tmpItem.ItemInfo.Durability)              'Durability Min
                        Next
                    End If
                    i += 1
                Next
            End If

            Client.Send(response)
            response.Dispose()

        Catch e As Exception
            Log.WriteLine(LogType.FAILED, "Error getting mail list: {0}{1}", vbNewLine, e.ToString)
        End Try
    End Sub
    Public Sub On_CMSG_MAIL_TAKE_ITEM(ByRef packet As PacketClass, ByRef Client As ClientClass)
        If (packet.Data.Length - 1) < 21 Then Exit Sub
        packet.GetInt16()
        Dim GameObjectGUID As ULong = packet.GetUInt64
        Dim MailID As Integer = packet.GetInt32
        Dim ItemGUID As Integer = packet.GetInt32 'Low guid

        Log.WriteLine(LogType.DEBUG, "[{0}:{1}] CMSG_MAIL_TAKE_ITEM [MailID={2} ItemGUID={3}]", Client.IP, Client.Port, MailID, ItemGUID)

        Try
            'DONE: Check if it's the receiver that is trying to get the item
            Dim MySQLQuery As New DataTable
            CharacterDatabase.Query(String.Format("SELECT mail_cod, mail_sender FROM characters_mail WHERE mail_id = {0} AND mail_receiver = {1};", MailID, Client.Character.GUID), MySQLQuery)
            If MySQLQuery.Rows.Count = 0 Then 'The mail didn't exit, wrong owner trying to get someone elses item?
                Dim response As New PacketClass(OPCODES.SMSG_SEND_MAIL_RESULT)
                response.AddInt32(MailID)
                response.AddInt32(MailResult.MAIL_ITEM_REMOVED)
                response.AddInt32(MailSentError.INTERNAL_ERROR)
                Client.Send(response)
                response.Dispose()
                Exit Sub
            End If

            'DONE: Check for COD
            If MySQLQuery.Rows(0).Item("mail_cod") <> 0 Then
                If Client.Character.Copper < MySQLQuery.Rows(0).Item("mail_cod") Then
                    Dim noMoney As New PacketClass(OPCODES.SMSG_SEND_MAIL_RESULT)
                    noMoney.AddInt32(MailID)
                    noMoney.AddInt32(MailResult.MAIL_SENT)
                    noMoney.AddInt32(MailSentError.NOT_ENOUGHT_MONEY)
                    Client.Send(noMoney)
                    noMoney.Dispose()
                    Exit Sub
                Else
                    'DONE: Pay COD and save
                    Client.Character.Copper -= MySQLQuery.Rows(0).Item("mail_cod")
                    CharacterDatabase.Update(String.Format("UPDATE characters_mail SET mail_cod = 0 WHERE mail_id = {0};", MailID))

                    'DONE: Send COD to sender
                    'TODO: Edit text to be more blizzlike
                    Dim MailTime As Integer = GetTimestamp(Now) + (86400 * 30) 'Set expiredate to today + 30 days
                    CharacterDatabase.Update(String.Format("INSERT INTO characters_mail (mail_sender, mail_receiver, mail_subject, mail_body,  mail_item_guid, mail_money, mail_COD, mail_time, mail_read, mail_type) VALUES ({0},{1},'{2}','{3}',{4},{5},{6},{7},{8},{9});", Client.Character.GUID, MySQLQuery.Rows(0).Item("mail_sender"), "", "", 0, MySQLQuery.Rows(0).Item("mail_cod"), 0, MailTime, MailReadInfo.COD, 0))
                End If
            End If

            'DONE: Get Item
            CharacterDatabase.Query(String.Format("SELECT item_guid FROM mail_items WHERE mail_id = {0} AND item_guid = {1};", MailID, ItemGUID + GUID_ITEM), MySQLQuery)
            If MySQLQuery.Rows.Count = 0 Then 'The item doesn't exist?
                Dim response As New PacketClass(OPCODES.SMSG_SEND_MAIL_RESULT)
                response.AddInt32(MailID)
                response.AddInt32(MailResult.MAIL_ITEM_REMOVED)
                response.AddInt32(MailSentError.INTERNAL_ERROR)
                Client.Send(response)
                response.Dispose()
                Exit Sub
            End If

            Dim tmpItem As ItemObject = LoadItemByGUID(CType(MySQLQuery.Rows(0).Item("item_guid"), Long) - GUID_ITEM)
            tmpItem.OwnerGUID = Client.Character.GUID
            tmpItem.Save()

            'DONE: Send error message if no slots
            If Client.Character.ItemADD(tmpItem) Then
                CharacterDatabase.Update(String.Format("DELETE FROM mail_items WHERE mail_id = {0} AND item_guid = {1};", MailID, MySQLQuery.Rows(0).Item("item_guid")))

                Dim response As New PacketClass(OPCODES.SMSG_SEND_MAIL_RESULT)
                response.AddInt32(MailID)
                response.AddInt32(MailResult.MAIL_ITEM_REMOVED)
                response.AddInt32(MailSentError.NO_ERROR)
                response.AddInt32(ItemGUID)
                response.AddInt32(tmpItem.StackCount)
                Client.Send(response)
                response.Dispose()
            Else
                tmpItem.Dispose()

                Dim response As New PacketClass(OPCODES.SMSG_SEND_MAIL_RESULT)
                response.AddInt32(MailID)
                response.AddInt32(MailResult.MAIL_ITEM_REMOVED)
                response.AddInt32(MailSentError.BAG_FULL)
                Client.Send(response)
                response.Dispose()
            End If
            Client.Character.Save()

        Catch e As Exception
            Log.WriteLine(LogType.FAILED, "Error getting item from mail: {0}{1}", vbNewLine, e.ToString)
        End Try
    End Sub
    Public Sub On_CMSG_MAIL_TAKE_MONEY(ByRef packet As PacketClass, ByRef Client As ClientClass)
        If (packet.Data.Length - 1) < 17 Then Exit Sub
        packet.GetInt16()
        Dim GameObjectGUID As ULong = packet.GetUInt64
        Dim MailID As Integer = packet.GetInt32

        Log.WriteLine(LogType.DEBUG, "[{0}:{1}] CMSG_MAIL_TAKE_MONEY [MailID={2}]", Client.IP, Client.Port, MailID)

        Dim MySQLQuery As New DataTable
        CharacterDatabase.Query(String.Format("SELECT mail_money FROM characters_mail WHERE mail_id = {0}; UPDATE characters_mail SET mail_money = 0 WHERE mail_id = {0};", MailID), MySQLQuery)
        Client.Character.Copper += MySQLQuery.Rows(0).Item("mail_money")
        Client.Character.SetUpdateFlag(EPlayerFields.PLAYER_FIELD_COINAGE, Client.Character.Copper)

        Dim response As New PacketClass(OPCODES.SMSG_SEND_MAIL_RESULT)
        response.AddInt32(MailID)
        response.AddInt32(MailResult.MAIL_MONEY_REMOVED)
        response.AddInt32(0)
        Client.Send(response)
        response.Dispose()

        Client.Character.SaveCharacter()
    End Sub
    Public Sub On_CMSG_ITEM_TEXT_QUERY(ByRef packet As PacketClass, ByRef Client As ClientClass)
        If (packet.Data.Length - 1) < 9 Then Exit Sub
        packet.GetInt16()
        Dim MailID As Integer = packet.GetInt32
        'Dim GameObjectGUID as ulong = packet.GetuInt64

        Log.WriteLine(LogType.DEBUG, "[{0}:{1}] CMSG_ITEM_TEXT_QUERY [MailID={2}]", Client.IP, Client.Port, MailID)

        Dim MySQLQuery As New DataTable
        CharacterDatabase.Query(String.Format("SELECT mail_body FROM characters_mail WHERE mail_id = {0};", MailID), MySQLQuery)

        Dim response As New PacketClass(OPCODES.SMSG_ITEM_TEXT_QUERY_RESPONSE)
        response.AddInt32(MailID)
        response.AddString(MySQLQuery.Rows(0).Item("mail_body"))
        Client.Send(response)
        response.Dispose()
    End Sub
    Public Sub On_CMSG_MAIL_CREATE_TEXT_ITEM(ByRef packet As PacketClass, ByRef Client As ClientClass)
        If (packet.Data.Length - 1) < 9 Then Exit Sub
        packet.GetInt16()
        Dim MailID As Integer = packet.GetInt32
        'Dim GameObjectGUID as ulong = packet.GetuInt64

        Log.WriteLine(LogType.DEBUG, "[{0}:{1}] CMSG_MAIL_CREATE_TEXT_ITEM [MailID={2}]", Client.IP, Client.Port, MailID)

        'DONE: Create Item with ITEM_FIELD_ITEM_TEXT_ID = MailID
        Dim tmpItem As New ItemObject(ITEM_MAILTEXT_ITEMID, Client.Character.GUID)
        tmpItem.ItemText = MailID
        If Not Client.Character.ItemADD(tmpItem) Then
            Dim response As New PacketClass(OPCODES.SMSG_ITEM_TEXT_QUERY_RESPONSE)
            response.AddInt32(MailID)
            response.AddInt32(0)
            response.AddInt32(1)
            Client.Send(response)
            response.Dispose()

            tmpItem.Delete()
        Else
            Client.Character.SendItemUpdate(tmpItem)
        End If
    End Sub
    Public Sub On_CMSG_SEND_MAIL(ByRef packet As PacketClass, ByRef Client As ClientClass)
        If (packet.Data.Length - 1) < 14 Then Exit Sub
        packet.GetInt16()
        Dim GameObjectGUID As ULong = packet.GetUInt64
        Dim Receiver As String = packet.GetString
        If (packet.Data.Length - 1) < (14 + Receiver.Length + 1) Then Exit Sub
        Dim Subject As String = packet.GetString
        If (packet.Data.Length - 1) < (14 + Receiver.Length + 2 + Subject.Length) Then Exit Sub
        Dim Body As String = packet.GetString
        If (packet.Data.Length - 1) < (14 + Receiver.Length + 2 + Subject.Length + Body.Length + 4 + 4 + 1) Then Exit Sub
        packet.GetInt32()
        packet.GetInt32()
        Dim ItemCount As Byte = packet.GetInt8
        If ItemCount > 12 Then Exit Sub 'Client limit
        Dim i As Byte, ItemSlot As Byte, ItemGUID As ULong
        Dim j As Byte = 1
        Dim Items As New Hashtable

        Try
            If ItemCount > 0 Then
                For i = 1 To ItemCount
                    ItemSlot = packet.GetInt8
                    ItemGUID = packet.GetInt64
                    'We don't want duplicate cheaters here!
                    If Items.ContainsValue(ItemGUID) = False Then
                        Items.Add(j, ItemGUID)
                        j += 1
                    End If
                Next
                ItemCount = Items.Count 'The real count after the duplacates were removed
            End If

            Dim money As Integer = packet.GetInt32
            Dim COD As Integer = packet.GetInt32

            Log.WriteLine(LogType.DEBUG, "[{0}:{1}] CMSG_SEND_MAIL [Receiver={2} Subject={3}]", Client.IP, Client.Port, Receiver, Subject)

            Dim MySQLQuery As New DataTable
            CharacterDatabase.Query("SELECT char_guid, char_race FROM characters WHERE char_name LIKE '" & Receiver & "';", MySQLQuery)

            If MySQLQuery.Rows.Count = 0 Then
                Dim response As New PacketClass(OPCODES.SMSG_SEND_MAIL_RESULT)
                response.AddInt32(0)
                response.AddInt32(MailResult.MAIL_SENT)
                response.AddInt32(MailSentError.CHARACTER_NOT_FOUND)
                Client.Send(response)
                response.Dispose()
                Exit Sub
            End If
            Dim ReceiverGUID As ULong = MySQLQuery.Rows(0).Item("char_guid")
            Dim ReceiverSide As Boolean = GetCharacterSide(CType(MySQLQuery.Rows(0).Item("char_race"), Byte))

            If Client.Character.GUID = ReceiverGUID Then
                Dim response As New PacketClass(OPCODES.SMSG_SEND_MAIL_RESULT)
                response.AddInt32(0)
                response.AddInt32(MailResult.MAIL_SENT)
                response.AddInt32(MailSentError.CANNOT_SEND_TO_SELF)
                Client.Send(response)
                response.Dispose()
                Exit Sub
            End If

            If (Client.Character.Copper - money) < (30 + (30 * (ItemCount - 1))) Then 'The first item is free
                Dim response As New PacketClass(OPCODES.SMSG_SEND_MAIL_RESULT)
                response.AddInt32(0)
                response.AddInt32(MailResult.MAIL_SENT)
                response.AddInt32(MailSentError.NOT_ENOUGHT_MONEY)
                Client.Send(response)
                response.Dispose()
                Exit Sub
            End If

            'Lets check so that the receiver doesn't have a full inbox
            CharacterDatabase.Query(String.Format("SELECT mail_id FROM characters_mail WHERE mail_receiver = {0}", ReceiverGUID), MySQLQuery)
            If MySQLQuery.Rows.Count >= 100 Then
                Dim response As New PacketClass(OPCODES.SMSG_SEND_MAIL_RESULT)
                response.AddInt32(0)
                response.AddInt32(MailResult.MAIL_SENT)
                response.AddInt32(MailSentError.INTERNAL_ERROR)
                Client.Send(response)
                response.Dispose()
                Exit Sub
            End If

            'You can only send mails to characters with your same faction, but GMs can ofc
            If Client.Access >= AccessLevel.GameMaster AndAlso Client.Character.Side <> ReceiverSide Then
                Dim response As New PacketClass(OPCODES.SMSG_SEND_MAIL_RESULT)
                response.AddInt32(0)
                response.AddInt32(MailResult.MAIL_SENT)
                response.AddInt32(MailSentError.NOT_YOUR_ALLIANCE)
                Client.Send(response)
                response.Dispose()
                Exit Sub
            End If

            Client.Character.Copper -= (30 + (30 * (ItemCount - 1)) + money)
            Client.Character.SetUpdateFlag(EPlayerFields.PLAYER_FIELD_COINAGE, Client.Character.Copper)

            Dim MailTime As Integer = GetTimestamp(Now) + (86400 * 30) 'Add 30 days to the current date/time
            CharacterDatabase.Update(String.Format("INSERT INTO characters_mail (mail_sender, mail_receiver, mail_subject, mail_body, mail_money, mail_COD, mail_time, mail_read, mail_type, mail_stationary) VALUES ({0},{1},'{2}','{3}',{4},{5},{6},{7},{8},41);", Client.Character.GUID, ReceiverGUID, Subject.Replace("'", "`"), Body.Replace("'", "`"), money, COD, MailTime, CType(MailReadInfo.Unread, Byte), 0))

            'Now get the mail we just inserted into the database
            CharacterDatabase.Query(String.Format("SELECT mail_id FROM characters_mail WHERE mail_sender = {0} AND mail_time = {1}", Client.Character.GUID, MailTime), MySQLQuery)
            If MySQLQuery.Rows.Count = 0 Then 'Somehow the mail wasn't even saved in the DB
                Dim response As New PacketClass(OPCODES.SMSG_SEND_MAIL_RESULT)
                response.AddInt32(0)
                response.AddInt32(MailResult.MAIL_SENT)
                response.AddInt32(MailSentError.INTERNAL_ERROR)
                Client.Send(response)
                response.Dispose()
                Exit Sub
            End If

            'And now we insert all of the items into the database
            Dim MailID As Integer = MySQLQuery.Rows(0).Item("mail_id")
            If ItemCount > 0 Then
                For i = 1 To ItemCount
                    Client.Character.ItemREMOVE(CType(Items(i), Long), False, True)
                    CharacterDatabase.Update(String.Format("INSERT INTO mail_items (mail_id, item_guid) VALUES ({0},{1});", MailID, CType(Items(i), Long)))
                Next
            End If

            'Tell the client we succeeded
            Dim sendOK As New PacketClass(OPCODES.SMSG_SEND_MAIL_RESULT)
            sendOK.AddInt32(0)
            sendOK.AddInt32(MailResult.MAIL_SENT)
            sendOK.AddInt32(MailSentError.NO_ERROR)
            Client.Send(sendOK)
            sendOK.Dispose()

            CHARACTERs_Lock.AcquireReaderLock(DEFAULT_LOCK_TIMEOUT)
            If CHARACTERs.ContainsKey(ReceiverGUID) Then
                Dim response As New PacketClass(OPCODES.SMSG_RECEIVED_MAIL)
                response.AddInt32(0)
                CHARACTERs(ReceiverGUID).Client.Send(response)
                response.Dispose()
            End If
            CHARACTERs_Lock.ReleaseReaderLock()
        Catch e As Exception
            Log.WriteLine(LogType.FAILED, "Error sending mail: {0}{1}", vbNewLine, e.ToString)
        End Try
    End Sub

    Public Sub SendNotify(ByRef client As ClientClass)
        Dim packet As New PacketClass(OPCODES.SMSG_RECEIVED_MAIL)
        packet.GetInt32(0)
        client.Send(packet)
    End Sub

#End Region

End Module