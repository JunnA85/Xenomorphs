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
    class JobGiver_StickTogether : ThinkNode_JobGiver
    {
        protected override Job TryGiveJob(Pawn pawn)
        {
            List<Pawn> potentialFriendlies = Find.CurrentMap.mapPawns.AllPawnsSpawned.Where(x => x.RaceProps.FleshType.defName == pawn.RaceProps.FleshType.defName).ToList();
            float radius = 5f;
            Log.Message(Convert.ToString(potentialFriendlies.Count));
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
                    Log.Message(pawn.ThingID + "Following: " + potentialFriendlies[i].ThingID);
					job.expiryInterval = 40;
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
