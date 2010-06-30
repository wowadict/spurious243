<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class Form1
    Inherits System.Windows.Forms.Form

    'Form overrides dispose to clean up the component list.
    <System.Diagnostics.DebuggerNonUserCode()> _
    Protected Overrides Sub Dispose(ByVal disposing As Boolean)
        Try
            If disposing AndAlso components IsNot Nothing Then
                components.Dispose()
            End If
        Finally
            MyBase.Dispose(disposing)
        End Try
    End Sub

    'Required by the Windows Form Designer
    Private components As System.ComponentModel.IContainer

    'NOTE: The following procedure is required by the Windows Form Designer
    'It can be modified using the Windows Form Designer.  
    'Do not modify it using the code editor.
    <System.Diagnostics.DebuggerStepThrough()> _
    Private Sub InitializeComponent()
        Me.cmdConnect = New System.Windows.Forms.Button
        Me.txtHost = New System.Windows.Forms.TextBox
        Me.txtPort = New System.Windows.Forms.TextBox
        Me.txtUser = New System.Windows.Forms.TextBox
        Me.txtPass = New System.Windows.Forms.TextBox
        Me.txtDatabase = New System.Windows.Forms.TextBox
        Me.lblHost = New System.Windows.Forms.Label
        Me.lblPort = New System.Windows.Forms.Label
        Me.lblUser = New System.Windows.Forms.Label
        Me.lblPass = New System.Windows.Forms.Label
        Me.lblDatabase = New System.Windows.Forms.Label
        Me.lblDBVersion = New System.Windows.Forms.Label
        Me.lblDBName = New System.Windows.Forms.Label
        Me.lblSpuriousRevision = New System.Windows.Forms.Label
        Me.lblVersion = New System.Windows.Forms.Label
        Me.lblName = New System.Windows.Forms.Label
        Me.lblRevision = New System.Windows.Forms.Label
        Me.cmdUpdate = New System.Windows.Forms.Button
        Me.SuspendLayout()
        '
        'cmdConnect
        '
        Me.cmdConnect.Location = New System.Drawing.Point(157, 141)
        Me.cmdConnect.Name = "cmdConnect"
        Me.cmdConnect.Size = New System.Drawing.Size(163, 27)
        Me.cmdConnect.TabIndex = 0
        Me.cmdConnect.Text = "Connect"
        Me.cmdConnect.UseVisualStyleBackColor = True
        '
        'txtHost
        '
        Me.txtHost.Location = New System.Drawing.Point(147, 11)
        Me.txtHost.Name = "txtHost"
        Me.txtHost.Size = New System.Drawing.Size(185, 20)
        Me.txtHost.TabIndex = 1
        Me.txtHost.Text = "localhost"
        '
        'txtPort
        '
        Me.txtPort.Location = New System.Drawing.Point(147, 37)
        Me.txtPort.Name = "txtPort"
        Me.txtPort.Size = New System.Drawing.Size(185, 20)
        Me.txtPort.TabIndex = 2
        Me.txtPort.Text = "3306"
        '
        'txtUser
        '
        Me.txtUser.Location = New System.Drawing.Point(147, 63)
        Me.txtUser.Name = "txtUser"
        Me.txtUser.Size = New System.Drawing.Size(185, 20)
        Me.txtUser.TabIndex = 3
        Me.txtUser.Text = "root"
        '
        'txtPass
        '
        Me.txtPass.Location = New System.Drawing.Point(147, 89)
        Me.txtPass.Name = "txtPass"
        Me.txtPass.PasswordChar = Global.Microsoft.VisualBasic.ChrW(42)
        Me.txtPass.Size = New System.Drawing.Size(185, 20)
        Me.txtPass.TabIndex = 4
        Me.txtPass.Text = "root"
        '
        'txtDatabase
        '
        Me.txtDatabase.Location = New System.Drawing.Point(147, 115)
        Me.txtDatabase.Name = "txtDatabase"
        Me.txtDatabase.Size = New System.Drawing.Size(185, 20)
        Me.txtDatabase.TabIndex = 5
        Me.txtDatabase.Text = "spurious"
        '
        'lblHost
        '
        Me.lblHost.AutoSize = True
        Me.lblHost.Font = New System.Drawing.Font("Microsoft Sans Serif", 8.25!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.lblHost.Location = New System.Drawing.Point(15, 11)
        Me.lblHost.Name = "lblHost"
        Me.lblHost.Size = New System.Drawing.Size(65, 13)
        Me.lblHost.TabIndex = 6
        Me.lblHost.Text = "SQL Host:"
        '
        'lblPort
        '
        Me.lblPort.AutoSize = True
        Me.lblPort.Font = New System.Drawing.Font("Microsoft Sans Serif", 8.25!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.lblPort.Location = New System.Drawing.Point(15, 37)
        Me.lblPort.Name = "lblPort"
        Me.lblPort.Size = New System.Drawing.Size(62, 13)
        Me.lblPort.TabIndex = 7
        Me.lblPort.Text = "SQL Port:"
        '
        'lblUser
        '
        Me.lblUser.AutoSize = True
        Me.lblUser.Font = New System.Drawing.Font("Microsoft Sans Serif", 8.25!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.lblUser.Location = New System.Drawing.Point(15, 63)
        Me.lblUser.Name = "lblUser"
        Me.lblUser.Size = New System.Drawing.Size(65, 13)
        Me.lblUser.TabIndex = 8
        Me.lblUser.Text = "SQL User:"
        '
        'lblPass
        '
        Me.lblPass.AutoSize = True
        Me.lblPass.Font = New System.Drawing.Font("Microsoft Sans Serif", 8.25!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.lblPass.Location = New System.Drawing.Point(15, 89)
        Me.lblPass.Name = "lblPass"
        Me.lblPass.Size = New System.Drawing.Size(66, 13)
        Me.lblPass.TabIndex = 9
        Me.lblPass.Text = "SQL Pass:"
        '
        'lblDatabase
        '
        Me.lblDatabase.AutoSize = True
        Me.lblDatabase.Font = New System.Drawing.Font("Microsoft Sans Serif", 8.25!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.lblDatabase.Location = New System.Drawing.Point(15, 115)
        Me.lblDatabase.Name = "lblDatabase"
        Me.lblDatabase.Size = New System.Drawing.Size(93, 13)
        Me.lblDatabase.TabIndex = 10
        Me.lblDatabase.Text = "SQL Database:"
        '
        'lblDBVersion
        '
        Me.lblDBVersion.AutoSize = True
        Me.lblDBVersion.Font = New System.Drawing.Font("Microsoft Sans Serif", 8.25!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.lblDBVersion.Location = New System.Drawing.Point(344, 11)
        Me.lblDBVersion.Name = "lblDBVersion"
        Me.lblDBVersion.Size = New System.Drawing.Size(74, 13)
        Me.lblDBVersion.TabIndex = 11
        Me.lblDBVersion.Text = "DB Version:"
        '
        'lblDBName
        '
        Me.lblDBName.AutoSize = True
        Me.lblDBName.Font = New System.Drawing.Font("Microsoft Sans Serif", 8.25!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.lblDBName.Location = New System.Drawing.Point(344, 37)
        Me.lblDBName.Name = "lblDBName"
        Me.lblDBName.Size = New System.Drawing.Size(64, 13)
        Me.lblDBName.TabIndex = 12
        Me.lblDBName.Text = "DB Name:"
        '
        'lblSpuriousRevision
        '
        Me.lblSpuriousRevision.AutoSize = True
        Me.lblSpuriousRevision.Font = New System.Drawing.Font("Microsoft Sans Serif", 8.25!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.lblSpuriousRevision.Location = New System.Drawing.Point(344, 63)
        Me.lblSpuriousRevision.Name = "lblSpuriousRevision"
        Me.lblSpuriousRevision.Size = New System.Drawing.Size(117, 13)
        Me.lblSpuriousRevision.TabIndex = 13
        Me.lblSpuriousRevision.Text = "Sprurious Revision:"
        '
        'lblVersion
        '
        Me.lblVersion.AutoSize = True
        Me.lblVersion.Location = New System.Drawing.Point(499, 11)
        Me.lblVersion.Name = "lblVersion"
        Me.lblVersion.Size = New System.Drawing.Size(0, 13)
        Me.lblVersion.TabIndex = 14
        '
        'lblName
        '
        Me.lblName.AutoSize = True
        Me.lblName.Location = New System.Drawing.Point(499, 37)
        Me.lblName.Name = "lblName"
        Me.lblName.Size = New System.Drawing.Size(0, 13)
        Me.lblName.TabIndex = 15
        '
        'lblRevision
        '
        Me.lblRevision.AutoSize = True
        Me.lblRevision.Location = New System.Drawing.Point(499, 63)
        Me.lblRevision.Name = "lblRevision"
        Me.lblRevision.Size = New System.Drawing.Size(0, 13)
        Me.lblRevision.TabIndex = 16
        '
        'cmdUpdate
        '
        Me.cmdUpdate.Enabled = False
        Me.cmdUpdate.Location = New System.Drawing.Point(347, 89)
        Me.cmdUpdate.Name = "cmdUpdate"
        Me.cmdUpdate.Size = New System.Drawing.Size(287, 20)
        Me.cmdUpdate.TabIndex = 17
        Me.cmdUpdate.Text = "Update Database"
        Me.cmdUpdate.UseVisualStyleBackColor = True
        '
        'Form1
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 13.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.ClientSize = New System.Drawing.Size(646, 173)
        Me.Controls.Add(Me.cmdUpdate)
        Me.Controls.Add(Me.lblRevision)
        Me.Controls.Add(Me.lblName)
        Me.Controls.Add(Me.lblVersion)
        Me.Controls.Add(Me.lblSpuriousRevision)
        Me.Controls.Add(Me.lblDBName)
        Me.Controls.Add(Me.lblDBVersion)
        Me.Controls.Add(Me.lblDatabase)
        Me.Controls.Add(Me.lblPass)
        Me.Controls.Add(Me.lblUser)
        Me.Controls.Add(Me.lblPort)
        Me.Controls.Add(Me.lblHost)
        Me.Controls.Add(Me.txtDatabase)
        Me.Controls.Add(Me.txtPass)
        Me.Controls.Add(Me.txtUser)
        Me.Controls.Add(Me.txtPort)
        Me.Controls.Add(Me.txtHost)
        Me.Controls.Add(Me.cmdConnect)
        Me.Name = "Form1"
        Me.Text = "Spurious Database Updater by UniX"
        Me.ResumeLayout(False)
        Me.PerformLayout()

    End Sub
    Friend WithEvents cmdConnect As System.Windows.Forms.Button
    Friend WithEvents txtHost As System.Windows.Forms.TextBox
    Friend WithEvents txtPort As System.Windows.Forms.TextBox
    Friend WithEvents txtUser As System.Windows.Forms.TextBox
    Friend WithEvents txtPass As System.Windows.Forms.TextBox
    Friend WithEvents txtDatabase As System.Windows.Forms.TextBox
    Friend WithEvents lblHost As System.Windows.Forms.Label
    Friend WithEvents lblPort As System.Windows.Forms.Label
    Friend WithEvents lblUser As System.Windows.Forms.Label
    Friend WithEvents lblPass As System.Windows.Forms.Label
    Friend WithEvents lblDatabase As System.Windows.Forms.Label
    Friend WithEvents lblDBVersion As System.Windows.Forms.Label
    Friend WithEvents lblDBName As System.Windows.Forms.Label
    Friend WithEvents lblSpuriousRevision As System.Windows.Forms.Label
    Friend WithEvents lblVersion As System.Windows.Forms.Label
    Friend WithEvents lblName As System.Windows.Forms.Label
    Friend WithEvents lblRevision As System.Windows.Forms.Label
    Friend WithEvents cmdUpdate As System.Windows.Forms.Button
End Class
