using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using Newtonsoft.Json;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using UnityEngine;

namespace Aloha.Coconut.Editor
{
    public class CSVImportSheetCheckBox : OdinEditorWindow
    {
        [ReadOnly, SerializeField] private CSVImporterType _csvImporterType;
        [ReadOnly, SerializeField] private string _csvImporterConfig;
        [OnValueChanged("OnSelectAllChanged")] public bool selectAll;
    
        public Dictionary<string, bool> checkBoxes = new ();
        [NonSerialized] private Dictionary<string, bool> _resultCheckBoxes = new ();
    
        private bool IsDownloading { get; set; }
        private ICSVImporter _csvImporter; 

        public static void Open(ICSVImporter csvImporter)
        {
            var window = GetWindow<CSVImportSheetCheckBox>();
            window.checkBoxes.Clear();
            window._csvImporter = csvImporter;
            window._csvImporterConfig = JsonConvert.SerializeObject(csvImporter);
            window._csvImporterType = csvImporter.Type;
            window.IsDownloading = true;

            csvImporter.Setup()
                .ContinueWith(() =>
                {
                    foreach (var sheetTitle in csvImporter.GetSheetTitles())
                    {
                        window.checkBoxes.Add(sheetTitle, false);
                    }
                    window.IsDownloading = false;
                });
        }

        private void OnSelectAllChanged()
        {
            var keys = checkBoxes.Keys.ToList();
            foreach (var key in keys)
            {
                checkBoxes[key] = selectAll;
            }
        }

        [Button, DisableIf("IsDownloading")]
        public void Import()
        {
            if (_csvImporter == null)
            {
                CreateCSVImporter().ContinueWith(Import);
                return;
            }
            
            _resultCheckBoxes = new Dictionary<string, bool>(checkBoxes);
            IsDownloading = true;
            _csvImporter.ReadTables(checkBoxes.Where(c => c.Value).Select(c => c.Key).ToList(), 
                result => IsDownloading = false);
        }

        private async UniTask CreateCSVImporter()
        {
            if (_csvImporterType == CSVImporterType.Excel)
            {
                _csvImporter = JsonConvert.DeserializeObject<ExcelCSVImporter>(_csvImporterConfig);
            }
            else if (_csvImporterType == CSVImporterType.Google)
            {
                _csvImporter = JsonConvert.DeserializeObject<GoogleSheetCSVImporter>(_csvImporterConfig);
            }
            else
            {
                throw new ArgumentOutOfRangeException();    
            }
            
            await _csvImporter.Setup();
        }
        
        [Button]
        public void Refresh()
        {
            checkBoxes.Clear();
            CreateCSVImporter().ContinueWith(() =>
            {
                foreach (var sheetTitle in _csvImporter.GetSheetTitles())
                {
                    checkBoxes.Add(sheetTitle, false);
                }
            });
        }
    }
}