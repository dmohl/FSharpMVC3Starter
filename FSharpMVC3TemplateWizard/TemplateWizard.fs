namespace FSharpMVC3TemplateWizard

open System
open System.Collections.Generic
open EnvDTE
open Microsoft.VisualStudio.TemplateWizard
open VSLangProj

type TemplateWizard() =
    [<DefaultValue>] val mutable Dte : DTE
    let AddProjectReference (targetProject:Option<Project>) (projectToAddAsAReference:Option<Project>) =
        if ((Option.isSome targetProject) && (Option.isSome projectToAddAsAReference)) then
            let vsControllerProject = targetProject.Value.Object :?> VSProject
            let enumerator = vsControllerProject.References.GetEnumerator() 
            enumerator.Reset()
            let rec buildProjectReferences() = 
                match enumerator.MoveNext() with
                | true -> 
                    let reference = enumerator.Current :?> Reference
                    if reference.Name = projectToAddAsAReference.Value.Name then reference.Remove()
                    vsControllerProject.References.AddProject(projectToAddAsAReference.Value) |> ignore
                    buildProjectReferences()
                | _ -> "End it" |> ignore
            buildProjectReferences()
    interface IWizard with
        member x.RunStarted (automationObject:Object, replacementsDictionary:Dictionary<string,string>, runKind:WizardRunKind, customParams:Object[]) =
            x.Dte <- automationObject :?> DTE
        member x.ProjectFinishedGenerating (project:Project) =
            try
                let enumerator = x.Dte.Solution.Projects.GetEnumerator() 
                let rec buildProjects (projectMap:Map<string,Project>) = 
                    match enumerator.MoveNext() with
                    | true -> let project = enumerator.Current :?> Project
                              projectMap |> Map.add project.Name project
                              |> buildProjects 
                    | _ -> projectMap
                let projects = buildProjects Map.empty
                do AddProjectReference (projects.TryFind("Controllers")) (projects.TryFind("Models"))
                do AddProjectReference (projects.TryFind("Web")) (projects.TryFind("Core"))
                do AddProjectReference (projects.TryFind("Web")) (projects.TryFind("Models"))
                do AddProjectReference (projects.TryFind("Web")) (projects.TryFind("Controllers"))
            with 
            | _ -> "Do Nothing" |> ignore
        member x.ProjectItemFinishedGenerating projectItem = "Do Nothing" |> ignore
        member x.ShouldAddProjectItem filePath = true
        member x.BeforeOpeningFile projectItem = "Do Nothing" |> ignore
        member x.RunFinished() = "Do Nothing" |> ignore
