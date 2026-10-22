using System;
using System.Collections.Generic;
namespace IonFrontier
{
    public sealed partial class IonGame
    {
        [Serializable] public sealed class DamageEntry {public string source;public float damage;}
        readonly Dictionary<string,float> damageSources=new Dictionary<string,float>();
        public string LastDamageSource {get;private set;}="";
        void RecordDamageSource(string source,float damage){LastDamageSource=source;if(!damageSources.ContainsKey(source))damageSources[source]=0;damageSources[source]+=damage;}
        void ResetDamageSources(){damageSources.Clear();LastDamageSource="";}
        public DamageEntry[] DamageBreakdown(){var result=new List<DamageEntry>();foreach(var pair in damageSources)result.Add(new DamageEntry{source=pair.Key,damage=pair.Value});return result.ToArray();}
    }
}
