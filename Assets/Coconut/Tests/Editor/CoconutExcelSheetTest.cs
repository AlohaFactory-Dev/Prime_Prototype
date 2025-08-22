using System.IO;
using Aloha.Coconut.ExcelEditor;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using NUnit.Framework;

public class CoconutExcelSheetTest
{
    private struct TestRecord
    {
        [CSVColumn, CSVKey] public int id;
        [CSVColumn] public string name;
        [CSVColumn] public int age;
        [CSVColumn] public decimal height;
    }
    
    private struct TestRecordMultiKey
    {
        [CSVColumn, CSVKey] public int classId;
        [CSVColumn, CSVKey] public int id;
        [CSVColumn] public string name;
        [CSVColumn] public int age;
        [CSVColumn] public decimal height;
    }
    
    private string _filePath = "Library/sample.xlsx";
    private CoconutExcel _excel;
    
    [SetUp]
    public void SetUp()
    {
        if (System.IO.File.Exists(_filePath))
        {
            System.IO.File.Delete(_filePath);
        }
        
        // 테스트를 위한 빈 Excel file 생성, 생성된 파일이 없으면 열 수 없음
        IWorkbook book = new XSSFWorkbook();
        book.CreateSheet("a");
        using(FileStream fs = new FileStream(_filePath, FileMode.Create, FileAccess.Write))
        {
            book.Write(fs);
        }
        
        _excel = new CoconutExcel(_filePath);
    }
    
    [Test]
    public void SimpleCreate()
    {
        CoconutExcelSheet<TestRecord> sheet = _excel.OpenSheet<TestRecord>("test");
        sheet.Create(new TestRecord {id = 1, name = "Alice", age = 20, height = 160.5m});
        sheet.Create(new TestRecord {id = 2, name = "Bob", age = 25, height = 170.5m});
        sheet.Create(new TestRecord {id = 3, name = "Charlie", age = 30, height = 180.5m});
        
        Assert.AreEqual(sheet.Retrieve(("id", 1)).name, "Alice");
        Assert.AreEqual(sheet.Retrieve(("id", 2)).name, "Bob");
        Assert.AreEqual(sheet.Retrieve(("id", 3)).name, "Charlie");
    }

    [Test]
    public void CreateKeyDuplicationFail()
    {
        CoconutExcelSheet<TestRecord> sheet = _excel.OpenSheet<TestRecord>("test");
        sheet.Create(new TestRecord {id = 1, name = "Alice", age = 20, height = 160.5m});

        Assert.Catch<CoconutExcelException>(() =>
            sheet.Create(new TestRecord { id = 1, name = "Bob", age = 25, height = 170.5m }));
    }
    
    [Test]
    public void SimpleUpdateTest()
    {
        CoconutExcelSheet<TestRecord> sheet = _excel.OpenSheet<TestRecord>("test");
        sheet.Create(new TestRecord {id = 1, name = "Alice", age = 20, height = 160.5m});
        sheet.Create(new TestRecord {id = 2, name = "Bob", age = 25, height = 170.5m});
        sheet.Create(new TestRecord {id = 3, name = "Charlie", age = 30, height = 180.5m});
        
        sheet.Update(new TestRecord {id = 1, name = "Alice", age = 25, height = 165.5m});
        Assert.AreEqual(sheet.Retrieve(("id", 1)).age, 25);
    }
    
    [TearDown]
    public void TearDown()
    {
        _excel.Save();
        _excel.Dispose();
    }
}
