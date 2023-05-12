
cd ..
dotnet clean -c Release
dotnet build -c Release

cd tools

del /f /s /q packages\*
rd packages
md packages

.\nuget pack Chloe.nuspec -OutputDirectory ./packages -OutputFileNamesWithoutVersion
.\nuget pack Chloe.Extension.nuspec -OutputDirectory ./packages -OutputFileNamesWithoutVersion
.\nuget pack Chloe.MySql.nuspec -OutputDirectory ./packages -OutputFileNamesWithoutVersion
.\nuget pack Chloe.Oracle.nuspec -OutputDirectory ./packages -OutputFileNamesWithoutVersion
.\nuget pack Chloe.PostgreSQL.nuspec -OutputDirectory ./packages -OutputFileNamesWithoutVersion
.\nuget pack Chloe.SQLite.nuspec -OutputDirectory ./packages -OutputFileNamesWithoutVersion
.\nuget pack Chloe.SqlServer.nuspec -OutputDirectory ./packages -OutputFileNamesWithoutVersion
.\nuget pack Chloe.Dameng.nuspec -OutputDirectory ./packages -OutputFileNamesWithoutVersion

pause