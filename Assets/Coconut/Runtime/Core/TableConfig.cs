using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Serialization;

namespace Aloha.Coconut
{
    [CreateAssetMenu(fileName = "TableConfig", menuName = "Coconut/Config/TableConfig")]
    public class TableConfig : CoconutConfig
    {
        [VerticalGroup("Root Folder")]
        [PropertyOrder(1)]
        [ReadOnly]
        [PropertySpace(SpaceBefore = 0, SpaceAfter = 20)]
        public string rootFolderAddress;


#if UNITY_EDITOR
        [FormerlySerializedAs("StartRow")]
        [InfoBox("몇 행부터 데이터를 불러올지 설정\n" +
            " 1부터 시작")]
        [BoxGroup("Import", order: 0)]
        [Range(1, 100)]
        public int startRow = 1;

        public bool IsRootFolderAddressable { get; set; }
        [NonSerialized] public string commandBuffer;

        [InfoBox("Root Folder Path 아래의 csv파일들을 TableManager.Get<T>([fileName])으로 로드할 수 있도록 설정\n" +
            "Root Folder Path를 입력하고, 반드시 해당 Root Folder를 Addressable로 설정")]
        [PropertyOrder(0)]
        [VerticalGroup("Root Folder")]
        [ValidateInput(nameof(IsRootFolderAddressable), "Root Folder is not Addressable.")]
        [FolderPath(RequireExistingPath = true)]
        public string rootFolderPath;

        [InfoBox("CSV 임포트 시 # 또는 //로 시작하는 시트와 컬럼은 무시됨!\n" +
            "string과 숫자형 데이터 타입에 \'-\'을 넣으면 TableManager.MagicNumber로 변환됨!\n" +
            "변수명 맨 뒤에 숫자를 붙이면 리스트로 불러옴\n" +
            "ex)int value0, int value1 = List<int> value\n" +
            "사용하지 않는 값(int,float,string)에 '-'을 넣으면 매직넘버 -99999로 변환 됨 TableManager 참고\n")]
        [BoxGroup("Import", order: 1)]
        [FoldoutGroup("Import/Excel", order: 1), Button]
        public void ImportExcel()
        {
            commandBuffer = "importExcel";
        }

        [Serializable]
        public class GoogleSheetInfo
        {
            public string alias;

            [InlineButton("Import"), InlineButton("Open")]
            public string sheetId;

            [NonSerialized] public TableConfig tableConfig;

            public void Import()
            {
                tableConfig.commandBuffer = $"importGoogleSheet {sheetId}";
            }

            public void Open()
            {
                Application.OpenURL("https://docs.google.com/spreadsheets/d/" + sheetId);
            }
        }

        [FoldoutGroup("Import/Google", order: 11)]
        public string googleClientId;

        [FoldoutGroup("Import/Google", order: 12)]
        public string googleClientSecret;

        [InfoBox("CSV 임포트 시 # 또는 //로 시작하는 시트와 컬럼은 무시됨!\n" +
            "string과 숫자형 데이터 타입에 \'-\'을 넣으면 TableManager.MagicNumber로 변환됨!\n" +
            "변수명 맨 뒤에 숫자를 붙이면 리스트로 불러옴\n" +
            "ex)int value0, int value1 = List<int> value\n" +
            "사용하지 않는 값(int,float,string)에 '-'을 넣으면 매직넘버 -99999로 변환 됨 TableManager 참고\n")]
        [FoldoutGroup("Import/Google", order: 13)]
        public List<GoogleSheetInfo> googleSheetInfos;

        private void OnEnable()
        {
            if (googleSheetInfos != null)
            {
                foreach (var googleSheetInfo in googleSheetInfos)
                {
                    googleSheetInfo.tableConfig = this;
                }
            }
        }

        private void OnValidate()
        {
            if (googleSheetInfos != null)
            {
                foreach (var googleSheetInfo in googleSheetInfos)
                {
                    googleSheetInfo.tableConfig = this;
                }
            }
        }

        [UnityEditor.MenuItem("Coconut/Select Table Config", priority = 10)]
        public static void SelectTableConfig()
        {
            UnityEditor.Selection.activeObject = CoconutConfig.Get<TableConfig>();
        }
#endif
    }
}