using Caliburn.Micro;
using Reposed.MVVM;

namespace Reposed.Menu
{
    public class MenuViewModel : ViewModelBase
    {
        readonly IWindowManager WINDOW_MANAGER;

        public MenuViewModel(IWindowManager windowManager)
        {
            WINDOW_MANAGER = windowManager;
        }

        public void OnOpenPreferences()
        {
            WINDOW_MANAGER.ShowDialog(IoC.Get<Preferences.PreferencesViewModel>());
        }
    }
}
