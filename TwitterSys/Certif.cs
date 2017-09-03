using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using CoreTweet;
using CoreTweet.Core;
using System.Windows.Forms;
using Codeplex.Data;

namespace AutoLike.TwitterSys
{
    class Certif
    {
        public OAuth.OAuthSession Session { get; private set; }
        public Tokens Token { get; set; }
        public bool TokenFlag { get; set; } = false;

        public Certif(string ck, string cs)
        {
            this.Session = OAuth.Authorize(ck, cs);
            Process.Start(this.Session.AuthorizeUri.AbsoluteUri);
        }


        public ListedResponse<Status> GetTweet(int cnt, string userid)
        {
            return this.Token.Statuses.UserTimeline(count=>cnt, screen_name=>userid);
        }


        public void TweetFavo(int cnt, string userid)
        {
            foreach (var tweet in this.GetTweet(cnt, userid))
            {
                if (tweet?.IsFavorited != null && (bool)!tweet.IsFavorited && !tweet.Text.Contains("@"))
                {
                    this.Token.Favorites.Create(id => tweet.Id);
                }
            }
        }
    }
}
