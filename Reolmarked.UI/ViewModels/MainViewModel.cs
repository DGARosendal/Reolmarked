using Reolmarked.Core.Repositories;
using Reolmarked.UI.Commands;
using System;
using System.Collections.Generic;
using System.Text;

namespace Reolmarked.UI.ViewModels
{
    public class MainViewModel : ViewModelBase
    {
        public RelayCommand ExampleView1Command { get; set; }
        public RelayCommand ExampleView2Command { get; set; }

        public Example1ViewModel Example1VM { get; set; }
        public Example2ViewModel Example2VM { get; set; }

        private object _currentView;

        public object CurrentView
        {
            get { return _currentView; }
            set { _currentView = value; OnPropertyChanged(); }
        }

        public MainViewModel(IShelfRepository shelfRepository)
        {
            Example1VM = new Example1ViewModel(shelfRepository);
            Example2VM = new Example2ViewModel();
            CurrentView = Example1VM;

            ExampleView1Command = new RelayCommand(o =>
                CurrentView = Example1VM
            );
            ExampleView2Command = new RelayCommand(o =>
                CurrentView = Example2VM
            );
        }
    }
}
