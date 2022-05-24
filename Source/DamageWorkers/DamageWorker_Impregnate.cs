using Aliensvspredator.Hediffs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;

namespace Aliensvspredator.DamageWorkers
{
    class DamageWorker_Impregnate : DamageWorker_Bite
    {
		protected override BodyPartRecord ChooseHitPart(DamageInfo dinfo, Pawn pawn)
		{
			DamageWorker_Utility.Impregnate(dinfo, pawn);
			return base.ChooseHitPart(dinfo, pawn);
		}
	}
	internal static class DamageWorker_Utility
	{
		// Token: 0x06000038 RID: 56 RVA: 0x000034B4 File Offset: 0x000016B4
		public static void Impregnate(DamageInfo dinfo, Pawn pawn)
		{
			Log.Message("Impregnate attempt");
			var rand = new Random();
			int randChance = rand.Next(1, 2);
			if(randChance % 2 == 0)
            {
				Log.Message(pawn.Name + " impregnated");
				pawn.health.AddHediff(XenoHediffDefOf.Embryo, null, null, null);
            }
		}
	}
}
