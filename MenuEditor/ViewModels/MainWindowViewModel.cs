﻿using MenuEditor.Services;
using Prism.Mvvm;
using Prism.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.ObjectModel;
using Newtonsoft.Json;
using System.Windows.Controls;

namespace MenuEditor.ViewModels
{
    class MainWindowViewModel : BindableBase
    {
        public MainWindowViewModel() { }

        public MainWindowViewModel(IDataService dataService)
        {
            workPath = Configuration.GetValue<string>("finallyopen");
            this.dataService = dataService;

            TopMenu.SaveData += onSaveData;
            TopMenu.NewProject += onNewProj;

            AddMenuCommand = new DelegateCommand<string>(new Action<string>(AddMenu));
            OpenDialogCommand = new DelegateCommand<string>(new Action<string>(OpenDialog));

            if (System.IO.Directory.Exists(workPath))
            {
                MainWindowViewModel rawMenu = null;
                rawMenu = dataService.LoadData<MainWindowViewModel>(workPath + "/src.menu");
                load(rawMenu);
            }
        }

        [JsonIgnore]
        private IDataService dataService;

        [JsonIgnore]
        private string workPath;

        public void onSaveData()
        {
            if (!System.IO.Directory.Exists(workPath))
            {
                var Dialog = new System.Windows.Forms.FolderBrowserDialog();
                Dialog.Description = "文件夹必须为空";
                var result = Dialog.ShowDialog();
                if (result == System.Windows.Forms.DialogResult.OK)
                {
                    System.IO.DirectoryInfo di = new System.IO.DirectoryInfo(Dialog.SelectedPath);
                    if (di.GetFiles().Length + di.GetDirectories().Length == 0)
                    {
                        //目录为空
                        workPath = Dialog.SelectedPath;
                        _ = dataService.SaveData(workPath + "/src.menu", this);
                    }
                }
            }
            else
            {
                _ = dataService.SaveData(workPath + "/src.menu", this);
            }
        }

        public void onNewProj()
        {
            var Dialog = new System.Windows.Forms.FolderBrowserDialog();
            Dialog.Description = "文件夹必须为空";
            var result = Dialog.ShowDialog();
            if (result == System.Windows.Forms.DialogResult.OK)
            {
                System.IO.DirectoryInfo di = new System.IO.DirectoryInfo(Dialog.SelectedPath);
                if (di.GetFiles().Length + di.GetDirectories().Length == 0)
                {
                    //目录为空
                    workPath = Dialog.SelectedPath;
                    _ = dataService.SaveData(workPath + "/src.menu", this);
                }
            }
        }

        private void load(MainWindowViewModel rawMenu)
        {
            MenuCollection = rawMenu.MenuCollection;
            ModalCollection = rawMenu.ModalCollection;

            foreach (var item in MenuCollection)
            {
                var vmodel = item;
                item.EditMenu = new Views.EditMenu(ref vmodel);
            }

            foreach (var item in ModalCollection)
            {
                var vmodel = item;
                item.EditModalDialog = new Views.EditModalDialog(ref vmodel);
            }
        }

        private PageViewModel currentEditMenu;
        [JsonIgnore]
        public PageViewModel CurrentEditMenu
        {
            get => currentEditMenu;
            set
            {
                _ = SetProperty(ref currentEditMenu, value);
                if (value != null)
                {
                    EditSpace = value.EditMenu;
                }
            }
        }

        private ModalDialogVewModel currentEditModal;
        [JsonIgnore]
        public ModalDialogVewModel CurrentEditModal
        {
            get => currentEditModal;
            set
            {
                _ = SetProperty(ref currentEditModal, value);
                if (value != null)
                {
                    EditSpace = value.EditModalDialog;
                }
            }
        }
        private Command currentButton;
        [JsonIgnore]
        public Command CurrentButton
        {
            get => currentButton;
            set
            {
                _ = SetProperty(ref currentButton, value);
                if (value != null)
                {
                    CmdSpace = value.AddButtonCommand;
                }
            }
        }

        private UserControl editSpace;
        [JsonIgnore]
        public UserControl EditSpace
        {
            get => editSpace;
            set => SetProperty(ref editSpace, value);
        }

        private UserControl cmdSpace;
        [JsonIgnore]
        public UserControl CmdSpace
        {
            get => cmdSpace;
            set => SetProperty(ref cmdSpace, value);
        }


        [JsonIgnore]
        public TopMenuViewModel TopMenu = new TopMenuViewModel();

        private ObservableCollection<PageViewModel> _MenuCollection = new() { };
        [JsonProperty]
        public ObservableCollection<PageViewModel> MenuCollection
        {
            get => _MenuCollection;
            set => SetProperty(ref _MenuCollection, value);
        }

        private ObservableCollection<ModalDialogVewModel> _ModalCollection = new() { };
        [JsonProperty]
        public ObservableCollection<ModalDialogVewModel> ModalCollection
        {
            get => _ModalCollection;
            set => SetProperty(ref _ModalCollection, value);
        }

        private string addType;
        [JsonIgnore]
        public string AddItemType
        {
            get => addType;
            set => SetProperty(ref addType, value);
        }

        private string addName;
        [JsonIgnore]
        public string AddItemName
        {
            get => addName;
            set => SetProperty(ref addName, value);
        }

        [JsonIgnore]
        public AddItemDialogViewModel DialogViewModel { get; set; } = new() { };

        [JsonIgnore]
        public DelegateCommand<string> AddMenuCommand { get; set; }
        private void AddMenu(string type)
        {
            if (type == "menu")
            {
                var menu = new PageViewModel()
                {
                    FileName = AddItemName
                };
                menu.EditMenu = new Views.EditMenu(ref menu);
                MenuCollection.Add(menu);
            }
            else if (type == "modal")
            {
                var menu = new ModalDialogVewModel()
                {
                    FileName = AddItemName
                };
                menu.EditModalDialog = new Views.EditModalDialog(ref menu);
                ModalCollection.Add(menu);
            }

            DialogViewModel.IsDialogOpen = false;
            AddItemName = "";
        }

        [JsonIgnore]
        public DelegateCommand<string> OpenDialogCommand { get; set; }
        private void OpenDialog(string arg)
        {
            AddItemType = arg;
            DialogViewModel.DialogContent = new Views.AddItemDialog();
            DialogViewModel.IsDialogOpen = true;
        }
    }
}
