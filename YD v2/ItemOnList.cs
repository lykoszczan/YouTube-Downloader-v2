using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YD_v2
{
    class ItemOnList
    {
        public string Name { get; set; }
        public string SongsCount { get; set; }
        public string LastUpdate { get; set; }
        public string Id { get; set; }
        public string Path { get; set; }
        public string ChannelName { get; set; }
        public System.Windows.Media.Brush StatusColor
        {
            get
            {
                if (LastUpdate == "Updating ...")
                    return System.Windows.Media.Brushes.Orange;
                else
                {
                    DateTime last = Convert.ToDateTime(LastUpdate);

                    if (DateTime.Compare(DateTime.Now.Date, last.Date) == 0)
                        return System.Windows.Media.Brushes.Green;
                    else
                        return System.Windows.Media.Brushes.Red;
                }
            }

            set
            {
                StatusColor = value;
            }
        }

        public ItemOnList()
        {

        }
    }
}
