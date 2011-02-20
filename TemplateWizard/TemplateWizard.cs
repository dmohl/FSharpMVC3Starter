using System;
using System.Collections.Generic;
using EnvDTE;
using Microsoft.VisualStudio.TemplateWizard;
using VSLangProj;
using System.Diagnostics;

namespace TemplateWizard
{
    public class TemplateWizard : IWizard
    {
        protected DTE Dte { get; set; }
        public void RunStarted(object automationObject, Dictionary<string, string> replacementsDictionary, WizardRunKind runKind, object[] customParams)
        {
            Dte = (DTE) automationObject;
        }

        public void ProjectFinishedGenerating(Project project)
        {
            try
            {
                var projects = new Dictionary<string, Project>();
                for (int i = 1; i <= Dte.Solution.Projects.Count; i++)
                {
                    projects.Add(Dte.Solution.Projects.Item(i).Name, Dte.Solution.Projects.Item(i));
                }

                AddProjectReference(projects["Controllers"], projects["Models"]);
                AddProjectReference(projects["Web"], projects["Core"]);
                AddProjectReference(projects["Web"], projects["Models"]);
                AddProjectReference(projects["Web"], projects["Controllers"]);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(string.Format("Error: {0}\r\nStack Trace: {1}", ex.Message, ex.StackTrace));
            }
        }

        private void AddProjectReference(Project targetProject, Project projectToAddAsAReference)
        {
            if (targetProject != null && projectToAddAsAReference != null)
            {
                var vsControllerProject = ((VSProject)targetProject.Object);
                for (int i = 1; i <= vsControllerProject.References.Count; i++)
                {
                    var reference = vsControllerProject.References.Item(i);
                    if (reference.Name == projectToAddAsAReference.Name)
                        reference.Remove();
                }
                vsControllerProject.References.AddProject(projectToAddAsAReference);
            }
        }

        public void ProjectItemFinishedGenerating(ProjectItem projectItem)
        {
        }

        public bool ShouldAddProjectItem(string filePath)
        {
            return true;
        }

        public void BeforeOpeningFile(ProjectItem projectItem)
        {
        }

        public void RunFinished()
        {
        }
    }
}
