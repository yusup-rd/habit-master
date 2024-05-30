using System.IO;
using static System.Net.WebRequestMethods;

namespace HabitMaster
{
    public partial class AddHabit : ContentPage
    {
        FirebaseHelper firebaseHelper = new FirebaseHelper();
        public AddHabit()
        {
            InitializeComponent();
            InitializePickers();
            Shell.SetTabBarIsVisible(this, false);
            unitsOptions.IsVisible = true;
            timerOptions.IsVisible = false;
            unitsButton.BackgroundColor = Color.FromHex("FF6F61"); // Active color
            timerButton.BackgroundColor = Color.FromHex("333333"); // Inactive color

            // Set the default values
            habitNameEntry.Text = "My Habit";
            selectedIcon.Source = "Resources/Images/heart.svg";
            unitsNumberPicker.SelectedIndex = 0;
            unitsUnitPicker.SelectedIndex = 0; 
            timerPicker.SelectedIndex = 0;      

            // Subscribe to the message sent from IconSelectionPage
            MessagingCenter.Subscribe<IconSelectionPage, string>(this, "IconSelected", (sender, selectedIconPath) =>
            {
                // Set the selected icon path to the ImageButton source
                selectedIcon.Source = selectedIconPath;
            });

        }

        private void UnitsButton_Clicked(object sender, EventArgs e)
        {
            unitsOptions.IsVisible = true;
            timerOptions.IsVisible = false;
            unitsButton.BackgroundColor = Color.FromHex("FF6F61"); // Active color
            timerButton.BackgroundColor = Color.FromHex("333333"); // Inactive color
        }

        private void TimerButton_Clicked(object sender, EventArgs e)
        {
            unitsOptions.IsVisible = false;
            timerOptions.IsVisible = true;
            unitsButton.BackgroundColor = Color.FromHex("333333"); // Inactive color
            timerButton.BackgroundColor = Color.FromHex("FF6F61"); // Active color
        }

        private void InitializePickers()
        {
            // Populate units number picker
            for (int i = 1; i <= 999; i++)
            {
                unitsNumberPicker.Items.Add(i.ToString());
            }

            // Populate units keyword picker
            unitsUnitPicker.Items.Add("Kilometers");
            unitsUnitPicker.Items.Add("Times");
            unitsUnitPicker.Items.Add("Repetitions");
            unitsUnitPicker.Items.Add("Servings");
            unitsUnitPicker.Items.Add("Pages");
            unitsUnitPicker.Items.Add("Chapters");
            unitsUnitPicker.Items.Add("Glasses");
            unitsUnitPicker.Items.Add("Cups");
            unitsUnitPicker.Items.Add("Steps");
            unitsUnitPicker.Items.Add("Sessions");


            // Populate timer picker
            for (int i = 1; i <= 60; i++)
            {
                string minuteText = i == 1 ? " minute" : " minutes";
                timerPicker.Items.Add(i.ToString() + minuteText);
            }
        }

        async void OnAddClicked(object sender, EventArgs e)
        {
            string habitName = habitNameEntry.Text.Trim();

            if (string.IsNullOrWhiteSpace(habitName))
            {
                // ALert on empty Habit Name input
                await DisplayAlert("Error", "Please fill in the habit name.", "OK");
                return;
            }

            // Call the CheckHabitExists method
            bool habitExists = await firebaseHelper.CheckHabitExists(habitName);

            if (habitExists)
            {
                // Habit with the same name already exists, show alert
                await DisplayAlert("Name already taken", "A habit with the same name already exists. Please choose a different name.", "OK");
                return;
            }

            string iconPath = selectedIcon.Source.ToString();

            // Extract just the filename from the icon path
            string iconName = Path.GetFileName(iconPath);

            string goalType;
            GoalOptionData goalOption;

            if (unitsOptions.IsVisible)
            {
                goalType = "Units";
                goalOption = new GoalOptionData
                {
                    UnitNumber = int.Parse(unitsNumberPicker.SelectedItem.ToString()),
                    UnitType = unitsUnitPicker.SelectedItem.ToString()
                };
            }
            else
            {
                goalType = "Timer";
                goalOption = new GoalOptionData
                {
                    UnitNumber = 60 * int.Parse(timerPicker.SelectedItem.ToString().Split(' ')[0]),
                    UnitType = "seconds"
                };
            }
            // Call the FirebaseHelper method to add the habit
            await firebaseHelper.AddHabit(habitName, iconName, goalType, goalOption);

            await DisplayAlert("New Habit added", "You have successfully added a new habit.", "OK");

            await Navigation.PopAsync();
        }


        private void OnBackClicked(object sender, EventArgs e)
        {
            Navigation.PopAsync();
        }

        private async void SelectIconClicked(object sender, EventArgs e)
        {
            loadingModal.IsVisible = true;
            await Navigation.PushAsync(new IconSelectionPage());
            loadingModal.IsVisible = false;
        }

    }
}
