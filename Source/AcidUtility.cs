using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;

namespace Aliensvspredator
{
	public static class AcidUtility
	{
		// Token: 0x0600687A RID: 26746 RVA: 0x00239314 File Offset: 0x00237514
		public static bool CanEverAttachFire(this Thing t)
		{
			return !t.Destroyed && t.FlammableNow && t.def.category == ThingCategory.Pawn && t.TryGetComp<CompAttachBase>() != null;
		}

		// Token: 0x0600687B RID: 26747 RVA: 0x00239348 File Offset: 0x00237548
		public static float ChanceToStartFireIn(IntVec3 c, Map map)
		{
			List<Thing> thingList = c.GetThingList(map);
			float num = c.TerrainFlammableNow(map) ? c.GetTerrain(map).GetStatValueAbstract(StatDefOf.Flammability, null) : 0f;
			for (int i = 0; i < thingList.Count; i++)
			{
				Thing thing = thingList[i];
				if (thing is Fire)
				{
					return 0f;
				}
				if (thing.def.category != ThingCategory.Pawn && thingList[i].FlammableNow)
				{
					num = Mathf.Max(num, thing.GetStatValue(StatDefOf.Flammability, true));
				}
			}
			if (num > 0f)
			{
				Building edifice = c.GetEdifice(map);
				if (edifice != null && edifice.def.passability == Traversability.Impassable && edifice.OccupiedRect().ContractedBy(1).Contains(c))
				{
					return 0f;
				}
				List<Thing> thingList2 = c.GetThingList(map);
				for (int j = 0; j < thingList2.Count; j++)
				{
					if (thingList2[j].def.category == ThingCategory.Filth && !thingList2[j].def.filth.allowsFire)
					{
						return 0f;
					}
				}
			}
			return num;
		}

		// Token: 0x0600687C RID: 26748 RVA: 0x00239476 File Offset: 0x00237676
		public static bool TryStartFireIn(IntVec3 c, Map map, float fireSize)
		{
			if (FireUtility.ChanceToStartFireIn(c, map) <= 0f)
			{
				return false;
			}
			Fire fire = (Fire)ThingMaker.MakeThing(ThingDefOf.Fire, null);
			fire.fireSize = fireSize;
			GenSpawn.Spawn(fire, c, map, Rot4.North, WipeMode.Vanish, false);
			return true;
		}

		// Token: 0x0600687D RID: 26749 RVA: 0x002394AF File Offset: 0x002376AF
		public static float ChanceToAttachFireFromEvent(Thing t)
		{
			return FireUtility.ChanceToAttachFireCumulative(t, 60f);
		}

		// Token: 0x0600687E RID: 26750 RVA: 0x002394BC File Offset: 0x002376BC
		public static float ChanceToAttachFireCumulative(Thing t, float freqInTicks)
		{
			if (!t.CanEverAttachFire())
			{
				return 0f;
			}
			if (t.HasAttachment(ThingDefOf.Fire))
			{
				return 0f;
			}
			float num = AcidUtility.ChanceToCatchFirePerSecondForPawnFromFlammability.Evaluate(t.GetStatValue(StatDefOf.Flammability, true));
			return 1f - Mathf.Pow(1f - num, freqInTicks / 60f);
		}

		// Token: 0x0600687F RID: 26751 RVA: 0x0023951C File Offset: 0x0023771C
		public static void TryAttachFire(this Thing t, float fireSize)
		{
			if (!t.CanEverAttachFire())
			{
				return;
			}
			if (t.HasAttachment(ThingDefOf.Fire))
			{
				return;
			}
			Fire fire = (Fire)ThingMaker.MakeThing(ThingDefOf.Fire, null);
			fire.fireSize = fireSize;
			fire.AttachTo(t);
			GenSpawn.Spawn(fire, t.Position, t.Map, Rot4.North, WipeMode.Vanish, false);
			Pawn pawn = t as Pawn;
			if (pawn != null)
			{
				pawn.jobs.StopAll(false, true);
				pawn.records.Increment(RecordDefOf.TimesOnFire);
			}
		}

		// Token: 0x06006880 RID: 26752 RVA: 0x0023959E File Offset: 0x0023779E
		public static bool IsBurning(this TargetInfo t)
		{
			if (t.HasThing)
			{
				return t.Thing.IsBurning();
			}
			return t.Cell.ContainsStaticFire(t.Map);
		}

		// Token: 0x06006881 RID: 26753 RVA: 0x002395CC File Offset: 0x002377CC
		public static bool IsBurning(this Thing t)
		{
			if (t.Destroyed || !t.Spawned)
			{
				return false;
			}
			if (!(t.def.size == IntVec2.One))
			{
				using (CellRect.Enumerator enumerator = t.OccupiedRect().GetEnumerator())
				{
					while (enumerator.MoveNext())
					{
						if (enumerator.Current.ContainsStaticFire(t.Map))
						{
							return true;
						}
					}
				}
				return false;
			}
			if (t is Pawn)
			{
				return t.HasAttachment(ThingDefOf.Fire);
			}
			return t.Position.ContainsStaticFire(t.Map);
		}

		// Token: 0x06006882 RID: 26754 RVA: 0x00239680 File Offset: 0x00237880
		public static bool ContainsStaticFire(this IntVec3 c, Map map)
		{
			List<Thing> list = map.thingGrid.ThingsListAt(c);
			for (int i = 0; i < list.Count; i++)
			{
				Fire fire = list[i] as Fire;
				if (fire != null && fire.parent == null)
				{
					return true;
				}
			}
			return false;
		}

		// Token: 0x06006883 RID: 26755 RVA: 0x002396C8 File Offset: 0x002378C8
		public static bool ContainsTrap(this IntVec3 c, Map map)
		{
			Building edifice = c.GetEdifice(map);
			return edifice != null && edifice is Building_Trap;
		}

		// Token: 0x06006884 RID: 26756 RVA: 0x002396EB File Offset: 0x002378EB
		public static bool Flammable(this TerrainDef terrain)
		{
			return terrain.GetStatValueAbstract(StatDefOf.Flammability, null) > 0.01f;
		}

		// Token: 0x06006885 RID: 26757 RVA: 0x00239700 File Offset: 0x00237900
		public static bool TerrainFlammableNow(this IntVec3 c, Map map)
		{
			if (!c.GetTerrain(map).Flammable())
			{
				return false;
			}
			List<Thing> thingList = c.GetThingList(map);
			for (int i = 0; i < thingList.Count; i++)
			{
				if (thingList[i].FireBulwark)
				{
					return false;
				}
			}
			return true;
		}

		// Token: 0x04003AC6 RID: 15046
		private static readonly SimpleCurve ChanceToCatchFirePerSecondForPawnFromFlammability = new SimpleCurve
		{
			{
				new CurvePoint(0f, 0f),
				true
			},
			{
				new CurvePoint(0.1f, 0.07f),
				true
			},
			{
				new CurvePoint(0.3f, 1f),
				true
			},
			{
				new CurvePoint(1f, 1f),
				true
			}
		};
	}
}
