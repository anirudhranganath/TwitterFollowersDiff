using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TwitterFollowersDiff
{
    /// <summary>
    /// The main application of TwitterFollowersDiff
    /// </summary>
    class Program
    {
        /// <summary>
        /// Program scope store of the handle
        /// </summary>
        static string handle;
        static UInt64 followerLimit = 70000;
        static void Main(string[] args)
        {
            App(); //calling main procedure
        }
        /// <summary>
        /// Method to check if connection to twitter and handle exist
        /// </summary>
        /// <param name="handle">User Handle</param>
        /// <returns>True if Connection and Handle Exist. Else False.</returns>
        static bool CheckConnectionAndHandle(string handle)
        {
            try
            {
                if (!WebInteraction.checkUserExists(handle))
                {
                    Console.WriteLine("No such handle exists.");
                    return false;
                }
            }
            catch (Exception e) //handles if theres no net connection
            {
                Console.WriteLine(e.Message);
                return false;
            }
            return true;
        }
        // Gets handle as input and returns as string
        static string InputGetHandle()
        {
            Console.Write("Enter Twitter handle: ");
            return Console.ReadLine().Trim();
        }
        /// <summary>
        /// Checks if history of the user exists. Which is equivalent to checking if 
        /// </summary>
        /// <param name="handle">User handle</param>
        /// <returns>True if history exists. Else False</returns>
        static bool hasHistory(string handle)
        {
            if (FileInteraction.hasFile(handle))
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        //Method that handles press any key to contiue from multiple locations
        static void AnyKey()
        {
            Console.WriteLine(Environment.NewLine+"Press any key to continue");
            Console.ReadKey();
        }
        /// <summary>
        /// Main procedure
        /// </summary>
        static void App()
        {
            #region HandleConnProtectedCheckFollowers
            handle = InputGetHandle(); //get and store the handle
            if (!CheckConnectionAndHandle(handle)) //no connection and no handle
            {
                AnyKey();
                return;
            }
            if(WebInteraction.isProtectedAccount(handle,TwitterClasses.UserInfoType.ScreenName)) //if account is protected
            {
                Console.WriteLine("{0} is a protected account. We access only public info.",handle);
                AnyKey();
                return;
            }
            UInt64 fc = WebInteraction.followerCount(handle);
            if (fc > followerLimit)
            {
                Console.WriteLine("Too many followers ({0}). Due to limited API calls, the restriction is {1} followers.",fc,followerLimit);
                AnyKey();
                return;
            }
            #endregion
            if (!hasHistory(handle)) //if user history does not exist
            {
                Console.WriteLine("This is the First Time you are using this app! We cant provide any stats but we'll save your data!");
                try
                {
                    FileInteraction.saveFollowers(handle, WebInteraction.followerIDsOfUser(handle));
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                    AnyKey();
                    return;
                }
            }
            else // we have twitter data
            {
                diffOutput(handle);                
            }
            AnyKey();
        }
        /// <summary>
        /// Method to find and output who followed and unfollowed you
        /// </summary>
        /// <param name="handle">The User Handle to Process</param>
        /// <remarks>
        /// Handles are found only for the differences. Else, we stick to the IDs and save them.
        /// This saves a lot of API calls
        /// Output is handled in this function
        /// Future Revisions:
        /// 1) This method of coding is bad. It needs to be modularised as:
        ///     a) Getting Lists
        ///     b) Finding Difference
        ///     c) Output them
        /// 2) There should be a better way to compare and remove list elements. We could use that.
        /// </remarks>
        private static void diffOutput(string handle)
        {
            List<string> saved = FileInteraction.getFollowers(handle);
            List<string> saved_copy = saved;
            List<string> current = new List<string>();
            #region GetFromTwitterAndSave
            try
            {
                current = WebInteraction.followerIDsOfUser(handle);
                Console.WriteLine("You have {0} followers currently", current.Count);
            }
            catch(Exception e)
            {
                Console.WriteLine(e.Message);
                return;
            }
            FileInteraction.saveFollowers(handle, current);
            #endregion
            #region RemovingDifferences
            foreach (string k in saved_copy.ToList())
            {
                if (current.Contains(k)) //removing whats there in both. We want handles of only those who have followed or unfollowed
                {
                    current.Remove(k);
                    saved.Remove(k);
                }
            }
            #endregion
            #region output
            Console.WriteLine(Environment.NewLine+"{0} people stopped following you and {1} started following you", saved.Count, current.Count);
            if (saved.Count > 0)
            {
                Console.WriteLine("Stopped Following: ");
                WebInteraction.handleOfIDs(saved).ForEach(Print);
            }
            if (current.Count > 0)
            {
                Console.WriteLine("\n\nStarted Following: ");
                WebInteraction.handleOfIDs(current).ForEach(Print);
            }
            #endregion
        }
        /// <summary>
        /// Just prints a string. Used as ForEach Delegate for the output
        /// </summary>
        /// <param name="s">string to print, usually handle</param>
        public static void Print(string s)
        {
            Console.WriteLine(s);
        }
    }
}
