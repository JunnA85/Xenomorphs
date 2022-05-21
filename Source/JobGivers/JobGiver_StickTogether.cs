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
   public abstract class JobGiver_StickTogether : ThinkNode_JobGiver
    {
        protected abstract Pawn GetFollowee(Pawn pawn);
        protected abstract float GetRadius(Pawn pawn);
        protected virtual int FollowJobExpireInterval
        {
            get
            {
                return 40;
            }
        }
        protected override Job TryGiveJob(Pawn pawn)
        {
            List<Pawn> potentialFriendlies = Find.CurrentMap.mapPawns.AllPawnsSpawned.Where(x => x.RaceProps.FleshType.defName == pawn.RaceProps.FleshType.defName).ToList();
            float radius = this.GetRadius(pawn);
            for (int i = 0; i < potentialFriendlies.Count; i++)
            {
                Thing thing = potentialFriendlies[i];
                int distance = thing.Position.DistanceToSquared(pawn.Position);
                if(distance < 500)
                {
					if (!JobDriver_FollowClose.FarEnoughAndPossibleToStartJob(pawn, potentialFriendlies[i], radius))
					{
						return null;
					}
					Job job = JobMaker.MakeJob(JobDefOf.FollowClose, potentialFriendlies[i]);
					job.expiryInterval = this.FollowJobExpireInterval;
					job.checkOverrideOnExpire = true;
					job.followRadius = radius;
                    Log.Message("Following: " + potentialFriendlies[i].RaceProps.FleshType);
					return job;
				}
            }
            return null;
        }
    }
}
