using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;

namespace Aliensvspredator.DeathActionWorkers
{
	public class DeathActionWorker_AcidSpill : DeathActionWorker
	{
		public override RulePackDef DeathRules
		{
			get
			{
				return RulePackDefOf.Transition_DiedExplosive;
			}
		}

		public override bool DangerousInMelee
		{
			get
			{
				return true;
			}
		}

		public override void PawnDied(Corpse corpse)
		{
			Log.Message("Acid spill");
			GenExplosion.DoExplosion(corpse.Position, corpse.Map, 0f, DamageDefOf.Acid, corpse.InnerPawn, 0, 0, null, null, null, null, null, 0f, 1, false, null, 0f, 1, 0f, false, null, null);
		}

	}
	[DefOf]
	public static class DamageDefOf
	{
		// Token: 0x060080E0 RID: 32992 RVA: 0x002E21C4 File Offset: 0x002E03C4
		static DamageDefOf()
		{
			DefOfHelper.EnsureInitializedInCtor(typeof(DamageDefOf));
		}

		public static DamageDef Acid;

		public static DamageDef Flame;
			
	}
}
