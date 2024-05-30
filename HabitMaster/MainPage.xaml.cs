using Microsoft.Maui.Controls;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Maui.Graphics;

namespace HabitMaster
{
    public partial class MainPage : ContentPage
    {
        FirebaseHelper firebaseHelper = new FirebaseHelper();
        private bool isLoaded = false;
        private string previousProgressStatus;
        private System.Timers.Timer timer;
        private int startTime;
        private int progressTime;
        private HabitRecord currentlyActiveHabit;
        private Label timerLabel;
        private bool shouldStartTimer = false;
        private SwipeItemView startItemView;

        public MainPage()
        {
            InitializeComponent();
            this.Appearing += MainPage_Appearing;
        }

        private async void MainPage_Appearing(object sender, EventArgs e)
        {
            if (!isLoaded)
            {
                await LoadHabits();
                isLoaded = true;
            }
            else
            {
                await LoadHabits();
            }
        }

        private async Task LoadHabits()
        {
            habitStackLayout.Children.Clear();
            List<HabitRecord> habits = await firebaseHelper.GetAllHabitRecord();

            foreach (var habit in habits)
            {
                if (habit.ProgressStatus == "Skipped")
                {
                    continue;
                }

                if (habit.Progress == 0)
                {
                    habit.ProgressStatus = "New";
                }
                else if (habit.Progress < habit.GoalOption.UnitNumber)
                {
                    habit.ProgressStatus = "In Progress";
                }
                else if (habit.Progress == habit.GoalOption.UnitNumber)
                {
                    habit.ProgressStatus = "Completed";
                }

                await firebaseHelper.UpdateHabitRecord(habit);
            }

            // Fetch habit records from the database
            habits = await firebaseHelper.GetAllHabitRecord();

            foreach (var habit in habits)
            {
                var swipeView = new SwipeView();

                // Left swipe items for Skip, Undo, Delete, and Reset
                var leftSwipeItems = new SwipeItems();

                var deleteItemView = new SwipeItemView
                {
                    Content = new Frame
                    {
                        WidthRequest = 88,
                        HeightRequest = 88,
                        CornerRadius = 15,
                        HasShadow = false,
                        BackgroundColor = new Color(235, 87, 87, 255),
                        Content = new Image { Source = "bin.svg", HorizontalOptions = LayoutOptions.Center, VerticalOptions = LayoutOptions.Center, WidthRequest = 24, HeightRequest = 24 }
                    }
                };
                deleteItemView.Invoked += (s, args) => HandleDelete(habit);
                leftSwipeItems.Add(deleteItemView);

                if (habit.ProgressStatus == "Skipped")
                {
                    var undoItemView = new SwipeItemView
                    {
                        Content = new Frame
                        {
                            WidthRequest = 88,
                            HeightRequest = 88,
                            CornerRadius = 15,
                            HasShadow = false,
                            BackgroundColor = Color.FromHex("#FFC154"),
                            Content = new Image { Source = "undo.svg", HorizontalOptions = LayoutOptions.Center, VerticalOptions = LayoutOptions.Center, WidthRequest = 32, HeightRequest = 32 }
                        }
                    };
                    undoItemView.Invoked += (s, args) => HandleUndoSkip(habit); // Add the event handler
                    leftSwipeItems.Add(undoItemView);
                }
                else
                {
                    var skipItemView = new SwipeItemView
                    {
                        Content = new Frame
                        {
                            WidthRequest = 88,
                            HeightRequest = 88,
                            CornerRadius = 15,
                            HasShadow = false,
                            BackgroundColor = Color.FromHex("#FFC154"),
                            Content = new Image { Source = "skip.svg", HorizontalOptions = LayoutOptions.Center, VerticalOptions = LayoutOptions.Center, WidthRequest = 40, HeightRequest = 40 }
                        }
                    };
                    skipItemView.Invoked += (s, args) => HandleSkip(habit);
                    leftSwipeItems.Add(skipItemView);
                }

                var resetItemView = new SwipeItemView
                {
                    Content = new Frame
                    {
                        WidthRequest = 88,
                        HeightRequest = 88,
                        CornerRadius = 15,
                        HasShadow = false,
                        BackgroundColor = Color.FromHex("#607D8B"),
                        Content = new Image { Source = "restart.svg", HorizontalOptions = LayoutOptions.Center, VerticalOptions = LayoutOptions.Center, WidthRequest = 24, HeightRequest = 24 }
                    }
                };
                resetItemView.Invoked += (s, args) => HandleReset(habit);

                if (habit.Progress != 0 && (habit.GoalType != "Timer" || !IsTimerActive(habit)))
                {
                    leftSwipeItems.Add(resetItemView);
                }

                swipeView.LeftItems = leftSwipeItems;

                // Right swipe items based on habit type
                var rightSwipeItems = new SwipeItems();
                if (habit.GoalType == "Units")
                {
                    // Calculate half of the remaining unit number
                    int remaining = habit.GoalOption.UnitNumber - habit.Progress;
                    int half = remaining / 2;

                    // Complete swipe item
                    if (habit.Progress < habit.GoalOption.UnitNumber)
                    {
                        var completeItemView = new SwipeItemView
                        {
                            Content = new Frame
                            {
                                WidthRequest = 88,
                                HeightRequest = 88,
                                CornerRadius = 15,
                                HasShadow = false,
                                BackgroundColor = Color.FromHex("#27AE60"),
                                Content = new Image { Source = "tick.svg", HorizontalOptions = LayoutOptions.Center, VerticalOptions = LayoutOptions.Center, WidthRequest = 50, HeightRequest = 50 }
                            }
                        };
                        completeItemView.Invoked += (s, args) => HandleComplete(habit);

                        rightSwipeItems.Add(completeItemView);
                    }

                    // Add "Add Half" swipe item only if the remaining value is greater than 1
                    if (half > 1)
                    {
                        var addHalfItemView = new SwipeItemView
                        {
                            Content = new Frame
                            {
                                WidthRequest = 88,
                                HeightRequest = 88,
                                CornerRadius = 15,
                                HasShadow = false,
                                BackgroundColor = Color.FromHex("#607D8B"),
                                Content = new Label
                                {
                                    Text = half.ToString(), // Display the actual value being added
                                    HorizontalOptions = LayoutOptions.Center,
                                    VerticalOptions = LayoutOptions.Center,
                                    TextColor = Color.FromHex("#F5F5F5"),
                                    FontFamily = "PoppinsBold",
                                    FontSize = 16
                                }
                            }
                        };
                        addHalfItemView.Invoked += (s, args) => HandleAddHalfStep(habit); // Add the event handler

                        rightSwipeItems.Add(addHalfItemView);
                    }
                    if (habit.Progress < habit.GoalOption.UnitNumber)
                    {
                        var addItemView = new SwipeItemView
                        {
                            Content = new Frame
                            {
                                WidthRequest = 88,
                                HeightRequest = 88,
                                CornerRadius = 15,
                                HasShadow = false,
                                BackgroundColor = Color.FromHex("#607D8B"),
                                Content = new Label
                                {
                                    Text = "1",
                                    HorizontalOptions = LayoutOptions.Center,
                                    VerticalOptions = LayoutOptions.Center,
                                    TextColor = Color.FromHex("#F5F5F5"),
                                    FontFamily = "PoppinsBold",
                                    FontSize = 16
                                }
                            }
                        };
                        addItemView.Invoked += (s, args) => HandleAddStep(habit);

                        rightSwipeItems.Add(addItemView);
                    }
                }
                else if (habit.GoalType == "Timer")
                {
                    if (habit.ProgressStatus != "Completed" && habit.ProgressStatus != "Skipped") 
                    { 
                    

                    startItemView = new SwipeItemView
                    {
                        Content = new Frame
                        {
                            WidthRequest = 88,
                            HeightRequest = 88,
                            CornerRadius = 15,
                            HasShadow = false,
                            BackgroundColor = Color.FromHex("#27AE60"),
                            Content = new Image { Source = "start.svg", HorizontalOptions = LayoutOptions.Center, VerticalOptions = LayoutOptions.Center, WidthRequest = 32, HeightRequest = 32 }
                        }
                    };
                    startItemView.Invoked += (s, args) => HandleStartTimer(habit);

                    rightSwipeItems.Add(startItemView);
                }
                    }
                swipeView.RightItems = rightSwipeItems;

                // Frame for habit content
                var frame = new Frame
                {
                    CornerRadius = 15,
                    Padding = 10,
                    HasShadow = false
                };

                // Stack layout for habit content
                var stackLayout = new StackLayout { Orientation = StackOrientation.Horizontal, Spacing = 10 };
                var iconImage = new Image { Source = habit.Icon, WidthRequest = 24, HeightRequest = 24, VerticalOptions = LayoutOptions.Center, Margin = new Thickness(10, 0, 0, 0) };
                var nameLabel = new Label { Text = habit.HabitName, TextColor = Color.FromHex("#F5F5F5"), FontSize = 16, FontFamily = "PoppinsSemibold", VerticalOptions = LayoutOptions.Center };
                var progressStatusLabel = new Label { Text = habit.ProgressStatus, TextColor = Color.FromHex("#CCCCCC"), FontSize = 12, FontFamily = "PoppinsRegular", VerticalOptions = LayoutOptions.Center };
                var boxView = new BoxView { HorizontalOptions = LayoutOptions.FillAndExpand, BackgroundColor = new Color(0, 0, 0, 0) };
                var goalFrame = new Frame
                {
                    BorderColor = new Color(0, 0, 0, 0),
                    CornerRadius = 10,
                    Padding = 15
                };

                if (habit.ProgressStatus == "Skipped")
                {
                    frame.BackgroundColor = Color.FromHex("#333333");
                    goalFrame.BackgroundColor = new Color(0, 0, 0, 0);
                }
                else if (habit.ProgressStatus == "Completed")
                {
                    frame.BackgroundColor = Color.FromHex("#27AE60");
                    goalFrame.BackgroundColor = new Color(0, 0, 0, 0);
                }
                else
                {
                    frame.BackgroundColor = Color.FromHex("#3B5998");
                    goalFrame.BackgroundColor = Color.FromHex("#FF6F61");
                }

                // Create goal content based on habit type
                goalFrame.Content = CreateGoalContent(habit);

                // Add views to stack layout
                stackLayout.Children.Add(iconImage);
                stackLayout.Children.Add(nameLabel);
                stackLayout.Children.Add(progressStatusLabel);
                stackLayout.Children.Add(boxView);
                stackLayout.Children.Add(goalFrame);

                // Add stack layout to frame content
                frame.Content = stackLayout;

                // Add frame to swipe view content
                swipeView.Content = frame;

                // Add swipe view to habit stack layout
                habitStackLayout.Children.Add(swipeView);
            }
        }

        // Handle skip action
        private async void HandleSkip(HabitRecord habit)
        {
            // Store the current ProgressStatus before setting it to "Skipped"
            string previousProgressStatus = habit.ProgressStatus;

            // Update the progress status to "Skipped"
            habit.ProgressStatus = "Skipped";

            // Update the habit record in the database
            await firebaseHelper.UpdateHabitRecord(habit);

            // Reload the habits to reflect the changes
            await LoadHabits();
        }

        // Handle undo action
        private async void HandleUndoSkip(HabitRecord habit)
        {
            // Set the ProgressStatus back to its previous value
            habit.ProgressStatus = previousProgressStatus;

            // Update the habit record in the database
            await firebaseHelper.UpdateHabitRecord(habit);

            // Reload the habits to reflect the changes
            await LoadHabits();
        }

        // Handle delete action
        private async Task<bool> DisplayConfirmationDialog(string habitName)
        {
            return await DisplayAlert("Confirm Deletion", $"Are you sure you want to delete the habit '{habitName}'?", "Yes", "No");
        }
        private async void HandleDelete(HabitRecord habit)
        {
            bool isConfirmed = await DisplayConfirmationDialog(habit.HabitName);
            if (isConfirmed)
            {
                // Remove the habit from the database
                await firebaseHelper.DeleteHabitRecord(habit);

                // Reload the habits to reflect the changes
                await LoadHabits();
            }
        }

        // Handle reset action
        private async void HandleReset(HabitRecord habit)
        {
            // Reset the progress to 0
            habit.Progress = 0;

            // Update the habit record in the database
            await firebaseHelper.UpdateHabitRecord(habit);

            // Reload the habits to reflect the changes
            await LoadHabits();
        }

        // Handle add step action
        private async void HandleAddStep(HabitRecord habit)
        {
            // Increment the progress by 1
            habit.Progress += 1;

            // Update the habit record in the database
            await firebaseHelper.UpdateHabitRecord(habit);

            // Reload the habits to reflect the changes
            await LoadHabits();
        }

        // Handle add half step action
        private async void HandleAddHalfStep(HabitRecord habit)
        {
            // Calculate half of the remaining unit number
            int remaining = habit.GoalOption.UnitNumber - habit.Progress;
            int half = (int)Math.Floor((double)remaining / 2);

            // Increment the progress by half
            habit.Progress += half;

            // Update the habit record in the database
            await firebaseHelper.UpdateHabitRecord(habit);

            // Reload the habits to reflect the changes
            await LoadHabits();
        }

        // Handle complete action
        private async void HandleComplete(HabitRecord habit)
        {
            // Set the progress to the unit number
            habit.Progress = habit.GoalOption.UnitNumber;

            // Update the habit record in the database
            await firebaseHelper.UpdateHabitRecord(habit);

            // Reload the habits to reflect the changes
            await LoadHabits();
        }

        private bool IsTimerActive(HabitRecord habit)
        {
            // Check if the specified habit matches the currently active habit
            return currentlyActiveHabit != null && currentlyActiveHabit == habit;
        }
        private async void HandleStartTimer(HabitRecord habit)
        {
            // Store the currently active habit
            currentlyActiveHabit = habit;

            // Set the shouldStartTimer flag to true
            shouldStartTimer = true;

            // Start the timer countdown only if remaining time is greater than 0
            if (habit.Progress < habit.GoalOption.UnitNumber)
            {
                int remainingSeconds = habit.GoalOption.UnitNumber - habit.Progress;

                // Create a new modal page
                var modalPage = new ContentPage();

                // Convert remaining time to minutes and seconds
                int minutes = remainingSeconds / 60;
                int seconds = remainingSeconds % 60;

                // Create a stack layout to hold timer icon and countdown label
                var stackLayout = new StackLayout
                {
                    VerticalOptions = LayoutOptions.CenterAndExpand,
                    HorizontalOptions = LayoutOptions.CenterAndExpand,
                    Spacing = 10
                };

                // Create the timer icon
                var timerIcon = new Image
                {
                    Source = "timer_light.svg",
                    WidthRequest = 48,
                    HeightRequest = 48,
                    HorizontalOptions = LayoutOptions.Center
                };

                // Create a label to display the countdown
                var countdownLabel = new Label
                {
                    Text = $"{minutes:00}:{seconds:00}",
                    FontSize = 24,
                    FontFamily="PoppinsSemibold",
                    HorizontalOptions = LayoutOptions.Center,
                    VerticalOptions = LayoutOptions.CenterAndExpand
                };

                // Add the timer icon and countdown label to the stack layout
                stackLayout.Children.Add(timerIcon);
                stackLayout.Children.Add(countdownLabel);

                // Create a button to stop the timer
                var stopButton = new Button
                {
                    Text = "Stop",
                    TextColor=Color.FromHex("#F5F5F5"),
                    FontFamily = "PoppinsSemibold",
                    BackgroundColor = Color.FromHex("#FF6F61"),
                    HorizontalOptions = LayoutOptions.Center,
                    Margin = new Thickness(0, 20, 0, 0),
                    WidthRequest = 200,
                    HeightRequest = 60   
                };

                // Handle the stop button click event
                stopButton.Clicked += async (sender, args) =>
                {
                    // Call the HandleStopTimer method
                    HandleStopTimer(habit);
                    // Dismiss the modal page
                    await Navigation.PopModalAsync();
                };

                // Add the timer icon, countdown label, and stop button to the modal page's content
                modalPage.Content = new StackLayout
                {
                    Children = { timerIcon, countdownLabel, stopButton },
                    VerticalOptions = LayoutOptions.CenterAndExpand,
                    HorizontalOptions = LayoutOptions.FillAndExpand 
                };

                // Start the timer if remaining time is greater than 0
                if (remainingSeconds > 0)
                {
                    timer = new System.Timers.Timer(1000);
                    timer.Elapsed += (sender, e) => TimerElapsed(sender, e, habit, countdownLabel, modalPage);
                    timer.Start();
                }

                // Present the modal page
                await Navigation.PushModalAsync(modalPage);
            }
        }

        // Handle stop timer action
        private async void HandleStopTimer(HabitRecord habit)
        {
            // Stop the timer
            if (timer != null)
            {
                timer.Stop();
            }

            // Reset the currently active habit
            currentlyActiveHabit = null;

            // Update the habit record in the database
            await firebaseHelper.UpdateHabitRecord(habit);
        }

        private async void TimerElapsed(object sender, System.Timers.ElapsedEventArgs e, HabitRecord habit, Label timerLabel, ContentPage modalPage)
        {
            // Check if the habit matches the currently active habit
            if (habit == currentlyActiveHabit)
            {
                // Check if the timer should be running
                if (shouldStartTimer && habit.Progress < habit.GoalOption.UnitNumber)
                {
                    // Calculate the remaining time based on habit progress
                    int remainingSeconds = habit.GoalOption.UnitNumber - habit.Progress;

                    // Ensure the remaining time is not negative
                    if (remainingSeconds < 0)
                    {
                        remainingSeconds = 0;
                    }

                    // Decrease the remaining time by 1 second
                    remainingSeconds--;

                    // Update the habit progress
                    habit.Progress++;

                    // Update the habit record in the database and await the operation
                    await firebaseHelper.UpdateHabitRecord(habit);

                    // Update the UI with the new time left
                    Device.BeginInvokeOnMainThread(() =>
                    {
                        int minutes = remainingSeconds / 60;
                        int seconds = remainingSeconds % 60;
                        timerLabel.Text = $"{minutes:00}:{seconds:00}";
                    });

                    // Check if time is up
                    if (remainingSeconds <= 0)
                    {
                        // Stop the timer
                        timer.Stop();

                        // Show alert of completion on the main UI thread
                        Device.BeginInvokeOnMainThread(async () =>
                        {
                            await modalPage.DisplayAlert("Time Is Up!", $"Habit '{habit.HabitName}' completed!", "OK");

                            // Dismiss the modal page
                            await Navigation.PopModalAsync();

                            // Check if the progress status was set to "Skipped"
                            if (habit.ProgressStatus != "Skipped")
                            {
                                // Set progress status to Completed
                                habit.ProgressStatus = "Completed";

                                // Update the habit record in the database and await the operation
                                await firebaseHelper.UpdateHabitRecord(habit);
                            }

                            // Set the visibility of the start button to false if the habit is timer-based and completed
                            if (habit.GoalType == "Timer")
                            {
                                startItemView.IsVisible = false;
                            }
                        });
                    }
                }
            }
        }

        private Dictionary<HabitRecord, Label> timerLabels = new Dictionary<HabitRecord, Label>();

        // Helper method to create goal content based on habit type
        private View CreateGoalContent(HabitRecord habit)
        {
            if (habit.GoalType == "Units")
            {
                return new StackLayout
                {
                    Children = {
                        new Label { Text = $"{habit.Progress}/{habit.GoalOption.UnitNumber}", TextColor = Color.FromHex("#F5F5F5"), FontFamily = "PoppinsSemibold", FontSize = 14, HorizontalTextAlignment = TextAlignment.Center },
                        new Label { Text = habit.GoalOption.UnitType, TextColor = Color.FromHex("#F5F5F5"), FontFamily = "PoppinsSemibold", FontSize = 14, HorizontalTextAlignment = TextAlignment.Center },
                    }
                };
            }
            else if (habit.GoalType == "Timer")
            {
                // Calculate remaining time
                int remainingSeconds = habit.GoalOption.UnitNumber - habit.Progress;

                // Convert remaining time to minutes and seconds
                int minutes = remainingSeconds / 60;
                int seconds = remainingSeconds % 60;

                var timerIcon = new Image { Source = "timer_light.svg", WidthRequest = 24, HeightRequest = 24, VerticalOptions = LayoutOptions.Center };
                var timerLabel = new Label
                {
                    Text = $"{minutes:00}:{seconds:00}",
                    FontFamily = "PoppinsSemibold",
                    TextColor = Color.FromHex("#F5F5F5"),
                    FontSize = 14,
                    HorizontalTextAlignment = TextAlignment.Center
                };
                // Add the timer label to the dictionary with the habit as the key
                timerLabels.Add(habit, timerLabel);

                return new StackLayout
                {
                    Children = {
                        timerIcon,
                        timerLabel
                    },
                    Orientation = StackOrientation.Vertical
                };
            }
            return null;
        }

        private async void OnAddHabitButtonClicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new AddHabit());
            await LoadHabits();
        }
    }
}