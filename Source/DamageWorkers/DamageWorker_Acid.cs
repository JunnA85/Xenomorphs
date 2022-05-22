using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;

namespace Aliensvspredator.DamageWorkers
{
    class DamageWorker_Acid : DamageWorker_AddInjury
    {
		public override DamageWorker.DamageResult Apply(DamageInfo dinfo, Thing victim)
		{
			Pawn pawn = victim as Pawn;
			if (pawn != null && pawn.Faction == Faction.OfPlayer)
			{
				Find.TickManager.slower.SignalForceNormalSpeedShort();
			}
			Map map = victim.Map;
			DamageWorker.DamageResult damageResult = base.Apply(dinfo, victim);
			if (!damageResult.deflected && !dinfo.InstantPermanentInjury && Rand.Chance(FireUtility.ChanceToAttachFireFromEvent(victim)))
			{
				victim.TryAttachFire(Rand.Range(0.15f, 0.25f));
			}
			if (victim.Destroyed && map != null && pawn == null)
			{
				foreach (IntVec3 c in victim.OccupiedRect())
				{
					FilthMaker.TryMakeFilth(c, map, ThingDefOf.Filth_Ash, 1, FilthSourceFlags.None);
				}
				Plant plant = victim as Plant;
				if (plant != null && plant.LifeStage != PlantLifeStage.Sowing && victim.def.plant.burnedThingDef != null)
				{
					((DeadPlant)GenSpawn.Spawn(victim.def.plant.burnedThingDef, victim.Position, map, WipeMode.Vanish)).Growth = plant.Growth;
				}
			}
			return damageResult;
		}
	}
}
