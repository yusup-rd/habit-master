namespace HabitMaster
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();

            Routing.RegisterRoute(nameof(AddHabit), typeof(AddHabit));

            GoToAsync("//MainPage");
        }
    }
}
