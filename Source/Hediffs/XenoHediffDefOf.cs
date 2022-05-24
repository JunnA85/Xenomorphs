using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;

namespace Aliensvspredator.Hediffs
{
    [DefOf]
    public static class XenoHediffDefOf
    {
        static XenoHediffDefOf()
        {
            DefOfHelper.EnsureInitializedInCtor(typeof(HediffDefOf));
        }
        public static HediffDef Embryo;
        public static HediffDef Facehugger_Grab;
    }
}
