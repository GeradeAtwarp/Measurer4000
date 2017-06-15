﻿using Measurer4000.Core.Models;
using Measurer4000.Core.Services;
using Measurer4000.Core.Services.Interfaces;
using Measurer4000.Services;
using Measurer4000.ViewModels.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;


namespace Measurer4000.ViewModels
{
    public class MainViewModel : BaseViewModel
    {
        private readonly MeasureService _measureService;
        private readonly IDialogService _fileDialogService;

        public MainViewModel()
        {
            _measureService = ServiceLocator.Get<MeasureService>();
            _measureService.FileManagerService = new FileManagerService();
            _fileDialogService = ServiceLocator.Get<FileDialogService>();
        }

        private bool _isBusy;

        public bool IsBusy
        {
            get { return _isBusy; }
            set
            {
                _isBusy = value;
                RaiseProperty();
            }
        }

        private CodeStats _stats = new CodeStats();

        public CodeStats Stats
        {
            get { return _stats; }
            set { _stats = value; RaiseProperty(); }
        }

        private Solution _currentSolution;

        public ICommand CommandSelectFile
        {
            get
            {
                return new Command(() =>
                {
                    _fileDialogService.OpenFileDialog(
                        (solutionPath) => OpenSolutionPath(solutionPath),
                        (error) => ShowError(error));                        
                });
            }
        }

        private void OpenSolutionPath(string solutionPath)
        {
            IsBusy = true;
            _currentSolution = _measureService.GetProjects(solutionPath);
            IsBusy = false; // just in case we later split this in 2 buttons
            MeasureSolution(_currentSolution);
        }

        private void MeasureSolution(Solution solution)
        {
            IsBusy = true;
            _currentSolution = _measureService.Measure(solution);
            Stats = new CodeStats() {
                ShareCodeInAndroid = Math.Round(((double) _currentSolution.Projects.Where(x => x.Platform == EnumPlatform.Core).SelectMany(x => x.Files).Where(x => x.IsUserInterface == false).Sum(x => x.LOC) 
                    / (_currentSolution.Projects.Where(x => x.Platform == EnumPlatform.Android).SelectMany(x => x.Files).Where(x => x.IsUserInterface == false).Sum(x => x.LOC)
                    + _currentSolution.Projects.Where(x => x.Platform == EnumPlatform.Core).SelectMany(x => x.Files).Where(x => x.IsUserInterface == false).Sum(x => x.LOC))
                    ) *100,2),
                ShareCodeIniOS = Math.Round(((double)_currentSolution.Projects.Where(x => x.Platform == EnumPlatform.Core).SelectMany(x => x.Files).Where(x => x.IsUserInterface == false).Sum(x => x.LOC)
                    / (_currentSolution.Projects.Where(x => x.Platform == EnumPlatform.iOS).SelectMany(x => x.Files).Where(x => x.IsUserInterface == false).Sum(x => x.LOC)
                    + _currentSolution.Projects.Where(x => x.Platform == EnumPlatform.Core).SelectMany(x => x.Files).Where(x => x.IsUserInterface == false).Sum(x => x.LOC))
                    ) * 100, 2),
                ShareUIInAndroid = Math.Round(((double)_currentSolution.Projects.Where(x => x.Platform == EnumPlatform.Core).SelectMany(x => x.Files).Where(x => x.IsUserInterface == true).Sum(x => x.LOC)
                    / (_currentSolution.Projects.Where(x => x.Platform == EnumPlatform.Android).SelectMany(x => x.Files).Where(x => x.IsUserInterface == true).Sum(x => x.LOC)
                    + _currentSolution.Projects.Where(x => x.Platform == EnumPlatform.Core).SelectMany(x => x.Files).Where(x => x.IsUserInterface == true).Sum(x => x.LOC))
                    ) * 100, 2),
                ShareUIIniOS = Math.Round(((double)_currentSolution.Projects.Where(x => x.Platform == EnumPlatform.Core).SelectMany(x => x.Files).Where(x => x.IsUserInterface == true).Sum(x => x.LOC)
                    / (_currentSolution.Projects.Where(x => x.Platform == EnumPlatform.iOS).SelectMany(x => x.Files).Where(x => x.IsUserInterface == true).Sum(x => x.LOC)
                    + _currentSolution.Projects.Where(x => x.Platform == EnumPlatform.Core).SelectMany(x => x.Files).Where(x => x.IsUserInterface == true).Sum(x => x.LOC))
                    ) * 100, 2),

                AmountOfFiles = _currentSolution.Projects.SelectMany(p => p.Files).Count(),
                CodeFiles = _currentSolution.Projects.SelectMany(x => x.Files).Count(x => x.IsUserInterface == false),
                UIFiles = _currentSolution.Projects.SelectMany(x => x.Files).Count(x => x.IsUserInterface == true),
                TotalLinesOfCode = _currentSolution.Projects.SelectMany(x => x.Files).Where(x => x.IsUserInterface == false).Sum(x => x.LOC),
                TotalLinesOfUI = _currentSolution.Projects.SelectMany(x => x.Files).Where(x => x.IsUserInterface == true).Sum(x => x.LOC),
                AndroidFiles = _currentSolution.Projects.Where(x => x.Platform == EnumPlatform.Android).SelectMany(x => x.Files).Count(),
                iOSFiles = _currentSolution.Projects.Where(x => x.Platform == EnumPlatform.iOS).SelectMany(x => x.Files).Count(),
                TotalLinesCore = _currentSolution.Projects.Where(x => x.Platform == EnumPlatform.Core).SelectMany(x => x.Files).Sum(x => x.LOC),
                TotalLinesInAndroid = _currentSolution.Projects.Where(x => x.Platform == EnumPlatform.Android).SelectMany(x => x.Files).Sum(x => x.LOC),
                TotalLinesIniOS = _currentSolution.Projects.Where(x => x.Platform == EnumPlatform.iOS).SelectMany(x => x.Files).Sum(x => x.LOC)
            };
            IsBusy = false;
        }

        private void ShowError(Exception e)
        {
            _fileDialogService.OpenError(e, string.Empty);
        }
    }
}
