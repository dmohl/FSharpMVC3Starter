namespace FSharpMVC3.Web.Models

open System
open System.Web.Security

module InputValidation =
    let Validate stringValue errorMessage =
        match String.IsNullOrEmpty(stringValue) with
        | true -> failwith(errorMessage)
        | _ -> "" |> ignore

type IMembershipService = 
    abstract MinPasswordLength : int 
    abstract ValidateUser : userName:string * password:string -> bool
    abstract CreateUser : userName:string * password:string*email:string -> MembershipCreateStatus
    abstract ChangePassword : userName:string * oldPassword:string * newPassword:string -> bool

type AccountMembershipService(provider : MembershipProvider) =
    new () = AccountMembershipService(Membership.Provider)
    interface IMembershipService with 
        member x.MinPasswordLength = provider.MinRequiredPasswordLength
        member x.ValidateUser (userName, password) =
            InputValidation.Validate userName "Username cannot be null or empty."
            InputValidation.Validate password "Password cannot be null or empty."
            provider.ValidateUser(userName, password)
        member x.CreateUser (userName, password, email) =
            InputValidation.Validate userName "Username cannot be null or empty."
            InputValidation.Validate password "Password cannot be null or empty."
            InputValidation.Validate email "Email cannot be null or empty."
            let (_, status) = provider.CreateUser(userName, password, email, null, null, true, null)
            status
        member x.ChangePassword(userName, oldPassword, newPassword) =
            InputValidation.Validate userName "Username cannot be null or empty."
            InputValidation.Validate oldPassword "OldPassword cannot be null or empty."
            InputValidation.Validate newPassword "NewPassword cannot be null or empty."            
            try
                let currentUser = provider.GetUser(userName, true)
                currentUser.ChangePassword(oldPassword, newPassword)
            with 
            | :? ArgumentException -> false
            | :? MembershipPasswordException -> false

type IFormsAuthenticationService =
    abstract SignIn : userName:string * createPersistentCookie:bool -> unit
    abstract SignOut : unit -> unit

type FormsAuthenticationService() =
    interface IFormsAuthenticationService with
        member x.SignIn(userName, createPersistentCookie) =
            InputValidation.Validate userName "Username cannot be null or empty"
            FormsAuthentication.SetAuthCookie(userName, createPersistentCookie)
        member x.SignOut () =
            FormsAuthentication.SignOut()

module AccountValidation =
    let ErrorCodeToString createStatus =
        match createStatus with
        | MembershipCreateStatus.DuplicateUserName -> "Username already exists. Please enter a different user name."
        | MembershipCreateStatus.DuplicateEmail -> "A username for that e-mail address already exists. Please enter a different e-mail address."
        | MembershipCreateStatus.InvalidPassword -> "The password provided is invalid. Please enter a valid password value."
        | MembershipCreateStatus.InvalidEmail -> "The e-mail address provided is invalid. Please check the value and try again."
        | MembershipCreateStatus.InvalidAnswer -> "The password retrieval answer provided is invalid. Please check the value and try again."
        | MembershipCreateStatus.InvalidQuestion -> "The password retrieval question provided is invalid. Please check the value and try again."
        | MembershipCreateStatus.InvalidUserName -> "The user name provided is invalid. Please check the value and try again."
        | MembershipCreateStatus.ProviderError -> 
            "The authentication provider returned an error. Please verify your entry and try again. If the problem persists, please contact your system administrator."
        | MembershipCreateStatus.UserRejected -> 
            "The user creation request has been canceled. Please verify your entry and try again. If the problem persists, please contact your system administrator."
        | _ -> "An unknown error occurred. Please verify your entry and try again. If the problem persists, please contact your system administrator."
