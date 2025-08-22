using System.Collections.Generic;
using System.IO;
using System.Text;

public static class CSVWriter
{
    public static void Write<T>(string filePath, List<T> dataList)
    {
        var fields = typeof(T).GetFields();

        var result = new List<string[]>();
        string[] header = new string[fields.Length];

        for (int i = 0; i < fields.Length; ++i)
        {
            header[i] = fields[i].Name;
        }

        result.Add(header);

        foreach (var data in dataList)
        {
            string[] row = new string[fields.Length];
            for (int i = 0; i < fields.Length; ++i)
            {
                var fieldInfo = fields[i];
                var value = fieldInfo.GetValue(data);
                if (fieldInfo.FieldType == typeof(bool))
                {
                    value = value.ToString().ToUpper();
                }

                row[i] = value == null ? "" : value.ToString();
            }

            result.Add(row);
        }

        string delimiter = ",";
        StringBuilder stringBuilder = new StringBuilder();
        foreach (var row in result)
        {
            for (int i = 0; i < row.Length; i++)
            {
                stringBuilder.Append(row[i]);
                if (i < row.Length - 1)
                {
                    stringBuilder.Append(delimiter);
                }
            }

            stringBuilder.Append("\n");
        }

        StreamWriter outStream = File.CreateText(filePath);
        outStream.Write(stringBuilder);
        outStream.Close();
    }
}