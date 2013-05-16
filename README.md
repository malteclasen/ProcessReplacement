ProcessReplacement
==================

I observed strange behavior on my notebook: At apparently random times, bcdedit.exe was started and used one CPU core at 100%. It did not stop on its own, and it refused to be terminated by force. ProcessExplorer reported only "access denied", Xperf showed no stack trace, only a full reboot could stop it. Anti-virus tools could not find anything, no trojan, no rootkit, no virus, no ad-ware. At this point, I decided to debug this in a somewhat unconventional way. I wrote this Process Replacement tool, which logs details about parent processes to the Windows event log when started. Then I registered it as a debugging replacement for bcdedit in the Windows registry:

[HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows NT\CurrentVersion\Image File Execution Options\bcdedit.exe]
"Debugger"="C:\\Users\\malte.clasen\\Documents\\Projects\\ProcessReplacement\\ProcessReplacement\\bin\\Release\\ProcessReplacement.exe"

Now everytime someone tried to run bcdedit.exe, ProcessReplacement is started instead. I quickly found out that the SoftThinks Agent Service was responsible, which is apparently a part of the Dell Backup and Recovery tool. Now I still have to work out how to deal with this, but at least I know where to start :)
