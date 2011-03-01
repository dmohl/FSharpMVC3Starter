namespace FSharpMVC3TemplateWizard

open System
open System.Collections.Generic
open System.Collections
open EnvDTE
open Microsoft.VisualStudio.TemplateWizard
open VSLangProj

[<AutoOpen>]
module TemplateWizardMod =
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

    let BuildProjectMap (projectEnumerator:IEnumerator) =
        let rec buildProjects (projectMap:Map<string,Project>) = 
            match projectEnumerator.MoveNext() with
            | true -> let project = projectEnumerator.Current :?> Project
                      projectMap 
                      |> Map.add project.Name project
                      |> buildProjects 
            | _ -> projectMap
        buildProjects Map.empty

type TemplateWizard() =
    let projectRefs = [("Controllers", "Models"); ("Web", "Core"); ("Web", "Models"); ("Web", "Controllers")]
    [<DefaultValue>] val mutable Dte : DTE
    interface IWizard with
        member x.RunStarted (automationObject:Object, replacementsDictionary:Dictionary<string,string>, runKind:WizardRunKind, customParams:Object[]) =
            x.Dte <- automationObject :?> DTE
        member x.ProjectFinishedGenerating (project:Project) =
            try
                let projects = BuildProjectMap (x.Dte.Solution.Projects.GetEnumerator())
                projectRefs |> Seq.iter (fun (target,source) -> do AddProjectReference (projects.TryFind(target)) (projects.TryFind(source)))
            with 
            | _ -> "Do Nothing" |> ignore
        member x.ProjectItemFinishedGenerating projectItem = "Do Nothing" |> ignore
        member x.ShouldAddProjectItem filePath = true
        member x.BeforeOpeningFile projectItem = "Do Nothing" |> ignore
        member x.RunFinished() = "Do Nothing" |> ignore
