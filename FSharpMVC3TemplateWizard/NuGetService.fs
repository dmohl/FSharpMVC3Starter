[<AutoOpen>]
module NuGetService
    open System
    open System.IO
    open EnvDTE
    open Microsoft.Win32
    open Microsoft.VisualStudio.ComponentModelHost
    open NuGet.VisualStudio

    let GetNuGetPackageLocalPath() = 
        let name = Path.Combine("Software\Microsoft", "ASP.NET MVC 3", "Runtime")
        use key = Registry.LocalMachine.OpenSubKey name
        match (key = null) with
        | false -> 
            let installPath = key.GetValue("InstallPath").ToString()
            match String.IsNullOrEmpty(installPath) with
            | false -> Path.Combine(installPath, "Packages")
            | true -> null
        | _ -> null 
    
    let InstallPackages (serviceProvider:IServiceProvider) (project:Project) packages =
        let componentModel = 
            serviceProvider.GetService(typeof<SComponentModel>) :?> IComponentModel
        let installer = componentModel.GetService<IVsPackageInstaller>()
        let nugetPackageLocalPath = GetNuGetPackageLocalPath()
        packages 
        |> Seq.iter (fun packageId -> 
                         installer.InstallPackage(nugetPackageLocalPath, 
                             project, packageId, null, false))  

