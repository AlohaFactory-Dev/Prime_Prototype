using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using UnityEditor;
using UnityEngine;
using UnityEngine.Assertions;

namespace Aloha.Coconut.ExcelEditor
{
    public class CoconutExcelException : Exception
    {
        public CoconutExcelException(string message) : base(message) { }
    }
    
    public class CoconutExcel : IDisposable
    {
        private readonly IWorkbook _book;
        private readonly string _filePath;

        public CoconutExcel(string filePath)
        {
            _filePath = filePath;
            using (FileStream fileStream = new FileStream(_filePath, FileMode.OpenOrCreate, FileAccess.Read))
            {
                _book = new XSSFWorkbook(fileStream);   
            }
        }

        public CoconutExcelSheet<TRecord> OpenSheet<TRecord>(string sheetName)
        {
            return new CoconutExcelSheet<TRecord>(_book, sheetName);
        }

        public void Save()
        {
            EditorUtility.DisplayProgressBar("Saving Excel", "Saving Excel...", 0.5f);
            using (FileStream fileStream = new FileStream(_filePath, FileMode.Open, FileAccess.Write))
            {
                _book.Write(fileStream);   
            }
            EditorUtility.ClearProgressBar();
        }

        public void Dispose()
        {
            _book.Dispose();
        }
    }
    
    public class CoconutExcelSheet<TRecord>
    {
        private readonly ISheet _sheet;

        private readonly List<FieldInfo> _header = new List<FieldInfo>();
        private readonly List<FieldInfo> _keys = new List<FieldInfo>();
        private readonly Dictionary<string, int> _columnIndex = new Dictionary<string, int>();
        
        private static readonly List<Type> AllowedColumnTypes = new List<Type>
        {
            typeof(string),
            typeof(int),
            typeof(float),
            typeof(double),
            typeof(decimal),
            typeof(bool)
        };
        
        public CoconutExcelSheet(IWorkbook workbook, string sheetName)
        {
            Type recordType = typeof(TRecord);
            // Find name of fields with Attribute [CSVColumn]
            foreach (var field in recordType.GetFields())
            {
                if (field.GetCustomAttributes(typeof(CSVColumnAttribute), false).Length > 0)
                {
                    Assert.IsTrue(AllowedColumnTypes.Contains(field.FieldType), "지원하지 않는 CSVColumn 타입입니다.");
                    _header.Add(field);
                }
                
                if (field.GetCustomAttributes(typeof(CSVKeyAttribute), false).Length > 0)
                {
                    _keys.Add(field);
                }
            }
            
            Assert.IsTrue(_keys.Count > 0, "CSVKeyAttribute가 적용된 필드가 존재하지 않습니다.");
            
            _sheet = workbook.GetSheet(sheetName);

            if (_sheet == null)
            {
                _sheet = workbook.CreateSheet(sheetName);
                IRow row = _sheet.CreateRow(0);
                for (int i = 0; i < _header.Count; i++)
                {
                    ICell cell = row.CreateCell(i);
                    cell.SetCellValue(_header[i].Name);
                }
            }
            
            foreach (ICell cell in _sheet.GetRow(0).Cells)
            {
                Assert.IsTrue(cell.CellType == CellType.String || cell.CellType == CellType.Blank);
                Assert.IsTrue(_header.Exists(f => f.Name == cell.StringCellValue), "Header 정보가 실제 테이블과 일치하지 않습니다.");
                _columnIndex.Add(cell.StringCellValue, cell.ColumnIndex);
            }
        }

        public void Create(TRecord record)
        {
            if (RetrieveRow(GetKeyQuery(record)) != null)
            {
                throw new CoconutExcelException($"동일한 Key{QueryToString(GetKeyQuery(record))}의 레코드가 이미 존재합니다.");
            }
            
            IRow newRow = _sheet.CreateRow(_sheet.LastRowNum + 1);
            UpdateRow(newRow, record);
        }

        private void UpdateRow(IRow row, TRecord record)
        {
            foreach (FieldInfo header in _header)
            {
                ICell cell = row.GetCell(_columnIndex[header.Name]);
                if (cell == null) cell = row.CreateCell(_columnIndex[header.Name]);
                
                object value = header.GetValue(record);
                switch (value)
                {
                    case string stringValue:
                        cell.SetCellValue(stringValue);
                        break;
                    case int intValue:
                        cell.SetCellValue(intValue);
                        break;
                    case float floatValue:
                        cell.SetCellValue(floatValue);
                        break;
                    case double doubleValue:
                        cell.SetCellValue(doubleValue);
                        break;
                    case decimal decimalValue:
                        cell.SetCellValue((double)decimalValue);
                        break;
                    case bool boolValue:
                        cell.SetCellValue(boolValue);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }
        
        private (string, object)[] GetKeyQuery(TRecord record)
        {
            List<(string, object)> keyValues = new List<(string, object)>();
            foreach (FieldInfo key in _keys)
            {
                keyValues.Add((key.Name, key.GetValue(record)));
            }

            return keyValues.ToArray();
        }
        
        public TRecord Retrieve(params (string, object)[] q)
        {
            IRow row = RetrieveRow(q);
            if (row == null)
            {
                Debug.LogError($"Key{QueryToString(q)}의 레코드가 존재하지 않습니다.");
                return default;
            }
            
            return ConvertRow(row);
        }

        public List<TRecord> RetrieveAll(int maxCount = int.MaxValue, params (string, object)[] q)
        {
            List<IRow> rows = RetrieveRows(q, maxCount);
            List<TRecord> records = new List<TRecord>();
            foreach (IRow row in rows)
            {
                records.Add(ConvertRow(row));
            }

            return records;
        }
        
        private string QueryToString((string, object)[] q)
        {
            string result = "[";
            foreach ((string, object) pair in q)
            {
                result += $"{pair.Item1}:{pair.Item2}" + ", ";
            }
            result = result.Substring(0, result.Length - 2);
            result += "]";

            return result;
        }

        public void Update(TRecord record, bool createIfNotExist = false)
        {
            IRow targetRow = RetrieveRow(GetKeyQuery(record));
            if (targetRow == null)
            {
                if (createIfNotExist)
                {
                    Create(record);
                    return;
                }
                else
                {
                    throw new CoconutExcelException($"Key{GetKeyQuery(record)}의 레코드가 존재하지 않습니다.");   
                }
            }
            
            UpdateRow(targetRow, record);
        }

        public void Delete(params (string, object)[] q)
        {
            IRow row = RetrieveRow(q);
            if (row != null) _sheet.RemoveRow(row);
        }
        
        public void DeleteAll(params (string, object)[] q)
        {
            if (q.Length == 0)
            {
                throw new CoconutExcelException("하나 이상의 쿼리를 넣어주세요.");
            }
            
            List<IRow> rows = RetrieveRows(q, int.MaxValue);
            foreach (IRow row in rows)
            {
                _sheet.RemoveRow(row);
            }
        }

        private IRow RetrieveRow((string, object)[] q)
        {
            List<IRow> rows = RetrieveRows(q, 1);
            return rows.Count > 0 ? rows[0] : null;
        }

        private List<IRow> RetrieveRows((string, object)[] q, int maxCount)
        {
            List<IRow> rows = new List<IRow>();
            
            for (int i = 1; i <= _sheet.LastRowNum; i++)
            {
                IRow row = _sheet.GetRow(i);
                if (row == null) continue;

                int matchCount = 0;
                for (int j = 0; j < q.Length; j++)
                {
                    object queryValue = q[j].Item2;
                    ICell cell = row.GetCell(_columnIndex[q[j].Item1]);
                    bool isMatch = false;
                    
                    switch (cell.CellType)
                    {
                        case CellType.Numeric:
                            double value = cell.NumericCellValue;
                            if (_keys[j].FieldType == typeof(int) && (int)value == (int)queryValue) isMatch = true;
                            if (_keys[j].FieldType == typeof(float) && (float)value == (float)queryValue) isMatch = true;
                            if (_keys[j].FieldType == typeof(double) && value == (double)queryValue) isMatch = true;
                            if (_keys[j].FieldType == typeof(decimal) && (decimal)value == (decimal)queryValue) isMatch = true;
                            break;
                        case CellType.String:
                            if (_keys[j].FieldType == typeof(string) && cell.StringCellValue == (string)queryValue) isMatch = true;
                            break;
                        case CellType.Boolean:
                            if (_keys[j].FieldType == typeof(bool) && cell.BooleanCellValue == (bool)queryValue) isMatch = true;
                            break;
                        case CellType.Blank:
                            break;
                        case CellType.Unknown:
                        case CellType.Formula:
                        case CellType.Error:
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                    
                    if (isMatch) matchCount++;
                    else break;
                }
                
                if (matchCount == q.Length) rows.Add(row);
                if (rows.Count >= maxCount) break;
            }
            
            return rows;
        }

        private TRecord ConvertRow(int rowIndex)
        {
            Assert.IsTrue(rowIndex > 0, "헤더 행은 제외하고 1부터 시작합니다.");
            IRow row = _sheet.GetRow(rowIndex);
            Assert.IsNotNull(row, "해당 행이 존재하지 않습니다.");
            
            return ConvertRow(row);
        }

        private TRecord ConvertRow(IRow row)
        {
            // struct의 경우, boxing을 하지 않으면 값이 정상적으로 set되지 않음
            // 따라서 CreateInstance<T>가 아닌 CreateInstance(typeof(T))로 생성
            object record = Activator.CreateInstance(typeof(TRecord));
            
            foreach (FieldInfo field in typeof(TRecord).GetFields())
            {
                if (field.GetCustomAttributes(typeof(CSVColumnAttribute), false).Length > 0)
                {
                    CellType cellType = row.GetCell(_columnIndex[field.Name]).CellType;
                    switch (cellType)
                    {
                        case CellType.Numeric:
                            double value = row.GetCell(_columnIndex[field.Name]).NumericCellValue;
                            if (field.FieldType == typeof(int)) field.SetValue(record, (int)value);
                            else if (field.FieldType == typeof(float)) field.SetValue(record, (float)value);
                            else if (field.FieldType == typeof(double)) field.SetValue(record, value);
                            else if (field.FieldType == typeof(decimal)) field.SetValue(record, (decimal)value);
                            break;
                        case CellType.String:
                            Assert.IsTrue(field.FieldType == typeof(string));
                            field.SetValue(record, row.GetCell(_columnIndex[field.Name]).StringCellValue);
                            break;
                        case CellType.Boolean:
                            Assert.IsTrue(field.FieldType == typeof(bool));
                            field.SetValue(record, row.GetCell(_columnIndex[field.Name]).BooleanCellValue);
                            break;
                        case CellType.Blank:
                            break;
                        case CellType.Unknown:
                        case CellType.Formula:
                        case CellType.Error:
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                }
            }
            
            return (TRecord)record;
        }
    }
}
