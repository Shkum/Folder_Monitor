Imports System.ComponentModel
Imports Microsoft.Win32
Public Class Form1
    Dim icoCnt As Integer
    Dim oldF As Long
    Dim newF As Long
    Dim aFlag As Boolean
    Private Sub CheckBox1_CheckedChanged(sender As Object, e As EventArgs) Handles CheckBox1.CheckedChanged
        Try
            Dim regStart As RegistryKey = Registry.LocalMachine.OpenSubKey("SOFTWARE\Microsoft\Windows\CurrentVersion\Run", True)
            If CheckBox1.Checked = True Then
                regStart.SetValue("NewFileMonitor", """" & Application.ExecutablePath & """" & " -Start")
            Else
                regStart.DeleteValue("NewFileMonitor")
            End If
        Catch ex As Exception
            MsgBox(ex.Message & vbCrLf & "Restart program as Administrator", vbInformation)
        End Try

    End Sub

    Private Sub Button1_Click(sender As Object, e As EventArgs) Handles Button1.Click
        If aFlag Then
            If MsgBox("Are you sure to stop monitoring and select new folder?", vbQuestion + vbYesNo, "Select new folder for monitoring?") = vbNo Then Exit Sub
        End If
        If aFlag = True Then
            aFlag = False
            ToolStripMenuItem2.Enabled = False
            ToolStripMenuItem3.Enabled = False
            Button2.Text = "Start monitoring"
            ToolStripMenuItem3.Text = "Start monitoring"
            Me.Text = "Monitoring stopped ..."
        End If
        Dim a As FolderBrowserDialog = New FolderBrowserDialog
        Dim s As String
        a.Description = "Select folder for monitoring"
        a.ShowDialog()
        s = a.SelectedPath
        If s <> "" Then
            Label2.Text = s
            Button2.Enabled = True
            ToolStripMenuItem2.Enabled = True
            ToolStripMenuItem3.Enabled = True
            SaveSetting("FolderMonitor", "Settings", "FolderName", s)
        End If
    End Sub

    Private Sub Form1_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        If Registry.LocalMachine.OpenSubKey("SOFTWARE\Microsoft\Windows\CurrentVersion\Run").GetValue("NewFileMonitor") <> "" Then CheckBox1.Checked = True
        'If GetSetting("FolderMonitor", "Settings", "CheckFolder") = "" Then SaveSetting("FolderMonitor", "Settings", "CheckFolder", CheckBox2.Checked.ToString)
        ' CheckBox2.Checked = GetSetting("FolderMonitor", "Settings", "CheckFolder")
        If IO.Directory.Exists(GetSetting("FolderMonitor", "Settings", "FolderName")) Then
            Label2.Text = GetSetting("FolderMonitor", "Settings", "FolderName")
            Dim enbl As Boolean = GetSetting("FolderMonitor", "Settings", "FolderName") <> ""
            Button2.Enabled = enbl
            ToolStripMenuItem2.Enabled = enbl
            ToolStripMenuItem3.Enabled = enbl
            If Command() = "-Start" And Label2.Text <> "" Then
                Me.Visible = False
                Me.ShowInTaskbar = False
                Me.WindowState = FormWindowState.Minimized

            End If
        ElseIf (GetSetting("FolderMonitor", "Settings", "FolderName") <> "") And (IO.Directory.Exists(GetSetting("FolderMonitor", "Settings", "FolderName")) = False) Then
            MsgBox("Folder not selected or wrong path.", vbInformation)
            SaveSetting("FolderMonitor", "Settings", "FolderName", "")
        End If

    End Sub

    Private Sub Button2_Click(sender As Object, e As EventArgs) Handles Button2.Click
        If IO.Directory.Exists(Label2.Text) = False Then
            MsgBox("Folder selected for monitoring not available", vbInformation)
            aFlag = False
            ToolStripMenuItem2.Enabled = False
            ToolStripMenuItem3.Enabled = False
            Button2.Text = "Start monitoring"
            ToolStripMenuItem3.Text = "Start monitoring"
            Me.Text = "Monitoring stopped ..."
            Exit Sub
        End If
        aFlag = Not aFlag
        Button2.Text = If(aFlag = False, "Start monitoring", "Stop monitoring")
        ToolStripMenuItem3.Text = If(aFlag = False, "Start monitoring", "Stop monitoring")
        Me.Text = If(aFlag = False, "Monitoring stopped ...", "Monitoring in progress ...")
        If aFlag = True Then
            ToolStripMenuItem2.Enabled = True
            ToolStripMenuItem3.Enabled = True
            StartMon()
        End If
    End Sub

    Private Sub StartMon()
        Me.Hide()
        oldF = System.IO.Directory.GetFiles(Label2.Text).Length
        Dim t As New Threading.Thread(AddressOf Monitoring) With {.Priority = Threading.ThreadPriority.Lowest}
        t.Start()
    End Sub

    Private Sub Monitoring()
        Do While (aFlag = True)
            If IO.Directory.Exists(Label2.Text) Then

                newF = System.IO.Directory.GetFiles(Label2.Text).Length
                If newF > oldF Then
                    Tric.BalloonTipText = "New file detected in your folder..."
                    Tric.Text = "New file detected..."
                    Tric.ShowBalloonTip(5000)
                    Timer1.Enabled = True
                    oldF = newF
                End If
                If newF < oldF Then oldF = newF

            End If
            Application.DoEvents()
            System.Threading.Thread.Sleep(500)
        Loop
    End Sub


    Private Sub Tric_MouseDoubleClick(sender As Object, e As MouseEventArgs) Handles Tric.MouseDoubleClick

        If e.Button = MouseButtons.Left Then
            Me.Visible = False
            Interaction.Shell("explorer " & Label2.Text, AppWinStyle.MaximizedFocus)
        End If

    End Sub

    Private Sub Tric_MouseClick(sender As Object, e As MouseEventArgs) Handles Tric.MouseClick
        If e.Button = MouseButtons.Right Then
            Tric.ContextMenuStrip.Show()
        End If
        If e.Button = MouseButtons.Left Then
            If IO.Directory.Exists(GetSetting("FolderMonitor", "Settings", "FolderName")) Then
                oldF = System.IO.Directory.GetFiles(Label2.Text).Length
                If Timer1.Enabled = True Then
                    Interaction.Shell("explorer " & Label2.Text, AppWinStyle.MaximizedFocus)
                Else
                    If e.Button = MouseButtons.Left Then Me.Visible = Not Me.Visible : Me.WindowState = vbNormal
                End If
                Timer1.Enabled = False
                Tric.BalloonTipText = "No new files detected in your folder..."
                Tric.Text = "No new files!"
                Tric.Icon = My.Resources.fire
            End If
        End If

    End Sub

    Private Sub ToolStripMenuItem1_Click(sender As Object, e As EventArgs) Handles ToolStripMenuItem1.Click
        Me.Show()
    End Sub

    Private Sub ToolStripMenuItem2_Click(sender As Object, e As EventArgs) Handles ToolStripMenuItem2.Click
        If Label2.Text <> "" Then Interaction.Shell("explorer " & Label2.Text, AppWinStyle.MaximizedFocus)
    End Sub

    Private Sub ToolStripMenuItem3_Click(sender As Object, e As EventArgs) Handles ToolStripMenuItem3.Click

        If IO.Directory.Exists(Label2.Text) = False Then
            MsgBox("Folder selected for monitoring not available", vbInformation)
            aFlag = False
            ToolStripMenuItem2.Enabled = False
            ToolStripMenuItem3.Enabled = False
            Button2.Text = "Start monitoring"
            ToolStripMenuItem3.Text = "Start monitoring"
            Me.Text = "Monitoring stopped ..."
            Exit Sub
        End If

        aFlag = Not aFlag
        Button2.Text = If(aFlag = False, "Start monitoring", "Stop Monitoring")
        ToolStripMenuItem3.Text = If(aFlag = False, "Start monitoring", "Stop monitoring")
        If aFlag = True Then StartMon()
    End Sub

    Private Sub ExitToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles ExitToolStripMenuItem.Click
        If aFlag Then
            If MsgBox("Are you sure to stop monitoring and exit?", vbQuestion + vbYesNo, "Exit program?") = vbYes Then
                aFlag = False
                Me.Close()
            End If
        Else
            aFlag = False
            Me.Close()
        End If
    End Sub

    Private Sub Timer1_Tick(sender As Object, e As EventArgs) Handles Timer1.Tick
        icoCnt += 1
        Tric.Icon = If(icoCnt = 1, My.Resources._1, If(icoCnt = 2, My.Resources._2, My.Resources._3))
        If icoCnt = 3 Then icoCnt = 0
    End Sub

    Private Sub Form1_Closing(sender As Object, e As CancelEventArgs) Handles Me.Closing
        If aFlag Then
            If MsgBox("Are you sure to stop monitoring and exit?", vbQuestion + vbYesNo, "Exit program?") = vbYes Then
                aFlag = False
            Else e.Cancel = True
            End If
        Else
            aFlag = False
        End If
    End Sub

    Private Sub Form1_Shown(sender As Object, e As EventArgs) Handles Me.Shown
        If Command() = "-Start" And Label2.Text <> "" Then
            Button2.PerformClick()
        End If
    End Sub

    Private Sub Button3_Click(sender As Object, e As EventArgs) Handles Button3.Click
        Me.Hide()
    End Sub

    Private Sub ToolStripMenuItem4_Click(sender As Object, e As EventArgs) Handles ToolStripMenuItem4.Click
        MsgBox("This super-puper-mega-hyper-cool program" & vbCrLf & "made by Sergiy Shkumat", vbInformation, "About this software...")
    End Sub

    Private Sub Button4_Click(sender As Object, e As EventArgs) Handles Button4.Click
        If aFlag Then
            If MsgBox("Are you sure to stop monitoring and exit?", vbQuestion + vbYesNo, "Exit program?") = vbYes Then
                aFlag = False
                Me.Close()
            End If
        Else
            aFlag = False
            Me.Close()
        End If
    End Sub

    Private Sub Mnu1_LostFocus(sender As Object, e As EventArgs) Handles Mnu1.LostFocus
        Tric.ShowBalloonTip(False)
    End Sub
End Class
