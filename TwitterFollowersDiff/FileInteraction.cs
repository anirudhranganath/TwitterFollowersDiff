using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace TwitterFollowersDiff
{
    /// <summary>
    /// This class is handles the File IO and the saving of twitter follower data
    /// </summary>
    class FileInteraction
    {
        /// <summary>
        /// Saves followers list(binary file) in userHandle.twFol file within the executing folder
        /// </summary>
        /// <param name="userHandle">the handle of the user</param>
        /// <param name="t">string list of followers</param>
        /// <remarks> 
        /// The standard for this program is to save their IDs
        /// There should be a better way to serialize and deserialize the followers
        /// </remarks>
        public static void saveFollowers(string userHandle,List<string> t)
        {
            FileStream fs = new FileStream(userHandle+".twFol", FileMode.Create);
            BinaryFormatter bf = new BinaryFormatter();
            bf.Serialize(fs, t);
            fs.Close();
        }
        /// <summary>
        /// Gets followers from userHandle.twFol file within the executing folder
        /// </summary>
        /// <param name="userHandle">the handle of the user</param>
        /// <remarks> 
        /// The standard for this program is to save their IDs
        /// There should be a better way to serialize and deserialize the followers
        /// </remarks>
        public static List<string> getFollowers(string userHandle)
        {
            List<string> followers;
            if(!File.Exists(userHandle+".twFol"))
                throw new FileNotFoundException();
            FileStream fs = new FileStream(userHandle + ".twFol", FileMode.Open);
            BinaryFormatter bf = new BinaryFormatter();
            followers = (List<String>)bf.Deserialize(fs);
            fs.Close();
            return followers;
        }
        /// <summary>
        /// Check if a file for a particular exists. Ergo, we know if the user has used this before
        /// in which case a history is present. History is limited to the past use of this app, 
        /// which stores only the previous list of followers
        /// </summary>
        /// <param name="handle">handle of the user</param>
        public static bool hasFile(string handle)
        {
            if (File.Exists(handle + ".twFol"))
                return true;
            else
                return false;
        }
    }
}
