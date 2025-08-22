using UnityToolbarExtender;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using Aloha.Coconut;
using Aloha.Coconut.Editor;

[InitializeOnLoad]
public static class DataTableReadToolBar
{
    static DataTableReadToolBar()
    {
        ToolbarExtender.RightToolbarGUI.Add(DataTableSet);
    }

    private static void DataTableSet()
    {
        GUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace(); // 버튼들을 오른쪽으로 정렬

        // Import 버튼
        if (GUILayout.Button(new GUIContent("Import Sheet", "Google Sheet 데이터를 가져오기"), GUILayout.Width(100f)))
        {
            var tableConfig = CoconutConfig.Get<TableConfig>();
            if (tableConfig != null)
            {
                if (tableConfig.googleSheetInfos != null)
                {
                    foreach (var sheetInfo in tableConfig.googleSheetInfos)
                    {
                        if (!string.IsNullOrEmpty(sheetInfo.sheetId))
                        {
                            CSVImportSheetCheckBox.Open(new GoogleSheetCSVImporter(
                                tableConfig.googleClientId,
                                tableConfig.googleClientSecret,
                                sheetInfo.sheetId,
                                tableConfig.rootFolderPath,
                                tableConfig.startRow
                            ));
                            Debug.Log($"Google Sheet {sheetInfo.alias} 데이터를 가져오는 창을 열었습니다.");
                        }
                        else
                        {
                            Debug.LogError("Sheet ID가 비어 있습니다.");
                        }
                    }
                }
                else
                {
                    Debug.LogError("GoogleSheetInfo가 설정되지 않았습니다.");
                }
            }
            else
            {
                Debug.LogError("TableConfig가 설정되지 않았습니다.");
            }
        }

        // Open 버튼
        if (GUILayout.Button(new GUIContent("Open Sheet", "Google Sheet 열기"), GUILayout.Width(100f)))
        {
            var tableConfig = CoconutConfig.Get<TableConfig>();
            if (tableConfig != null && tableConfig.googleSheetInfos != null)
            {
                foreach (var sheetInfo in tableConfig.googleSheetInfos)
                {
                    if (!string.IsNullOrEmpty(sheetInfo.sheetId))
                    {
                        sheetInfo.Open();
                        Debug.Log($"Google Sheet {sheetInfo.alias}를 열었습니다.");
                    }
                    else
                    {
                        Debug.LogError("Sheet ID가 비어 있습니다.");
                    }
                }
            }
            else
            {
                Debug.LogError("TableConfig 또는 GoogleSheetInfo가 설정되지 않았습니다.");
            }
        }

        GUILayout.EndHorizontal();
    }
}