using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TwitterFollowersDiff.TwitterClasses
{
    enum UserInfoType
    {
        ID,ScreenName
    }
    /// <summary>
    /// A list of JSON Twitter API urls that the application could make use of,
    /// with placeholders for get vars 
    /// </summary>
    /// <remarks>Could create another placeholder for xml/json choice. and ability to add more getvars.
    /// The ability to create post variables should also be done.
    /// </remarks> 
    public static class JSONTwitterURLs
    {
        public static string followers = "https://api.twitter.com/1/followers/ids.json?cursor={1}&screen_name={0}";
        public static string userInfo = "https://api.twitter.com/1/users/show.json?screen_name={0}";
        public static string lookupID = "https://api.twitter.com/1/users/lookup.json?&include_entities=1&user_id={0}";
        public static string lookupScreenName = "https://api.twitter.com/1/users/lookup.json?&include_entities=1&screen_name={0}";
    }
}
