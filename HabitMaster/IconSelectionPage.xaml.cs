using Microsoft.Maui.Controls;
using System.Collections.ObjectModel;

namespace HabitMaster
{
    public partial class IconSelectionPage : ContentPage
    {
        public ObservableCollection<IconItem> IconList { get; set; }

        public IconSelectionPage()
        {
            InitializeComponent();
            Shell.SetTabBarIsVisible(this, false);
            IconList = new ObservableCollection<IconItem>
            {
                new IconItem("heart.svg"),
                new IconItem("yinyang.svg"),
                new IconItem("waterdrop.svg"),
                new IconItem("dumbbell.svg"),
                new IconItem("exercise.svg"),
                new IconItem("run.svg"),
                new IconItem("star.svg"),
                new IconItem("wind.svg"),
                new IconItem("sun.svg"),
                new IconItem("thundercloud.svg"),
                new IconItem("winter.svg"),
                new IconItem("earth.svg"),
                new IconItem("flower.svg"),
                new IconItem("tree.svg"),
                new IconItem("moon.svg"),
                new IconItem("smile.svg"),
                new IconItem("ticket.svg"),
                new IconItem("compass.svg"),
                new IconItem("dice.svg"),
                new IconItem("cross.svg"),
                new IconItem("message.svg"),
                new IconItem("fire.svg"),
                new IconItem("lightbulb.svg"),
                new IconItem("music.svg"),
                new IconItem("tv.svg"),
                new IconItem("photos.svg"),
                new IconItem("book.svg"),
                new IconItem("balloon.svg"),
                new IconItem("gift.svg"),
                new IconItem("cake.svg"),
                new IconItem("forkandspoon.svg"),
                new IconItem("coffee.svg"),
                new IconItem("martini.svg"),
                new IconItem("wine.svg"),
                new IconItem("beerglass.svg"),
                new IconItem("shoppingbag.svg"),
                new IconItem("price.svg"),
                new IconItem("cart.svg"),
                new IconItem("hanger.svg"),
                new IconItem("tshirt.svg"),
                new IconItem("bag.svg"),
                new IconItem("glasses.svg"),
                new IconItem("umbrella.svg"),
                new IconItem("nail.svg"),
                new IconItem("fastfood.svg"),
                new IconItem("scissors.svg"),
                new IconItem("comb.svg"),
                new IconItem("watch.svg"),
                new IconItem("time.svg"),
                new IconItem("alarm.svg"),
                new IconItem("bell.svg"),
                new IconItem("fruit.svg"),
                new IconItem("medicine.svg"),
                new IconItem("medicalsignboard.svg"),
                new IconItem("pulse.svg"),
                new IconItem("tooth.svg"),
                new IconItem("mortarboard.svg"),
                new IconItem("letter.svg"),
                new IconItem("pen.svg"),
                new IconItem("brush.svg"),
                new IconItem("coins.svg"),
                new IconItem("creditcard.svg"),
                new IconItem("stats.svg"),
                new IconItem("notes.svg"),
                new IconItem("male.svg"),
                new IconItem("female.svg"),
                new IconItem("meeting.svg"),
                new IconItem("house.svg"),
                new IconItem("bed.svg"),
                new IconItem("key.svg"),
                new IconItem("bin.svg"),
                new IconItem("fishing.svg"),
                new IconItem("cleaning.svg"),
                new IconItem("washingmachine.svg"),
                new IconItem("boat.svg"),
                new IconItem("cat.svg"),
                new IconItem("dog.svg"),
                new IconItem("cigarette.svg"),
                new IconItem("puzzle.svg"),
                new IconItem("todo.svg"),
                new IconItem("target.svg"),
            };
            BindingContext = this;
        }

        [Obsolete]
        private async void IconClicked(object sender, System.EventArgs e)
        {
            var selectedIcon = (sender as ImageButton)?.BindingContext as IconItem;
            if (selectedIcon != null)
            {
                MessagingCenter.Send(this, "IconSelected", selectedIcon.IconPath);
            }
            await Navigation.PopAsync();
        }

    }

    public class IconItem
    {
        public string IconPath { get; set; }

        public IconItem(string fileName)
        {
            IconPath = $"Resources/Images/{fileName}";
        }
    }
}
