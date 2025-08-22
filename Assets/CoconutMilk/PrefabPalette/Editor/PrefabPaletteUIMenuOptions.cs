using UnityEditor;
using Aloha.Coconut;

namespace Aloha.CoconutMilk.Editor
{
    public static class PrefabPaletteUIMenuOptions
    {
        private enum PriorityOrder
        {
            CoconutCanvas = 0,
            Button = 1,
            Text = 2,
            Dim = 3,
            SafeArea = 5,
            
            PopupWindow = 11,
        }
        
        private const string MENU_PATH = "GameObject/CoconutUI/";
        
        private static PrefabPalette PrefabPalette => CoconutConfig.Get<PrefabPalette>();
        
        [MenuItem(MENU_PATH + "CoconutCanvas", false, (int)PriorityOrder.CoconutCanvas)]
        public static void CreateCoconutCanvas()
        {
            PrefabPalette.coconutCanvas.Instantiate();
        }
        
        [MenuItem(MENU_PATH + "Button", false, (int)PriorityOrder.Button)]
        public static void CreateButton()
        {
            PrefabPalette.button.Instantiate();
        }
        
        [MenuItem(MENU_PATH + "Text", false, (int)PriorityOrder.Text)]
        public static void CreateText()
        {
            PrefabPalette.text.Instantiate();
        }
        
        [MenuItem(MENU_PATH + "Dim", false, (int)PriorityOrder.Dim)]
        public static void CreateDim()
        {
            PrefabPalette.dim.Instantiate();
        }
        
        [MenuItem(MENU_PATH + "SafeArea", false, (int)PriorityOrder.SafeArea)]
        public static void CreateSafeArea()
        {
            PrefabPalette.safeArea.Instantiate();
        }
        
        [MenuItem(MENU_PATH + "PopupWindow", false, (int)PriorityOrder.PopupWindow)]
        public static void CreatePopupWindow()
        {
            PrefabPalette.popupWindow.Instantiate();
        }
        
        [MenuItem(MENU_PATH + "BarGauge", false, (int)PriorityOrder.PopupWindow + 1)]
        public static void CreateBarGauge()
        {
            PrefabPalette.barGauge.Instantiate();
        }
    }
}