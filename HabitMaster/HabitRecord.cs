using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HabitMaster
{
    public class HabitRecord
    {
        public string HabitName { get; set; }
        public string Icon { get; set; }
        public string GoalType { get; set; }
        public GoalOptionData GoalOption { get; set; }
        public string ProgressStatus { get; set; }
        public int Progress { get; set; }
        public SwipeItemView StartItemView { get; set; }
        public SwipeItemView StopItemView { get; set; }

    }
    public class GoalOptionData
    {
        public int UnitNumber { get; set; }
        public string UnitType { get; set; }
    }

}
