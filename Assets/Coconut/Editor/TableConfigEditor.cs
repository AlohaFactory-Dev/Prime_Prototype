using System.IO;
using Aloha.Coconut;
using Aloha.Coconut.Editor;
using Sirenix.OdinInspector.Editor;
using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEngine;

[CustomEditor(typeof(TableConfig))]
public class TableConfigEditor : OdinEditor
{
    private TableConfig _tableConfig;

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        _tableConfig = (TableConfig)target;
        var prevRootFolderAddress = _tableConfig.rootFolderAddress;

        _tableConfig.IsRootFolderAddressable = false;

        if (ValidateRootFolderPath(_tableConfig))
        {
            var settings = AddressableAssetSettingsDefaultObject.Settings;
            foreach (var group in settings.groups)
            {
                foreach (var entry in group.entries)
                {
                    if (entry.AssetPath == _tableConfig.rootFolderPath)
                    {
                        _tableConfig.IsRootFolderAddressable = true;
                        _tableConfig.rootFolderAddress = entry.address;
                    }
                }
            }
        }

        if (!_tableConfig.IsRootFolderAddressable || !ValidateRootFolderPath(_tableConfig))
        {
            _tableConfig.rootFolderAddress = "";
        }

        if (prevRootFolderAddress != _tableConfig.rootFolderAddress)
        {
            EditorUtility.SetDirty(_tableConfig);
        }

        var command = _tableConfig.commandBuffer;
        _tableConfig.commandBuffer = "";

        if (!string.IsNullOrEmpty(command))
        {
            if (command == "importExcel")
            {
                var lastLoadedExcelFilePath = GetLastExcelFilePath();
                string filePath = null;

                if (!string.IsNullOrEmpty(lastLoadedExcelFilePath))
                {
                    if (EditorUtility.DisplayDialog("Import Excel",
                        $"마지막으로 불러온 파일을 다시 불러오시겠습니까?\n{lastLoadedExcelFilePath}", "Yes", "No"))
                    {
                        filePath = lastLoadedExcelFilePath;
                    }
                }

                if (filePath == null)
                {
                    var directory = string.IsNullOrEmpty(lastLoadedExcelFilePath)
                        ? Application.dataPath
                        : Path.GetDirectoryName(GetLastExcelFilePath());

                    filePath = EditorUtility.OpenFilePanel("Import Excel", directory, "xls,xlsx,xlsm");
                }

                if (!string.IsNullOrEmpty(filePath))
                {
                    CSVImportSheetCheckBox.Open(new ExcelCSVImporter(filePath, _tableConfig.rootFolderPath, _tableConfig.startRow));
                    SetLastExcelFilePath(filePath);
                }
            }

            var split = command.Split(' ');
            if (split[0] == "importGoogleSheet")
            {
                CSVImportSheetCheckBox.Open(new GoogleSheetCSVImporter(_tableConfig.googleClientId, _tableConfig.googleClientSecret,
                    split[1], _tableConfig.rootFolderPath, _tableConfig.startRow));
            }
        }
    }

    private string GetLastExcelFilePath()
    {
        if (File.Exists("Library/LastExcelFilePath"))
        {
            return File.ReadAllText("Library/LastExcelFilePath");
        }

        return null;
    }

    private void SetLastExcelFilePath(string filePath)
    {
        File.WriteAllText("Library/LastExcelFilePath", filePath);
    }

    private bool ValidateRootFolderPath(TableConfig tableConfig)
    {
        if (string.IsNullOrEmpty(tableConfig.rootFolderPath) || !Directory.Exists(tableConfig.rootFolderPath))
        {
            return false;
        }

        return true;
    }
}