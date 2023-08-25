@ECHO OFF

xcopy /q /y /s ..\CHANGELOG SignPadPicker-v%1\CHANGELOG\
xcopy /q /y /s ..\bin\x86\Release\SignPadPicker.dll SignPadPicker-v%1\publish\SignPadPicker\
xcopy /q /y /s ..\bin\x86\Release\SignPadPicker.ScreenSignPadAdaptor.dll SignPadPicker-v%1\publish\SignPadPicker.ScreenSignPadAdaptor\

xcopy /q /y /s ..\bin\x86\Release\SignPadPicker.KisSignPadAdaptor.dll SignPadPicker-v%1\publish\SignPadPicker.KisSignPadAdaptor\
xcopy /q /y /s ..\bin\x86\Release\KisDongleDll.dll SignPadPicker-v%1\publish\SignPadPicker.KisSignPadAdaptor\

xcopy /q /y /s ..\bin\x86\Release\SignPadPicker.KscatSignPadAdaptor.dll SignPadPicker-v%1\publish\SignPadPicker.KscatSignPadAdaptor\
xcopy /q /y /s ..\bin\x86\Release\ksnetcomm.dll SignPadPicker-v%1\publish\SignPadPicker.KscatSignPadAdaptor\

xcopy /q /y /s ..\bin\x86\Release\SignPadPicker.NicePosSignPadAdaptor.dll SignPadPicker-v%1\publish\SignPadPicker.NicePosSignPadAdaptor\
xcopy /q /y /s ..\bin\x86\Release\NICEPOSICV105.dll SignPadPicker-v%1\publish\SignPadPicker.NicePosSignPadAdaptor\

xcopy /q /y /s ..\bin\x86\Release\SignPadPicker.SmartroSignPadAdaptor.dll SignPadPicker-v%1\publish\SignPadPicker.SmartroSignPadAdaptor\
xcopy /q /y /s ..\bin\x86\Release\libeay32_smt.dll SignPadPicker-v%1\publish\SignPadPicker.SmartroSignPadAdaptor\
xcopy /q /y /s ..\bin\x86\Release\ssleay32_smt.dll SignPadPicker-v%1\publish\SignPadPicker.SmartroSignPadAdaptor\
xcopy /q /y /s ..\bin\x86\Release\SmartroSign.dll SignPadPicker-v%1\publish\SignPadPicker.SmartroSignPadAdaptor\
