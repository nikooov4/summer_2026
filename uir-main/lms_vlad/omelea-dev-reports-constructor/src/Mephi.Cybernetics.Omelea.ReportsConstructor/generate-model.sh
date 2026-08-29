dotnet build ./../Mephi.Cybernetics.Omelea.ReportsConstructor.DbScripts/
# почему-то если reverse engineer-ить из dacpac то dbdiagram.md не рендерится 
# (нормально рендерится если получать из развёрнутой бд)
dotnet efcpt "./../Mephi.Cybernetics.Omelea.ReportsConstructor.DbScripts/bin/Debug/Mephi.Cybernetics.Omelea.ReportsConstructor.DbScripts.dacpac" mssql