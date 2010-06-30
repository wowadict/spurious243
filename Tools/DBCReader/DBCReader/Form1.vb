Imports System.IO

Public Class Form1

    Private Sub cmdBrowse_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles cmdBrowse.Click
        Dim fdlg As OpenFileDialog = New OpenFileDialog()
        fdlg.Title = "Find your DBC File"
        fdlg.Filter = "All files (*.*)|*.*|All files (*.*)|*.*"
        fdlg.FilterIndex = 2
        fdlg.RestoreDirectory = True
        If fdlg.ShowDialog() = DialogResult.OK Then
            txtFile.Text = fdlg.FileName

            DBCData.Clear()
            Dim fs As New FileStream(fdlg.FileName, FileMode.Open, FileAccess.Read)
            Dim s As New BinaryReader(fs)
            s.BaseStream.Seek(0, SeekOrigin.Begin)
            Dim Buffer() As Byte = s.ReadBytes(FileLen(fdlg.FileName))
            HandleDBCData(Buffer)
            Buffer = Nothing
            s.Close()
        End If
    End Sub

    Private Sub HandleDBCData(ByVal Data() As Byte)
        Dim DBCType As String = Chr(Data(0)) & Chr(Data(1)) & Chr(Data(2)) & Chr(Data(3))
        Dim Rows As Integer = BitConverter.ToInt32(Data, 4)
        Dim Columns As Integer = BitConverter.ToInt32(Data, 8)
        Dim RowLength As Integer = BitConverter.ToInt32(Data, 12)
        Dim StringPartLength As Integer = BitConverter.ToInt32(Data, 16)

        If DBCType <> "WDBC" Then MsgBox("This file is not a DBC file.", MsgBoxStyle.Critical, "Error") : Exit Sub
        If Rows <= 0 Or Columns <= 0 Or RowLength <= 0 Then MsgBox("This file is not a DBC file.", MsgBoxStyle.Critical, "Error") : Exit Sub

        Dim i As Integer, j As Integer, tmpOffset As Integer
        Dim tmpStr(Columns - 1) As String, IsFloat As New Hashtable

        For i = 0 To Columns - 1
            Dim tmpColumn As New System.Windows.Forms.ColumnHeader
            tmpColumn.Text = CStr(i)
            tmpColumn.Width = 90
            DBCData.Columns.Add(tmpColumn)
        Next

        'Check if any column uses floats instead of uint32's
        'Code below doesn't work at the moment, flags are in some cases counted as floats
        'Dim tmpSng As Single, tmpString As String
        'For i = 0 To Rows - 1
        '   For j = 0 To Columns - 1
        '       If IsFloat.ContainsKey(j) = False Then
        '           tmpOffset = 20 + i * RowLength + j * 4
        '           tmpSng = BitConverter.ToSingle(Data, tmpOffset)
        '           If tmpSng > 1 And tmpSng < 50000 Then 'Only allow floats to be between 0 and 50000
        '               tmpString = CStr(CSng(tmpSng - CInt(tmpSng))) 'Make sure we only get the decimals + 0.
        '               If tmpString.Length > 2 And tmpString.Length < 8 Then 'Only allow a minimum of 1 decimal and a maximum of 5 decimals
        '                   IsFloat.Add(j, True)
        '               End If
        '           End If
        '       End If
        '    Next
        'Next

        For i = 0 To Rows - 1
            For j = 0 To Columns - 1
                tmpOffset = 20 + i * RowLength + j * 4
                If IsFloat.ContainsKey(i) Then
                    tmpStr(j) = CStr(BitConverter.ToSingle(Data, tmpOffset))
                Else
                    tmpStr(j) = CStr(BitConverter.ToInt32(Data, tmpOffset))
                End If
            Next

            With DBCData.Items.Add(tmpStr(0))
                If Columns > 1 Then
                    For j = 1 To Columns - 1
                        .SubItems.Add(tmpStr(j))
                    Next j
                End If
            End With

            ProgressBar.Value = CInt(((i + 1) / Rows) * 100)
        Next

        ProgressBar.Value = 0
    End Sub

    Private Sub Form1_Resize(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Resize
        txtFile.Width = Me.Width - 108
        cmdBrowse.Left = Me.Width - 100
        DBCData.Width = Me.Width - 13
        DBCData.Height = Me.Height - 89
        ProgressBar.Width = Me.Width - 13
        ProgressBar.Top = Me.Height - 55
    End Sub

    Private Sub DBCData_ColumnClick(ByVal sender As Object, ByVal e As System.Windows.Forms.ColumnClickEventArgs) Handles DBCData.ColumnClick
        Dim i As Integer, tmpInt As Integer, tmpSng As Single, Buffer(3) As Byte
        If DBCData.Items.Count = 0 Then Exit Sub
        For i = 0 To DBCData.Items.Count - 1
            If DBCData.Items(i).SubItems(e.Column).Tag = "1" Then
                tmpSng = CSng(DBCData.Items(i).SubItems(e.Column).Text)
                DBCData.Items(i).SubItems(e.Column).Tag = "0"
                Buffer = BitConverter.GetBytes(tmpSng)
                DBCData.Items(i).SubItems(e.Column).Text = BitConverter.ToInt32(Buffer, 0)
            Else
                tmpInt = CInt(DBCData.Items(i).SubItems(e.Column).Text)
                DBCData.Items(i).SubItems(e.Column).Tag = "1"
                Buffer = BitConverter.GetBytes(tmpInt)
                DBCData.Items(i).SubItems(e.Column).Text = BitConverter.ToSingle(Buffer, 0)
            End If

            ProgressBar.Value = CInt(((i + 1) / DBCData.Items.Count) * 100)
        Next i

        ProgressBar.Value = 0
    End Sub
End Class
