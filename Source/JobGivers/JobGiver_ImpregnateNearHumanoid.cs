using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse.AI;
using Verse;
using RimWorld;
using Verse.AI.Group;
using Aliensvspredator.Hediffs;

namespace Aliensvspredator.JobGivers
{
    class JobGiver_ImpregnateNearHumanoid : ThinkNode_JobGiver
    {
        protected override Job TryGiveJob(Pawn pawn)
        {
            List<Pawn> potentialTargetsFor = Find.CurrentMap.mapPawns.AllPawnsSpawned.Where(x => x.RaceProps.FleshType.defName != pawn.RaceProps.FleshType.defName && x.IsColonist).ToList();
            for (int i = 0; i < potentialTargetsFor.Count; i++)
            {

                bool fenceBlocked = pawn.def.race.FenceBlocked;

                Thing thing = potentialTargetsFor[i];
                int distance = thing.Position.DistanceToSquared(pawn.Position);
                if (distance < 45)
                {
                    if (!potentialTargetsFor[i].Downed && potentialTargetsFor[i].RaceProps.FleshType.defName != pawn.RaceProps.FleshType.defName && potentialTargetsFor[i] != null && potentialTargetsFor[i] != pawn && pawn.CanReach(potentialTargetsFor[i], PathEndMode.Touch, Danger.Deadly, false, fenceBlocked, TraverseMode.ByPawn))
                    {
                        return this.MeleeAttackJob(potentialTargetsFor[i], fenceBlocked);
                    }
                }

            }

            return null;
        }
        private Job MeleeAttackJob(Thing target, bool canBashFences)
        {
            Job job = JobMaker.MakeJob(JobDefOf.AttackMelee, target);
            job.maxNumMeleeAttacks = 1;
            job.expiryInterval = Rand.Range(420, 900);
            job.attackDoorIfTargetLost = false;
            job.canBashFences = canBashFences;
            return job;
        }
    }
}

