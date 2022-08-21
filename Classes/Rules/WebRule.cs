using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TwitchKeyboard.Enums;

namespace TwitchKeyboard.Classes.Rules
{
  public class WebRule : BaseRule
  {
    public new string name = "";
    public string url = "https://";
    public string content = "";
    public HttpMethod method = HttpMethod.GET;
    public Dictionary<string, string> headers = new() {
      { "Content-Type", "application/json;charset=UTF-8" },
      { "Accept", "application/json" },
    };

    public override string GetName()
    {
      return name;
    }
  }
}
