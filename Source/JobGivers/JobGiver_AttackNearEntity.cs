using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse.AI;
using Verse;
using RimWorld;
using Verse.AI.Group;

namespace Aliensvspredator.JobGivers
{
    class JobGiver_AttackNearEntity : ThinkNode_JobGiver
    {
        protected override Job TryGiveJob(Pawn pawn)
        {
            List<IAttackTarget> potentialTargetsFor = pawn.Map.attackTargetsCache.GetPotentialTargetsFor(pawn);
            Log.Message("Test");
            
            for (int i = 0; i < potentialTargetsFor.Count; i++)
            {
                var distance = Math.Sqrt((Math.Pow(pawn.Position.x - pawn.Position.y, 2) + Math.Pow(potentialTargetsFor[i].Thing.Position.x - potentialTargetsFor[i].Thing.Position.y, 2)));

                bool fenceBlocked = pawn.def.race.FenceBlocked;
                if (distance < 5)
                {
                    if (potentialTargetsFor[i] != null && pawn.CanReach(potentialTargetsFor[i].Thing, PathEndMode.Touch, Danger.Deadly, false, fenceBlocked, TraverseMode.ByPawn))
                    {
                        return this.MeleeAttackJob(potentialTargetsFor[i].Thing, fenceBlocked);
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
            job.attackDoorIfTargetLost = true;
            job.canBashFences = canBashFences;
            return job;
        }
    }
}

