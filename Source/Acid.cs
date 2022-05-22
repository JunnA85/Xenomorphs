using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace Aliensvspredator
{
    public class Acid : AttachableThing, ISizeReporter
    {
        // Token: 0x170011F4 RID: 4596
        // (get) Token: 0x06006867 RID: 26727 RVA: 0x0023891B File Offset: 0x00236B1B
        public int TicksSinceSpawn
        {
            get
            {
                return this.ticksSinceSpawn;
            }
        }

        // Token: 0x170011F5 RID: 4597
        // (get) Token: 0x06006868 RID: 26728 RVA: 0x00238923 File Offset: 0x00236B23
        public override string Label
        {
            get
            {
                if (this.parent != null)
                {
                    return "FireOn".Translate(this.parent.LabelCap, this.parent);
                }
                return this.def.label;
            }
        }

        // Token: 0x170011F6 RID: 4598
        // (get) Token: 0x06006869 RID: 26729 RVA: 0x00238964 File Offset: 0x00236B64
        public override string InspectStringAddon
        {
            get
            {
                return "Burning".Translate() + " (" + "FireSizeLower".Translate((this.fireSize * 100f).ToString("F0")) + ")";
            }
        }

        // Token: 0x170011F7 RID: 4599
        // (get) Token: 0x0600686A RID: 26730 RVA: 0x002389C4 File Offset: 0x00236BC4
        private float SpreadInterval
        {
            get
            {
                float num = 150f - (this.fireSize - 1f) * 40f;
                if (num < 75f)
                {
                    num = 75f;
                }
                return num;
            }
        }

        // Token: 0x0600686B RID: 26731 RVA: 0x002389F9 File Offset: 0x00236BF9
        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look<int>(ref this.ticksSinceSpawn, "ticksSinceSpawn", 0, false);
            Scribe_Values.Look<float>(ref this.fireSize, "fireSize", 0f, false);
        }

        // Token: 0x0600686C RID: 26732 RVA: 0x00238A29 File Offset: 0x00236C29
        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            Log.Message("SpawnSetup");
            base.SpawnSetup(map, respawningAfterLoad);
            this.RecalcPathsOnAndAroundMe(map);
            LessonAutoActivator.TeachOpportunity(ConceptDefOf.HomeArea, this, OpportunityType.Important);
            this.ticksSinceSpread = (int)(this.SpreadInterval * Rand.Value);
        }

        // Token: 0x0600686D RID: 26733 RVA: 0x00238A59 File Offset: 0x00236C59
        public float CurrentSize()
        {
            return this.fireSize;
        }

        // Token: 0x0600686E RID: 26734 RVA: 0x00238A64 File Offset: 0x00236C64
        public override void DeSpawn(DestroyMode mode = DestroyMode.Vanish)
        {
            if (this.sustainer != null)
            {
                if (this.sustainer.externalParams.sizeAggregator == null)
                {
                    this.sustainer.externalParams.sizeAggregator = new SoundSizeAggregator();
                }
                this.sustainer.externalParams.sizeAggregator.RemoveReporter(this);
            }
            Map map = base.Map;
            base.DeSpawn(mode);
            this.RecalcPathsOnAndAroundMe(map);
        }

        // Token: 0x0600686F RID: 26735 RVA: 0x00238ACC File Offset: 0x00236CCC
        private void RecalcPathsOnAndAroundMe(Map map)
        {
            IntVec3[] adjacentCellsAndInside = GenAdj.AdjacentCellsAndInside;
            for (int i = 0; i < adjacentCellsAndInside.Length; i++)
            {
                IntVec3 c = base.Position + adjacentCellsAndInside[i];
                if (c.InBounds(map))
                {
                    map.pathing.RecalculatePerceivedPathCostAt(c);
                }
            }
        }

        // Token: 0x06006870 RID: 26736 RVA: 0x00238B18 File Offset: 0x00236D18
        public override void AttachTo(Thing parent)
        {
            base.AttachTo(parent);
            Pawn pawn = parent as Pawn;
            if (pawn != null)
            {
                TaleRecorder.RecordTale(TaleDefOf.WasOnFire, new object[]
                {
                    pawn
                });
            }
        }

        // Token: 0x06006871 RID: 26737 RVA: 0x00238B4C File Offset: 0x00236D4C
        public override void Tick()
        {
            this.ticksSinceSpawn++;
            if (Acid.lastFireCountUpdateTick != Find.TickManager.TicksGame)
            {
                Acid.fireCount = base.Map.listerThings.ThingsOfDef(this.def).Count;
                Acid.lastFireCountUpdateTick = Find.TickManager.TicksGame;
            }
            if (this.sustainer != null)
            {
                this.sustainer.Maintain();
            }
            else if (!base.Position.Fogged(base.Map))
            {
                SoundInfo info = SoundInfo.InMap(new TargetInfo(base.Position, base.Map, false), MaintenanceType.PerTick);
                this.sustainer = SustainerAggregatorUtility.AggregateOrSpawnSustainerFor(this, SoundDefOf.FireBurning, info);
            }
            this.ticksUntilSmoke--;
            if (this.ticksUntilSmoke <= 0)
            {
                this.SpawnSmokeParticles();
            }
            if (Acid.fireCount < 15 && this.fireSize > 0.7f && Rand.Value < this.fireSize * 0.01f)
            {
                FleckMaker.ThrowMicroSparks(this.DrawPos, base.Map);
            }
            if (this.fireSize > 1f)
            {
                this.ticksSinceSpread++;
                if ((float)this.ticksSinceSpread >= this.SpreadInterval)
                {
                    this.TrySpread();
                    this.ticksSinceSpread = 0;
                }
            }
            if (this.IsHashIntervalTick(150))
            {
                this.DoComplexCalcs();
            }
            if (this.ticksSinceSpawn >= 7500)
            {
                this.TryBurnFloor();
            }
        }

        // Token: 0x06006872 RID: 26738 RVA: 0x00238CB0 File Offset: 0x00236EB0
        private void SpawnSmokeParticles()
        {
            if (Acid.fireCount < 15)
            {
                FleckMaker.ThrowSmoke(this.DrawPos, base.Map, this.fireSize);
            }
            if (this.fireSize > 0.5f && this.parent == null)
            {
                FleckMaker.ThrowFireGlow(base.Position.ToVector3Shifted(), base.Map, this.fireSize);
            }
            float num = this.fireSize / 2f;
            if (num > 1f)
            {
                num = 1f;
            }
            num = 1f - num;
            this.ticksUntilSmoke = Acid.SmokeIntervalRange.Lerped(num) + (int)(10f * Rand.Value);
        }

        // Token: 0x06006873 RID: 26739 RVA: 0x00238D58 File Offset: 0x00236F58
        private void DoComplexCalcs()
        {
            bool flag = false;
            Acid.flammableList.Clear();
            this.flammabilityMax = 0f;
            if (!base.Position.GetTerrain(base.Map).extinguishesFire)
            {
                if (this.parent == null)
                {
                    if (base.Position.TerrainFlammableNow(base.Map))
                    {
                        this.flammabilityMax = base.Position.GetTerrain(base.Map).GetStatValueAbstract(StatDefOf.Flammability, null);
                    }
                    List<Thing> list = base.Map.thingGrid.ThingsListAt(base.Position);
                    for (int i = 0; i < list.Count; i++)
                    {
                        Thing thing = list[i];
                        if (thing is Building_Door)
                        {
                            flag = true;
                        }
                        float statValue = thing.GetStatValue(StatDefOf.Flammability, true);
                        if (statValue >= 0.01f)
                        {
                            Acid.flammableList.Add(list[i]);
                            if (statValue > this.flammabilityMax)
                            {
                                this.flammabilityMax = statValue;
                            }
                            if (this.parent == null && this.fireSize > 0.4f && list[i].def.category == ThingCategory.Pawn && Rand.Chance(FireUtility.ChanceToAttachFireCumulative(list[i], 150f)))
                            {
                                list[i].TryAttachFire(this.fireSize * 0.2f);
                            }
                        }
                    }
                }
                else
                {
                    Acid.flammableList.Add(this.parent);
                    this.flammabilityMax = this.parent.GetStatValue(StatDefOf.Flammability, true);
                }
            }
            if (this.flammabilityMax < 0.01f)
            {
                this.Destroy(DestroyMode.Vanish);
                return;
            }
            Thing thing2;
            if (this.parent != null)
            {
                thing2 = this.parent;
            }
            else if (Acid.flammableList.Count > 0)
            {
                thing2 = Acid.flammableList.RandomElement<Thing>();
            }
            else
            {
                thing2 = null;
            }
            if (thing2 != null && (this.fireSize >= 0.4f || thing2 == this.parent || thing2.def.category != ThingCategory.Pawn))
            {
                this.DoFireDamage(thing2);
            }
            if (base.Spawned)
            {
                float num = this.fireSize * 160f;
                if (flag)
                {
                    num *= 0.15f;
                }
                GenTemperature.PushHeat(base.Position, base.Map, num);
                if (Rand.Value < 0.4f)
                {
                    float radius = this.fireSize * 3f;
                    SnowUtility.AddSnowRadial(base.Position, base.Map, radius, -(this.fireSize * 0.1f));
                }
                this.fireSize += 0.00055f * this.flammabilityMax * 150f;
                if (this.fireSize > 1.75f)
                {
                    this.fireSize = 1.75f;
                }
                if (base.Map.weatherManager.RainRate > 0.01f && this.VulnerableToRain() && Rand.Value < 6f)
                {
                    base.TakeDamage(new DamageInfo(DamageDefOf.Extinguish, 10f, 0f, -1f, null, null, null, DamageInfo.SourceCategory.ThingOrUnknown, null, true, true));
                }
            }
        }

        // Token: 0x06006874 RID: 26740 RVA: 0x00239041 File Offset: 0x00237241
        private void TryBurnFloor()
        {
            if (this.parent != null || !base.Spawned)
            {
                return;
            }
            if (base.Position.TerrainFlammableNow(base.Map))
            {
                base.Map.terrainGrid.Notify_TerrainBurned(base.Position);
            }
        }

        // Token: 0x06006875 RID: 26741 RVA: 0x00239080 File Offset: 0x00237280
        private bool VulnerableToRain()
        {
            if (!base.Spawned)
            {
                return false;
            }
            RoofDef roofDef = base.Map.roofGrid.RoofAt(base.Position);
            if (roofDef == null)
            {
                return true;
            }
            if (roofDef.isThickRoof)
            {
                return false;
            }
            Thing edifice = base.Position.GetEdifice(base.Map);
            return edifice != null && edifice.def.holdsRoof;
        }

        // Token: 0x06006876 RID: 26742 RVA: 0x002390E0 File Offset: 0x002372E0
        private void DoFireDamage(Thing targ)
        {
            int num = GenMath.RoundRandom(Mathf.Clamp(0.0125f + 0.0036f * this.fireSize, 0.0125f, 0.05f) * 150f);
            if (num < 1)
            {
                num = 1;
            }
            Pawn pawn = targ as Pawn;
            if (pawn != null)
            {
                BattleLogEntry_DamageTaken battleLogEntry_DamageTaken = new BattleLogEntry_DamageTaken(pawn, RulePackDefOf.DamageEvent_Fire, null);
                Find.BattleLog.Add(battleLogEntry_DamageTaken);
                DamageInfo dinfo = new DamageInfo(DamageDefOf.Flame, (float)num, 0f, -1f, this, null, null, DamageInfo.SourceCategory.ThingOrUnknown, null, true, true);
                dinfo.SetBodyRegion(BodyPartHeight.Undefined, BodyPartDepth.Outside);
                targ.TakeDamage(dinfo).AssociateWithLog(battleLogEntry_DamageTaken);
                Apparel apparel;
                if (pawn.apparel != null && pawn.apparel.WornApparel.TryRandomElement(out apparel))
                {
                    apparel.TakeDamage(new DamageInfo(DamageDefOf.Flame, (float)num, 0f, -1f, this, null, null, DamageInfo.SourceCategory.ThingOrUnknown, null, true, true));
                    return;
                }
            }
            else
            {
                targ.TakeDamage(new DamageInfo(DamageDefOf.Flame, (float)num, 0f, -1f, this, null, null, DamageInfo.SourceCategory.ThingOrUnknown, null, true, true));
            }
        }

        // Token: 0x06006877 RID: 26743 RVA: 0x002391E0 File Offset: 0x002373E0
        protected void TrySpread()
        {
            IntVec3 intVec = base.Position;
            bool flag;
            if (Rand.Chance(0.8f))
            {
                intVec = base.Position + GenRadial.ManualRadialPattern[Rand.RangeInclusive(1, 8)];
                flag = true;
            }
            else
            {
                intVec = base.Position + GenRadial.ManualRadialPattern[Rand.RangeInclusive(10, 20)];
                flag = false;
            }
            if (!intVec.InBounds(base.Map))
            {
                return;
            }
            if (Rand.Chance(FireUtility.ChanceToStartFireIn(intVec, base.Map)))
            {
                if (!flag)
                {
                    CellRect startRect = CellRect.SingleCell(base.Position);
                    CellRect endRect = CellRect.SingleCell(intVec);
                    if (!GenSight.LineOfSight(base.Position, intVec, base.Map, startRect, endRect, null))
                    {
                        return;
                    }
                    ((Spark)GenSpawn.Spawn(ThingDefOf.Spark, base.Position, base.Map, WipeMode.Vanish)).Launch(this, intVec, intVec, ProjectileHitFlags.All, false, null);
                    return;
                }
                else
                {
                    FireUtility.TryStartFireIn(intVec, base.Map, 0.1f);
                }
            }
        }

        // Token: 0x04003AA8 RID: 15016
        private int ticksSinceSpawn;

        // Token: 0x04003AA9 RID: 15017
        public float fireSize = 0.1f;

        // Token: 0x04003AAA RID: 15018
        private int ticksSinceSpread;

        // Token: 0x04003AAB RID: 15019
        private float flammabilityMax = 0.5f;

        // Token: 0x04003AAC RID: 15020
        private int ticksUntilSmoke;

        // Token: 0x04003AAD RID: 15021
        private Sustainer sustainer;

        // Token: 0x04003AAE RID: 15022
        private static List<Thing> flammableList = new List<Thing>();

        // Token: 0x04003AAF RID: 15023
        private static int fireCount;

        // Token: 0x04003AB0 RID: 15024
        private static int lastFireCountUpdateTick;

        // Token: 0x04003AB1 RID: 15025
        public const float MinFireSize = 0.1f;

        // Token: 0x04003AB2 RID: 15026
        private const float MinSizeForSpark = 1f;

        // Token: 0x04003AB3 RID: 15027
        private const float TicksBetweenSparksBase = 150f;

        // Token: 0x04003AB4 RID: 15028
        private const float TicksBetweenSparksReductionPerFireSize = 40f;

        // Token: 0x04003AB5 RID: 15029
        private const float MinTicksBetweenSparks = 75f;

        // Token: 0x04003AB6 RID: 15030
        private const float MinFireSizeToEmitSpark = 1f;

        // Token: 0x04003AB7 RID: 15031
        public const float MaxFireSize = 1.75f;

        // Token: 0x04003AB8 RID: 15032
        private const int TicksToBurnFloor = 7500;

        // Token: 0x04003AB9 RID: 15033
        private const int ComplexCalcsInterval = 150;

        // Token: 0x04003ABA RID: 15034
        private const float CellIgniteChancePerTickPerSize = 0.01f;

        // Token: 0x04003ABB RID: 15035
        private const float MinSizeForIgniteMovables = 0.4f;

        // Token: 0x04003ABC RID: 15036
        private const float FireBaseGrowthPerTick = 0.00055f;

        // Token: 0x04003ABD RID: 15037
        private static readonly IntRange SmokeIntervalRange = new IntRange(130, 200);

        // Token: 0x04003ABE RID: 15038
        private const int SmokeIntervalRandomAddon = 10;

        // Token: 0x04003ABF RID: 15039
        private const float BaseSkyExtinguishChance = 0.04f;

        // Token: 0x04003AC0 RID: 15040
        private const int BaseSkyExtinguishDamage = 10;

        // Token: 0x04003AC1 RID: 15041
        private const float HeatPerFireSizePerInterval = 160f;

        // Token: 0x04003AC2 RID: 15042
        private const float HeatFactorWhenDoorPresent = 0.15f;

        // Token: 0x04003AC3 RID: 15043
        private const float SnowClearRadiusPerFireSize = 3f;

        // Token: 0x04003AC4 RID: 15044
        private const float SnowClearDepthFactor = 0.1f;

        // Token: 0x04003AC5 RID: 15045
        private const int FireCountParticlesOff = 15;
    }
}



