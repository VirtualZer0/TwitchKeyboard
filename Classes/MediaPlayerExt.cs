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
      base.Open(source);
      this.Pause();
      this.Stop();
      this.IsMuted = false;

    }
  }
}
