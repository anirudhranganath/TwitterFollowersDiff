using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.IO;
using System.Web.Script.Serialization;
using System.Collections;
using TwitterFollowersDiff.TwitterClasses;


namespace TwitterFollowersDiff
{
    /// <summary>
    /// The primary interface of the application with twitter api. 
    /// </summary>
    class WebInteraction
    {
        /// <summary>
        /// Method to get the IDs of a user's followers. 
        /// Use in conjunction with the handleOfIDs method that uses the lookup api call for their handles
        /// </summary>
        /// <param name="user">User Handle</param>
        /// <returns>A string list of follower ids</returns>
        /// <remarks>
        /// Twitter api returns only 5000 ids at a time, and we need to repeat the process using cursors
        /// This method can handle more than that, limited only by twitter rate limit.
        /// Of course lady gaga's followers would take quite a while, requiring about 4800 api calls for 24,062,354 followers
        /// Be aware that twitter rate limit is 150 per hour for non-authenticated users
        /// </remarks>
        public static List<string> followerIDsOfUser(string user)
        {
            List<string> followers = new List<string>();
            Dictionary<string, dynamic> resp = new Dictionary<string, dynamic>();
            for (Int64 cursor = -1; cursor != 0; )
            {
                string url = string.Format(TwitterClasses.JSONTwitterURLs.followers, "{0}", cursor); //keep the first placeholder {0} intact and replace second with cursor
                resp = DictDeserializeJSON(JSONresponse(url, user));
                ArrayList followerID = (ArrayList)((resp)["ids"]);
                cursor = Int64.Parse(resp["next_cursor_str"]);
                foreach (object k in followerID)
                    followers.Add(k.ToString());
            }
                return followers;
        }
        /// <summary>
        /// Method to deserialize JSON and return a dictionary of key value pairs
        /// Must use ListDictDeserializeJSON(string json) if the JSON beings with an array
        /// </summary>
        /// <param name="json">The JSON as a string</param>
        /// <returns> a (string,dynamic) dictionary </returns>
        private static Dictionary<string, dynamic> DictDeserializeJSON(string json)
        {
            try
            {
                var serializer = new JavaScriptSerializer();
                Dictionary<string, dynamic> deserialDict = serializer.Deserialize<Dictionary<string, dynamic>>(json);
                return deserialDict;
            }
            catch (Exception e)
            {
                throw e;
            }
        }
        /// <summary>
        /// Method to deserialize JSON and return a list of dictionary of key value pairs
        /// Must use DictDeserializeJSON(string json) if the JSON does not being with an array
        /// </summary>
        /// <param name="json">The JSON as a string</param>
        /// <returns> a (string,dynamic) dictionary </returns>
        private static List<Dictionary<string, dynamic>> ListDictDeserializeJSON(String json)
        {
            var serializer = new JavaScriptSerializer();
            try
            {
                List<Dictionary<string, dynamic>> DeserialDictList = serializer.Deserialize<List<Dictionary<string, dynamic>>>(json);
                return DeserialDictList;
            }
            catch (Exception e)
            {
                throw e;
            }
        }
        /// <summary>
        /// Get the JSON response of twitter api call by specifying url and the handle or 
        /// comma-separated handles if the api allows. URL can be chosen from TwitterClasses.JSONTwitterURLs
        /// </summary>
        /// <param name="url">The url. Choose from TwitterClasses.JSONTwitterURLs</param>
        /// <param name="user">the comma separated handle(s) or ID(s) of the user(s)</param>
        /// <returns>The JSON string</returns>
        public static string JSONresponse(string url, string user)
        {
            string requestURL = string.Format(url, user);
            HttpWebRequest webRequest = (HttpWebRequest)WebRequest.Create(requestURL);
            try
            {
                using (HttpWebResponse webResponse = (HttpWebResponse)webRequest.GetResponse())
                {

                    StreamReader responseStream = new StreamReader(webResponse.GetResponseStream());
                    return responseStream.ReadToEnd();
                }
            }
            catch (WebException ex)
            {
                if (ex.Status == WebExceptionStatus.ProtocolError && ex.Response != null)
                {
                    var resp = (HttpWebResponse)ex.Response;
                    if (resp.StatusCode == HttpStatusCode.NotFound)
                    {
                        throw new TwitterFollowersDiff.Exceptions.UserNotFoundException("User " + user + " not found", ex);
                    }
                    else if (resp.StatusCode == HttpStatusCode.Unauthorized)
                    {
                        throw new UnauthorizedAccessException("User " + user + " has a protected account", ex);
                    }
                    else if (resp.StatusCode == HttpStatusCode.BadRequest)
                    {
                        throw new Exception("Rate Limit Hit. You have a maximum of 150 api calls per hour. Try after an hour.", ex);
                    }
                    else
                    {
                        throw new Exception("Unknown level 1 Exception", ex);
                    }
                }
                else
                {
                    if (ex.Status == WebExceptionStatus.NameResolutionFailure)
                        throw new Exception("Internet connection failure/Name Resolution Failure", ex);
                    throw new Exception("Unknown level 2 Exception", ex);
                }
            }
        }
        /// <summary>
        /// Returns the handle(s) of user ID(s) 
        /// </summary>
        /// <param name="ID">the ID or comma separated IDs of the users</param>
        /// <returns>a string list of handles</returns>
        public static List<string> handleOfIDs(List<string> ID)
        {
            if (ID.Count == 0) return new List<string>(); //handle empty input
            List<Dictionary<string, dynamic>> users = getUserInfo(ID,UserInfoType.ID);
            List<string> handles = new List<string>();
            foreach (Dictionary<string, dynamic> t in users)
            {
                handles.Add(t["screen_name"]);
            }
            return handles;
        }
        /// <summary>
        /// Check if a user exists, with the given handle
        /// </summary>
        /// <param name="user">the handle of the user</param>
        /// <returns>Bool: True if found. Else False</returns>
        /// <remarks>
        /// Should be able to handle IDs as well in future Revision
        /// </remarks>
        public static bool checkUserExists(string user)
        {
            try
            {
                JSONresponse(JSONTwitterURLs.userInfo, user);
                return true;
            }
            catch (Exceptions.UserNotFoundException ex)
            {
                return false;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        /// <summary>
        /// Returns the userinfo(s) of user ID(s)or screen name(s), using the twitter lookup api call.
        /// Assumes ids/Screen names are valid. 
        /// include_entities is set as true
        /// </summary>
        /// <param name="ID">String List of IDs or screen names that we need the info of</param>
        /// <param name="uitype">UserInfoType Can be ID or ScreenName</param>
        /// <returns>A Dictionary List of user info</returns>
        /// <remarks>
        /// Api allows a max of 100 IDs in 1 call
        /// It would be better to implement this as a POST method
        /// Assumes IDs are handled, and throws exception and quits even if one of the ids is invalid
        /// Twitter API allows both UserID and ScreenName in the same call, but this offers only either 1 of them
        /// </remarks>
        public static List<Dictionary<string, dynamic>> getUserInfo(List<string> ID,TwitterClasses.UserInfoType uitype)
        {
            if (ID.Count == 0) return new List<Dictionary<string, dynamic>>(); //handle empty input
            string call_params = "";
            List<Dictionary<string, dynamic>> response = new List<Dictionary<string, dynamic>>();
            for (int i = 0; i < ID.Count; i++)
            {
                call_params += ID[i] + ","; //comma separation
                if ((i + 1) % 99 == 0  //100 is max number of requests allowed, but we go for 99 
                     || i == (ID.Count - 1))  //or if it's the last ID
                {
                    try
                    {
                        if(uitype==UserInfoType.ID)
                            response = ListDictDeserializeJSON(JSONresponse(JSONTwitterURLs.lookupID, call_params));
                        else
                            response = ListDictDeserializeJSON(JSONresponse(JSONTwitterURLs.lookupScreenName, call_params));
                        call_params = string.Empty; //empty it
                    }
                    catch (Exceptions.UserNotFoundException e)
                    {
                        Console.WriteLine("At least 1 user not found");
                        throw e;
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine("Some Error");
                        throw e;
                    }
                }
            }
            return response;
        }
        /// <summary>
        /// Method to check if an account is protected. Accepts either ID or Screen Name. (Default Screen Name).
        /// </summary>
        /// <param name="user">User Id or handle</param>
        /// <param name="uitype">Specify if it is an ID or handle. Screen Name By Default</param>
        /// <returns>True if protected. False if Not</returns>
        public static bool isProtectedAccount(string user,UserInfoType uitype=UserInfoType.ScreenName)
        {
            List<string> userL = new List<string>(1);
            userL.Add(user);
            return getUserInfo(userL,uitype)[0]["protected"];
        }
        public static UInt64 followerCount(string handle)
        {
            List<string> userL = new List<string>(1);
            userL.Add(handle);
            return (UInt64)getUserInfo(userL, UserInfoType.ScreenName)[0]["followers_count"];
        }
    }
}
