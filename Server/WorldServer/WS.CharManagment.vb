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
Imports System.Reflection
Imports System.Text.RegularExpressions
Imports Spurious.Common.BaseWriter



Public Module WS_CharManagment



#Region "WS.CharMangment.CharacterInitializators"
    Enum ManaTypes
        TYPE_MANA = 0
        TYPE_RAGE = 1
        TYPE_FOCUS = 2
        TYPE_ENERGY = 3
        TYPE_HAPPINESS = 4
    End Enum

    Public Const groundFlagsMask As Integer = &HFFFFFFFF And Not (MovementFlags.MOVEMENTFLAG_LEFT Or MovementFlags.MOVEMENTFLAG_RIGHT Or MovementFlags.MOVEMENTFLAG_BACKWARD Or MovementFlags.MOVEMENTFLAG_FORWARD Or MovementFlags.MOVEMENTFLAG_WALK)
    Public Const movementFlagsMask As Integer = MovementFlags.MOVEMENTFLAG_FORWARD Or MovementFlags.MOVEMENTFLAG_BACKWARD Or MovementFlags.MOVEMENTFLAG_STRAFE_LEFT Or _
    MovementFlags.MOVEMENTFLAG_STRAFE_RIGHT Or MovementFlags.MOVEMENTFLAG_PITCH_UP Or MovementFlags.MOVEMENTFLAG_PITCH_DOWN Or MovementFlags.MOVEMENTFLAG_FLY_UNK1 Or MovementFlags.MOVEMENTFLAG_JUMPING Or _
    MovementFlags.MOVEMENTFLAG_FALLING Or MovementFlags.MOVEMENTFLAG_SWIMMING Or MovementFlags.MOVEMENTFLAG_FLY_UP Or MovementFlags.MOVEMENTFLAG_FLYING Or MovementFlags.MOVEMENTFLAG_SPLINE
    Public Const TurningFlagsMask As Integer = MovementFlags.MOVEMENTFLAG_LEFT Or MovementFlags.MOVEMENTFLAG_RIGHT
    Public Const movementOrTurningFlagsMask As Integer = movementFlagsMask Or TurningFlagsMask

    Public XPTable(80) As Integer
    Public MAX_LEVEL As Integer = 70

    Public Function CalculateStartingLIFE(ByRef c As CharacterObject, ByVal baseLIFE As Integer) As Integer
        If (c.Stamina.Base < 20) Then
            Return baseLIFE + (c.Stamina.Base - 20)
        Else
            Return baseLIFE + 10 * (c.Stamina.Base - 20)
        End If
    End Function
    Public Function CalculateStartingMANA(ByRef c As CharacterObject, ByVal baseMANA As Integer) As Integer
        If (c.Intellect.Base < 20) Then
            Return baseMANA + (c.Intellect.Base - 20)
        Else
            Return baseMANA + 15 * (c.Intellect.Base - 20)
        End If
    End Function
    Private Function gainStat(ByVal level As Integer, ByVal a3 As Double, ByVal a2 As Double, ByVal a1 As Double, ByVal a0 As Double) As Integer
        Return CType(System.Math.Round(a3 * level * level * level + a2 * level * level + a1 * level + a0), Integer) - _
                CType(System.Math.Round(a3 * (level - 1) * (level - 1) * (level - 1) + a2 * (level - 1) * (level - 1) + a1 * (level - 1) + a0), Integer)
    End Function
    Public Sub CalculateOnLevelUP(ByRef c As CharacterObject)
        Dim baseInt As Integer = c.Intellect.Base
        'Dim baseStr As Integer = c.Strength.Base
        'Dim baseSta As Integer = c.Stamina.Base
        Dim baseSpi As Integer = c.Spirit.Base
        Dim baseAgi As Integer = c.Agility.Base
        'Dim baseMana As Integer = c.Mana.Maximum
        Dim baseLife As Integer = c.Life.Maximum

        Select Case c.Classe
            Case Classes.CLASS_DRUID
                If c.Level <= 17 Then
                    c.Life.Base += 17
                Else
                    c.Life.Base += c.Level
                End If
                If c.Level <= 25 Then
                    c.Mana.Base += 20 + c.Level
                Else
                    c.Mana.Base += 45
                End If
                c.Strength.Base += gainStat(c.Level, 0.000021, 0.003009, 0.486493, -0.400003)
                c.Intellect.Base += gainStat(c.Level, 0.000038, 0.005145, 0.871006, -0.832029)
                c.Agility.Base += gainStat(c.Level, 0.000041, 0.00044, 0.512076, -1.000317)
                c.Stamina.Base += gainStat(c.Level, 0.000023, 0.003345, 0.56005, -0.562058)
                c.Spirit.Base += gainStat(c.Level, 0.000059, 0.004044, 1.04, -1.488504)
            Case Classes.CLASS_HUNTER
                If c.Level <= 13 Then
                    c.Life.Base += 17
                Else
                    c.Life.Base += c.Level + 4
                End If
                If c.Level <= 27 Then
                    c.Mana.Base += 18 + c.Level
                Else
                    c.Mana.Base += 45
                End If
                c.Strength.Base += gainStat(c.Level, 0.000022, 0.0018, 0.407867, -0.550889)
                c.Intellect.Base += gainStat(c.Level, 0.00002, 0.003007, 0.505215, -0.500642)
                c.Agility.Base += gainStat(c.Level, 0.00004, 0.007416, 1.125108, -1.003045)
                c.Stamina.Base += gainStat(c.Level, 0.000031, 0.00448, 0.78004, -0.800471)
                c.Spirit.Base += gainStat(c.Level, 0.000017, 0.003803, 0.536846, -0.490026)
            Case Classes.CLASS_MAGE
                If c.Level <= 25 Then
                    c.Life.Base += 15
                Else
                    c.Life.Base += c.Level - 8
                End If
                If c.Level <= 27 Then
                    c.Mana.Base += 23 + c.Level
                Else
                    c.Mana.Base += 51
                End If
                c.Strength.Base += gainStat(c.Level, 0.000002, 0.001003, 0.10089, -0.076055)
                c.Intellect.Base += gainStat(c.Level, 0.00004, 0.007416, 1.125108, -1.003045)
                c.Agility.Base += gainStat(c.Level, 0.000008, 0.001001, 0.16319, -0.06428)
                c.Stamina.Base += gainStat(c.Level, 0.000006, 0.002031, 0.27836, -0.340077)
                c.Spirit.Base += gainStat(c.Level, 0.000039, 0.006981, 1.09009, -1.00607)
            Case Classes.CLASS_PALADIN
                If c.Level <= 14 Then
                    c.Life.Base += 18
                Else
                    c.Life.Base += c.Level + 4
                End If
                If c.Level <= 25 Then
                    c.Mana.Base += 17 + c.Level
                Else
                    c.Mana.Base += 42
                End If
                c.Strength.Base += gainStat(c.Level, 0.000037, 0.005455, 0.940039, -1.00009)
                c.Intellect.Base += gainStat(c.Level, 0.000023, 0.003345, 0.56005, -0.562058)
                c.Agility.Base += gainStat(c.Level, 0.00002, 0.003007, 0.505215, -0.500642)
                c.Stamina.Base += gainStat(c.Level, 0.000038, 0.005145, 0.871006, -0.832029)
                c.Spirit.Base += gainStat(c.Level, 0.000032, 0.003025, 0.61589, -0.640307)
            Case Classes.CLASS_PRIEST
                If c.Level <= 22 Then
                    c.Life.Base += 15
                Else
                    c.Life.Base += c.Level - 6
                End If
                If c.Level <= 33 Then
                    c.Mana.Base += 22 + c.Level
                Else
                    c.Mana.Base += 54
                End If
                If c.Level = 34 Then c.Mana.Base += 15
                c.Strength.Base += gainStat(c.Level, 0.000008, 0.001001, 0.16319, -0.06428)
                c.Intellect.Base += gainStat(c.Level, 0.000039, 0.006981, 1.09009, -1.00607)
                c.Agility.Base += gainStat(c.Level, 0.000022, 0.000022, 0.260756, -0.494)
                c.Stamina.Base += gainStat(c.Level, 0.000024, 0.000981, 0.364935, -0.5709)
                c.Spirit.Base += gainStat(c.Level, 0.00004, 0.007416, 1.125108, -1.003045)
            Case Classes.CLASS_ROGUE
                If c.Level <= 15 Then
                    c.Life.Base += 17
                Else
                    c.Life.Base += c.Level + 2
                End If
                c.Strength.Base += gainStat(c.Level, 0.000025, 0.00417, 0.654096, -0.601491)
                c.Intellect.Base += gainStat(c.Level, 0.000008, 0.001001, 0.16319, -0.06428)
                c.Agility.Base += gainStat(c.Level, 0.000038, 0.007834, 1.191028, -1.20394)
                c.Stamina.Base += gainStat(c.Level, 0.000032, 0.003025, 0.61589, -0.640307)
                c.Spirit.Base += gainStat(c.Level, 0.000024, 0.000981, 0.364935, -0.5709)
            Case Classes.CLASS_SHAMAN
                If c.Level <= 16 Then
                    c.Life.Base += 17
                Else
                    c.Life.Base += c.Level + 1
                End If
                If c.Level <= 32 Then
                    c.Mana.Base += 19 + c.Level
                Else
                    c.Mana.Base += 52
                End If
                c.Strength.Base += gainStat(c.Level, 0.000035, 0.003641, 0.73431, -0.800626)
                c.Intellect.Base += gainStat(c.Level, 0.000031, 0.00448, 0.78004, -0.800471)
                c.Agility.Base += gainStat(c.Level, 0.000022, 0.0018, 0.407867, -0.550889)
                c.Stamina.Base += gainStat(c.Level, 0.00002, 0.00603, 0.80957, -0.80922)
                c.Spirit.Base += gainStat(c.Level, 0.000038, 0.005145, 0.871006, -0.832029)
            Case Classes.CLASS_WARLOCK
                If c.Level <= 17 Then
                    c.Life.Base += 15
                Else
                    c.Life.Base += c.Level - 2
                End If
                If c.Level <= 30 Then
                    c.Mana.Base += 21 + c.Level
                Else
                    c.Mana.Base += 51
                End If
                c.Strength.Base += gainStat(c.Level, 0.000006, 0.002031, 0.27836, -0.340077)
                c.Intellect.Base += gainStat(c.Level, 0.000059, 0.004044, 1.04, -1.488504)
                c.Agility.Base += gainStat(c.Level, 0.000024, 0.000981, 0.364935, -0.5709)
                c.Stamina.Base += gainStat(c.Level, 0.000021, 0.003009, 0.486493, -0.400003)
                c.Spirit.Base += gainStat(c.Level, 0.00004, 0.006404, 1.038791, -1.039076)
            Case Classes.CLASS_WARRIOR
                If c.Level <= 14 Then
                    c.Life.Base += 19
                Else
                    c.Life.Base += c.Level + 10
                End If
                c.Strength.Base += gainStat(c.Level, 0.000039, 0.006902, 1.08004, -1.051701)
                c.Intellect.Base += gainStat(c.Level, 0.000002, 0.001003, 0.10089, -0.076055)
                c.Agility.Base += gainStat(c.Level, 0.000022, 0.0046, 0.655333, -0.600356)
                c.Stamina.Base += gainStat(c.Level, 0.000059, 0.004044, 1.04, -1.488504)
                c.Spirit.Base += gainStat(c.Level, 0.000006, 0.002031, 0.27836, -0.340077)
        End Select

        'Calculate new spi/int gain
        If c.Agility.Base <> baseAgi Then c.Resistances(DamageTypes.DMG_PHYSICAL).Base += (c.Agility.Base - baseAgi) * 2
        If c.Spirit.Base <> baseSpi Then c.Life.Base += 10 * (c.Spirit.Base - baseSpi)
        If c.Intellect.Base <> baseInt AndAlso c.ManaType = ManaTypes.TYPE_MANA Then c.Mana.Base += 15 * (c.Intellect.Base - baseInt)

        c.Damage.Minimum += 1
        c.RangedDamage.Minimum += 1
        c.Damage.Maximum += 1
        c.RangedDamage.Maximum += 1
        If c.Level > 9 Then c.TalentPoints += 1

        For Each Skill As KeyValuePair(Of Integer, TSkill) In c.Skills
            If SkillLines(CType(Skill.Key, Integer)) = SKILL_LineCategory.WEAPON_SKILLS Then
                CType(Skill.Value, TSkill).Base += 5
            End If
        Next
    End Sub
    Public Function GetClassManaType(ByVal Classe As Classes) As ManaTypes
        Select Case Classe
            Case Classes.CLASS_DRUID, Classes.CLASS_HUNTER, Classes.CLASS_MAGE, Classes.CLASS_PALADIN, Classes.CLASS_PRIEST, Classes.CLASS_SHAMAN, Classes.CLASS_WARLOCK
                Return ManaTypes.TYPE_MANA
            Case Classes.CLASS_ROGUE
                Return ManaTypes.TYPE_ENERGY
            Case Classes.CLASS_WARRIOR
                Return ManaTypes.TYPE_RAGE
            Case Else
                Return ManaTypes.TYPE_MANA
        End Select
    End Function

    Public Sub InitializeReputations(ByRef c As CharacterObject)
        Dim i As Byte
        For i = 0 To 63
            c.Reputation(i) = New TReputation
            c.Reputation(i).Value = 0
            c.Reputation(i).Flags = 0

            For Each tmpFactionInfo As KeyValuePair(Of Integer, TFaction) In FactionInfo
                If tmpFactionInfo.Value.VisibleID = i Then
                    If HaveFlag(tmpFactionInfo.Value.flags(0), c.Race - 1) Then
                        c.Reputation(i).Flags = tmpFactionInfo.Value.rep_flags(0)
                    ElseIf HaveFlag(tmpFactionInfo.Value.flags(1), c.Race - 1) Then
                        c.Reputation(i).Flags = tmpFactionInfo.Value.rep_flags(1)
                    ElseIf HaveFlag(tmpFactionInfo.Value.flags(2), c.Race - 1) Then
                        c.Reputation(i).Flags = tmpFactionInfo.Value.rep_flags(2)
                    ElseIf HaveFlag(tmpFactionInfo.Value.flags(3), c.Race - 1) Then
                        c.Reputation(i).Flags = tmpFactionInfo.Value.rep_flags(3)
                    End If
                    Exit For
                End If
            Next
        Next
    End Sub
#End Region
#Region "WS.CharMangment.CharacterHelpingTypes"
    Public Class TSkill
        Private _Current As Int16 = 0
        Public Bonus As Int16 = 0
        Public Base As Int16 = 375
        Public Sub New(ByVal CurrentVal As Int16, Optional ByVal MaximumVal As Int16 = 375)
            Current = CurrentVal
            Base = MaximumVal
        End Sub
        Public Sub Increment(Optional ByVal Incrementator As Int16 = 1)
            If (Current + Incrementator) < Base Then
                Current = Current + Incrementator
            Else
                Current = Base
            End If
        End Sub
        Public ReadOnly Property Maximum() As Integer
            Get
                Return Bonus + Base
            End Get
        End Property
        Public Property Current() As Int16
            Get
                Return _Current
            End Get
            Set(ByVal Value As Int16)
                If Value <= Maximum Then _Current = Value
            End Set
        End Property

        Public ReadOnly Property GetSkill() As Integer
            Get
                Return CType((_Current + (CType(Base + Bonus, Integer) << 16)), Integer)
            End Get
        End Property
    End Class
    Public Class TStatBar
        Private _Current As Integer = 0
        Public Bonus As Integer = 0
        Public Base As Integer = 0
        Public Modifier As Single = 1
        Public Sub Increment(Optional ByVal Incrementator As Integer = 1)
            If (Current + Incrementator) < (Bonus + Base) Then
                Current = Current + Incrementator
            Else
                Current = Maximum
            End If
        End Sub
        Public Sub New(ByVal CurrentVal As Integer, ByVal BaseVal As Integer, ByVal BonusVal As Integer)
            _Current = CurrentVal
            Bonus = BonusVal
            Base = BaseVal
        End Sub
        Public ReadOnly Property Maximum() As Integer
            Get
                Return (Bonus + Base) * Modifier
            End Get
        End Property
        Public Property Current() As Integer
            Get
                Return _Current * Modifier
            End Get
            Set(ByVal Value As Integer)
                If Value <= Me.Maximum Then _Current = Value Else _Current = Me.Maximum
                If _Current < 0 Then _Current = 0
            End Set
        End Property
    End Class
    Public Class TStat
        Public Base As Short = 0
        Public PositiveBonus As Short = 0
        Public NegativeBonus As Short = 0
        Public BaseModifier As Single = 1
        Public Modifier As Single = 1
        Public Property RealBase() As Integer
            Get
                Return (Base - PositiveBonus + NegativeBonus)
            End Get
            Set(ByVal value As Integer)
                Base = Base - PositiveBonus + NegativeBonus
                Base = value
                Base = Base + PositiveBonus - NegativeBonus
            End Set
        End Property

        Public Sub New(Optional ByVal BaseValue As Byte = 0, Optional ByVal PosValue As Byte = 0, Optional ByVal NegValue As Byte = 0)
            Base = BaseValue
            PositiveBonus = PosValue
            PositiveBonus = NegValue
        End Sub
    End Class
    Public Class TDamageBonus
        Public PositiveBonus As Integer = 0
        Public NegativeBonus As Integer = 0
        Public Modifier As Single = 1
        Public ReadOnly Property Value() As Integer
            Get
                Return (PositiveBonus - NegativeBonus) * Modifier
            End Get
        End Property

        Public Sub New(Optional ByVal PosValue As Byte = 0, Optional ByVal NegValue As Byte = 0)
            PositiveBonus = PosValue
            PositiveBonus = NegValue
        End Sub
    End Class
    Public Class THonor
        Public HonorPounts As Short = 0                 '! MAX=1000 ?
        Public HonorRank As Byte = 0
        Public HonorHightestRank As Byte = 0
        Public Standing As Integer = 0

        Public HonorLastWeek As Integer = 0
        Public HonorThisWeek As Integer = 0
        Public HonorYesterday As Integer = 0

        Public KillsLastWeek As Integer = 0
        Public KillsThisWeek As Integer = 0
        Public KillsYesterday As Integer = 0

        Public KillsHonorableToday As Integer = 0
        Public KillsDisHonorableToday As Integer = 0
        Public KillsHonorableLifetime As Integer = 0
        Public KillsDisHonorableLifetime As Integer = 0

        Public Sub Save(ByVal GUID As ULong)
            Dim tmp As String = "UPDATE characters_honor SET"

            tmp = tmp & " honor_points=""" & HonorPounts & """"
            tmp = tmp & ", honor_rank=" & HonorRank
            tmp = tmp & ", honor_hightestRank=" & HonorHightestRank
            tmp = tmp & ", honor_standing=" & Standing
            tmp = tmp & ", honor_lastWeekContribution=" & HonorLastWeek
            tmp = tmp & ", honor_thisWeebContribution=" & HonorThisWeek
            tmp = tmp & ", honor_yesterdayContribution=" & HonorYesterday
            tmp = tmp & ", kills_lastWeek=" & KillsLastWeek
            tmp = tmp & ", kills_thisWeek=" & KillsThisWeek
            tmp = tmp & ", kills_yesterday=" & KillsYesterday
            tmp = tmp & ", kills_dishonorableToday=" & KillsDisHonorableToday
            tmp = tmp & ", kills_honorableToday=" & KillsHonorableToday
            tmp = tmp & ", kills_dishonorableLifetime=" & KillsDisHonorableLifetime
            tmp = tmp & ", kills_honorableLifetime=" & KillsHonorableLifetime

            tmp = tmp + String.Format(" WHERE char_guid = ""{0}"";", GUID)
            Database.Update(tmp)
        End Sub
        Public Sub Load(ByVal GUID As ULong)

        End Sub
        Public Sub SaveAsNew(ByVal GUID As ULong)

        End Sub
    End Class
    Public Class TReputation
        '1:"AtWar" clickable but not checked
        '3:"AtWar" clickable and checked
        Public Flags As Integer = 0
        Public Value As Integer = 0
    End Class
    Public Class TActionButton
        Public ActionType As Byte = 0
        Public ActionMisc As Byte = 0
        Public Action As Integer = 0
        Public Sub New(ByVal Action_ As Integer, ByVal Type_ As Byte, ByVal Misc_ As Byte)
            ActionType = Type_
            ActionMisc = Misc_
            Action = Action_
        End Sub
    End Class
    Public Class TDrowningTimer
        Implements IDisposable

        Private DrowningTimer As Threading.Timer = Nothing
        Public DrowningValue As Integer = 70000
        Public DrowningDamage As Byte = 1
        Public CharacterGUID As ULong = 0

        Public Sub New(ByRef Character As CharacterObject)
            CharacterGUID = Character.GUID
            Character.StartMirrorTimer(MirrorTimer.DROWNING, 70000)
            DrowningTimer = New Threading.Timer(AddressOf Character.HandleDrowning, Nothing, 2000, 1000)
        End Sub
        Public Sub Dispose() Implements System.IDisposable.Dispose
            DrowningTimer.dispose()
            DrowningTimer = Nothing
            If CHARACTERs.ContainsKey(CharacterGUID) Then CHARACTERs(CharacterGUID).StopMirrorTimer(1)
        End Sub
    End Class
    Public Class TRepopTimer
        Implements IDisposable

        Private RepopTimer As Threading.Timer = Nothing
        Public CharacterGUID As ULong = 0

        Public Sub New(ByRef Character As CharacterObject)
            CharacterGUID = Character.GUID
            RepopTimer = New Threading.Timer(AddressOf Repop, Nothing, 360000, 360000)
        End Sub
        Public Sub Repop(ByVal Obj As Object)
            CharacterRepop(CHARACTERs(CharacterGUID).Client)
            CHARACTERs(CharacterGUID).repopTimer = Nothing
            Me.Dispose()
        End Sub
        Public Sub Dispose() Implements System.IDisposable.Dispose
            RepopTimer.dispose()
            RepopTimer = Nothing
        End Sub
    End Class
#End Region
#Region "WS.CharMangment.CharacterHelpingSubs"

    Public Sub SendBindPointUpdate(ByRef Client As ClientClass, ByRef Character As CharacterObject)
        Dim SMSG_BINDPOINTUPDATE As New PacketClass(OPCODES.SMSG_BINDPOINTUPDATE)
        SMSG_BINDPOINTUPDATE.AddSingle(Character.bindpoint_positionX)
        SMSG_BINDPOINTUPDATE.AddSingle(Character.bindpoint_positionY)
        SMSG_BINDPOINTUPDATE.AddSingle(Character.bindpoint_positionZ)
        SMSG_BINDPOINTUPDATE.AddInt32(Character.bindpoint_map_id)
        SMSG_BINDPOINTUPDATE.AddInt32(Character.bindpoint_zone_id)
        Client.Send(SMSG_BINDPOINTUPDATE)
        SMSG_BINDPOINTUPDATE.Dispose()

        Log.WriteLine(LogType.DEBUG, "[{0}:{1}] SMSG_BINDPOINTUPDATE", Client.IP, Client.Port)
    End Sub
    Public Sub Send_SMSG_SET_REST_START(ByRef Client As ClientClass, ByRef Character As CharacterObject)
        Dim SMSG_SET_REST_START As New PacketClass(OPCODES.SMSG_SET_REST_START)
        SMSG_SET_REST_START.AddInt32(timeGetTime)
        Client.Send(SMSG_SET_REST_START)
        SMSG_SET_REST_START.Dispose()
        Log.WriteLine(LogType.DEBUG, "[{0}:{1}] SMSG_SET_REST_START", Client.IP, Client.Port)
    End Sub
    Public Sub SendTutorialFlags(ByRef Client As ClientClass, ByRef Character As CharacterObject)
        Dim SMSG_TUTORIAL_FLAGS As New PacketClass(OPCODES.SMSG_TUTORIAL_FLAGS)
        '[8*Int32] or [32 Bytes] or [256 Bits Flags] Total!!!
        'SMSG_TUTORIAL_FLAGS.AddInt8(0)
        'SMSG_TUTORIAL_FLAGS.AddInt8(Character.TutorialFlags.Length)
        SMSG_TUTORIAL_FLAGS.AddByteArray(Character.TutorialFlags)
        Client.Send(SMSG_TUTORIAL_FLAGS)
        SMSG_TUTORIAL_FLAGS.Dispose()

        Log.WriteLine(LogType.DEBUG, "[{0}:{1}] SMSG_TUTORIAL_FLAGS", Client.IP, Client.Port)
    End Sub
    Public Sub SendFactions(ByRef Client As ClientClass, ByRef Character As CharacterObject)
        Dim packet As New PacketClass(OPCODES.SMSG_INITIALIZE_FACTIONS)
        Dim i As Byte

        packet.AddInt32(64)
        For i = 0 To 63
            packet.AddInt8(Character.Reputation(i).Flags)                               'Flags
            packet.AddInt32(Character.Reputation(i).Value)                              'Standing
        Next i

        Client.Send(packet)
        packet.Dispose()
        Log.WriteLine(LogType.DEBUG, "[{0}:{1}] SMSG_INITIALIZE_FACTIONS", Client.IP, Client.Port)
    End Sub
    Public Sub SendActionButtons(ByRef Client As ClientClass, ByRef Character As CharacterObject)
        Dim packet As New PacketClass(OPCODES.SMSG_ACTION_BUTTONS)

        Dim i As Byte
        For i = 0 To 119    'or 480 ?
            If Character.ActionButtons.ContainsKey(i) Then
                packet.AddUInt16(Character.ActionButtons(i).Action)
                packet.AddInt8(Character.ActionButtons(i).ActionType)
                packet.AddInt8(Character.ActionButtons(i).ActionMisc)
            Else
                packet.AddInt32(0)
            End If
        Next

        Client.Send(packet)
        packet.Dispose()
        Log.WriteLine(LogType.DEBUG, "[{0}:{1}] SMSG_ACTION_BUTTONS", Client.IP, Client.Port)
    End Sub
    Public Sub SendInitialSpells(ByRef Client As ClientClass, ByRef Character As CharacterObject)
        Dim packet As New PacketClass(OPCODES.SMSG_INITIAL_SPELLS)
        packet.AddInt8(0)
        packet.AddInt16(Character.Spells.Count)

        For Each Spell As Integer In Character.Spells
            'packet.AddInt16(Spell.Key)      'SpellID
            'packet.AddInt16(Spell.Value)    'SlotID
            packet.AddInt32(Spell)
        Next
        packet.AddInt16(0)

        Client.Send(packet)
        packet.Dispose()
        Log.WriteLine(LogType.DEBUG, "[{0}:{1}] SMSG_INITIAL_SPELLS", Client.IP, Client.Port)
    End Sub


    Public Sub InitializeTalentSpells(ByVal c As CharacterObject)
        Dim t As New SpellTargets
        t.SetTarget_SELF(CType(c, CharacterObject))

        For Each SpellID As Integer In c.Spells
            If SPELLs.ContainsKey(SpellID) AndAlso (SPELLs(SpellID).IsPassive) Then
                'DONE: Add passive spell we don't have
                'DONE: Remove passive spells we can't have anymore
                If c.HavePassiveAura(SpellID) = False AndAlso SPELLs(SpellID).CanCast(c, t) = SpellFailedReason.CAST_NO_ERROR Then
                    SPELLs(SpellID).Apply(CType(c, CharacterObject), t)
                ElseIf c.HavePassiveAura(SpellID) AndAlso SPELLs(SpellID).CanCast(c, t) <> SpellFailedReason.CAST_NO_ERROR Then
                    c.RemoveAuraBySpell(SpellID)
                End If
            End If
        Next
    End Sub

#End Region

#Region "WS.CharMangment.CharacterDataType"


    Public Const ITEM_SLOT_NULL As Byte = 255
    Public Const ITEM_BAG_NULL As Long = -1

    Public Class CharacterObject
        Inherits BaseUnit
        Implements IDisposable

        'Connection Information
        Public Client As ClientClass
        Public Access As AccessLevel = AccessLevel.Player
        Public LogoutTimer As Threading.Timer
        Public FullyLoggedIn As Boolean = False

        'Character Information
        Public TargetGUID As ULong = 0
        Public Model_Native As Integer = 0
        Public cPlayerFlags As Integer = 0
        Public Rage As New TStatBar(1, 1, 0)
        Public Energy As New TStatBar(1, 1, 0)
        Public Strength As New TStat
        Public Agility As New TStat
        Public Stamina As New TStat
        Public Intellect As New TStat
        Public Spirit As New TStat
        Public Faction As Short = FactionTemplates.None

        'Combat related
        Public attackState As TAttackTimer = New TAttackTimer(Me)
        Public attackSelection As BaseObject = Nothing
        Public attackSheathState As SHEATHE_SLOT = SHEATHE_SLOT.SHEATHE_NONE

        ' Miscellaneous Information
        Public MenuNumber As Integer = 0

        Public ReadOnly Property GetCriticalWithSpells() As Byte
            ' From http://www.wowwiki.com/Spell_critical_strike
            ' TODO: Need to add SpellCritical Value in this format -- (Intellect/80 '82 for Warlocks) + (Spell Critical Strike Rating/22.08) + Class Specific Constant 
            ' How do you generate the base spell crit rating... and then we can fix the formula
            Get
                Select Case Classe
                    Case Classes.CLASS_DRUID
                        Return Fix(Intellect.Base / 80 + 1.85F)
                    Case Classes.CLASS_MAGE
                        Return Fix(Intellect.Base / 80 + 0.91F)
                    Case Classes.CLASS_PRIEST
                        Return Fix(Intellect.Base / 80 + 1.24F)
                    Case Classes.CLASS_WARLOCK
                        Return Fix(Intellect.Base / 82 + 1.701F)
                    Case Classes.CLASS_PALADIN
                        Return Fix(Intellect.Base / 80 + 3.336F)
                    Case Classes.CLASS_SHAMAN
                        Return Fix(Intellect.Base / 80 + 2.2F)
                    Case Classes.CLASS_HUNTER
                        Return Fix(Intellect.Base / 80 + 3.6F)
                    Case Else
                        'CLASS_ROGUE
                        'CLASS_WARRIOR
                        Return 0
                End Select

            End Get
        End Property
        Public spellCastState As SpellCastState = WS_Spells.SpellCastState.SPELL_STATE_IDLE
        Public spellCasted As Integer = 0
        Public spellRandom As Integer = 0
        Public spellCastManaRegeneration As Byte = 0
        Public spellCanDualWeild As Boolean = False
        Public healing As New TDamageBonus
        Public spellDamage(6) As TDamageBonus
        Public spellCriticalRating As Integer = 0
        Public combatCanDualWield As Boolean = False
        Public combatBlock As Integer = 0
        Public combatBlockValue As Integer = 0
        Public combatParry As Integer = 0
        Public combatCrit As Integer = 0
        Public combatDodge As Integer = 0
        Public Resistances(6) As TStat
        Public Damage As New TDamage
        Public RangedDamage As New TDamage
        Public OffHandDamage As New TDamage
        Public ReadOnly Property BaseUnarmedDamage() As Integer
            Get
                Return (AttackPower + AttackPowerMods) * 0.071428571428571425
            End Get
        End Property
        Public ReadOnly Property BaseRangedDamage() As Integer
            Get
                Return (AttackPowerRanged + AttackPowerModsRanged) * 0.071428571428571425
            End Get
        End Property
        Public ReadOnly Property AttackPower() As Integer
            ' From http://www.wowwiki.com/Attack_power
            Get
                Select Case Classe
                    Case Classes.CLASS_WARRIOR, Classes.CLASS_PALADIN
                        Return (Level * 3 + Strength.Base * 3 - 20)
                    Case Classes.CLASS_SHAMAN
                        Return (Level * 2 + Strength.Base * 2 - 20)
                    Case Classes.CLASS_MAGE, Classes.CLASS_PRIEST, Classes.CLASS_WARLOCK
                        Return (Strength.Base - 10)
                    Case Classes.CLASS_ROGUE, Classes.CLASS_HUNTER
                        Return (Level * 2 + Strength.Base + Agility.Base - 20)
                    Case Classes.CLASS_DRUID
                        If Me.ShapeshiftForm = WS_Spells.ShapeshiftForm.FORM_CAT Then
                            Return (Level * 2 + Strength.Base * 2 + Agility.Base - 20)
                        ElseIf Me.ShapeshiftForm = WS_Spells.ShapeshiftForm.FORM_BEAR Or Me.ShapeshiftForm = WS_Spells.ShapeshiftForm.FORM_DIREBEAR Then
                            Return (Level * 3 + Strength.Base * 2 - 20)
                        ElseIf Me.ShapeshiftForm = WS_Spells.ShapeshiftForm.FORM_MOONKIN Then
                            Return (Level * 1.5 + Agility.Base + Strength.Base * 2 - 20)
                        Else
                            Return (Strength.Base * 2 - 20)
                        End If
                End Select
            End Get
        End Property
        Public ReadOnly Property AttackPowerRanged() As Integer
            ' From http://www.wowwiki.com/Attack_power
            Get
                Select Case Classe
                    Case Classes.CLASS_WARRIOR, Classes.CLASS_ROGUE
                        Return (Level + Agility.Base - 10)
                    Case Classes.CLASS_HUNTER
                        Return (Level * 2 + Agility.Base - 10)
                    Case Classes.CLASS_PALADIN, Classes.CLASS_SHAMAN, Classes.CLASS_MAGE, Classes.CLASS_PRIEST, Classes.CLASS_WARLOCK
                        Return (Agility.Base - 10)
                    Case Classes.CLASS_DRUID
                        If Me.ShapeshiftForm = WS_Spells.ShapeshiftForm.FORM_CAT Or Me.ShapeshiftForm = WS_Spells.ShapeshiftForm.FORM_BEAR Or Me.ShapeshiftForm = WS_Spells.ShapeshiftForm.FORM_DIREBEAR Or Me.ShapeshiftForm = WS_Spells.ShapeshiftForm.FORM_MOONKIN Then
                            Return 0
                        Else
                            Return (Agility.Base - 10)
                        End If
                End Select
            End Get
        End Property
        Public ReadOnly Property AttackTime(ByVal index As Byte) As Short
            Get
                Return Fix(AttackTimeBase(index) * AttackTimeMods(index))
            End Get
        End Property
        Public AttackTimeBase() As Short = {2000, 2000, 2000}
        Public AttackTimeMods() As Single = {1.0, 1.0, 1.0}

        'Item Bonuses
        Public ManaRegenerationModifier As Single = Config.ManaRegenerationRate
        Public LifeRegenerationModifier As Single = Config.HealthRegenerationRate
        Public ManaRegenBonus As Integer = 0
        Public ManaRegenPercent As Single = 1
        Public ManaRegen As Integer = 0
        Public ManaRegenInterrupt As Integer = 0
        Public LifeRegenBonus As Integer = 0
        Public RageRegenBonus As Integer = 0

        Public Function GetStat(ByVal Type As Byte) As Short
            Select Case Type
                Case 0
                    Return Strength.Base
                Case 1
                    Return Agility.Base
                Case 2
                    Return Stamina.Base
                Case 3
                    Return Intellect.Base
                Case 4
                    Return Spirit.Base
                Case Else
                    Return 0
            End Select
        End Function
        Public Function GetOCTRegenMP() As Single
            Dim tmpLevel As Integer = Level
            If tmpLevel > GT_MAX_LEVEL Then tmpLevel = GT_MAX_LEVEL
            Try
                Dim RegenMPPerSpirit As Single = gtRegenMPPerSpt.Item((CInt(Classe) - 1) * GT_MAX_LEVEL + (tmpLevel - 1))
                Return RegenMPPerSpirit * Spirit.Base
            Catch
                Return 0
            End Try
        End Function

        Public Sub UpdateManaRegen()
            If FullyLoggedIn = False Then Exit Sub
            Dim PowerRegen As Single = Math.Sqrt(Intellect.Base) * GetOCTRegenMP()
            PowerRegen *= ManaRegenPercent
            Dim PowerRegenMP5 As Single = (ManaRegenBonus / 5)
            Dim PowerRegenInterrupt As Integer = 0

            For i As Integer = 0 To MAX_AURA_EFFECTs - 1
                If Not ActiveSpells(i) Is Nothing Then
                    If Not ActiveSpells(i).Aura1_Info Is Nothing Then
                        If ActiveSpells(i).Aura1_Info.ApplyAuraIndex = AuraEffects_Names.SPELL_AURA_MOD_MANA_REGEN_FROM_STAT Then
                            PowerRegenMP5 += GetStat(ActiveSpells(i).Aura1_Info.MiscValue) * ActiveSpells(i).Aura1_Info.GetValue(Level) / 500.0F
                        ElseIf ActiveSpells(i).SpellID = 34074 AndAlso ActiveSpells(i).Aura1_Info.ApplyAuraIndex = AuraEffects_Names.SPELL_AURA_PERIODIC_DUMMY Then
                            PowerRegenMP5 += (ActiveSpells(i).Aura1_Info.GetValue(Level) * Intellect.Base / 500.0F) + (CInt(Level) * 35 / 100)
                        ElseIf ActiveSpells(i).Aura1_Info.ApplyAuraIndex = AuraEffects_Names.SPELL_AURA_MOD_MANA_REGEN_INTERRUPT Then
                            PowerRegenInterrupt += ActiveSpells(i).Aura1_Info.GetValue(Level)
                        End If
                    End If
                    If Not ActiveSpells(i).Aura2_Info Is Nothing Then
                        If ActiveSpells(i).Aura2_Info.ApplyAuraIndex = AuraEffects_Names.SPELL_AURA_MOD_MANA_REGEN_FROM_STAT Then
                            PowerRegenMP5 += GetStat(ActiveSpells(i).Aura2_Info.MiscValue) * ActiveSpells(i).Aura2_Info.GetValue(Level) / 500.0F
                        ElseIf ActiveSpells(i).SpellID = 34074 AndAlso ActiveSpells(i).Aura1_Info.ApplyAuraIndex = AuraEffects_Names.SPELL_AURA_PERIODIC_DUMMY Then
                            PowerRegenMP5 += (ActiveSpells(i).Aura1_Info.GetValue(Level) * Intellect.Base / 500.0F) + (CInt(Level) * 35 / 100)
                        ElseIf ActiveSpells(i).Aura2_Info.ApplyAuraIndex = AuraEffects_Names.SPELL_AURA_MOD_MANA_REGEN_INTERRUPT Then
                            PowerRegenInterrupt += ActiveSpells(i).Aura2_Info.GetValue(Level)
                        End If
                    End If
                    If Not ActiveSpells(i).Aura3_Info Is Nothing Then
                        If ActiveSpells(i).Aura3_Info.ApplyAuraIndex = AuraEffects_Names.SPELL_AURA_MOD_MANA_REGEN_FROM_STAT Then
                            PowerRegenMP5 += GetStat(ActiveSpells(i).Aura3_Info.MiscValue) * ActiveSpells(i).Aura3_Info.GetValue(Level) / 500.0F
                        ElseIf ActiveSpells(i).SpellID = 34074 AndAlso ActiveSpells(i).Aura1_Info.ApplyAuraIndex = AuraEffects_Names.SPELL_AURA_PERIODIC_DUMMY Then
                            PowerRegenMP5 += (ActiveSpells(i).Aura1_Info.GetValue(Level) * Intellect.Base / 500.0F) + (CInt(Level) * 35 / 100)
                        ElseIf ActiveSpells(i).Aura3_Info.ApplyAuraIndex = AuraEffects_Names.SPELL_AURA_MOD_MANA_REGEN_INTERRUPT Then
                            PowerRegenInterrupt += ActiveSpells(i).Aura3_Info.GetValue(Level)
                        End If
                    End If
                End If
            Next

            If PowerRegenInterrupt > 100 Then PowerRegenInterrupt = 100
            PowerRegenInterrupt = (PowerRegenMP5 + PowerRegen * PowerRegenInterrupt / 100.0F)
            PowerRegen = CInt(PowerRegenMP5 + PowerRegen)
            ManaRegen = PowerRegen
            ManaRegenInterrupt = PowerRegenInterrupt

            SetUpdateFlag(EPlayerFields.PLAYER_FIELD_MOD_MANA_REGEN_INTERRUPT, CSng(ManaRegenInterrupt))
            SetUpdateFlag(EPlayerFields.PLAYER_FIELD_MOD_MANA_REGEN, CSng(ManaRegen))
            SendCharacterUpdate(False)
        End Sub

        'Temporaly variables
        Public Spell_Language As LANGUAGES = -1
        Public Spell_PET As PetObject = Nothing

        'Honor And Arena
        Public HonorCurrency As Integer = 0
        Public ArenaCurrency As Integer = 0
        Public HonorTitle As PlayerHonorTitle = PlayerHonorTitle.RANK_NONE
        Public HonorTitles As PlayerHonorTitles = PlayerHonorTitles.RANK_NONE
        Public HonorKillsToday As Short = 0
        Public HonorKillsYesterday As Short = 0
        Public HonorKillsLifeTime As Integer = 0
        Public HonorPointsToday As Short = 0
        Public HonorPointsYesterday As Short = 0
        Public Sub HonorLearnNewRank(ByVal RankTitle As PlayerHonorTitle)
            HonorTitle = RankTitle
            If Not HaveFlags(HonorTitles, CType(1, Integer) << RankTitle) Then
                HonorTitles = HonorTitles + (CType(1, Integer) << RankTitle)
            End If

            SetUpdateFlag(EPlayerFields.PLAYER_CHOSEN_TITLE, HonorTitle)
            SetUpdateFlag(EPlayerFields.PLAYER_FIELD_KNOWN_TITLES, HonorTitles)
            Me.SendCharacterUpdate(True)
        End Sub
        Public Sub HonorSaveAsNew()
            Database.Update("INSERT INTO characters_honor (char_guid)  VALUES (" & GUID & ");")
        End Sub
        Public Sub HonorSave()
            Dim tmp As String = "UPDATE characters_honor SET"

            tmp = tmp & " arena_currency=""" & ArenaCurrency & """"
            tmp = tmp & " honor_currency=""" & HonorCurrency & """"
            tmp = tmp & ", honor_title=" & HonorTitle
            tmp = tmp & ", honor_knownTitles=" & HonorTitles
            tmp = tmp & ", honor_killsToday=" & HonorKillsToday
            tmp = tmp & ", honor_killsYesterday=" & HonorKillsYesterday
            tmp = tmp & ", honor_pointsToday=" & HonorPointsToday
            tmp = tmp & ", honor_pointsYesterday=" & HonorPointsYesterday
            tmp = tmp & ", honor_kills=" & HonorKillsLifeTime

            tmp = tmp + String.Format(" WHERE char_guid = ""{0}"";", GUID)
            Database.Update(tmp)
        End Sub
        Public Sub HonorLoad()
            Dim MySQLQuery As New DataTable
            Database.Query(String.Format("SELECT * FROM characters_honor WHERE char_guid = {0};", GUID), MySQLQuery)
            If MySQLQuery.Rows.Count = 0 Then
                Log.WriteLine(LogType.FAILED, "Unable to get SQLDataBase honor info for character [GUID={0:X}]", GUID)
                Exit Sub
            End If

            ArenaCurrency = MySQLQuery.Rows(0).Item("arena_currency")
            HonorCurrency = MySQLQuery.Rows(0).Item("honor_currency")
            HonorTitle = MySQLQuery.Rows(0).Item("honor_title")
            HonorTitles = MySQLQuery.Rows(0).Item("honor_knownTitles")
            HonorKillsToday = MySQLQuery.Rows(0).Item("honor_killsToday")
            HonorKillsYesterday = MySQLQuery.Rows(0).Item("honor_killsYesterday")
            HonorPointsToday = MySQLQuery.Rows(0).Item("honor_pointsToday")
            HonorPointsYesterday = MySQLQuery.Rows(0).Item("honor_pointsYesterday")
            HonorKillsLifeTime = MySQLQuery.Rows(0).Item("honor_kills")

            MySQLQuery.Dispose()
        End Sub
        Public Sub HonorLog(ByVal honorPoints As Integer, ByVal victimGUID As ULong, ByVal victimRank As Long)
            'GUID = 0 : You have been awarded %h honor points.
            'GUID <>0 : %p dies, honorable kill Rank: %r (Estimated Honor Points: %h)

            Dim packet As New PacketClass(OPCODES.SMSG_PVP_CREDIT)
            packet.AddInt32(honorPoints)
            packet.AddUInt64(victimGUID)
            packet.AddInt32(victimRank)
            Client.Send(packet)
            packet.Dispose()
        End Sub


        Public Copper As UInteger = 0
        Public Name As String = ""
        Public Race As Races = 0
        Public Classe As Classes = 0
        Public Gender As Byte = 0
        Public Skin As Byte = 0
        Public Face As Byte = 0
        Public HairStyle As Byte = 0
        Public HairColor As Byte = 0
        Public FacialHair As Byte = 0
        Public OutfitId As Byte = 0

        Public ActionButtons As New Dictionary(Of Byte, TActionButton)
        Public TaxiZones As BitArray = New BitArray(8 * 32, False)
        Public TaxiNodes As New Queue(Of Integer)
        Public ZonesExplored() As UInteger = {0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0}

        Public WalkSpeed As Single = UNIT_NORMAL_WALK_SPEED
        Public RunSpeed As Single = UNIT_NORMAL_RUN_SPEED
        Public RunBackSpeed As Single = UNIT_NORMAL_WALK_BACK_SPEED
        Public SwimSpeed As Single = UNIT_NORMAL_SWIM_SPEED
        Public SwimBackSpeed As Single = UNIT_NORMAL_SWIM_BACK_SPEED
        Public FlySpeed As Single = UNIT_NORMAL_FLY_SPEED
        Public FlyBackSpeed As Single = UNIT_NORMAL_FLY_BACK_SPEED
        Public TurnRate As Single = UNIT_NORMAL_TURN_RATE

        Public movementFlags As Integer = 0
        Public ZoneID As Integer = 0
        Public bindpoint_positionX As Single = 0
        Public bindpoint_positionY As Single = 0
        Public bindpoint_positionZ As Single = 0
        Public bindpoint_map_id As Integer = 0
        Public bindpoint_zone_id As Integer = 0
        Public DEAD As Boolean = False
        Public ReadOnly Property ClassMask() As UInteger
            Get
                Return (1 << Classe - 1)
            End Get
        End Property
        Public ReadOnly Property RaceMask() As UInteger
            Get
                Return (1 << Race - 1)
            End Get
        End Property

        Public Overrides ReadOnly Property isDead() As Boolean
            Get
                Return DEAD
            End Get
        End Property
        Public ReadOnly Property isMoving() As Boolean
            Get
                Return (movementFlagsMask And movementFlags)
            End Get
        End Property
        Public ReadOnly Property isTurning() As Boolean
            Get
                Return (TurningFlagsMask And movementFlags)
            End Get
        End Property
        Public ReadOnly Property isMovingOrTurning() As Boolean
            Get
                Return (movementOrTurningFlagsMask And movementFlags)
            End Get
        End Property
        Public Property isPvP() As Boolean
            Get
                Return (cUnitFlags And UnitFlags.UNIT_FLAG_PVP)
            End Get
            Set(ByVal Enabled As Boolean)
                If Enabled Then
                    cUnitFlags = cUnitFlags Or UnitFlags.UNIT_FLAG_PVP
                Else
                    cUnitFlags = cUnitFlags And (Not UnitFlags.UNIT_FLAG_PVP)
                End If
            End Set
        End Property

        Public exploreCheckQueued_ As Boolean = False
        Public outsideMapID_ As Boolean = False
        Public antiHackSpeedChanged_ As Integer = 0

        Public underWaterTimer As TDrowningTimer = Nothing
        Public underWaterBreathing As Boolean = False
        Public lootGUID As ULong = 0
        Public repopTimer As TRepopTimer = Nothing
        Public tradeInfo As TTradeInfo = Nothing
        Public corpseGUID As ULong = 0
        Public corpseMapID As Integer = 0
        Public corpsePositionX As Single = 0
        Public corpsePositionY As Single = 0
        Public corpsePositionZ As Single = 0

        Public guidsForRemoving_Lock As New ReaderWriterLock
        Public guidsForRemoving As New List(Of ULong)
        Public creaturesNear As New List(Of ULong)
        Public playersNear As New List(Of ULong)
        Public gameObjectsNear As New List(Of ULong)
        Public dynamicObjectsNear As New List(Of ULong)
        Public corpseObjectsNear As New List(Of ULong)
        Public inCombatWith As New List(Of ULong)

        Public ReadOnly Property IsInCombat() As Boolean
            Get
                Return (inCombatWith.Count > 0)
            End Get
        End Property

        'NOTE: This function removes combat if there's no one else in your combat array
        Public Sub CheckCombat()
            If (cUnitFlags And UnitFlags.UNIT_FLAG_IN_COMBAT) Then
                If inCombatWith.Count > 0 Then Exit Sub

                SetPlayerOutOfCombat(Me)
            Else
                If inCombatWith.Count = 0 Then Exit Sub

                SetPlayerInCombat(Me)
            End If
        End Sub

        Public Overrides Function CanSee(ByRef c As BaseObject) As Boolean
            If GUID = c.GUID Then Return False
            If instance <> c.instance Then Return False
            If c.MapID <> MapID Then Return False

            If TypeOf c Is CreatureObject Then
                If Not CType(c, CreatureObject).aiScript Is Nothing Then
                    If CType(c, CreatureObject).aiScript.State = TBaseAI.AIState.AI_RESPAWN Then Return False
                End If
            ElseIf TypeOf c Is GameObjectObject Then
                If CType(c, GameObjectObject).Despawned Then Return False
            End If

            'DONE: See party members
            If (Group IsNot Nothing) AndAlso (TypeOf c Is CharacterObject) Then
                If (CType(c, CharacterObject).Group Is Group) Then
                    If (Math.Sqrt((c.positionX - positionX) ^ 2 + (c.positionY - positionY) ^ 2) > DEFAULT_DISTANCE_VISIBLE) Then Return False Else Return True
                End If
            End If

            'DONE: Check dead
            If DEAD Then
                'TODO: See only around the corpse
                'DONE: See only dead
                If corpseGUID = c.GUID Then Return True
                If c.Invisibility <> InvisibilityLevel.DEAD Then Return False
            Else
                'DONE: GM and DEAD invisibility
                If c.Invisibility > CanSeeInvisibility Then Return False
                'DONE: Stealth Detection
                If c.Invisibility = InvisibilityLevel.STEALTH AndAlso (Math.Sqrt((c.positionX - positionX) ^ 2 + (c.positionY - positionY) ^ 2) < DEFAULT_DISTANCE_DETECTION) Then Return True
                'DONE: Check invisibility
                If c.Invisibility = InvisibilityLevel.INIVISIBILITY AndAlso c.Invisibility_Value > CanSeeInvisibility_Invisibility Then Return False
                If c.Invisibility = InvisibilityLevel.STEALTH AndAlso c.Invisibility_Value > CanSeeInvisibility_Stealth Then Return False
            End If

            'DONE: Check distance
            If Math.Sqrt((c.positionX - positionX) ^ 2 + (c.positionY - positionY) ^ 2) > DEFAULT_DISTANCE_VISIBLE Then Return False
            Return True
        End Function

        Public TutorialFlags() As Byte = {0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0}

        'Updating
        Private UpdateMask As New BitArray(FIELD_MASK_SIZE_PLAYER, False)
        Private UpdateData As New Hashtable
        Public Sub SetUpdateFlag(ByVal pos As Integer, ByVal value As Integer)
            UpdateMask.Set(pos, True)
            UpdateData(pos) = (CType(value, Integer))
        End Sub
        Public Sub SetUpdateFlag(ByVal pos As Integer, ByVal value As UInteger)
            UpdateMask.Set(pos, True)
            UpdateData(pos) = (CType(value, UInteger))
        End Sub
        Public Sub SetUpdateFlag(ByVal pos As Integer, ByVal value As Long)
            UpdateMask.Set(pos, True)
            UpdateMask.Set(pos + 1, True)
            UpdateData(pos) = (CType((value And UInteger.MaxValue), Integer))
            UpdateData(pos + 1) = (CType(((value >> 32) And UInteger.MaxValue), Integer))
        End Sub
        Public Sub SetUpdateFlag(ByVal pos As Integer, ByVal value As ULong)
            UpdateMask.Set(pos, True)
            UpdateMask.Set(pos + 1, True)
            UpdateData(pos) = (CType((value And UInteger.MaxValue), UInteger))
            UpdateData(pos + 1) = (CType(((value >> 32) And UInteger.MaxValue), UInteger))
        End Sub
        Public Sub SetUpdateFlag(ByVal pos As Integer, ByVal value As Single)
            UpdateMask.Set(pos, True)
            UpdateData(pos) = (CType(value, Single))
        End Sub

        Public Sub SendOutOfRangeUpdate()
            Dim GUIDs() As ULong

            guidsForRemoving_Lock.AcquireWriterLock(DEFAULT_LOCK_TIMEOUT)
            GUIDs = guidsForRemoving.ToArray()
            guidsForRemoving.Clear()
            guidsForRemoving_Lock.ReleaseWriterLock()

            If GUIDs.Length > 0 Then
                Dim packet As New PacketClass(OPCODES.SMSG_UPDATE_OBJECT)
                packet.AddInt32(1)      'Operations.Count
                packet.AddInt8(0)
                packet.AddInt8(ObjectUpdateType.UPDATETYPE_OUT_OF_RANGE_OBJECTS)
                packet.AddInt32(GUIDs.Length)

                For Each g As ULong In GUIDs
                    packet.AddInt8(&HFF)      'VERSION 1.9.X+ FIX
                    packet.AddUInt64(g)
                Next

                Client.Send(packet)
                packet.Dispose()
            End If
        End Sub
        Public Sub SendUpdate()
            Dim packet As New PacketClass(OPCODES.SMSG_UPDATE_OBJECT)
            packet.AddInt32(1 + Items.Count)    'Operations.Count
            'packet.AddInt32(1)    'Operations.Count
            packet.AddInt8(0)

            Me.PrepareUpdate(packet, ObjectUpdateType.UPDATETYPE_CREATE_OBJECT_SELF)

            For Each tmpItem As KeyValuePair(Of Byte, ItemObject) In Items
                Dim tmpUpdate As New UpdateClass(FIELD_MASK_SIZE_ITEM)
                tmpItem.Value.FillAllUpdateFlags(tmpUpdate)
                tmpUpdate.AddToPacket(packet, ObjectUpdateType.UPDATETYPE_CREATE_OBJECT, CType(tmpItem.Value, ItemObject), 0)
                tmpUpdate.Dispose()

                'DONE: Update Items In bag
                If tmpItem.Value.ItemInfo.IsContainer Then
                    tmpItem.Value.SendContainedItemsUpdate(Client, ObjectUpdateType.UPDATETYPE_CREATE_OBJECT)
                End If
            Next

            'DumpPacket(packet.Data, Client)

            Client.Send(packet)
            packet.Dispose()
        End Sub                                               'Used only for first updating (creating)
        Public Sub SendItemUpdate(ByVal Item As ItemObject)
            Dim packet As New PacketClass(OPCODES.SMSG_UPDATE_OBJECT)
            packet.AddInt32(1)      'Operations.Count
            packet.AddInt8(0)

            Dim tmpUpdate As New UpdateClass(FIELD_MASK_SIZE_ITEM)
            Item.FillAllUpdateFlags(tmpUpdate)
            tmpUpdate.AddToPacket(packet, ObjectUpdateType.UPDATETYPE_VALUES, Item, 0)

            Client.Send(packet)
            packet.Dispose()
            tmpUpdate.Dispose()
        End Sub
        Public Sub SendInventoryUpdate()
            Dim packet As New PacketClass(OPCODES.SMSG_UPDATE_OBJECT)
            packet.AddInt32(1)      'Operations.Count
            packet.AddInt8(0)

            Dim i As Byte
            For i = 0 To INVENTORY_SLOT_ITEM_END - 1
                If Items.ContainsKey(i) Then
                    SetUpdateFlag(EPlayerFields.PLAYER_FIELD_INV_SLOT_HEAD + i * 2, Items(i).GUID)
                    If i < EQUIPMENT_SLOT_END Then
                        SetUpdateFlag(EPlayerFields.PLAYER_VISIBLE_ITEM_1_0 + (i * PLAYER_VISIBLE_ITEM_SIZE), Items(i).ItemEntry)

                        'SetUpdateFlag(EPlayerFields.PLAYER_VISIBLE_ITEM_1_1 + (i * PLAYER_VISIBLE_ITEM_SIZE), 0)           'ITEM_FIELD_ENCHANTMENT
                        'SetUpdateFlag(EPlayerFields.PLAYER_VISIBLE_ITEM_1_2 + (i * PLAYER_VISIBLE_ITEM_SIZE), 0)           'ITEM_FIELD_ENCHANTMENT + 3
                        'SetUpdateFlag(EPlayerFields.PLAYER_VISIBLE_ITEM_1_3 + (i * PLAYER_VISIBLE_ITEM_SIZE), 0)           'ITEM_FIELD_ENCHANTMENT + 6
                        'SetUpdateFlag(EPlayerFields.PLAYER_VISIBLE_ITEM_1_4 + (i * PLAYER_VISIBLE_ITEM_SIZE), 0)           'ITEM_FIELD_ENCHANTMENT + 9
                        'SetUpdateFlag(EPlayerFields.PLAYER_VISIBLE_ITEM_1_5 + (i * PLAYER_VISIBLE_ITEM_SIZE), 0)           'ITEM_FIELD_ENCHANTMENT + 12
                        'SetUpdateFlag(EPlayerFields.PLAYER_VISIBLE_ITEM_1_6 + (i * PLAYER_VISIBLE_ITEM_SIZE), 0)           'ITEM_FIELD_ENCHANTMENT + 15
                        'SetUpdateFlag(EPlayerFields.PLAYER_VISIBLE_ITEM_1_7 + (i * PLAYER_VISIBLE_ITEM_SIZE), 0)           'ITEM_FIELD_ENCHANTMENT + 18
                        SetUpdateFlag(EPlayerFields.PLAYER_VISIBLE_ITEM_1_PROPERTIES + (i * PLAYER_VISIBLE_ITEM_SIZE), 0)   'ITEM_FIELD_RANDOM_PROPERTIES_ID
                    End If
                Else
                    SetUpdateFlag(EPlayerFields.PLAYER_FIELD_INV_SLOT_HEAD + i * 2, CType(0, Long))
                    If i < EQUIPMENT_SLOT_END Then
                        SetUpdateFlag(EPlayerFields.PLAYER_VISIBLE_ITEM_1_0 + i * PLAYER_VISIBLE_ITEM_SIZE, 0)
                    End If
                End If
            Next

            Me.PrepareUpdate(packet, ObjectUpdateType.UPDATETYPE_VALUES)

            Client.Send(packet)
            packet.Dispose()
        End Sub
        Public Sub SendItemAndCharacterUpdate(ByVal Item As ItemObject, Optional ByVal UPDATETYPE As Integer = ObjectUpdateType.UPDATETYPE_VALUES)
            Dim packet As New PacketClass(OPCODES.SMSG_UPDATE_OBJECT)
            packet.AddInt32(2)      'Operations.Count
            packet.AddInt8(0)


            Dim tmpUpdate As New UpdateClass(FIELD_MASK_SIZE_ITEM)
            Item.FillAllUpdateFlags(tmpUpdate)
            tmpUpdate.AddToPacket(packet, UPDATETYPE, CType(Item, ItemObject), 0)

            Dim i As Byte
            For i = EQUIPMENT_SLOT_START To KEYRING_SLOT_END - 1
                If Items.ContainsKey(i) Then
                    SetUpdateFlag(EPlayerFields.PLAYER_FIELD_INV_SLOT_HEAD + i * 2, Items(i).GUID)
                    If i < EQUIPMENT_SLOT_END Then
                        'SetUpdateFlag(EPlayerFields.PLAYER_VISIBLE_ITEM_1_0 + i * PLAYER_VISIBLE_ITEM_SIZE, ITEMDatabase(WORLD_ITEMs(Items(i)).ItemEntry).Model)
                        SetUpdateFlag(EPlayerFields.PLAYER_VISIBLE_ITEM_1_0 + (i * PLAYER_VISIBLE_ITEM_SIZE), Items(i).ItemEntry)

                        'SetUpdateFlag(EPlayerFields.PLAYER_VISIBLE_ITEM_1_1 + (i * PLAYER_VISIBLE_ITEM_SIZE), 0)           'ITEM_FIELD_ENCHANTMENT
                        'SetUpdateFlag(EPlayerFields.PLAYER_VISIBLE_ITEM_1_2 + (i * PLAYER_VISIBLE_ITEM_SIZE), 0)           'ITEM_FIELD_ENCHANTMENT + 3
                        'SetUpdateFlag(EPlayerFields.PLAYER_VISIBLE_ITEM_1_3 + (i * PLAYER_VISIBLE_ITEM_SIZE), 0)           'ITEM_FIELD_ENCHANTMENT + 6
                        'SetUpdateFlag(EPlayerFields.PLAYER_VISIBLE_ITEM_1_4 + (i * PLAYER_VISIBLE_ITEM_SIZE), 0)           'ITEM_FIELD_ENCHANTMENT + 9
                        'SetUpdateFlag(EPlayerFields.PLAYER_VISIBLE_ITEM_1_5 + (i * PLAYER_VISIBLE_ITEM_SIZE), 0)           'ITEM_FIELD_ENCHANTMENT + 12
                        'SetUpdateFlag(EPlayerFields.PLAYER_VISIBLE_ITEM_1_6 + (i * PLAYER_VISIBLE_ITEM_SIZE), 0)           'ITEM_FIELD_ENCHANTMENT + 15
                        'SetUpdateFlag(EPlayerFields.PLAYER_VISIBLE_ITEM_1_7 + (i * PLAYER_VISIBLE_ITEM_SIZE), 0)           'ITEM_FIELD_ENCHANTMENT + 18
                        SetUpdateFlag(EPlayerFields.PLAYER_VISIBLE_ITEM_1_PROPERTIES + (i * PLAYER_VISIBLE_ITEM_SIZE), 0)   'ITEM_FIELD_RANDOM_PROPERTIES_ID
                    End If
                Else
                    SetUpdateFlag(EPlayerFields.PLAYER_FIELD_INV_SLOT_HEAD + i * 2, CType(0, Long))
                    If i < EQUIPMENT_SLOT_END Then
                        SetUpdateFlag(EPlayerFields.PLAYER_VISIBLE_ITEM_1_0 + i * PLAYER_VISIBLE_ITEM_SIZE, 0)
                    End If
                End If
            Next

            Me.PrepareUpdate(packet, ObjectUpdateType.UPDATETYPE_VALUES)

            Client.Send(packet)
            packet.Dispose()
            tmpUpdate.Dispose()
        End Sub
        Public Sub SendCharacterUpdate(Optional ByVal toNear As Boolean = True)
            If UpdateData.Count = 0 Then Exit Sub

            'DONE: Send to near
            If toNear AndAlso SeenBy.Count > 0 Then
                Dim forOthers As New UpdateClass
                forOthers.UpdateData = UpdateData.Clone
                forOthers.UpdateMask = UpdateMask.Clone

                Dim packetForOthers As New PacketClass(OPCODES.SMSG_UPDATE_OBJECT)
                packetForOthers.AddInt32(1)       'Operations.Count
                packetForOthers.AddInt8(0)
                forOthers.AddToPacket(packetForOthers, ObjectUpdateType.UPDATETYPE_VALUES, Me, 0)
                SendToNearPlayers(packetForOthers)
                packetForOthers.Dispose()
            End If

            'DONE: Send to me
            Dim packet As New PacketClass(OPCODES.SMSG_UPDATE_OBJECT)
            packet.AddInt32(1)       'Operations.Count
            packet.AddInt8(0)
            Me.PrepareUpdate(packet, ObjectUpdateType.UPDATETYPE_VALUES)
            Client.Send(packet)
            packet.Dispose()
        End Sub                                      'Sends update for character to him and near players
        Public Sub FillStatsUpdateFlags()
            SetUpdateFlag(EUnitFields.UNIT_FIELD_MAXHEALTH, CType(Life.Maximum, Integer))
            SetUpdateFlag(EUnitFields.UNIT_FIELD_MAXPOWER1, CType(Mana.Maximum, Integer))
            SetUpdateFlag(EUnitFields.UNIT_FIELD_MAXPOWER2, CType(Rage.Maximum, Integer))
            SetUpdateFlag(EUnitFields.UNIT_FIELD_MAXPOWER4, CType(Energy.Maximum, Integer))

            SetUpdateFlag(EPlayerFields.PLAYER_BLOCK_PERCENTAGE, combatBlock)
            SetUpdateFlag(EUnitFields.UNIT_FIELD_MINDAMAGE, Damage.Minimum)
            SetUpdateFlag(EUnitFields.UNIT_FIELD_MAXDAMAGE, CType(Damage.Maximum + BaseUnarmedDamage, Single))
            SetUpdateFlag(EUnitFields.UNIT_FIELD_MINOFFHANDDAMAGE, OffHandDamage.Minimum)
            SetUpdateFlag(EUnitFields.UNIT_FIELD_MAXOFFHANDDAMAGE, OffHandDamage.Maximum)
            SetUpdateFlag(EUnitFields.UNIT_FIELD_MINRANGEDDAMAGE, RangedDamage.Minimum)
            SetUpdateFlag(EUnitFields.UNIT_FIELD_MAXRANGEDDAMAGE, CType(RangedDamage.Maximum + BaseRangedDamage, Single))

            SetUpdateFlag(EUnitFields.UNIT_FIELD_BASEATTACKTIME, AttackTime(0))
            SetUpdateFlag(EUnitFields.UNIT_FIELD_OFFHANDATTACKTIME, AttackTime(1))
            SetUpdateFlag(EUnitFields.UNIT_FIELD_RANGEDATTACKTIME, AttackTime(2))

            SetUpdateFlag(EPlayerFields.PLAYER_BLOCK_PERCENTAGE, GetBasePercentBlock(Me, 0))
            SetUpdateFlag(EPlayerFields.PLAYER_DODGE_PERCENTAGE, GetBasePercentDodge(Me, 0))
            SetUpdateFlag(EPlayerFields.PLAYER_PARRY_PERCENTAGE, GetBasePercentParry(Me, 0))
            SetUpdateFlag(EPlayerFields.PLAYER_CRIT_PERCENTAGE, GetBasePercentCrit(Me, 0))

            SetUpdateFlag(EPlayerFields.PLAYER_FIELD_COINAGE, Copper)
            SetUpdateFlag(EUnitFields.UNIT_FIELD_STAT0, CType(Strength.Base, Integer))
            SetUpdateFlag(EUnitFields.UNIT_FIELD_STAT1, CType(Agility.Base, Integer))
            SetUpdateFlag(EUnitFields.UNIT_FIELD_STAT2, CType(Stamina.Base, Integer))
            SetUpdateFlag(EUnitFields.UNIT_FIELD_STAT3, CType(Intellect.Base, Integer))
            SetUpdateFlag(EUnitFields.UNIT_FIELD_STAT4, CType(Spirit.Base, Integer))
            SetUpdateFlag(EUnitFields.UNIT_FIELD_POSSTAT0, CType(Strength.PositiveBonus, Integer))
            SetUpdateFlag(EUnitFields.UNIT_FIELD_POSSTAT1, CType(Agility.PositiveBonus, Integer))
            SetUpdateFlag(EUnitFields.UNIT_FIELD_POSSTAT2, CType(Stamina.PositiveBonus, Integer))
            SetUpdateFlag(EUnitFields.UNIT_FIELD_POSSTAT3, CType(Intellect.PositiveBonus, Integer))
            SetUpdateFlag(EUnitFields.UNIT_FIELD_POSSTAT4, CType(Spirit.PositiveBonus, Integer))
            SetUpdateFlag(EUnitFields.UNIT_FIELD_NEGSTAT0, CType(Strength.NegativeBonus, Integer))
            SetUpdateFlag(EUnitFields.UNIT_FIELD_NEGSTAT1, CType(Agility.NegativeBonus, Integer))
            SetUpdateFlag(EUnitFields.UNIT_FIELD_NEGSTAT2, CType(Stamina.NegativeBonus, Integer))
            SetUpdateFlag(EUnitFields.UNIT_FIELD_NEGSTAT3, CType(Intellect.NegativeBonus, Integer))
            SetUpdateFlag(EUnitFields.UNIT_FIELD_NEGSTAT4, CType(Spirit.NegativeBonus, Integer))

            SetUpdateFlag(EUnitFields.UNIT_FIELD_RESISTANCES + DamageTypes.DMG_PHYSICAL, Resistances(DamageTypes.DMG_PHYSICAL).RealBase)
            SetUpdateFlag(EUnitFields.UNIT_FIELD_RESISTANCES + DamageTypes.DMG_HOLY, Resistances(DamageTypes.DMG_HOLY).RealBase)
            SetUpdateFlag(EUnitFields.UNIT_FIELD_RESISTANCES + DamageTypes.DMG_FIRE, Resistances(DamageTypes.DMG_FIRE).RealBase)
            SetUpdateFlag(EUnitFields.UNIT_FIELD_RESISTANCES + DamageTypes.DMG_NATURE, Resistances(DamageTypes.DMG_NATURE).RealBase)
            SetUpdateFlag(EUnitFields.UNIT_FIELD_RESISTANCES + DamageTypes.DMG_FROST, Resistances(DamageTypes.DMG_FROST).RealBase)
            SetUpdateFlag(EUnitFields.UNIT_FIELD_RESISTANCES + DamageTypes.DMG_SHADOW, Resistances(DamageTypes.DMG_SHADOW).RealBase)
            SetUpdateFlag(EUnitFields.UNIT_FIELD_RESISTANCES + DamageTypes.DMG_ARCANE, Resistances(DamageTypes.DMG_ARCANE).RealBase)

            SetUpdateFlag(EUnitFields.UNIT_FIELD_RESISTANCEBUFFMODSPOSITIVE + DamageTypes.DMG_PHYSICAL, Resistances(DamageTypes.DMG_PHYSICAL).PositiveBonus)
            SetUpdateFlag(EUnitFields.UNIT_FIELD_RESISTANCEBUFFMODSPOSITIVE + DamageTypes.DMG_HOLY, Resistances(DamageTypes.DMG_HOLY).PositiveBonus)
            SetUpdateFlag(EUnitFields.UNIT_FIELD_RESISTANCEBUFFMODSPOSITIVE + DamageTypes.DMG_FIRE, Resistances(DamageTypes.DMG_FIRE).PositiveBonus)
            SetUpdateFlag(EUnitFields.UNIT_FIELD_RESISTANCEBUFFMODSPOSITIVE + DamageTypes.DMG_NATURE, Resistances(DamageTypes.DMG_NATURE).PositiveBonus)
            SetUpdateFlag(EUnitFields.UNIT_FIELD_RESISTANCEBUFFMODSPOSITIVE + DamageTypes.DMG_FROST, Resistances(DamageTypes.DMG_FROST).PositiveBonus)
            SetUpdateFlag(EUnitFields.UNIT_FIELD_RESISTANCEBUFFMODSPOSITIVE + DamageTypes.DMG_SHADOW, Resistances(DamageTypes.DMG_SHADOW).PositiveBonus)
            SetUpdateFlag(EUnitFields.UNIT_FIELD_RESISTANCEBUFFMODSPOSITIVE + DamageTypes.DMG_ARCANE, Resistances(DamageTypes.DMG_ARCANE).PositiveBonus)

            SetUpdateFlag(EUnitFields.UNIT_FIELD_RESISTANCEBUFFMODSNEGATIVE + DamageTypes.DMG_PHYSICAL, Resistances(DamageTypes.DMG_PHYSICAL).NegativeBonus)
            SetUpdateFlag(EUnitFields.UNIT_FIELD_RESISTANCEBUFFMODSNEGATIVE + DamageTypes.DMG_HOLY, Resistances(DamageTypes.DMG_HOLY).NegativeBonus)
            SetUpdateFlag(EUnitFields.UNIT_FIELD_RESISTANCEBUFFMODSNEGATIVE + DamageTypes.DMG_FIRE, Resistances(DamageTypes.DMG_FIRE).NegativeBonus)
            SetUpdateFlag(EUnitFields.UNIT_FIELD_RESISTANCEBUFFMODSNEGATIVE + DamageTypes.DMG_NATURE, Resistances(DamageTypes.DMG_NATURE).NegativeBonus)
            SetUpdateFlag(EUnitFields.UNIT_FIELD_RESISTANCEBUFFMODSNEGATIVE + DamageTypes.DMG_FROST, Resistances(DamageTypes.DMG_FROST).NegativeBonus)
            SetUpdateFlag(EUnitFields.UNIT_FIELD_RESISTANCEBUFFMODSNEGATIVE + DamageTypes.DMG_SHADOW, Resistances(DamageTypes.DMG_SHADOW).NegativeBonus)
            SetUpdateFlag(EUnitFields.UNIT_FIELD_RESISTANCEBUFFMODSNEGATIVE + DamageTypes.DMG_ARCANE, Resistances(DamageTypes.DMG_ARCANE).NegativeBonus)
        End Sub                                     'Used for this player's stats updates
        Public Sub FillAllUpdateFlags()
            Dim i As Byte

            SetUpdateFlag(EObjectFields.OBJECT_FIELD_GUID, GUID)
            SetUpdateFlag(EObjectFields.OBJECT_FIELD_TYPE, CType(25, Integer))
            SetUpdateFlag(EObjectFields.OBJECT_FIELD_SCALE_X, Size)

            SetUpdateFlag(EUnitFields.UNIT_FIELD_HEALTH, CType(Life.Current, Integer))
            SetUpdateFlag(EUnitFields.UNIT_FIELD_POWER1, CType(Mana.Current, Integer))
            SetUpdateFlag(EUnitFields.UNIT_FIELD_POWER2, CType(Rage.Current, Integer))
            SetUpdateFlag(EUnitFields.UNIT_FIELD_POWER4, CType(Energy.Current, Integer))
            SetUpdateFlag(EUnitFields.UNIT_FIELD_MAXHEALTH, CType(Life.Maximum, Integer))
            SetUpdateFlag(EUnitFields.UNIT_FIELD_MAXPOWER1, CType(Mana.Maximum, Integer))
            SetUpdateFlag(EUnitFields.UNIT_FIELD_MAXPOWER2, CType(Rage.Maximum, Integer))
            SetUpdateFlag(EUnitFields.UNIT_FIELD_MAXPOWER4, CType(Energy.Maximum, Integer))
            SetUpdateFlag(EUnitFields.UNIT_FIELD_BASE_HEALTH, CType(Life.Base, Integer))
            SetUpdateFlag(EUnitFields.UNIT_FIELD_BASE_MANA, CType(Mana.Base, Integer))
            SetUpdateFlag(EUnitFields.UNIT_FIELD_LEVEL, CType(Level, Integer))
            SetUpdateFlag(EUnitFields.UNIT_FIELD_FACTIONTEMPLATE, CType(Faction, Integer))
            'SetUpdateFlag(EUnitFields.UNIT_FIELD_FACTIONTEMPLATE, CType(4, Integer))

            SetUpdateFlag(EUnitFields.UNIT_FIELD_FLAGS, cUnitFlags)
            SetUpdateFlag(EUnitFields.UNIT_FIELD_FLAGS_2, 0)
            SetUpdateFlag(EPlayerFields.PLAYER_FIELD_MAX_LEVEL, CType(IIf(Client.Expansion = ExpansionLevel.EXPANSION_1, 70, 60), Integer))
            'SetUpdateFlag(EPlayerFields.PLAYER_FIELD_MAX_LEVEL, CType(70, Integer))
            SetUpdateFlag(EUnitFields.UNIT_FIELD_STAT0, CType(Strength.Base, Integer))
            SetUpdateFlag(EUnitFields.UNIT_FIELD_STAT1, CType(Agility.Base, Integer))
            SetUpdateFlag(EUnitFields.UNIT_FIELD_STAT2, CType(Stamina.Base, Integer))
            SetUpdateFlag(EUnitFields.UNIT_FIELD_STAT3, CType(Spirit.Base, Integer))
            SetUpdateFlag(EUnitFields.UNIT_FIELD_STAT4, CType(Intellect.Base, Integer))
            SetUpdateFlag(EUnitFields.UNIT_FIELD_POSSTAT0, CType(Strength.PositiveBonus, Integer))
            SetUpdateFlag(EUnitFields.UNIT_FIELD_POSSTAT1, CType(Agility.PositiveBonus, Integer))
            SetUpdateFlag(EUnitFields.UNIT_FIELD_POSSTAT2, CType(Stamina.PositiveBonus, Integer))
            SetUpdateFlag(EUnitFields.UNIT_FIELD_POSSTAT3, CType(Intellect.PositiveBonus, Integer))
            SetUpdateFlag(EUnitFields.UNIT_FIELD_POSSTAT4, CType(Spirit.PositiveBonus, Integer))
            SetUpdateFlag(EUnitFields.UNIT_FIELD_NEGSTAT0, CType(Strength.NegativeBonus, Integer))
            SetUpdateFlag(EUnitFields.UNIT_FIELD_NEGSTAT1, CType(Agility.NegativeBonus, Integer))
            SetUpdateFlag(EUnitFields.UNIT_FIELD_NEGSTAT2, CType(Stamina.NegativeBonus, Integer))
            SetUpdateFlag(EUnitFields.UNIT_FIELD_NEGSTAT3, CType(Intellect.NegativeBonus, Integer))
            SetUpdateFlag(EUnitFields.UNIT_FIELD_NEGSTAT4, CType(Spirit.NegativeBonus, Integer))
            SetUpdateFlag(EUnitFields.UNIT_FIELD_BYTES_0, CType(CType(Race, Integer) + (CType(Classe, Integer) << 8) + (CType(Gender, Integer) << 16) + (CType(ManaType, Integer) << 24), Integer))
            'SetUpdateFlag(EUnitFields.UNIT_FIELD_BYTES_2, &HEEEEEE00)
            SetUpdateFlag(EUnitFields.UNIT_FIELD_DISPLAYID, Model)
            SetUpdateFlag(EUnitFields.UNIT_FIELD_NATIVEDISPLAYID, Model_Native)
            SetUpdateFlag(EUnitFields.UNIT_FIELD_MOUNTDISPLAYID, Mount)
            SetUpdateFlag(EUnitFields.UNIT_DYNAMIC_FLAGS, cDynamicFlags)

            SetUpdateFlag(EPlayerFields.PLAYER_BYTES, (Skin + (CType(Face, Integer) << 8) + (CType(HairStyle, Integer) << 16) + (CType(HairColor, Integer) << 24)))

            'FacialHair,Unk1,BankSlotsAvailable,RestState;
            SetUpdateFlag(EPlayerFields.PLAYER_BYTES_2, (FacialHair + (&HEE << 8) + (CType(Items_AvailableBankSlots, Integer) << 16) + (CType(RestState, Integer) << 24)))

            ''Gender(for sound),Alchohol,Unk3,HonorRank?
            SetUpdateFlag(EPlayerFields.PLAYER_BYTES_3, (Gender + (0 << 8) + (0 << 16) + (0 << 24)))


            SetUpdateFlag(EPlayerFields.PLAYER_FIELD_WATCHED_FACTION_INDEX, WatchedFactionIndex)

            SetUpdateFlag(EPlayerFields.PLAYER_XP, XP)
            SetUpdateFlag(EPlayerFields.PLAYER_NEXT_LEVEL_XP, XPTable(Level))

            SetUpdateFlag(EPlayerFields.PLAYER_FLAGS, cPlayerFlags)
            ''ComboPoints, Unk4, Unk5, HonorRank?
            ''SetUpdateFlag(EPlayerFields.PLAYER_FIELD_BYTES, &HEEE00000)         '&HF0008
            'SetUpdateFlag(EPlayerFields.PLAYER_FIELD_BYTES, 8) ' ????
            SetUpdateFlag(EUnitFields.UNIT_FIELD_BOUNDINGRADIUS, CType(0.389, Single))
            SetUpdateFlag(EUnitFields.UNIT_FIELD_COMBATREACH, CType(1.5, Single))
            SetUpdateFlag(EPlayerFields.PLAYER_CHARACTER_POINTS1, CType(TalentPoints, Integer))
            'SetUpdateFlag(EPlayerFields.PLAYER_CHARACTER_POINTS2, 2) '???

            SetUpdateFlag(EPlayerFields.PLAYER_GUILDID, GuildID)
            SetUpdateFlag(EPlayerFields.PLAYER_GUILDRANK, GuildRank)

            SetUpdateFlag(EUnitFields.UNIT_FIELD_MINDAMAGE, Damage.Minimum)
            SetUpdateFlag(EUnitFields.UNIT_FIELD_MAXDAMAGE, CType(Damage.Maximum + BaseUnarmedDamage, Single))
            SetUpdateFlag(EUnitFields.UNIT_FIELD_BASEATTACKTIME, AttackTime(0))
            SetUpdateFlag(EUnitFields.UNIT_MOD_CAST_SPEED, 1.0F)
            SetUpdateFlag(EUnitFields.UNIT_FIELD_ATTACK_POWER, AttackPower)
            SetUpdateFlag(EUnitFields.UNIT_FIELD_RANGED_ATTACK_POWER, AttackPowerRanged)
            SetUpdateFlag(EPlayerFields.PLAYER_CRIT_PERCENTAGE, GetBasePercentCrit(Me, 0))
            SetUpdateFlag(EPlayerFields.PLAYER_RANGED_CRIT_PERCENTAGE, GetBasePercentCrit(Me, 0))
            SetUpdateFlag(EPlayerFields.PLAYER_FIELD_MOD_HEALING_DONE_POS, healing.PositiveBonus)

            For i = 0 To 6
                SetUpdateFlag(EPlayerFields.PLAYER_SPELL_CRIT_PERCENTAGE1 + i, CType(0, Single))
                SetUpdateFlag(EPlayerFields.PLAYER_FIELD_MOD_DAMAGE_DONE_POS + i, spellDamage(i).PositiveBonus)
                SetUpdateFlag(EPlayerFields.PLAYER_FIELD_MOD_DAMAGE_DONE_NEG + i, spellDamage(i).NegativeBonus)
                SetUpdateFlag(EPlayerFields.PLAYER_FIELD_MOD_DAMAGE_DONE_PCT + i, spellDamage(i).Modifier)
            Next

            SetUpdateFlag(EUnitFields.UNIT_FIELD_RESISTANCES + DamageTypes.DMG_PHYSICAL, Resistances(DamageTypes.DMG_PHYSICAL).Base)
            SetUpdateFlag(EUnitFields.UNIT_FIELD_RESISTANCES + DamageTypes.DMG_HOLY, Resistances(DamageTypes.DMG_HOLY).Base)
            SetUpdateFlag(EUnitFields.UNIT_FIELD_RESISTANCES + DamageTypes.DMG_FIRE, Resistances(DamageTypes.DMG_FIRE).Base)
            SetUpdateFlag(EUnitFields.UNIT_FIELD_RESISTANCES + DamageTypes.DMG_NATURE, Resistances(DamageTypes.DMG_NATURE).Base)
            SetUpdateFlag(EUnitFields.UNIT_FIELD_RESISTANCES + DamageTypes.DMG_FROST, Resistances(DamageTypes.DMG_FROST).Base)
            SetUpdateFlag(EUnitFields.UNIT_FIELD_RESISTANCES + DamageTypes.DMG_SHADOW, Resistances(DamageTypes.DMG_SHADOW).Base)
            SetUpdateFlag(EUnitFields.UNIT_FIELD_RESISTANCES + DamageTypes.DMG_ARCANE, Resistances(DamageTypes.DMG_ARCANE).Base)

            SetUpdateFlag(EUnitFields.UNIT_FIELD_RESISTANCEBUFFMODSPOSITIVE + DamageTypes.DMG_PHYSICAL, Resistances(DamageTypes.DMG_PHYSICAL).PositiveBonus)
            SetUpdateFlag(EUnitFields.UNIT_FIELD_RESISTANCEBUFFMODSPOSITIVE + DamageTypes.DMG_HOLY, Resistances(DamageTypes.DMG_HOLY).PositiveBonus)
            SetUpdateFlag(EUnitFields.UNIT_FIELD_RESISTANCEBUFFMODSPOSITIVE + DamageTypes.DMG_FIRE, Resistances(DamageTypes.DMG_FIRE).PositiveBonus)
            SetUpdateFlag(EUnitFields.UNIT_FIELD_RESISTANCEBUFFMODSPOSITIVE + DamageTypes.DMG_NATURE, Resistances(DamageTypes.DMG_NATURE).PositiveBonus)
            SetUpdateFlag(EUnitFields.UNIT_FIELD_RESISTANCEBUFFMODSPOSITIVE + DamageTypes.DMG_FROST, Resistances(DamageTypes.DMG_FROST).PositiveBonus)
            SetUpdateFlag(EUnitFields.UNIT_FIELD_RESISTANCEBUFFMODSPOSITIVE + DamageTypes.DMG_SHADOW, Resistances(DamageTypes.DMG_SHADOW).PositiveBonus)
            SetUpdateFlag(EUnitFields.UNIT_FIELD_RESISTANCEBUFFMODSPOSITIVE + DamageTypes.DMG_ARCANE, Resistances(DamageTypes.DMG_ARCANE).PositiveBonus)

            SetUpdateFlag(EUnitFields.UNIT_FIELD_RESISTANCEBUFFMODSNEGATIVE + DamageTypes.DMG_PHYSICAL, Resistances(DamageTypes.DMG_PHYSICAL).NegativeBonus)
            SetUpdateFlag(EUnitFields.UNIT_FIELD_RESISTANCEBUFFMODSNEGATIVE + DamageTypes.DMG_HOLY, Resistances(DamageTypes.DMG_HOLY).NegativeBonus)
            SetUpdateFlag(EUnitFields.UNIT_FIELD_RESISTANCEBUFFMODSNEGATIVE + DamageTypes.DMG_FIRE, Resistances(DamageTypes.DMG_FIRE).NegativeBonus)
            SetUpdateFlag(EUnitFields.UNIT_FIELD_RESISTANCEBUFFMODSNEGATIVE + DamageTypes.DMG_NATURE, Resistances(DamageTypes.DMG_NATURE).NegativeBonus)
            SetUpdateFlag(EUnitFields.UNIT_FIELD_RESISTANCEBUFFMODSNEGATIVE + DamageTypes.DMG_FROST, Resistances(DamageTypes.DMG_FROST).NegativeBonus)
            SetUpdateFlag(EUnitFields.UNIT_FIELD_RESISTANCEBUFFMODSNEGATIVE + DamageTypes.DMG_SHADOW, Resistances(DamageTypes.DMG_SHADOW).NegativeBonus)
            SetUpdateFlag(EUnitFields.UNIT_FIELD_RESISTANCEBUFFMODSNEGATIVE + DamageTypes.DMG_ARCANE, Resistances(DamageTypes.DMG_ARCANE).NegativeBonus)

            SetUpdateFlag(EPlayerFields.PLAYER_FIELD_MOD_MANA_REGEN_INTERRUPT, CSng(ManaRegenInterrupt))
            SetUpdateFlag(EPlayerFields.PLAYER_FIELD_MOD_MANA_REGEN, CSng(ManaRegen))

            SetUpdateFlag(EPlayerFields.PLAYER_FIELD_COINAGE, Copper)

            For Each Skill As KeyValuePair(Of Integer, TSkill) In Skills
                SetUpdateFlag(EPlayerFields.PLAYER_SKILL_INFO_1_1 + SkillsPositions(Skill.Key) * 3, Skill.Key)                                    'skill1.Id
                SetUpdateFlag(EPlayerFields.PLAYER_SKILL_INFO_1_1 + SkillsPositions(Skill.Key) * 3 + 1, CType(Skill.Value, TSkill).GetSkill)      'CType((skill1.CurrentVal(Me) + (skill1.Cap(Me) << 16)), Integer)
                SetUpdateFlag(EPlayerFields.PLAYER_SKILL_INFO_1_1 + SkillsPositions(Skill.Key) * 3 + 2, CType(Skill.Value, TSkill).Bonus)         'skill1.Bonus
            Next

            For i = 0 To 2
                Client.Character.SetUpdateFlag(EPlayerFields.PLAYER_FIELD_ARENA_TEAM_INFO_1_1 + (i * 6), ArenaTeamID(i))
            Next

            'DONE: List Auras Here
            For i = 0 To MAX_AURA_EFFECT_FLAGs - 1
                SetUpdateFlag(EUnitFields.UNIT_FIELD_AURAFLAGS + i, Me.ActiveSpells_Flags(i))   ' Aura Flag
                SetUpdateFlag(EUnitFields.UNIT_FIELD_AURAAPPLICATIONS + i, 0)                   ' AuraStackCount
                SetUpdateFlag(EUnitFields.UNIT_FIELD_AURALEVELS + i, 0)                         ' AuraLevel
            Next
            For i = 0 To MAX_AURA_EFFECTs - 1
                If Not Me.ActiveSpells(i) Is Nothing Then
                    SetUpdateFlag(EUnitFields.UNIT_FIELD_AURA + i, Me.ActiveSpells(i).SpellID)  ' Aura Spell ID
                End If
            Next
            'For i = 0 To 15
            '    SetUpdateFlag(EUnitFields.UNIT_FIELD_AURAAPPLICATIONS + i, 0)
            'Next
            'SetUpdateFlag(EUnitFields.UNIT_FIELD_AURASTATE, 2)

            'If Summon <> 0 Then
            '   SetUpdateFlag(EUnitFields.UNIT_FIELD_SUMMON, Summon.GUID)
            'End If

            'SetUpdateFlag(EUnitFields.UNIT_FIELD_TARGET, CType(0, Integer))
            ''StandState, PetLoyalty << 8, ShapeShift << 16, UnkFlag << 24, InvisibilityFlag << 25
            'SetUpdateFlag(EUnitFields.UNIT_FIELD_BYTES_1, CType(StandState, Integer) + CType(Invisibility > InvisibilityLevel.VISIBLE, Integer) * 2 << 24)
            SetUpdateFlag(EUnitFields.UNIT_FIELD_BYTES_1, cBytes1)

            SetUpdateFlag(EUnitFields.UNIT_FIELD_OFFHANDATTACKTIME, AttackTime(1))
            SetUpdateFlag(EUnitFields.UNIT_FIELD_RANGEDATTACKTIME, AttackTime(2))
            SetUpdateFlag(EUnitFields.UNIT_FIELD_MINOFFHANDDAMAGE, OffHandDamage.Minimum)
            SetUpdateFlag(EUnitFields.UNIT_FIELD_MAXOFFHANDDAMAGE, OffHandDamage.Maximum)
            SetUpdateFlag(EUnitFields.UNIT_FIELD_STRENGTH, CType(Strength.Base, Integer))
            SetUpdateFlag(EUnitFields.UNIT_FIELD_AGILITY, CType(Agility.Base, Integer))
            SetUpdateFlag(EUnitFields.UNIT_FIELD_STAMINA, CType(Stamina.Base, Integer))
            SetUpdateFlag(EUnitFields.UNIT_FIELD_SPIRIT, CType(Spirit.Base, Integer))
            SetUpdateFlag(EUnitFields.UNIT_FIELD_INTELLECT, CType(Intellect.Base, Integer))

            SetUpdateFlag(EUnitFields.UNIT_FIELD_ATTACK_POWER_MODS, AttackPowerMods)
            SetUpdateFlag(EUnitFields.UNIT_FIELD_RANGED_ATTACK_POWER_MODS, AttackPowerModsRanged)
            SetUpdateFlag(EUnitFields.UNIT_FIELD_MINRANGEDDAMAGE, RangedDamage.Minimum)
            SetUpdateFlag(EUnitFields.UNIT_FIELD_MAXRANGEDDAMAGE, CType(RangedDamage.Maximum + BaseRangedDamage, Single))
            SetUpdateFlag(EUnitFields.UNIT_FIELD_ATTACK_POWER_MULTIPLIER, 1.0F)
            SetUpdateFlag(EUnitFields.UNIT_FIELD_RANGED_ATTACK_POWER_MULTIPLIER, 1.0F)
            SetUpdateFlag(EUnitFields.UNIT_FIELD_MAXHEALTHMODIFIER, 1.0F)

            For i = 0 To QUEST_SLOTS
                If TalkQuests(i) Is Nothing Then
                    SetUpdateFlag(EPlayerFields.PLAYER_QUEST_LOG_1_1 + i * 4, 0) 'ID
                    SetUpdateFlag(EPlayerFields.PLAYER_QUEST_LOG_1_2 + i * 4, 0) 'State
                    SetUpdateFlag(EPlayerFields.PLAYER_QUEST_LOG_1_3 + i * 4, 0) 'Counts
                    SetUpdateFlag(EPlayerFields.PLAYER_QUEST_LOG_1_4 + i * 4, 0) 'Timer
                Else
                    SetUpdateFlag(EPlayerFields.PLAYER_QUEST_LOG_1_1 + i * 4, TalkQuests(i).ID)
                    SetUpdateFlag(EPlayerFields.PLAYER_QUEST_LOG_1_2 + i * 4, 0)
                    SetUpdateFlag(EPlayerFields.PLAYER_QUEST_LOG_1_3 + i * 4, TalkQuests(i).GetState)
                    SetUpdateFlag(EPlayerFields.PLAYER_QUEST_LOG_1_4 + i * 4, 0) 'Timer
                End If
            Next i

            SetUpdateFlag(EPlayerFields.PLAYER_BLOCK_PERCENTAGE, GetBasePercentBlock(Me, 0))
            SetUpdateFlag(EPlayerFields.PLAYER_DODGE_PERCENTAGE, GetBasePercentDodge(Me, 0))
            SetUpdateFlag(EPlayerFields.PLAYER_PARRY_PERCENTAGE, GetBasePercentParry(Me, 0))

            For i = 0 To PLAYER_EXPLORED_ZONES_SIZE
                SetUpdateFlag(EPlayerFields.PLAYER_EXPLORED_ZONES_1 + i, ZonesExplored(i))
            Next i

            ''SetUpdateFlag(EPlayerFields.PLAYER_REST_STATE_EXPERIENCE, 0)
            ''SetUpdateFlag(EPlayerFields.PLAYER_AMMO_ID, 0)

            ' ''SetUpdateFlag(EPlayerFields.PLAYER_FIELD_BYTES2, 0)
            ' ''SetUpdateFlag(EPlayerFields.PLAYER_FIELD_PVP_MEDALS, 0)

            ''SetUpdateFlag(EPlayerFields.PLAYER_CHOSEN_TITLE, HonorTitle)
            ''SetUpdateFlag(EPlayerFields.PLAYER_FIELD_KNOWN_TITLES, HonorTitles)
            ''SetUpdateFlag(EPlayerFields.PLAYER_FIELD_HONOR_CURRENCY, HonorCurrency)
            ''SetUpdateFlag(EPlayerFields.PLAYER_FIELD_ARENA_CURRENCY, ArenaCurrency)
            ''SetUpdateFlag(EPlayerFields.PLAYER_FIELD_KILLS, HonorKillsToday + (HonorKillsYesterday * 10 << 16))
            ''SetUpdateFlag(EPlayerFields.PLAYER_FIELD_TODAY_CONTRIBUTION, HonorPointsToday)
            ''SetUpdateFlag(EPlayerFields.PLAYER_FIELD_YESTERDAY_CONTRIBUTION, HonorPointsYesterday)
            ''SetUpdateFlag(EPlayerFields.PLAYER_FIELD_LIFETIME_HONORBALE_KILLS, HonorKillsLifeTime)



            For i = EQUIPMENT_SLOT_START To KEYRING_SLOT_END - 1
                If Items.ContainsKey(i) Then
                    If i < EQUIPMENT_SLOT_END Then
                        SetUpdateFlag(EPlayerFields.PLAYER_VISIBLE_ITEM_1_0 + (i * PLAYER_VISIBLE_ITEM_SIZE), CType(Items(i).ItemEntry, Long))

                        'DONE: Include enchantment info
                        For j As Integer = 0 To CType(Items(i), ItemObject).Enchantments.Length - 1
                            If Not CType(Items(i), ItemObject).Enchantments(j) Is Nothing Then SetUpdateFlag(EPlayerFields.PLAYER_VISIBLE_ITEM_1_1 + j + i * PLAYER_VISIBLE_ITEM_SIZE, CType(Items(i), ItemObject).Enchantments(j).SpellID)
                        Next
                        SetUpdateFlag(EPlayerFields.PLAYER_VISIBLE_ITEM_1_PROPERTIES + i * PLAYER_VISIBLE_ITEM_SIZE, CType(Items(i), ItemObject).RandomProperties)
                    End If
                    SetUpdateFlag(EPlayerFields.PLAYER_FIELD_INV_SLOT_HEAD + i * 2, CType(Items(i).GUID, Long))
                Else
                    If i < EQUIPMENT_SLOT_END Then
                        SetUpdateFlag(EPlayerFields.PLAYER_VISIBLE_ITEM_1_0 + i * PLAYER_VISIBLE_ITEM_SIZE, 0)
                        SetUpdateFlag(EPlayerFields.PLAYER_VISIBLE_ITEM_1_PROPERTIES + i * PLAYER_VISIBLE_ITEM_SIZE, 0)
                    End If
                    SetUpdateFlag(EPlayerFields.PLAYER_FIELD_INV_SLOT_HEAD + i * 2, 0)
                End If
            Next



        End Sub                                       'Used for this player's update packets
        Public Sub FillAllUpdateFlags(ByRef Update As UpdateClass, ByRef Character As CharacterObject)
            Dim i As Byte

            Update.SetUpdateFlag(EObjectFields.OBJECT_FIELD_GUID, GUID)
            Update.SetUpdateFlag(EObjectFields.OBJECT_FIELD_SCALE_X, Size)
            Update.SetUpdateFlag(EObjectFields.OBJECT_FIELD_TYPE, CType(25, Integer))

            'If Summon <> 0 Then
            '   Update.SetUpdateFlag(EUnitFields.UNIT_FIELD_SUMMON, Summon.GUID)
            'End If
            Update.SetUpdateFlag(EUnitFields.UNIT_FIELD_HEALTH, CType(Life.Current, Integer))
            Update.SetUpdateFlag(EUnitFields.UNIT_FIELD_POWER1, CType(Mana.Current, Integer))
            Update.SetUpdateFlag(EUnitFields.UNIT_FIELD_POWER2, CType(Rage.Current, Integer))
            Update.SetUpdateFlag(EUnitFields.UNIT_FIELD_POWER4, CType(Energy.Current, Integer))
            Update.SetUpdateFlag(EUnitFields.UNIT_FIELD_MAXHEALTH, CType(Life.Maximum, Integer))
            Update.SetUpdateFlag(EUnitFields.UNIT_FIELD_MAXPOWER1, CType(Mana.Maximum, Integer))
            Update.SetUpdateFlag(EUnitFields.UNIT_FIELD_MAXPOWER2, CType(Rage.Maximum, Integer))
            Update.SetUpdateFlag(EUnitFields.UNIT_FIELD_MAXPOWER4, CType(Energy.Maximum, Integer))
            Update.SetUpdateFlag(EUnitFields.UNIT_FIELD_FLAGS, cUnitFlags)
            Update.SetUpdateFlag(EUnitFields.UNIT_FIELD_LEVEL, CType(Level, Integer))
            Update.SetUpdateFlag(EUnitFields.UNIT_FIELD_FACTIONTEMPLATE, CType(Faction, Integer))


            Update.SetUpdateFlag(EUnitFields.UNIT_FIELD_BYTES_0, CType(CType(Race, Integer) + (CType(Classe, Integer) << 8) + (CType(Gender, Integer) << 16) + (CType(ManaType, Integer) << 24), Integer))
            'StandState, PetLoyalty << 8, ShapeShift << 16, UnkFlag << 24, InvisibilityFlag << 25
            Update.SetUpdateFlag(EUnitFields.UNIT_FIELD_BYTES_1, CType(StandState, Integer) + CType(Invisibility > InvisibilityLevel.VISIBLE, Integer) * 2 << 24)
            'Update.SetUpdateFlag(EUnitFields.UNIT_FIELD_BYTES_1, cBytes1)
            'Update.SetUpdateFlag(EUnitFields.UNIT_FIELD_BYTES_2, &HEEEEEE00) '????????????
            Update.SetUpdateFlag(EUnitFields.UNIT_FIELD_BYTES_2, 0) ' WHAT EXACTLY IS THIS FIELD SUPPOSED TO BE????
            Update.SetUpdateFlag(EUnitFields.UNIT_FIELD_DISPLAYID, Model)
            Update.SetUpdateFlag(EUnitFields.UNIT_FIELD_NATIVEDISPLAYID, Model_Native)
            Update.SetUpdateFlag(EUnitFields.UNIT_FIELD_MOUNTDISPLAYID, Mount)


            Update.SetUpdateFlag(EUnitFields.UNIT_DYNAMIC_FLAGS, cDynamicFlags)

            Update.SetUpdateFlag(EPlayerFields.PLAYER_BYTES, (Skin + (CType(Face, Integer) << 8) + (CType(HairStyle, Integer) << 16) + (CType(HairColor, Integer) << 24)))

            'FacialHair,Unk1,BankSlotsAvailable,RestState;
            Update.SetUpdateFlag(EPlayerFields.PLAYER_BYTES_2, (FacialHair + (&HEE << 8) + (&H1 << 16) + (CType(RestState, Integer) << 24)))

            'Gender,Alchohol,Unk3,HonorRank
            Update.SetUpdateFlag(EPlayerFields.PLAYER_BYTES_3, (Gender + (0 << 8) + (1 << 16) + (0 << 24)))

            Update.SetUpdateFlag(EPlayerFields.PLAYER_FLAGS, cPlayerFlags)

            Update.SetUpdateFlag(EUnitFields.UNIT_FIELD_BOUNDINGRADIUS, CType(0.389, Single))
            Update.SetUpdateFlag(EUnitFields.UNIT_FIELD_COMBATREACH, CType(0.389, Single))
            Update.SetUpdateFlag(EUnitFields.UNIT_FIELD_TARGET, Me.TargetGUID)

            'DONE: List Auras Here
            For i = 0 To MAX_AURA_EFFECT_FLAGs - 1
                Update.SetUpdateFlag(EUnitFields.UNIT_FIELD_AURAFLAGS + i, Me.ActiveSpells_Flags(i))    ' Aura Flag
                Update.SetUpdateFlag(EUnitFields.UNIT_FIELD_AURAAPPLICATIONS + i, 0)                    ' AuraStackCount
                Update.SetUpdateFlag(EUnitFields.UNIT_FIELD_AURALEVELS + i, 0)                          ' AuraLevel
            Next
            For i = 0 To MAX_AURA_EFFECTs - 1
                If Not Me.ActiveSpells(i) Is Nothing Then
                    Update.SetUpdateFlag(EUnitFields.UNIT_FIELD_AURA + i, Me.ActiveSpells(i).SpellID)   ' Aura Spell ID
                End If
            Next
            'For i = 0 To 15
            '    Update.SetUpdateFlag(EUnitFields.UNIT_FIELD_AURAAPPLICATIONS + i, 0)
            'Next
            'Update.SetUpdateFlag(EUnitFields.UNIT_FIELD_AURASTATE, 2)

            Update.SetUpdateFlag(EPlayerFields.PLAYER_GUILDID, GuildID)
            Update.SetUpdateFlag(EPlayerFields.PLAYER_GUILDRANK, GuildRank)

            For i = 0 To 2
                Client.Character.SetUpdateFlag(EPlayerFields.PLAYER_FIELD_ARENA_TEAM_INFO_1_1 + (i * 6), ArenaTeamID(i))
            Next

            ''ComboPoints, Unk4, Unk5, HonorRank?
            ''SetUpdateFlag(EPlayerFields.PLAYER_FIELD_BYTES, CType(Honor.HonorHightestRank, Integer) << 24)
            ''SetUpdateFlag(EPlayerFields.PLAYER_FIELD_BYTES2, 0)
            ''SetUpdateFlag(EPlayerFields.PLAYER_FIELD_PVP_MEDALS, 0)

            ''SetUpdateFlag(EPlayerFields.PLAYER_CHOSEN_TITLE, HonorTitle)
            ''SetUpdateFlag(EPlayerFields.PLAYER_FIELD_KILLS, HonorKillsToday + (HonorKillsYesterday * 10 << 16))
            ''SetUpdateFlag(EPlayerFields.PLAYER_FIELD_TODAY_CONTRIBUTION, HonorPointsToday)
            ''SetUpdateFlag(EPlayerFields.PLAYER_FIELD_YESTERDAY_CONTRIBUTION, HonorPointsYesterday)
            ''SetUpdateFlag(EPlayerFields.PLAYER_FIELD_LIFETIME_HONORBALE_KILLS, HonorKillsLifeTime)



            For i = EQUIPMENT_SLOT_START To EQUIPMENT_SLOT_END - 1
                If Items.ContainsKey(i) Then
                    If i < EQUIPMENT_SLOT_END Then
                        Update.SetUpdateFlag(EPlayerFields.PLAYER_VISIBLE_ITEM_1_0 + (i * PLAYER_VISIBLE_ITEM_SIZE), Items(i).ItemEntry)

                        'DONE: Include enchantment info
                        For j As Integer = 0 To CType(Items(i), ItemObject).Enchantments.Length - 1
                            If Not CType(Items(i), ItemObject).Enchantments(j) Is Nothing Then Update.SetUpdateFlag(EPlayerFields.PLAYER_VISIBLE_ITEM_1_1 + j + i * PLAYER_VISIBLE_ITEM_SIZE, CType(Items(i), ItemObject).Enchantments(j).SpellID)
                        Next
                        Update.SetUpdateFlag(EPlayerFields.PLAYER_VISIBLE_ITEM_1_PROPERTIES + i * PLAYER_VISIBLE_ITEM_SIZE, CType(Items(i), ItemObject).RandomProperties)
                    End If
                    Update.SetUpdateFlag(EPlayerFields.PLAYER_FIELD_INV_SLOT_HEAD + i * 2, CType(Items(i).GUID, Long))
                Else
                    If i < EQUIPMENT_SLOT_END Then
                        Update.SetUpdateFlag(EPlayerFields.PLAYER_VISIBLE_ITEM_1_0 + i * PLAYER_VISIBLE_ITEM_SIZE, 0)
                        Update.SetUpdateFlag(EPlayerFields.PLAYER_VISIBLE_ITEM_1_PROPERTIES + i * PLAYER_VISIBLE_ITEM_SIZE, 0)
                    End If
                    Update.SetUpdateFlag(EPlayerFields.PLAYER_FIELD_INV_SLOT_HEAD + i * 2, 0)
                End If
            Next

        End Sub                                       'Used for other players' update packets
        Public Sub PrepareUpdate(ByRef packet As PacketClass, Optional ByVal UPDATETYPE As Integer = ObjectUpdateType.UPDATETYPE_CREATE_OBJECT)
            packet.AddInt8(UPDATETYPE)
            packet.AddPackGUID(Me.GUID)

            If UPDATETYPE = ObjectUpdateType.UPDATETYPE_CREATE_OBJECT Or UPDATETYPE = ObjectUpdateType.UPDATETYPE_CREATE_OBJECT_SELF Then
                packet.AddInt8(ObjectTypeID.TYPEID_PLAYER)
            End If

            If UPDATETYPE = ObjectUpdateType.UPDATETYPE_CREATE_OBJECT Or UPDATETYPE = ObjectUpdateType.UPDATETYPE_MOVEMENT Or UPDATETYPE = ObjectUpdateType.UPDATETYPE_CREATE_OBJECT_SELF Then
                packet.AddInt8(&H71) 'flags
                packet.AddInt32(0) 'flags2
                packet.AddInt8(0) ' When was this added?
                packet.AddInt32(timeGetTime)
                packet.AddSingle(positionX)
                packet.AddSingle(positionY)
                packet.AddSingle(positionZ)
                packet.AddSingle(orientation)

                packet.AddSingle(0)

                packet.AddSingle(WalkSpeed)
                packet.AddSingle(RunSpeed)
                packet.AddSingle(RunBackSpeed)
                packet.AddSingle(SwimSpeed)
                packet.AddSingle(SwimBackSpeed)
                packet.AddSingle(FlySpeed)
                packet.AddSingle(FlyBackSpeed)
                packet.AddSingle(TurnRate)

                packet.AddUInt32(GuidLOW(Me.GUID))
            End If

            If UPDATETYPE = ObjectUpdateType.UPDATETYPE_CREATE_OBJECT Or UPDATETYPE = ObjectUpdateType.UPDATETYPE_VALUES Or UPDATETYPE = ObjectUpdateType.UPDATETYPE_CREATE_OBJECT_SELF Then
                Dim UpdateCount As Integer = 0
                Dim i As Integer
                For i = 0 To UpdateMask.Count - 1
                    If UpdateMask.Get(i) Then UpdateCount = i
                Next

                packet.AddInt8(CType((UpdateCount + 32) \ 32, Byte))
                packet.AddBitArray(UpdateMask, CType((UpdateCount + 32) \ 32, Byte) * 4)      'OK Flags are Int32, so to byte -> *4
                For i = 0 To UpdateMask.Count - 1
                    If UpdateMask.Get(i) Then
                        If TypeOf UpdateData(i) Is UInteger Then
                            packet.AddUInt32(UpdateData(i))
                        ElseIf TypeOf UpdateData(i) Is Single Then
                            packet.AddSingle(UpdateData(i))
                        Else
                            packet.AddInt32(UpdateData(i))
                        End If
                    End If
                Next

                UpdateData.Clear()
                UpdateMask.SetAll(False)
            End If
        End Sub

        'Packets and Events
        Public Property AFK() As Boolean
            Get
                Return HaveFlag(cPlayerFlags, PlayerFlag.PLAYER_FLAG_AFK)
            End Get
            Set(ByVal Value As Boolean)
                SetFlag(cPlayerFlags, PlayerFlag.PLAYER_FLAG_AFK, Value)

                If Value Then
                    WS.Cluster.ClientSetChatFlag(Client.Index, ChatFlag.FLAG_AFK)
                Else
                    WS.Cluster.ClientSetChatFlag(Client.Index, 0)
                End If
            End Set
        End Property
        Public Property DND() As Boolean
            Get
                Return HaveFlag(cPlayerFlags, PlayerFlag.PLAYER_FLAG_DND)
            End Get
            Set(ByVal Value As Boolean)
                SetFlag(cPlayerFlags, PlayerFlag.PLAYER_FLAG_DND, Value)

                If Value Then
                    WS.Cluster.ClientSetChatFlag(Client.Index, ChatFlag.FLAG_DND)
                Else
                    WS.Cluster.ClientSetChatFlag(Client.Index, 0)
                End If
            End Set
        End Property
        Public Property GM() As Boolean
            Get
                Return HaveFlag(cPlayerFlags, PlayerFlag.PLAYER_FLAG_GM)
            End Get
            Set(ByVal Value As Boolean)
                SetFlag(cPlayerFlags, PlayerFlag.PLAYER_FLAG_GM, Value)

                If Value Then
                    WS.Cluster.ClientSetChatFlag(Client.Index, ChatFlag.FLAG_GM)
                Else
                    WS.Cluster.ClientSetChatFlag(Client.Index, 0)
                End If
            End Set
        End Property

        'Chat
        Public Sub SendChatMessage(ByRef Sender As CharacterObject, ByVal Message As String, ByVal msgType As ChatMsg, ByVal msgLanguage As Integer, Optional ByVal ChannelName As String = "Global", Optional ByVal SendToMe As Boolean = False)
            Dim packet As PacketClass = BuildChatMessage(Sender.GUID, Message, msgType, msgLanguage, GetChatFlag(Sender), ChannelName)

            SendToNearPlayers(packet)
            If SendToMe Then Client.SendMultiplyPackets(packet)
            packet.Dispose()
        End Sub
        Public Sub CommandResponse(ByVal Message As String)
            Dim packet As PacketClass = BuildChatMessage(ServerGUID, Message, ChatMsg.CHAT_MSG_WHISPER, LANGUAGES.LANG_UNIVERSAL)
            Client.Send(packet)
            packet.Dispose()
            Log.WriteLine(LogType.DEBUG, "[{0}:{1}] SMSG_MESSAGECHAT", Client.IP, Client.Port)
        End Sub
        Public Sub SystemMessage(ByVal Message As String)
            SendMessageSystem(Client, Message)
        End Sub


        'Spell/Skill/Talents System
        Public TalentPoints As Byte = 0
        Public AmmoID As Integer = 0
        Public AmmoDPS As Single = 0
        Public OldCritter As CreatureObject = Nothing
        Public Skills As New Dictionary(Of Integer, TSkill)
        Public SkillsPositions As New Dictionary(Of Integer, Short)
        Public Spells As New List(Of Integer)
        Public ReadOnly Property isRooted() As Boolean
            Get
                Return (cUnitFlags And UnitFlags.UNIT_FLAG_ROOTED)
            End Get
        End Property
        Public Sub CastOnSelf(ByVal SpellID As Integer)
            If WS_Spells.SPELLs.ContainsKey(SpellID) = False Then Exit Sub
            Dim Targets As New SpellTargets
            Targets.SetTarget_UNIT(Me)
            WS_Spells.SPELLs(SpellID).Cast(Me, Targets, 0)
        End Sub
        Public Sub ApplySpell(ByVal SpellID As Integer)
            If WS_Spells.SPELLs.ContainsKey(SpellID) = False Then Exit Sub
            Dim t As New SpellTargets
            t.SetTarget_SELF(Me)
            If WS_Spells.SPELLs(SpellID).CanCast(Me, t) = SpellFailedReason.CAST_NO_ERROR Then
                WS_Spells.SPELLs(SpellID).Apply(Me, t)
            End If
        End Sub
        Public Sub LearnSpell(ByVal SpellID As Integer)
            If Spells.Contains(SpellID) Then Exit Sub
            Spells.Add(SpellID)

            If Client Is Nothing Then Exit Sub
            Dim SMSG_LEARNED_SPELL As New PacketClass(OPCODES.SMSG_LEARNED_SPELL)
            SMSG_LEARNED_SPELL.AddInt32(SpellID)
            Client.Send(SMSG_LEARNED_SPELL)
            SMSG_LEARNED_SPELL.Dispose()
        End Sub
        Public Sub UnLearnSpell(ByVal SpellID As Integer)
            If Not Spells.Contains(SpellID) Then
                Log.WriteLine(LogType.WARNING, "Trying to remove SpellID {0} not known by player {1}.", SpellID, Name)
                Exit Sub
            End If
            Spells.Remove(SpellID)

            Dim SMSG_REMOVED_SPELL As New PacketClass(OPCODES.SMSG_REMOVED_SPELL)
            SMSG_REMOVED_SPELL.AddInt32(SpellID)
            Client.Send(SMSG_REMOVED_SPELL)
            SMSG_REMOVED_SPELL.Dispose()

            'DONE: Remove Aura by this spell
            Client.Character.RemoveAuraBySpell(SpellID)
        End Sub
        Public Function HaveSpell(ByVal SpelLID As Integer) As Boolean
            Return Spells.Contains(SpelLID)
        End Function
        Public Sub LearnSkill(ByVal SkillID As Integer, Optional ByVal Current As Int16 = 1, Optional ByVal Maximum As Int16 = 1)
            If Skills.ContainsKey(SkillID) Then

                'DONE: Already know this skill, just increase value
                CType(Skills(SkillID), TSkill).Base = Maximum
                If Current <> 1 Then CType(Skills(SkillID), TSkill).Current = Current

            Else

                'DONE: Learn this skill as new
                Dim i As Short = 0
                For i = 0 To PLAYER_SKILL_INFO_SIZE
                    If Not SkillsPositions.ContainsValue(i) Then
                        Exit For
                    End If
                Next

                If i > PLAYER_SKILL_INFO_SIZE Then Exit Sub

                SkillsPositions.Add(SkillID, i)
                Skills.Add(SkillID, New TSkill(Current, Maximum))
            End If

            If Client Is Nothing Then Exit Sub

            'DONE: Set update parameters
            SetUpdateFlag(EPlayerFields.PLAYER_SKILL_INFO_1_1 + SkillsPositions(SkillID) * 3, SkillID)                            'skill1.Id
            SetUpdateFlag(EPlayerFields.PLAYER_SKILL_INFO_1_1 + SkillsPositions(SkillID) * 3 + 1, Skills(SkillID).GetSkill)       'CType((skill1.CurrentVal(Me) + (skill1.Cap(Me) << 16)), Integer)
        End Sub
        Public Function HaveSkill(ByVal SkillID As Integer, Optional ByVal SkillValue As Integer = 0) As Boolean
            If Skills.ContainsKey(SkillID) Then
                Return CType(Skills(SkillID), TSkill).Current >= SkillValue
            Else
                Return False
            End If
        End Function
        Public Sub UpdateSkill(ByVal SkillID As Integer, Optional ByVal SpeedMod As Single = 0)
            If SkillID = 0 Then Exit Sub
            If CType(Skills(SkillID), TSkill).Current >= CType(Skills(SkillID), TSkill).Maximum Then Exit Sub

            If ((CType(Skills(SkillID), TSkill).Current / CType(Skills(SkillID), TSkill).Maximum) - SpeedMod) < Rnd.NextDouble Then
                CType(Skills(SkillID), TSkill).Increment()
                SetUpdateFlag(EPlayerFields.PLAYER_SKILL_INFO_1_1 + SkillsPositions(SkillID) * 3 + 1, Skills(SkillID).GetSkill)
                SendCharacterUpdate()
            End If
        End Sub


        'XP and Level Managment
        Public RestState As Byte = XPSTATE.Normal
        Public RestXP As Byte = 0
        Public XP As Integer = 0
        Public Sub AddXP(ByVal Ammount As Integer, Optional ByVal VictimGUID As ULong = 0, Optional ByVal LogIt As Boolean = True)
            If Level < MAX_LEVEL Then

                XP = XP + Ammount
                If LogIt Then LogXPGain(Ammount, 0, 0, VictimGUID)

CheckXPAgain:
                If XP >= XPTable(Level) Then
                    XP -= XPTable(Level)
                    Level = Level + 1

                    Dim oldLife As Integer = Life.Maximum
                    Dim oldMana As Integer = Mana.Maximum
                    Dim oldStrength As Integer = Strength.Base
                    Dim oldAgility As Integer = Agility.Base
                    Dim oldStamina As Integer = Stamina.Base
                    Dim oldIntellect As Integer = Intellect.Base
                    Dim oldSpirit As Integer = Spirit.Base
                    CalculateOnLevelUP(Me)
                    Dim SMSG_LEVELUP_INFO As New PacketClass(OPCODES.SMSG_LEVELUP_INFO)
                    SMSG_LEVELUP_INFO.AddInt32(Level)
                    SMSG_LEVELUP_INFO.AddInt32(Life.Maximum - oldLife)
                    SMSG_LEVELUP_INFO.AddInt32(Mana.Maximum - oldMana)
                    SMSG_LEVELUP_INFO.AddInt32(0)
                    SMSG_LEVELUP_INFO.AddInt32(0)
                    SMSG_LEVELUP_INFO.AddInt32(0)
                    SMSG_LEVELUP_INFO.AddInt32(0)
                    SMSG_LEVELUP_INFO.AddInt32(Strength.Base - oldStrength)
                    SMSG_LEVELUP_INFO.AddInt32(Agility.Base - oldAgility)
                    SMSG_LEVELUP_INFO.AddInt32(Stamina.Base - oldStamina)
                    SMSG_LEVELUP_INFO.AddInt32(Intellect.Base - oldIntellect)
                    SMSG_LEVELUP_INFO.AddInt32(Spirit.Base - oldSpirit)
                    Client.Send(SMSG_LEVELUP_INFO)
                    SMSG_LEVELUP_INFO.Dispose()

                    Life.Current = Life.Maximum
                    Mana.Current = Mana.Maximum

                    Resistances(DamageTypes.DMG_PHYSICAL).Base += (Agility.Base - oldAgility) * 2

                    SetUpdateFlag(EPlayerFields.PLAYER_XP, XP)
                    SetUpdateFlag(EPlayerFields.PLAYER_NEXT_LEVEL_XP, XPTable(Level))
                    SetUpdateFlag(EPlayerFields.PLAYER_REST_STATE_EXPERIENCE, 0)
                    SetUpdateFlag(EPlayerFields.PLAYER_CHARACTER_POINTS1, CType(TalentPoints, Integer))
                    SetUpdateFlag(EUnitFields.UNIT_FIELD_LEVEL, CType(Level, Integer))
                    SetUpdateFlag(EUnitFields.UNIT_FIELD_STRENGTH, CType(Strength.Base, Integer))
                    SetUpdateFlag(EUnitFields.UNIT_FIELD_AGILITY, CType(Agility.Base, Integer))
                    SetUpdateFlag(EUnitFields.UNIT_FIELD_STAMINA, CType(Stamina.Base, Integer))
                    SetUpdateFlag(EUnitFields.UNIT_FIELD_SPIRIT, CType(Spirit.Base, Integer))
                    SetUpdateFlag(EUnitFields.UNIT_FIELD_INTELLECT, CType(Intellect.Base, Integer))
                    SetUpdateFlag(EUnitFields.UNIT_FIELD_HEALTH, CType(Life.Current, Integer))
                    SetUpdateFlag(EUnitFields.UNIT_FIELD_POWER1, CType(Mana.Current, Integer))
                    SetUpdateFlag(EUnitFields.UNIT_FIELD_MAXHEALTH, CType(Life.Maximum, Integer))
                    SetUpdateFlag(EUnitFields.UNIT_FIELD_MAXPOWER1, CType(Mana.Maximum, Integer))
                    SetUpdateFlag(EUnitFields.UNIT_FIELD_ATTACK_POWER, AttackPower)
                    SetUpdateFlag(EUnitFields.UNIT_FIELD_ATTACK_POWER_MODS, AttackPowerMods)
                    SetUpdateFlag(EUnitFields.UNIT_FIELD_RANGED_ATTACK_POWER, AttackPowerRanged)
                    SetUpdateFlag(EUnitFields.UNIT_FIELD_RANGED_ATTACK_POWER_MODS, AttackPowerModsRanged)
                    SetUpdateFlag(EUnitFields.UNIT_FIELD_RESISTANCES + DamageTypes.DMG_PHYSICAL, Resistances(DamageTypes.DMG_PHYSICAL).Base)
                    SetUpdateFlag(EUnitFields.UNIT_FIELD_MINDAMAGE, Damage.Minimum)
                    SetUpdateFlag(EUnitFields.UNIT_FIELD_MAXDAMAGE, CType(Damage.Maximum + (AttackPower + AttackPowerMods) * 0.071428571428571425, Single))
                    SetUpdateFlag(EUnitFields.UNIT_FIELD_MINOFFHANDDAMAGE, OffHandDamage.Minimum)
                    SetUpdateFlag(EUnitFields.UNIT_FIELD_MAXOFFHANDDAMAGE, OffHandDamage.Maximum)
                    SetUpdateFlag(EUnitFields.UNIT_FIELD_MINRANGEDDAMAGE, RangedDamage.Minimum)
                    SetUpdateFlag(EUnitFields.UNIT_FIELD_MAXRANGEDDAMAGE, CType(RangedDamage.Maximum + BaseRangedDamage, Single))

                    For Each Skill As KeyValuePair(Of Integer, TSkill) In Skills
                        SetUpdateFlag(EPlayerFields.PLAYER_SKILL_INFO_1_1 + SkillsPositions(Skill.Key) * 3 + 1, Skill.Value.GetSkill)       'CType((skill1.CurrentVal(Me) + (skill1.Cap(Me) << 16)), Integer)
                    Next

                    SendCharacterUpdate()
                    UpdateManaRegen()
                Else
                    SetUpdateFlag(EPlayerFields.PLAYER_XP, XP)
                    SendCharacterUpdate(False)
                End If

                'We just dinged more than one level
                If XP >= XPTable(Level) AndAlso Level < MAX_LEVEL Then GoTo CheckXPAgain

                'Fix if we add very big number XP
                If XP > XPTable(Level) Then XP = XPTable(Level)

                SaveCharacter()
            End If
        End Sub

        'Item Managment
        Public Items As New Dictionary(Of Byte, ItemObject)
        Public Items_AvailableBankSlots As Byte = 0
        Public Sub ItemADD(ByVal ItemEntry As Integer, ByVal dstBag As Byte, ByVal dstSlot As Byte, Optional ByVal Count As Integer = 1)
            Dim tmpItem As New ItemObject(ItemEntry, GUID)
            If Count > tmpItem.ItemInfo.MaxCount Then Count = tmpItem.ItemInfo.MaxCount
            tmpItem.StackCount = Count
            ItemSETSLOT(tmpItem, dstBag, dstSlot)
            If dstBag = 0 And dstSlot < EQUIPMENT_SLOT_END AndAlso Client IsNot Nothing Then UpdateAddItemStats(tmpItem, dstSlot)
            Log.WriteLine(LogType.DEBUG, "Adding [{0}] of Item [{1}]", Count, ItemEntry)
        End Sub
        Public Sub ItemREMOVE(ByVal srcBag As Byte, ByVal srcSlot As Byte, ByVal Destroy As Boolean, ByVal Update As Boolean)
            If srcBag = 0 Then
                If srcSlot < EQUIPMENT_SLOT_END Then
                    SetUpdateFlag(EPlayerFields.PLAYER_VISIBLE_ITEM_1_0 + srcSlot * PLAYER_VISIBLE_ITEM_SIZE, 0)
                    UpdateRemoveItemStats(Items(srcSlot), srcSlot)
                End If
                SetUpdateFlag(EPlayerFields.PLAYER_FIELD_INV_SLOT_HEAD + srcSlot * 2, 0)

                Database.Update(String.Format("UPDATE characters_inventory SET item_slot = {0}, item_bag = {1} WHERE item_guid = {2};", ITEM_SLOT_NULL, ITEM_BAG_NULL, Items(srcSlot).GUID - GUID_ITEM))
                If Destroy Then CType(Items(srcSlot), ItemObject).Delete()
                Items.Remove(srcSlot)
                If Update Then SendCharacterUpdate()
            Else
                Database.Update(String.Format("UPDATE characters_inventory SET item_slot = {0}, item_bag = {1} WHERE item_guid = {2};", ITEM_SLOT_NULL, ITEM_BAG_NULL, Items(srcBag).Items(srcSlot).GUID - GUID_ITEM))
                If Destroy Then CType(Items(srcBag).Items(srcSlot), ItemObject).Delete()
                CType(Items(srcBag), ItemObject).Items.Remove(srcSlot)
                If Update Then SendItemUpdate(Items(srcBag))
            End If
        End Sub
        Public Sub ItemREMOVE(ByVal ItemGUID As ULong, ByVal Destroy As Boolean, ByVal Update As Boolean)
            'DONE: Search in inventory
            For slot As Byte = EQUIPMENT_SLOT_START To KEYRING_SLOT_END - 1
                If Items.ContainsKey(slot) Then
                    If Items(slot).GUID = ItemGUID Then

                        Database.Update(String.Format("UPDATE characters_inventory SET item_slot = {0}, item_bag = {1} WHERE item_guid = {2};", ITEM_SLOT_NULL, ITEM_BAG_NULL, Items(slot).GUID - GUID_ITEM))
                        If slot < EQUIPMENT_SLOT_END Then
                            SetUpdateFlag(EPlayerFields.PLAYER_VISIBLE_ITEM_1_0 + slot * PLAYER_VISIBLE_ITEM_SIZE, 0)
                            UpdateRemoveItemStats(Items(slot), slot)
                        End If
                        SetUpdateFlag(EPlayerFields.PLAYER_FIELD_INV_SLOT_HEAD + slot * 2, 0)

                        If Destroy Then Items(slot).Delete()
                        Items.Remove(slot)
                        If Update Then SendCharacterUpdate(True)
                        Exit Sub

                    End If
                End If
            Next slot

            'DONE: Search in bags
            For bag As Byte = INVENTORY_SLOT_BAG_1 To INVENTORY_SLOT_BAG_END - 1
                If Items.ContainsKey(bag) Then

                    'DONE: Search this bag
                    Dim slot As Byte = 0
                    For slot = 0 To Items(bag).ItemInfo.ContainerSlots - 1
                        If Items(bag).Items.ContainsKey(slot) = False Then

                            If Items(bag).Items(slot).GUID = ItemGUID Then
                                Database.Update(String.Format("UPDATE characters_inventory SET item_slot = {0}, item_bag = {1} WHERE item_guid = {2};", ITEM_SLOT_NULL, ITEM_BAG_NULL, Items(bag).Items(slot).GUID - GUID_ITEM))

                                If Destroy Then Items(bag).Items(slot).Delete()
                                Items(bag).Items.Remove(slot)
                                If Update Then SendItemUpdate(Items(bag))
                                Exit Sub
                            End If
                        End If
                    Next slot
                End If
            Next

            Throw New ApplicationException("Unable to remove item becouse character doesn't have it in inventory or bags.")
        End Sub
        Public Function ItemADD(ByRef Item As ItemObject) As Boolean
            Dim tmpEntry As Integer = Item.ItemEntry
            Dim tmpCount As Byte = Item.StackCount
            If ItemADD_AutoSlot(Item) AndAlso (Not Client Is Nothing) Then
                'DONE: Fire quest event to check for if this item is required for quest
                'NOTE: Not only quest items are needed for quests
                OnQuestItemAdd(Me, tmpEntry, tmpCount)

                Return True
            End If
            Return False
        End Function

        Public BuyBackTimeStamp(0 To ((BUYBACK_SLOT_END - BUYBACK_SLOT_START) - 1)) As Integer

        Public Sub ItemADD_BuyBack(ByRef Item As ItemObject)
            Dim i As Byte, Slot As Byte, eSlot As Byte, OldestTime As Integer, OldestSlot As Byte
            Slot = ITEM_SLOT_NULL
            OldestTime = GetTimestamp(Now)
            OldestSlot = ITEM_SLOT_NULL
            For i = BUYBACK_SLOT_START To BUYBACK_SLOT_END - 1
                If Items.ContainsKey(i) = False OrElse BuyBackTimeStamp(i - BUYBACK_SLOT_START) = 0 Then 'Woho we found a empty slot to use!
                    If Slot = ITEM_SLOT_NULL Then Slot = i
                Else 'If not let's find out the oldest item in the buyback
                    If BuyBackTimeStamp(i - BUYBACK_SLOT_START) < OldestTime Then
                        OldestTime = BuyBackTimeStamp(i - BUYBACK_SLOT_START)
                        OldestSlot = i
                    End If
                End If
            Next
            If Slot = ITEM_SLOT_NULL Then 'We never found a empty slot so let's just remove the oldest item
                If OldestSlot <> ITEM_SLOT_NULL Then Exit Sub 'Somehow it all got very wrong o_O
                ItemREMOVE(0, OldestSlot, True, True)
                Slot = OldestSlot
            End If
            'Now we have a empty slow so let's just put our item there
            eSlot = Slot - BUYBACK_SLOT_START
            BuyBackTimeStamp(eSlot) = GetTimestamp(Now)
            SetUpdateFlag(EPlayerFields.PLAYER_FIELD_BUYBACK_TIMESTAMP_1 + eSlot, BuyBackTimeStamp(eSlot))
            SetUpdateFlag(EPlayerFields.PLAYER_FIELD_BUYBACK_PRICE_1 + eSlot, Item.ItemInfo.SellPrice * Item.StackCount)
            ItemSETSLOT(Item, 0, Slot)
        End Sub
        Public Function ItemADD_AutoSlot(ByRef Item As ItemObject) As Boolean

            If Item.ItemInfo.Stackable > 1 Then
                'DONE: Search for stackable in main bag
                For slot As Byte = INVENTORY_SLOT_ITEM_START To INVENTORY_SLOT_ITEM_END - 1
                    If Items.ContainsKey(slot) AndAlso Items(slot).ItemEntry = Item.ItemEntry AndAlso Items(slot).StackCount < Items(slot).ItemInfo.Stackable Then
                        Dim stacked As Byte = Items(slot).ItemInfo.Stackable - Items(slot).StackCount
                        If stacked >= Item.StackCount Then
                            Items(slot).StackCount += Item.StackCount
                            Item.Delete()

                            SendItemUpdate(Items(slot))
                            Return True
                        Else
                            Items(slot).StackCount += stacked
                            Item.StackCount -= stacked
                            Items(slot).Save()

                            SendItemUpdate(Items(slot))
                            Return ItemADD_AutoSlot(Item)
                        End If
                    End If
                Next
                'DONE: Search for stackable in bags
                For j As Byte = INVENTORY_SLOT_BAG_START To INVENTORY_SLOT_BAG_END - 1
                    If Items.ContainsKey(j) Then

                        For Each i As KeyValuePair(Of Byte, ItemObject) In Items(j).Items
                            If CType(i.Value, ItemObject).ItemEntry = Item.ItemEntry AndAlso CType(i.Value, ItemObject).StackCount < CType(i.Value, ItemObject).ItemInfo.Stackable Then
                                Dim stacked As Byte = CType(i.Value, ItemObject).ItemInfo.Stackable - CType(i.Value, ItemObject).StackCount
                                If stacked >= Item.StackCount Then
                                    CType(i.Value, ItemObject).StackCount += Item.StackCount
                                    Item.Delete()

                                    SendItemUpdate(CType(i.Value, ItemObject))
                                    Return True
                                Else
                                    CType(i.Value, ItemObject).StackCount += stacked
                                    Item.StackCount -= stacked
                                    CType(i.Value, ItemObject).Save()
                                    SendItemUpdate(CType(i.Value, ItemObject))
                                    Return ItemADD_AutoSlot(Item)
                                End If
                            End If
                        Next
                    End If
                Next
            End If


            If Item.ItemInfo.BagFamily = ITEM_BAG.KEYRING Then
                'DONE: Insert as keyring
                For i As Byte = KEYRING_SLOT_START To KEYRING_SLOT_END - 1
                    If Not Items.ContainsKey(i) Then
                        Return ItemSETSLOT(Item, 0, i)
                    End If
                Next
            ElseIf Item.ItemInfo.BagFamily <> 0 Then
                'DONE: Insert in free special bag
                For j As Byte = INVENTORY_SLOT_BAG_START To INVENTORY_SLOT_BAG_END - 1
                    If Items.ContainsKey(j) AndAlso CType(Items(j), ItemObject).ItemInfo.SubClass <> ITEM_SUBCLASS.ITEM_SUBCLASS_BAG Then
                        If (CType(Items(j), ItemObject).ItemInfo.SubClass = ITEM_SUBCLASS.ITEM_SUBCLASS_SOUL_BAG AndAlso Item.ItemInfo.BagFamily = ITEM_BAG.SOUL_SHARD) OrElse _
                        (CType(Items(j), ItemObject).ItemInfo.SubClass = ITEM_SUBCLASS.ITEM_SUBCLASS_HERB_BAG AndAlso Item.ItemInfo.BagFamily = ITEM_BAG.HERB) OrElse _
                        (CType(Items(j), ItemObject).ItemInfo.SubClass = ITEM_SUBCLASS.ITEM_SUBCLASS_ENCHANTING_BAG AndAlso Item.ItemInfo.BagFamily = ITEM_BAG.ENCHANTING) OrElse _
                        (CType(Items(j), ItemObject).ItemInfo.SubClass = ITEM_SUBCLASS.ITEM_SUBCLASS_ENGINEERING_BAG AndAlso Item.ItemInfo.BagFamily = ITEM_BAG.ENGINEERING) OrElse _
                        (CType(Items(j), ItemObject).ItemInfo.SubClass = ITEM_SUBCLASS.ITEM_SUBCLASS_MINNING_BAG AndAlso Item.ItemInfo.BagFamily = ITEM_BAG.MINNING) OrElse _
                        (CType(Items(j), ItemObject).ItemInfo.SubClass = ITEM_SUBCLASS.ITEM_SUBCLASS_GEM_BAG AndAlso Item.ItemInfo.BagFamily = ITEM_BAG.JEWELCRAFTING) Then
                            For i As Byte = 0 To CType(Items(i), ItemObject).ItemInfo.ContainerSlots - 1
                                If Not Items(j).Items.ContainsKey(i) Then
                                    Return ItemSETSLOT(Item, j, i)
                                End If
                            Next
                        End If
                    End If
                Next
            End If


            'DONE: Insert as new item in inventory
            For i As Byte = INVENTORY_SLOT_ITEM_START To INVENTORY_SLOT_ITEM_END - 1
                If Not Items.ContainsKey(i) Then
                    Return ItemSETSLOT(Item, 0, i)
                End If
            Next
            'DONE: Insert as new item in bag
            For j As Byte = INVENTORY_SLOT_BAG_START To INVENTORY_SLOT_BAG_END - 1
                If Items.ContainsKey(j) AndAlso CType(Items(j), ItemObject).ItemInfo.SubClass = ITEM_SUBCLASS.ITEM_SUBCLASS_BAG Then
                    For i As Byte = 0 To CType(Items(j), ItemObject).ItemInfo.ContainerSlots - 1
                        If (Not Items(j).Items.ContainsKey(i)) AndAlso ItemCANEQUIP(Item, j, i) = WS_Items.InventoryChangeFailure.EQUIP_ERR_OK Then
                            Return ItemSETSLOT(Item, j, i)
                        End If
                    Next
                End If
            Next

            'DONE: Send error, not free slot
            Dim response As New PacketClass(OPCODES.SMSG_INVENTORY_CHANGE_FAILURE)
            response.AddInt8(InventoryChangeFailure.EQUIP_ERR_INVENTORY_FULL)
            response.AddUInt64(0)
            response.AddUInt64(0)
            response.AddInt8(0)
            Client.Send(response)
            response.Dispose()
            Return False
        End Function
        Public Function ItemADD_AutoBag(ByRef Item As ItemObject, ByVal dstBag As Byte) As Boolean
            If dstBag = 0 Then
                If Item.ItemInfo.Stackable > 1 Then
                    'DONE: Search for stackable in main bag
                    For slot As Byte = INVENTORY_SLOT_ITEM_START To INVENTORY_SLOT_ITEM_END - 1
                        If Items(slot).ItemEntry = Item.ItemEntry AndAlso Items(slot).StackCount < Items(slot).ItemInfo.Stackable Then
                            Dim stacked As Byte = Items(slot).ItemInfo.Stackable - Items(slot).StackCount
                            If stacked >= Item.StackCount Then
                                Item.Delete()
                                Return True
                            Else
                                Item.StackCount -= stacked
                            End If

                            Items(slot).Save()
                            SendItemUpdate(Items(slot))
                        End If
                    Next
                End If
                'DONE: Insert as keyring
                If Item.ItemInfo.BagFamily = ITEM_BAG.KEYRING Then
                    For slot As Byte = KEYRING_SLOT_START To KEYRING_SLOT_END - 1
                        If Not Items.ContainsKey(slot) Then
                            Return ItemSETSLOT(Item, 0, slot)
                        End If
                    Next
                End If
                'DONE: Insert as new item in inventory
                For slot As Byte = INVENTORY_SLOT_ITEM_START To INVENTORY_SLOT_ITEM_END - 1
                    If Not Items.ContainsKey(slot) Then
                        Return ItemSETSLOT(Item, 0, slot)
                    End If
                Next

            Else
                If Items.ContainsKey(dstBag) Then

                    If Item.ItemInfo.Stackable > 1 Then
                        'DONE: Search for stackable in bag
                        For Each i As KeyValuePair(Of Byte, ItemObject) In Items(dstBag).Items
                            If CType(i.Value, ItemObject).ItemEntry = Item.ItemEntry AndAlso CType(i.Value, ItemObject).StackCount < CType(i.Value, ItemObject).ItemInfo.Stackable Then
                                Dim stacked As Byte = CType(i.Value, ItemObject).ItemInfo.Stackable - CType(i.Value, ItemObject).StackCount
                                If stacked >= Item.StackCount Then
                                    Item.Delete()
                                    Return True
                                Else
                                    Item.StackCount -= stacked
                                End If

                                CType(i.Value, ItemObject).Save()
                                SendItemUpdate(CType(i.Value, ItemObject))
                            End If
                        Next
                    End If
                    'DONE: Insert as new item in bag
                    If CType(Items(dstBag), ItemObject).ItemInfo.SubClass = ITEM_SUBCLASS.ITEM_SUBCLASS_BAG Then
                        For slot As Byte = 0 To CType(Items(dstBag), ItemObject).ItemInfo.ContainerSlots - 1
                            If (Not Items(dstBag).Items.ContainsKey(slot)) AndAlso ItemCANEQUIP(Item, dstBag, slot) = InventoryChangeFailure.EQUIP_ERR_OK Then
                                Return ItemSETSLOT(Item, dstBag, slot)
                            End If
                        Next
                    End If


                End If
            End If

            'DONE: Send error, not free slot
            Dim response As New PacketClass(OPCODES.SMSG_INVENTORY_CHANGE_FAILURE)
            response.AddInt8(InventoryChangeFailure.EQUIP_ERR_BAG_FULL)
            response.AddUInt64(Item.GUID)
            response.AddUInt64(0)
            response.AddInt8(0)
            Client.Send(response)
            response.Dispose()
            Return False
        End Function
        Public Function ItemSETSLOT(ByRef Item As ItemObject, ByVal dstBag As Byte, ByVal dstSlot As Byte) As Boolean
            If dstBag = 0 Then
                'DONE: Bind a nonbinded BIND WHEN PICKED UP item or a nonbinded quest item
                If Item.ItemInfo.Bonding = ITEM_BONDING_TYPE.BIND_WHEN_PICKED_UP AndAlso Item.IsSoulBound = False Then Item.SoulbindItem()
                If (Item.ItemInfo.Bonding = ITEM_BONDING_TYPE.BIND_UNK_QUESTITEM1 OrElse Item.ItemInfo.Bonding = ITEM_BONDING_TYPE.BIND_UNK_QUESTITEM2) AndAlso Item.IsSoulBound = False Then Item.SoulbindItem()
                'DONE: Put in inventory
                Items(dstSlot) = Item
                Database.Update(String.Format("UPDATE characters_inventory SET item_slot = {0}, item_bag = {1}, item_stackCount = {2} WHERE item_guid = {3};", dstSlot, Me.GUID, Item.StackCount, Item.GUID - GUID_ITEM))

                SetUpdateFlag(EPlayerFields.PLAYER_FIELD_INV_SLOT_HEAD + dstSlot * 2, Item.GUID)
                If dstSlot < EQUIPMENT_SLOT_END Then
                    SetUpdateFlag(EPlayerFields.PLAYER_VISIBLE_ITEM_1_0 + dstSlot * PLAYER_VISIBLE_ITEM_SIZE, Item.ItemEntry)
                    For j As Integer = 0 To Item.Enchantments.Length - 1
                        If Not Item.Enchantments(j) Is Nothing Then SetUpdateFlag(EPlayerFields.PLAYER_VISIBLE_ITEM_1_1 + j + dstSlot * PLAYER_VISIBLE_ITEM_SIZE, Item.Enchantments(j).SpellID)
                    Next
                    SetUpdateFlag(EPlayerFields.PLAYER_VISIBLE_ITEM_1_PROPERTIES + dstSlot * PLAYER_VISIBLE_ITEM_SIZE, Item.RandomProperties)
                    'DONE: Bind a nonbinded BIND WHEN EQUIPPED item
                    If Item.ItemInfo.Bonding = ITEM_BONDING_TYPE.BIND_WHEN_EQUIPED AndAlso Item.IsSoulBound = False Then Item.SoulbindItem()
                End If
            Else
                'DONE: Put in bag
                CType(Items(dstBag), ItemObject).Items(dstSlot) = Item
                Database.Update(String.Format("UPDATE characters_inventory SET item_slot = {0}, item_bag = {1} WHERE item_guid = {2};", dstSlot, Items(dstBag).GUID, Item.GUID - GUID_ITEM))

                If Not Client Is Nothing Then SendItemUpdate(Items(dstBag))
            End If

            'DONE: Send updates
            If Not Client Is Nothing Then
                SendItemAndCharacterUpdate(Item, ObjectUpdateType.UPDATETYPE_CREATE_OBJECT)
            End If
            Return True
        End Function
        Public Function ItemCOUNT(ByVal ItemEntry As Integer) As Integer
            Dim count As Integer = 0

            'DONE: Search in inventory
            For slot As Byte = EQUIPMENT_SLOT_START To INVENTORY_SLOT_ITEM_END - 1
                If Items.ContainsKey(slot) Then
                    If Items(slot).ItemEntry = ItemEntry Then count += Items(slot).StackCount
                End If
            Next slot

            'DONE: Search in keyring
            For slot As Byte = KEYRING_SLOT_START To KEYRING_SLOT_END - 1
                If Items.ContainsKey(slot) Then
                    If Items(slot).ItemEntry = ItemEntry Then count += Items(slot).StackCount
                End If
            Next slot

            'DONE: Search in bags
            For bag As Byte = INVENTORY_SLOT_BAG_1 To INVENTORY_SLOT_BAG_END - 1
                If Items.ContainsKey(bag) Then

                    'DONE: Search this bag
                    Dim slot As Byte = 0
                    For slot = 0 To Items(bag).ItemInfo.ContainerSlots - 1
                        If Items(bag).Items.ContainsKey(slot) Then
                            If Items(bag).Items(slot).ItemEntry = ItemEntry Then count += Items(bag).Items(slot).StackCount
                        End If
                    Next slot
                End If
            Next

            Return count
        End Function
        Public Function ItemCONSUME(ByVal ItemEntry As Integer, ByVal Count As Integer) As Boolean
            'DONE: Search in inventory
            For slot As Byte = EQUIPMENT_SLOT_START To INVENTORY_SLOT_ITEM_END - 1
                If Items.ContainsKey(slot) Then
                    If Items(slot).ItemEntry = ItemEntry Then

                        If Items(slot).StackCount <= Count Then
                            Count -= Items(slot).StackCount
                            ItemREMOVE(0, slot, True, True)
                            If Count <= 0 Then Return True
                        Else
                            Items(slot).StackCount -= Count
                            Items(slot).Save(False)
                            SendItemUpdate(Items(slot))
                            Return True
                        End If

                    End If
                End If
            Next slot


            'DONE: Search in keyring slot
            For slot As Byte = KEYRING_SLOT_START To KEYRING_SLOT_END - 1
                If Items.ContainsKey(slot) Then
                    If Items(slot).ItemEntry = ItemEntry Then

                        If Items(slot).StackCount <= Count Then
                            Count -= Items(slot).StackCount
                            ItemREMOVE(0, slot, True, True)
                            If Count <= 0 Then Return True
                        Else
                            Items(slot).StackCount -= Count
                            Items(slot).Save(False)
                            SendItemUpdate(Items(slot))
                            Return True
                        End If

                    End If
                End If
            Next slot


            'DONE: Search in bags
            For bag As Byte = INVENTORY_SLOT_BAG_1 To INVENTORY_SLOT_BAG_END - 1
                If Items.ContainsKey(bag) Then

                    'DONE: Search this bag
                    Dim slot As Byte = 0
                    For slot = 0 To Items(bag).ItemInfo.ContainerSlots - 1
                        If Items(bag).Items.ContainsKey(slot) Then
                            If Items(bag).Items(slot).ItemEntry = ItemEntry Then

                                If Items(bag).Items(slot).StackCount <= Count Then
                                    Count -= Items(bag).Items(slot).StackCount
                                    ItemREMOVE(bag, slot, True, True)
                                    If Count <= 0 Then Return True
                                Else
                                    Items(bag).Items(slot).StackCount -= Count
                                    Items(bag).Items(slot).Save(False)
                                    SendItemUpdate(Items(bag).Items(slot))
                                    Return True
                                End If

                            End If
                        End If
                    Next slot
                End If
            Next

            Return False
        End Function
        Public Function ItemFREESLOTS() As Integer
            Dim foundFreeSlots As Integer = 0

            'DONE Find space in main bag
            For slot As Byte = INVENTORY_SLOT_ITEM_START To INVENTORY_SLOT_ITEM_END - 1
                If Not Items.ContainsKey(slot) Then
                    foundFreeSlots += 1
                End If
            Next slot

            'DONE: Find space in other bags
            For bag As Byte = INVENTORY_SLOT_BAG_START To INVENTORY_SLOT_BAG_END - 1
                If Items.ContainsKey(bag) Then
                    For slot As Byte = 0 To Items(bag).ItemInfo.ContainerSlots - 1
                        If Not Items(bag).Items.ContainsKey(slot) Then
                            foundFreeSlots += 1
                        End If
                    Next slot
                End If
            Next bag

            Return foundFreeSlots
        End Function
        Public Function ItemCANEQUIP(ByVal Item As ItemObject, ByVal dstBag As Byte, ByVal dstSlot As Byte) As InventoryChangeFailure
            'DONE: if dead then EQUIP_ERR_YOU_ARE_DEAD
            If DEAD Then Return InventoryChangeFailure.EQUIP_ERR_YOU_ARE_DEAD

            Dim ItemInfo As ItemInfo = Item.ItemInfo

            Try
                If dstBag = 0 Then
                    'DONE: items in inventory
                    Select Case dstSlot
                        Case Is < EQUIPMENT_SLOT_END
                            If ItemInfo.IsContainer Then
                                Return InventoryChangeFailure.EQUIP_ERR_ITEM_CANT_BE_EQUIPPED
                            End If

                            If Not HaveFlag(ItemInfo.AvailableClasses, Classe - 1) Then
                                Return InventoryChangeFailure.EQUIP_ERR_YOU_CAN_NEVER_USE_THAT_ITEM
                            End If
                            If Not HaveFlag(ItemInfo.AvailableRaces, Race - 1) Then
                                Return InventoryChangeFailure.EQUIP_ERR_YOU_CAN_NEVER_USE_THAT_ITEM2
                            End If
                            If ItemInfo.ReqLevel > Level Then
                                Return InventoryChangeFailure.EQUIP_ERR_CANT_EQUIP_LEVEL_I
                            End If

                            Dim tmp As Boolean = False
                            For Each SlotVal As Byte In ItemInfo.GetSlots
                                If dstSlot = SlotVal Then tmp = True
                            Next
                            If Not tmp Then Return InventoryChangeFailure.EQUIP_ERR_ITEM_DOESNT_GO_TO_SLOT

                            If dstSlot = EQUIPMENT_SLOT_MAINHAND AndAlso ItemInfo.InventoryType = INVENTORY_TYPES.INVTYPE_TWOHAND_WEAPON AndAlso Items.ContainsKey(EQUIPMENT_SLOT_OFFHAND) Then
                                Return InventoryChangeFailure.EQUIP_ERR_CANT_EQUIP_WITH_TWOHANDED
                            End If
                            If dstSlot = EQUIPMENT_SLOT_OFFHAND AndAlso Not Items.ContainsKey(EQUIPMENT_SLOT_MAINHAND) Then
                                If Items(EQUIPMENT_SLOT_MAINHAND).ItemInfo.InventoryType = INVENTORY_TYPES.INVTYPE_TWOHAND_WEAPON Then
                                    Return InventoryChangeFailure.EQUIP_ERR_CANT_EQUIP_WITH_TWOHANDED
                                End If
                            End If
                            If dstSlot = EQUIPMENT_SLOT_OFFHAND AndAlso ItemInfo.InventoryType = INVENTORY_TYPES.INVTYPE_WEAPON Then
                                If Not Skills.ContainsKey(SKILL_IDs.SKILL_DUAL_WIELD) Then Return InventoryChangeFailure.EQUIP_ERR_CANT_DUAL_WIELD
                            End If

                            If ItemInfo.GetReqSkill <> 0 Then
                                If Not Skills.ContainsKey(CType(ItemInfo.GetReqSkill, Integer)) Then Return InventoryChangeFailure.EQUIP_ERR_NO_REQUIRED_PROFICIENCY
                            End If
                            If ItemInfo.GetReqSpell <> 0 Then
                                If Not Spells.Contains(CType(ItemInfo.GetReqSpell, Integer)) Then Return InventoryChangeFailure.EQUIP_ERR_NO_REQUIRED_PROFICIENCY
                            End If
                            If ItemInfo.ReqSkill <> 0 Then
                                If Not Skills.ContainsKey(CType(ItemInfo.ReqSkill, Integer)) Then Return InventoryChangeFailure.EQUIP_ERR_NO_REQUIRED_PROFICIENCY
                                If Skills(CType(ItemInfo.ReqSkill, Integer)).GetSkill < ItemInfo.ReqSkillRank Then Return InventoryChangeFailure.EQUIP_ERR_ERR_CANT_EQUIP_SKILL
                            End If
                            If ItemInfo.ReqSpell <> 0 Then
                                If Not Spells.Contains(CType(ItemInfo.ReqSpell, Integer)) Then Return InventoryChangeFailure.EQUIP_ERR_NO_REQUIRED_PROFICIENCY
                            End If
                            'NOTE: Not used anymore in new honor system
                            'If ITEMDatabase(ItemID).ReqHonorRank <> 0 Then
                            '    If Honor.HonorHightestRank < ITEMDatabase(ItemID).ReqHonorRank Then Return InventoryChangeFailure.EQUIP_ITEM_RANK_NOT_ENOUGH
                            'End If
                            If ItemInfo.ReqFaction <> 0 Then
                                If Client.Character.GetReputation(ItemInfo.ReqFaction) <= ItemInfo.ReqFactionLevel Then Return InventoryChangeFailure.EQUIP_ERR_CANT_EQUIP_REPUTATION
                            End If

                            Return InventoryChangeFailure.EQUIP_ERR_OK

                        Case Is < INVENTORY_SLOT_BAG_END
                            If Not ItemInfo.IsContainer Then Return InventoryChangeFailure.EQUIP_ERR_NOT_A_BAG
                            If Not Item.IsFree Then Return InventoryChangeFailure.EQUIP_ERR_NONEMPTY_BAG_OVER_OTHER_BAG
                            Return InventoryChangeFailure.EQUIP_ERR_OK

                        Case Is < INVENTORY_SLOT_ITEM_END
                            If ItemInfo.IsContainer Then
                                'DONE: Move only empty bags
                                If Item.IsFree Then
                                    Return InventoryChangeFailure.EQUIP_ERR_OK
                                Else
                                    Return InventoryChangeFailure.EQUIP_ERR_CAN_ONLY_DO_WITH_EMPTY_BAGS
                                End If
                            End If
                            Return InventoryChangeFailure.EQUIP_ERR_OK

                        Case Is < BANK_SLOT_ITEM_END
                            If ItemInfo.IsContainer Then
                                'DONE: Move only empty bags
                                If Item.IsFree Then
                                    Return InventoryChangeFailure.EQUIP_ERR_OK
                                Else
                                    Return InventoryChangeFailure.EQUIP_ERR_CAN_ONLY_DO_WITH_EMPTY_BAGS
                                End If
                            End If
                            Return InventoryChangeFailure.EQUIP_ERR_OK

                        Case Is < BANK_SLOT_BAG_END
                            If dstSlot >= (BANK_SLOT_BAG_START + Me.Items_AvailableBankSlots) Then Return InventoryChangeFailure.EQUIP_ERR_MUST_PURCHASE_THAT_BAG_SLOT
                            If Not ItemInfo.IsContainer Then Return InventoryChangeFailure.EQUIP_ERR_NOT_A_BAG
                            If Not Item.IsFree Then Return InventoryChangeFailure.EQUIP_ERR_NONEMPTY_BAG_OVER_OTHER_BAG
                            Return InventoryChangeFailure.EQUIP_ERR_OK

                        Case Is < KEYRING_SLOT_END
                            If ItemInfo.BagFamily <> ITEM_BAG.KEYRING Then Return WS_Items.InventoryChangeFailure.EQUIP_ERR_ITEM_DOESNT_GO_TO_SLOT
                            Return InventoryChangeFailure.EQUIP_ERR_OK

                        Case Else
                            Return InventoryChangeFailure.EQUIP_ERR_ITEM_CANT_BE_EQUIPPED
                    End Select
                Else
                    'DONE: Items in bags
                    If Not Items.ContainsKey(dstBag) Then Return InventoryChangeFailure.EQUIP_ERR_ITEM_DOESNT_GO_INTO_BAG
                    If ItemInfo.IsContainer Then
                        If Item.IsFree Then
                            Return InventoryChangeFailure.EQUIP_ERR_OK
                        Else
                            Return InventoryChangeFailure.EQUIP_ERR_CAN_ONLY_DO_WITH_EMPTY_BAGS
                        End If
                    End If

                    If Items(dstBag).ItemInfo.ObjectClass = ITEM_CLASS.ITEM_CLASS_QUIVER Then
                        If ItemInfo.ObjectClass = ITEM_CLASS.ITEM_CLASS_PROJECTILE Then
                            If Items(dstBag).ItemInfo.SubClass <> ItemInfo.SubClass Then
                                'Inserting Ammo in not proper AmmoType bag
                                Return InventoryChangeFailure.EQUIP_ERR_ITEM_DOESNT_GO_INTO_BAG
                            Else
                                'Inserting Ammo in proper AmmoType bag
                                Return InventoryChangeFailure.EQUIP_ERR_OK
                            End If
                        Else
                            Return InventoryChangeFailure.EQUIP_ERR_ONLY_AMMO_CAN_GO_HERE
                        End If
                    Else
                        Return InventoryChangeFailure.EQUIP_ERR_OK
                    End If

                End If
            Catch err As Exception
                Log.WriteLine(LogType.FAILED, "[{0}:{1}] Unable to equip item. {2}{3}", Client.IP, Client.Port, vbNewLine, err.ToString)
                Return InventoryChangeFailure.EQUIP_ERR_CANT_DO_RIGHT_NOW
            End Try
        End Function
        Public Function ItemSTACK(ByVal srcBag As Byte, ByVal srcSlot As Byte, ByVal dstBag As Byte, ByVal dstSlot As Byte) As Boolean
            Dim srcItem As ItemObject = Nothing
            Dim dstItem As ItemObject = Nothing
            If srcBag <> 0 Then
                srcItem = Items(srcBag).Items(srcSlot)
            Else
                srcItem = Items(srcSlot)
            End If
            If dstBag <> 0 Then
                dstItem = Items(dstBag).Items(dstSlot)
            Else
                dstItem = Items(dstSlot)
            End If


            'DONE: If already full, just swap
            If srcItem.StackCount = dstItem.ItemInfo.Stackable Or dstItem.StackCount = dstItem.ItemInfo.Stackable Then Return False

            'DONE: Same item types -> stack if not full, else just swap !Nooo, else fill
            If (srcItem.ItemEntry = dstItem.ItemEntry) AndAlso (dstItem.StackCount + srcItem.StackCount) <= dstItem.ItemInfo.Stackable Then
                dstItem.StackCount += srcItem.StackCount
                ItemREMOVE(srcBag, srcSlot, True, True)

                SendItemUpdate(dstItem)
                dstItem.Save(False)
                Return True
            End If
            'DONE: Same item types, but bigger than max count -> fill destination
            If (srcItem.ItemEntry = dstItem.ItemEntry) Then
                srcItem.StackCount -= dstItem.ItemInfo.Stackable - dstItem.StackCount
                dstItem.StackCount = dstItem.ItemInfo.Stackable

                SendItemUpdate(dstItem)
                SendItemUpdate(srcItem)
                srcItem.Save(False)
                dstItem.Save(False)
                Return True
            End If
            Return False
        End Function
        Public Sub ItemSPLIT(ByVal srcBag As Byte, ByVal srcSlot As Byte, ByVal dstBag As Byte, ByVal dstSlot As Byte, ByVal Count As Integer)
            Dim srcItem As ItemObject = Nothing
            Dim dstItem As ItemObject = Nothing

            'DONE: Get source item
            If srcBag = 0 Then
                If Not Client.Character.Items.ContainsKey(srcSlot) Then
                    Dim EQUIP_ERR_ITEM_NOT_FOUND As New PacketClass(OPCODES.SMSG_INVENTORY_CHANGE_FAILURE)
                    EQUIP_ERR_ITEM_NOT_FOUND.AddInt8(InventoryChangeFailure.EQUIP_ERR_ITEM_NOT_FOUND)
                    EQUIP_ERR_ITEM_NOT_FOUND.AddUInt64(0)
                    EQUIP_ERR_ITEM_NOT_FOUND.AddUInt64(0)
                    EQUIP_ERR_ITEM_NOT_FOUND.AddInt8(0)
                    Client.Send(EQUIP_ERR_ITEM_NOT_FOUND)
                    EQUIP_ERR_ITEM_NOT_FOUND.Dispose()
                    Exit Sub
                End If
                srcItem = Items(srcSlot)
            Else
                If Not Client.Character.Items(srcBag).Items.ContainsKey(srcSlot) Then
                    Dim EQUIP_ERR_ITEM_NOT_FOUND As New PacketClass(OPCODES.SMSG_INVENTORY_CHANGE_FAILURE)
                    EQUIP_ERR_ITEM_NOT_FOUND.AddInt8(InventoryChangeFailure.EQUIP_ERR_ITEM_NOT_FOUND)
                    EQUIP_ERR_ITEM_NOT_FOUND.AddUInt64(0)
                    EQUIP_ERR_ITEM_NOT_FOUND.AddUInt64(0)
                    EQUIP_ERR_ITEM_NOT_FOUND.AddInt8(0)
                    Client.Send(EQUIP_ERR_ITEM_NOT_FOUND)
                    EQUIP_ERR_ITEM_NOT_FOUND.Dispose()
                    Exit Sub
                End If
                srcItem = Items(srcBag).Items(srcSlot)
            End If

            'DONE: Get destination item
            If dstBag = 0 Then
                If Items.ContainsKey(dstSlot) Then dstItem = Items(dstSlot)
            Else
                If Items(dstBag).Items.ContainsKey(dstSlot) Then dstItem = Items(dstBag).Items(dstSlot)
            End If

            If dstSlot = 255 Then
                Dim notHandledYet As New PacketClass(OPCODES.SMSG_INVENTORY_CHANGE_FAILURE)
                notHandledYet.AddInt8(InventoryChangeFailure.EQUIP_ERR_COULDNT_SPLIT_ITEMS)
                notHandledYet.AddUInt64(srcItem.GUID)
                notHandledYet.AddUInt64(dstItem.GUID)
                notHandledYet.AddInt8(0)
                Client.Send(notHandledYet)
                notHandledYet.Dispose()
                Exit Sub
            End If

            If Count = srcItem.StackCount Then
                ItemSWAP(srcBag, srcSlot, dstBag, dstSlot)
                Exit Sub
            End If

            If Count > srcItem.StackCount Then
                Dim EQUIP_ERR_TRIED_TO_SPLIT_MORE_THAN_COUNT As New PacketClass(OPCODES.SMSG_INVENTORY_CHANGE_FAILURE)
                EQUIP_ERR_TRIED_TO_SPLIT_MORE_THAN_COUNT.AddInt8(InventoryChangeFailure.EQUIP_ERR_TRIED_TO_SPLIT_MORE_THAN_COUNT)
                EQUIP_ERR_TRIED_TO_SPLIT_MORE_THAN_COUNT.AddUInt64(srcItem.GUID)
                EQUIP_ERR_TRIED_TO_SPLIT_MORE_THAN_COUNT.AddUInt64(0)
                EQUIP_ERR_TRIED_TO_SPLIT_MORE_THAN_COUNT.AddInt8(0)
                Client.Send(EQUIP_ERR_TRIED_TO_SPLIT_MORE_THAN_COUNT)
                EQUIP_ERR_TRIED_TO_SPLIT_MORE_THAN_COUNT.Dispose()
                Exit Sub
            End If

            'DONE: Create new item if needed
            If dstItem Is Nothing Then
                srcItem.StackCount -= Count

                Dim tmpItem As New ItemObject(srcItem.ItemEntry, GUID)
                tmpItem.StackCount = Count
                dstItem = tmpItem
                tmpItem.Save()
                ItemSETSLOT(tmpItem, dstBag, dstSlot)

                Dim SMSG_UPDATE_OBJECT As New UpdatePacketClass
                Dim tmpUpdate As New UpdateClass(FIELD_MASK_SIZE_ITEM)
                tmpItem.FillAllUpdateFlags(tmpUpdate)
                tmpUpdate.AddToPacket(CType(SMSG_UPDATE_OBJECT, UpdatePacketClass), ObjectUpdateType.UPDATETYPE_CREATE_OBJECT, tmpItem, 0)
                Client.Send(CType(SMSG_UPDATE_OBJECT, UpdatePacketClass))
                SMSG_UPDATE_OBJECT.Dispose()
                tmpUpdate.Dispose()

                If srcBag <> 0 Then
                    SendItemUpdate(Items(srcBag))
                    Items(srcBag).Save(False)
                End If
                If dstBag <> 0 Then
                    SendItemUpdate(Items(dstBag))
                    Items(dstBag).Save(False)
                End If
                SendItemUpdate(srcItem)
                SendItemUpdate(dstItem)
                srcItem.Save(False)
                dstItem.Save(False)
                Exit Sub
            End If

            'DONE: Split
            If srcItem.ItemEntry = dstItem.ItemEntry Then
                If (dstItem.StackCount + Count) <= dstItem.ItemInfo.Stackable Then
                    srcItem.StackCount -= Count
                    dstItem.StackCount += Count

                    SendItemUpdate(srcItem)
                    SendItemUpdate(dstItem)
                    srcItem.Save(False)
                    dstItem.Save(False)

                    Dim EQUIP_ERR_OK As New PacketClass(OPCODES.SMSG_INVENTORY_CHANGE_FAILURE)
                    EQUIP_ERR_OK.AddInt8(InventoryChangeFailure.EQUIP_ERR_OK)
                    EQUIP_ERR_OK.AddUInt64(srcItem.GUID)
                    EQUIP_ERR_OK.AddUInt64(dstItem.GUID)
                    EQUIP_ERR_OK.AddInt8(0)
                    Client.Send(EQUIP_ERR_OK)
                    EQUIP_ERR_OK.Dispose()
                    Exit Sub
                End If
            End If


            Dim response As New PacketClass(OPCODES.SMSG_INVENTORY_CHANGE_FAILURE)
            response.AddInt8(InventoryChangeFailure.EQUIP_ERR_COULDNT_SPLIT_ITEMS)
            response.AddUInt64(srcItem.GUID)
            response.AddUInt64(dstItem.GUID)
            response.AddInt8(0)
            Client.Send(response)
            response.Dispose()
        End Sub
        Public Sub ItemSWAP(ByVal srcBag As Byte, ByVal srcSlot As Byte, ByVal dstBag As Byte, ByVal dstSlot As Byte)
            'DONE: Disable when dead, attackTarget<>0
            If DEAD Then
                SendInventoryChangeFailure(Me, InventoryChangeFailure.EQUIP_ERR_YOU_ARE_DEAD, ItemGetGUID(srcBag, srcSlot), ItemGetGUID(dstBag, dstSlot))
                Exit Sub
            End If

            Dim errCode As Byte = InventoryChangeFailure.EQUIP_ERR_ITEMS_CANT_BE_SWAPPED

            'Disable moving the bag into same bag
            If (srcBag = 0 AndAlso srcSlot = dstBag AndAlso dstBag > 0) OrElse (dstBag = 0 AndAlso dstSlot = srcBag AndAlso srcBag > 0) Then
                SendInventoryChangeFailure(Me, errCode, Items(srcSlot).GUID, 0)
                Exit Sub
            End If


            Try
                If srcBag > 0 AndAlso dstBag > 0 Then
                    'DONE: Betwen Bags Moving
                    If Not Items(srcBag).Items.ContainsKey(srcSlot) Then
                        errCode = InventoryChangeFailure.EQUIP_ERR_SLOT_IS_EMPTY
                    Else
                        errCode = ItemCANEQUIP(Items(srcBag).Items(srcSlot), dstBag, dstSlot)
                        If errCode = InventoryChangeFailure.EQUIP_ERR_OK AndAlso Items(dstBag).Items.ContainsKey(dstSlot) Then
                            errCode = ItemCANEQUIP(Items(dstBag).Items(dstSlot), srcBag, srcSlot)
                        End If

                        'DONE: Moving item
                        If errCode = InventoryChangeFailure.EQUIP_ERR_OK Then

                            If Not Items(dstBag).Items.ContainsKey(dstSlot) Then
                                If Not Items(srcBag).Items.ContainsKey(srcSlot) Then
                                    Items(dstBag).Items.Remove(dstSlot)
                                    Items(srcBag).Items.Remove(srcSlot)
                                Else
                                    Items(dstBag).Items(dstSlot) = Items(srcBag).Items(srcSlot)
                                    Items(srcBag).Items.Remove(srcSlot)
                                End If
                            Else
                                If Not Items(srcBag).Items.ContainsKey(srcSlot) Then
                                    Items(srcBag).Items(srcSlot) = Items(dstBag).Items(dstSlot)
                                    Items(dstBag).Items.Remove(dstSlot)
                                Else
                                    If ItemSTACK(srcBag, srcSlot, dstBag, dstSlot) Then Exit Sub
                                    Dim tmp As ItemObject = Items(dstBag).Items(dstSlot)
                                    Items(dstBag).Items(dstSlot) = Items(srcBag).Items(srcSlot)
                                    Items(srcBag).Items(srcSlot) = tmp
                                    tmp = Nothing
                                End If
                            End If


                            SendItemUpdate(Items(srcBag))
                            If dstBag <> srcBag Then
                                SendItemUpdate(Items(dstBag))
                            End If
                            Database.Update(String.Format("UPDATE characters_inventory SET item_slot = {0}, item_bag = {1} WHERE item_guid = {2};", dstSlot, Items(dstBag).GUID, Items(dstBag).Items(dstSlot).GUID - GUID_ITEM))
                            If Items(srcBag).Items.ContainsKey(srcSlot) Then Database.Update(String.Format("UPDATE characters_inventory SET item_slot = {0}, item_bag = {1} WHERE item_guid = {2};", srcSlot, Items(srcBag).GUID, Items(srcBag).Items(srcSlot).GUID - GUID_ITEM))
                        End If
                    End If



                ElseIf srcBag > 0 Then
                    'DONE: from Bag to Inventory
                    If Not Items(srcBag).Items.ContainsKey(srcSlot) Then
                        errCode = InventoryChangeFailure.EQUIP_ERR_SLOT_IS_EMPTY
                    Else
                        errCode = ItemCANEQUIP(Items(srcBag).Items(srcSlot), dstBag, dstSlot)
                        If errCode = InventoryChangeFailure.EQUIP_ERR_OK AndAlso Items.ContainsKey(dstSlot) Then
                            errCode = ItemCANEQUIP(Items(dstSlot), srcBag, srcSlot)
                        End If

                        'DONE: Moving item
                        If errCode = InventoryChangeFailure.EQUIP_ERR_OK Then

                            If Not Items.ContainsKey(dstSlot) Then
                                If Not Items(srcBag).Items.ContainsKey(srcSlot) Then
                                    Items.Remove(dstSlot)
                                    Items(srcBag).Items.Remove(srcSlot)
                                Else
                                    Items(dstSlot) = Items(srcBag).Items(srcSlot)
                                    Items(srcBag).Items.Remove(srcSlot)
                                    If dstSlot < EQUIPMENT_SLOT_END Then UpdateAddItemStats(Items(dstSlot), dstSlot)
                                End If
                            Else
                                If Not Items(srcBag).Items.ContainsKey(srcSlot) Then
                                    Items(srcBag).Items(srcSlot) = Items(dstSlot)
                                    Items.Remove(dstSlot)
                                    If dstSlot < EQUIPMENT_SLOT_END Then UpdateRemoveItemStats(Items(srcBag).Items(srcSlot), dstSlot)
                                Else
                                    If ItemSTACK(srcBag, srcSlot, dstBag, dstSlot) Then Exit Sub
                                    Dim tmp As ItemObject = Items(dstSlot)
                                    Items(dstSlot) = Items(srcBag).Items(srcSlot)
                                    Items(srcBag).Items(srcSlot) = tmp
                                    If dstSlot < EQUIPMENT_SLOT_END Then
                                        UpdateAddItemStats(Items(dstSlot), dstSlot)
                                        UpdateRemoveItemStats(Items(srcBag).Items(srcSlot), dstSlot)
                                    End If
                                    tmp = Nothing
                                End If
                            End If

                            SendItemAndCharacterUpdate(Items(srcBag))
                            Database.Update(String.Format("UPDATE characters_inventory SET item_slot = {0}, item_bag = {1} WHERE item_guid = {2};", dstSlot, Me.GUID, Items(dstSlot).GUID - GUID_ITEM))
                            If Items(srcBag).Items.ContainsKey(srcSlot) Then Database.Update(String.Format("UPDATE characters_inventory SET item_slot = {0}, item_bag = {1} WHERE item_guid = {2};", srcSlot, Items(srcBag).GUID, Items(srcBag).Items(srcSlot).GUID - GUID_ITEM))
                        End If
                    End If



                ElseIf dstBag > 0 Then
                    'DONE: from Inventory to Bag
                    If Not Items.ContainsKey(srcSlot) Then
                        errCode = InventoryChangeFailure.EQUIP_ERR_SLOT_IS_EMPTY
                    Else
                        errCode = ItemCANEQUIP(Items(srcSlot), dstBag, dstSlot)
                        If errCode = InventoryChangeFailure.EQUIP_ERR_OK AndAlso Items(dstBag).Items.ContainsKey(dstSlot) Then
                            errCode = ItemCANEQUIP(Items(dstBag).Items(dstSlot), srcBag, srcSlot)
                        End If

                        'DONE: Moving item
                        If errCode = InventoryChangeFailure.EQUIP_ERR_OK Then

                            If Not Items(dstBag).Items.ContainsKey(dstSlot) Then
                                If Not Items.ContainsKey(srcSlot) Then
                                    Items(dstBag).Items.Remove(dstSlot)
                                    Items.Remove(srcSlot)
                                Else
                                    Items(dstBag).Items(dstSlot) = Items(srcSlot)
                                    Items.Remove(srcSlot)
                                    If srcSlot < EQUIPMENT_SLOT_END Then UpdateRemoveItemStats(Items(dstBag).Items(dstSlot), srcSlot)
                                End If
                            Else
                                If Not Items.ContainsKey(srcSlot) Then
                                    Items(srcSlot) = Items(dstBag).Items(dstSlot)
                                    Items(dstBag).Items.Remove(dstSlot)
                                    If srcSlot < EQUIPMENT_SLOT_END Then UpdateAddItemStats(Items(srcSlot), srcSlot)
                                Else
                                    If ItemSTACK(srcBag, srcSlot, dstBag, dstSlot) Then Exit Sub
                                    Dim tmp As ItemObject = Items(dstBag).Items(dstSlot)
                                    Items(dstBag).Items(dstSlot) = Items(srcSlot)
                                    Items(srcSlot) = tmp
                                    If srcSlot < EQUIPMENT_SLOT_END Then
                                        UpdateAddItemStats(Items(srcSlot), srcSlot)
                                        UpdateRemoveItemStats(Items(dstBag).Items(dstSlot), srcSlot)
                                    End If
                                    tmp = Nothing
                                End If
                            End If

                            SendItemAndCharacterUpdate(Items(dstBag))
                            Database.Update(String.Format("UPDATE characters_inventory SET item_slot = {0}, item_bag = {1} WHERE item_guid = {2};", dstSlot, Items(dstBag).GUID, Items(dstBag).Items(dstSlot).GUID - GUID_ITEM))
                            If Items.ContainsKey(srcSlot) Then Database.Update(String.Format("UPDATE characters_inventory SET item_slot = {0}, item_bag = {1} WHERE item_guid = {2};", srcSlot, Me.GUID, Items(srcSlot).GUID - GUID_ITEM))
                        End If
                    End If






                Else
                    'DONE: Inventory Moving
                    If Not Items.ContainsKey(srcSlot) Then
                        errCode = InventoryChangeFailure.EQUIP_ERR_SLOT_IS_EMPTY
                    Else
                        errCode = ItemCANEQUIP(Items(srcSlot), dstBag, dstSlot)
                        If errCode = InventoryChangeFailure.EQUIP_ERR_OK AndAlso Items.ContainsKey(dstSlot) Then
                            errCode = ItemCANEQUIP(Items(dstSlot), srcBag, srcSlot)
                        End If

                        'DONE: Moving item
                        If errCode = InventoryChangeFailure.EQUIP_ERR_OK Then

                            If Not Items.ContainsKey(dstSlot) Then
                                If Not Items.ContainsKey(srcSlot) Then
                                    Items.Remove(dstSlot)
                                    Items.Remove(srcSlot)
                                Else
                                    Items(dstSlot) = Items(srcSlot)
                                    Items.Remove(srcSlot)
                                    If dstSlot < EQUIPMENT_SLOT_END Then UpdateAddItemStats(Items(dstSlot), dstSlot)
                                    If srcSlot < EQUIPMENT_SLOT_END Then UpdateRemoveItemStats(Items(dstSlot), srcSlot)
                                End If
                            Else
                                If Not Items.ContainsKey(srcSlot) Then
                                    Items(srcSlot) = Items(dstSlot)
                                    Items.Remove(dstSlot)
                                    If dstSlot < EQUIPMENT_SLOT_END Then UpdateRemoveItemStats(Items(srcSlot), dstSlot)
                                    If srcSlot < EQUIPMENT_SLOT_END Then UpdateAddItemStats(Items(srcSlot), srcSlot)
                                Else
                                    If ItemSTACK(srcBag, srcSlot, dstBag, dstSlot) Then Exit Sub
                                    Dim tmp As ItemObject = Items(dstSlot)
                                    Items(dstSlot) = Items(srcSlot)
                                    Items(srcSlot) = tmp
                                    If dstSlot < EQUIPMENT_SLOT_END Then
                                        UpdateAddItemStats(Items(dstSlot), dstSlot)
                                        UpdateRemoveItemStats(Items(srcSlot), dstSlot)
                                    End If
                                    If srcSlot < EQUIPMENT_SLOT_END Then
                                        UpdateAddItemStats(Items(srcSlot), srcSlot)
                                        UpdateRemoveItemStats(Items(dstSlot), srcSlot)
                                    End If
                                    tmp = Nothing
                                End If
                            End If

                            SendItemAndCharacterUpdate(Items(dstSlot))
                            Database.Update(String.Format("UPDATE characters_inventory SET item_slot = {0}, item_bag = {1} WHERE item_guid = {2};", dstSlot, Me.GUID, Items(dstSlot).GUID - GUID_ITEM))
                            If Items.ContainsKey(srcSlot) Then Database.Update(String.Format("UPDATE characters_inventory SET item_slot = {0}, item_bag = {1} WHERE item_guid = {2};", srcSlot, Me.GUID, Items(srcSlot).GUID - GUID_ITEM))
                        End If
                    End If
                End If


            Catch err As Exception
                Log.WriteLine(LogType.DEBUG, "[{0}:{1}] Unable to swap items. {2}{3}", Client.IP, Client.Port, vbNewLine, err.ToString)
            Finally

                If errCode <> InventoryChangeFailure.EQUIP_ERR_OK Then
                    SendInventoryChangeFailure(Me, errCode, ItemGetGUID(srcBag, srcSlot), ItemGetGUID(dstBag, dstSlot))
                End If
            End Try
        End Sub
        Public Function ItemGET(ByVal srcBag As Byte, ByVal srcSlot As Byte) As ItemObject
            If srcBag = 0 Then
                If Items.ContainsKey(srcSlot) Then Return Items(srcSlot)
            Else
                If Items.ContainsKey(srcBag) AndAlso Items(srcBag).Items.ContainsKey(srcSlot) Then Return Items(srcBag).Items(srcSlot)
            End If

            Return Nothing
        End Function
        Public Function ItemGetGUID(ByVal srcBag As Byte, ByVal srcSlot As Byte) As ULong
            If srcBag = 0 Then
                If Items.ContainsKey(srcSlot) Then Return Items(srcSlot).GUID
            Else
                If Items.ContainsKey(srcBag) AndAlso Items(srcBag).Items.ContainsKey(srcSlot) Then Return Items(srcBag).Items(srcSlot).GUID
            End If

            Return 0
        End Function
        Public Function ItemGetSLOTBAG(ByVal GUID As ULong, ByRef bag As Byte) As Byte

            For slot As Byte = EQUIPMENT_SLOT_START To INVENTORY_SLOT_ITEM_END - 1
                If Items.ContainsKey(slot) AndAlso Items(slot).GUID = GUID Then
                    bag = 0
                    Return slot
                End If
            Next
            For slot As Byte = KEYRING_SLOT_START To KEYRING_SLOT_END - 1
                If Items.ContainsKey(slot) AndAlso Items(slot).GUID = GUID Then
                    bag = 0
                    Return slot
                End If
            Next
            For bag = INVENTORY_SLOT_BAG_START To INVENTORY_SLOT_BAG_END - 1
                If Items.ContainsKey(bag) Then
                    For Each item As KeyValuePair(Of Byte, ItemObject) In Items(bag).Items
                        If item.Value.GUID = GUID Then Return item.Key
                    Next
                End If
            Next

        End Function
        Public Sub UpdateAddItemStats(ByRef Item As ItemObject, ByVal slot As Byte)
            'TODO: Fill in the other item stat types also
            Dim i As Byte
            For i = 0 To 9
                Select Case Item.ItemInfo.ItemBonusStatType(i)
                    Case ITEM_STAT_TYPE.HEALTH
                        Life.Bonus += Item.ItemInfo.ItemBonusStatValue(i)
                    Case ITEM_STAT_TYPE.AGILITY
                        Agility.Base += Item.ItemInfo.ItemBonusStatValue(i)
                        Agility.PositiveBonus += Item.ItemInfo.ItemBonusStatValue(i)
                        Resistances(DamageTypes.DMG_PHYSICAL).Base += Item.ItemInfo.ItemBonusStatValue(i) * 2
                    Case ITEM_STAT_TYPE.STRENGTH
                        Strength.Base += Item.ItemInfo.ItemBonusStatValue(i)
                        Strength.PositiveBonus += Item.ItemInfo.ItemBonusStatValue(i)
                    Case ITEM_STAT_TYPE.INTELLECT
                        Intellect.Base += Item.ItemInfo.ItemBonusStatValue(i)
                        Intellect.PositiveBonus += Item.ItemInfo.ItemBonusStatValue(i)
                        Life.Bonus += Item.ItemInfo.ItemBonusStatValue(i) * 15
                    Case ITEM_STAT_TYPE.SPIRIT
                        Spirit.Base += Item.ItemInfo.ItemBonusStatValue(i)
                        Spirit.PositiveBonus += Item.ItemInfo.ItemBonusStatValue(i)
                    Case ITEM_STAT_TYPE.STAMINA
                        Stamina.Base += Item.ItemInfo.ItemBonusStatValue(i)
                        Stamina.PositiveBonus += Item.ItemInfo.ItemBonusStatValue(i)
                        Life.Bonus += Item.ItemInfo.ItemBonusStatValue(i) * 10
                    Case ITEM_STAT_TYPE.BLOCK
                        combatBlockValue += Item.ItemInfo.ItemBonusStatValue(i)
                End Select
            Next

            For i = DamageTypes.DMG_PHYSICAL To DamageTypes.DMG_ARCANE
                Resistances(i).Base += CType(Item.ItemInfo, ItemInfo).Resistances(i)
            Next

            combatBlockValue += Item.ItemInfo.Block

            If Item.ItemInfo.Delay > 0 AndAlso slot <> EQUIPMENT_SLOT_OFFHAND Then
                AttackTimeBase(0) = Item.ItemInfo.Delay
            Else
                AttackTimeBase(1) = Item.ItemInfo.Delay
            End If

            'DONE: Add the equip spells to the character
            Dim j As Byte
            For i = 0 To 4
                If CType(Item.ItemInfo, ItemInfo).Spells(i).SpellID > 0 Then
                    If WS_Spells.SPELLs.ContainsKey(CType(Item.ItemInfo, ItemInfo).Spells(i).SpellID) Then
                        Dim SpellInfo As SpellInfo = WS_Spells.SPELLs(CType(Item.ItemInfo, ItemInfo).Spells(i).SpellID)
                        If CType(Item.ItemInfo, ItemInfo).Spells(i).SpellTrigger = ITEM_SPELLTRIGGER_TYPE.ON_EQUIP Then
                            For j = 0 To 2
                                If Not (SpellInfo.SpellEffects(j) Is Nothing) Then
                                    Select Case SpellInfo.SpellEffects(j).ID
                                        Case SpellEffects_Names.SPELL_EFFECT_APPLY_AURA
                                            AURAs(SpellInfo.SpellEffects(j).ApplyAuraIndex).Invoke(Me, Me, SpellInfo.SpellEffects(j), SpellInfo.ID, 1, AuraAction.AURA_ADD)
                                    End Select
                                End If
                            Next j
                        ElseIf CType(Item.ItemInfo, ItemInfo).Spells(i).SpellTrigger = ITEM_SPELLTRIGGER_TYPE.USE Then
                            'DONE: Show item cooldown when equipped
                            Dim cooldown As New PacketClass(OPCODES.SMSG_ITEM_COOLDOWN)
                            cooldown.AddUInt64(Item.GUID)
                            cooldown.AddInt32(Item.ItemInfo.Spells(i).SpellID)
                            Client.Send(cooldown)
                            cooldown.Dispose()
                        End If
                    End If
                End If
            Next i

            'DONE: Bind item to player
            If Item.ItemInfo.Bonding = ITEM_BONDING_TYPE.BIND_WHEN_EQUIPED AndAlso Not Item.IsSoulBound Then Item.SoulbindItem()

            'DONE: Cancel any spells that are being casted while equipping an item
            If spellCasted > 0 And spellCastState = spellCastState.SPELL_STATE_PREPARING Then
                spellCastState = spellCastState.SPELL_STATE_IDLE
                Dim tmpCaster As WS_Base.BaseObject
                tmpCaster = Me
                CType(WS_Spells.SPELLs(spellCasted), SpellInfo).SendInterrupted(0, tmpCaster)
                SendCastResult(SpellFailedReason.CAST_FAIL_INTERRUPTED, Client, spellCasted, 0)
                spellCasted = 0
            End If

            CalculateMinMaxDamage(Me, WeaponAttackType.BASE_ATTACK)
            CalculateMinMaxDamage(Me, WeaponAttackType.OFF_ATTACK)
            CalculateMinMaxDamage(Me, WeaponAttackType.RANGED_ATTACK)

            If ManaType = ManaTypes.TYPE_MANA OrElse Classe = Classes.CLASS_DRUID Then UpdateManaRegen()
            FillStatsUpdateFlags()
        End Sub
        Public Sub UpdateRemoveItemStats(ByRef Item As ItemObject, ByVal slot As Byte)
            'TODO: Add the other item stat types here also
            Dim i As Byte
            For i = 0 To 9
                Select Case CType(Item.ItemInfo, ItemInfo).ItemBonusStatType(i)
                    Case ITEM_STAT_TYPE.HEALTH
                        Life.Bonus -= Item.ItemInfo.ItemBonusStatValue(i)
                    Case ITEM_STAT_TYPE.AGILITY
                        Agility.Base -= Item.ItemInfo.ItemBonusStatValue(i)
                        Agility.PositiveBonus -= Item.ItemInfo.ItemBonusStatValue(i)
                        Resistances(DamageTypes.DMG_PHYSICAL).Base -= Item.ItemInfo.ItemBonusStatValue(i) * 2
                    Case ITEM_STAT_TYPE.STRENGTH
                        Strength.Base -= Item.ItemInfo.ItemBonusStatValue(i)
                        Strength.PositiveBonus -= Item.ItemInfo.ItemBonusStatValue(i)
                    Case ITEM_STAT_TYPE.INTELLECT
                        Intellect.Base -= Item.ItemInfo.ItemBonusStatValue(i)
                        Intellect.PositiveBonus -= Item.ItemInfo.ItemBonusStatValue(i)
                        Mana.Bonus -= Item.ItemInfo.ItemBonusStatValue(i) * 15
                    Case ITEM_STAT_TYPE.SPIRIT
                        Spirit.Base -= Item.ItemInfo.ItemBonusStatValue(i)
                        Spirit.PositiveBonus -= Item.ItemInfo.ItemBonusStatValue(i)
                    Case ITEM_STAT_TYPE.STAMINA
                        Stamina.Base -= Item.ItemInfo.ItemBonusStatValue(i)
                        Stamina.PositiveBonus -= Item.ItemInfo.ItemBonusStatValue(i)
                        Life.Bonus -= Item.ItemInfo.ItemBonusStatValue(i) * 10
                    Case ITEM_STAT_TYPE.BLOCK
                        combatBlockValue -= Item.ItemInfo.ItemBonusStatValue(i)
                End Select
            Next

            For i = DamageTypes.DMG_PHYSICAL To DamageTypes.DMG_ARCANE
                Resistances(i).Base -= CType(Item.ItemInfo, ItemInfo).Resistances(i)
            Next

            combatBlockValue -= Item.ItemInfo.Block

            If Item.ItemInfo.Delay > 0 AndAlso slot <> EQUIPMENT_SLOT_OFFHAND Then
                If Classe = Classes.CLASS_ROGUE Then AttackTimeBase(0) = 1900 Else AttackTimeBase(0) = 2000
            Else
                AttackTimeBase(1) = 2000
            End If

            'DONE: Remove the equip spells to the character
            Dim j As Byte
            For i = 0 To 4
                If CType(Item.ItemInfo, ItemInfo).Spells(i).SpellID > 0 Then
                    If WS_Spells.SPELLs.ContainsKey(CType(Item.ItemInfo, ItemInfo).Spells(i).SpellID) Then
                        Dim SpellInfo As SpellInfo = WS_Spells.SPELLs(CType(Item.ItemInfo, ItemInfo).Spells(i).SpellID)
                        If CType(Item.ItemInfo, ItemInfo).Spells(i).SpellTrigger = ITEM_SPELLTRIGGER_TYPE.ON_EQUIP Then
                            For j = 0 To 2
                                If Not (SpellInfo.SpellEffects(j) Is Nothing) Then
                                    Select Case SpellInfo.SpellEffects(j).ID
                                        Case SpellEffects_Names.SPELL_EFFECT_APPLY_AURA
                                            AURAs(SpellInfo.SpellEffects(j).ApplyAuraIndex).Invoke(Me, Me, SpellInfo.SpellEffects(j), SpellInfo.ID, 1, AuraAction.AURA_REMOVE)
                                    End Select
                                End If
                            Next j
                        End If
                    End If
                End If
            Next i

            CalculateMinMaxDamage(Me, WeaponAttackType.BASE_ATTACK)
            CalculateMinMaxDamage(Me, WeaponAttackType.OFF_ATTACK)
            CalculateMinMaxDamage(Me, WeaponAttackType.RANGED_ATTACK)

            If ManaType = ManaTypes.TYPE_MANA OrElse Classe = Classes.CLASS_DRUID Then UpdateManaRegen()
            FillStatsUpdateFlags()
        End Sub

        'Creature Interactions
        Public Sub SendGossip(ByVal cGUID As ULong, ByVal cTextID As Integer, Optional ByRef Menu As GossipMenu = Nothing, Optional ByRef qMenu As QuestMenu = Nothing)
            Dim SMSG_GOSSIP_MESSAGE As PacketClass = New PacketClass(OPCODES.SMSG_GOSSIP_MESSAGE)
            SMSG_GOSSIP_MESSAGE.AddUInt64(cGUID)
            SMSG_GOSSIP_MESSAGE.AddInt32(0) ' some new menu type in 2.4?
            SMSG_GOSSIP_MESSAGE.AddInt32(cTextID)
            If Menu Is Nothing Then
                SMSG_GOSSIP_MESSAGE.AddInt32(0)
            Else
                SMSG_GOSSIP_MESSAGE.AddInt32(Menu.Menus.Count)
                Dim index As Integer = 0
                While index < Menu.Menus.Count
                    SMSG_GOSSIP_MESSAGE.AddInt32(index)
                    SMSG_GOSSIP_MESSAGE.AddInt8(Menu.Icons(index))
                    SMSG_GOSSIP_MESSAGE.AddInt8(Menu.Coded(index))
                    SMSG_GOSSIP_MESSAGE.AddInt32(Menu.Costs(index))
                    SMSG_GOSSIP_MESSAGE.AddString(Menu.Menus(index))
                    SMSG_GOSSIP_MESSAGE.AddString(Menu.WarningMessages(index))
                    index += 1
                End While
            End If

            If qMenu Is Nothing Then
                SMSG_GOSSIP_MESSAGE.AddInt32(0)
            Else
                SMSG_GOSSIP_MESSAGE.AddInt32(qMenu.Names.Count)
                Dim index As Integer = 0
                While index < qMenu.Names.Count
                    SMSG_GOSSIP_MESSAGE.AddInt32(qMenu.IDs(index))
                    SMSG_GOSSIP_MESSAGE.AddInt32(qMenu.Icons(index))
                    SMSG_GOSSIP_MESSAGE.AddInt32(qMenu.Levels(index))
                    SMSG_GOSSIP_MESSAGE.AddString(qMenu.Names(index))
                    index += 1
                End While
            End If

            Client.Send(SMSG_GOSSIP_MESSAGE)
            SMSG_GOSSIP_MESSAGE.Dispose()
        End Sub
        Public Sub SendGossipComplete()
            Dim SMSG_GOSSIP_COMPLETE As PacketClass = New PacketClass(OPCODES.SMSG_GOSSIP_COMPLETE)
            Client.Send(SMSG_GOSSIP_COMPLETE)
            SMSG_GOSSIP_COMPLETE.Dispose()
        End Sub
        Public Sub SendPointOfInterest(ByVal x As Single, ByVal y As Single, ByVal icon As Integer, ByVal flags As Integer, ByVal data As Integer, ByVal name As String)
            Dim SMSG_GOSSIP_POI As PacketClass = New PacketClass(OPCODES.SMSG_GOSSIP_POI)
            SMSG_GOSSIP_POI.AddInt32(flags)
            SMSG_GOSSIP_POI.AddSingle(x)
            SMSG_GOSSIP_POI.AddSingle(y)
            SMSG_GOSSIP_POI.AddInt32(icon)
            SMSG_GOSSIP_POI.AddInt32(data)
            SMSG_GOSSIP_POI.AddString(name)
            Client.Send(SMSG_GOSSIP_POI)
            SMSG_GOSSIP_POI.Dispose()
        End Sub
        Public Sub BindPlayer(ByVal cGUID As ULong)
            'TODO: Send spell for bind 3286 bind magic

            bindpoint_positionX = positionX
            bindpoint_positionY = positionY
            bindpoint_positionZ = positionZ
            bindpoint_map_id = MapID
            bindpoint_zone_id = ZoneID
            SaveCharacter()

            Dim SMSG_BINDPOINTUPDATE As New PacketClass(OPCODES.SMSG_BINDPOINTUPDATE)
            SMSG_BINDPOINTUPDATE.AddSingle(bindpoint_positionX)
            SMSG_BINDPOINTUPDATE.AddSingle(bindpoint_positionY)
            SMSG_BINDPOINTUPDATE.AddSingle(bindpoint_positionZ)
            SMSG_BINDPOINTUPDATE.AddInt32(bindpoint_map_id)
            SMSG_BINDPOINTUPDATE.AddInt32(bindpoint_zone_id)
            Client.Send(SMSG_BINDPOINTUPDATE)
            SMSG_BINDPOINTUPDATE.Dispose()

            Log.WriteLine(LogType.DEBUG, "[{0}:{1}] SMSG_BINDPOINTUPDATE", Client.IP, Client.Port)

            Dim SMSG_PLAYERBOUND As New PacketClass(OPCODES.SMSG_PLAYERBOUND)
            SMSG_PLAYERBOUND.AddUInt64(cGUID)
            SMSG_PLAYERBOUND.AddInt32(bindpoint_zone_id)
            Client.Send(SMSG_PLAYERBOUND)
            SMSG_PLAYERBOUND.Dispose()

            Log.WriteLine(LogType.DEBUG, "[{0}:{1}] SMSG_PLAYERBOUND", Client.IP, Client.Port)
        End Sub

        'Character Movement
        Public Sub Teleport(ByVal posX As Single, ByVal posY As Single, ByVal posZ As Single, ByVal ori As Single, ByVal map As Integer)
            If MapID <> map Then
                Transfer(posX, posY, posZ, ori, map)
                Return
            End If

            Log.WriteLine(LogType.INFORMATION, "World: Player Teleport: X[{0}], Y[{1}], Z[{2}], O[{3}]", posX, posY, posZ, ori)

            Client.Character.movementFlags = 0

            Dim packet As New PacketClass(OPCODES.MSG_MOVE_TELEPORT_ACK)
            packet.AddPackGUID(GUID)
            packet.AddInt32(0)              'Counter
            packet.AddInt32(0)              'Movement flags
            packet.AddInt8(0)               '2.3.0
            packet.AddInt32(timeGetTime)
            packet.AddSingle(posX)
            packet.AddSingle(posY)
            packet.AddSingle(posZ)
            packet.AddSingle(ori)
            packet.AddInt32(0)
            Client.Send(packet)
            packet.Dispose()


            Client.Character.positionX = posX
            Client.Character.positionY = posY
            Client.Character.positionZ = posZ
            Client.Character.orientation = ori

            MoveCell(Me)
            UpdateCell(Me)

            Client.Character.ZoneID = AreaTable(GetAreaFlag(posX, posY, Client.Character.MapID)).Zone
        End Sub
        Public Sub Transfer(ByVal posX As Single, ByVal posY As Single, ByVal posZ As Single, ByVal ori As Single, ByVal map As Integer)
            Log.WriteLine(LogType.INFORMATION, "World: Player Transfer: X[{0}], Y[{1}], Z[{2}], O[{3}], MAP[{4}]", posX, posY, posZ, ori, map)


            Dim p As New PacketClass(OPCODES.SMSG_TRANSFER_PENDING)
            p.AddInt32(map)
            'p.AddInt32(TransportEntry)     'Only if on transport
            'p.AddInt32(TransportMap)       'Only if on transport
            Client.Send(p)
            p.Dispose()

            'Actions Here
            RemoveFromWorld(Me)

            Client.Character.movementFlags = 0
            Client.Character.positionX = posX
            Client.Character.positionY = posY
            Client.Character.positionZ = posZ
            Client.Character.orientation = ori
            Client.Character.MapID = map
            Client.Character.Save()

            'Do global transfer
            WS.ClientTransfer(Client.Index, posX, posY, posZ, ori, map)
        End Sub
        Public Sub ZoneCheck()
            Dim ZoneFlag As Integer = GetAreaFlag(positionX, positionY, MapID)
            If AreaTable.ContainsKey(ZoneFlag) = False Then
                Log.WriteLine(LogType.WARNING, "Zone Flag {0} does not exist.", ZoneFlag)
                Exit Sub
            End If
            If AreaTable(ZoneFlag).Zone = 0 Then
                ZoneID = AreaTable(ZoneFlag).ID
            Else
                ZoneID = AreaTable(ZoneFlag).Zone
            End If

            'DONE: Set rested in citys
            If AreaTable(ZoneFlag).IsCity Then
                If (cPlayerFlags And PlayerFlags.PLAYER_FLAG_RESTING) = 0 AndAlso Level < MAX_LEVEL Then
                    cPlayerFlags = cPlayerFlags Or PlayerFlags.PLAYER_FLAG_RESTING
                    SetUpdateFlag(EPlayerFields.PLAYER_FLAGS, cPlayerFlags)
                    SendCharacterUpdate()
                End If
            Else
                If (cPlayerFlags And PlayerFlags.PLAYER_FLAG_RESTING) Then
                    cPlayerFlags = cPlayerFlags And (Not PlayerFlags.PLAYER_FLAG_RESTING)
                    SetUpdateFlag(EPlayerFields.PLAYER_FLAGS, cPlayerFlags)
                    SendCharacterUpdate()
                End If
            End If
            'DONE: Sanctuary turns players into blue and not attackable
            If AreaTable(ZoneFlag).IsSanctuary Then
                If (cUnitFlags And UnitFlags.UNIT_FLAG_NON_PVP_PLAYER) < UnitFlags.UNIT_FLAG_NON_PVP_PLAYER Then
                    cUnitFlags = cUnitFlags Or UnitFlags.UNIT_FLAG_NON_PVP_PLAYER
                    SetUpdateFlag(EUnitFields.UNIT_FIELD_FLAGS, cUnitFlags)
                    SendCharacterUpdate()
                End If
            Else
                If (cUnitFlags And UnitFlags.UNIT_FLAG_NON_PVP_PLAYER) = UnitFlags.UNIT_FLAG_NON_PVP_PLAYER Then
                    cUnitFlags = cUnitFlags And (Not UnitFlags.UNIT_FLAG_NON_PVP_PLAYER)
                    cUnitFlags = cUnitFlags Or UnitFlags.UNIT_FLAG_ATTACKABLE 'To still be able to attack neutral
                    SetUpdateFlag(EUnitFields.UNIT_FIELD_FLAGS, cUnitFlags)
                    SendCharacterUpdate()
                End If

                'DONE: Activate Arena PvP (Can attack people from your own faction)
                If AreaTable(ZoneFlag).IsArena Then
                    If (cPlayerFlags And PlayerFlags.PLAYER_FLAG_DUEL) = 0 Then
                        cPlayerFlags = cPlayerFlags Or PlayerFlags.PLAYER_FLAG_DUEL
                        SetUpdateFlag(EPlayerFields.PLAYER_FLAGS, cPlayerFlags)
                        SendCharacterUpdate()
                    End If
                Else
                    'TODO: It takes 1 minute before the Arena flag wears off
                    If (cPlayerFlags And PlayerFlags.PLAYER_FLAG_DUEL) Then
                        cPlayerFlags = cPlayerFlags And (Not PlayerFlags.PLAYER_FLAG_DUEL)
                        SetUpdateFlag(EPlayerFields.PLAYER_FLAGS, cPlayerFlags)
                        SendCharacterUpdate()
                    End If
                    'DONE: Activate PvP
                    'TODO: Only for PvP realms
                    If AreaTable(ZoneFlag).IsMyLand(Me) = False Then
                        If (cUnitFlags And UnitFlags.UNIT_FLAG_PVP) = 0 Then
                            cUnitFlags = cUnitFlags Or UnitFlags.UNIT_FLAG_PVP
                            SetUpdateFlag(EUnitFields.UNIT_FIELD_FLAGS, cUnitFlags)
                            SendCharacterUpdate()
                        End If
                    Else
                        'TODO: It takes 5 minutes before the PVP flag wears off
                        If (cUnitFlags And UnitFlags.UNIT_FLAG_PVP) Then
                            cUnitFlags = cUnitFlags And (Not UnitFlags.UNIT_FLAG_PVP)
                            SetUpdateFlag(EUnitFields.UNIT_FIELD_FLAGS, cUnitFlags)
                            SendCharacterUpdate()
                        End If
                    End If
                End If
            End If
        End Sub
        Public Enum ChangeSpeedType As Byte
            RUN
            RUNBACK
            SWIM
            SWIMBACK
            TURNRATE
            FLY
            FLYBACK
        End Enum
        Public Sub ChangeSpeed(ByVal Type As ChangeSpeedType, ByVal NewSpeed As Single)
            Dim packet As PacketClass = Nothing
            Select Case Type
                Case ChangeSpeedType.RUN
                    RunSpeed = NewSpeed
                    packet = New PacketClass(OPCODES.MSG_MOVE_SET_RUN_SPEED)
                Case ChangeSpeedType.RUNBACK
                    RunBackSpeed = NewSpeed
                    packet = New PacketClass(OPCODES.MSG_MOVE_SET_RUN_BACK_SPEED)
                Case ChangeSpeedType.SWIM
                    SwimSpeed = NewSpeed
                    packet = New PacketClass(OPCODES.MSG_MOVE_SET_SWIM_SPEED)
                Case ChangeSpeedType.SWIMBACK
                    SwimSpeed = NewSpeed
                    packet = New PacketClass(OPCODES.MSG_MOVE_SET_SWIM_BACK_SPEED)
                Case ChangeSpeedType.TURNRATE
                    TurnRate = NewSpeed
                    packet = New PacketClass(OPCODES.MSG_MOVE_SET_TURN_RATE)
                Case ChangeSpeedType.FLY
                    FlySpeed = NewSpeed
                    packet = New PacketClass(OPCODES.MSG_MOVE_SET_FLIGHT_SPEED)
                Case ChangeSpeedType.FLYBACK
                    FlyBackSpeed = NewSpeed
                    packet = New PacketClass(OPCODES.MSG_MOVE_SET_FLIGHT_BACK_SPEED)
            End Select

            'DONE: Send to nearby players
            packet.AddPackGUID(Client.Character.GUID)
            packet.AddInt32(0) 'Movement flags
            packet.AddInt8(0) 'Unk
            packet.AddInt32(timeGetTime)
            packet.AddSingle(positionX)
            packet.AddSingle(positionY)
            packet.AddSingle(positionZ)
            packet.AddSingle(orientation)
            packet.AddInt32(0) 'Unk flag
            packet.AddSingle(NewSpeed)
            Client.SendMultiplyPackets(packet)
            Client.Character.SendToNearPlayers(packet)
            packet.Dispose()
        End Sub
        Public Sub ChangeSpeedForced(ByVal Type As ChangeSpeedType, ByVal NewSpeed As Single)
            antiHackSpeedChanged_ += 1
            Dim packet As PacketClass = Nothing

            Select Case Type
                Case ChangeSpeedType.RUN
                    packet = New PacketClass(OPCODES.SMSG_FORCE_RUN_SPEED_CHANGE)
                    RunSpeed = NewSpeed
                Case ChangeSpeedType.RUNBACK
                    packet = New PacketClass(OPCODES.SMSG_FORCE_RUN_BACK_SPEED_CHANGE)
                    RunBackSpeed = NewSpeed
                Case ChangeSpeedType.SWIM
                    packet = New PacketClass(OPCODES.SMSG_FORCE_SWIM_SPEED_CHANGE)
                    SwimSpeed = NewSpeed
                Case ChangeSpeedType.SWIMBACK
                    packet = New PacketClass(OPCODES.SMSG_FORCE_SWIM_BACK_SPEED_CHANGE)
                    SwimBackSpeed = NewSpeed
                Case ChangeSpeedType.TURNRATE
                    packet = New PacketClass(OPCODES.SMSG_FORCE_TURN_RATE_CHANGE)
                    TurnRate = NewSpeed
                Case ChangeSpeedType.FLY
                    packet = New PacketClass(OPCODES.SMSG_FORCE_FLIGHT_SPEED_CHANGE)
                    FlySpeed = NewSpeed
                Case ChangeSpeedType.FLYBACK
                    packet = New PacketClass(OPCODES.SMSG_FORCE_FLIGHT_BACK_SPEED_CHANGE)
                    FlyBackSpeed = NewSpeed
                Case Else
                    Exit Sub
            End Select
            packet.AddPackGUID(GUID)
            packet.AddInt32(antiHackSpeedChanged_)
            If Type = ChangeSpeedType.RUN Then packet.AddInt8(1)
            packet.AddSingle(NewSpeed)
            Client.SendMultiplyPackets(packet)
            Client.Character.SendToNearPlayers(packet)
            packet.Dispose()
        End Sub
        Public Sub SetWaterWalk()
            Dim SMSG_MOVE_WATER_WALK As New PacketClass(OPCODES.SMSG_MOVE_WATER_WALK)
            SMSG_MOVE_WATER_WALK.AddPackGUID(GUID)
            SMSG_MOVE_WATER_WALK.AddInt32(0)
            Client.Send(SMSG_MOVE_WATER_WALK)
            SMSG_MOVE_WATER_WALK.Dispose()
        End Sub
        Public Sub SetLandWalk()
            Dim SMSG_MOVE_LAND_WALK As New PacketClass(OPCODES.SMSG_MOVE_LAND_WALK)
            SMSG_MOVE_LAND_WALK.AddPackGUID(GUID)
            SMSG_MOVE_LAND_WALK.AddInt32(0)
            Client.Send(SMSG_MOVE_LAND_WALK)
            SMSG_MOVE_LAND_WALK.Dispose()
        End Sub
        Public Sub SetMoveRoot()
            Dim SMSG_FORCE_MOVE_ROOT As New PacketClass(OPCODES.SMSG_FORCE_MOVE_ROOT)
            SMSG_FORCE_MOVE_ROOT.AddPackGUID(GUID)
            SMSG_FORCE_MOVE_ROOT.AddInt32(0)
            Client.Send(SMSG_FORCE_MOVE_ROOT)
            SMSG_FORCE_MOVE_ROOT.Dispose()

            Log.WriteLine(LogType.DEBUG, "[{0}:{1}] SMSG_FORCE_MOVE_ROOT", Client.IP, Client.Port)
        End Sub
        Public Sub SetMoveUnroot()
            Dim SMSG_FORCE_MOVE_UNROOT As New PacketClass(OPCODES.SMSG_FORCE_MOVE_UNROOT)
            SMSG_FORCE_MOVE_UNROOT.AddPackGUID(GUID)
            SMSG_FORCE_MOVE_UNROOT.AddInt32(0)
            Client.Send(SMSG_FORCE_MOVE_UNROOT)
            SMSG_FORCE_MOVE_UNROOT.Dispose()

            Log.WriteLine(LogType.DEBUG, "[{0}:{1}] SMSG_FORCE_MOVE_UNROOT", Client.IP, Client.Port)
        End Sub
        Public Sub SetCanFly()
            Dim SMSG_MOVE_SET_CAN_FLY As New PacketClass(OPCODES.SMSG_MOVE_SET_CAN_FLY)
            SMSG_MOVE_SET_CAN_FLY.AddPackGUID(GUID)
            SMSG_MOVE_SET_CAN_FLY.AddInt32(2)
            Client.Send(SMSG_MOVE_SET_CAN_FLY)
            SMSG_MOVE_SET_CAN_FLY.Dispose()

            Log.WriteLine(LogType.DEBUG, "[{0}:{1}] SMSG_MOVE_SET_CAN_FLY", Client.IP, Client.Port)
        End Sub
        Public Sub UnSetCanFly()
            Dim SMSG_MOVE_UNSET_CAN_FLY As New PacketClass(OPCODES.SMSG_MOVE_UNSET_CAN_FLY)
            SMSG_MOVE_UNSET_CAN_FLY.AddPackGUID(GUID)
            SMSG_MOVE_UNSET_CAN_FLY.AddInt32(5)
            Client.Send(SMSG_MOVE_UNSET_CAN_FLY)
            SMSG_MOVE_UNSET_CAN_FLY.Dispose()

            Log.WriteLine(LogType.DEBUG, "[{0}:{1}] SMSG_MOVE_UNSET_CAN_FLY", Client.IP, Client.Port)
        End Sub
        Public Sub StartMirrorTimer(ByVal Type As MirrorTimer, ByVal MaxValue As Integer)
            Dim SMSG_START_MIRROR_TIMER As New PacketClass(OPCODES.SMSG_START_MIRROR_TIMER)
            SMSG_START_MIRROR_TIMER.AddInt32(Type)
            SMSG_START_MIRROR_TIMER.AddInt32(MaxValue)
            SMSG_START_MIRROR_TIMER.AddInt32(MaxValue)
            SMSG_START_MIRROR_TIMER.AddInt32(-1)
            SMSG_START_MIRROR_TIMER.AddInt32(0)
            SMSG_START_MIRROR_TIMER.AddInt8(0)

            Client.Send(SMSG_START_MIRROR_TIMER)
            SMSG_START_MIRROR_TIMER.Dispose()
        End Sub
        Public Sub ModifyMirrorTimer(ByVal Type As MirrorTimer, ByVal MaxValue As Integer, ByVal CurrentValue As Integer, ByVal Regen As Integer)
            'TYPE: 0 = fartigua 1 = breath 2 = fire
            Dim SMSG_START_MIRROR_TIMER As New PacketClass(OPCODES.SMSG_START_MIRROR_TIMER)
            SMSG_START_MIRROR_TIMER.AddInt32(Type)
            SMSG_START_MIRROR_TIMER.AddInt32(CurrentValue)
            SMSG_START_MIRROR_TIMER.AddInt32(MaxValue)
            SMSG_START_MIRROR_TIMER.AddInt32(Regen)
            SMSG_START_MIRROR_TIMER.AddInt32(0)
            SMSG_START_MIRROR_TIMER.AddInt8(0)

            Client.Send(SMSG_START_MIRROR_TIMER)
            SMSG_START_MIRROR_TIMER.Dispose()
        End Sub
        Public Sub StopMirrorTimer(ByVal Type As MirrorTimer)
            Dim SMSG_STOP_MIRROR_TIMER As New PacketClass(OPCODES.SMSG_STOP_MIRROR_TIMER)
            SMSG_STOP_MIRROR_TIMER.AddInt32(Type)

            Client.Send(SMSG_STOP_MIRROR_TIMER)
            SMSG_STOP_MIRROR_TIMER.Dispose()

            'If Type = 1 And (Not (underWaterTimer Is Nothing)) Then
            '    underWaterTimer.Dispose()
            '    underWaterTimer = Nothing
            'End If
        End Sub
        Public Sub HandleDrowning(ByVal state As Object)
            Try
                If positionZ > (GetWaterLevel(positionX, positionY, MapID) - 1.6) Then
                    underWaterTimer.DrowningValue += 2000
                    If underWaterTimer.DrowningValue > 70000 Then underWaterTimer.DrowningValue = 70000
                    ModifyMirrorTimer(MirrorTimer.DROWNING, 70000, underWaterTimer.DrowningValue, 2)
                Else
                    underWaterTimer.DrowningValue -= 1000
                    If underWaterTimer.DrowningValue < 0 Then
                        underWaterTimer.DrowningValue = 0
                        LogEnvironmentalDamage(EnviromentalDamage.DAMAGE_DROWNING, Fix(0.1F * Life.Maximum * underWaterTimer.DrowningDamage))
                        DealDamage(Fix(0.1F * Life.Maximum * underWaterTimer.DrowningDamage))
                        underWaterTimer.DrowningDamage = underWaterTimer.DrowningDamage * 2
                        If DEAD Then
                            underWaterTimer.Dispose()
                            underWaterTimer = Nothing
                        End If
                    End If
                    ModifyMirrorTimer(MirrorTimer.DROWNING, 70000, underWaterTimer.DrowningValue, -1)
                End If
            Catch e As Exception
                Log.WriteLine(LogType.FAILED, "Error at HandleDrowning():", e.ToString)
                underWaterTimer.Dispose()
                underWaterTimer = Nothing
            End Try
        End Sub

        'Reputation
        Public WatchedFactionIndex As Byte = &HFF
        Public Reputation(63) As TReputation
        Public Sub InitializeReputation(ByVal FactionID As Integer)
            If FactionInfo(FactionID).VisibleID > -1 Then
                Reputation(FactionInfo(FactionID).VisibleID).Value = 0
                If Reputation(FactionInfo(FactionID).VisibleID).Flags = 0 Then
                    Reputation(FactionInfo(FactionID).VisibleID).Flags = 1
                End If
            End If
        End Sub
        Public Function GetReaction(ByVal FactionID As Integer) As TReaction
            If FactionTemplatesInfo.ContainsKey(FactionID) = False OrElse FactionTemplatesInfo.ContainsKey(Faction) = False Then Return TReaction.NEUTRAL

            'DONE: Neutral to everyone
            If FactionTemplatesInfo(FactionID).enemyMask = 0 AndAlso FactionTemplatesInfo(FactionID).friendMask = 0 AndAlso _
            FactionTemplatesInfo(FactionID).enemyFaction1 = 0 And FactionTemplatesInfo(FactionID).enemyFaction2 = 0 AndAlso _
            FactionTemplatesInfo(FactionID).enemyFaction3 = 0 AndAlso FactionTemplatesInfo(FactionID).enemyFaction4 = 0 Then Return TReaction.NEUTRAL

            'DONE: Hostile to any players
            If FactionTemplatesInfo(FactionID).enemyMask And FactionMasks.FACTION_MASK_PLAYER Then Return TReaction.HOSTILE

            'DONE: Friendly to your faction
            If FactionTemplatesInfo(FactionID).friendFaction1 = Faction OrElse FactionTemplatesInfo(FactionID).friendFaction2 = Faction OrElse _
            FactionTemplatesInfo(FactionID).friendFaction3 = Faction OrElse FactionTemplatesInfo(FactionID).friendFaction4 = Faction Then Return TReaction.FIGHT_SUPPORT

            'DONE: Friendly to your faction mask
            If FactionTemplatesInfo(FactionID).friendMask And FactionTemplatesInfo(Faction).ourMask Then Return TReaction.FIGHT_SUPPORT

            'DONE: Hostile to your faction
            If FactionTemplatesInfo(FactionID).enemyFaction1 = Faction OrElse FactionTemplatesInfo(FactionID).enemyFaction2 = Faction OrElse _
            FactionTemplatesInfo(FactionID).enemyFaction3 = Faction OrElse FactionTemplatesInfo(FactionID).enemyFaction4 = Faction Then Return TReaction.HOSTILE

            'DONE: Hostile to your faction mask
            If FactionTemplatesInfo(FactionID).enemyMask And FactionTemplatesInfo(Faction).ourMask Then Return TReaction.HOSTILE

            'DONE: Hostile by reputation
            Dim Rank As ReputationRank = GetReputation(FactionTemplatesInfo(FactionID).FactionID)
            If Rank >= ReputationRank.Hostile Then
                Return TReaction.HOSTILE
            ElseIf Rank <= ReputationRank.Revered Then
                Return TReaction.FIGHT_SUPPORT
            ElseIf Rank <= ReputationRank.Friendly Then
                Return TReaction.FRIENDLY
            Else
                Return TReaction.NEUTRAL
            End If
        End Function
        Public Function GetReputation(ByVal FactionID As Integer) As ReputationRank
            If Not FactionInfo.ContainsKey(FactionID) Then Return ReputationRank.Neutral
            If FactionInfo(FactionID).VisibleID = -1 Then Return ReputationRank.Neutral

            Dim points As Integer
            If HaveFlag(FactionInfo(FactionID).flags(0), Race - 1) Then
                points = FactionInfo(FactionID).rep_stats(0)
            ElseIf HaveFlag(FactionInfo(FactionID).flags(1), Race - 1) Then
                points = FactionInfo(FactionID).rep_stats(1)
            ElseIf HaveFlag(FactionInfo(FactionID).flags(2), Race - 1) Then
                points = FactionInfo(FactionID).rep_stats(2)
            ElseIf HaveFlag(FactionInfo(FactionID).flags(3), Race - 1) Then
                points = FactionInfo(FactionID).rep_stats(3)
            Else
                points = 0
            End If

            If Reputation(FactionInfo(FactionID).VisibleID).Flags > 0 Then
                points = points + Reputation(FactionInfo(FactionID).VisibleID).Value
            End If

            Select Case points
                Case Is > ReputationPoints.Revered
                    Return ReputationRank.Exalted
                Case Is > ReputationPoints.Honored
                    Return ReputationRank.Revered
                Case Is > ReputationPoints.Friendly
                    Return ReputationRank.Honored
                Case Is > ReputationPoints.Neutral
                    Return ReputationRank.Friendly
                Case Is > ReputationPoints.Unfriendly
                    Return ReputationRank.Neutral
                Case Is > ReputationPoints.Hostile
                    Return ReputationRank.Unfriendly
                Case Is > ReputationPoints.Hated
                    Return ReputationRank.Hostile
                Case Else
                    Return ReputationRank.Hated
            End Select
        End Function
        Public Sub SetReputation(ByVal FactionID As Integer, ByVal Value As Integer)
            If FactionInfo(FactionID).VisibleID > -1 Then
                Reputation(FactionInfo(FactionID).VisibleID).Value = Reputation(FactionInfo(FactionID).VisibleID).Value + Value
            End If

            If Not Client Is Nothing Then
                Dim packet As New PacketClass(OPCODES.SMSG_SET_FACTION_STANDING)
                packet.AddInt32(Reputation(FactionInfo(FactionID).VisibleID).Flags)
                packet.AddInt32(FactionInfo(FactionID).VisibleID)
                packet.AddInt32(Reputation(FactionInfo(FactionID).VisibleID).Value)
                Client.Send(packet)
                packet.Dispose()
            End If
        End Sub
        Public Function GetDiscountMod(ByVal FactionID As Integer) As Single
            Dim Rank As ReputationRank = GetReputation(FactionID)
            If Rank <= ReputationRank.Neutral Then Return 1
            Return (1 - 0.05 * (Rank - ReputationRank.Neutral))
        End Function

        'Death
        Public Overrides Sub Die(ByRef Attacker As BaseUnit)
            'DONE: Check if player is in duel
            If IsInDuel Then
                DuelComplete(DuelPartner, Me)
                Exit Sub
            End If

            'DONE: Remove all spells when you die
            For i As Integer = 0 To MAX_AURA_EFFECTs_VISIBLE - 1
                If Not ActiveSpells(i) Is Nothing Then RemoveAura(i, ActiveSpells(i).SpellCaster)
            Next

            'DONE: Save as DEAD (GHOST)!
            DEAD = True
            repopTimer = New TRepopTimer(Me)
            cDynamicFlags = DynamicFlags.UNIT_DYNFLAG_DEAD
            cUnitFlags = 8          'player death animation, also can be used with cDynamicFlags

            SetUpdateFlag(EUnitFields.UNIT_FIELD_HEALTH, 0)
            SetUpdateFlag(EUnitFields.UNIT_FIELD_FLAGS, cUnitFlags)
            SetUpdateFlag(EUnitFields.UNIT_DYNAMIC_FLAGS, cDynamicFlags)
            SendCharacterUpdate(True)

            'DONE: 10% Durability lost, and only if the killer is a creature or you died by enviromental damage
            If Attacker Is Nothing OrElse TypeOf Attacker Is CreatureObject Then
                Dim i As Byte
                For i = 0 To EQUIPMENT_SLOT_END - 1
                    If Items.ContainsKey(i) Then Items(i).ModifyDurability(0.1F, Client)
                Next
                Dim SMSG_DURABILITY_DAMAGE_DEATH As New PacketClass(OPCODES.SMSG_DURABILITY_DAMAGE_DEATH)
                Client.Send(SMSG_DURABILITY_DAMAGE_DEATH)
                SMSG_DURABILITY_DAMAGE_DEATH.Dispose()
            End If
        End Sub
        Public Sub SendDeathReleaseLoc(ByVal x As Single, ByVal y As Single, ByVal z As Single, ByVal MapID As Integer)
            'Show spirit healer position on minimap
            Dim p As New PacketClass(OPCODES.SMSG_DEATH_RELEASE_LOC)
            p.AddInt32(MapID)
            p.AddSingle(x)
            p.AddSingle(y)
            p.AddSingle(z)
            Client.Send(p)
            p.Dispose()
        End Sub

        'Combat
        Public Overrides Sub DealDamageMagical(ByRef Damage As Integer, ByVal DamageType As DamageTypes, Optional ByRef Attacker As BaseUnit = Nothing)
            'DONE: Check for dead
            If DEAD Then Exit Sub

            Select Case DamageType
                Case DamageTypes.DMG_PHYSICAL
                    Me.DealDamage(Damage, Attacker)
                    Return
                Case Else
                    'TODO: Magical resists here
            End Select

            If Life.Current = 0 Then
                Me.Die(Attacker)
            Else
                SetUpdateFlag(EUnitFields.UNIT_FIELD_HEALTH, CType(Life.Current, Integer))
                SendCharacterUpdate(True)
            End If
        End Sub
        Public Overrides Sub DealDamage(ByVal Damage As Integer, Optional ByRef Attacker As BaseUnit = Nothing)
            'DONE: Check for dead
            If DEAD Then Exit Sub

            'DONE: Break some spells
            RemoveAurasByInterruptFlag(SpellAuraInterruptFlags.AURA_INTERRUPT_FLAG_DIRECT_DAMAGE)

            If TypeOf Attacker Is CharacterObject Then
                SetPlayerInCombat(Me)
            End If

            Life.Current -= Damage

            If Life.Current = 0 Then
                Me.Die(Attacker)
            Else
                SetUpdateFlag(EUnitFields.UNIT_FIELD_HEALTH, CType(Life.Current, Integer))
                SendCharacterUpdate()
            End If
        End Sub
        Public Overrides Sub Heal(ByVal Damage As Integer, Optional ByRef Attacker As BaseUnit = Nothing)
            If DEAD Then Exit Sub

            Life.Current += Damage
            SetUpdateFlag(EUnitFields.UNIT_FIELD_HEALTH, CType(Life.Current, Integer))
            SendCharacterUpdate()
        End Sub
        Public Overrides Sub Energize(ByVal Damage As Integer, ByVal Power As ManaTypes, Optional ByRef Attacker As BaseUnit = Nothing)
            If DEAD Then Exit Sub
            If ManaType <> ManaTypes.TYPE_MANA Then Exit Sub
            Select Case Power
                Case ManaTypes.TYPE_MANA
                    If Mana.Current = Mana.Maximum Then Exit Sub
                    Mana.Current += Damage
                    SetUpdateFlag(EUnitFields.UNIT_FIELD_POWER1, CType(Mana.Current, Integer))
                Case ManaTypes.TYPE_RAGE
                    If Rage.Current = Rage.Maximum Then Exit Sub
                    Rage.Current += Damage
                    SetUpdateFlag(EUnitFields.UNIT_FIELD_POWER2, CType(Rage.Current, Integer))
                Case ManaTypes.TYPE_ENERGY
                    If Energy.Current = Energy.Maximum Then Exit Sub
                    Energy.Current += Damage
                    SetUpdateFlag(EUnitFields.UNIT_FIELD_POWER4, CType(Energy.Current, Integer))
                Case Else
                    Exit Sub
            End Select
            SendCharacterUpdate()
        End Sub

        'System
        Public Sub Logout(Optional ByVal StateObj As Object = Nothing)
            Try
                CType(LogoutTimer, Threading.Timer).Dispose()
                LogoutTimer = Nothing
            Catch
            End Try

            'DONE: Spawn corpse and remove repop timer if present
            If Not (repopTimer Is Nothing) Then
                repopTimer.Dispose()
                repopTimer = Nothing
                'DONE: Spawn Corpse
                Dim myCorpse As New CorpseObject(Me)
                myCorpse.Save()
                myCorpse.AddToWorld()
            End If

            'DONE: Leave group
            If IsInGroup Then
                If Group.LocalMembers.Count <= 2 Then
                    Group.Dispose()
                    Group = Nothing
                End If
            End If

            'DONE: Disconnect the client
            Dim SMSG_LOGOUT_COMPLETE As New PacketClass(OPCODES.SMSG_LOGOUT_COMPLETE)
            Client.Send(SMSG_LOGOUT_COMPLETE)
            SMSG_LOGOUT_COMPLETE.Dispose()
            Log.WriteLine(LogType.DEBUG, "[{0}:{1}] SMSG_LOGOUT_COMPLETE", Client.IP, Client.Port)

            Client.Character = Nothing
            Log.WriteLine(LogType.USER, "Character {0} logged off.", Name)

            Client.Delete()
            Client = Nothing

            Me.Dispose()
        End Sub
        Public Sub Login()
            'Loading map cell if not loaded
            GetMapTile(positionX, positionY, CellX, CellY)
            If Maps(MapID).Tiles(CellX, CellY) Is Nothing Then MAP_Load(CellX, CellY, MapID)

            'DONE: SMSG_BINDPOINTUPDATE
            SendBindPointUpdate(Client, Me)

            'TODO: SMSG_SET_REST_START
            Send_SMSG_SET_REST_START(Client, Me)

            'TODO: SMSG_SET_FLAT_SPELL_MODIFIER
            SendFlatSpellMod(Client, Me)

            'DONE: SMSG_TUTORIAL_FLAGS
            SendTutorialFlags(Client, Me)

            'DONE: SMSG_SET_PROFICIENCY
            Dim ProficiencyFlags As Integer = 0
            If Spells.Contains(9125) Then ProficiencyFlags += (1 << ITEM_SUBCLASS.ITEM_SUBCLASS_MISC) 'Here using spell "Generic"
            If Spells.Contains(9078) Then ProficiencyFlags += (1 << ITEM_SUBCLASS.ITEM_SUBCLASS_CLOTH)
            If Spells.Contains(9077) Then ProficiencyFlags += (1 << ITEM_SUBCLASS.ITEM_SUBCLASS_LEATHER)
            If Spells.Contains(8737) Then ProficiencyFlags += (1 << ITEM_SUBCLASS.ITEM_SUBCLASS_MAIL)
            If Spells.Contains(750) Then ProficiencyFlags += (1 << ITEM_SUBCLASS.ITEM_SUBCLASS_PLATE)
            If Spells.Contains(9124) Then ProficiencyFlags += (1 << ITEM_SUBCLASS.ITEM_SUBCLASS_BUCKLER)
            If Spells.Contains(9116) Then ProficiencyFlags += (1 << ITEM_SUBCLASS.ITEM_SUBCLASS_SHIELD)
            If Spells.Contains(27762) Then ProficiencyFlags += (1 << ITEM_SUBCLASS.ITEM_SUBCLASS_LIBRAM)
            If Spells.Contains(27763) Then ProficiencyFlags += (1 << ITEM_SUBCLASS.ITEM_SUBCLASS_TOTEM)
            If Spells.Contains(27764) Then ProficiencyFlags += (1 << ITEM_SUBCLASS.ITEM_SUBCLASS_IDOL)
            SendProficiency(Client, ITEM_CLASS.ITEM_CLASS_ARMOR, ProficiencyFlags)

            ProficiencyFlags = 0
            If Spells.Contains(196) Then ProficiencyFlags += (1 << ITEM_SUBCLASS.ITEM_SUBCLASS_AXE)
            If Spells.Contains(197) Then ProficiencyFlags += (1 << ITEM_SUBCLASS.ITEM_SUBCLASS_TWOHAND_AXE)
            If Spells.Contains(264) Then ProficiencyFlags += (1 << ITEM_SUBCLASS.ITEM_SUBCLASS_BOW)
            If Spells.Contains(266) Then ProficiencyFlags += (1 << ITEM_SUBCLASS.ITEM_SUBCLASS_GUN)
            If Spells.Contains(198) Then ProficiencyFlags += (1 << ITEM_SUBCLASS.ITEM_SUBCLASS_MACE)
            If Spells.Contains(199) Then ProficiencyFlags += (1 << ITEM_SUBCLASS.ITEM_SUBCLASS_TWOHAND_MACE)
            If Spells.Contains(200) Then ProficiencyFlags += (1 << ITEM_SUBCLASS.ITEM_SUBCLASS_POLEARM)
            If Spells.Contains(201) Then ProficiencyFlags += (1 << ITEM_SUBCLASS.ITEM_SUBCLASS_SWORD)
            If Spells.Contains(202) Then ProficiencyFlags += (1 << ITEM_SUBCLASS.ITEM_SUBCLASS_TWOHAND_SWORD)
            'If Spells.Contains() Then ProficiencyFlags += (1 << ITEM_SUBCLASS.ITEM_SUBCLASS_WEAPON_obsolete)
            If Spells.Contains(227) Then ProficiencyFlags += (1 << ITEM_SUBCLASS.ITEM_SUBCLASS_STAFF)
            If Spells.Contains(262) Then ProficiencyFlags += (1 << ITEM_SUBCLASS.ITEM_SUBCLASS_WEAPON_EXOTIC)
            If Spells.Contains(263) Then ProficiencyFlags += (1 << ITEM_SUBCLASS.ITEM_SUBCLASS_WEAPON_EXOTIC2)
            If Spells.Contains(15590) Then ProficiencyFlags += (1 << ITEM_SUBCLASS.ITEM_SUBCLASS_FIST_WEAPON)
            If Spells.Contains(2382) Then ProficiencyFlags += (1 << ITEM_SUBCLASS.ITEM_SUBCLASS_MISC_WEAPON) 'Here using spell "Generic"
            If Spells.Contains(1180) Then ProficiencyFlags += (1 << ITEM_SUBCLASS.ITEM_SUBCLASS_DAGGER)
            If Spells.Contains(2567) Then ProficiencyFlags += (1 << ITEM_SUBCLASS.ITEM_SUBCLASS_THROWN)
            If Spells.Contains(3386) Then ProficiencyFlags += (1 << ITEM_SUBCLASS.ITEM_SUBCLASS_SPEAR)
            If Spells.Contains(5011) Then ProficiencyFlags += (1 << ITEM_SUBCLASS.ITEM_SUBCLASS_CROSSBOW)
            If Spells.Contains(5009) Then ProficiencyFlags += (1 << ITEM_SUBCLASS.ITEM_SUBCLASS_WAND)
            If Spells.Contains(7738) Then ProficiencyFlags += (1 << ITEM_SUBCLASS.ITEM_SUBCLASS_FISHING_POLE)
            SendProficiency(Client, ITEM_CLASS.ITEM_CLASS_WEAPON, ProficiencyFlags)

            'TODO: SMSG_UPDATE_AURA_DURATION
            'TODO: SMSG_PET_SPELLS

            'DONE: SMSG_INITIAL_SPELLS
            SendInitialSpells(Client, Me)
            'DONE: SMSG_INITIALIZE_FACTIONS
            SendFactions(Client, Me)
            'DONE: SMSG_ACTION_BUTTONS
            SendActionButtons(Client, Me)

            'DONE: Setting instance ID
            InstanceMapEnter(Me)

            'DONE: SMSG_UPDATE_OBJECT for ourself
            FillAllUpdateFlags()
            SendUpdate()

            'DONE: Adding to World
            AddToWorld(Me)

            'DONE: Enable client moving
            SendTimeSyncReq(Client)

            FullyLoggedIn = True
            UpdateManaRegen()
        End Sub
        Public Sub Dispose() Implements System.IDisposable.Dispose
            'WARNING: Do not save character here!!!

            'DONE: Remove buyback items when logged out
            Database.Update(String.Format("DELETE FROM characters_inventory WHERE item_bag = {0} AND item_slot >= {1} AND item_slot <= {2}", GUID, BUYBACK_SLOT_START, BUYBACK_SLOT_END - 1))

            If Not underWaterTimer Is Nothing Then underWaterTimer.Dispose()

            'DONE: Spawn corpse and remove repop timer if present
            If Not (repopTimer Is Nothing) Then
                repopTimer.Dispose()
                repopTimer = Nothing
                'DONE: Spawn Corpse
                Dim myCorpse As New CorpseObject(Me)
                myCorpse.Save()
                myCorpse.AddToWorld()
            End If

            'DONE: Remove non-combat pets
            If OldCritter IsNot Nothing Then OldCritter.Destroy()

            CHARACTERs_Lock.AcquireWriterLock(DEFAULT_LOCK_TIMEOUT)
            CHARACTERs.Remove(GUID)
            CHARACTERs_Lock.ReleaseWriterLock()

            RemoveFromWorld(Me)

            Log.WriteLine(LogType.USER, "Character {0} disposed.", Name)

            For Each Item As KeyValuePair(Of Byte, ItemObject) In Items
                'DONE: Dispose items in bags (done in Item.Dispose)
                Item.Value.Dispose()
            Next

            If Not Client Is Nothing Then Client.Character = Nothing
            If Not LogoutTimer Is Nothing Then CType(LogoutTimer, Threading.Timer).Dispose()
            LogoutTimer = Nothing

            GC.Collect()
        End Sub
        Public Sub Initialize()
            Me.CanSeeInvisibility_Stealth = 0
            Me.CanSeeInvisibility_Invisibility = 0
            Me.Model_Native = Me.Model

            'If Classe = Classes.CLASS_WARRIOR Then Me.ShapeshiftForm = WS_Spells.ShapeshiftForm.FORM_BATTLESTANCE
            If Classe = Classes.CLASS_WARRIOR Then ApplySpell(2457)

            Resistances(DamageTypes.DMG_PHYSICAL).Base += Agility.Base * 2
            Damage.Type = DamageTypes.DMG_PHYSICAL
            Damage.Minimum += 1
            RangedDamage.Type = DamageTypes.DMG_PHYSICAL
            RangedDamage.Minimum += 1
            'TODO: Calculate base dodge, parry, block

            If Access >= AccessLevel.GameMaster Then GM = True
        End Sub
        Public Sub New()
            Level = 1
            UpdateMask.SetAll(False)

            For i As Byte = DamageTypes.DMG_PHYSICAL To DamageTypes.DMG_ARCANE
                spellDamage(i) = New TDamageBonus
                Resistances(i) = New TStat
            Next

        End Sub
        Public Sub New(ByRef ClientVal As ClientClass, ByVal GuidVal As ULong)
            Dim i As Integer

            'DONE: Add space for passive auras
            ReDim ActiveSpells(MAX_AURA_EFFECTs - 1)

            'DONE: Initialize Defaults
            Client = ClientVal
            GUID = GuidVal
            Client.Character = Me

            For i = DamageTypes.DMG_PHYSICAL To DamageTypes.DMG_ARCANE
                spellDamage(i) = New TDamageBonus
                Resistances(i) = New TStat
            Next

            'DONE: Get character info from DB
            Dim MySQLQuery As New DataTable
            Database.Query(String.Format("SELECT * FROM characters WHERE char_guid = {0}; UPDATE characters SET char_online = 1 WHERE char_guid = {0};", GUID), MySQLQuery)
            If MySQLQuery.Rows.Count = 0 Then
                Log.WriteLine(LogType.DEBUG, "[{0}:{1}] Unable to get SQLDataBase info for character [GUID={2:X}]", Client.IP, Client.Port, GUID)
                Me.Dispose()
                Exit Sub
            End If

            If CharRaces.ContainsKey(CType(MySQLQuery.Rows(0).Item("char_race"), Byte)) = True Then
                Faction = CType(CharRaces(CType(MySQLQuery.Rows(0).Item("char_race"), Byte)), TCharRace).FactionID
            End If

            'DONE: Get BindPoint Coords
            bindpoint_positionX = CType(MySQLQuery.Rows(0).Item("bindpoint_positionX"), Single)
            bindpoint_positionY = CType(MySQLQuery.Rows(0).Item("bindpoint_positionY"), Single)
            bindpoint_positionZ = CType(MySQLQuery.Rows(0).Item("bindpoint_positionZ"), Single)
            bindpoint_map_id = CType(MySQLQuery.Rows(0).Item("bindpoint_map_id"), Integer)
            bindpoint_zone_id = CType(MySQLQuery.Rows(0).Item("bindpoint_zone_id"), Integer)

            'DONE: Get CharCreate Vars
            Race = CType(MySQLQuery.Rows(0).Item("char_race"), Byte)
            Classe = CType(MySQLQuery.Rows(0).Item("char_class"), Byte)
            Gender = CType(MySQLQuery.Rows(0).Item("char_gender"), Byte)
            Skin = CType(MySQLQuery.Rows(0).Item("char_skin"), Byte)
            Face = CType(MySQLQuery.Rows(0).Item("char_face"), Byte)
            HairStyle = CType(MySQLQuery.Rows(0).Item("char_hairStyle"), Byte)
            HairColor = CType(MySQLQuery.Rows(0).Item("char_hairColor"), Byte)
            FacialHair = CType(MySQLQuery.Rows(0).Item("char_facialHair"), Byte)
            OutfitId = CType(MySQLQuery.Rows(0).Item("char_outfitId"), Byte)
            Model = CType(MySQLQuery.Rows(0).Item("char_model"), Integer)
            ManaType = CType(MySQLQuery.Rows(0).Item("char_manaType"), Byte)
            Life.Base = CType(MySQLQuery.Rows(0).Item("char_life"), Short)
            Life.Current = Life.Maximum
            Mana.Base = CType(MySQLQuery.Rows(0).Item("char_mana"), Short)
            Mana.Current = Mana.Maximum
            Rage.Base = CType(MySQLQuery.Rows(0).Item("char_rage"), Short)
            Rage.Current = 0
            Energy.Base = CType(MySQLQuery.Rows(0).Item("char_energy"), Short)
            Energy.Current = Energy.Maximum
            XP = CType(MySQLQuery.Rows(0).Item("char_xp"), Integer)

            'DONE: Get Guild Info
            GuildID = MySQLQuery.Rows(0).Item("char_guildId")
            GuildRank = MySQLQuery.Rows(0).Item("char_guildRank")

            'DONE: Get all other vars
            Name = CType(MySQLQuery.Rows(0).Item("char_name"), String)
            Level = CType(MySQLQuery.Rows(0).Item("char_level"), Byte)
            Access = CType(MySQLQuery.Rows(0).Item("char_access"), Byte)
            Copper = CType(MySQLQuery.Rows(0).Item("char_copper"), UInteger)
            HonorCurrency = CType(MySQLQuery.Rows(0).Item("char_honorpoints"), Integer)
            ArenaCurrency = CType(MySQLQuery.Rows(0).Item("char_arenapoints"), Integer)
            positionX = CType(MySQLQuery.Rows(0).Item("char_positionX"), Single)
            positionY = CType(MySQLQuery.Rows(0).Item("char_positionY"), Single)
            positionZ = CType(MySQLQuery.Rows(0).Item("char_positionZ"), Single)
            orientation = CType(MySQLQuery.Rows(0).Item("char_orientation"), Single)
            ZoneID = CType(MySQLQuery.Rows(0).Item("char_zone_id"), Single)
            MapID = CType(MySQLQuery.Rows(0).Item("char_map_id"), Single)
            Strength.Base = CType(MySQLQuery.Rows(0).Item("char_strength"), Byte)
            Agility.Base = CType(MySQLQuery.Rows(0).Item("char_agility"), Byte)
            Stamina.Base = CType(MySQLQuery.Rows(0).Item("char_stamina"), Byte)
            Intellect.Base = CType(MySQLQuery.Rows(0).Item("char_intellect"), Byte)
            Spirit.Base = CType(MySQLQuery.Rows(0).Item("char_spirit"), Byte)
            TalentPoints = CType(MySQLQuery.Rows(0).Item("char_talentpoints"), Byte)
            Items_AvailableBankSlots = CType(MySQLQuery.Rows(0).Item("char_bankSlots"), Byte)
            WatchedFactionIndex = CType(MySQLQuery.Rows(0).Item("char_watchedFactionIndex"), Byte)

            Dim tmp() As String

            'DONE: Get SpellList -> Saved as STRING like "SpellID1 SpellID2 SpellID3"
            tmp = Split(CType(MySQLQuery.Rows(0).Item("char_spellList"), String), " ")
            If tmp.Length > 0 Then
                For i = 0 To tmp.Length - 1
                    If Trim(tmp(i)) <> "" Then Spells.Add(CType(tmp(i), Integer))
                Next i
            End If

            'DONE: Get SkillList -> Saved as STRING like "SkillID1:Current:Maximum SkillID2:Current:Maximum SkillID3:Current:Maximum"
            tmp = Split(CType(MySQLQuery.Rows(0).Item("char_skillList"), String), " ")
            If tmp.Length > 0 Then
                For i = 0 To tmp.Length - 1
                    If Trim(tmp(i)) <> "" Then
                        Dim tmp2() As String
                        tmp2 = Split(tmp(i), ":")
                        Skills(CType(tmp2(0), Integer)) = New TSkill(tmp2(1), tmp2(2))
                        SkillsPositions(CType(tmp2(0), Integer)) = i
                    End If
                Next i
            End If

            'DONE: Get TutorialFlags -> Saved as STRING like "Flag1 Flag2 Flag3"
            tmp = Split(CType(MySQLQuery.Rows(0).Item("char_tutorialFlags"), String), " ")
            If tmp.Length > 0 Then
                For i = 0 To tmp.Length - 1
                    If Trim(tmp(i)) <> "" Then TutorialFlags(i) = tmp(i)
                Next i
            End If

            'DONE: Get TaxiFlags -> Saved as STRING like "Flag1 Flag2 Flag3"
            tmp = Split(CType(MySQLQuery.Rows(0).Item("char_taxiFlags"), String), " ")
            If tmp.Length > 0 Then
                For i = 0 To tmp.Length - 1
                    If Trim(tmp(i)) <> "" Then
                        For j As Byte = 0 To 7
                            If (tmp(i) And (1 << j)) Then
                                TaxiZones.Set((i * 8) + j, True)
                            End If
                        Next j
                    End If
                Next i
            End If

            'DONE: Get ZonesExplored -> Saved as STRING like "Flag1 Flag2 Flag3"
            tmp = Split(CType(MySQLQuery.Rows(0).Item("char_mapExplored"), String), " ")
            If tmp.Length > 0 Then
                For i = 0 To tmp.Length - 1
                    If Trim(tmp(i)) <> "" Then ZonesExplored(i) = UInteger.Parse(tmp(i))
                Next i
            End If

            'DONE: Get ActionButtons -> Saved as STRING like "Button1:Action1:Type1:Misc1 Button2:Action2:Type2:Misc2"
            tmp = Split(CType(MySQLQuery.Rows(0).Item("char_actionBar"), String), " ")
            If tmp.Length > 0 Then
                For i = 0 To tmp.Length - 1
                    If Trim(tmp(i)) <> "" Then
                        Dim tmp2() As String
                        tmp2 = Split(tmp(i), ":")
                        ActionButtons(CType(tmp2(0), Byte)) = New TActionButton(tmp2(1), tmp2(2), tmp2(3))
                    End If
                Next i
            End If

            'DONE: Get ReputationPoints -> Saved as STRING like "Flags1:Standing1 Flags2:Standing2"
            tmp = Split(CType(MySQLQuery.Rows(0).Item("char_reputation"), String), " ")
            For i = 0 To 63
                Dim tmp2() As String
                tmp2 = Split(tmp(i), ":")
                Reputation(i) = New TReputation
                Reputation(i).Flags = Trim(tmp2(0))
                Reputation(i).Value = Trim(tmp2(1))
            Next


            'DONE: Get Items
            MySQLQuery.Clear()
            Database.Query(String.Format("SELECT * FROM characters_inventory WHERE item_bag = {0};", GUID), MySQLQuery)
            For Each row As DataRow In MySQLQuery.Rows
                If row.Item("item_slot") <> ITEM_SLOT_NULL Then
                    Dim tmpItem As ItemObject = LoadItemByGUID(CType(row.Item("item_guid"), Long))
                    Items(CType(row.Item("item_slot"), Byte)) = tmpItem
                    If CType(row.Item("item_slot"), Byte) < EQUIPMENT_SLOT_END Then UpdateAddItemStats(tmpItem, row.Item("item_slot"))
                End If
            Next

            'DONE: Get Honor Point
            Me.HonorLoad()

            'DONE: Load quests in progress
            LoadQuests(Me)

            'DONE: Initialize Internal fields
            Me.Initialize()

            'DONE: Load arena teams
            MySQLQuery.Clear()
            Database.Query(String.Format("SELECT member_team, member_type FROM arena_members WHERE member_id = {0}", GUID), MySQLQuery)
            If MySQLQuery.Rows.Count > 0 Then
                For i = 0 To MySQLQuery.Rows.Count - 1
                    Dim Slot As Byte = 0
                    Select CByte(MySQLQuery.Rows(i).Item("member_type"))
                        Case 2
                            Slot = 0
                        Case 3
                            Slot = 1
                        Case 5
                            Slot = 2
                        Case Else
                            Slot = 255
                    End Select
                    If Slot < 255 Then ArenaTeamID(i) = CUInt(MySQLQuery.Rows(i).Item("member_team"))
                Next i
            End If

            'DONE: Load corpse if present
            MySQLQuery.Clear()
            Database.Query(String.Format("SELECT * FROM tmpspawnedcorpses WHERE corpse_owner = {0};", GUID), MySQLQuery)
            If MySQLQuery.Rows.Count > 0 Then
                corpseGUID = MySQLQuery.Rows(0).Item("corpse_guid") + GUID_CORPSE
                corpseMapID = MySQLQuery.Rows(0).Item("corpse_MapID")
                corpsePositionX = MySQLQuery.Rows(0).Item("corpse_positionX")
                corpsePositionY = MySQLQuery.Rows(0).Item("corpse_positionY")
                corpsePositionZ = MySQLQuery.Rows(0).Item("corpse_positionZ")

                'DONE: Make Dead
                DEAD = True
                SetFlag(cPlayerFlags, PlayerFlag.PLAYER_FLAG_DEAD, True)

                'DONE: Update to see only dead
                Invisibility = InvisibilityLevel.DEAD
                CanSeeInvisibility = InvisibilityLevel.DEAD

                SetWaterWalk()

                'DONE: Set Auras
                If Client.Character.Race = Races.RACE_NIGHT_ELF Then
                    Client.Character.ApplySpell(20584)
                Else
                    Client.Character.ApplySpell(8326)
                End If

                Mana.Current = 0
                Rage.Current = 0
                Energy.Current = 0
                Life.Current = 1
                cUnitFlags = UnitFlags.UNIT_FLAG_ATTACKABLE
                cDynamicFlags = 0
            Else
                'DONE: Calculate the bonus health and mana from stats
                Life.Bonus = ((Stamina.Base - 18) * 10)
                Mana.Bonus = ((Intellect.Base - 18) * 15)
                Life.Current = Life.Maximum
                Mana.Current = Life.Maximum
            End If

        End Sub
        Public Sub SaveAsNewCharacter(ByVal Account_ID As Integer)
            'Only for creating New Character
            Dim tmpCMD As String = "INSERT INTO characters (account_id"
            Dim tmpValues As String = " VALUES (" & Account_ID
            Dim temp As New ArrayList

            tmpCMD = tmpCMD & ", char_name"
            tmpValues = tmpValues & ", """ & Name & """"
            tmpCMD = tmpCMD & ", char_race"
            tmpValues = tmpValues & ", " & Race
            tmpCMD = tmpCMD & ", char_class"
            tmpValues = tmpValues & ", " & Classe
            tmpCMD = tmpCMD & ", char_gender"
            tmpValues = tmpValues & ", " & Gender
            tmpCMD = tmpCMD & ", char_skin"
            tmpValues = tmpValues & ", " & Skin
            tmpCMD = tmpCMD & ", char_face"
            tmpValues = tmpValues & ", " & Face
            tmpCMD = tmpCMD & ", char_hairStyle"
            tmpValues = tmpValues & ", " & HairStyle
            tmpCMD = tmpCMD & ", char_hairColor"
            tmpValues = tmpValues & ", " & HairColor
            tmpCMD = tmpCMD & ", char_facialHair"
            tmpValues = tmpValues & ", " & FacialHair
            tmpCMD = tmpCMD & ", char_outfitId"
            tmpValues = tmpValues & ", " & OutfitId
            tmpCMD = tmpCMD & ", char_level"
            tmpValues = tmpValues & ", " & Level
            tmpCMD = tmpCMD & ", char_model"
            tmpValues = tmpValues & ", " & Model
            tmpCMD = tmpCMD & ", char_manaType"
            tmpValues = tmpValues & ", " & ManaType

            tmpCMD = tmpCMD & ", char_mana"
            tmpValues = tmpValues & ", " & Mana.Base
            tmpCMD = tmpCMD & ", char_rage"
            tmpValues = tmpValues & ", " & Rage.Base
            tmpCMD = tmpCMD & ", char_energy"
            tmpValues = tmpValues & ", " & Energy.Base
            tmpCMD = tmpCMD & ", char_life"
            tmpValues = tmpValues & ", " & Life.Base

            tmpCMD = tmpCMD & ", char_positionX"
            tmpValues = tmpValues & ", " & Trim(Str(positionX))
            tmpCMD = tmpCMD & ", char_positionY"
            tmpValues = tmpValues & ", " & Trim(Str(positionY))
            tmpCMD = tmpCMD & ", char_positionZ"
            tmpValues = tmpValues & ", " & Trim(Str(positionZ))
            tmpCMD = tmpCMD & ", char_map_id"
            tmpValues = tmpValues & ", " & MapID
            tmpCMD = tmpCMD & ", char_zone_id"
            tmpValues = tmpValues & ", " & ZoneID
            tmpCMD = tmpCMD & ", char_orientation"
            tmpValues = tmpValues & ", " & Trim(Str(orientation))
            tmpCMD = tmpCMD & ", bindpoint_positionX"
            tmpValues = tmpValues & ", " & Trim(Str(bindpoint_positionX))
            tmpCMD = tmpCMD & ", bindpoint_positionY"
            tmpValues = tmpValues & ", " & Trim(Str(bindpoint_positionY))
            tmpCMD = tmpCMD & ", bindpoint_positionZ"
            tmpValues = tmpValues & ", " & Trim(Str(bindpoint_positionZ))
            tmpCMD = tmpCMD & ", bindpoint_map_id"
            tmpValues = tmpValues & ", " & bindpoint_map_id
            tmpCMD = tmpCMD & ", bindpoint_zone_id"
            tmpValues = tmpValues & ", " & bindpoint_zone_id

            tmpCMD = tmpCMD & ", char_copper"
            tmpValues = tmpValues & ", " & Copper
            tmpCMD = tmpCMD & ", char_xp"
            tmpValues = tmpValues & ", " & XP

            'char_spellList
            temp.Clear()
            For Each Spell As String In Spells
                temp.Add(Spell)
            Next
            tmpCMD = tmpCMD & ", char_spellList"
            tmpValues = tmpValues & ", """ & Join(temp.ToArray, " ") & """"

            'char_skillList
            temp.Clear()
            For Each Skill As KeyValuePair(Of Integer, TSkill) In Skills
                temp.Add(String.Format("{0}:{1}:{2}", Skill.Key, Skill.Value.Current, Skill.Value.Maximum))
            Next
            tmpCMD = tmpCMD & ", char_skillList"
            tmpValues = tmpValues & ", """ & Join(temp.ToArray, " ") & """"

            'char_tutorialFlags
            temp.Clear()
            For Each Flag As Byte In TutorialFlags
                temp.Add(Flag)
            Next
            tmpCMD = tmpCMD & ", char_tutorialFlags"
            tmpValues = tmpValues & ", """ & Join(temp.ToArray, " ") & """"

            'char_mapExplored
            temp.Clear()
            For Each Flag As Byte In ZonesExplored
                temp.Add(Flag)
            Next
            tmpCMD = tmpCMD & ", char_mapExplored"
            tmpValues = tmpValues & ", """ & Join(temp.ToArray, " ") & """"

            'char_reputation
            temp.Clear()
            For Each Reputation_Point As TReputation In Reputation
                temp.Add(Reputation_Point.Flags & ":" & Reputation_Point.Value)
            Next
            tmpCMD = tmpCMD & ", char_reputation"
            tmpValues = tmpValues & ", """ & Join(temp.ToArray, " ") & """"

            'char_actionBar
            temp.Clear()
            For Each ActionButton As KeyValuePair(Of Byte, TActionButton) In ActionButtons
                temp.Add(String.Format("{0}:{1}:{2}:{3}", ActionButton.Key, ActionButton.Value.Action, ActionButton.Value.ActionType, ActionButton.Value.ActionMisc))
            Next
            tmpCMD = tmpCMD & ", char_actionBar"
            tmpValues = tmpValues & ", """ & Join(temp.ToArray, " ") & """"

            tmpCMD = tmpCMD & ", char_access"
            tmpValues = tmpValues & ", " & Access
            tmpCMD = tmpCMD & ", char_strength"
            tmpValues = tmpValues & ", " & Strength.RealBase
            tmpCMD = tmpCMD & ", char_agility"
            tmpValues = tmpValues & ", " & Agility.RealBase
            tmpCMD = tmpCMD & ", char_stamina"
            tmpValues = tmpValues & ", " & Stamina.RealBase
            tmpCMD = tmpCMD & ", char_intellect"
            tmpValues = tmpValues & ", " & Intellect.RealBase
            tmpCMD = tmpCMD & ", char_spirit"
            tmpValues = tmpValues & ", " & Spirit.RealBase

            tmpCMD = tmpCMD & ") " & tmpValues & ");"
            Database.Update(tmpCMD)

            Dim MySQLQuery As New DataTable
            Database.Query(String.Format("SELECT char_guid FROM characters WHERE char_name = '{0}';", Name), MySQLQuery)
            GUID = CType(MySQLQuery.Rows(0).Item("char_guid"), Long)

            HonorSaveAsNew()
        End Sub
        Public Sub Save()
            Me.SaveCharacter()

            For Each Item As KeyValuePair(Of Byte, ItemObject) In Items
                Item.Value.Save()
            Next
        End Sub
        Public Sub SaveCharacter()
            Dim tmp As String = "UPDATE characters SET"

            tmp = tmp & " char_name=""" & Name & """"
            tmp = tmp & ", char_race=" & Race
            tmp = tmp & ", char_class=" & Classe
            tmp = tmp & ", char_gender=" & Gender
            tmp = tmp & ", char_skin=" & Skin
            tmp = tmp & ", char_face=" & Face
            tmp = tmp & ", char_hairStyle=" & HairStyle
            tmp = tmp & ", char_hairColor=" & HairColor
            tmp = tmp & ", char_facialHair=" & FacialHair
            tmp = tmp & ", char_outfitId=" & OutfitId
            tmp = tmp & ", char_level=" & Level
            tmp = tmp & ", char_model=" & Model_Native
            tmp = tmp & ", char_manaType=" & ManaType

            tmp = tmp & ", char_life=" & Life.Base
            tmp = tmp & ", char_rage=" & Rage.Base
            tmp = tmp & ", char_mana=" & Mana.Base
            tmp = tmp & ", char_energy=" & Energy.Base

            tmp = tmp & ", char_strength=" & Strength.RealBase
            tmp = tmp & ", char_agility=" & Agility.RealBase
            tmp = tmp & ", char_stamina=" & Stamina.RealBase
            tmp = tmp & ", char_intellect=" & Intellect.RealBase
            tmp = tmp & ", char_spirit=" & Spirit.RealBase

            tmp = tmp & ", char_positionX=" & Trim(Str(positionX))
            tmp = tmp & ", char_positionY=" & Trim(Str(positionY))
            tmp = tmp & ", char_positionZ=" & Trim(Str(positionZ))
            tmp = tmp & ", char_map_id=" & MapID
            tmp = tmp & ", char_zone_id=" & ZoneID
            tmp = tmp & ", char_orientation=" & Trim(Str(orientation))
            tmp = tmp & ", bindpoint_positionX=" & Trim(Str(bindpoint_positionX))
            tmp = tmp & ", bindpoint_positionY=" & Trim(Str(bindpoint_positionY))
            tmp = tmp & ", bindpoint_positionZ=" & Trim(Str(bindpoint_positionZ))
            tmp = tmp & ", bindpoint_map_id=" & bindpoint_map_id
            tmp = tmp & ", bindpoint_zone_id=" & bindpoint_zone_id

            tmp = tmp & ", char_copper=" & Copper
            tmp = tmp & ", char_honorpoints=" & HonorCurrency
            tmp = tmp & ", char_arenapoints=" & ArenaCurrency
            tmp = tmp & ", char_xp=" & XP
            tmp = tmp & ", char_access=" & Access

            tmp = tmp & ", char_guildId=" & GuildID
            tmp = tmp & ", char_guildRank=" & GuildRank

            Dim temp As New ArrayList

            'char_spellList
            temp.Clear()
            For Each Spell As String In Spells
                temp.Add(Spell)
            Next
            tmp = tmp & ", char_spellList=""" & Join(temp.ToArray, " ") & """"

            'char_skillList
            temp.Clear()
            For Each Skill As KeyValuePair(Of Integer, TSkill) In Skills
                temp.Add(String.Format("{0}:{1}:{2}", Skill.Key, CType(Skill.Value, TSkill).Current, CType(Skill.Value, TSkill).Maximum))
            Next
            tmp = tmp & ", char_skillList=""" & Join(temp.ToArray, " ") & """"

            'char_tutorialFlags
            temp.Clear()
            For Each Flag As Byte In TutorialFlags
                temp.Add(Flag)
            Next
            tmp = tmp & ", char_tutorialFlags=""" & Join(temp.ToArray, " ") & """"

            'char_taxiFlags
            temp.Clear()
            Dim TmpArray(31) As Byte
            TaxiZones.CopyTo(TmpArray, 0)
            For Each Flag As Byte In TmpArray
                temp.Add(Flag)
            Next
            tmp = tmp & ", char_taxiFlags=""" & Join(temp.ToArray, " ") & """"

            'char_mapExplored
            temp.Clear()
            For Each Flag As UInteger In ZonesExplored
                temp.Add(Flag)
            Next
            tmp = tmp & ", char_mapExplored=""" & Join(temp.ToArray, " ") & """"

            'char_reputation
            temp.Clear()
            For Each Reputation_Point As TReputation In Reputation
                temp.Add(Reputation_Point.Flags & ":" & Reputation_Point.Value)
            Next
            tmp = tmp & ", char_reputation=""" & Join(temp.ToArray, " ") & """"

            'char_actionBar
            temp.Clear()
            For Each ActionButton As KeyValuePair(Of Byte, TActionButton) In ActionButtons
                temp.Add(String.Format("{0}:{1}:{2}:{3}", ActionButton.Key, ActionButton.Value.Action, ActionButton.Value.ActionType, ActionButton.Value.ActionMisc))
            Next
            tmp = tmp & ", char_actionBar=""" & Join(temp.ToArray, " ") & """"

            tmp = tmp & ", char_talentpoints=" & TalentPoints

            tmp = tmp + String.Format(" WHERE char_guid = ""{0}"";", GUID)
            Database.Update(tmp)
        End Sub

        'Party/Raid
        Public Group As Group = Nothing
        Public ReadOnly Property IsInGroup() As Boolean
            Get
                Return Not (Group Is Nothing)
            End Get
        End Property
        Public ReadOnly Property IsInRaid() As Boolean
            Get
                Return ((Not (Group Is Nothing)) AndAlso (Group.Type = GroupType.RAID))
            End Get
        End Property
        Public ReadOnly Property IsGroupLeader() As Boolean
            Get
                Return (Group.Leader = GUID)
            End Get
        End Property
        Public Sub GroupUpdate(ByVal Flag As Integer)
            If Group Is Nothing Then Exit Sub
            Dim Packet As PacketClass = BuildPartyMemberStats(Me, Flag)
            If Not Packet Is Nothing Then Group.Broadcast(Packet)
        End Sub

        'Arenas
        Public ArenaTeamID() As UInteger = {0, 0, 0}

        'Guilds
        Public GuildID As Integer = 0
        Public GuildRank As Byte = 0
        Public GuildInvited As Integer = 0
        Public GuildInvitedBy As Integer = 0
        Public ReadOnly Property IsInGuild() As Boolean
            Get
                Return GuildID <> 0
            End Get
        End Property
        Public ReadOnly Property IsGuildLeader() As Boolean
            Get
                Dim MySQLQuery As New DataTable
                Database.Query("SELECT guild_id FROM guilds WHERE guild_id = " & GuildID & " AND guild_leader = " & GUID & " LIMIT 1;", MySQLQuery)
                Return MySQLQuery.Rows.Count <> 0
            End Get
        End Property
        Public ReadOnly Property IsGuildRightSet(ByVal rights As GuildRankRights) As Boolean
            Get
                Dim MySQLQuery As New DataTable
                Database.Query(String.Format("SELECT guild_rank{0}_Rights FROM guilds WHERE guild_id = {1} LIMIT 1;", GuildRank, GuildID), MySQLQuery)
                Return ((CType(MySQLQuery.Rows(0).Item(0), Integer) And CType(rights, Integer)) = CType(rights, Integer))
            End Get
        End Property

        'Duel
        Public DuelArbiter As ULong = 0
        Public DuelPartner As CharacterObject = Nothing
        Public DuelOutOfBounds As Byte = DUEL_COUNTER_DISABLED
        Public ReadOnly Property IsInDuel() As Boolean
            Get
                Return (Not (DuelPartner Is Nothing))
            End Get
        End Property

        'NPC Talking and Quests
        Public TalkMenuTypes As New ArrayList
        Public TalkQuests(QUEST_SLOTS) As BaseQuest
        Public TalkCurrentQuest As QuestInfo = Nothing
        Public Function TalkAddQuest(ByRef Quest As QuestInfo) As Boolean
            Dim i As Integer
            For i = 0 To QUEST_SLOTS
                If TalkQuests(i) Is Nothing Then
                    'DONE: Initialize quest info
                    CreateQuest(TalkQuests(i), Quest)

                    'DONE: Initialize quest
                    If TypeOf TalkQuests(i) Is BaseQuestScripted Then
                        CType(TalkQuests(i), BaseQuestScripted).OnQuestStart(Me)
                    Else
                        TalkQuests(i).Initialize(Me)
                    End If

                    TalkQuests(i).Slot = i


                    Dim updateDataCount As Integer = UpdateData.Count
                    Dim questState As Integer = TalkQuests(i).GetState

                    SetUpdateFlag(EPlayerFields.PLAYER_QUEST_LOG_1_1 + i * 4, TalkQuests(i).ID)
                    SetUpdateFlag(EPlayerFields.PLAYER_QUEST_LOG_1_2 + i * 4, 0)
                    SetUpdateFlag(EPlayerFields.PLAYER_QUEST_LOG_1_3 + i * 4, questState)
                    SetUpdateFlag(EPlayerFields.PLAYER_QUEST_LOG_1_4 + i * 4, 0) 'Timer

                    Database.Update(String.Format("INSERT INTO characters_quests (char_guid, quest_id, quest_status) VALUES ({0}, {1}, {2});", GUID, TalkQuests(i).ID, questState))

                    SendCharacterUpdate(updateDataCount <> 0)
                    Return True
                End If
            Next

            Return False
        End Function
        Public Function TalkDeleteQuest(ByVal QuestSlot As Byte) As Boolean
            If TalkQuests(QuestSlot) Is Nothing Then
                Return False
            Else
                If TypeOf TalkQuests(QuestSlot) Is BaseQuestScripted Then CType(TalkQuests(QuestSlot), BaseQuestScripted).OnQuestCancel(Me)

                Dim updateDataCount As Integer = UpdateData.Count

                SetUpdateFlag(EPlayerFields.PLAYER_QUEST_LOG_1_1 + QuestSlot * 4, 0)
                SetUpdateFlag(EPlayerFields.PLAYER_QUEST_LOG_1_2 + QuestSlot * 4, 0)
                SetUpdateFlag(EPlayerFields.PLAYER_QUEST_LOG_1_3 + QuestSlot * 4, 0)
                SetUpdateFlag(EPlayerFields.PLAYER_QUEST_LOG_1_4 + QuestSlot * 4, 0)

                Database.Update(String.Format("DELETE  FROM characters_quests WHERE char_guid = {0} AND quest_id = {1};", GUID, TalkQuests(QuestSlot).ID))
                TalkQuests(QuestSlot) = Nothing

                SendCharacterUpdate(updateDataCount <> 0)
                Return True
            End If
        End Function
        Public Function TalkCompleteQuest(ByVal QuestSlot As Byte) As Boolean
            If TalkQuests(QuestSlot) Is Nothing Then
                Return False
            Else
                If TypeOf TalkQuests(QuestSlot) Is BaseQuestScripted Then CType(TalkQuests(QuestSlot), BaseQuestScripted).OnQuestComplete(Me)
                Dim updateDataCount As Integer = UpdateData.Count

                SetUpdateFlag(EPlayerFields.PLAYER_QUEST_LOG_1_1 + QuestSlot * 4, 0)
                SetUpdateFlag(EPlayerFields.PLAYER_QUEST_LOG_1_2 + QuestSlot * 4, 0)
                SetUpdateFlag(EPlayerFields.PLAYER_QUEST_LOG_1_3 + QuestSlot * 4, 0)
                SetUpdateFlag(EPlayerFields.PLAYER_QUEST_LOG_1_4 + QuestSlot * 4, 0)

                Database.Update(String.Format("UPDATE characters_quests SET quest_status = -1 WHERE char_guid = {0} AND quest_id = {1};", GUID, TalkQuests(QuestSlot).ID))
                TalkQuests(QuestSlot) = Nothing

                'SendCharacterUpdate(updateDataCount <> 0)
                Return True
            End If
        End Function
        Public Function TalkUpdateQuest(ByVal QuestSlot As Byte) As Boolean
            If TalkQuests(QuestSlot) Is Nothing Then
                Return False
            Else
                Dim updateDataCount As Integer = UpdateData.Count
                Dim tmpState As Integer
                If TalkQuests(QuestSlot).Complete Then tmpState = 1
                If TalkQuests(QuestSlot).Failed Then tmpState = 2
                Dim tmpProgress As Integer = TalkQuests(QuestSlot).GetState
                Dim tmpTimer As Integer = 0
                If TalkQuests(QuestSlot).TimeEnd > 0 Then tmpTimer = TalkQuests(QuestSlot).TimeEnd - GetTimestamp(Now)
                Database.Update(String.Format("UPDATE characters_quests SET quest_status = {2} WHERE char_guid = {0} AND quest_id = {1};", GUID, TalkQuests(QuestSlot).ID, tmpProgress))

                SetUpdateFlag(EPlayerFields.PLAYER_QUEST_LOG_1_2 + QuestSlot * 4, tmpState)
                SetUpdateFlag(EPlayerFields.PLAYER_QUEST_LOG_1_3 + QuestSlot * 4, tmpProgress)
                SetUpdateFlag(EPlayerFields.PLAYER_QUEST_LOG_1_4 + QuestSlot * 4, tmpTimer)
                SendCharacterUpdate(updateDataCount <> 0)

                Return True
            End If
        End Function
        Public Function TalkCanAccept(ByRef Quest As QuestInfo) As Boolean

            If Quest.RequiredRace <> 0 AndAlso (Quest.RequiredRace And (1 << (Race - 1))) = 0 Then
                Dim packet As New PacketClass(OPCODES.SMSG_QUESTGIVER_QUEST_INVALID)
                packet.AddInt32(QuestInvalidError.INVALIDREASON_DONT_HAVE_RACE)
                Client.Send(packet)
                packet.Dispose()
                Return False
            End If

            If Quest.RequiredClass <> 0 AndAlso (Quest.RequiredClass And (1 << (Classe - 1))) = 0 Then
                'TODO: Find constant for INVALIDREASON_DONT_HAVE_CLASS if exists
                Dim packet As New PacketClass(OPCODES.SMSG_QUESTGIVER_QUEST_INVALID)
                packet.AddInt32(QuestInvalidError.INVALIDREASON_DONT_HAVE_REQ)
                Client.Send(packet)
                packet.Dispose()
                Return False
            End If

            If Quest.RequiredTradeSkill <> 0 AndAlso Not Skills.ContainsKey(Quest.RequiredTradeSkill) Then
                'TODO: Find constant for INVALIDREASON_DONT_HAVE_SKILL if exists
                Dim packet As New PacketClass(OPCODES.SMSG_QUESTGIVER_QUEST_INVALID)
                packet.AddInt32(QuestInvalidError.INVALIDREASON_DONT_HAVE_REQ)
                Client.Send(packet)
                packet.Dispose()
                Return False
            End If

            'TODO: Check requirements for reputation
            'TODO: Check requirements for honor?

            Return True
        End Function
        Public Function IsQuestCompleted(ByVal QuestID As Integer) As Boolean
            Dim q As New DataTable
            Database.Query(String.Format("SELECT quest_id FROM characters_quests WHERE char_guid = {0} AND quest_status = -1 AND quest_id = {1};", GUID, QuestID), q)

            Return q.Rows.Count <> 0
        End Function
        Public Function IsQuestInProgress(ByVal QuestID As Integer) As Boolean
            Dim i As Integer
            For i = 0 To QUEST_SLOTS
                If Not TalkQuests(i) Is Nothing Then
                    If TalkQuests(i).ID = QuestID Then Return True
                End If
            Next

            Return False
        End Function

        'Helper Funtions
        Public Sub LogXPGain(ByVal Ammount As Integer, Optional ByVal Bonus As Integer = 0, Optional ByVal RestXP As Integer = 0, Optional ByVal VictimGUID As ULong = 0, Optional ByVal Silent As Boolean = 1)

            Dim SMSG_LOG_XPGAIN As New PacketClass(OPCODES.SMSG_LOG_XPGAIN)
            SMSG_LOG_XPGAIN.AddUInt64(VictimGUID)
            SMSG_LOG_XPGAIN.AddInt32(Ammount)

            If Silent Then
                SMSG_LOG_XPGAIN.AddInt8(1)
            Else
                'TODO: Test
                SMSG_LOG_XPGAIN.AddInt8(0)
                If Bonus > 0 Then
                    SMSG_LOG_XPGAIN.AddInt32(Bonus)
                    SMSG_LOG_XPGAIN.AddInt32(0)
                Else
                    SMSG_LOG_XPGAIN.AddInt32(RestXP)
                    SMSG_LOG_XPGAIN.AddInt32(&H803F)
                End If
            End If

            Client.Send(SMSG_LOG_XPGAIN)
            SMSG_LOG_XPGAIN.Dispose()
        End Sub
        Public Sub LogHonorGain(ByVal Ammount As Integer, Optional ByVal VictimGUID As ULong = 0, Optional ByVal VictimRANK As Byte = 0)
            Dim SMSG_PVP_CREDIT As New PacketClass(OPCODES.SMSG_PVP_CREDIT)
            SMSG_PVP_CREDIT.AddInt32(Ammount)
            SMSG_PVP_CREDIT.AddUInt64(VictimGUID)
            SMSG_PVP_CREDIT.AddInt32(VictimRANK)
            Client.Send(SMSG_PVP_CREDIT)
            SMSG_PVP_CREDIT.Dispose()
        End Sub
        Public Sub LogLootItem(ByVal Item As ItemObject, ByVal ItemCount As Byte, ByVal Recieved As Boolean, ByVal Created As Boolean)
            Dim response As New PacketClass(OPCODES.SMSG_ITEM_PUSH_RESULT)
            response.AddUInt64(GUID)
            response.AddInt32(Recieved) '0 = Looted, 1 = From NPC?
            response.AddInt32(Created) '0 = Recieved, 1 = Created
            response.AddInt32(1) 'Unk, always 1
            response.AddInt8(Item.GetBagSlot)
            If Item.StackCount = ItemCount Then
                response.AddInt32(Item.GetSlot) 'Item Slot (When added to stack: 0xFFFFFFFF)
            Else 'Added to stack
                response.AddInt32(&HFFFFFFFF)
            End If
            response.AddInt32(Item.ItemEntry)
            response.AddInt32(Item.SuffixFactor)
            response.AddInt32(Item.RandomProperties)
            response.AddInt32(ItemCount) 'Count of items
            response.AddInt32(Me.ItemCOUNT(Item.ItemEntry)) 'Count of items in inventory
            Client.SendMultiplyPackets(response)
            If IsInGroup Then Group.Broadcast(response)
            response.Dispose()
        End Sub
        Public Sub LogEnvironmentalDamage(ByVal dmgType As DamageTypes, ByVal Damage As Integer)
            Dim SMSG_ENVIRONMENTALDAMAGELOG As New PacketClass(OPCODES.SMSG_ENVIRONMENTALDAMAGELOG)
            SMSG_ENVIRONMENTALDAMAGELOG.AddUInt64(GUID)
            SMSG_ENVIRONMENTALDAMAGELOG.AddInt8(dmgType)
            SMSG_ENVIRONMENTALDAMAGELOG.AddInt32(Damage)
            SMSG_ENVIRONMENTALDAMAGELOG.AddInt32(0)
            SMSG_ENVIRONMENTALDAMAGELOG.AddInt32(0)

            Client.SendMultiplyPackets(SMSG_ENVIRONMENTALDAMAGELOG)
            SendToNearPlayers(SMSG_ENVIRONMENTALDAMAGELOG)
            SMSG_ENVIRONMENTALDAMAGELOG.Dispose()
        End Sub
        Public ReadOnly Property Side() As Boolean
            Get
                Select Case Race
                    Case Races.RACE_DWARF, Races.RACE_GNOME, Races.RACE_HUMAN, Races.RACE_NIGHT_ELF, Races.RACE_DRAENEI
                        Return False
                    Case Else
                        Return True
                End Select
            End Get
        End Property
    End Class

    Public Enum MirrorTimer As Byte
        FIRE = 5
        SLIME = 4
        LAVA = 3
        FALLING = 2
        DROWNING = 1
        FATIGUE = 0
    End Enum

#End Region

#Region "WS.CharMangment.Handlers"


    Public Sub On_CMSG_LFM_SET_AUTOFILL(ByRef packet As PacketClass, ByRef Client As ClientClass)
        'Unsure how this works
    End Sub
    Public Sub On_CMSG_LFG_SET_AUTOJOIN(ByRef packet As PacketClass, ByRef Client As ClientClass)
        'Unsure how this works
    End Sub

    Public Sub On_CMSG_SET_ACTION_BUTTON(ByRef packet As PacketClass, ByRef Client As ClientClass)
        If (packet.Data.Length - 1) < 10 Then Exit Sub
        packet.GetInt16()
        Dim button As Byte = packet.GetInt8(6)
        Dim action As UShort = packet.GetUInt16(7)
        Dim actionMisc As Byte = packet.GetInt8(9)
        Dim actionType As Byte = packet.GetInt8(10)

        If action = 0 Then
            Log.WriteLine(LogType.DEBUG, "[{0}:{1}] MSG_SET_ACTION_BUTTON [Remove action from button {2}]", Client.IP, Client.Port, button)
            Client.Character.ActionButtons.Remove(button)
        ElseIf actionType = 64 Then
            Log.WriteLine(LogType.DEBUG, "[{0}:{1}] CMSG_SET_ACTION_BUTTON [Added Macro {2} into button {3}]", Client.IP, Client.Port, action, button)
        ElseIf actionType = 128 Then
            Log.WriteLine(LogType.DEBUG, "[{0}:{1}] CMSG_SET_ACTION_BUTTON [Added Item {2} into button {3}]", Client.IP, Client.Port, action, button)
        Else
            Log.WriteLine(LogType.DEBUG, "[{0}:{1}] CMSG_SET_ACTION_BUTTON [Added Action {2}:{4}:{5} into button {3}]", Client.IP, Client.Port, action, button, actionType, actionMisc)
        End If
        Client.Character.ActionButtons(button) = New TActionButton(action, actionType, actionMisc)
    End Sub

    Public Enum LogoutResponseCode As Byte
        LOGOUT_RESPONSE_ACCEPTED = &H0
        LOGOUT_RESPONSE_DENIED = &HC
    End Enum
    Public Sub On_CMSG_LOGOUT_REQUEST(ByRef packet As PacketClass, ByRef Client As ClientClass)
        Log.WriteLine(LogType.DEBUG, "[{0}:{1}] CMSG_LOGOUT_REQUEST", Client.IP, Client.Port)
        Client.Character.Save()

        'TODO: Lose Invisibility
        'TODO: Is In Combat?

        If Not Client.Character.positionZ > (GetZCoord(Client.Character.positionX, Client.Character.positionY, Client.Character.positionZ, Client.Character.MapID) + 10) Then
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
            Client.SendMultiplyPackets(SMSG_UPDATE_OBJECT)
            Client.Character.SendToNearPlayers(SMSG_UPDATE_OBJECT)
            SMSG_UPDATE_OBJECT.Dispose()

            Dim packetACK As New PacketClass(OPCODES.SMSG_STANDSTATE_UPDATE)
            packetACK.AddInt8(StandStates.STANDSTATE_SIT)
            Client.Send(packetACK)
            packetACK.Dispose()
        End If




        'DONE: Let the client to exit
        Dim SMSG_LOGOUT_RESPONSE As New PacketClass(OPCODES.SMSG_LOGOUT_RESPONSE)
        SMSG_LOGOUT_RESPONSE.AddInt32(0)
        SMSG_LOGOUT_RESPONSE.AddInt8(LogoutResponseCode.LOGOUT_RESPONSE_ACCEPTED)     'Logout Accepted
        Client.Send(SMSG_LOGOUT_RESPONSE)
        SMSG_LOGOUT_RESPONSE.Dispose()
        Log.WriteLine(LogType.DEBUG, "[{0}:{1}] SMSG_LOGOUT_RESPONSE", Client.IP, Client.Port)

        'DONE: While logout, the player can't move
        Client.Character.SetMoveRoot()

        Client.Character.LogoutTimer = New Threading.Timer(AddressOf Client.Character.Logout, Nothing, 20000, Timeout.Infinite)
    End Sub
    Public Sub On_CMSG_LOGOUT_CANCEL(ByRef packet As PacketClass, ByRef Client As ClientClass)
        Try
            Log.WriteLine(LogType.DEBUG, "[{0}:{1}] CMSG_LOGOUT_CANCEL", Client.IP, Client.Port)
            If Client Is Nothing Then Exit Sub
            If Client.Character Is Nothing Then Exit Sub
            If Client.Character.LogoutTimer Is Nothing Then Exit Sub
            Try
                Client.Character.LogoutTimer.Dispose()
                Client.Character.LogoutTimer = Nothing
            Catch
            End Try



            'DONE: Initialize packet
            Dim UpdateData As New UpdateClass
            Dim SMSG_UPDATE_OBJECT As New PacketClass(OPCODES.SMSG_UPDATE_OBJECT)
            SMSG_UPDATE_OBJECT.AddInt32(1)      'Operations.Count
            SMSG_UPDATE_OBJECT.AddInt8(0)

            'DONE: Enable turn
            Client.Character.cUnitFlags = Client.Character.cUnitFlags And (Not UnitFlags.UNIT_FLAG_STUNTED)
            UpdateData.SetUpdateFlag(EUnitFields.UNIT_FIELD_FLAGS, Client.Character.cUnitFlags)
            'DONE: StandState -> Stand
            Client.Character.StandState = StandStates.STANDSTATE_STAND
            UpdateData.SetUpdateFlag(EUnitFields.UNIT_FIELD_BYTES_1, Client.Character.cBytes1)

            'DONE: Send packet
            UpdateData.AddToPacket(SMSG_UPDATE_OBJECT, ObjectUpdateType.UPDATETYPE_VALUES, CType(Client.Character, CharacterObject), 1)
            Client.Send(SMSG_UPDATE_OBJECT)
            SMSG_UPDATE_OBJECT.Dispose()

            Dim packetACK As New PacketClass(OPCODES.SMSG_STANDSTATE_UPDATE)
            packetACK.AddInt8(StandStates.STANDSTATE_STAND)
            Client.Send(packetACK)
            packetACK.Dispose()



            'DONE: Stop client logout
            Dim SMSG_LOGOUT_CANCEL_ACK As New PacketClass(OPCODES.SMSG_LOGOUT_CANCEL_ACK)
            Client.Send(SMSG_LOGOUT_CANCEL_ACK)
            SMSG_LOGOUT_CANCEL_ACK.Dispose()
            Log.WriteLine(LogType.DEBUG, "[{0}:{1}] SMSG_LOGOUT_CANCEL_ACK", Client.IP, Client.Port)

            'DONE: Enable moving
            Client.Character.SetMoveUnroot()
        Catch e As Exception
            Log.WriteLine(LogType.CRITICAL, "Error while trying to cancel logout.{0}", vbNewLine & e.ToString)
        End Try
    End Sub

    Public Sub On_CMSG_STANDSTATECHANGE(ByRef packet As PacketClass, ByRef Client As ClientClass)
        If (packet.Data.Length - 1) < 6 Then Exit Sub
        packet.GetInt16()

        Dim StandState As Byte = packet.GetInt8

        If StandState = StandStates.STANDSTATE_STAND Then
            Client.Character.RemoveAurasByInterruptFlag(SpellAuraInterruptFlags.AURA_INTERRUPT_FLAG_NOT_SEATED)
        End If

        Client.Character.StandState = StandState
        Client.Character.SetUpdateFlag(EUnitFields.UNIT_FIELD_BYTES_1, Client.Character.cBytes1)
        Client.Character.SendCharacterUpdate()

        Dim packetACK As New PacketClass(OPCODES.SMSG_STANDSTATE_UPDATE)
        packetACK.AddInt8(StandState)
        Client.Send(packetACK)
        packetACK.Dispose()

        Log.WriteLine(LogType.DEBUG, "[{0}:{1}] CMSG_STANDSTATECHANGE [{2}]", Client.IP, Client.Port, Client.Character.StandState)
    End Sub
    Public Sub On_CMSG_CANCEL_MOUNT_AURA(ByRef packet As PacketClass, ByRef Client As ClientClass)
        Client.Character.RemoveAurasOfType(78) 'Remove all mounted spells
    End Sub

    Public Function CanUseAmmo(ByRef c As CharacterObject, ByVal AmmoID As Integer) As InventoryChangeFailure
        If c.DEAD Then Return InventoryChangeFailure.EQUIP_ERR_YOU_ARE_DEAD
        If ITEMDatabase.ContainsKey(AmmoID) = False Then Return InventoryChangeFailure.EQUIP_ERR_ITEM_NOT_FOUND
        If ITEMDatabase(AmmoID).InventoryType <> INVENTORY_TYPES.INVTYPE_AMMO Then Return InventoryChangeFailure.EQUIP_ERR_ONLY_AMMO_CAN_GO_HERE
        If ITEMDatabase(AmmoID).AvailableClasses <> 0 AndAlso (ITEMDatabase(AmmoID).AvailableClasses And c.ClassMask) = 0 Then Return InventoryChangeFailure.EQUIP_ERR_YOU_CAN_NEVER_USE_THAT_ITEM
        If ITEMDatabase(AmmoID).AvailableRaces <> 0 AndAlso (ITEMDatabase(AmmoID).AvailableRaces And c.RaceMask) = 0 Then Return InventoryChangeFailure.EQUIP_ERR_YOU_CAN_NEVER_USE_THAT_ITEM

        If ITEMDatabase(AmmoID).ReqSkill <> 0 Then
            If c.HaveSkill(ITEMDatabase(AmmoID).ReqSkill) = False Then Return InventoryChangeFailure.EQUIP_ERR_NO_REQUIRED_PROFICIENCY
            If c.HaveSkill(ITEMDatabase(AmmoID).ReqSkill, ITEMDatabase(AmmoID).ReqSkillRank) = False Then Return InventoryChangeFailure.EQUIP_ERR_ERR_CANT_EQUIP_SKILL
        End If
        If ITEMDatabase(AmmoID).ReqSpell <> 0 Then
            If c.HaveSpell(ITEMDatabase(AmmoID).ReqSpell) = False Then Return InventoryChangeFailure.EQUIP_ERR_NO_REQUIRED_PROFICIENCY
        End If
        If ITEMDatabase(AmmoID).ReqLevel > c.Level Then Return InventoryChangeFailure.EQUIP_ERR_CANT_EQUIP_LEVEL_I
        If c.HavePassiveAura(46699) Then Return InventoryChangeFailure.EQUIP_ERR_BAG_FULL6 'Required no ammoe

        Return InventoryChangeFailure.EQUIP_ERR_OK
    End Function

    Public Function CheckAmmoCompatibility(ByRef c As CharacterObject, ByVal AmmoID As Integer) As Boolean
        If ITEMDatabase.ContainsKey(AmmoID) = False Then Return False
        If c.Items.ContainsKey(EQUIPMENT_SLOT_RANGED) = False OrElse c.Items(EQUIPMENT_SLOT_RANGED).IsBroken Then Return False
        If c.Items(EQUIPMENT_SLOT_RANGED).ItemInfo.ObjectClass <> ITEM_CLASS.ITEM_CLASS_WEAPON Then Return False

        Select Case c.Items(EQUIPMENT_SLOT_RANGED).ItemInfo.SubClass
            Case ITEM_SUBCLASS.ITEM_SUBCLASS_BOW, ITEM_SUBCLASS.ITEM_SUBCLASS_CROSSBOW
                If ITEMDatabase(AmmoID).SubClass <> ITEM_SUBCLASS.ITEM_SUBCLASS_ARROW Then Return False
            Case ITEM_SUBCLASS.ITEM_SUBCLASS_GUN
                If ITEMDatabase(AmmoID).SubClass <> ITEM_SUBCLASS.ITEM_SUBCLASS_BULLET Then Return False
            Case Else
                Return False
        End Select

        Return True
    End Function

    Public Sub On_CMSG_SET_AMMO(ByRef packet As PacketClass, ByRef Client As ClientClass)
        If (packet.Data.Length - 1) < 9 Then Exit Sub
        packet.GetInt16()
        Dim AmmoID As Integer = packet.GetInt32
        If Client.Character.isDead Then
            SendInventoryChangeFailure(Client.Character, InventoryChangeFailure.EQUIP_ERR_YOU_ARE_DEAD, 0, 0)
            Exit Sub
        End If

        'TODO: Check if the client even can use that ammo

        Client.Character.AmmoID = AmmoID
        If AmmoID Then 'Set Ammo
            If ITEMDatabase.ContainsKey(AmmoID) = False Then Dim tmpItem As ItemInfo = New ItemInfo(AmmoID)
            Dim CanUse As InventoryChangeFailure = CanUseAmmo(Client.Character, AmmoID)
            If CanUse <> InventoryChangeFailure.EQUIP_ERR_OK Then
                SendInventoryChangeFailure(Client.Character, CanUse, 0, 0)
                Exit Sub
            End If
            Dim currentDPS As Single = 0
            If ITEMDatabase.ContainsKey(AmmoID) = True AndAlso ITEMDatabase(AmmoID).ObjectClass = ITEM_CLASS.ITEM_CLASS_PROJECTILE OrElse CheckAmmoCompatibility(Client.Character, AmmoID) Then
                currentDPS = ITEMDatabase(AmmoID).Damage(0).Minimum
            End If
            If Client.Character.AmmoDPS <> currentDPS Then
                Client.Character.AmmoDPS = currentDPS
                CalculateMinMaxDamage(Client.Character, WeaponAttackType.RANGED_ATTACK)

                Client.Character.SetUpdateFlag(EUnitFields.UNIT_FIELD_MINRANGEDDAMAGE, Client.Character.RangedDamage.Minimum)
                Client.Character.SetUpdateFlag(EUnitFields.UNIT_FIELD_MAXRANGEDDAMAGE, Client.Character.RangedDamage.Maximum)
            End If
            'TODO: Change the ranged damage
            Client.Character.AmmoID = AmmoID
            Client.Character.SetUpdateFlag(EPlayerFields.PLAYER_AMMO_ID, Client.Character.AmmoID)
            Client.Character.SendCharacterUpdate(False)
        Else 'Remove Ammo
            If Client.Character.AmmoID Then
                'TODO: Change the ranged damage
                Client.Character.AmmoID = 0
                Client.Character.SetUpdateFlag(EPlayerFields.PLAYER_AMMO_ID, 0)
                Client.Character.SendCharacterUpdate(False)
            End If
        End If
    End Sub
    Public Sub On_CMSG_SET_WATCHED_FACTION(ByRef packet As PacketClass, ByRef Client As ClientClass)
        If (packet.Data.Length - 1) < 6 Then Exit Sub
        packet.GetInt16()
        Client.Character.WatchedFactionIndex = packet.GetInt8()

        Log.WriteLine(LogType.DEBUG, "[{0}:{1}] CMSG_SET_WATCHED_FACTION [{2}]", Client.IP, Client.Port, Client.Character.WatchedFactionIndex)

        Database.Update(String.Format("UPDATE characters SET char_watchedFactionIndex = {0} WHERE char_guid = {1};", Client.Character.WatchedFactionIndex, Client.Character.GUID - GUID_PLAYER))
        Client.Character.SetUpdateFlag(EPlayerFields.PLAYER_FIELD_WATCHED_FACTION_INDEX, CType(Client.Character.WatchedFactionIndex, Integer))
        Client.Character.SendCharacterUpdate(False)
    End Sub
    Public Sub On_CMSG_SET_TITLE(ByRef packet As PacketClass, ByRef Client As ClientClass)
        If (packet.Data.Length - 1) < 9 Then Exit Sub
        packet.GetInt16()
        Client.Character.HonorTitle = packet.GetInt32()

        Log.WriteLine(LogType.DEBUG, "[{0}:{1}] CMSG_SET_TITLE [{2}]", Client.IP, Client.Port, Client.Character.HonorTitle)

        Client.Character.SetUpdateFlag(EPlayerFields.PLAYER_CHOSEN_TITLE, CType(Client.Character.HonorTitle, Integer))
        Client.Character.SendCharacterUpdate(False)
    End Sub


#End Region

#Region "WS.CharMangment.CreateCharacter"

    Public Function CreateCharacter(ByVal Account As String, ByVal Name As String, ByVal Race As Byte, ByVal Classe As Byte, ByVal Gender As Byte, ByVal Skin As Byte, ByVal Face As Byte, ByVal HairStyle As Byte, ByVal HairColor As Byte, ByVal FacialHair As Byte, ByVal OutfitID As Byte) As Integer
        Dim Character As New CharacterObject
        Dim MySQLQuery As New DataTable


        'DONE: Make name capitalized as on official
        Character.Name = CapitalizeName(Name)
        Character.Race = Race
        Character.Classe = Classe
        Character.Gender = Gender
        Character.Skin = Skin
        Character.Face = Face
        Character.HairStyle = HairStyle
        Character.HairColor = HairColor
        Character.FacialHair = FacialHair
        Character.OutfitId = OutfitID


        'DONE: Query Access Level and Account ID
        Database.Query(String.Format("SELECT account_id, plevel, expansion FROM accounts WHERE account = ""{0}"";", Account), MySQLQuery)
        Dim Account_ID As Integer = CType(MySQLQuery.Rows(0).Item("account_id"), Integer)
        Dim Account_Access As AccessLevel = CType(MySQLQuery.Rows(0).Item("plevel"), AccessLevel)
        Dim Account_Expansion As ExpansionLevel = CType(MySQLQuery.Rows(0).Item("expansion"), AccessLevel)
        Character.Access = Account_Access

        If Not ValidateName(Character.Name) Then
            Return AuthResponseCodes.CHAR_NAME_FAILURE
        End If

        'DONE: Name In Use
        Try
            MySQLQuery.Clear()
            Database.Query(String.Format("SELECT char_name FROM characters WHERE char_name = ""{0}"";", Character.Name), MySQLQuery)
            If MySQLQuery.Rows.Count > 0 Then
                Return AuthResponseCodes.CHAR_CREATE_NAME_IN_USE
            End If
        Catch
            Return AuthResponseCodes.CHAR_CREATE_FAILED
        End Try

        'DONE: Can't create character named as the bot
        If UCase(Character.Name) = UCase(ServerNAME) Then
            Return AuthResponseCodes.CHAR_CREATE_NAME_IN_USE
        End If

        'DONE: Disbale Draenei and Blood Elf races if don't have expansion
        If Account_Expansion = ExpansionLevel.NORMAL AndAlso (Race = Races.RACE_DRAENEI OrElse Race = Races.RACE_BLOOD_ELF) Then
            Return AuthResponseCodes.CHAR_CREATE_EXPANSION
        End If

        'DONE: Disbale Dark Knight if don't have expansion
        If (Account_Expansion = ExpansionLevel.NORMAL OrElse Account_Expansion = ExpansionLevel.EXPANSION_1) AndAlso (Classe = Classes.CLASS_DARK_KNIGHT) Then
            Return AuthResponseCodes.CHAR_CREATE_EXPANSION
        End If

        'DONE: Check for disabled class/race, only for non GM/Admin
        If (SERVER_CONFIG_DISABLED_CLASSES(Character.Classe - 1) = True) OrElse (SERVER_CONFIG_DISABLED_RACES(Character.Race - 1) = True) AndAlso Account_Access < AccessLevel.GameMaster Then
            Return AuthResponseCodes.CHAR_CREATE_DISABLED
        End If

        'DONE: Check for both horde and alliance
        If Account_Access <= AccessLevel.PlayerVip Then
            MySQLQuery.Clear()
            Database.Query(String.Format("SELECT char_race FROM characters WHERE account_id = ""{0}"" LIMIT 1;", Account_ID), MySQLQuery)
            If MySQLQuery.Rows.Count > 0 Then
                If Character.Side <> GetCharacterSide(CByte(MySQLQuery.Rows(0).Item("char_race"))) Then
                    Return AuthResponseCodes.CHAR_CREATE_PVP_TEAMS_VIOLATION
                End If
            End If
        End If

        'DONE: Check for MAX characters limit, only for non GM/Admin
        MySQLQuery.Clear()
        Database.Query(String.Format("SELECT char_name FROM characters WHERE account_id = ""{0}"";", Account_ID), MySQLQuery)
        If MySQLQuery.Rows.Count >= 10 AndAlso Account_Access <= AccessLevel.PlayerVip Then
            Return AuthResponseCodes.CHAR_CREATE_SERVER_LIMIT
        End If

        'DONE: Generate GUID, MySQL Auto generation
        'DONE: Create Char
        Try
            InitializeReputations(Character)
            'CharacterCreation.Invoke("CharacterCreation", "StartRace", New Object() {Character})
            'CharacterCreation.Invoke("CharacterCreation", "StartClass", New Object() {Character})
            'CharacterCreation.Invoke("CharacterCreation", "StartOther", New Object() {Character})
            CreateCharacter(Character)
            'CharacterCreation.Invoke("CharacterCreation", "CreateCharacter", New Object() {Character})
            Character.SaveAsNewCharacter(Account_ID)
            CreateCharacterItems(Character)
            'CharacterCreation.Invoke("CharacterCreation", "StartItems", New Object() {Character})

            'Log.WriteLine(LogType.DEBUG, "[{0}:{1}] SMSG_CHAR_CREATE [{2}]", Client.IP, Client.Port, Character.Name)
        Catch err As Exception
            Log.WriteLine(LogType.FAILED, "Error initializing character!{0}{1}", vbNewLine, err.ToString)
            Return AuthResponseCodes.CHAR_CREATE_FAILED
        Finally
            Character.Dispose()
        End Try

        Return AuthResponseCodes.CHAR_CREATE_SUCCESS
    End Function
    Public Sub CreateCharacter(ByRef c As CharacterObject)
        Dim CreateInfo As New DataTable
        Dim CreateInfoBars As New DataTable
        Dim CreateInfoSkills As New DataTable
        Dim CreateInfoSpells As New DataTable

        Dim ButtonPos As Integer = 0

        Database.Query(String.Format("SELECT * FROM playercreateinfo WHERE race = {0} AND class = {1};", CType(c.Race, Integer), CType(c.Classe, Integer)), CreateInfo)
        If CreateInfo.Rows.Count <= 0 Then
            Log.WriteLine(LogType.FAILED, "No information found in playercreateinfo table for race={0}, class={1}", c.Race, c.Classe)
        End If
        Dim CreateIndex As Integer = CreateInfo.Rows(0).Item("Index")

        Database.Query(String.Format("SELECT * FROM playercreateinfo_bars WHERE race = {0} AND class = {1} ORDER BY button;", CType(c.Race, Integer), CType(c.Classe, Integer)), CreateInfoBars)
        If CreateInfoBars.Rows.Count <= 0 Then
            Log.WriteLine(LogType.FAILED, "No information found in playercreateinfo_bars table for race={0}, class={1}", c.Race, c.Classe)
        End If
        Database.Query(String.Format("SELECT * FROM playercreateinfo_skills WHERE indexid = {0};", CreateIndex), CreateInfoSkills)
        If CreateInfoSkills.Rows.Count <= 0 Then
            Log.WriteLine(LogType.FAILED, "No information found in playercreateinfo_skills table for Index={0}", CreateIndex)
        End If
        Database.Query(String.Format("SELECT * FROM playercreateinfo_spells WHERE indexid = {0};", CreateIndex), CreateInfoSpells)
        If CreateInfoSpells.Rows.Count <= 0 Then
            Log.WriteLine(LogType.FAILED, "No information found in playercreateinfo_spells table for Index={0}", CreateIndex)
        End If

        ' Initialize Character Variables
        c.Copper = 0
        c.XP = 0
        c.Size = 1.0F
        c.Life.Base = 0
        c.Life.Current = 0
        c.Mana.Base = 0
        c.Mana.Current = 0
        c.Rage.Current = 0
        c.Rage.Base = 0
        c.Energy.Current = 0
        c.Energy.Base = 0
        c.ManaType = GetClassManaType(c.Classe)

        ' Set Character Create Information
        c.Model = GetRaceModel(c.Race, c.Gender)

        c.Faction = CreateInfo.Rows(0).Item("factiontemplate")
        c.MapID = CreateInfo.Rows(0).Item("mapID")
        c.ZoneID = CreateInfo.Rows(0).Item("zoneID")
        c.positionX = CreateInfo.Rows(0).Item("positionX")
        c.positionY = CreateInfo.Rows(0).Item("positionY")
        c.positionZ = CreateInfo.Rows(0).Item("positionZ")
        c.bindpoint_map_id = c.MapID
        c.bindpoint_zone_id = c.ZoneID
        c.bindpoint_positionX = c.positionX
        c.bindpoint_positionY = c.positionY
        c.bindpoint_positionZ = c.positionZ
        c.Strength.Base = CreateInfo.Rows(0).Item("BaseStrength")
        c.Agility.Base = CreateInfo.Rows(0).Item("BaseAgility")
        c.Stamina.Base = CreateInfo.Rows(0).Item("BaseStamina")
        c.Intellect.Base = CreateInfo.Rows(0).Item("BaseIntellect")
        c.Spirit.Base = CreateInfo.Rows(0).Item("BaseSpirit")
        c.Life.Base = CreateInfo.Rows(0).Item("BaseHealth")
        c.Life.Current = c.Life.Maximum
        c.Mana.Base = CreateInfo.Rows(0).Item("BaseMana")
        c.Mana.Current = c.Mana.Maximum
        c.Rage.Base = CreateInfo.Rows(0).Item("BaseRage")
        c.Rage.Current = 0
        c.Energy.Base = CreateInfo.Rows(0).Item("BaseEnergy")
        c.Energy.Current = 0
        'c.AttackPower = CreateInfo.Rows(0).Item("attackpower")
        c.Damage.Minimum = CreateInfo.Rows(0).Item("mindmg")
        c.Damage.Maximum = CreateInfo.Rows(0).Item("maxdmg")

        ' Set Player Create Skills
        For Each SkillRow As DataRow In CreateInfoSkills.Rows
            c.LearnSkill(SkillRow.Item("skillid"), SkillRow.Item("level"), SkillRow.Item("maxlevel"))
        Next

        ' Set Player Create Spells
        For Each SpellRow As DataRow In CreateInfoSpells.Rows
            c.LearnSpell(SpellRow.Item("spellid"))
        Next

        ' Set Player Reputation
        Select Case c.Faction
            Case Factions.PLAYERDwarf, Factions.PLAYERGnome, Factions.PLAYERHuman, Factions.PLAYERNightElf, Factions.PLAYERDraenei
                c.InitializeReputation(Factions.Stormwind)
                c.InitializeReputation(Factions.GnomereganExiles)
                c.InitializeReputation(Factions.Darnassus)
                c.InitializeReputation(Factions.Ironforge)
                c.InitializeReputation(Factions.Exodar)

            Case Factions.PLAYEROrc, Factions.PLAYERTauren, Factions.PLAYERTroll, Factions.PLAYERUndead, Factions.PLAYERBloodElf
                c.InitializeReputation(Factions.Orgrimmar)
                c.InitializeReputation(Factions.Undercity)
                c.InitializeReputation(Factions.ThunderBluff)
                c.InitializeReputation(Factions.DarkspearTrolls)
                c.InitializeReputation(Factions.SilvermoonCity)

        End Select

        ' Set Player Taxi Zones (May have to change this in the future)
        Select Case c.Race
            Case Races.RACE_DWARF, Races.RACE_GNOME
                c.TaxiZones.Set(6, True)

            Case Races.RACE_HUMAN
                c.TaxiZones.Set(2, True)

            Case Races.RACE_NIGHT_ELF
                c.TaxiZones.Set(26, True)
                c.TaxiZones.Set(27, True)

            Case Races.RACE_ORC, Races.RACE_TROLL
                c.TaxiZones.Set(23, True)

            Case Races.RACE_TAUREN
                c.TaxiZones.Set(22, True)

            Case Races.RACE_UNDEAD
                c.TaxiZones.Set(11, True)

            Case Races.RACE_DRAENEI, Races.RACE_BLOOD_ELF
                ' Hmmmmm......

        End Select

        ' Set Player Create Action Buttons
        For Each BarRow As DataRow In CreateInfoBars.Rows
            If BarRow.Item("action") > 0 Then
                c.ActionButtons(ButtonPos) = New TActionButton(BarRow.Item("action"), BarRow.Item("type"), BarRow.Item("misc"))
                ButtonPos = ButtonPos + 1
            End If
        Next

    End Sub
    Public Sub CreateCharacterItems(ByRef c As CharacterObject)
        Dim CreateInfo As New DataTable
        Database.Query(String.Format("SELECT * FROM playercreateinfo WHERE race = {0} AND class = {1};", CType(c.Race, Integer), CType(c.Classe, Integer)), CreateInfo)
        If CreateInfo.Rows.Count <= 0 Then
            Log.WriteLine(LogType.FAILED, "No information found in playercreateinfo table for race={0}, class={1}", c.Race, c.Classe)
        End If
        Dim CreateIndex As Integer = CreateInfo.Rows(0).Item("Index")

        Dim CreateInfoItems As New DataTable
        Database.Query(String.Format("SELECT * FROM playercreateinfo_items WHERE indexid = {0};", CreateIndex), CreateInfoItems)
        If CreateInfoItems.Rows.Count <= 0 Then
            Log.WriteLine(LogType.FAILED, "No information found in playercreateinfo_bars table for Index={0}", CreateIndex)
        End If

        ' Set Player Create Items
        For Each ItemRow As DataRow In CreateInfoItems.Rows
            c.ItemADD(CType(ItemRow.Item("protoid"), Integer), CType(0, Byte), CType(ItemRow.Item("slotid"), Byte), CType(ItemRow.Item("amount"), Integer))
        Next

    End Sub

#End Region


End Module


