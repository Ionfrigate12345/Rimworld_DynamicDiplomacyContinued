using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using Verse.AI.Group;
using Verse;

namespace DynamicDiplomacy.AI.LordJobs
{
    internal class LordJobDefendThenAssaultEnemy : LordJob_DefendPoint
    {
        private IntVec3 _point;
        private float? _wanderRadius;

        private List<Pawn> _enemyPawns;

        public override bool IsCaravanSendable => true;
        public override bool AddFleeToil => false;

        public LordJobDefendThenAssaultEnemy()
        {
        }

        public LordJobDefendThenAssaultEnemy(IntVec3 point, List<Pawn> enemyPawns, float? wanderRadius = null)
        {
            _point = point;
            _wanderRadius = wanderRadius;
            _enemyPawns = enemyPawns;
        }

        // 创建状态机
        public override StateGraph CreateGraph()
        {
            //状态1：往指定地点集合。
            var stateGraph = new StateGraph();

            var lordToilDefendPoint = new LordToil_DefendPoint(_point, wanderRadius: _wanderRadius);
            stateGraph.AddToil(lordToilDefendPoint);

            var lordToilAssaultEnemy = new LordToil_AssaultThings(_enemyPawns);
            stateGraph.AddToil(lordToilAssaultEnemy);

            var lordToilLeaveMap = new LordToil_ExitMap();
            stateGraph.AddToil(lordToilLeaveMap);

            //添加流程：X小时后进攻。
            var transition1 = new Transition(lordToilDefendPoint, lordToilAssaultEnemy);
            var triggerXHoursAfter1 = new Trigger_TicksPassed(GenDate.TicksPerHour * 2);
            transition1.AddTrigger(triggerXHoursAfter1);
            stateGraph.AddTransition(transition1);

            return stateGraph;
        }

        /// <summary>
        /// 序列化
        /// </summary>
        public override void ExposeData()
        {
            Scribe_Values.Look(ref _point, "DMP_PermanentAlliance_LordJobDefendMarriageLeave_point");
            Scribe_Values.Look(ref _wanderRadius, "DMP_PermanentAlliance_LordJobDefendMarriageLeave_wanderRadius");
        }
    }
}
