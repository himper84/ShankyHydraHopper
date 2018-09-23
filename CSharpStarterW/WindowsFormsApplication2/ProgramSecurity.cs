using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace csFTPStarter
{
    class ProgramSecurity
    {
        private string sEncryptionKey;
        private string sFakeDLLName;
        private int iTrialDays;
        
        public string sApplicationName;
        private string sUID;
        private string sIni;

        private string sMAC;
        private string sGenerate;
        private string sRestore;
        private bool bIsDebug;
        private bool bHasRegCode;
        private bool bShouldEvaluationWarning;
        private string sDllEncodeKey;
        private string sDllIniSection;

        static public int CheckSecurity()
        {
            string sEncryptionKey = "FTPStarterCS";
            string sFakeDLLName = "546465";
            int iTrialDays = 3;

            string sApplicationName = "Codestormer";
            string sUID = System.Guid.NewGuid().ToString();
            Console.WriteLine(sUID + "\n");
            return 1;
            //sIni = FindValidIni();
            /*
            sMAC;
            sGenerate;
            sRestore;
            bIsDebug;
            bHasRegCode = true;
            bShouldEvaluationWarning = true;
            sDllEncodeKey;
            sDllIniSection;        
             */
        }

        /*
Func AddDLL($DLLPath)
	If StringRight($DLLPath,1)<>"\" Then
		$DLLPath = $DLLPath & ":" & $FakeDLLName
	Else
		$DLLPath = StringTrimRight($DLLPath,1) & ":" & $FakeDLLName
	EndIf
	Return $DLLPath
EndFunc

$ini = AddDLL(@SystemDir)
If Not FileExists($ini) Then
	IniWrite($ini, "t", "t", "t")
	If Not FileExists($ini) Then
		$ini = AddDLL(@WindowsDir)
		IniWrite($ini, "t", "t", "t")
		If Not FileExists($ini) Then
			$ini = AddDLL(StringLeft(@ScriptDir, 3))
			IniWrite($ini, "t", "t", "t")
			If Not FileExists($ini) Then
				$ini = AddDLL(@ScriptDir)
				IniWrite($ini, "t", "t", "t")
				If not FileExists($ini) Then
					$ini = @ScriptDir & "\" & $FakeDLLName
					IniWrite($ini, "t", "t", "t")
					If not FileExists($ini) Then
						MsgBox(32, "", "Program unable to save settings. Please ensure this app has write access to save settings.")
						Exit
					EndIf
				EndIf
			EndIf
		EndIf
	EndIf
	IniDelete($ini, "t")
EndIf
FileSetAttrib($ini, "+HS")
;~ ConsoleWrite($ini & " " & FileExists($ini))

;~ Global $validation = 0  ;if $validation==1 then it's registered, if it's -1 it's trial
Global $mac
Global $Generate
Global $restore
Global $VDebugit=True
Global $RequiresRegCode = 1
Global $ShowEvaluationWarning = 1 ;1 displays an evaluation warning during checking the authorisation, 0 will not show a warning
Global $dllEncKey = "whatzi" ;you can change this to whatever you like it is the ini encryption if made too long and it takes longer to read and write
Global $dlliniSection = Encrypt("iniSecti0nName");you can change this too if you really want to

If $RequiresRegCode Then
	$mac = StringStripWS(StringUpper(StringReplace(_GetMAC(), ":", "") & StringRight(Hex(@MON), 1)),3); this makes the mac address only valid during this month or registration, so if they discover the fake dll and delete it,
																					 ; the code only registers during this month of cousre they could figure on putting it back to the original registration month
	If StringLen($mac)<12 AND StringLen($uid)>12 Then
		$mac = $mac & StringRight($uid, (12-StringLen($mac)))
	EndIf
	While StringLen($mac)<12
		$mac &= $mac
	WEnd
	If StringLen($mac)>12 Then $mac = StringLeft($mac, 12)

	$Generate = StringRight($mac, 5)
	$restore = StringUpper(_StringEncrypt(1, $Generate, $securityCodeEncryptionKey, 1))
EndIf



Global $validation = CheckValidation()
CheckOnline()

Func _GetMAC($getmacindex = 1)
	$objWMIService = ObjGet("winmgmts:{impersonationLevel=Impersonate}!\\.\root\cimv2")
	If Not IsObj($objWMIService) Then
		fConsoleWrite("HKSUSPECT")
		Exit
	EndIf

	$colAdapters = $objWMIService.ExecQuery ("SELECT * FROM Win32_NetworkAdapterConfiguration WHERE IPEnabled = True")
	$read = ""
	For $objAdapter in $colAdapters
		For $i = 0 to UBound($objAdapter.IPAddress)-1
			If StringInStr($read, $objAdapter.MACAddress)==0 Then
				$read &= $objAdapter.MACAddress & @CRLF
			EndIf
		Next
	Next

	$macdashed = StringRegExp($read, '([0-9A-F]{2}:[0-9A-F]{2}:[0-9A-F]{2}:[0-9A-F]{2}:[0-9A-F]{2}:[0-9A-F]{2})', 3)
	If Not IsArray($macdashed) Then Return 0
	If $getmacindex < 1 Then Return 0
	If $getmacindex > UBound($macdashed) Or $getmacindex = -1 Then $getmacindex = UBound($macdashed)
	$macnosemicolon = StringReplace($macdashed[$getmacindex - 1], '-', ':', 0)
	Return $macnosemicolon
EndFunc   ;==>_GetMAC

Func EnterNewCode($trial=0)
	ClipPut($mac)
;~ 	If $RequiresRegCode Then
		$entered = InputBox($ProductName&$version, "ID: " & @CRLF & $mac & @CRLF & @CRLF & "Registration code (Hit Cancel for trial mode)", "", "", -1, 200)
;~ 		If @error = 1 Then Exit ;CANCEL WAS PRESSED
;~ 	Else
;~ 		$entered = $restore
;~ 	EndIf

	If $entered = $restore Then
		If $RequiresRegCode Then MsgBox(262144, $ProductName&$version, "Successfully Registered")

		IniWrite($ini, $dlliniSection, Encrypt("DecodeKey"), Encrypt($restore))
		IniWrite($ini, $dlliniSection, Encrypt("GeneratedMac"), Encrypt($mac))

	Else
		If Not $trial Then
			MsgBox(262144, $ProductName&$version, "Registration code is incorrect")
			Exit
		Else
			If $trialPeriod <> -1 Then
;~ 				MsgBox(262144, $applicationName, "Running in Trial mode.")
				IniWrite($ini, $dlliniSection, Encrypt("GeneratedMac"), Encrypt($mac))
				IniWrite($ini, $dlliniSection, Encrypt("RegDate"), Encrypt(_NowCalcDate()))
				$sysprep = _DateAdd("D", $trialPeriod, _NowCalcDate())
				IniWrite($ini, $dlliniSection, Encrypt("FinalDate"), Encrypt($sysprep))
				IniWrite($ini, $dlliniSection, Encrypt("LastRunDate"), Encrypt(_NowCalcDate()))
				IniWrite($ini, $dlliniSection, Encrypt("Count"), Encrypt(0))
			EndIf
		EndIf
	EndIf
EndFunc   ;==>EnterNewCode

Func VDebug ($var)
	If $VDebugit then ConsoleWrite ($var & @crlf)
EndFunc;==> VDebug to Scite()

Func CheckValidation()
	If $RequiresRegCode Then
		$RecordedMacCheck = StringLeft(StringRight(Decrypt(IniRead($ini, $dlliniSection, Encrypt("GeneratedMac"), "Weyland")), 5), 4)
		$MacCheck = StringLeft(StringRight($mac, 5), 4)
;~ 		ConsoleWrite($RecordedMacCheck & @cr)
;~ 		ConsoleWrite($MacCheck & @cr)
;~ 					IniWrite($ini, $dlliniSection, Encrypt("GeneratedMac"), Encrypt($mac))
;~ ConsoleWrite(IniRead($ini, $dlliniSection, Encrypt("GeneratedMac"), ""))
;~ Sleep(10000)
		If Not FileExists($ini) OR IniRead($ini, $dlliniSection, Encrypt("DecodeKey"), "")=="" Then
			ClipPut($mac)
			$entered = InputBox($applicationName, "ID: " & @CRLF & $mac & @CRLF & @CRLF & "Registration code (Hit Cancel for trial mode.)", "", "", -1, 200)
			If $entered = $restore Then
				If $RequiresRegCode Then
					MsgBox(262144, $applicationName, "Successfully Registered")
					IniWrite($ini, $dlliniSection, Encrypt("DecodeKey"), Encrypt($restore))
					IniWrite($ini, $dlliniSection, Encrypt("GeneratedMac"), Encrypt($mac))
					IniWrite($ini, $dlliniSection, Encrypt("pass"), Encrypt($mac))
					Return 1
				EndIf
			Else
				If $trialPeriod <> -1 Then
;~ 					MsgBox(262144, $applicationName, "Running in Trial mode.")
					IniWrite($ini, $dlliniSection, Encrypt("GeneratedMac"), Encrypt($mac))
					IniWrite($ini, $dlliniSection, Encrypt("RegDate"), Encrypt(_NowCalcDate()))
					$sysprep = _DateAdd("D", $trialPeriod, _NowCalcDate())
					IniWrite($ini, $dlliniSection, Encrypt("FinalDate"), Encrypt($sysprep))
					IniWrite($ini, $dlliniSection, Encrypt("LastRunDate"), Encrypt(_NowCalcDate()))
					IniWrite($ini, $dlliniSection, Encrypt("Count"), Encrypt(0))
					Return -1
				Else
					MsgBox(262144, $ProductName&$version, "Registration code is incorrect")
					Exit
				EndIf
			EndIf
		ElseIf $RecordedMacCheck <> $MacCheck Then
			VDebug("Recorded Mac = " & $RecordedMacCheck)
			VDebug("Macaddress = " & $MacCheck)
			VDebug("The PC code didn't match")
;~ 			If FileExists($ini) Then FileDelete($ini)
			$entered = InputBox($applicationName, "ID: " & @CRLF & $mac & @CRLF & @CRLF & "Registration code", "", "", -1, 200)
			If $entered = $restore Then
				If $RequiresRegCode Then MsgBox(262144, $applicationName, "Successfully Registered")
					IniWrite($ini, $dlliniSection, Encrypt("DecodeKey"), Encrypt($restore))
					IniWrite($ini, $dlliniSection, Encrypt("GeneratedMac"), Encrypt($mac))
					Return 1
				Else
					Exit
				EndIf
			Exit
		ElseIf $RecordedMacCheck == $MacCheck Then
			Return 1
		EndIf
	EndIf

	If $trialPeriod = -1 Then Return 1 ;There's no trial period... Go ahead and run

	$RecordedDecodeKey = IniRead($ini, $dlliniSection, Encrypt("pass"), "")==Encrypt($mac)
	If $RecordedDecodeKey Then Return 1
	If $ShowEvaluationWarning AND Not $RecordedDecodeKey Then
		Local $string = "ID: " & $mac & @CRLF & @CRLF & "Registration code"

		$finalDateCheck = Decrypt(IniRead($ini, $dlliniSection, Encrypt("FinalDate"), ""))
		$RegDateCheck = Decrypt(IniRead($ini, $dlliniSection, Encrypt("RegDate"), ""))
		$LastRunDateCheck = Decrypt(IniRead($ini, $dlliniSection, Encrypt("LastRunDate"), ""))
		$CountCheck = Number( Decrypt(IniRead($ini, $dlliniSection, Encrypt("Count"), "")))
		Local $Exit = False

		If _DateDiff("D", _NowCalcDate(), $finalDateCheck) < 0 Then
			;the date is $finalDateCheck days past the registration date
			VDebug("passed the " & $trialPeriod & " days. Expired " & _DateDiff("D", $finalDateCheck, _NowCalcDate()))
;~ 			SplashOff()
			$string = "Evaluation Expired" & @CRLF & $string
			IniWrite($ini, $dlliniSection, Encrypt("Count"), Encrypt($trialPeriod + 1))
;~ 			$entered = InputBox($applicationName, "Evaluation Expired" & @CRLF & "ID: " & @CRLF & $mac & @CRLF & @CRLF & "Registration code", "", "", -1, 200)
;~ 			If $entered = $restore Then
;~ 				If $RequiresRegCode Then MsgBox(262144, $applicationName, "Successfully Registered")
;~ 					IniWrite($ini, $dlliniSection, Encrypt("DecodeKey"), Encrypt($restore))
;~ 					IniWrite($ini, $dlliniSection, Encrypt("GeneratedMac"), Encrypt($mac))
;~ 					Return 1
;~ 				Else
;~ 					Exit
;~ 				EndIf
			$Exit = True
		EndIf

		If Not $Exit AND _DateDiff("D", $LastRunDateCheck, _NowCalcDate()) < 0 Then
			;they have changed the clock bacwards so expire
			VDebug("Clock date is set before last run date")
;~ 			SplashOff()
			$string = "Date tampering detected - Evaluation Expired"& @CRLF & $string
			IniWrite($ini, $dlliniSection, Encrypt("Count"), Encrypt($trialPeriod + 1))
;~ 			$entered = InputBox($applicationName, "Date tampering detected Evaluation Expired" & @CRLF & "ID: " & @CRLF & $mac & @CRLF & @CRLF & "Registration code", "", "", -1, 200)
;~ 			If $entered = $restore Then
;~ 				If $RequiresRegCode Then MsgBox(262144, $applicationName, "Successfully Registered")
;~ 					IniWrite($ini, $dlliniSection, Encrypt("DecodeKey"), Encrypt($restore))
;~ 					IniWrite($ini, $dlliniSection, Encrypt("GeneratedMac"), Encrypt($mac))
;~ 					Return 1
;~ 				Else
;~ 					Exit
;~ 				EndIf
			$Exit = True
		EndIf

		If Not $Exit AND $CountCheck >= $trialPeriod Then ;second method for checking days used
			;trialPeriod days are up so expire
			VDebug("Count is greater than " & $trialPeriod)
			SplashOff()
			$string = "Evaluation Expired" & @CRLF & $string
;~ 			$entered = InputBox($applicationName, "Evaluation Expired" & @CRLF & "ID: " & @CRLF & $mac & @CRLF & @CRLF & "Registration code", "", "", -1, 200)
;~ 			If $entered = $restore Then
;~ 				If $RequiresRegCode Then MsgBox(262144, $applicationName, "Successfully Registered")
;~ 					IniWrite($ini, $dlliniSection, Encrypt("DecodeKey"), Encrypt($restore))
;~ 					IniWrite($ini, $dlliniSection, Encrypt("GeneratedMac"), Encrypt($mac))
;~ 					Return 1
;~ 				Else
;~ 					Exit
;~ 				EndIf
			$Exit = True
		EndIf

		If  _NowCalcDate() <> $LastRunDateCheck Then ;if the date is different to the last ran date then increase the days used by 1
			$CountCheck += 1
			VDebug("Increase the used count; Count is now " & $CountCheck)
			IniWrite($ini, $dlliniSection, Encrypt("Count"), Encrypt($CountCheck))
			IniWrite($ini, $dlliniSection, Encrypt("LastRunDate"), Encrypt(_NowCalcDate()));log the new last run date
		EndIf

		If ($trialPeriod - $CountCheck) == 0 Then
			$string = "Evaluation Expired" & @CRLF & $string
			$Exit=True
		EndIf

		If Not $Exit Then $string = "Trial mode. "& ($trialPeriod - $CountCheck) &" day(s) remaining. " & @CRLF & $string & @CRLF & "(Cancel to continue trial)"
		ClipPut($mac)
		$entered = InputBox($applicationName, $string, "", "", -1, 200)
		If $entered = $restore Then
			If $RequiresRegCode Then
				MsgBox(262144, $applicationName, "Successfully Registered")
				IniWrite($ini, $dlliniSection, Encrypt("DecodeKey"), Encrypt($restore))
				IniWrite($ini, $dlliniSection, Encrypt("GeneratedMac"), Encrypt($mac))
				Return 1
			EndIf
		Else
			If $Exit Then
				Exit
			Else
				Return -1
			EndIf
		EndIf

	EndIf
	Return -1 ;If it gets this far - it's Trial software
EndFunc   ;==>CheckValidation

Func Encrypt($string)
	Return _StringEncrypt(1, $string, $dllEncKey, 1)
EndFunc   ;==>Encrypt


Func Decrypt($string)
	Return _StringEncrypt(0, $string, $dllEncKey, 1)
EndFunc   ;==>Decrypt

Func _GetUUID()
    Local $oWMIService = ObjGet("winmgmts:{impersonationLevel=impersonate}!\\localhost\root\cimv2")
    If Not IsObj($oWMIService) Then
        Return SetError(1, 0, -1)
    EndIf
    Local $oSysProd = $oWMIService.ExecQuery("Select * From Win32_ComputerSystemProduct")
    For $oSysProp In $oSysProd
        Return SetError(0, 0, $oSysProp.UUID)
    Next
    Return SetError(2, 0, -1)
EndFunc

#include <CheckOnline.au3>
*/
    }
}
