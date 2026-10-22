using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
namespace IonFrontier
{
    public sealed class IonFrameMetrics : MonoBehaviour
    {
        [Serializable] public sealed class Report
        {
            public string version="0.11.1",scope="All positive frame times after 1.5s warm-up; includes screenshots and resizes; main-thread allocation only";
            public int frames,over33ms,over50ms,over100ms,peakProjectiles,peakEnemies,peakWorld,gc0,gc1,gc2,focusedFrames,backgroundFrames;
            public double meanFps,p50ms,p95ms,p99ms,maxMs,seconds,focusedSeconds,focusedMeanFps,focusedP95ms,focusedP99ms,focusedMaxMs;public long allocatedMainThreadBytes,peakManagedBytes;public bool allocationSupported;
        }
        readonly List<double> samples=new List<double>(),focused=new List<double>();readonly Report report=new Report();IonGame game;float begin;long allocationStart;int g0,g1,g2;bool started;
        void Start(){game=GetComponent<IonGame>();begin=Time.realtimeSinceStartup;}
        void Update()
        {
            if(Time.realtimeSinceStartup-begin<1.5f)return;
            if(!started){started=true;allocationStart=GC.GetAllocatedBytesForCurrentThread();report.allocationSupported=allocationStart>0;g0=GC.CollectionCount(0);g1=GC.CollectionCount(1);g2=GC.CollectionCount(2);}
            double dt=Time.unscaledDeltaTime;if(dt<=0)return;samples.Add(dt*1000);report.frames++;report.seconds+=dt;report.maxMs=Math.Max(report.maxMs,dt*1000);
            if(dt>.033)report.over33ms++;if(dt>.05)report.over50ms++;if(dt>.1)report.over100ms++;
            if(Application.isFocused){focused.Add(dt*1000);report.focusedFrames++;report.focusedSeconds+=dt;report.focusedMaxMs=Math.Max(report.focusedMaxMs,dt*1000);}else report.backgroundFrames++;
            report.peakManagedBytes=Math.Max(report.peakManagedBytes,UnityEngine.Profiling.Profiler.GetMonoUsedSizeLong());
            if(game!=null){report.peakProjectiles=Math.Max(report.peakProjectiles,game.ActiveBullets);report.peakEnemies=Math.Max(report.peakEnemies,game.ActiveEnemies);report.peakWorld=Math.Max(report.peakWorld,game.ActiveWorldCount);}
        }
        public void Write()
        {
            var values=samples.ToArray();Array.Sort(values);if(values.Length>0){report.p50ms=values[(int)((values.Length-1)*.50)];report.p95ms=values[(int)((values.Length-1)*.95)];report.p99ms=values[(int)((values.Length-1)*.99)];}
            var fg=focused.ToArray();Array.Sort(fg);if(fg.Length>0){report.focusedP95ms=fg[(int)((fg.Length-1)*.95)];report.focusedP99ms=fg[(int)((fg.Length-1)*.99)];}report.focusedMeanFps=report.focusedSeconds>0?report.focusedFrames/report.focusedSeconds:0;
            report.meanFps=report.seconds>0?report.frames/report.seconds:0;report.allocatedMainThreadBytes=started&&report.allocationSupported?Math.Max(0,GC.GetAllocatedBytesForCurrentThread()-allocationStart):-1;report.gc0=GC.CollectionCount(0)-g0;report.gc1=GC.CollectionCount(1)-g1;report.gc2=GC.CollectionCount(2)-g2;
            var args=Environment.GetCommandLineArgs();int at=Array.IndexOf(args,"-ion-captures");string path=at>=0&&at+1<args.Length?args[at+1]:Application.persistentDataPath;Directory.CreateDirectory(path);File.WriteAllText(Path.Combine(path,"frame-metrics.json"),JsonUtility.ToJson(report,true));
        }
        void OnApplicationQuit(){Write();}
    }
}
