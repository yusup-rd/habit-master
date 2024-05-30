using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Firebase;
using Firebase.Database;
using Firebase.Database.Query;

namespace HabitMaster
{
    internal class FirebaseHelper
    {
        private static FirebaseClient firebase = new FirebaseClient("https://habitmaster-4e5bd-default-rtdb.asia-southeast1.firebasedatabase.app/");
        private static Timer _cleanupTimer;
        private static readonly object _lock = new object();
        private static bool _isCleanupTimerInitialized = false;

        static FirebaseHelper()
        {
            InitializeCleanupTimer();
        }

        public FirebaseHelper()
        {
            // Ensure the cleanup timer is initialized
            InitializeCleanupTimer();
        }

        private static void InitializeCleanupTimer()
        {
            lock (_lock)
            {
                if (!_isCleanupTimerInitialized)
                {
                    // Set up the timer to run the cleanup task every week (7 days)
                    _cleanupTimer = new Timer(async (e) => await CleanUpDatabase(), null, TimeSpan.Zero, TimeSpan.FromDays(7));
                    _isCleanupTimerInitialized = true;
                }
            }
        }

        private static async Task CleanUpDatabase()
        {
            try
            {
                var habitRecords = await firebase.Child("HabitRecords").OnceAsync<HabitRecord>();

                foreach (var record in habitRecords)
                {
                    await firebase.Child("HabitRecords").Child(record.Key).DeleteAsync();
                }

                Console.WriteLine("Database cleanup completed successfully.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error during database cleanup: {ex.Message}");
            }
        }

        public async Task AddHabit(string habitName, string icon, string goalType, GoalOptionData goalOption)
        {
            await firebase
                .Child("HabitRecords")
                .PostAsync(new HabitRecord()
                {
                    HabitName = habitName,
                    Icon = icon,
                    GoalType = goalType,
                    GoalOption = goalOption,
                    ProgressStatus = "New",
                    Progress = 0
                });
        }

        public async Task<List<HabitRecord>> GetAllHabitRecord()
        {
            return (await firebase
                .Child("HabitRecords")
                .OnceAsync<HabitRecord>()).Select(item => new HabitRecord
                {
                    HabitName = item.Object.HabitName,
                    Icon = item.Object.Icon,
                    GoalType = item.Object.GoalType,
                    GoalOption = item.Object.GoalOption,
                    ProgressStatus = item.Object.ProgressStatus,
                    Progress = item.Object.Progress
                }).ToList();
        }

        public async Task UpdateHabitRecord(HabitRecord updatedHabit)
        {
            var habitRecord = (await firebase
                .Child("HabitRecords")
                .OnceAsync<HabitRecord>()).FirstOrDefault(h => h.Object.HabitName == updatedHabit.HabitName);

            if (habitRecord != null)
            {
                await firebase
                    .Child("HabitRecords")
                    .Child(habitRecord.Key)
                    .PutAsync(updatedHabit);
            }
        }

        public async Task DeleteHabitRecord(HabitRecord habit)
        {
            var habitRecord = (await firebase
                .Child("HabitRecords")
                .OnceAsync<HabitRecord>()).FirstOrDefault(h => h.Object.HabitName == habit.HabitName);

            if (habitRecord != null)
            {
                await firebase
                    .Child("HabitRecords")
                    .Child(habitRecord.Key)
                    .DeleteAsync();
            }
        }

        public async Task<bool> CheckHabitExists(string habitName)
        {
            try
            {
                // Reference to the "HabitRecords" node in your Firebase database
                var habitsNode = firebase.Child("HabitRecords");

                // Query to check if any habit has the given name
                var habitQuery = await habitsNode
                    .OrderBy("HabitName")
                    .EqualTo(habitName)
                    .OnceAsync<HabitRecord>();

                // If any result is returned, it means a habit with the same name exists
                return habitQuery.Any();
            }
            catch (Exception ex)
            {
                // Handle any exceptions (e.g., network errors, database access issues)
                Console.WriteLine($"Error checking habit existence: {ex.Message}");
                return false;
            }
        }
    }
}