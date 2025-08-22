using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Cysharp.Threading.Tasks;
using ExcelDataReader;
using Newtonsoft.Json;
using UnityEditor;
using UnityEngine;

namespace Aloha.Coconut.Editor
{
    [JsonObject(MemberSerialization.OptIn)]
    public class ExcelCSVImporter : ICSVImporter
    {
        public CSVImporterType Type => CSVImporterType.Excel;

        [JsonProperty] private readonly string _excelFilePath;
        [JsonProperty] private readonly string _rootFolderPath;
        [JsonProperty] private readonly int _startRow;
        private DataSet _dataSet;

        [JsonConstructor]
        public ExcelCSVImporter() { }

        public ExcelCSVImporter(string excelFilePath, string rootFolderPath, int startRow)
        {
            _excelFilePath = excelFilePath;
            _rootFolderPath = rootFolderPath;
            _startRow = startRow;
        }

        public async UniTask Setup()
        {
            if (_dataSet == null) InitializeDataSet();
        }

        // https://stackoverflow.com/questions/2536181/is-there-any-simple-way-to-convert-xls-file-to-csv-file-excel
        public List<string> GetSheetTitles()
        {
            try
            {
                var result = new List<string>();
                for (var tableIndex = 0; tableIndex < _dataSet.Tables.Count; tableIndex++)
                {
                    var table = _dataSet.Tables[tableIndex];
                    // #로 시작하는 시트는 스킵
                    if (table.TableName.StartsWith("#")) continue;

                    EditorUtility.DisplayProgressBar("ExcelCSVImporter", $"시트 이름들 읽어오는 중...",
                        (float)(tableIndex + 1) / _dataSet.Tables.Count);

                    result.Add(table.TableName);
                }

                return result;
            }
            catch (Exception e)
            {
                EditorUtility.DisplayDialog("ExcelCSVImporter", $"에러 발생\n{e.Message}", "확인");
                Debug.LogError(e);

                throw;
            }
            finally
            {
                EditorUtility.ClearProgressBar();
            }
        }

        private void InitializeDataSet()
        {
            // 원드라이브에 올라온 파일의 경우, 파일이 열려 있을 때 읽기 전용으로 접근해도 sharing violation 발생
            // 복사본을 temp 폴더에 생성한 후 해당 파일을 이용하는 것으로 해결
            var copiedFilePath = Path.Combine("Temp", Path.GetFileName(_excelFilePath));
            if (File.Exists(copiedFilePath)) File.Delete(copiedFilePath);
            File.Copy(_excelFilePath, copiedFilePath);

            try
            {
                using var stream = new FileStream(copiedFilePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                IExcelDataReader reader = null;
                if (_excelFilePath.EndsWith(".xls"))
                {
                    reader = ExcelReaderFactory.CreateBinaryReader(stream);
                }
                else if (_excelFilePath.EndsWith(".xlsx") || _excelFilePath.EndsWith(".xlsm"))
                {
                    reader = ExcelReaderFactory.CreateOpenXmlReader(stream);
                }

                if (reader == null) return;

                EditorUtility.DisplayProgressBar("ExcelCSVImporter", "엑셀 파일 읽어오는 중...", 0);

                _dataSet = reader.AsDataSet(new ExcelDataSetConfiguration
                {
                    ConfigureDataTable = (tableReader) => new ExcelDataTableConfiguration
                    {
                        UseHeaderRow = true, // 헤더 행 사용 설정
                        ReadHeaderRow = (rowReader) =>
                        {
                            // _startRow로 이동
                            for (var i = 0; i < _startRow - 1; i++)
                            {
                                rowReader.Read();
                            }
                        },
                        FilterColumn = (rowReader, columnIndex) =>
                        {
                            var value = rowReader.GetValue(columnIndex);
                            if (value == null || string.IsNullOrEmpty(value.ToString())) return false;
                            if (value is string s && (s.StartsWith("#") || s.StartsWith("//"))) return false;
                            return true;
                        },
                        FilterRow = (rowReader) =>
                        {
                            if (rowReader.GetValue(0) is string s) return !(s.StartsWith('#') || s.StartsWith("//"));
                            return true;
                        }
                    }
                });
            }
            catch (Exception e)
            {
                EditorUtility.DisplayDialog("ExcelCSVImporter", $"에러 발생\n{e.Message}", "확인");
                Debug.LogError(e);
            }
            finally
            {
                if (File.Exists(copiedFilePath)) File.Delete(copiedFilePath);
                EditorUtility.ClearProgressBar();
            }
        }

        public void ReadTables(List<string> sheetTitles, Action<bool> onComplete)
        {
            InitializeDataSet();
            try
            {
                var sheetCount = 0;
                for (var tableIndex = 0; tableIndex < _dataSet.Tables.Count; tableIndex++)
                {
                    var table = _dataSet.Tables[tableIndex];
                    if (!sheetTitles.Contains(table.TableName)) continue;

                    EditorUtility.DisplayProgressBar("ExcelCSVImporter", $"시트 {table.TableName} 읽어오는 중...",
                        (float)(tableIndex + 1) / _dataSet.Tables.Count);
                    sheetCount++;

                    // Column 입력
                    var csvContent = string.Empty;
                    var columns = new List<string>();
                    var columnMappings = new Dictionary<int, string>();

                    for (var i = 0; i < table.Columns.Count; i++)
                    {
                        var columnName = table.Columns[i].ColumnName;
                        if (columnName.StartsWith("#") || columnName.StartsWith("//")) continue;

                        var match = Regex.Match(columnName, @"^(.*?)(\d+)$");
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
                            columns.Add(columnName);
                        }

                        columnMappings[i] = columnName;
                    }

                    csvContent += string.Join(",", columns) + "\n";

                    // Row 입력
                    for (var rowIndex = 0; rowIndex < table.Rows.Count; rowIndex++)
                    {
                        var row = table.Rows[rowIndex];
                        var rowContents = new List<string>(new string[columns.Count]);

                        for (var columnIndex = 0; columnIndex < table.Columns.Count; columnIndex++)
                        {
                            if (!columnMappings.ContainsKey(columnIndex)) continue;

                            var content = row[columnIndex]?.ToString() ?? "";

                            // "-"를 매직넘버로 변환
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
                    var filePath = Path.Combine(_rootFolderPath, table.TableName) + ".csv";
                    if (File.Exists(filePath)) File.Delete(filePath);

                    using var csv = new StreamWriter(filePath, false, Encoding.UTF8);
                    csv.Write(csvContent);
                    csv.Close();
                }

                AssetDatabase.Refresh();
                onComplete?.Invoke(true);
                EditorUtility.DisplayDialog("ExcelCSVImporter", $"테이블 {sheetCount}개 임포트 완료.", "확인");
            }
            catch (Exception e)
            {
                onComplete?.Invoke(false);
                EditorUtility.DisplayDialog("ExcelCSVImporter", $"에러 발생\n{e.Message}", "확인");
                Debug.LogError(e);
                throw;
            }
            finally
            {
                EditorUtility.ClearProgressBar();
            }
        }
    }
}