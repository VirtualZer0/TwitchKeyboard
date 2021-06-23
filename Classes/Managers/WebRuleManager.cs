using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.Windows.Input;
using System.Windows.Media;
using TwitchKeyboard.Classes.Controllers;
using TwitchKeyboard.Classes.RuleControllers;
using TwitchKeyboard.Classes.Rules;
using TwitchKeyboard.Enums;
using WindowsInput;
using WindowsInput.Native;

namespace TwitchKeyboard.Classes.Managers
{
    public class WebRuleManager : BaseRuleManager
    {

        public void CreateControllers (List<WebRule> rules)
        {
            CreateControllers<WebRuleController>(rules.ToArray());
        }

        public override void UpdateRule(BaseRuleController baseRule, int elapsedTime)
        {
            base.UpdateRule(baseRule, elapsedTime);
            if (baseRule.state != RuleState.Active) return;
            baseRule.state = RuleState.Inactive;

            var ruleController = (WebRuleController)baseRule;
            var rule = (WebRule)ruleController.model;

            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(rule.url);
            request.CachePolicy = new(System.Net.Cache.RequestCacheLevel.BypassCache);

            foreach (var header in rule.headers)
            {
                request.Headers.Set(header.Key, header.Value);
            }

            byte[] byteArray = Encoding.UTF8.GetBytes(rule.content);
            request.ContentLength = byteArray.Length;
            request.Method = rule.method.ToString();

            Task.Run(async () =>
            {
                if (request.Method != "GET" && request.Method != "HEAD" && request.Method != "OPTIONS")
                {
                    using Stream dataStream = request.GetRequestStream();
                    try
                    {
                        await dataStream.WriteAsync(byteArray.AsMemory(0, byteArray.Length));
                        await dataStream.FlushAsync();
                        dataStream.Close();
                    }
                    catch (Exception e) { }
                }

                try
                {
                    var response = await request.GetResponseAsync();
                }
                catch (Exception e) { }
            });
        }
    }
}
