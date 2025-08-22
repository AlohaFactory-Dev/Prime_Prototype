using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Build;

public class CoconutSymbolsConfig : EditorWindow
{
    private class ExtensionSymbolState
    {
        public string symbol;
        public bool isOn;
    }

    private List<ExtensionSymbolState> _extensionStates;
    
    private List<string> _extensionSymbols = new List<string>
    {
        "COCONUT_ALOHA_SDK",
        "COCONUT_DURIAN"
    };
    
    [MenuItem("Coconut/SymbolsConfig", priority = 0)]
    public static void ShowWindow()
    {
        GetWindow<CoconutSymbolsConfig>("Coconut Symbols Config");
    }

    private void OnEnable()
    {
        _extensionStates = new List<ExtensionSymbolState>();

        foreach (var symbol in _extensionSymbols)
        {
            _extensionStates.Add(new ExtensionSymbolState
            {
                symbol = symbol,
                isOn = ContainsScriptingDefineSymbol(symbol)
            });
        }
    }

    private void OnGUI()
    {
        foreach (var extension in _extensionStates)
        {
            var prevIsOn = extension.isOn;
            extension.isOn = EditorGUILayout.Toggle(extension.symbol, extension.isOn);
            if(prevIsOn && !extension.isOn)
            {
                RemoveScriptingDefineSymbol(extension.symbol);
                AssetDatabase.Refresh();
            }
            else if(!prevIsOn && extension.isOn)
            {
                AddScriptingDefineSymbol(extension.symbol);
                AssetDatabase.Refresh();
            }
        }
    }
    
    private bool ContainsScriptingDefineSymbol(string symbol)
    {
        var buildTargets = new[] {NamedBuildTarget.Android, NamedBuildTarget.iOS, NamedBuildTarget.Standalone};

        foreach (var buildTarget in buildTargets)
        {
            var symbols = PlayerSettings.GetScriptingDefineSymbols(buildTarget);
            if (symbols.Contains(symbol))
            {
                return true;
            }
        }

        return false;
    }

    private void AddScriptingDefineSymbol(string symbol)
    {
        var buildTargets = new[] {NamedBuildTarget.Android, NamedBuildTarget.iOS, NamedBuildTarget.Standalone};

        foreach (var buildTarget in buildTargets)
        {
            var symbols = PlayerSettings.GetScriptingDefineSymbols(buildTarget);
            if (symbols.Contains(symbol))
            {
                continue;
            }

            symbols += ";" + symbol;
            PlayerSettings.SetScriptingDefineSymbols(buildTarget, symbols);   
        }
    }
    
    private void RemoveScriptingDefineSymbol(string symbol)
    {
        var buildTargets = new[] {NamedBuildTarget.Android, NamedBuildTarget.iOS, NamedBuildTarget.Standalone};

        foreach (var buildTarget in buildTargets)
        {
            var symbols = PlayerSettings.GetScriptingDefineSymbols(buildTarget);
            if (!symbols.Contains(symbol))
            {
                continue;
            }

            symbols = symbols.Replace(";" + symbol, "");
            PlayerSettings.SetScriptingDefineSymbols(buildTarget, symbols);   
        }
    }
}
