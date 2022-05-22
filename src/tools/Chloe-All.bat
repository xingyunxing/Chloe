
del /f /s /q packages\*
rd packages
md packages

.\nuget pack Chloe.nuspec -OutputDirectory ./packages
.\nuget pack Chloe.Extension.nuspec -OutputDirectory ./packages
.\nuget pack Chloe.MySql.nuspec -OutputDirectory ./packages
.\nuget pack Chloe.Oracle.nuspec -OutputDirectory ./packages
.\nuget pack Chloe.PostgreSQL.nuspec -OutputDirectory ./packages
.\nuget pack Chloe.SQLite.nuspec -OutputDirectory ./packages
.\nuget pack Chloe.SqlServer.nuspec -OutputDirectory ./packages

pause