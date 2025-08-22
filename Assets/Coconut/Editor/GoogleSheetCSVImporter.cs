using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;
using Google.Apis.Sheets.v4;
using Google.Apis.Sheets.v4.Data;
using Newtonsoft.Json;
using UnityEditor;
using UnityEngine;
using Progress = UnityEditor.Progress;

namespace Aloha.Coconut.Editor
{
    [JsonObject(MemberSerialization.OptIn)]
    public class GoogleSheetCSVImporter : ICSVImporter
    {
        public CSVImporterType Type => CSVImporterType.Google;

        [JsonProperty] private readonly string _clientId;
        [JsonProperty] private readonly string _clientSecret;
        [JsonProperty] private readonly string _sheetId;
        [JsonProperty] private readonly string _rootTableFolder;
        [JsonProperty] private readonly int _startRow;

        private UserCredential _credential;
        private SheetsService _sheetsService;
        private Spreadsheet _spreadsheet;

        [JsonConstructor]
        public GoogleSheetCSVImporter() { }

        public GoogleSheetCSVImporter(string clientId, string clientSecret, string sheetId, string rootTableFolder, int startRow)
        {
            _clientId = clientId;
            _clientSecret = clientSecret;
            _sheetId = sheetId;
            _rootTableFolder = rootTableFolder;
            _startRow = startRow;
        }

        public async UniTask Setup()
        {
            var progressId = Progress.Start("Opening Google Sheet");
            Progress.Report(progressId, .5f, "Opening Google Sheet");

            var pass = new ClientSecrets
            {
                ClientId = _clientId,
                ClientSecret = _clientSecret
            };

            var scopes = new string[] { SheetsService.Scope.SpreadsheetsReadonly };
            _credential = await GoogleWebAuthorizationBroker.AuthorizeAsync(pass, scopes, pass.ClientId, CancellationToken.None);

            _sheetsService = new SheetsService(new BaseClientService.Initializer
            {
                HttpClientInitializer = _credential
            });

            _spreadsheet = await _sheetsService.Spreadsheets.Get(_sheetId).ExecuteAsync();

            Progress.Finish(progressId);
        }

        public List<string> GetSheetTitles()
        {
            return _spreadsheet.Sheets.Where(sheet => !sheet.Properties.Title.StartsWith("#")).Select(s => s.Properties.Title).ToList();
        }

        public void ReadTables(List<string> sheetTitles, Action<bool> onComplete)
        {
            AsyncReadTables(sheetTitles, onComplete).Forget();
        }

        private async UniTask AsyncReadTables(List<string> sheetTitles, Action<bool> onComplete)
        {
            var readTasks = sheetTitles
                .Select(ReadTable)
                .ToList();

            await Task.WhenAll(readTasks);
            EditorUtility.DisplayDialog("GoogleSheetCSVImporter", $"테이블 {sheetTitles.Count}개 임포트 완료.", "확인");
            onComplete?.Invoke(true);
            AssetDatabase.Refresh();
        }

        private string GetColumnName(int index)
        {
            var dividend = index + 1;
            var columnName = "";
            while (dividend > 0)
            {
                var modulo = (dividend - 1) % 26;
                columnName = Convert.ToChar(65 + modulo) + columnName;
                dividend = (dividend - modulo - 1) / 26;
            }

            return columnName;
        }

        private async Task ReadTable(string sheetTitle)
        {
            var lastRow = -1;
            var lastCol = -1;
            try
            {
                var request = _sheetsService.Spreadsheets.Values.Get(_sheetId, sheetTitle + $"!A{_startRow}:CA");
                var values = (await request.ExecuteAsync()).Values;

                var headerRow = values[0];
                var columns = new List<string>();
                var columnMappings = new Dictionary<int, string>();

                for (var i = 0; i < headerRow.Count; i++)
                {
                    var header = (string)headerRow[i];
                    if (header.StartsWith('#') || header.StartsWith("//")) continue;

                    var match = Regex.Match(header, @"^(.*?)(\d+)$");
                    if (match.Success)
                    {
                        var baseName = match.Groups[1].Value;
                        if (!columns.Contains(baseName))
                        {
                            columns.Add(baseName);
                        }
                    }
                    else
                    {
                        columns.Add(header);
                    }

                    columnMappings[i] = header;
                }

                var csvContent = string.Join(",", columns) + "\n";
                for (var i = 1; i < values.Count; i++)
                {
                    lastRow = i;
                    var row = values[i];
                    if (row[0] is string s && (s.StartsWith('#') || s.StartsWith("//"))) continue;

                    var rowContents = new List<string>(new string[columns.Count]);
                    foreach (var columnIndex in columnMappings.Keys)
                    {
                        lastCol = columnIndex;
                        try
                        {
                            if (row.Count > columnIndex)
                            {
                                var content = row[columnIndex]?.ToString() ?? "";

                                if (content == "-")
                                {
                                    content = TableManager.MAGIC_NUMBER.ToString();
                                }

                                content = content.Replace(",", "<c>").Replace("\n", "<br>");

                                var header = columnMappings[columnIndex];
                                var match = Regex.Match(header, @"^(.*?)(\d+)$");
                                if (match.Success)
                                {
                                    var baseName = match.Groups[1].Value;
                                    var groupIndex = columns.IndexOf(baseName);
                                    if (rowContents[groupIndex] == null || rowContents[groupIndex] == "")
                                    {
                                        rowContents[groupIndex] = $"[{content}";
                                    }
                                    else
                                    {
                                        rowContents[groupIndex] += $";{content}";
                                    }
                                }
                                else
                                {
                                    var index = columns.IndexOf(header);
                                    rowContents[index] = content;
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            Debug.LogError($"에러: 시트 '{sheetTitle}', {i + 1}행, {GetColumnName(columnIndex)}열에서 발생\n{ex}");
                            throw;
                        }
                    }

                    // 리스트로 묶인 항목 닫기
                    for (var j = 0; j < rowContents.Count; j++)
                    {
                        if (rowContents[j]?.StartsWith("[") == true)
                        {
                            rowContents[j] += "]";
                        }
                    }

                    csvContent += string.Join(",", rowContents) + "\n";
                }

                // 저장
                var filePath = Path.Combine(_rootTableFolder, sheetTitle) + ".csv";
                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                }

                using var csv = new StreamWriter(filePath, false);
                csv.Write(csvContent);
                csv.Close();
            }
            catch (Exception e)
            {
                Debug.LogError($"[ReadTable] 에러: 시트 '{sheetTitle}', {lastRow + 1}행, {GetColumnName(lastCol)}열에서 발생\n{e}");
                EditorUtility.DisplayDialog("GoogleSheetCSVImporter", $"에러 발생 (시트: {sheetTitle}, {lastRow + 1}행, {GetColumnName(lastCol)}열)\n{e.Message}", "확인");
            }
        }
    }
}