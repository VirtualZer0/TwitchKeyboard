using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Media;

namespace TwitchKeyboard.Classes
{
    public class MediaPlayerExt : MediaPlayer
    {

        public new void Open(Uri source)
        {
            this.IsMuted = true;
            _ = new Timer((obj) => DelayedOpen(source), null, 20, Timeout.Infinite);
            
        }

        private void DelayedOpen(Uri source)
        {
            Dispatcher.Invoke(() =>
            {
                base.Open(source);
                this.Pause();
                this.Stop();
                _ = new Timer(PostDelayedOpen, null, 20, Timeout.Infinite);
            });
        }

        private void PostDelayedOpen(object state)
        {
            Dispatcher.Invoke(() =>
            {
                this.IsMuted = false;
            });
        }
    }
}
