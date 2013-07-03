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
Imports System.Collections.Generic
Imports Spurious.Common.BaseWriter

Public Module WS_TimerBasedEvents

    Public Regenerator As TRegenerator
    Public AIManager As TAIManager
    Public SpellManager As TSpellManager
    Public CharacterSaver As TCharacterSaver

    'NOTE: Regenerates players' Mana, Life and Rage
    Public Class TRegenerator
        Implements IDisposable

        Private RegenerationTimer As Threading.Timer = Nothing
        Private RegenerationWorking As Boolean = False

        Private operationsCount As Integer
        Private BaseMana As Integer
        Private BaseLife As Integer
        Private BaseRage As Integer
        Private BaseEnergy As Integer
        Private _updateFlag As Boolean

        Public Const REGENERATION_TIMER As Integer = 2          'Timer period (ms)
        Public Const REGENERATION_ENERGY As Integer = 20        'Base energy regeneration rate
        Public Const REGENERATION_RAGE As Integer = 25          'Base rage degeneration rate (Rage = 1000 but shows only as 100 in game)
        Public Sub New()
            RegenerationTimer = New Threading.Timer(AddressOf Regenerate, Nothing, 10000, REGENERATION_TIMER * 1000)
        End Sub
        Private Sub Regenerate(ByVal state As Object)
            If RegenerationWorking Then
                Log.WriteLine(LogType.WARNING, "Update: Regenerator skipping update")
                Exit Sub
            End If

            RegenerationWorking = True
            Try
                CHARACTERs_Lock.AcquireReaderLock(DEFAULT_LOCK_TIMEOUT)
                For Each Character As KeyValuePair(Of ULong, CharacterObject) In CHARACTERs
                    'DONE: If all invalid check passed then regenerate
                    'DONE: If dead don't regenerate
                    If (Not Character.Value.DEAD) AndAlso (Character.Value.underWaterTimer Is Nothing) AndAlso (Character.Value.LogoutTimer Is Nothing) AndAlso (Character.Value.Client IsNot Nothing) Then
                        With CType(Character.Value, CharacterObject)


                            BaseMana = .Mana.Current
                            BaseRage = .Rage.Current
                            BaseEnergy = .Energy.Current
                            BaseLife = .Life.Current
                            _updateFlag = False

                            'Rage
                            'DONE: In combat do not decrease, but send updates
                            If .ManaType = ManaTypes.TYPE_RAGE Then
                                If (.cUnitFlags And UnitFlags.UNIT_FLAG_IN_COMBAT) = 0 Then
                                    If .Rage.Current > 0 Then
                                        .Rage.Current -= REGENERATION_RAGE
                                        If .Rage.Current < 0 Then .Rage.Current = 0
                                    End If
                                ElseIf .RageRegenBonus <> 0 Then 'In Combat Regen from spells
                                    .Rage.Increment(.RageRegenBonus)
                                End If
                            End If

                            'Energy
                            If .ManaType = ManaTypes.TYPE_ENERGY AndAlso .Energy.Current < .Energy.Maximum Then
                                .Energy.Increment(REGENERATION_ENERGY)
                            End If

                            'Mana
                            If .ManaRegen = 0 Then .UpdateManaRegen()
                            'DONE: Don't regenerate while casting, 5 second rule
                            'TODO: If c.ManaRegenerationWhileCastingPercent > 0 ...
                            If .spellCastManaRegeneration = 0 Then
                                If .spellCastState = SpellCastState.SPELL_STATE_IDLE AndAlso (.ManaType = ManaTypes.TYPE_MANA OrElse .Classe = Classes.CLASS_DRUID) AndAlso .Mana.Current < .Mana.Maximum Then
                                    .Mana.Increment(.ManaRegen)
                                ElseIf (.ManaType = ManaTypes.TYPE_MANA OrElse .Classe = Classes.CLASS_DRUID) AndAlso .Mana.Current < .Mana.Maximum Then
                                    .Mana.Increment(.ManaRegenInterrupt)
                                End If
                            Else
                                If (.ManaType = ManaTypes.TYPE_MANA OrElse .Classe = Classes.CLASS_DRUID) AndAlso .Mana.Current < .Mana.Maximum Then
                                    .Mana.Increment(.ManaRegenInterrupt)
                                End If
                                If .spellCastManaRegeneration < REGENERATION_TIMER Then
                                    .spellCastManaRegeneration = 0
                                Else
                                    .spellCastManaRegeneration -= REGENERATION_TIMER
                                End If
                            End If

                            'Life
                            'DONE: Don't regenerate in combat
                            'TODO: If c.LifeRegenWhileFightingPercent > 0 ...
                            If .Life.Current < .Life.Maximum AndAlso (.cUnitFlags And UnitFlags.UNIT_FLAG_IN_COMBAT) = 0 Then
                                Select Case .Classe
                                    Case Classes.CLASS_MAGE
                                        .Life.Increment(CType((.Spirit.Base * 0.1) * .LifeRegenerationModifier, Integer) + .LifeRegenBonus)
                                    Case Classes.CLASS_PRIEST
                                        .Life.Increment(CType((.Spirit.Base * 0.1) * .LifeRegenerationModifier, Integer) + .LifeRegenBonus)
                                    Case Classes.CLASS_WARLOCK
                                        .Life.Increment(CType((.Spirit.Base * 0.11) * .LifeRegenerationModifier, Integer) + .LifeRegenBonus)
                                    Case Classes.CLASS_DRUID
                                        .Life.Increment(CType((.Spirit.Base * 0.11) * .LifeRegenerationModifier, Integer) + .LifeRegenBonus)
                                    Case Classes.CLASS_SHAMAN
                                        .Life.Increment(CType((.Spirit.Base * 0.11) * .LifeRegenerationModifier, Integer) + .LifeRegenBonus)
                                    Case Classes.CLASS_ROGUE
                                        .Life.Increment(CType((.Spirit.Base * 0.5) * .LifeRegenerationModifier, Integer) + .LifeRegenBonus)
                                    Case Classes.CLASS_WARRIOR
                                        .Life.Increment(CType((.Spirit.Base * 0.8) * .LifeRegenerationModifier, Integer) + .LifeRegenBonus)
                                    Case Classes.CLASS_HUNTER
                                        .Life.Increment(CType((.Spirit.Base * 0.25) * .LifeRegenerationModifier, Integer) + .LifeRegenBonus)
                                    Case Classes.CLASS_PALADIN
                                        .Life.Increment(CType((.Spirit.Base * 0.25) * .LifeRegenerationModifier, Integer) + .LifeRegenBonus)
                                End Select
                            End If

                            Dim UpdateData As New UpdateClass
                            'DONE: Send updates to players near
                            If BaseMana <> .Mana.Current Then
                                _updateFlag = True
                                UpdateData.SetUpdateFlag(EUnitFields.UNIT_FIELD_POWER1, CType(.Mana.Current, Integer))
                            End If
                            If BaseRage <> .Rage.Current Or ((.cUnitFlags And UnitFlags.UNIT_FLAG_IN_COMBAT) = UnitFlags.UNIT_FLAG_IN_COMBAT) Then
                                _updateFlag = True
                                UpdateData.SetUpdateFlag(EUnitFields.UNIT_FIELD_POWER2, CType(.Rage.Current, Integer))
                            End If
                            If BaseEnergy <> .Energy.Current Then
                                _updateFlag = True
                                UpdateData.SetUpdateFlag(EUnitFields.UNIT_FIELD_POWER4, CType(.Energy.Current, Integer))
                            End If
                            If BaseLife <> .Life.Current Then
                                _updateFlag = True
                                UpdateData.SetUpdateFlag(EUnitFields.UNIT_FIELD_HEALTH, CType(.Life.Current, Integer))
                            End If

                            If _updateFlag Then
                                Dim myPacket As New PacketClass(OPCODES.SMSG_UPDATE_OBJECT)
                                myPacket.AddInt32(1)      'Operations.Count
                                myPacket.AddInt8(0)
                                UpdateData.AddToPacket(myPacket, ObjectUpdateType.UPDATETYPE_VALUES, CType(Character.Value, CharacterObject), 1)
                                .Client.Send(myPacket)
                                myPacket.Dispose()

                                Dim tmpPacket As New PacketClass(OPCODES.SMSG_UPDATE_OBJECT)
                                tmpPacket.AddInt32(1)      'Operations.Count
                                tmpPacket.AddInt8(0)
                                UpdateData.AddToPacket(tmpPacket, ObjectUpdateType.UPDATETYPE_VALUES, CType(Character.Value, CharacterObject), 0)
                                .SendToNearPlayers(tmpPacket)
                                tmpPacket.Dispose()
                            End If
                            UpdateData.Dispose()


                            'DONE: Duel counter
                            If .DuelOutOfBounds <> DUEL_COUNTER_DISABLED Then
                                .DuelOutOfBounds -= REGENERATION_TIMER
                                If .DuelOutOfBounds = 0 Then DuelComplete(.DuelPartner, .Client.Character)
                            End If
                        End With
                    End If

                    'Send UPDATE_OUT_OF_RANGE
                    If Character.Value.guidsForRemoving.Count > 0 Then Character.Value.SendOutOfRangeUpdate()
                Next

            Catch ex As Exception
                Log.WriteLine(LogType.WARNING, "Error at regenerate.{0}", vbNewLine & ex.ToString)
            Finally
                CHARACTERs_Lock.ReleaseReaderLock()
            End Try
            RegenerationWorking = False
        End Sub
        Public Sub Dispose() Implements System.IDisposable.Dispose
            RegenerationTimer.Dispose()
            RegenerationTimer = Nothing
        End Sub
    End Class


    'NOTE: Manages spell durations and DOT spells
    Public Class TSpellManager
        Implements IDisposable

        Private SpellManagerTimer As Threading.Timer = Nothing
        Private SpellManagerWorking As Boolean = False

        Public Const UPDATE_TIMER As Integer = 1000        'Timer period (ms)
        Public Sub New()
            SpellManagerTimer = New Threading.Timer(AddressOf Update, Now, 10000, UPDATE_TIMER) 'AddressOf Update, Nothing equals to a possible System.ArgumentNullException: Value cannot be null? !!!Documented!!!
        End Sub
        Private Sub Update(ByVal state As Object)
            If SpellManagerWorking Then
                Log.WriteLine(LogType.WARNING, "Update: Spell Manager skipping update")
                Exit Sub
            End If

            SpellManagerWorking = True

            Try
                WORLD_CREATUREs_Lock.AcquireReaderLock(DEFAULT_LOCK_TIMEOUT)
                For Each de As KeyValuePair(Of ULong, CreatureObject) In WORLD_CREATUREs
                    UpdateSpells(de.Value)
                Next
            Catch ex As Exception
                Log.WriteLine(LogType.FAILED, ex.ToString, Nothing)
            Finally
                WORLD_CREATUREs_Lock.ReleaseReaderLock()
            End Try

            Try
                CHARACTERs_Lock.AcquireReaderLock(DEFAULT_LOCK_TIMEOUT)
                For Each Character As KeyValuePair(Of ULong, CharacterObject) In CHARACTERs
                    UpdateSpells(Character.Value)
                Next
            Catch ex As Exception
                Log.WriteLine(LogType.FAILED, ex.ToString, Nothing)
            Finally
                CHARACTERs_Lock.ReleaseReaderLock()
            End Try

            Dim DynamicObjectsToDelete As New List(Of DynamicObjectObject)
            Try
                WORLD_DYNAMICOBJECTs_Lock.AcquireReaderLock(DEFAULT_LOCK_TIMEOUT)
                For Each Dynamic As KeyValuePair(Of ULong, DynamicObjectObject) In WORLD_DYNAMICOBJECTs
                    If Dynamic.Value.Update(UPDATE_TIMER) Then
                        DynamicObjectsToDelete.Add(Dynamic.Value)
                    End If
                Next
            Catch ex As Exception
                Log.WriteLine(LogType.FAILED, ex.ToString, Nothing)
            Finally
                WORLD_DYNAMICOBJECTs_Lock.ReleaseReaderLock()
            End Try

            For Each Dynamic As DynamicObjectObject In DynamicObjectsToDelete
                Dynamic.Delete()
            Next

            SpellManagerWorking = False
        End Sub
        Public Sub Dispose() Implements System.IDisposable.Dispose
            SpellManagerTimer.Dispose()
            SpellManagerTimer = Nothing
        End Sub

        Private Sub UpdateSpells(ByVal c As BaseUnit)
            For i As Integer = 0 To MAX_AURA_EFFECTs_VISIBLE - 1
                If Not c.ActiveSpells(i) Is Nothing Then


                    'DONE: Count aura duration
                    If c.ActiveSpells(i).SpellDuration <> SPELL_DURATION_INFINITE Then
                        c.ActiveSpells(i).SpellDuration -= UPDATE_TIMER

                        'DONE: Cast aura (check if: there is aura; aura is periodic; time for next activation)
                        If Not c.ActiveSpells(i) Is Nothing AndAlso Not c.ActiveSpells(i).Aura1 Is Nothing AndAlso _
                        c.ActiveSpells(i).Aura1_Info.Amplitude <> 0 AndAlso _
                        ((c.ActiveSpells(i).GetSpellInfo.GetDuration - c.ActiveSpells(i).SpellDuration) Mod c.ActiveSpells(i).Aura1_Info.Amplitude) = 0 Then
                            c.ActiveSpells(i).Aura1.Invoke(c, c.ActiveSpells(i).SpellCaster, c.ActiveSpells(i).Aura1_Info, c.ActiveSpells(i).SpellID, c.ActiveSpells(i).StackCount + 1, WS_Spells.AuraAction.AURA_UPDATE)
                        End If
                        If Not c.ActiveSpells(i) Is Nothing AndAlso Not c.ActiveSpells(i).Aura2 Is Nothing AndAlso _
                        c.ActiveSpells(i).Aura2_Info.Amplitude <> 0 AndAlso _
                        ((c.ActiveSpells(i).GetSpellInfo.GetDuration - c.ActiveSpells(i).SpellDuration) Mod c.ActiveSpells(i).Aura2_Info.Amplitude) = 0 Then
                            c.ActiveSpells(i).Aura2.Invoke(c, c.ActiveSpells(i).SpellCaster, c.ActiveSpells(i).Aura2_Info, c.ActiveSpells(i).SpellID, c.ActiveSpells(i).StackCount + 1, WS_Spells.AuraAction.AURA_UPDATE)
                        End If
                        If Not c.ActiveSpells(i) Is Nothing AndAlso Not c.ActiveSpells(i).Aura3 Is Nothing AndAlso _
                        c.ActiveSpells(i).Aura3_Info.Amplitude <> 0 AndAlso _
                        ((c.ActiveSpells(i).GetSpellInfo.GetDuration - c.ActiveSpells(i).SpellDuration) Mod c.ActiveSpells(i).Aura3_Info.Amplitude) = 0 Then
                            c.ActiveSpells(i).Aura3.Invoke(c, c.ActiveSpells(i).SpellCaster, c.ActiveSpells(i).Aura3_Info, c.ActiveSpells(i).SpellID, c.ActiveSpells(i).StackCount + 1, WS_Spells.AuraAction.AURA_UPDATE)
                        End If

                        'DONE: Remove finished aura
                        If Not c.ActiveSpells(i) Is Nothing AndAlso c.ActiveSpells(i).SpellDuration <= 0 AndAlso c.ActiveSpells(i).SpellDuration <> SPELL_DURATION_INFINITE Then c.RemoveAura(i, c.ActiveSpells(i).SpellCaster)
                    End If


                End If
            Next

        End Sub
    End Class


        'NOTE: Manages ai movement
    Public Class TAIManager
        Implements IDisposable

        Public AIManagerTimer As Threading.Timer = Nothing
        Private AIManagerWorking As Boolean = False

        Public Const UPDATE_TIMER As Integer = 1000     'Timer period (ms)
        Public Sub New()
            AIManagerTimer = New Threading.Timer(AddressOf Update, Nothing, 10000, UPDATE_TIMER)
        End Sub
        Private Sub Update(ByVal state As Object)
            If AIManagerWorking Then
                Log.WriteLine(LogType.WARNING, "Update: AI Manager skipping update")
                Exit Sub
            End If

            Dim aiCreature As CreatureObject = Nothing
            Dim iKey As Long = 0

            Dim StartTime As Integer = timeGetTime
            AIManagerWorking = True
            Try
                WORLD_CREATUREs_Lock.AcquireReaderLock(DEFAULT_LOCK_TIMEOUT)

                '''''For Each de As KeyValuePair(Of ULong, CreatureObject) In WORLD_CREATUREsClone
                'For Each de As KeyValuePair(Of ULong, CreatureObject) In WORLD_CREATUREs
                '    If de.Value IsNot Nothing AndAlso de.Value.aiScript IsNot Nothing Then de.Value.aiScript.DoThink()
                'Next
                'For i As Long = 0 To WORLD_CREATUREsKeys.Count - 1
                'If WORLD_CREATUREs(WORLD_CREATUREsKeys(i)) IsNot Nothing AndAlso WORLD_CREATUREs(WORLD_CREATUREsKeys(i)).aiScript IsNot Nothing Then
                'WORLD_CREATUREs(WORLD_CREATUREsKeys(i)).aiScript.DoThink()
                'End If
                'Next

                aiCreature = Nothing

            Catch ex As Exception
                Log.WriteLine(LogType.CRITICAL, ex.ToString, Nothing)
            Finally
                WORLD_CREATUREs_Lock.ReleaseReaderLock()
            End Try
            AIManagerWorking = False
        End Sub
        Public Sub Dispose() Implements System.IDisposable.Dispose
            AIManagerTimer.Dispose()
            AIManagerTimer = Nothing
        End Sub

        Protected Overrides Sub Finalize()
            MyBase.Finalize()
        End Sub
    End Class


    'NOTE: Manages character savings
    Public Class TCharacterSaver
        Implements IDisposable

        Public CharacterSaverTimer As Threading.Timer = Nothing
        Private CharacterSaverWorking As Boolean = False

        Public UPDATE_TIMER As Integer = Config.SaveTimer     'Timer period (ms)
        Public Sub New()
            CharacterSaverTimer = New Threading.Timer(AddressOf Update, Nothing, 10000, UPDATE_TIMER)
        End Sub
        Private Sub Update(ByVal state As Object)
            If CharacterSaverWorking Then
                Log.WriteLine(LogType.WARNING, "Update: Character Saver skipping update")
                Exit Sub
            End If

            CharacterSaverWorking = True
            Try
                CHARACTERs_Lock.AcquireReaderLock(DEFAULT_LOCK_TIMEOUT)
                For Each Character As KeyValuePair(Of ULong, CharacterObject) In CHARACTERs
                    Character.Value.SaveCharacter()
                Next
            Catch ex As Exception
                Log.WriteLine(LogType.FAILED, ex.ToString, Nothing)
            Finally
                CHARACTERs_Lock.ReleaseReaderLock()
            End Try
            CharacterSaverWorking = False
        End Sub
        Public Sub Dispose() Implements System.IDisposable.Dispose
            CharacterSaverTimer.Dispose()
            CharacterSaverTimer = Nothing
        End Sub
    End Class

    'TODO: Timer for kicking not connected players (ping timeout)
    'TODO: Timer for auction items and mails
    'TODO: Timer for weather change

End Module


