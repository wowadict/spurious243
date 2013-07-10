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

Imports System.Xml
Imports System.Reflection
Imports System.Threading
Imports System.Diagnostics.PerformanceCounter
Imports mangosVB.Common

Public Module WC_Stats

    'http://www.15seconds.com/issue/050615.htm

    Private ConnectionsHandled As Integer = 0
    Private ConnectionsPeak As Integer = 0
    Private ConnectionsCurrent As Integer = 0

    Public Sub ConnectionsIncrement()
        Interlocked.Increment(ConnectionsHandled)
        If Interlocked.Increment(ConnectionsCurrent) > ConnectionsPeak Then
            ConnectionsPeak = ConnectionsCurrent
        End If
    End Sub
    Public Sub ConnectionsDecrement()
        Interlocked.Decrement(ConnectionsCurrent)
    End Sub

    Public DataTransferOut As Long = 0
    Public DataTransferIn As Long = 0

    Private ThreadsWorker As Integer = 0
    Private ThreadsComletion As Integer = 0
    Private LastCheck As Date = Now
    Private LastCPUTime As Double = 0
    Private Uptime As TimeSpan
    Private Latency As Long = 0
    Private UsageCPU As Single = 0.0F
    Private UsageMemory As Long = 0

    Private CountPlayers As Integer = 0
    Private CountPlayersAlliance As Integer = 0
    Private CountPlayersHorde As Integer = 0
    Private CountGMs As Integer = 0

    Private w As New Dictionary(Of WorldInfo, List(Of String))

    Private Function FormatUptime(ByVal Time As TimeSpan) As String
        Return String.Format("{0}d {1}h {2}m {3}s {4}ms", Time.Days, Time.Hours, Time.Minutes, Time.Seconds, Time.Milliseconds)
    End Function

    Public Sub CheckCPU(ByVal State As Object)
        Dim TimeSinceLastCheck As TimeSpan = Now.Subtract(LastCheck)
        UsageCPU = ((Process.GetCurrentProcess().TotalProcessorTime.TotalMilliseconds - LastCPUTime) / TimeSinceLastCheck.TotalMilliseconds) * 100
        LastCheck = Now
        LastCPUTime = Process.GetCurrentProcess().TotalProcessorTime.TotalMilliseconds
    End Sub

    Private Sub PrepareStats()
        Uptime = Now.Subtract(Process.GetCurrentProcess().StartTime)

        ThreadPool.GetAvailableThreads(ThreadsWorker, ThreadsComletion)

        UsageMemory = Process.GetCurrentProcess().WorkingSet64 / (1024 * 1024)

        CountPlayers = 0
        CountPlayersHorde = 0
        CountPlayersAlliance = 0
        CountGMs = 0
        Latency = 0

        CHARACTERs_Lock.AcquireReaderLock(DEFAULT_LOCK_TIMEOUT)
        For Each c As KeyValuePair(Of ULong, CharacterObject) In CHARACTERs
            If c.Value.IsInWorld Then
                CountPlayers += 1

                If c.Value.Race = Races.RACE_BLOOD_ELF OrElse c.Value.Race = Races.RACE_ORC OrElse c.Value.Race = Races.RACE_TAUREN OrElse c.Value.Race = Races.RACE_TROLL OrElse c.Value.Race = Races.RACE_UNDEAD Then
                    CountPlayersHorde += 1
                Else
                    CountPlayersAlliance += 1
                End If

                If c.Value.Access > AccessLevel.Player Then CountGMs += 1
                Latency += c.Value.Latency
            End If
        Next
        CHARACTERs_Lock.ReleaseReaderLock()

        If CountPlayers > 1 Then
            Latency = Latency \ CountPlayers
        End If

        For Each c As KeyValuePair(Of UInteger, WorldInfo) In WS.WorldsInfo
            If Not w.ContainsKey(c.Value) Then
                w.Add(c.Value, New List(Of String))
            End If
            w(c.Value).Add(c.Key)
        Next
    End Sub
    Public Sub GenerateStats(ByVal State As Object)
        Log.WriteLine(BaseWriter.LogType.DEBUG, "Generating stats")
        PrepareStats()

        If IO.File.Exists(Config.StatsLocation) = False Then
            Log.WriteLine(BaseWriter.LogType.WARNING, "Can't find stat.xsl")
            Exit Sub
        End If

        Dim f As XmlWriter = XmlWriter.Create(Config.StatsLocation)
        f.WriteStartDocument(True)
        f.WriteComment("generated at " & DateTime.Now.ToString("hh:mm:ss"))
        '<?xml-stylesheet type="text/xsl" href="stats.xsl"?>
        f.WriteProcessingInstruction("xml-stylesheet", "type=""text/xsl"" href=""stats.xsl""")
        '<server>
        f.WriteStartElement("server")

        '<cluster>
        f.WriteStartElement("cluster")

        f.WriteStartElement("platform")
        f.WriteValue(String.Format("MaNGOSvb rev{0} v{1}", 0, [Assembly].GetExecutingAssembly().GetName().Version))
        f.WriteEndElement()

        f.WriteStartElement("uptime")
        f.WriteValue(FormatUptime(Uptime))
        f.WriteEndElement()

        f.WriteStartElement("onlineplayers")
        f.WriteValue(CountPlayers)
        f.WriteEndElement()

        f.WriteStartElement("gmcount")
        f.WriteValue(CountGMs)
        f.WriteEndElement()

        f.WriteStartElement("alliance")
        f.WriteValue(CountPlayersAlliance)
        f.WriteEndElement()

        f.WriteStartElement("horde")
        f.WriteValue(CountPlayersHorde)
        f.WriteEndElement()

        f.WriteStartElement("cpu")
        f.WriteValue(Format(UsageCPU, "0.00"))
        f.WriteEndElement()

        f.WriteStartElement("ram")
        f.WriteValue(UsageMemory)
        f.WriteEndElement()

        f.WriteStartElement("latency")
        f.WriteValue(Latency)
        f.WriteEndElement()

        f.WriteStartElement("connaccepted")
        f.WriteValue(ConnectionsHandled)
        f.WriteEndElement()

        f.WriteStartElement("connpeak")
        f.WriteValue(ConnectionsPeak)
        f.WriteEndElement()

        f.WriteStartElement("conncurrent")
        f.WriteValue(ConnectionsCurrent)
        f.WriteEndElement()

        f.WriteStartElement("networkin")
        f.WriteValue(DataTransferIn)
        f.WriteEndElement()

        f.WriteStartElement("networkout")
        f.WriteValue(DataTransferOut)
        f.WriteEndElement()

        f.WriteStartElement("threadsw")
        f.WriteValue(ThreadsWorker)
        f.WriteEndElement()

        f.WriteStartElement("threadsc")
        f.WriteValue(ThreadsComletion)
        f.WriteEndElement()

        f.WriteStartElement("lastupdate")
        f.WriteValue(Now.ToString)
        f.WriteEndElement()

        '</cluster>
        f.WriteEndElement()

        '<world>
        f.WriteStartElement("world")
        Try
            For Each c As KeyValuePair(Of WorldInfo, List(Of String)) In w
                f.WriteStartElement("instance")
                f.WriteStartElement("uptime")
                f.WriteValue(FormatUptime(Now - c.Key.Started))
                f.WriteEndElement()
                f.WriteStartElement("players")
                f.WriteValue("-")
                f.WriteEndElement()
                f.WriteStartElement("maps")
                f.WriteValue(Join(c.Value.ToArray, ", "))
                f.WriteEndElement()
                f.WriteStartElement("cpu")
                f.WriteValue(Format(c.Key.CPUUsage, "0.00"))
                f.WriteEndElement()
                f.WriteStartElement("ram")
                f.WriteValue(c.Key.MemoryUsage)
                f.WriteEndElement()
                f.WriteStartElement("latency")
                f.WriteValue(c.Key.Latency)
                f.WriteEndElement()

                f.WriteEndElement()
            Next
        Catch ex As Exception
            Log.WriteLine(BaseWriter.LogType.FAILED, "Error while generating stats file: {0}", ex.ToString)
        End Try
        '</world>
        f.WriteEndElement()

        CHARACTERs_Lock.AcquireReaderLock(DEFAULT_LOCK_TIMEOUT)

        f.WriteStartElement("users")
        For Each c As KeyValuePair(Of ULong, CharacterObject) In CHARACTERs
            If c.Value.IsInWorld AndAlso c.Value.Access >= AccessLevel.GameMaster Then
                f.WriteStartElement("gmplayer")
                f.WriteStartElement("name")
                f.WriteValue(c.Value.Name)
                f.WriteEndElement()
                f.WriteStartElement("access")
                f.WriteValue(c.Value.Access)
                f.WriteEndElement()
                f.WriteEndElement()
            End If
        Next
        f.WriteEndElement()

        f.WriteStartElement("sessions")
        For Each c As KeyValuePair(Of ULong, CharacterObject) In CHARACTERs
            If c.Value.IsInWorld Then
                f.WriteStartElement("player")
                f.WriteStartElement("name")
                f.WriteValue(c.Value.Name)
                f.WriteEndElement()
                f.WriteStartElement("race")
                f.WriteValue(c.Value.Race)
                f.WriteEndElement()
                f.WriteStartElement("class")
                f.WriteValue(c.Value.Classe)
                f.WriteEndElement()
                f.WriteStartElement("level")
                f.WriteValue(c.Value.Level)
                f.WriteEndElement()
                f.WriteStartElement("map")
                f.WriteValue(c.Value.Map)
                f.WriteEndElement()
                f.WriteStartElement("zone")
                f.WriteValue(c.Value.Zone)
                f.WriteEndElement()
                f.WriteStartElement("ontime")
                f.WriteValue(FormatUptime(Now - c.Value.Time))
                f.WriteEndElement()
                f.WriteStartElement("latency")
                f.WriteValue(c.Value.Latency)
                f.WriteEndElement()

                f.WriteEndElement()
            End If
        Next
        f.WriteEndElement()

        CHARACTERs_Lock.ReleaseReaderLock()

        '</server>
        f.WriteEndElement()
        f.WriteEndDocument()
        f.Close()

        w.Clear()
    End Sub

End Module