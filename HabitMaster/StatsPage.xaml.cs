using Microsoft.Maui.Graphics;
using System;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Shapes;

namespace HabitMaster
{
    public partial class StatsPage : ContentPage
    {
        FirebaseHelper firebaseHelper = new FirebaseHelper();

        public StatsPage()
        {
            InitializeComponent();
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            await DisplayProductivity();
        }

        private async Task DisplayProductivity()
        {
            // Wrap the statistics layout inside a ScrollView
            ScrollView scrollView = new ScrollView();

            // Create the statistics layout
            StackLayout statisticsLayout = new StackLayout
            {
                Margin = new Thickness(10)
            };

            double overallCompletionPercentage = 0;

            // Intro label
            Label introLabel = new Label
            {
                Text = "Track Your Progress!",
                TextColor = Color.FromHex("#F5F5F5"),
                FontFamily = "PoppinsRegular",
                FontSize = 16,
                HorizontalTextAlignment = TextAlignment.Center,
                Margin = new Thickness(0, 15, 0, 15)
            };

            // Frame for overall productivity
            Frame overallFrame = new Frame
            {
                Margin = new Thickness(10),
                Padding = new Thickness(10),
                BackgroundColor = Color.FromHex("#333333"),
                HasShadow = true
            };

            // Stack layout for overall productivity content
            StackLayout overallContentLayout = new StackLayout();

            Border border = new Border
            {
                WidthRequest = 150,
                HeightRequest = 150,
                BackgroundColor = Color.FromRgba(0, 0, 0, 0),
                Stroke = Color.FromHex("#FF6F61"),
                StrokeThickness = 10,
                Padding = new Thickness(10),
                StrokeShape = new RoundRectangle { CornerRadius = new CornerRadius(75) },
                HorizontalOptions = LayoutOptions.Center,
                VerticalOptions = LayoutOptions.Center,
            };

            // Productivity label
            Label productivityLabel = new Label
            {
                Text = "Productivity",
                FontSize = 18,
                FontFamily = "PoppinsSemibold",
                HorizontalOptions = LayoutOptions.Center,
                VerticalOptions = LayoutOptions.End,
                TextColor = Color.FromHex("#F5F5F5"),
                Margin = new Thickness(0, 5, 0, 20)
            };

            // Description label
            Label desc = new Label
            {
                Text = "To get to 100%, complete all your daily tasks and habits.",
                FontSize = 14,
                FontFamily = "PoppinsRegular",
                HorizontalOptions = LayoutOptions.Center,
                VerticalOptions = LayoutOptions.Start,
                HorizontalTextAlignment = TextAlignment.Center,
                TextColor = Color.FromHex("#F5F5F5"),
                Margin = new Thickness(0, 20, 0, 5)
            };

            // Productivity Percentage label
            Label percentageLabel = new Label
            {
                Text = $"{overallCompletionPercentage:F2}%",
                FontSize = 24,
                FontFamily = "PoppinsSemibold",
                HorizontalOptions = LayoutOptions.Center,
                VerticalOptions = LayoutOptions.Center,
                TextColor = Color.FromHex("#F5F5F5")
            };

            // Productivity Progress bar
            ProgressBar overallProgressBar = new ProgressBar
            {
                Progress = overallCompletionPercentage / 100,
                ProgressColor = Color.FromHex("#FF6F61"),
                HeightRequest = 10,
                Margin = new Thickness(0, 10, 0, 0)
            };

            // Adding labels inside the circle border
            StackLayout circleContentLayout = new StackLayout
            {
                Children = { percentageLabel, overallProgressBar },
                HorizontalOptions = LayoutOptions.Center,
                VerticalOptions = LayoutOptions.Center
            };

            // Ordering the Productivity frame
            StackLayout overallFrameLayout = new StackLayout
            {
                Children = { productivityLabel, border, desc },
                HorizontalOptions = LayoutOptions.Center,
                VerticalOptions = LayoutOptions.Center
            };

            // Set the content of the Productivity frame layout
            border.Content = circleContentLayout;

            // Adding the stack layout to the Productivity frame
            overallFrame.Content = overallFrameLayout;

            // Adding the Productivity frame to the statistics layout
            statisticsLayout.Children.Add(introLabel);
            statisticsLayout.Children.Add(overallFrame);

            var habitRecords = await firebaseHelper.GetAllHabitRecord();

            int totalTasks = habitRecords.Count;

            double equalAllocationPercentage = 100.0 / totalTasks;

            Color[] habitColors = {
                Color.FromHex("#FF5733"),
                Color.FromHex("#C70039"),
                Color.FromHex("#6A1B9A"),
                Color.FromHex("#03A9F4"),
                Color.FromHex("#B71C1C"),
                Color.FromHex("#009688"),
                Color.FromHex("#388E3C"),
                Color.FromHex("#FFC107")
            };



            foreach (var habitRecord in habitRecords)
            {
                int progress;

                // Calculate the index of the color in the range 0 to 7
                int colorIndex = habitRecords.IndexOf(habitRecord) % 8;

                if (habitRecord.ProgressStatus == "Skipped")
                {
                    progress = 0;
                }
                else
                {
                    progress = habitRecord.Progress;
                }

                double completionPercentage = (double)progress / habitRecord.GoalOption.UnitNumber * 100;

                overallCompletionPercentage += equalAllocationPercentage * (completionPercentage / 100);

                // Creating a frame for each habit
                Frame habitFrame = new Frame
                {
                    Margin = new Thickness(10),
                    Padding = new Thickness(10),
                    BackgroundColor = Color.FromHex("#333333"),
                    HasShadow = true
                };

                // Habit name label
                Label habitNameLabel = new Label
                {
                    Text = habitRecord.HabitName,
                    FontSize = 18,
                    FontFamily = "PoppinsSemibold",
                    HorizontalOptions = LayoutOptions.Start,
                    VerticalOptions = LayoutOptions.Start
                };

                // Percentage label
                Label habitPercentageLabel = new Label
                {
                    Text = $"{completionPercentage:F2}%",
                    FontSize = 16,
                    FontFamily = "PoppinsSemibold",
                    HorizontalOptions = LayoutOptions.End,
                    VerticalOptions = LayoutOptions.Start
                };

                // Adding labels to a horizontal stack layout
                StackLayout labelsLayout = new StackLayout
                {
                    Orientation = StackOrientation.Horizontal,
                    HorizontalOptions = LayoutOptions.FillAndExpand,
                    Children = { habitNameLabel, new BoxView { HorizontalOptions = LayoutOptions.FillAndExpand, BackgroundColor = Color.FromRgba(0, 0, 0, 0) }, habitPercentageLabel }
                };

                // Habit progress bar
                ProgressBar habitProgressBar = new ProgressBar
                {
                    Progress = completionPercentage / 100,
                    ProgressColor = habitColors[colorIndex],
                    HeightRequest = 10,
                    Margin = new Thickness(0, 10, 0, 0)
                };

                // Adding labels layout and progress bar to the habit frame
                habitFrame.Content = new StackLayout
                {
                    Children = { labelsLayout, habitProgressBar }
                };

                // Adding the habit frame to the statistics layout
                statisticsLayout.Children.Add(habitFrame);
            }

            // Update overall percentage label and progress bar
            percentageLabel.Text = $"{overallCompletionPercentage:F2}%";
            overallProgressBar.Progress = overallCompletionPercentage / 100;

            // Set the content of the ScrollView
            scrollView.Content = statisticsLayout;

            // Set the Content of the page to the ScrollView
            Content = scrollView;
        }
    }
}
