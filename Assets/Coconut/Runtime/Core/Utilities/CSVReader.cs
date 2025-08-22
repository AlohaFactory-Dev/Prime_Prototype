using System;
using System.Collections.Generic;
using System.Globalization;
using System.Numerics;
using System.Reflection;
using System.Text.RegularExpressions;
using Aloha.Coconut;
using UnityEngine;

public class CSVReader
{
    static string SPLIT_RE = @",(?=(?:[^""]*""[^""]*"")*(?![^""]*""))";
    static string LINE_SPLIT_RE = @"\r\n|\n\r|\n|\r";
    static char[] TRIM_CHARS = { '\"' };

    public static List<Dictionary<string, object>> ReadResource(string path)
    {
        var textAsset = Resources.Load<TextAsset>(path);
        if (textAsset != null) return ReadCSV(textAsset.text);

        Debug.LogError($"Failed to read resource at {path}");
        return new List<Dictionary<string, object>>();
    }

    public static List<Dictionary<string, object>> ReadTextAsset(TextAsset textAsset)
    {
        return ReadCSV(textAsset.text);
    }

    public static List<Dictionary<string, object>> ReadCSV(string text)
    {
        var list = new List<Dictionary<string, object>>();
        var lines = Regex.Split(text, LINE_SPLIT_RE);

        if (lines.Length <= 1) return list;

        var header = Regex.Split(lines[0], SPLIT_RE);
        for (var i = 1; i < lines.Length; i++)
        {
            if (string.IsNullOrEmpty(lines[i])) continue;

            var values = Regex.Split(lines[i], SPLIT_RE);
            if (values.Length == 0) continue;

            var entry = new Dictionary<string, object>();
            for (var j = 0; j < header.Length && j < values.Length; j++)
            {
                if (header[j].StartsWith("//")) continue;

                string value = values[j];
                value = value.TrimStart(TRIM_CHARS).TrimEnd(TRIM_CHARS).Replace("\\", "");

                value = value.Replace("<br>", "\n"); // 추가된 부분. 개행문자를 \n대신 <br>로 사용한다.
                value = value.Replace("<c>", ",");

                object finalvalue = value;
                ulong u;
                int n;
                float f;

                BigInteger bi;

                if (header[j].EndsWith("_big"))
                {
                    if (BigInteger.TryParse(value, out bi))
                    {
                        finalvalue = bi;
                    }
                }
                else if (ExponentialParser.TryParse(value, out bi))
                {
                    finalvalue = bi;
                }
                else if (int.TryParse(value, out n))
                {
                    finalvalue = n;
                }
                else if (float.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out f))
                {
                    finalvalue = f;
                }
                else if (ulong.TryParse(value, out u))
                {
                    finalvalue = u;
                }
                else if (DateTime.TryParseExact(value, "yyyy-MM-dd H:mm", CultureInfo.InvariantCulture,
                    DateTimeStyles.None, out var d))
                {
                    finalvalue = d;
                }

                entry[header[j]] = finalvalue;
            }

            list.Add(entry);
        }

        return list;
    }

    public static List<T> ReadResource<T>(string path)
    {
        var textAsset = Resources.Load<TextAsset>(path);
        if (textAsset != null) return ReadTextAsset<T>(textAsset);

        Debug.LogError($"Failed to read resource at {path}");
        return new List<T>();
    }

    public static List<T> ReadTextAsset<T>(TextAsset textAsset)
    {
        return ReadTable<T>(ReadCSV(textAsset.text));
    }

    public static List<T> ReadCSV<T>(string text)
    {
        return ReadTable<T>(ReadCSV(text));
    }

    private static List<T> ReadTable<T>(List<Dictionary<string, object>> table)
    {
        var fields = typeof(T).GetFields();
        var result = new List<T>();

        for (int rowIndex = 0; rowIndex < table.Count; rowIndex++)
        {
            var row = table[rowIndex];
            var rowObject = Activator.CreateInstance(typeof(T));
            foreach (FieldInfo fieldInfo in fields)
            {
                var columnAttribute = fieldInfo.GetCustomAttribute<CSVColumnAttribute>();
                if (columnAttribute != null)
                {
                    if (!row.ContainsKey(fieldInfo.Name)) continue;

                    var value = row[fieldInfo.Name];
                    if (value == "" && fieldInfo.FieldType != typeof(string)) continue;

                    object convertedValue = null;
                    try
                    {
                        if (fieldInfo.FieldType.IsEnum)
                        {
                            if (value is string v) convertedValue = Enum.Parse(fieldInfo.FieldType, v);
                            else convertedValue = Enum.ToObject(fieldInfo.FieldType, value);
                        }
                        else
                        {
                            if (fieldInfo.FieldType == typeof(int) && value is BigInteger bi)
                            {
                                convertedValue = (int)bi;
                            }
                            else if (fieldInfo.FieldType == typeof(BigInteger) && value is int i)
                            {
                                convertedValue = (BigInteger)i;
                            }
                            else if (fieldInfo.FieldType.IsGenericType && fieldInfo.FieldType.GetGenericTypeDefinition() == typeof(List<>))
                            {
                                var elementType = fieldInfo.FieldType.GetGenericArguments()[0];
                                if (value is string stringValue && stringValue.StartsWith("[") && stringValue.EndsWith("]"))
                                {
                                    var listValues = stringValue.Trim('[', ']').Split(';');
                                    var convertedList = Activator.CreateInstance(fieldInfo.FieldType) as System.Collections.IList;

                                    foreach (var listValue in listValues)
                                    {
                                        object convertedElement;
                                        if (elementType.IsEnum)
                                        {
                                            convertedElement = Enum.Parse(elementType, listValue);
                                        }
                                        else
                                        {
                                            convertedElement = Convert.ChangeType(listValue, elementType);
                                        }

                                        convertedList?.Add(convertedElement);
                                    }

                                    convertedValue = convertedList;
                                }
                                else
                                {
                                    convertedValue = null;
                                }
                            }
                            else
                            {
                                convertedValue = Convert.ChangeType(value, fieldInfo.FieldType);
                            }
                        }

                        fieldInfo.SetValue(rowObject, convertedValue);
                    }
                    catch (Exception ex)
                    {
                        Debug.LogError($"CSV 파싱 오류: {typeof(T).Name} {rowIndex + 1}행, {fieldInfo.Name} 열, 값: {value} - {ex.Message}");
                    }
                }
            }

            result.Add((T)rowObject);
        }

        return result;
    }
}

public class CSVColumnAttribute : System.Attribute
{
}

public class CSVKeyAttribute : System.Attribute
{
}