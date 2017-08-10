# C# service for NTP synchronization
This solution contains a C# project which will create a windows service (which will run on startup) and will synchronize date and time according to the specified NTP server

Settings (NTP.exe.config):
- NTP_Server: represent the URL address to the NTP server
- UTC_Offset: represent the offset according to the UTC time zone. For example, the offset for CEST time zone is 2