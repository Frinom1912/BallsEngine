using Editor.Common;
using Editor.Utilities;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Editor.GameProject
{
    [DataContract]
    public class ProjectTemplate
    {
        [DataMember]
        public string ProjectType { get; set; }
        
        [DataMember]
        public string ProjectFile { get; set; }
        
        [DataMember]
        public List<string> Folders { get; set; }

        public byte[] Icon { get; set; }
        public byte[] Screen { get; set; }
        public string IconFilePath { get; set; }
        public string ScreenFilePath { get; set; }
        public string ProjectFilePath { get; set; }

    }

    class NewProject : ViewModelBase
    {
        // TODO: change
        private readonly string _templatePath = $@"..\..\Editor\ProjectTemplates";

        private string _name = "NewProject";
        public string Name
        {
            get => _name;
            set
            {
                if (_name != value)
                {
                    _name = value;
                    ValidateProjectPath();
                    OnPropertyChanged(nameof(Name));
                }
            }
        }

        private string _path = $@"{Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)}\MegaProjects\";
        public string ProjectPath
        {
            get => _path;
            set
            {
                if (_path != value)
                {
                    _path = value;
                    ValidateProjectPath();
                    OnPropertyChanged(nameof(ProjectPath));
                }
            }
        }

        private bool _isValid;

        public bool IsValid {
            get => _isValid;
            set {
                if(_isValid != value)
                {
                    _isValid = value;
                    OnPropertyChanged(nameof(IsValid));
                }
            }
        }

        private string __errorMessage;

        public string ErrorMessage
        {
            get => __errorMessage;
            set
            {
                if (__errorMessage != value)
                {
                    __errorMessage = value;
                    OnPropertyChanged(nameof(ErrorMessage));
                }
            }
        }

        private ObservableCollection<ProjectTemplate> _projectTemplates = new ObservableCollection<ProjectTemplate>();

        public ReadOnlyObservableCollection<ProjectTemplate> ProjectTemplates
        {
            get;
        }

        private bool ValidateProjectPath()
        {
            var path = ProjectPath;
            path = StringUtils.EnsureDirectorySeparator(path);

            path += $@"{Name}\";

            IsValid = false;

            if (string.IsNullOrWhiteSpace(Name.Trim()))
            {
                ErrorMessage = "Type a project name";
            }
            else if (Name.IndexOfAny(Path.GetInvalidFileNameChars()) != -1)
            {
                ErrorMessage = "Invalid characters in name";
            }
            else if (string.IsNullOrWhiteSpace(ProjectPath.Trim()))
            {
                ErrorMessage = "Invalid project folder";
            }
            else if (ProjectPath.IndexOfAny(Path.GetInvalidPathChars()) != -1)
            {
                ErrorMessage = "Invalid characters in path";
            }
            else if (Directory.Exists(path) && Directory.EnumerateFileSystemEntries(path).Any())
            {
                ErrorMessage = "Path is not empty";
            }
            else
            {
                IsValid = true;
                ErrorMessage = string.Empty;
            }
            return IsValid;
        }

        public string CreateProject(ProjectTemplate template)
        {
            ValidateProjectPath();

            if (!IsValid)
            {
                return "";
            }

            var path = StringUtils.EnsureDirectorySeparator(ProjectPath) + $@"{Name}\";

            try
            {
                if (!Directory.Exists(path)) Directory.CreateDirectory(path);

                foreach(var folder in template.Folders)
                {
                    Directory.CreateDirectory(Path.GetFullPath(Path.Combine(Path.GetDirectoryName(path), folder)));
                }
                var dirInfo = new DirectoryInfo(path + @".Mega\");
                dirInfo.Attributes |= FileAttributes.Hidden;
                
                File.Copy(template.IconFilePath, Path.GetFullPath(Path.Combine(dirInfo.FullName, "Icon.png")));
                File.Copy(template.ScreenFilePath, Path.GetFullPath(Path.Combine(dirInfo.FullName, "Screen.png")));

                return path;
                
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                return string.Empty;
            }
        }

        public NewProject()
        {
            ProjectTemplates = new ReadOnlyObservableCollection<ProjectTemplate>(_projectTemplates);

            try
            {
                var templateFiles = Directory.GetFiles(_templatePath, "template.xml", SearchOption.AllDirectories);

                Debug.Assert(templateFiles.Any());

                foreach(var file in templateFiles)
                {
                    var template = Serializer.FromFile<ProjectTemplate>(file);
                    template.IconFilePath = Path.GetFullPath(Path.Combine(Path.GetDirectoryName(file), "Icon.png"));
                    template.Icon = File.ReadAllBytes(template.IconFilePath);
                    template.ScreenFilePath = Path.GetFullPath(Path.Combine(Path.GetDirectoryName(file), "Screen.png"));
                    template.Screen = File.ReadAllBytes(template.ScreenFilePath);
                    template.ProjectFilePath = Path.GetFullPath(Path.Combine(Path.GetDirectoryName(file), template.ProjectFile));

                    _projectTemplates.Add(template);
                }
                ValidateProjectPath();

            }
            catch (Exception ex)
            {
                // TODO: logging
                Debug.WriteLine(ex.Message);
            }
        }
    }
}
