using MaterialDesignThemes.Wpf;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using TwitchKeyboard.Classes;
using TwitchKeyboard.Classes.Rules;
using TwitchKeyboard.Components;

namespace TwitchKeyboard.Windows.Editors
{
    /// <summary>
    /// Логика взаимодействия для WebRuleEditor.xaml
    /// </summary>
    public partial class WebRuleEditor : Window
    {
        readonly WebRule rule = null;

        /// <summary>
        /// Raised when the rule is saved or created.
        /// </summary>
        /// <param name="sender">Current window</param>
        /// <param name="rule">New rule object</param>
        public delegate void OnSaveRuleHandler(object sender, WebRule rule);
        public event OnSaveRuleHandler OnSaveRule;

        public WebRuleEditor(WebRule rule = null)
        {
            InitializeComponent();

            if (rule == null)
            {
                rule = this.rule = new WebRule();
                this.rule.events.Add(new TwitchTrigger());
            }
            else
            {
                // Creating copy of current rule (not the most optimal method, but simple enough)
                this.rule = JsonConvert.DeserializeObject<WebRule>(
                    JsonConvert.SerializeObject(rule)
                );

                titleText.Text = Properties.Resources.t_editHttpRule;
            }

            for (int i = 0; i < this.rule.events.Count; i++)
            {
                createEventUIElement(this.rule.events[i]);
            }

            foreach (var header in rule.headers)
            {
                Chip headerChip = new()
                {
                    IsDeletable = true,
                    Margin = new Thickness(0,0,8,8),
                    Content = $"{header.Key}: {header.Value}",
                    Tag = header.Key
                };

                headerChip.DeleteClick += HeaderChip_DeleteClick;
                headersList.Children.Add(headerChip);
            }

            urlValue.Text = rule.url;
            methodValue.SelectedIndex = (int)rule.method;
            bodyValue.Text = rule.content;
            ruleNameValue.Text = rule.name;
            delayValue.Text = Helper.TimerIntToString(rule.delay);
            cooldownValue.Text = Helper.TimerIntToString(rule.cooldown);
        }

        private void HeaderChip_DeleteClick(object sender, RoutedEventArgs e)
        {
            var chip = (Chip)sender;
            rule.headers.Remove((string)chip.Tag);
            headersList.Children.Remove(chip);
        }

        private void createEventUIElement(TwitchTrigger trigger)
        {
            TriggerEditor editor = new(trigger);
            editor.OnRemove += Editor_OnRemove;
            editor.OnDuplicate += Editor_OnDuplicate;
            editor.Margin = new Thickness(0, 0, 0, 12);
            eventsContainer.Children.Add(editor);
        }

        private void Editor_OnDuplicate(object sender, TwitchTrigger triggerCopy)
        {
            createEventUIElement(triggerCopy);
            rule.events.Add(triggerCopy);
        }

        private void Editor_OnRemove(object sender, TwitchTrigger removedTrigger)
        {
            eventsContainer.Children.Remove((UIElement)sender);
            rule.events.Remove(removedTrigger);
        }

        private void addEventButton_Click(object sender, RoutedEventArgs e)
        {
            TwitchTrigger trigger = new();
            rule.events.Add(trigger);
            createEventUIElement(trigger);
        }

        private void saveRuleButton_Click(object sender, RoutedEventArgs e)
        {
            rule.url = urlValue.Text;
            rule.method = (Enums.HttpMethod)methodValue.SelectedIndex;
            rule.content = bodyValue.Text;
            rule.name = ruleNameValue.Text;
            rule.delay = Helper.StringToTimerInt(delayValue.Text);
            rule.cooldown = Helper.StringToTimerInt(cooldownValue.Text);

            for (int i = 0; i < eventsContainer.Children.Count; i++)
            {
                if (eventsContainer.Children[i] is TriggerEditor tEditor)
                {
                    tEditor.SaveTrigger();
                }
            }

            OnSaveRule?.Invoke(this, rule);
            this.Close();
        }

        private void cancelRuleButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void addHeaderButton_Click(object sender, RoutedEventArgs e)
        {
            if (rule.headers.ContainsKey(headerNameValue.Text)) return;

            rule.headers.Add(headerNameValue.Text, headerValue.Text);

            Chip headerChip = new()
            {
                IsDeletable = true,
                Margin = new Thickness(0, 0, 8, 8),
                Content = $"{headerNameValue.Text}: {headerValue.Text}",
                Tag = headerNameValue.Text
            };

            headerChip.DeleteClick += HeaderChip_DeleteClick;
            headersList.Children.Add(headerChip);

            headerNameValue.Text = "";
            headerValue.Text = "";
        }

        private void runRuleButton_Click(object sender, RoutedEventArgs e)
        {
            responseValue.Dispatcher.Invoke(() => responseValue.Text = Properties.Resources.t_running);

            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(urlValue.Text);
            request.CachePolicy = new(System.Net.Cache.RequestCacheLevel.BypassCache);
            foreach (var header in rule.headers)
            {
                request.Headers.Set(header.Key, header.Value);
            }

            byte[] byteArray = Encoding.UTF8.GetBytes(bodyValue.Text);
            request.ContentLength = byteArray.Length;
            request.Method = ((Enums.HttpMethod)methodValue.SelectedIndex).ToString();

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
                    catch (Exception e) { responseValue.Dispatcher.Invoke(() => responseValue.Text = $"{Properties.Resources.t_error}: {e}"); }
                }

                try
                {
                    var response = await request.GetResponseAsync();
                    var reader = new StreamReader(response.GetResponseStream());
                    var text = await reader.ReadToEndAsync();
                    responseValue.Dispatcher.Invoke(() => responseValue.Text = text);
                }
                catch (Exception e) { responseValue.Dispatcher.Invoke(() => responseValue.Text = $"{Properties.Resources.t_error}: {e}"); }
            });

            responseValue.Visibility = Visibility.Visible;
        }
    }
}
