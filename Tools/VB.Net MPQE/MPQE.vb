Imports System
Imports System.IO
Imports System.Collections
Imports VB.Net_MPQE.SFmpqapi

Namespace MPQE
    Class MPQE

        Private version As String = "1.4"
        Private hMPQ As Integer = 0
        Private hFile As Integer = 0
        Private FileSize As Integer = 0
        Private FileRead As Integer = 0

        Private option_usepatchfiles As Boolean = False
        '	private bool option_listonly = false;
        Private option_verbose As Boolean = False
        Private option_outdir As String = "MPQOUT"
        Private option_lowercase As Boolean = False
        Private option_baseMPQ As String
        Private option_searchglob As String

        Private extractedFiles As New ArrayList()

        <STAThread()> _
        Public Shared Sub Main(ByVal args As String())
            Dim mpqe As New MPQE()
            Console.Write("MPQ-Extractor v{0} by WRS - Modified and Converted to VB.Net by Trippy", mpqe.version)
            Try
                Console.WriteLine(" powered by {0}", SFmpq.MpqGetVersionString())
            Catch e As Exception
                Console.WriteLine()
                Console.WriteLine("Fatal: Could not locate SFmpq.dll")
                Return
            End Try
            Console.WriteLine()
            If args.Length = 0 Then
                mpqe.helper()
                Return
            End If
            For i As Integer = 0 To args.Length - 1
                If args(i).StartsWith("/") Then
                    Select Case args(i)
                        Case "/p"
                            ' use patch files if available
                            Console.WriteLine("Using patch MPQ archives if available: Enabled")
                            mpqe.option_usepatchfiles = True
                            Exit Select
                        Case "/l"
                            Console.WriteLine("Lowercase output: Enabled")
                            mpqe.option_lowercase = True
                            Exit Select
                        Case "/v"
                            Console.WriteLine("Verbose output: Enabled")
                            mpqe.option_verbose = True
                            Exit Select
                        Case "/d"
                            If i + 1 >= args.Length Then
                                Console.WriteLine("Fatal: No output directory specified for /d")
                                mpqe.helper()
                                Return
                            End If


                            i += 1
                            Console.WriteLine("Output directory: {0}", args(i))
                            mpqe.option_outdir = args(i)
                            Exit Select
                        Case Else
                            Console.WriteLine("Fatal: Unknown option {0}", args(i))
                            mpqe.helper()
                            Return
                            Exit Select
                    End Select


                Else
                    If mpqe.option_baseMPQ Is Nothing Then
                        mpqe.option_baseMPQ = args(i)
                    Else
                        mpqe.option_searchglob = args(i)
                    End If
                End If


            Next
            If mpqe.option_baseMPQ Is Nothing Then
                Console.WriteLine("Fatal: Did not provide MPQ file for extraction!")
                mpqe.helper()
                Return
            End If


            mpqe.worker()
        End Sub

        Private Sub helper()
            Console.WriteLine("Extracts compressed files from MoPAQ archives.")
            Console.WriteLine()
            Console.WriteLine("MPQE [options] <MPQfile> [glob]")
            Console.WriteLine()
            Console.WriteLine("Options:")
            Console.WriteLine(" /p" & Chr(9) & "" & Chr(9) & "Extract newer files from patch MPQ archives if available")
            Console.WriteLine(" /d <directory>" & Chr(9) & "Set output directory ( default: MPQOUT )")
            Console.WriteLine(" /v" & Chr(9) & "" & Chr(9) & "Enable verbose output")
            Console.WriteLine(" /l" & Chr(9) & "" & Chr(9) & "Use lowercase filenames")
            '	Console.WriteLine("\t /l\tExtract listfile ( dumped to console )");
        End Sub

        Private Sub worker()
            If Not File.Exists(option_baseMPQ) Then
                Console.WriteLine("Fatal: Could not locate MPQ archive {0}", option_baseMPQ)
                Return
            End If


            Dim fi As New FileInfo(option_baseMPQ)
            If option_usepatchfiles AndAlso fi.Name.ToLower().StartsWith("patch") Then
                Console.WriteLine("Error: {0} is already a patch file, ignoring /p option", fi.Name)
                option_usepatchfiles = False
            End If
            If option_usepatchfiles Then
                For Each patchfile As String In Directory.GetFiles(fi.DirectoryName, "patch*.MPQ")
                    mpqExtract(patchfile)
                Next
            End If
            mpqExtract(option_baseMPQ)
        End Sub

        Private Sub mpqExtract(ByVal fileMPQ As String)
            Console.WriteLine("Extracting from " + fileMPQ)
            If SFmpq.SFileOpenArchive(fileMPQ, 0, 0, hMPQ) <> 1 Then
                Console.WriteLine("Error: Could not open {0}", fileMPQ)
                Return
            End If


            If SFmpq.SFileOpenFileEx(hMPQ, "(listfile)", 0, hFile) <> 1 Then
                SFmpq.SFileCloseArchive(hMPQ)
                Console.WriteLine("Error: Could not find (listfile) in " + fileMPQ)
                Return
            End If


            Dim buffer As Byte()
            FileSize = SFmpq.SFileGetFileSize(hFile, FileSize)
            buffer = New Byte(FileSize - 1) {}
            If SFmpq.SFileReadFile(hFile, buffer, CInt(FileSize), FileRead, IntPtr.Zero) <> 1 Then
                SFmpq.SFileCloseFile(hFile)
                SFmpq.SFileCloseArchive(hMPQ)
                Console.WriteLine("Error: Could not read (listfile) in " + fileMPQ)
                Return
            End If


            SFmpq.SFileCloseFile(hFile)
            Dim enc As New System.Text.ASCIIEncoding()
            Dim list As String = enc.GetString(buffer)
            Dim segs As String()
            Dim dirpath As String
            For Each file As String In list.Split(Chr(10))
                If file = "" Then
                    Return
                End If
                If Me.extractedFiles.Contains(file) Then
                    Continue For
                End If
                segs = file.Split("\"c)
                dirpath = [String].Join("\", segs, 0, segs.Length - 1)
                If option_lowercase Then
                    dirpath = dirpath.ToLower()
                End If
                If Me.option_searchglob IsNot Nothing AndAlso Not Match(option_searchglob, file.Trim(), False) Then
                    Continue For
                End If
                If SFmpq.SFileOpenFileEx(hMPQ, file.Trim(), 0, hFile) <> 1 Then
                    Console.WriteLine("Error: Could not find " + file.Trim() + " in " + fileMPQ)
                    Continue For
                End If


                FileSize = SFmpq.SFileGetFileSize(hFile, FileSize)
                If FileSize = 0 Then
                    If option_verbose Then
                        Console.WriteLine("Skipping: {0} (NULL)", file.Trim())
                    End If
                    Continue For
                End If
                buffer = New Byte(FileSize - 1) {}
                If SFmpq.SFileReadFile(hFile, buffer, CInt(FileSize), FileRead, IntPtr.Zero) <> 1 Then
                    SFmpq.SFileCloseFile(hFile)
                    Console.WriteLine("Error: Could not read " + file.Trim() + " in " + fileMPQ)
                    Continue For
                End If


                If Not Directory.Exists(option_outdir + Path.DirectorySeparatorChar + dirpath) Then
                    Directory.CreateDirectory(option_outdir + Path.DirectorySeparatorChar + dirpath)
                End If
                Dim fs As FileStream
                If option_lowercase Then
                    fs = New FileStream(option_outdir + Path.DirectorySeparatorChar + file.Trim().ToLower(), FileMode.Create, FileAccess.Write)
                Else
                    fs = New FileStream(option_outdir + Path.DirectorySeparatorChar + file.Trim(), FileMode.Create, FileAccess.Write)
                End If
                fs.Write(buffer, 0, FileSize)
                fs.Flush()
                fs.Close()
                If option_verbose Then
                    Console.WriteLine("Extracted: {0} ({1})", file.Trim(), bytes2text(FileSize))
                End If
                Me.extractedFiles.Add(file)
            Next


            SFmpq.SFileCloseArchive(hMPQ)
        End Sub

        Public Function Match(ByVal pattern As String, ByVal s As String, ByVal caseSensitive As Boolean) As Boolean
            Dim Wildcards As Char() = New Char() {"*"c, "?"c}
            If Not caseSensitive Then
                pattern = pattern.ToLower()
                s = s.ToLower()
            End If
            If pattern.IndexOfAny(Wildcards) = -1 Then
                Return (s = pattern)
            End If
            Dim i As Integer = 0
            Dim j As Integer = 0
            While i < s.Length AndAlso j < pattern.Length AndAlso pattern(j) <> "*"c
                If (pattern(j) <> s(i)) AndAlso (pattern(j) <> "?"c) Then
                    Return False
                End If
                i += 1
                j += 1
            End While
            If j = pattern.Length Then
                Return s.Length = pattern.Length
            End If

            Dim cp As Integer = 0
            Dim mp As Integer = 0
            While i < s.Length
                If j < pattern.Length AndAlso pattern(j) = "*"c Then
                    If (System.Math.Max(System.Threading.Interlocked.Increment(j), j - 1)) >= pattern.Length Then
                        Return True
                    End If
                    mp = j
                    cp = i + 1
                ElseIf j < pattern.Length AndAlso (pattern(j) = s(i) OrElse pattern(j) = "?"c) Then
                    j += 1
                    i += 1
                Else
                    j = mp
                    i = System.Math.Max(System.Threading.Interlocked.Increment(cp), cp - 1)
                End If


            End While

            While j < pattern.Length AndAlso pattern(j) = "*"c
                j += 1
            End While
            Return j >= pattern.Length
        End Function

        Public Function bytes2text(ByVal bytes As Integer) As String
            ' not long?
            If bytes < 1024 Then
                Return bytes + "B"
            End If
            If bytes < 1024 * 1024 Then
                Return Math.Round(CDec(bytes) / 1024, 0) + "K"
            End If
            If bytes < 1024 * 1024 * 1024 Then
                Return Math.Round(CDec(bytes) / 1024 / 1024, 1) + "M"
            End If
            Return "moo"
        End Function
    End Class
End Namespace
