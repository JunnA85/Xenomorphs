using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;

namespace Aliensvspredator.ThingDefs
{
    [DefOf]
    public static class XenoThingDefOf
    {
        // Token: 0x060080D3 RID: 32979 RVA: 0x002E20E7 File Offset: 0x002E02E7
        static XenoThingDefOf()
        {
            DefOfHelper.EnsureInitializedInCtor(typeof(ThingDefOf));
        }

        // Token: 0x04004713 RID: 18195
        public static ThingDef Facehugger_Grab;

    }
}