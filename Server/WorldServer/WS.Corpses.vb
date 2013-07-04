' 
' Copyright (C) 2008 Spurious <http://SpuriousEmu.com>
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
Imports Spurious.Common.BaseWriter

Public Module WS_Corpses
    'WARNING: Use only with WORLD_GAMEOBJECTs()
    Public Class CorpseObject
        Inherits BaseObject
        Implements IDisposable

        Public DynFlags As Integer = 0
        Public Flags As Integer = 0
        Public Owner As ULong = 0
        Public Bytes1 As Integer = 0
        Public Bytes2 As Integer = 0
        Public Model As Integer = 0
        Public Guild As Integer = 0
        Public Items(EQUIPMENT_SLOT_END - 1) As Integer

        Public Sub FillAllUpdateFlags(ByRef Update As UpdateClass, ByRef Character As CharacterObject)
            Update.SetUpdateFlag(EObjectFields.OBJECT_FIELD_GUID, GUID)
            Update.SetUpdateFlag(EObjectFields.OBJECT_FIELD_TYPE, CType(ObjectType.TYPE_CORPSE + ObjectType.TYPE_OBJECT, Integer))
            Update.SetUpdateFlag(EObjectFields.OBJECT_FIELD_ENTRY, 0)
            Update.SetUpdateFlag(EObjectFields.OBJECT_FIELD_SCALE_X, 1.0F)

            Update.SetUpdateFlag(ECorpseFields.CORPSE_FIELD_OWNER, Owner)
            Update.SetUpdateFlag(ECorpseFields.CORPSE_FIELD_FACING, orientation)
            Update.SetUpdateFlag(ECorpseFields.CORPSE_FIELD_POS_X, positionX)
            Update.SetUpdateFlag(ECorpseFields.CORPSE_FIELD_POS_Y, positionY)
            Update.SetUpdateFlag(ECorpseFields.CORPSE_FIELD_POS_Z, positionZ)
            Update.SetUpdateFlag(ECorpseFields.CORPSE_FIELD_DISPLAY_ID, Model)

            Dim i As Integer
            For i = 0 To EQUIPMENT_SLOT_END - 1
                Update.SetUpdateFlag(ECorpseFields.CORPSE_FIELD_ITEM + i, Items(i))
            Next

            Update.SetUpdateFlag(ECorpseFields.CORPSE_FIELD_BYTES_1, Bytes1)
            Update.SetUpdateFlag(ECorpseFields.CORPSE_FIELD_BYTES_2, Bytes2)
            Update.SetUpdateFlag(ECorpseFields.CORPSE_FIELD_GUILD, Guild)
            Update.SetUpdateFlag(ECorpseFields.CORPSE_FIELD_FLAGS, Flags)
            Update.SetUpdateFlag(ECorpseFields.CORPSE_FIELD_DYNAMIC_FLAGS, DynFlags)

        End Sub

        Public Sub ConvertToBones()
            'DONE: Delete from database
            Database.Update(String.Format("DELETE FROM tmpspawnedcorpses WHERE corpse_owner = ""{0}"";", Owner))

            Flags = 5
            Owner = 0
            Dim i As Integer
            For i = 0 To EQUIPMENT_SLOT_END - 1
                Items(i) = 0
            Next

            Dim packet As New PacketClass(OPCODES.SMSG_UPDATE_OBJECT)
            packet.AddInt32(1)
            packet.AddInt8(0)
            Dim tmpUpdate As New UpdateClass(FIELD_MASK_SIZE_CORPSE)
            tmpUpdate.SetUpdateFlag(ECorpseFields.CORPSE_FIELD_OWNER, 0)
            tmpUpdate.SetUpdateFlag(ECorpseFields.CORPSE_FIELD_FLAGS, 5)
            For i = 0 To EQUIPMENT_SLOT_END - 1
                tmpUpdate.SetUpdateFlag(ECorpseFields.CORPSE_FIELD_ITEM + i, 0)
            Next
            tmpUpdate.AddToPacket(packet, ObjectUpdateType.UPDATETYPE_VALUES, Me, 0)

            SendToNearPlayers(packet)

            tmpUpdate.Dispose()
            packet.Dispose()
        End Sub
        Public Sub Save()
            'Only for creating New Character
            Dim tmpCMD As String = "INSERT INTO tmpspawnedcorpses (corpse_guid"
            Dim tmpValues As String = " VALUES (" & (GUID - GUID_CORPSE)

            tmpCMD = tmpCMD & ", corpse_owner"
            tmpValues = tmpValues & ", " & Owner

            tmpCMD = tmpCMD & ", corpse_positionX"
            tmpValues = tmpValues & ", " & Trim(Str(positionX))
            tmpCMD = tmpCMD & ", corpse_positionY"
            tmpValues = tmpValues & ", " & Trim(Str(positionY))
            tmpCMD = tmpCMD & ", corpse_positionZ"
            tmpValues = tmpValues & ", " & Trim(Str(positionZ))
            tmpCMD = tmpCMD & ", corpse_mapid"
            tmpValues = tmpValues & ", " & MapID
            tmpCMD = tmpCMD & ", corpse_orientation"
            tmpValues = tmpValues & ", " & Trim(Str(orientation))

            tmpCMD = tmpCMD & ", corpse_bytes1"
            tmpValues = tmpValues & ", " & Bytes1
            tmpCMD = tmpCMD & ", corpse_bytes2"
            tmpValues = tmpValues & ", " & Bytes2
            tmpCMD = tmpCMD & ", corpse_model"
            tmpValues = tmpValues & ", " & Model
            tmpCMD = tmpCMD & ", corpse_guild"
            tmpValues = tmpValues & ", " & Guild

            Dim i As Byte
            Dim temp(EQUIPMENT_SLOT_END - 1) As String
            For i = 0 To EQUIPMENT_SLOT_END - 1
                temp(i) = Items(i)
            Next
            tmpCMD = tmpCMD & ", corpse_items"
            tmpValues = tmpValues & ", """ & Join(temp, " ") & """"



            tmpCMD = tmpCMD & ") " & tmpValues & ");"
            Database.Update(tmpCMD)
        End Sub
        Public Sub Destroy()
            Dim packet As New PacketClass(OPCODES.SMSG_DESTROY_OBJECT)
            packet.AddUInt64(GUID)
            SendToNearPlayers(packet)
            packet.Dispose()

            Me.Dispose()
        End Sub
        Private Sub Dispose() Implements System.IDisposable.Dispose
            Me.RemoveFromWorld()
            WORLD_CORPSEOBJECTs.Remove(GUID)
        End Sub
        Public Sub New(ByRef Character As CharacterObject)
            'WARNING: Use only for spawning new object
            GUID = GetNewGUID()
            Bytes1 = (CType(Character.Race, Integer) << 8) + (CType(Character.Skin, Integer) << 24)
            Bytes2 = Character.Face + (CType(Character.HairStyle, Integer) << 8) + (CType(Character.HairColor, Integer) << 16) + (CType(Character.FacialHair, Integer) << 24)
            Model = Character.Model
            positionX = Character.positionX
            positionY = Character.positionY
            positionZ = Character.positionZ
            orientation = Character.orientation
            MapID = Character.MapID
            Owner = Character.GUID

            Character.corpseGUID = GUID
            Character.corpsePositionX = positionX
            Character.corpsePositionY = positionY
            Character.corpsePositionZ = positionZ
            Character.corpseMapID = MapID

            Dim i As Byte
            For i = 0 To EQUIPMENT_SLOT_END - 1
                If Character.Items.ContainsKey(i) Then
                    Items(i) = Character.Items(i).ItemInfo.Model + (CType(Character.Items(i).ItemInfo.InventoryType, Integer) << 24)
                Else
                    Items(i) = 0
                End If
            Next

            Flags = 4

            WORLD_CORPSEOBJECTs.Add(GUID, Me)
        End Sub
        Public Sub New(ByVal cGUID As ULong, Optional ByRef Info As DataRow = Nothing)
            'WARNING: Use only for loading from DB
            If Info Is Nothing Then
                Dim MySQLQuery As New DataTable
                Database.Query(String.Format("SELECT * FROM tmpspawnedcorpses WHERE corpse_guid = {0};", cGUID), MySQLQuery)
                If MySQLQuery.Rows.Count > 0 Then
                    Info = MySQLQuery.Rows(0)
                Else
                    Log.WriteLine(LogType.FAILED, "Corpse not found in database. [corpseGUID={0:X}]", cGUID)
                    Return
                End If
            End If

            positionX = Info.Item("corpse_positionX")
            positionY = Info.Item("corpse_positionY")
            positionZ = Info.Item("corpse_positionZ")
            orientation = Info.Item("corpse_orientation")

            MapID = Info.Item("corpse_mapId")

            Owner = Info.Item("corpse_owner")
            Bytes1 = Info.Item("corpse_bytes1")
            Bytes2 = Info.Item("corpse_bytes2")
            Model = Info.Item("corpse_model")
            Guild = Info.Item("corpse_guild")

            Dim tmp() As String
            Dim i As Integer
            tmp = Split(CType(Info.Item("corpse_items"), String), " ")
            For i = 0 To tmp.Length - 1
                Items(i) = tmp(i)
            Next i

            Flags = 4

            GUID = cGUID + GUID_CORPSE
            WORLD_CORPSEOBJECTs.Add(GUID, Me)
        End Sub
        Public Sub AddToWorld()
            GetMapTile(positionX, positionY, CellX, CellY)
            If Maps(MapID).Tiles(CellX, CellY) Is Nothing Then MAP_Load(CellX, CellY, MapID)
            Maps(MapID).Tiles(CellX, CellY).CorpseObjectsHere.Add(GUID)

            Dim list() As ULong

                        'DONE: Sending to players in nearby cells
            Dim packet As New PacketClass(OPCODES.SMSG_UPDATE_OBJECT)
            packet.AddInt32(1)
            'packet.AddInt8(0)
            Dim tmpUpdate As New UpdateClass(FIELD_MASK_SIZE_CORPSE)
            FillAllUpdateFlags(tmpUpdate, Nothing)
            tmpUpdate.AddToPacket(packet, ObjectUpdateType.UPDATETYPE_CREATE_OBJECT, Me)
            tmpUpdate.Dispose()

            For i As Short = -1 To 1
                For j As Short = -1 To 1
                    If (CellX + i) >= 0 AndAlso (CellX + i) <= 63 AndAlso (CellY + j) >= 0 AndAlso (CellY + j) <= 63 AndAlso Maps(MapID).Tiles(CellX + i, CellY + j) IsNot Nothing AndAlso Maps(MapID).Tiles(CellX + i, CellY + j).PlayersHere.Count > 0 Then
                        With Maps(MapID).Tiles(CellX + i, CellY + j)
                            list = .PlayersHere.ToArray
                            For Each plGUID As ULong In list
                                If CHARACTERs.ContainsKey(plGUID) AndAlso CHARACTERs(plGUID).CanSee(Me) Then
                                    CHARACTERs(plGUID).Client.SendMultiplyPackets(packet)
                                    CHARACTERs(plGUID).corpseObjectsNear.Add(GUID)
                                    SeenBy.Add(plGUID)
                                End If
                            Next
                        End With
                    End If
                Next
            Next

            packet.Dispose()

        End Sub
        Public Sub RemoveFromWorld()
            GetMapTile(positionX, positionY, CellX, CellY)
            Maps(MapID).Tiles(CellX, CellY).CorpseObjectsHere.Remove(GUID)

            Dim list() As ULong

            'DONE: Removing from players in <CENTER> Cell wich can see it
            If Maps(MapID).Tiles(CellX, CellY).PlayersHere.Count > 0 Then
                With Maps(MapID).Tiles(CellX, CellY)
                    list = .PlayersHere.ToArray
                    For Each plGUID As ULong In list
                        If CHARACTERs(plGUID).corpseObjectsNear.Contains(GUID) Then
                            CHARACTERs(plGUID).guidsForRemoving_Lock.AcquireWriterLock(DEFAULT_LOCK_TIMEOUT)
                            CHARACTERs(plGUID).guidsForRemoving.Add(GUID)
                            CHARACTERs(plGUID).guidsForRemoving_Lock.ReleaseWriterLock()

                            CHARACTERs(plGUID).corpseObjectsNear.Remove(GUID)
                        End If
                    Next
                End With
            End If
        End Sub
    End Class

    <MethodImplAttribute(MethodImplOptions.Synchronized)> _
    Private Function GetNewGUID() As ULong
        CorpseGUIDCounter += 1
        GetNewGUID = CorpseGUIDCounter
    End Function

End Module
