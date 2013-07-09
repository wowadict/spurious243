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

Module WS_CharMovement

#Region "WS.CharacterMovement.MovementHandlers"

    Public Enum MovementFlags As Integer
        MOVEMENTFLAG_NONE = &H0
        MOVEMENTFLAG_FORWARD = &H1
        MOVEMENTFLAG_BACKWARD = &H2
        MOVEMENTFLAG_STRAFE_LEFT = &H4
        MOVEMENTFLAG_STRAFE_RIGHT = &H8
        MOVEMENTFLAG_LEFT = &H10
        MOVEMENTFLAG_RIGHT = &H20
        MOVEMENTFLAG_PITCH_UP = &H40
        MOVEMENTFLAG_PITCH_DOWN = &H80
        MOVEMENTFLAG_WALK = &H100
        MOVEMENTFLAG_ONTRANSPORT = &H200
        MOVEMENTFLAG_UNK1 = &H400
        MOVEMENTFLAG_FLY_UNK1 = &H800
        MOVEMENTFLAG_JUMPING = &H1000
        MOVEMENTFLAG_UNK4 = &H2000
        MOVEMENTFLAG_FALLING = &H4000
        MOVEMENTFLAG_SWIMMING = &H200000 ' appears with fly flag also
        MOVEMENTFLAG_FLY_UP = &H400000
        MOVEMENTFLAG_CAN_FLY = &H800000
        MOVEMENTFLAG_FLYING = &H1000000
        MOVEMENTFLAG_FLYING2 = &H2000000
        MOVEMENTFLAG_SPLINE = &H4000000 ' probably wrong name
        MOVEMENTFLAG_SPLINE2 = &H8000000
        MOVEMENTFLAG_WATERWALKING = &H10000000
        MOVEMENTFLAG_SAFE_FALL = &H20000000 ' active rogue safe fall spell (passive)
        MOVEMENTFLAG_UNK3 = &H40000000
    End Enum

    Public Sub OnMovementPacket(ByRef packet As PacketClass, ByRef Client As ClientClass)
        packet.GetInt16()

        Client.Character.movementFlags = packet.GetInt32()
        packet.GetInt8()
        Dim time As Integer = packet.GetInt32()
        Client.Character.positionX = packet.GetFloat()
        Client.Character.positionY = packet.GetFloat()
        Client.Character.positionZ = packet.GetFloat()
        Client.Character.orientation = packet.GetFloat()

#If DEBUG Then
        'Log.WriteLine(LogType.NETWORK, "[{0}:{1}] {2} [0x{3:X} {4} {5} {6} {7}]", Client.IP, Client.Port, packet.OpCode, Client.Character.movementFlags, Client.Character.positionX, Client.Character.positionY, Client.Character.positionZ, Client.Character.orientation)
#End If

#If ENABLE_PPOINTS Then
        If (Client.Character.movementFlags And groundFlagsMask) = 0 AndAlso _
           Math.Abs(GetZCoord(Client.Character.positionX, Client.Character.positionY, Client.Character.positionZ, Client.Character.MapID) - Client.Character.positionZ) > PPOINT_LIMIT Then
            Log.WriteLine(LogType.DEBUG, "PPoints: {0} [MapZ = {1}]", Client.Character.positionZ, GetZCoord(Client.Character.positionX, Client.Character.positionY, Client.Character.MapID))
            SetZCoord_PP(Client.Character.positionX, Client.Character.positionY, Client.Character.MapID, Client.Character.positionZ)
        End If
#End If

        If HaveFlags(Client.Character.movementFlags, MovementFlags.MOVEMENTFLAG_ONTRANSPORT) Then
            Dim transportGUID As ULong = packet.GetUInt64
            Dim transportX As Single = packet.GetFloat
            Dim transportY As Single = packet.GetFloat
            Dim transportZ As Single = packet.GetFloat
            Dim transportO As Single = packet.GetFloat
        End If
        If HaveFlags(Client.Character.movementFlags, MovementFlags.MOVEMENTFLAG_SWIMMING) Then
            Dim swimAngle As Single = packet.GetFloat
            '#If DEBUG Then
            '                Console.WriteLine("[{0}] [{1}:{2}] Client swim angle:{3}", Format(TimeOfDay, "hh:mm:ss"), Client.IP, Client.Port, swimAngle)
            '#End If
        End If

        Dim fallTime As Integer = packet.GetInt32

        If HaveFlags(Client.Character.movementFlags, MovementFlags.MOVEMENTFLAG_JUMPING) Then
            Dim unk As Integer = packet.GetInt32
            Dim sinAngle As Single = packet.GetFloat
            Dim cosAngle As Single = packet.GetFloat
            Dim xySpeed As Single = packet.GetFloat
            '#If DEBUG Then
            '                Console.WriteLine("[{0}] [{1}:{2}] Client jump: 0x{3:X} {4} {5} {6}", Format(TimeOfDay, "hh:mm:ss"), Client.IP, Client.Port, unk, sinAngle, cosAngle, xySpeed)
            '#End If
        End If

        If HaveFlags(Client.Character.movementFlags, MovementFlags.MOVEMENTFLAG_SPLINE) Then
            Dim unk1 As Single = packet.GetFloat
        End If

        If Client.Character.exploreCheckQueued_ AndAlso (Not Client.Character.DEAD) Then
            Dim exploreFlag As Integer = GetAreaFlag(Client.Character.positionX, Client.Character.positionY, Client.Character.MapID)

            'DONE: Checking Explore System
            If exploreFlag <> &HFFFF Then
                Dim areaFlag As Integer = exploreFlag Mod 32
                Dim areaFlagOffset As Byte = exploreFlag \ 32

                If Not HaveFlag(Client.Character.ZonesExplored(areaFlagOffset), areaFlag) Then
                    SetFlag(Client.Character.ZonesExplored(areaFlagOffset), areaFlag, True)

                    Dim GainedXP As Integer = AreaTable(exploreFlag).Level * 10
                    Dim percent As Integer = 100 - (((CInt(Client.Character.Level) - CInt(AreaTable(exploreFlag).Level)) - 5) * 5)
                    If percent < 0 Then percent = 0
                    If percent > 100 Then percent = 100
                    GainedXP = (GainedXP * percent) / 100

                    Dim SMSG_EXPLORATION_EXPERIENCE As New PacketClass(OPCODES.SMSG_EXPLORATION_EXPERIENCE)
                    SMSG_EXPLORATION_EXPERIENCE.AddInt32(AreaTable(exploreFlag).ID)
                    SMSG_EXPLORATION_EXPERIENCE.AddInt32(GainedXP)
                    Client.Send(SMSG_EXPLORATION_EXPERIENCE)
                    SMSG_EXPLORATION_EXPERIENCE.Dispose()

                    Client.Character.SetUpdateFlag(EPlayerFields.PLAYER_EXPLORED_ZONES_1 + areaFlagOffset, Client.Character.ZonesExplored(areaFlagOffset))
                    Client.Character.AddXP(GainedXP, , False)

                    'DONE: Fire quest event to check for if this area is used in explore area quest
                    OnQuestExplore(Client.Character, exploreFlag)
                End If
            End If
        End If

        'No calculation anymore? :S
        packet.AddInt32(timeGetTime, 11)

        'DONE: Send to nearby players
        Dim response As New PacketClass(packet.OpCode)
        response.AddPackGUID(Client.Character.GUID)
        Dim tempArray(packet.Data.Length - 6) As Byte
        Array.Copy(packet.Data, 6, tempArray, 0, packet.Data.Length - 6)
        response.AddByteArray(tempArray)
        Client.Character.SendToNearPlayers(response)
        response.Dispose()

        'NOTE: They may slow the movement down so let's do them after the packet is sent
        'DONE: Remove auras that requires you to not move
        If Client.Character.isMoving Then
            Client.Character.RemoveAurasByInterruptFlag(SpellAuraInterruptFlags.AURA_INTERRUPT_FLAG_MOVE)
        End If
        'DONE: Remove auras that requires you to not turn
        If Client.Character.isTurning Then
            Client.Character.RemoveAurasByInterruptFlag(SpellAuraInterruptFlags.AURA_INTERRUPT_FLAG_TURNING)
        End If
    End Sub
    Public Sub OnStartSwim(ByRef packet As PacketClass, ByRef Client As ClientClass)

        If (Client.Character.underWaterTimer Is Nothing) AndAlso (Not Client.Character.underWaterBreathing) AndAlso (Not Client.Character.DEAD) Then
            Client.Character.underWaterTimer = New TDrowningTimer(Client.Character)
        End If

        OnMovementPacket(packet, Client)
    End Sub
    Public Sub OnStopSwim(ByRef packet As PacketClass, ByRef Client As ClientClass)

        If Not Client.Character.underWaterTimer Is Nothing Then
            Client.Character.underWaterTimer.Dispose()
            Client.Character.underWaterTimer = Nothing
        End If

        OnMovementPacket(packet, Client)
    End Sub

    Public Sub OnChangeSpeed(ByRef packet As PacketClass, ByRef Client As ClientClass)
        packet.GetInt16()
        Dim GUID As ULong = packet.GetUInt64

        If GUID <> Client.Character.GUID Then Exit Sub 'Skip it, it's not our packet

        packet.GetInt32()
        Dim flags As Integer = packet.GetInt32()
        packet.GetInt8()
        Dim time As Integer = packet.GetInt32()
        Client.Character.positionX = packet.GetFloat()
        Client.Character.positionY = packet.GetFloat()
        Client.Character.positionZ = packet.GetFloat()
        Client.Character.orientation = packet.GetFloat()

        If (flags And MovementFlags.MOVEMENTFLAG_ONTRANSPORT) Then
            packet.GetInt64()
            packet.GetFloat()
            packet.GetFloat()
            packet.GetFloat()
            packet.GetFloat()
            packet.GetInt32()
        End If
        If (flags And (MovementFlags.MOVEMENTFLAG_SWIMMING Or MovementFlags.MOVEMENTFLAG_FLYING2)) Then
            packet.GetFloat()
        End If

        Dim falltime As Single = packet.GetInt32()

        If (flags And MovementFlags.MOVEMENTFLAG_JUMPING) Or (flags And MovementFlags.MOVEMENTFLAG_FALLING) Then
            packet.GetFloat()
            packet.GetFloat()
            packet.GetFloat()
            packet.GetFloat()
        End If
        If (flags And MovementFlags.MOVEMENTFLAG_SPLINE) Then
            packet.GetFloat()
        End If

        Dim newSpeed As Single = packet.GetFloat()

        Log.WriteLine(LogType.DEBUG, "[{0}:{1}] {3} [{2}]", Client.IP, Client.Port, newSpeed, packet.OpCode)

        'DONE: Anti hack
        If Client.Character.antiHackSpeedChanged_ <= 0 Then
            Log.WriteLine(LogType.WARNING, "[{0}:{1}] CHEAT: Possible speed hack detected!", Client.IP, Client.Port)
            Client.Character.Logout(Nothing)
            Exit Sub
        End If

        'DONE: Update speed value and create packet
        Client.Character.antiHackSpeedChanged_ -= 1
        Select Case packet.OpCode
            Case OPCODES.CMSG_FORCE_RUN_SPEED_CHANGE_ACK
                Client.Character.RunSpeed = newSpeed
            Case OPCODES.CMSG_FORCE_RUN_BACK_SPEED_CHANGE_ACK
                Client.Character.RunBackSpeed = newSpeed
            Case OPCODES.CMSG_FORCE_SWIM_BACK_SPEED_CHANGE_ACK
                Client.Character.SwimBackSpeed = newSpeed
            Case OPCODES.CMSG_FORCE_SWIM_SPEED_CHANGE_ACK
                Client.Character.SwimSpeed = newSpeed
            Case OPCODES.CMSG_FORCE_TURN_RATE_CHANGE_ACK
                Client.Character.TurnRate = newSpeed
            Case OPCODES.CMSG_FORCE_FLIGHT_SPEED_CHANGE_ACK
                Client.Character.FlySpeed = newSpeed
            Case OPCODES.CMSG_FORCE_FLIGHT_BACK_SPEED_CHANGE_ACK
                Client.Character.FlyBackSpeed = newSpeed
        End Select
    End Sub

    Public Sub SendAreaTriggerMessage(ByRef Client As ClientClass, ByVal Text As String)
        Dim p As New PacketClass(OPCODES.SMSG_AREA_TRIGGER_MESSAGE)
        p.AddInt32(Text.Length)
        p.AddString(Text)
        Client.Send(p)
        p.Dispose()
    End Sub
    Public Sub On_CMSG_AREATRIGGER(ByRef packet As PacketClass, ByRef Client As ClientClass)
        Try
            If (packet.Data.Length - 1) < 9 Then Exit Sub
            packet.GetInt16()
            Dim triggerID As Integer = packet.GetInt32
            Log.WriteLine(LogType.DEBUG, "[{0}:{1}] CMSG_AREATRIGGER [triggerID={2}]", Client.IP, Client.Port, triggerID)

            'TODO: Check if in combat?

            Dim q As New DataTable

            'DONE: Handling quest triggers
            q.Clear()
            CharacterDatabase.Query(String.Format("SELECT * FROM areatrigger_involvedrelation WHERE id = {0};", triggerID), q)
            If q.Rows.Count > 0 Then
                OnQuestExplore(Client.Character, triggerID)
                Exit Sub
            End If

            'TODO: Handling tavern triggers
            q.Clear()
            CharacterDatabase.Query(String.Format("SELECT * FROM areatrigger_tavern WHERE id = {0};", triggerID), q)
            If q.Rows.Count > 0 Then
                SetFlag(Client.Character.cPlayerFlags, PlayerFlag.PLAYER_FLAG_RESTING, True)
                Client.Character.SetUpdateFlag(EPlayerFields.PLAYER_FLAGS, Client.Character.cPlayerFlags)
                Exit Sub
            End If

            'DONE: Handling teleport triggers
            q.Clear()
            CharacterDatabase.Query(String.Format("SELECT * FROM areatrigger_teleport WHERE id = {0};", triggerID), q)
            If q.Rows.Count > 0 Then
                If q.Rows(0).Item("required_level") <> 0 AndAlso Client.Character.Level < q.Rows(0).Item("required_level") Then
                    SendAreaTriggerMessage(Client, "Your level is too low")
                    Exit Sub
                End If
                If q.Rows(0).Item("required_item") <> 0 AndAlso Client.Character.ItemCOUNT(q.Rows(0).Item("required_item")) = 0 Then
                    SendAreaTriggerMessage(Client, "You don't have the required item")
                    Exit Sub
                End If
                If q.Rows(0).Item("required_item2") <> 0 AndAlso Client.Character.ItemCOUNT(q.Rows(0).Item("required_item2")) = 0 Then
                    SendAreaTriggerMessage(Client, "You don't have the required item")
                    Exit Sub
                End If
                'TODO: Get Heroic State
                'If q.Rows(0).Item("heroic_key") <> 0 AndAlso Client.Character.ItemCOUNT(q.Rows(0).Item("heroic_key")) = 0 Then
                '    SendAreaTriggerMessage(Client, "You don't have the required key")
                '    Exit Sub
                'End If
                'If q.Rows(0).Item("heroic_key") <> 0 AndAlso Client.Character.ItemCOUNT(q.Rows(0).Item("heroic_key")) = 0 Then
                '    SendAreaTriggerMessage(Client, "You don't have the required key")
                '    Exit Sub
                'End If
                'If q.Rows(0).Item("required_quest_done") <> 0 Then
                '    SendAreaTriggerMessage(Client, q.Rows(0).Item("required_failed_text"))
                '    Exit Sub
                'End If

                Client.Character.Teleport(q.Rows(0).Item("target_position_x"), q.Rows(0).Item("target_position_y"), q.Rows(0).Item("target_position_z"), _
                                          q.Rows(0).Item("target_orientation"), q.Rows(0).Item("target_map"))
                Exit Sub
            End If

            'DONE: Handling all other scripted triggers
            If AreaTriggers.ContainsMethod("AreaTriggers", String.Format("HandleAreaTrigger_{0}", triggerID)) Then
                AreaTriggers.Invoke("AreaTriggers", String.Format("HandleAreaTrigger_{0}", triggerID), New Object() {Client.Character.GUID})
            Else
                Log.WriteLine(LogType.WARNING, "[{0}:{1}] AreaTrigger [{2}] not found!", Client.IP, Client.Port, triggerID)
            End If
        Catch e As Exception
            Log.WriteLine(LogType.CRITICAL, "Error when entering areatrigger.{0}", vbNewLine & e.ToString)
        End Try
    End Sub

    Public Sub On_MSG_MOVE_FALL_LAND(ByRef packet As PacketClass, ByRef Client As ClientClass)
        Try
            OnMovementPacket(packet, Client)

            Dim FallTime As Integer = packet.GetInt32(31)

            'DONE: If FallTime > 1100 and not Dead
            'TODO: Check if the character have any feather or anything that removes fall damage
            If FallTime > 1100 AndAlso (Not Client.Character.DEAD) AndAlso Client.Character.positionZ > GetWaterLevel(Client.Character.positionX, Client.Character.positionY, Client.Character.MapID) Then
                'TODO: Use the code below to calculate the fall damage after fall damage reduction (rogues or druids in catform f.ex)
                'Mangos code:
                'int32 safe_fall = target->GetTotalAuraModifier(SPELL_AURA_SAFE_FALL);
                'uint32 fall_time = (movementInfo.fallTime > (safe_fall*10)) ? movementInfo.fallTime - (safe_fall*10) : 0;
                If FallTime > 1100 Then
                    'DONE: Caluclate fall damage
                    Dim FallPerc As Single = FallTime / 1100
                    Dim FallDamage As Integer = (FallPerc * FallPerc - 1) / 9 * Client.Character.Life.Maximum

                    If FallDamage > 0 Then
                        'Prevent the fall damage to be more than your maximum health
                        If FallDamage > Client.Character.Life.Maximum Then FallDamage = Client.Character.Life.Maximum
                        'Deal the damage
                        Client.Character.LogEnvironmentalDamage(EnviromentalDamage.DAMAGE_FALL, FallDamage)
                        Client.Character.DealDamage(FallDamage)

#If DEBUG Then
                        Log.WriteLine(LogType.USER, "[{0}:{1}] Client fall time: {2}  Damage: {3}", Client.IP, Client.Port, FallTime, FallDamage)
#End If
                    End If
                End If
            End If

            If Not Client.Character.underWaterTimer Is Nothing Then
                Client.Character.underWaterTimer.Dispose()
                Client.Character.underWaterTimer = Nothing
            End If
            If Not Client.Character.LogoutTimer Is Nothing Then
                'DONE: Initialize packet
                Dim UpdateData As New UpdateClass
                Dim SMSG_UPDATE_OBJECT As New PacketClass(OPCODES.SMSG_UPDATE_OBJECT)
                SMSG_UPDATE_OBJECT.AddInt32(1)      'Operations.Count
                SMSG_UPDATE_OBJECT.AddInt8(0)

                'DONE: Disable Turn
                Client.Character.cUnitFlags = Client.Character.cUnitFlags Or UnitFlags.UNIT_FLAG_STUNTED
                UpdateData.SetUpdateFlag(EUnitFields.UNIT_FIELD_FLAGS, Client.Character.cUnitFlags)
                'DONE: StandState -> Sit
                Client.Character.StandState = StandStates.STANDSTATE_SIT
                UpdateData.SetUpdateFlag(EUnitFields.UNIT_FIELD_BYTES_1, Client.Character.cBytes1)

                'DONE: Send packet
                UpdateData.AddToPacket(SMSG_UPDATE_OBJECT, ObjectUpdateType.UPDATETYPE_VALUES, CType(Client.Character, CharacterObject), 1)
                Client.Send(SMSG_UPDATE_OBJECT)
                SMSG_UPDATE_OBJECT.Dispose()

                Dim packetACK As New PacketClass(OPCODES.SMSG_STANDSTATE_UPDATE)
                packetACK.AddInt8(StandStates.STANDSTATE_SIT)
                Client.Send(packetACK)
                packetACK.Dispose()
            End If
        Catch e As Exception
            Log.WriteLine(LogType.DEBUG, "Error when falling.{0}", vbNewLine & e.ToString)
        End Try
    End Sub
    Public Sub On_CMSG_ZONEUPDATE(ByRef packet As PacketClass, ByRef Client As ClientClass)
        If (packet.Data.Length - 1) < 9 Then Exit Sub
        packet.GetInt16()
        Dim newZone As Integer = packet.GetInt32
        Log.WriteLine(LogType.DEBUG, "[{0}:{1}] CMSG_ZONEUPDATE [newZone={2}]", Client.IP, Client.Port, newZone)
        Client.Character.ZoneID = newZone
        Client.Character.exploreCheckQueued_ = True

        Client.Character.ZoneCheck()

        'DONE: Update zone on cluster
        WS.Cluster.ClientUpdate(Client.Index, Client.Character.ZoneID, Client.Character.Level)

        'DONE: Send weather
        Dim MySQLQuery As New DataTable
        CharacterDatabase.Query(String.Format("SELECT * FROM weather WHERE weather_zone = {0};", Client.Character.ZoneID), MySQLQuery)
        If MySQLQuery.Rows.Count = 0 Then
            SendWeather(0, 0, Client)
        Else
            SendWeather(MySQLQuery.Rows(0).Item("weather_type"), MySQLQuery.Rows(0).Item("weather_intensity"), Client)
        End If
    End Sub
    Public Sub On_MSG_MOVE_HEARTBEAT(ByRef packet As PacketClass, ByRef Client As ClientClass)
        'Log.WriteLine(LogType.DEBUG, "[{0}:{1}] MSG_MOVE_HEARTBEAT", Client.IP, Client.Port)

        OnMovementPacket(packet, Client)

        If Client.Character.CellX <> GetMapTileX(Client.Character.positionX) Or Client.Character.CellY <> GetMapTileY(Client.Character.positionY) Then
            MoveCell(Client.Character)
        End If
        UpdateCell(Client.Character)

        Client.Character.ZoneCheck()

        'DONE: Check for out of continent - coordinates from WorldMapContinent.dbc
        If IsOutsideOfMap(CType(Client.Character, CharacterObject)) Then
            If Client.Character.outsideMapID_ = False Then
                Client.Character.outsideMapID_ = True
                Client.Character.StartMirrorTimer(MirrorTimer.FATIGUE, 30000)
            End If
        Else
            If Client.Character.outsideMapID_ = True Then
                Client.Character.outsideMapID_ = False
                Client.Character.StopMirrorTimer(MirrorTimer.FATIGUE)
            End If
        End If

        'DONE: Duel check
        If Client.Character.IsInDuel Then CheckDuelDistance(Client.Character)

        'DONE: Aggro range
        For Each cGUID As ULong In Client.Character.creaturesNear.ToArray
            If WORLD_CREATUREs.ContainsKey(cGUID) AndAlso WORLD_CREATUREs(cGUID).aiScript IsNot Nothing AndAlso ((TypeOf WORLD_CREATUREs(cGUID).aiScript Is DefaultAI) OrElse (TypeOf WORLD_CREATUREs(cGUID).aiScript Is GuardAI)) Then
                If WORLD_CREATUREs(cGUID).aiScript.State <> TBaseAI.AIState.AI_DEAD AndAlso WORLD_CREATUREs(cGUID).aiScript.State <> TBaseAI.AIState.AI_RESPAWN AndAlso WORLD_CREATUREs(cGUID).aiScript.InCombat() = False Then
                    If Client.Character.GetReaction(WORLD_CREATUREs(cGUID).Faction) = TReaction.HOSTILE AndAlso GetDistance(WORLD_CREATUREs(cGUID), Client.Character) <= WORLD_CREATUREs(cGUID).AggroRange(Client.Character) Then
                        WORLD_CREATUREs(cGUID).aiScript.aiHateTable.Add(Client.Character, 0)
                        SetPlayerInCombat(Client.Character)
                        Client.Character.inCombatWith.Add(cGUID)
                        WORLD_CREATUREs(cGUID).aiScript.State = TBaseAI.AIState.AI_ATTACKING
                        WORLD_CREATUREs(cGUID).aiScript.DoThink()
                    End If
                End If
            End If
        Next

        Exit Sub
        'DONE: Creatures that are following you will have a more smooth movement
        For Each CombatUnit As ULong In Client.Character.inCombatWith.ToArray
            If GuidIsCreature(CombatUnit) AndAlso WORLD_CREATUREs.ContainsKey(CombatUnit) AndAlso CType(WORLD_CREATUREs(CombatUnit), CreatureObject).aiScript IsNot Nothing Then
                With CType(WORLD_CREATUREs(CombatUnit), CreatureObject)
                    If (Not .aiScript.aiTarget Is Nothing) AndAlso .aiScript.aiTarget.GUID = Client.Character.GUID Then
                        .aiScript.State = TBaseAI.AIState.AI_MOVE_FOR_ATTACK
                        .aiScript.DoThink()
                    End If
                End With
            End If
        Next
    End Sub

#End Region
#Region "WS.CharacterMovement.CellFramework"

    Public Sub MAP_Load(ByVal x As Byte, ByVal y As Byte, ByVal Map As Integer)
        For i As Short = -1 To 1
            For j As Short = -1 To 1
                If x + i > -1 AndAlso x + i < 64 AndAlso y + j > -1 AndAlso y + j < 64 Then
                    If Maps(Map).Tiles(x + i, y + j) Is Nothing Then
                        Log.WriteLine(LogType.INFORMATION, "Loading map [{2}: {0},{1}]...", x + i, y + j, Map)
                        Maps(Map).Tiles(x + i, y + j) = New TMapTile(x + i, y + j, Map)
                        'DONE: Load spawns
                        LoadSpawns(x + i, y + j, Map)
                    End If
                End If
            Next
        Next
    End Sub
    Public Sub MAP_UnLoad(ByVal x As Byte, ByVal y As Byte, ByVal Map As Integer)
        If Maps(Map).Tiles(x, y).PlayersHere.Count = 0 Then
            Log.WriteLine(LogType.INFORMATION, "Unloading map [{2}: {0},{1}]...", x, y, Map)
            Maps(Map).Tiles(x, y).Dispose()
            Maps(Map).Tiles(x, y) = Nothing
        End If
    End Sub

    Public Sub AddToWorld(ByRef Character As CharacterObject)
        GetMapTile(Character.positionX, Character.positionY, Character.CellX, Character.CellY)

        'DONE: Dynamic map loading
        If Maps(Character.MapID).Tiles(Character.CellX, Character.CellY) Is Nothing Then MAP_Load(Character.CellX, Character.CellY, Character.MapID)

        Dim list() As ULong

        'DONE: Sending near players in <CENTER> Cell
        If Maps(Character.MapID).Tiles(Character.CellX, Character.CellY).PlayersHere.Count > 0 Then
            With Maps(Character.MapID).Tiles(Character.CellX, Character.CellY)
                list = .PlayersHere.ToArray
                For Each GUID As ULong In list
                    If Character.CanSee(CType(CHARACTERs(GUID), BaseObject)) Then
                        Dim youpacket As New UpdatePacketClass
                        Dim tmpUpdate As New UpdateClass(FIELD_MASK_SIZE_PLAYER)
                        CHARACTERs(GUID).FillAllUpdateFlags(tmpUpdate, Character)
                        tmpUpdate.AddToPacket(CType(youpacket, UpdatePacketClass), ObjectUpdateType.UPDATETYPE_CREATE_OBJECT, CType(CHARACTERs(GUID), CharacterObject), 0)
                        tmpUpdate.Dispose()
                        Character.Client.Send(CType(youpacket, UpdatePacketClass))
                        youpacket.Dispose()

                        CHARACTERs(GUID).SeenBy.Add(Character.GUID)

                        Character.playersNear.Add(GUID)
                    End If
                    If CHARACTERs(GUID).CanSee(CType(Character, BaseObject)) Then
                        Dim myPacket As New UpdatePacketClass
                        Dim myTmpUpdate As New UpdateClass(FIELD_MASK_SIZE_PLAYER)
                        Character.FillAllUpdateFlags(myTmpUpdate, CHARACTERs(GUID))
                        myTmpUpdate.AddToPacket(CType(myPacket, UpdatePacketClass), ObjectUpdateType.UPDATETYPE_CREATE_OBJECT, Character, 0)
                        myTmpUpdate.Dispose()
                        CHARACTERs(GUID).Client.Send(CType(myPacket, UpdatePacketClass))
                        myPacket.Dispose()

                        Character.SeenBy.Add(GUID)

                        CHARACTERs(GUID).playersNear.Add(Character.GUID)
                    End If
                Next
            End With
        End If

        'DONE: Cleanig
        'myPacket.Dispose()
        Maps(Character.MapID).Tiles(Character.CellX, Character.CellY).PlayersHere.Add(Character.GUID)

        'DONE: Creatures and GameObjects are sent in the same packet
        Dim packet As New UpdatePacketClass

        'DONE: Sending near Creatures in <CENTER> Cell
        If Maps(Character.MapID).Tiles(Character.CellX, Character.CellY).CreaturesHere.Count > 0 Then
            With Maps(Character.MapID).Tiles(Character.CellX, Character.CellY)
                list = .CreaturesHere.ToArray
                For Each GUID As ULong In list
                    If Character.CanSee(WORLD_CREATUREs(GUID)) Then
                        Dim tmpUpdate As New UpdateClass(FIELD_MASK_SIZE_UNIT)
                        WORLD_CREATUREs(GUID).FillAllUpdateFlags(tmpUpdate, Character)
                        tmpUpdate.AddToPacket(CType(packet, UpdatePacketClass), ObjectUpdateType.UPDATETYPE_CREATE_OBJECT, CType(WORLD_CREATUREs(GUID), CreatureObject), 0)
                        tmpUpdate.Dispose()

                        Character.creaturesNear.Add(GUID)

                        WORLD_CREATUREs(GUID).SeenBy.Add(Character.GUID)
                    End If
                Next
            End With
        End If

        'DONE: Sending near GameObjects in <CENTER> Cell
        If Maps(Character.MapID).Tiles(Character.CellX, Character.CellY).GameObjectsHere.Count > 0 Then
            With Maps(Character.MapID).Tiles(Character.CellX, Character.CellY)
                list = .GameObjectsHere.ToArray
                For Each GUID As ULong In list
                    If Not Character.gameObjectsNear.Contains(GUID) Then
                        If Character.CanSee(WORLD_GAMEOBJECTs(GUID)) Then
                            Dim tmpUpdate As New UpdateClass(FIELD_MASK_SIZE_GAMEOBJECT)
                            WORLD_GAMEOBJECTs(GUID).FillAllUpdateFlags(tmpUpdate, Character)
                            tmpUpdate.AddToPacket(CType(packet, UpdatePacketClass), ObjectUpdateType.UPDATETYPE_CREATE_OBJECT, CType(WORLD_GAMEOBJECTs(GUID), GameObjectObject), 0)
                            tmpUpdate.Dispose()

                            Character.gameObjectsNear.Add(GUID)

                            WORLD_GAMEOBJECTs(GUID).SeenBy.Add(Character.GUID)
                        End If
                    End If
                Next
            End With
        End If

        'DONE: Sending near CorpseObjects in <CENTER> Cell
        If Maps(Character.MapID).Tiles(Character.CellX, Character.CellY).CorpseObjectsHere.Count > 0 Then
            With Maps(Character.MapID).Tiles(Character.CellX, Character.CellY)
                list = .CorpseObjectsHere.ToArray
                For Each GUID As ULong In list
                    If Not Character.corpseObjectsNear.Contains(GUID) Then
                        If Character.CanSee(WORLD_CORPSEOBJECTs(GUID)) Then
                            Dim tmpUpdate As New UpdateClass(FIELD_MASK_SIZE_CORPSE)
                            WORLD_CORPSEOBJECTs(GUID).FillAllUpdateFlags(tmpUpdate, Character)
                            tmpUpdate.AddToPacket(CType(packet, UpdatePacketClass), ObjectUpdateType.UPDATETYPE_CREATE_OBJECT, CType(WORLD_CORPSEOBJECTs(GUID), CorpseObject), 0)
                            tmpUpdate.Dispose()

                            Character.corpseObjectsNear.Add(GUID)

                            WORLD_CORPSEOBJECTs(GUID).SeenBy.Add(Character.GUID)
                        End If
                    End If
                Next
            End With
        End If

        'DONE: Send the packet
        Character.Client.Send(CType(packet, UpdatePacketClass))
        packet.Dispose()

        'Update nearby cells alaso
        UpdateCell(Character)
    End Sub
    Public Sub RemoveFromWorld(ByRef Character As CharacterObject)
        If Not Maps.ContainsKey(Character.MapID) Then Return

        If Not Maps(Character.MapID).Tiles(Character.CellX, Character.CellY) Is Nothing Then
            'DONE: Remove character from map
            GetMapTile(Character.positionX, Character.positionY, Character.CellX, Character.CellY)
            Maps(Character.MapID).Tiles(Character.CellX, Character.CellY).PlayersHere.Remove(Character.GUID)
        End If

        Dim list() As ULong

        'DONE: Removing from players wich can see it
        list = Character.SeenBy.ToArray
        For Each GUID As ULong In list

            If CHARACTERs(GUID).playersNear.Contains(Character.GUID) Then
                CHARACTERs(GUID).guidsForRemoving_Lock.AcquireWriterLock(DEFAULT_LOCK_TIMEOUT)
                CHARACTERs(GUID).guidsForRemoving.Add(Character.GUID)
                CHARACTERs(GUID).guidsForRemoving_Lock.ReleaseWriterLock()

                CHARACTERs(GUID).playersNear.Remove(Character.GUID)
            End If
            'DONE: Fully clean
            CHARACTERs(GUID).SeenBy.Remove(Character.GUID)

        Next
        Character.playersNear.Clear()
        Character.SeenBy.Clear()

        'DONE: Removing from creatures wich can see it
        list = Character.creaturesNear.ToArray
        For Each GUID As ULong In list

            If WORLD_CREATUREs(GUID).SeenBy.Contains(Character.GUID) Then
                WORLD_CREATUREs(GUID).SeenBy.Remove(Character.GUID)
            End If
        Next
        Character.creaturesNear.Clear()

        'DONE: Removing from gameObjects wich can see it
        list = Character.gameObjectsNear.ToArray
        For Each GUID As ULong In list

            If WORLD_GAMEOBJECTs(GUID).SeenBy.Contains(Character.GUID) Then
                WORLD_GAMEOBJECTs(GUID).SeenBy.Remove(Character.GUID)
            End If
        Next
        Character.gameObjectsNear.Clear()

        'DONE: Removing from corpseObjects wich can see it
        list = Character.corpseObjectsNear.ToArray
        For Each GUID As ULong In list

            If WORLD_CORPSEOBJECTs(GUID).SeenBy.Contains(Character.GUID) Then
                WORLD_CORPSEOBJECTs(GUID).SeenBy.Remove(Character.GUID)
            End If
        Next
        Character.corpseObjectsNear.Clear()
    End Sub
    Public Sub MoveCell(ByRef Character As CharacterObject)
        Dim oldX As Byte = Character.CellX
        Dim oldY As Byte = Character.CellY
        GetMapTile(Character.positionX, Character.positionY, Character.CellX, Character.CellY)

        'Map Loading
        If Maps(Character.MapID).Tiles(Character.CellX, Character.CellY) Is Nothing Then MAP_Load(Character.CellX, Character.CellY, Character.MapID)

        'TODO: Fix map unloading

        If Character.CellX <> oldX Or Character.CellY <> oldY Then
            Maps(Character.MapID).Tiles(oldX, oldY).PlayersHere.Remove(Character.GUID)
            'MAP_UnLoad(oldX, oldY)
            Maps(Character.MapID).Tiles(Character.CellX, Character.CellY).PlayersHere.Add(Character.GUID)
        End If
    End Sub
    Public Sub UpdateCell(ByRef Character As CharacterObject)
        'Dim start As Integer = timeGetTime
        Dim list() As ULong

        'DONE: Remove players,creatures,objects if dist is >
        list = Character.playersNear.ToArray
        For Each GUID As ULong In list
            If Not Character.CanSee(CHARACTERs(GUID)) Then
                Character.guidsForRemoving_Lock.AcquireWriterLock(DEFAULT_LOCK_TIMEOUT)
                Character.guidsForRemoving.Add(GUID)
                Character.guidsForRemoving_Lock.ReleaseWriterLock()

                CHARACTERs(GUID).SeenBy.Remove(Character.GUID)
                Character.playersNear.Remove(GUID)
            End If
            'Remove me for him
            If (Not CHARACTERs(GUID).CanSee(Character)) AndAlso Character.SeenBy.Contains(GUID) Then
                CHARACTERs(GUID).guidsForRemoving_Lock.AcquireWriterLock(DEFAULT_LOCK_TIMEOUT)
                CHARACTERs(GUID).guidsForRemoving.Add(Character.GUID)
                CHARACTERs(GUID).guidsForRemoving_Lock.ReleaseWriterLock()

                Character.SeenBy.Remove(GUID)
                CHARACTERs(GUID).playersNear.Remove(Character.GUID)
            End If
        Next

        list = Character.creaturesNear.ToArray
        For Each GUID As ULong In list
            If Not Character.CanSee(WORLD_CREATUREs(GUID)) Then
                Character.guidsForRemoving_Lock.AcquireWriterLock(DEFAULT_LOCK_TIMEOUT)
                Character.guidsForRemoving.Add(GUID)
                Character.guidsForRemoving_Lock.ReleaseWriterLock()

                WORLD_CREATUREs(GUID).SeenBy.Remove(Character.GUID)
                Character.creaturesNear.Remove(GUID)
            End If
        Next

        list = Character.gameObjectsNear.ToArray
        For Each GUID As ULong In list
            If Not Character.CanSee(WORLD_GAMEOBJECTs(GUID)) Then
                Character.guidsForRemoving_Lock.AcquireWriterLock(DEFAULT_LOCK_TIMEOUT)
                Character.guidsForRemoving.Add(GUID)
                Character.guidsForRemoving_Lock.ReleaseWriterLock()

                WORLD_GAMEOBJECTs(GUID).SeenBy.Remove(Character.GUID)
                Character.gameObjectsNear.Remove(GUID)
            End If
        Next

        list = Character.dynamicObjectsNear.ToArray
        For Each GUID As ULong In list
            If Not Character.CanSee(WORLD_DYNAMICOBJECTs(GUID)) Then
                Character.guidsForRemoving_Lock.AcquireWriterLock(DEFAULT_LOCK_TIMEOUT)
                Character.guidsForRemoving.Add(GUID)
                Character.guidsForRemoving_Lock.ReleaseWriterLock()

                WORLD_DYNAMICOBJECTs(GUID).SeenBy.Remove(Character.GUID)
                Character.dynamicObjectsNear.Remove(GUID)
            End If
        Next

        list = Character.corpseObjectsNear.ToArray
        For Each GUID As ULong In list

            If Not Character.CanSee(WORLD_CORPSEOBJECTs(GUID)) Then
                Character.guidsForRemoving_Lock.AcquireWriterLock(DEFAULT_LOCK_TIMEOUT)
                Character.guidsForRemoving.Add(GUID)
                Character.guidsForRemoving_Lock.ReleaseWriterLock()

                WORLD_CORPSEOBJECTs(GUID).SeenBy.Remove(Character.GUID)
                Character.corpseObjectsNear.Remove(GUID)
            End If
        Next

        'DONE: Add new if dist is <
        Dim CellXAdd As Short = -1
        Dim CellYAdd As Short = -1
        If GetSubMapTileX(Character.positionX) > 32 Then CellXAdd = 1
        If GetSubMapTileX(Character.positionY) > 32 Then CellYAdd = 1
        If (Character.CellX + CellXAdd) > 63 Or (Character.CellX + CellXAdd) < 0 Then CellXAdd = 0
        If (Character.CellY + CellYAdd) > 63 Or (Character.CellY + CellYAdd) < 0 Then CellYAdd = 0

        'DONE: Load cell if needed
        If Maps(Character.MapID).Tiles(Character.CellX, Character.CellY) Is Nothing Then
            MAP_Load(Character.CellX, Character.CellY, Character.MapID)
        End If
        'DONE: Sending near creatures and gameobjects in <CENTER CELL>
        If Maps(Character.MapID).Tiles(Character.CellX, Character.CellY).CreaturesHere.Count > 0 OrElse Maps(Character.MapID).Tiles(Character.CellX, Character.CellY).GameObjectsHere.Count > 0 Then
            UpdateCreaturesAndGameObjectsInCell(Maps(Character.MapID).Tiles(Character.CellX, Character.CellY), Character)
        End If
        'DONE: Sending near players in <CENTER CELL>
        If Maps(Character.MapID).Tiles(Character.CellX, Character.CellY).PlayersHere.Count > 0 Then
            UpdatePlayersInCell(Maps(Character.MapID).Tiles(Character.CellX, Character.CellY), Character)
        End If
        'DONE: Sending near corpseobjects in <CENTER CELL>
        If Maps(Character.MapID).Tiles(Character.CellX, Character.CellY).CorpseObjectsHere.Count > 0 Then
            UpdateCorpseObjectsInCell(Maps(Character.MapID).Tiles(Character.CellX, Character.CellY), Character)
        End If

        If CellXAdd <> 0 Then
            'DONE: Load cell if needed
            If Maps(Character.MapID).Tiles(Character.CellX + CellXAdd, Character.CellY) Is Nothing Then
                MAP_Load(Character.CellX + CellXAdd, Character.CellY, Character.MapID)
            End If
            'DONE: Sending near creatures and gameobjects in <LEFT/RIGHT CELL>
            If Maps(Character.MapID).Tiles(Character.CellX + CellXAdd, Character.CellY).CreaturesHere.Count > 0 OrElse Maps(Character.MapID).Tiles(Character.CellX + CellXAdd, Character.CellY).GameObjectsHere.Count > 0 Then
                UpdateCreaturesAndGameObjectsInCell(Maps(Character.MapID).Tiles(Character.CellX + CellXAdd, Character.CellY), Character)
            End If
            'DONE: Sending near players in <LEFT/RIGHT CELL>
            If Maps(Character.MapID).Tiles(Character.CellX + CellXAdd, Character.CellY).PlayersHere.Count > 0 Then
                UpdatePlayersInCell(Maps(Character.MapID).Tiles(Character.CellX + CellXAdd, Character.CellY), Character)
            End If
            'DONE: Sending near corpseobjects in <LEFT/RIGHT CELL>
            If Maps(Character.MapID).Tiles(Character.CellX + CellXAdd, Character.CellY).CorpseObjectsHere.Count > 0 Then
                UpdateCorpseObjectsInCell(Maps(Character.MapID).Tiles(Character.CellX + CellXAdd, Character.CellY), Character)
            End If
        End If

        If CellYAdd <> 0 Then
            'DONE: Load cell if needed
            If Maps(Character.MapID).Tiles(Character.CellX, Character.CellY + CellYAdd) Is Nothing Then
                MAP_Load(Character.CellX, Character.CellY + CellYAdd, Character.MapID)
            End If
            'DONE: Sending near creatures and gameobjects in <TOP/BOTTOM CELL>
            If Maps(Character.MapID).Tiles(Character.CellX, Character.CellY + CellYAdd).CreaturesHere.Count > 0 OrElse Maps(Character.MapID).Tiles(Character.CellX, Character.CellY + CellYAdd).GameObjectsHere.Count > 0 Then
                UpdateCreaturesAndGameObjectsInCell(Maps(Character.MapID).Tiles(Character.CellX, Character.CellY + CellYAdd), Character)
            End If
            'DONE: Sending near players in <TOP/BOTTOM CELL>
            If Maps(Character.MapID).Tiles(Character.CellX, Character.CellY + CellYAdd).PlayersHere.Count > 0 Then
                UpdatePlayersInCell(Maps(Character.MapID).Tiles(Character.CellX, Character.CellY + CellYAdd), Character)
            End If
            'DONE: Sending near corpseobjects in <TOP/BOTTOM CELL>
            If Maps(Character.MapID).Tiles(Character.CellX, Character.CellY + CellYAdd).CorpseObjectsHere.Count > 0 Then
                UpdateCorpseObjectsInCell(Maps(Character.MapID).Tiles(Character.CellX, Character.CellY + CellYAdd), Character)
            End If
        End If

        If CellYAdd <> 0 AndAlso CellXAdd <> 0 Then
            'DONE: Load cell if needed
            If Maps(Character.MapID).Tiles(Character.CellX + CellXAdd, Character.CellY + CellYAdd) Is Nothing Then
                MAP_Load(Character.CellX + CellXAdd, Character.CellY + CellYAdd, Character.MapID)
            End If
            'DONE: Sending near creatures and gameobjects in <CORNER CELL>
            If Maps(Character.MapID).Tiles(Character.CellX + CellXAdd, Character.CellY + CellYAdd).CreaturesHere.Count > 0 OrElse Maps(Character.MapID).Tiles(Character.CellX + CellXAdd, Character.CellY + CellYAdd).GameObjectsHere.Count > 0 Then
                UpdateCreaturesAndGameObjectsInCell(Maps(Character.MapID).Tiles(Character.CellX + CellXAdd, Character.CellY + CellYAdd), Character)
            End If
            'DONE: Sending near players in <LEFT/RIGHT CELL>
            If Maps(Character.MapID).Tiles(Character.CellX + CellXAdd, Character.CellY + CellYAdd).PlayersHere.Count > 0 Then
                UpdatePlayersInCell(Maps(Character.MapID).Tiles(Character.CellX + CellXAdd, Character.CellY + CellYAdd), Character)
            End If
            'DONE: Sending near corpseobjects in <LEFT/RIGHT CELL>
            If Maps(Character.MapID).Tiles(Character.CellX + CellXAdd, Character.CellY + CellYAdd).CorpseObjectsHere.Count > 0 Then
                UpdateCorpseObjectsInCell(Maps(Character.MapID).Tiles(Character.CellX + CellXAdd, Character.CellY + CellYAdd), Character)
            End If
        End If

        Character.SendOutOfRangeUpdate()
        'Log.WriteLine(LogType.DEBUG, "Update: {0}ms", timeGetTime - start)
    End Sub

    <MethodImplAttribute(MethodImplOptions.Synchronized)> _
    Public Sub UpdatePlayersInCell(ByRef MapTile As TMapTile, ByRef Character As CharacterObject)
        Dim list() As ULong

        With MapTile
            list = .PlayersHere.ToArray
            For Each GUID As ULong In list

                'DONE: Send to me
                If Not CHARACTERs(GUID).SeenBy.Contains(Character.GUID) Then
                    If Character.CanSee(CHARACTERs(GUID)) Then
                        Dim packet As New PacketClass(OPCODES.SMSG_UPDATE_OBJECT)
                        packet.AddInt32(1)
                        packet.AddInt8(0)
                        Dim tmpUpdate As New UpdateClass(FIELD_MASK_SIZE_PLAYER)
                        CHARACTERs(GUID).FillAllUpdateFlags(tmpUpdate, Character)
                        tmpUpdate.AddToPacket(packet, ObjectUpdateType.UPDATETYPE_CREATE_OBJECT, CType(CHARACTERs(GUID), CharacterObject), 0)
                        tmpUpdate.Dispose()
                        Character.Client.Send(packet)
                        packet.Dispose()

                        CHARACTERs(GUID).SeenBy.Add(Character.GUID)
                        Character.playersNear.Add(GUID)
                    End If
                End If
                'DONE: Send to him
                If Not Character.SeenBy.Contains(GUID) Then
                    If CHARACTERs(GUID).CanSee(Character) Then
                        Dim myPacket As New PacketClass(OPCODES.SMSG_UPDATE_OBJECT)
                        myPacket.AddInt32(1)
                        myPacket.AddInt8(0)
                        Dim myTmpUpdate As New UpdateClass(FIELD_MASK_SIZE_PLAYER)
                        Character.FillAllUpdateFlags(myTmpUpdate, CHARACTERs(GUID))
                        myTmpUpdate.AddToPacket(myPacket, ObjectUpdateType.UPDATETYPE_CREATE_OBJECT, Character, 0)
                        myTmpUpdate.Dispose()

                        CHARACTERs(GUID).Client.Send(myPacket)
                        myPacket.Dispose()

                        Character.SeenBy.Add(GUID)
                        CHARACTERs(GUID).playersNear.Add(Character.GUID)
                    End If
                End If
            Next
        End With
    End Sub
    Public Sub UpdateCreaturesAndGameObjectsInCell(ByRef MapTile As TMapTile, ByRef Character As CharacterObject)
        Dim list() As ULong
        Dim packet As New UpdatePacketClass

        With MapTile
            list = .CreaturesHere.ToArray
            For Each GUID As ULong In list
                If Not Character.creaturesNear.Contains(GUID) Then
                    If Character.CanSee(WORLD_CREATUREs(GUID)) Then
                        Dim tmpUpdate As New UpdateClass(FIELD_MASK_SIZE_UNIT)
                        WORLD_CREATUREs(GUID).FillAllUpdateFlags(tmpUpdate, Character)
                        tmpUpdate.AddToPacket(packet, ObjectUpdateType.UPDATETYPE_CREATE_OBJECT, WORLD_CREATUREs(GUID), 0)
                        tmpUpdate.Dispose()

                        Character.creaturesNear.Add(GUID)
                        WORLD_CREATUREs(GUID).SeenBy.Add(Character.GUID)
                    End If
                End If
            Next

            list = .GameObjectsHere.ToArray
            For Each GUID As ULong In list
                If Not Character.gameObjectsNear.Contains(GUID) Then
                    If Character.CanSee(WORLD_GAMEOBJECTs(GUID)) Then
                        Dim tmpUpdate As New UpdateClass(FIELD_MASK_SIZE_GAMEOBJECT)
                        WORLD_GAMEOBJECTs(GUID).FillAllUpdateFlags(tmpUpdate, Character)
                        tmpUpdate.AddToPacket(packet, ObjectUpdateType.UPDATETYPE_CREATE_OBJECT, WORLD_GAMEOBJECTs(GUID), 0)
                        tmpUpdate.Dispose()

                        Character.gameObjectsNear.Add(GUID)

                        WORLD_GAMEOBJECTs(GUID).SeenBy.Add(Character.GUID)
                    End If
                End If
            Next

            list = .DynamicObjectsHere.ToArray
            For Each GUID As ULong In list
                If Not Character.dynamicObjectsNear.Contains(GUID) Then
                    If Character.CanSee(WORLD_DYNAMICOBJECTs(GUID)) Then
                        Dim tmpUpdate As New UpdateClass(FIELD_MASK_SIZE_DYNAMICOBJECT)
                        WORLD_DYNAMICOBJECTs(GUID).FillAllUpdateFlags(tmpUpdate)
                        tmpUpdate.AddToPacket(packet, ObjectUpdateType.UPDATETYPE_CREATE_OBJECT_SELF, WORLD_DYNAMICOBJECTs(GUID), 0)
                        tmpUpdate.Dispose()

                        Character.dynamicObjectsNear.Add(GUID)

                        WORLD_DYNAMICOBJECTs(GUID).SeenBy.Add(Character.GUID)
                    End If
                End If
            Next
        End With

        'DONE: Send creatures, game objects and dynamic objects in the same packet
        If packet.UpdatesCount > 0 Then Character.Client.Send(packet)
        packet.Dispose()
    End Sub
    Public Sub UpdateCreaturesInCell(ByRef MapTile As TMapTile, ByRef Character As CharacterObject)
        Dim list() As ULong

        With MapTile
            list = .CreaturesHere.ToArray
            For Each GUID As ULong In list

                If Not Character.creaturesNear.Contains(GUID) Then
                    If Character.CanSee(WORLD_CREATUREs(GUID)) Then
                        Dim packet As New PacketClass(OPCODES.SMSG_UPDATE_OBJECT)
                        packet.AddInt32(1)
                        packet.AddInt8(0)
                        Dim tmpUpdate As New UpdateClass(FIELD_MASK_SIZE_UNIT)
                        WORLD_CREATUREs(GUID).FillAllUpdateFlags(tmpUpdate, Character)
                        tmpUpdate.AddToPacket(packet, ObjectUpdateType.UPDATETYPE_CREATE_OBJECT, CType(WORLD_CREATUREs(GUID), CreatureObject), 0)
                        tmpUpdate.Dispose()
                        Character.Client.Send(packet)
                        packet.Dispose()

                        Character.creaturesNear.Add(GUID)
                        WORLD_CREATUREs(GUID).SeenBy.Add(Character.GUID)
                    End If
                End If
            Next
        End With
    End Sub
    Public Sub UpdateGameObjectsInCell(ByRef MapTile As TMapTile, ByRef Character As CharacterObject)
        With MapTile

            Dim list() As ULong

            list = .GameObjectsHere.ToArray
            For Each GUID As ULong In list

                If Not Character.gameObjectsNear.Contains(GUID) Then
                    If Character.CanSee(WORLD_GAMEOBJECTs(GUID)) Then
                        Dim packet As New PacketClass(OPCODES.SMSG_UPDATE_OBJECT)
                        packet.AddInt32(1)
                        packet.AddInt8(0)
                        Dim tmpUpdate As New UpdateClass(FIELD_MASK_SIZE_GAMEOBJECT)
                        WORLD_GAMEOBJECTs(GUID).FillAllUpdateFlags(tmpUpdate, Character)
                        tmpUpdate.AddToPacket(packet, ObjectUpdateType.UPDATETYPE_CREATE_OBJECT, CType(WORLD_GAMEOBJECTs(GUID), GameObjectObject), 0)
                        tmpUpdate.Dispose()
                        Character.Client.Send(packet)
                        packet.Dispose()

                        Character.gameObjectsNear.Add(GUID)

                        WORLD_GAMEOBJECTs(GUID).SeenBy.Add(Character.GUID)
                    End If
                End If
            Next

        End With
    End Sub
    Public Sub UpdateCorpseObjectsInCell(ByRef MapTile As TMapTile, ByRef Character As CharacterObject)
        With MapTile

            Dim list() As ULong

            list = .CorpseObjectsHere.ToArray
            For Each GUID As ULong In list

                If Not Character.corpseObjectsNear.Contains(GUID) Then
                    If Character.CanSee(WORLD_CORPSEOBJECTs(GUID)) Then
                        Dim packet As New PacketClass(OPCODES.SMSG_UPDATE_OBJECT)
                        packet.AddInt32(1)
                        packet.AddInt8(0)
                        Dim tmpUpdate As New UpdateClass(FIELD_MASK_SIZE_CORPSE)
                        WORLD_CORPSEOBJECTs(GUID).FillAllUpdateFlags(tmpUpdate, Character)
                        tmpUpdate.AddToPacket(packet, ObjectUpdateType.UPDATETYPE_CREATE_OBJECT, CType(WORLD_CORPSEOBJECTs(GUID), CorpseObject), 0)
                        tmpUpdate.Dispose()
                        Character.Client.Send(packet)
                        packet.Dispose()

                        Character.corpseObjectsNear.Add(GUID)
                        WORLD_CORPSEOBJECTs(GUID).SeenBy.Add(Character.GUID)
                    End If
                End If
            Next

        End With
    End Sub

#End Region

    Public Sub SendWeather(ByVal Type As Byte, ByVal Intensity As Single, ByRef Client As ClientClass)
        ' WEATHER_SOUND_NOSOUND                 0
        ' WEATHER_SOUND_RAINLIGHT               8533
        ' WEATHER_SOUND_RAINMEDIUM              8534
        ' WEATHER_SOUND_RAINHEAVY               8535
        ' WEATHER_SOUND_SNOWLIGHT               8536
        ' WEATHER_SOUND_SNOWMEDIUM              8537
        ' WEATHER_SOUND_SNOWHEAVY               8538
        ' WEATHER_SOUND_SANDSTORMLIGHT          8556
        ' WEATHER_SOUND_SANDSTORMMEDIUM         8557
        ' WEATHER_SOUND_SANDSTORMHEAVY          8558

        ' WEATHER_RAIN							1
        ' WEATHER_SNOW							2
        ' WEATHER_SANDSTORM						3

        Dim SMSG_WEATHER As New PacketClass(OPCODES.SMSG_WEATHER)
        SMSG_WEATHER.AddInt32(Type)
        SMSG_WEATHER.AddSingle(Intensity)
        'SMSG_WEATHER.AddInt32(Sound)
        Select Case Intensity
            Case 0
                SMSG_WEATHER.AddInt32(0)
            Case Is <= 0.33
                If Type = 1 Then SMSG_WEATHER.AddInt32(8533)
                If Type = 2 Then SMSG_WEATHER.AddInt32(8533 + 3)
                If Type = 3 Then SMSG_WEATHER.AddInt32(8533 + 23)
            Case Is <= 0.66
                If Type = 1 Then SMSG_WEATHER.AddInt32(8534)
                If Type = 2 Then SMSG_WEATHER.AddInt32(8534 + 3)
                If Type = 3 Then SMSG_WEATHER.AddInt32(8534 + 23)
            Case Else
                If Type = 1 Then SMSG_WEATHER.AddInt32(8535)
                If Type = 2 Then SMSG_WEATHER.AddInt32(8535 + 3)
                If Type = 3 Then SMSG_WEATHER.AddInt32(8535 + 23)
        End Select
        Client.Send(SMSG_WEATHER)
        'Client.Character.SendToNearPlayers(SMSG_WEATHER)
        SMSG_WEATHER.Dispose()
    End Sub

End Module