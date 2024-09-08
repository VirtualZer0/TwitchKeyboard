using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using TwitchKeyboard.Classes.Rules;
using TwitchKeyboard.Enums;

namespace TwitchKeyboard.Classes.Controllers
{
  /// <summary>
  /// Controls the operation of a specific sfx rule, based on its model
  /// </summary>
  public class CmdRuleController : BaseRuleController
  {
    public override ManagerType cType { get => ManagerType.CMD; }
  }
}
