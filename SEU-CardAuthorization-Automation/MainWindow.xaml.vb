Imports mshtml
'Imports HtmlAgilityPack
Imports System.Threading
Class MainWindow
    Private Sub LockUI()
        txtTeacherName.IsEnabled = False
        txtTeacherPhoneNumber.IsEnabled = False
        txtReason.IsEnabled = False
        dtpEndDate.IsEnabled = False
        dtpStartDate.IsEnabled = False
        btnStart.IsEnabled = False
        'wbbEhallContainer.IsEnabled = False
    End Sub
    Private Sub UnlockUI()
        txtTeacherName.IsEnabled = True
        txtTeacherPhoneNumber.IsEnabled = True
        txtReason.IsEnabled = True
        dtpEndDate.IsEnabled = True
        dtpStartDate.IsEnabled = True
        btnStart.IsEnabled = True
        'wbbEhallContainer.IsEnabled = True
    End Sub
    Private Sub MainWindow_Loaded(sender As Object, e As RoutedEventArgs) Handles Me.Loaded
        Dim ActiveX = wbbEhallContainer.GetType().InvokeMember("ActiveXInstance", Reflection.BindingFlags.GetProperty Or Reflection.BindingFlags.Instance Or Reflection.BindingFlags.NonPublic, Nothing, wbbEhallContainer, Nothing)
        ActiveX.Silent = True
        dtpStartDate.DisplayDateStart = Now.Date
        dtpEndDate.DisplayDateStart = Now.Date.AddDays(1)
        dtpStartDate.SelectedDate = Now.Date
        dtpEndDate.SelectedDate = New Date(2021, 2, 1)
        wbbEhallContainer.Navigate("https://newids.seu.edu.cn/authserver/login?service=http://ehall.seu.edu.cn/xsfw/sys/xsqjapp/*default/index.do")
        MessageBox.Show("请先登录到东南大学本科生出校登记审批系统。请放心，本程序不会试图储存您的登录资讯。", "需要登录", MessageBoxButton.OK, MessageBoxImage.Information)
    End Sub
    Private Function GenerateAuthorizationStartTime(DateToProcess As Date) As String
        Return DateToProcess.Year.ToString & "-" & DateToProcess.Month.ToString("00") & "-" & DateToProcess.Day.ToString("00") & " 08:30:01"
    End Function
    Private Function GenerateAuthorizationEndTime(DateToProcess As Date) As String
        Return DateToProcess.Year.ToString & "-" & DateToProcess.Month.ToString("00") & "-" & DateToProcess.Day.ToString("00") & " 23:59:59"
    End Function
    Private Sub btnStart_Click(sender As Object, e As RoutedEventArgs) Handles btnStart.Click
        LockUI()
        Dim CurrentDate As Date = dtpStartDate.SelectedDate
        Dim AuthorizationStartTime As String
        Dim AuthorizationEndTime As String
        Dim TeacherName As String = txtTeacherName.Text
        Dim TeacherPhoneNumber As String = txtTeacherPhoneNumber.Text
        Dim Reason As String = txtReason.Text
        If txtReason.Text.Trim = "" Or txtTeacherName.Text.Trim = "" Or txtTeacherPhoneNumber.Text.Trim = "" Then
            MessageBox.Show("请确保辅导员姓名、辅导员手机号和请假事由均已填入且非空。", "错误", MessageBoxButton.OK, MessageBoxImage.Error)
            UnlockUI()
            Exit Sub
        End If
        Try
            While CurrentDate < dtpEndDate.SelectedDate
                AuthorizationStartTime = GenerateAuthorizationStartTime(CurrentDate)
                AuthorizationEndTime = GenerateAuthorizationEndTime(CurrentDate)
                Dim CurrentHTMLDocument As HTMLDocument
                '取当前页面
                CurrentHTMLDocument = wbbEhallContainer.Document
                '获取所有按钮(标签是“a”的元素)，不支持getElementsByClassName()方法。
                Dim PossibleElements As IHTMLElementCollection = CurrentHTMLDocument.getElementsByTagName("a")
                '寻找“我要请假”按钮
                For Each CurrentButton As IHTMLElement In PossibleElements
                    If CurrentButton.className = "bh-btn bh-btn-primary" Then
                        CurrentButton.click()
                        Exit For
                    End If
                Next
                Thread.Sleep(1000)
                '跳过可能的“请先销假”提示
                CurrentHTMLDocument = wbbEhallContainer.Document
                PossibleElements = CurrentHTMLDocument.getElementsByTagName("a")
                For Each CurrentButton As IHTMLElement In PossibleElements
                    If CurrentButton.className = "bh-dialog-btn bh-bg-primary bh-color-primary-5" Then
                        CurrentButton.click()
                        Exit For
                    End If
                Next
                DoEvents()
                Thread.Sleep(500)
                DoEvents()
                '同意协议
                CurrentHTMLDocument = wbbEhallContainer.Document
                CurrentHTMLDocument.getElementById("CheckCns").click()
                PossibleElements = CurrentHTMLDocument.getElementsByTagName("button")
                For Each CurrentButton As IHTMLElement In PossibleElements
                    If CurrentButton.className = "bh-btn bh-btn-primary bh-pull-right" Then
                        CurrentButton.click()
                        Exit For
                    End If
                Next
                DoEvents()
                Thread.Sleep(1000)
                DoEvents()
                '开始填报
                CurrentHTMLDocument = wbbEhallContainer.Document
                Dim DataInputBox As IHTMLInputTextElement
                Dim DataElement As IHTMLElement
                DataInputBox = CurrentHTMLDocument.getElementsByName("FZLSXM")(0)
                DataInputBox.value = TeacherName
                DataInputBox = CurrentHTMLDocument.getElementsByName("FZLSDH")(0)
                DataInputBox.value = TeacherPhoneNumber
                DataElement = CurrentHTMLDocument.getElementsByName("QJSY")(0)
                DataElement.setAttribute("value", Reason)
                PossibleElements = CurrentHTMLDocument.getElementsByTagName("div")
                For Each CurrentDiv As IHTMLElement In PossibleElements
                    If CurrentDiv.getAttribute("data-name").GetType().ToString = "System.DBNull" Then
                        Continue For
                    End If
                    If CurrentDiv.getAttribute("data-name") = "QJKSRQ" Then
                        Dim ChildrenCollection As IHTMLElementCollection
                        ChildrenCollection = CurrentDiv.children
                        DataInputBox = ChildrenCollection(0)
                        DataInputBox.value = AuthorizationStartTime
                    End If
                Next
                DoEvents()
                PossibleElements = CurrentHTMLDocument.getElementsByTagName("div")
                For Each CurrentDiv As IHTMLElement In PossibleElements
                    If CurrentDiv.getAttribute("data-name").GetType().ToString = "System.DBNull" Then
                        Continue For
                    End If
                    If CurrentDiv.getAttribute("data-name") = "QJJSRQ" Then
                        Dim ChildrenCollection As IHTMLElementCollection
                        ChildrenCollection = CurrentDiv.children
                        DataInputBox = ChildrenCollection(0)
                        DataInputBox.value = AuthorizationEndTime
                    End If
                Next
                DoEvents()
                PossibleElements = CurrentHTMLDocument.getElementsByTagName("div")
                For Each CurrentDiv As IHTMLElement In PossibleElements
                    If CurrentDiv.getAttribute("data-name").GetType().ToString = "System.DBNull" Then
                        Continue For
                    End If
                    If CurrentDiv.getAttribute("data-name") = "DZQJSY" Then
                        Dim ListBoxId As String = CurrentDiv.id
                        For Each DataElement In CurrentHTMLDocument.getElementsByTagName("input")
                            If DataElement.parentElement.id = ListBoxId Then
                                DataElement.setAttribute("value", "9d1fea506de3484486cfd8235d0d19d8")
                                Exit For
                            End If
                        Next
                        Exit For
                    End If
                Next
                DoEvents()
                PossibleElements = CurrentHTMLDocument.getElementsByTagName("div")
                For Each CurrentDiv As IHTMLElement In PossibleElements
                    If CurrentDiv.getAttribute("data-name").GetType().ToString = "System.DBNull" Then
                        Continue For
                    End If
                    If CurrentDiv.getAttribute("data-name") = "QJSX" Then
                        Dim ListBoxId As String = CurrentDiv.id
                        For Each DataElement In CurrentHTMLDocument.getElementsByTagName("input")
                            If DataElement.parentElement.id = ListBoxId Then
                                DataElement.setAttribute("value", "1")
                                Exit For
                            End If
                        Next
                        Exit For
                    End If
                Next
                DoEvents()
                PossibleElements = CurrentHTMLDocument.getElementsByTagName("div")
                For Each CurrentDiv As IHTMLElement In PossibleElements
                    If CurrentDiv.getAttribute("data-name").GetType().ToString = "System.DBNull" Then
                        Continue For
                    End If
                    If CurrentDiv.getAttribute("data-name") = "HDXQ" Then
                        Dim ListBoxId As String = CurrentDiv.id
                        For Each DataElement In CurrentHTMLDocument.getElementsByTagName("input")
                            If DataElement.parentElement.id = ListBoxId Then
                                DataElement.setAttribute("value", "1,2,3")
                                Exit For
                            End If
                        Next
                        Exit For
                    End If
                Next
                DoEvents()
                PossibleElements = CurrentHTMLDocument.getElementsByTagName("div")
                For Each CurrentDiv As IHTMLElement In PossibleElements
                    If CurrentDiv.getAttribute("data-name").GetType().ToString = "System.DBNull" Then
                        Continue For
                    End If
                    If CurrentDiv.getAttribute("data-name") = "DZSFLN" Then
                        Dim ListBoxId As String = CurrentDiv.id
                        For Each DataElement In CurrentHTMLDocument.getElementsByTagName("input")
                            If DataElement.parentElement.id = ListBoxId Then
                                DataElement.setAttribute("value", "0")
                                Exit For
                            End If
                        Next
                        Exit For
                    End If
                Next
                DoEvents()
                DataInputBox = CurrentHTMLDocument.getElementsByName("XXDZ")(0)
                DataInputBox.value = "东南大学九龙湖校区"
                '提交
                CurrentHTMLDocument = wbbEhallContainer.Document
                PossibleElements = CurrentHTMLDocument.getElementsByTagName("a")
                For Each CurrentButton As IHTMLElement In PossibleElements
                    If CurrentButton.className = "bh-btn bh-btn-primary waves-effect" And CurrentButton.innerHTML = "提交" Then
                        CurrentButton.click()
                        Exit For
                    End If
                Next
                DoEvents()
                Thread.Sleep(1000)
                DoEvents()
                Thread.Sleep(2500)
                DoEvents()
                CurrentDate = CurrentDate.AddDays(1)
            End While
        Catch ex As Exception
            MessageBox.Show("发生错误: " & vbCrLf & ex.Message, "错误", MessageBoxButton.OK, MessageBoxImage.Error)
            UnlockUI()
            wbbEhallContainer.Refresh()
            Exit Sub
        End Try
        UnlockUI()
        wbbEhallContainer.Refresh()
    End Sub

    Private Sub dtpStartDate_SelectedDateChanged(sender As Object, e As SelectionChangedEventArgs) Handles dtpStartDate.SelectedDateChanged
        If IsNothing(dtpEndDate.SelectedDate) Then
            Exit Sub
        End If
        dtpEndDate.DisplayDateStart = dtpStartDate.SelectedDate.Value.AddDays(1)
        If dtpEndDate.SelectedDate.Value.CompareTo(dtpStartDate.SelectedDate.Value) <= 0 Then
            dtpEndDate.SelectedDate = dtpStartDate.SelectedDate.Value.AddDays(1)
        End If
    End Sub
End Class
