using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;

public enum CSVImporterType
{
    Excel,
    Google
}

public interface ICSVImporter
{
    CSVImporterType Type { get; }
    UniTask Setup();
    List<string> GetSheetTitles();
    void ReadTables(List<string> sheetTitles, Action<bool> onComplete);
}
