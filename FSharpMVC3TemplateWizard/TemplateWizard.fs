namespace FsFullExMVC3TemplateWizard

open System
open System.IO
open System.Windows.Forms
open System.Collections.Generic
open EnvDTE
open EnvDTE80
open Microsoft.VisualStudio.Shell
open Microsoft.VisualStudio.TemplateWizard
open VSLangProj
open CsFullMvc3Dialog

type TemplateWizard() =
    [<DefaultValue>] val mutable solution : Solution2
    [<DefaultValue>] val mutable dte : DTE
    [<DefaultValue>] val mutable dte2 : DTE2
    [<DefaultValue>] val mutable serviceProvider : IServiceProvider
    [<DefaultValue>] val mutable destinationPath : string
    [<DefaultValue>] val mutable safeProjectName : string
    let mutable selectedWebProjectName = "Razor"
    interface IWizard with
        member this.RunStarted (automationObject:Object, 
                                replacementsDictionary:Dictionary<string,string>, 
                                runKind:WizardRunKind, customParams:Object[]) =
            this.dte <- automationObject :?> DTE
            this.dte2 <- automationObject :?> DTE2
            this.solution <- this.dte2.Solution :?> Solution2
            this.serviceProvider <- new ServiceProvider(automationObject :?> 
                                     Microsoft.VisualStudio.OLE.Interop.IServiceProvider)
            this.destinationPath <- replacementsDictionary.["$destinationdirectory$"]
            this.safeProjectName <- replacementsDictionary.["$safeprojectname$"]

            let dialog = new TemplateWizardDialog()
            match dialog.ShowDialog().Value with
            | true -> 
                selectedWebProjectName <- match dialog.SelectedViewEngine with
                                          | "ASPX" -> "Web"
                                          | _ -> dialog.SelectedViewEngine
            | _ ->
                raise (new WizardCancelledException())
        member this.ProjectFinishedGenerating project = "Not Implemented" |> ignore
        member this.ProjectItemFinishedGenerating projectItem = "Not Implemented" |> ignore
        member this.ShouldAddProjectItem filePath = true
        member this.BeforeOpeningFile projectItem = "Not Implemented" |> ignore
        member this.RunFinished() = 
            let currentCursor = Cursor.Current
            Cursor.Current <- Cursors.WaitCursor
            try
                let webName = this.safeProjectName + "Web"
                let webAppName = this.safeProjectName + "App"
                let templatePath = this.solution.GetProjectTemplate("FSMVC3.zip", "FSharp")
                try
                    let AddProject status projectVsTemplateName projectName =
                        this.dte2.StatusBar.Text <- status
                        let path = templatePath.Replace("FSMVC3.vstemplate", projectVsTemplateName)
                        this.solution.AddFromTemplate(path, Path.Combine(this.destinationPath, projectName), 
                            projectName, false) |> ignore
                    AddProject "Installing the C# Web project..." 
                        (Path.Combine(selectedWebProjectName, selectedWebProjectName+ ".vstemplate")) webName
                    AddProject "Adding the F# Web App project..." 
                        (Path.Combine("App", "App.vstemplate")) webAppName

                    let projects = BuildProjectMap (this.dte.Solution.Projects)

                    this.dte2.StatusBar.Text <- "Adding NuGet packages..."
                    (projects.TryFind webName).Value |> InstallPackages this.serviceProvider 
                    <| [("jQuery.vsdoc", "1.5.1"); ("jQuery.Validation", "1.8.0"); ("jQuery.UI.Combined", "1.8.11"); ("Modernizr", "1.7"); ("EntityFramework", "4.1.10331.0")]

                    this.dte2.StatusBar.Text <- "Updating project references..."
                    [(webName, webAppName)]
                    |> BuildProjectReferences projects 
                with
                | ex -> failwith (sprintf "%s\n\r%s\n\r%s\n\r%s\n\r%s" 
                            "The project creation has failed."
                            "Ensure that you have installed the ASP.NET MVC 3 Tools Refresh." 
                            "See http://bit.ly/mvc3-tools-refresh for more information."
                            "The actual exception message is: "
                            ex.Message)
            finally
                Cursor.Current <- currentCursor
            
