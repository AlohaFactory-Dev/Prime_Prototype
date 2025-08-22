// https://alohafactoryworkspace.slack.com/archives/C026XNPN4LA/p1729496054194109
// Durian API 코드 뽑는 절차 자동화한 코드

using System;
using System.Collections;
using System.Diagnostics;
using System.IO;
using UnityEditor;

public class DurianAPIUpdater
{
    private const string API_URL_TARGET = "https://aih-aloha-demo-api-1094360770825.asia-northeast1.run.app";
    
    [MenuItem("Durian/Update API")]
    public static void UpdateAPI()
    {
        // 파일 오픈 패널로 프로젝트 경로 찾기
        string durianPath = EditorUtility.OpenFolderPanel("Select Durian Path", "", "");

        if (!File.Exists($"{durianPath}/build.gradle.kts"))
        {
            EditorUtility.DisplayDialog("Error", "Invalid Durian Path - cannot find build.gradle.kts", "OK");
            return;
        }

        if (!File.Exists($"{durianPath}/gradlew.bat"))
        {
            EditorUtility.DisplayDialog("Error", "Invalid Durian Path - cannot find gradlew.bat", "OK");
            return;
        }

        if (!File.Exists($"{durianPath}/gradlew"))
        {
            EditorUtility.DisplayDialog("Error", "Invalid Durian Path - cannot find gradlew", "OK");
            return;
        }
        
        // build.gradle.kts 파일에서 src((apiUrl ?: "http://localhost:8080") + "/docs/v1")를 찾아 localhost를 API_URL_TARGET으로 변경
        string buildGradleKtsPath = $"{durianPath}/build.gradle.kts";
        string buildGradleKtsOriginalText = File.ReadAllText(buildGradleKtsPath);
        string buildGradleKtsText = 
            buildGradleKtsOriginalText.Replace("src((apiUrl ?: \"http://localhost:8080\") + \"/docs/v1\")", $"src((apiUrl ?: \"{API_URL_TARGET}\") + \"/docs/v1\")");
        
#if UNITY_EDITOR_WIN
        // Window의 경우, npx를 찾지 못하고 npx.cmd를 찾아서 실행해야 함
        buildGradleKtsText = buildGradleKtsText.Replace("\"npx\"", "\"npx.cmd\"");
#endif
        File.WriteAllText(buildGradleKtsPath, buildGradleKtsText);
        
        // ./gradlew bootRun
        EditorUtility.DisplayProgressBar("Durian API Updater", "bootRun", 0.5f);
        ProcessStartInfo processStartInfo = new ProcessStartInfo();
        processStartInfo.WorkingDirectory = durianPath;
        processStartInfo.FileName = Path.Combine(durianPath, "gradlew.bat");
        processStartInfo.UseShellExecute = false;
        processStartInfo.CreateNoWindow = true;
        processStartInfo.Arguments = "bootRun";
        
        IDictionary systemEnvironmentVarialbes = Environment.GetEnvironmentVariables(EnvironmentVariableTarget.Machine);
        foreach (DictionaryEntry entry in systemEnvironmentVarialbes)
        {
            processStartInfo.EnvironmentVariables[(string)entry.Key] = (string)entry.Value;
        }

        Process bootProcess = Process.Start(processStartInfo);
        bootProcess.OutputDataReceived += (sender, args) => EditorUtility.DisplayProgressBar("Durian API Updater", args.Data, 0.5f);
        
        bootProcess.WaitForExit();
        if (bootProcess.ExitCode != 0)
        {
            EditorUtility.DisplayDialog("Error", "Failed to bootRun", "OK");
            return;
        }
        
        // ./gradlew generateOpenApiCSharpClient
        EditorUtility.DisplayProgressBar("Durian API Updater", "generateOpenApiCSharpClient", 0.8f);
        processStartInfo.Arguments = "generateOpenApiCSharpClient";
        Process generateProcess = Process.Start(processStartInfo);
        generateProcess.WaitForExit();
        if (generateProcess.ExitCode != 0)
        {
            EditorUtility.DisplayDialog("Error", "Failed to generateOpenApiCSharpClient", "OK");
            return;
        }
        
        // 의존 관계 보존을 위해, Assets/Durian/API/Alohacorp.Durian.asmdef의 .meta 파일을 Temp 폴더에 백업
        string metaPath = "Assets/Durian/API/Alohacorp.Durian/Alohacorp.Durian.asmdef.meta";
        string metaBackupPath = "Temp/Alohacorp.Durian.asmdef.meta";
        File.Copy(metaPath, metaBackupPath, true);
        
        // Durian 프로젝트 경로 아래 build/openapi-client/unityWebRequest/Alohacorp.Durian 폴더를 Assets/Durian/API/Alohacorp.Durian 복사
        Directory.Delete("Assets/Durian/API/Alohacorp.Durian", true);
        Directory.Move($"{durianPath}/build/openapi-client/unityWebRequest/src/Alohacorp.Durian", "Assets/Durian/API/Alohacorp.Durian");
        
        // 백업한 .meta 파일을 Assets/Durian/API/Alohacorp.Durian 아래로 복사
        File.Copy(metaBackupPath, metaPath);        
        
        // build.gradle.kts 복구
        File.WriteAllText(buildGradleKtsPath, buildGradleKtsOriginalText);
    }
}
