using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TwitchKeyboard.Classes.RuleControllers;
using TwitchKeyboard.Classes.Rules;
using TwitchKeyboard.Enums;

namespace TwitchKeyboard.Classes.Controllers
{
    public class BaseRuleController
    {
        public TwitchTriggerController[] triggers;
        public RuleState state = RuleState.Inactive;
        public int curCooldown = 0;
        public int curDelay = 0;

        public BaseRule model;

        public virtual ManagerType cType { get => ManagerType.MANAGERS_COUNT; } 

        public void Bind()
        {
            triggers = new TwitchTriggerController[model.events.Count];
            for (int i = 0; i < model.events.Count; i++)
            {
                triggers[i] = new TwitchTriggerController(model.events[i]);
            }
            this.curDelay = model.delay;
        }

        public virtual void Init() { }
        public virtual void Destroy() { }
    }
}
