using System;
using System.IO;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditor.Build.Reporting;
using UnityEngine.Rendering;
using IonFrontier;

public static class IonBuild
{
    static double readyAfter;
    [InitializeOnLoadMethod]
    static void WatchRequestedBuild()
    {
        readyAfter=EditorApplication.timeSinceStartup+5;
        EditorApplication.update+=CheckRequestedBuild;
    }
    static void CheckRequestedBuild()
    {
        if(EditorApplication.isCompiling||EditorApplication.isUpdating||EditorApplication.timeSinceStartup<readyAfter)return;
        if(!File.Exists("ion-build.request"))return;
        EditorApplication.update-=CheckRequestedBuild;
        string output=File.ReadAllText("ion-build.request").Trim();
        if(File.Exists("ion-build.running"))File.Delete("ion-build.running"); File.Move("ion-build.request","ion-build.running");
        try { BuildWindows(output);File.WriteAllText("ion-build.done","Succeeded"); }
        catch(Exception e){File.WriteAllText("ion-build.done",e.ToString());Debug.LogException(e);}
    }
    [MenuItem("Ion Frontier/Build Windows")]
    public static void Windows()
    {
        var args=Environment.GetCommandLineArgs();int index=Array.IndexOf(args,"-ion-build");
        string output=index>=0&&index+1<args.Length?args[index+1]:Path.GetFullPath(Path.Combine(Application.dataPath,"../Builds/Windows/IonFrontier.exe"));
        BuildWindows(output);
    }
    static void BuildWindows(string destination)
    {
        var importer=AssetImporter.GetAtPath("Assets/IonFrontier/Resources/ion-fleet.png") as TextureImporter;
        if(importer==null)throw new Exception("Missing spacecraft atlas");
        importer.textureType=TextureImporterType.Default;importer.alphaIsTransparency=true;importer.mipmapEnabled=false;
        importer.wrapMode=TextureWrapMode.Clamp;importer.filterMode=FilterMode.Bilinear;importer.maxTextureSize=2048;
        importer.textureCompression=TextureImporterCompression.Uncompressed;importer.SaveAndReimport();
        if(Resources.Load<IonFleetLibrary>("FleetLibrary")==null)throw new Exception("Missing FleetLibrary asset");
        var scene=EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);
        var cameraObject=new GameObject("Flight camera");cameraObject.tag="MainCamera";
        var camera=cameraObject.AddComponent<Camera>();camera.orthographic=true;camera.orthographicSize=7;cameraObject.transform.position=new Vector3(0,0,-10);
        cameraObject.AddComponent<AudioListener>();
        new GameObject("Ion Frontier").AddComponent<IonGame>();
        Directory.CreateDirectory("Assets/IonFrontier/Scenes");
        string scenePath="Assets/IonFrontier/Scenes/Flight.unity";EditorSceneManager.SaveScene(scene,scenePath);
        EditorBuildSettings.scenes=new[]{new EditorBuildSettingsScene(scenePath,true)};
        PlayerSettings.companyName="Vanguard Studio";PlayerSettings.productName="ION FRONTIER";PlayerSettings.bundleVersion="0.11.1";
        PlayerSettings.SetApplicationIdentifier(UnityEditor.Build.NamedBuildTarget.Standalone,"com.vanguard.ionfrontier");
        PlayerSettings.defaultScreenWidth=1280;PlayerSettings.defaultScreenHeight=720;
        PlayerSettings.fullScreenMode=FullScreenMode.Windowed;PlayerSettings.resizableWindow=true;PlayerSettings.runInBackground=false;
        PlayerSettings.SetScriptingBackend(UnityEditor.Build.NamedBuildTarget.Standalone,ScriptingImplementation.Mono2x);
        PlayerSettings.SetUseDefaultGraphicsAPIs(BuildTarget.StandaloneWindows64,false);
        PlayerSettings.SetGraphicsAPIs(BuildTarget.StandaloneWindows64,new[]{GraphicsDeviceType.Direct3D11});
        // The project is a lightweight unlit 2D shooter; avoid the old URP scene dependencies.
        GraphicsSettings.defaultRenderPipeline=null;
        int qualityLevel=QualitySettings.GetQualityLevel(); for(int i=0;i<QualitySettings.names.Length;i++){ QualitySettings.SetQualityLevel(i,false); QualitySettings.renderPipeline=null; } QualitySettings.SetQualityLevel(qualityLevel,false);
        QualitySettings.vSyncCount=1;PlayerSettings.colorSpace=ColorSpace.Gamma;
        PlayerSettings.SplashScreen.show=false;
        AssetDatabase.SaveAssets();
        Directory.CreateDirectory(Path.GetDirectoryName(destination));
        var report=BuildPipeline.BuildPlayer(new BuildPlayerOptions{scenes=new[]{scenePath},locationPathName=destination,target=BuildTarget.StandaloneWindows64,options=BuildOptions.None});
        Debug.Log("ION_BUILD_RESULT: "+report.summary.result+"; bytes="+report.summary.totalSize);
        if(report.summary.result!=BuildResult.Succeeded)throw new Exception("Ion Frontier Windows build failed");
    }
}

