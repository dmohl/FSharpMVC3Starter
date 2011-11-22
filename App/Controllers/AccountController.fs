namespace FSharpMVC3.Web.Controllers

open System
open System.Web
open System.Web.Mvc
open System.Web.Security
open FSharpMVC3.Web.Models
open System.Security.Principal

module AccountControllerModule =
    let AssignIfNull value newValue =
        match value with
        | _ when value = null -> newValue
        | _ -> value

[<HandleError>]
type AccountController() =
    inherit Controller()
    let mutable formsService = new FormsAuthenticationService() :> IFormsAuthenticationService
    let mutable membershipService = new AccountMembershipService() :> IMembershipService

    // **************************************
    // URL: /Account/LogOn
    // **************************************
    member x.LogOn () =
          x.View() :> ActionResult
    [<HttpPost>]
    member x.LogOn (model:LogOnModel) returnUrl =
        match x.ModelState.IsValid with
        | true ->
            match membershipService.ValidateUser(model.UserName, model.Password) with
            | true ->
                formsService.SignIn(model.UserName, model.RememberMe)
                match String.IsNullOrEmpty(returnUrl) with
                | false -> x.Redirect(returnUrl) :> ActionResult
                | _ -> x.RedirectToAction("Index", "Home") :> ActionResult
            | _-> x.ModelState.AddModelError("", 
                    "The user name or password provided is incorrect.")
                  x.View(model) :> ActionResult
        | _ -> x.View(model) :> ActionResult
    // **************************************
    // URL: /Account/LogOff
    // **************************************
    member x.LogOff () =
        formsService.SignOut()
        x.RedirectToAction("Index", "Home") :> ActionResult
    // **************************************
    // URL: /Account/Register
    // **************************************
    member x.Register () =
        x.ViewData.["PasswordLength"] <- membershipService.MinPasswordLength
        x.View() :> ActionResult
    [<HttpPost>]
    member x.Register(model:RegisterModel) =
        match x.ModelState.IsValid with
        | true ->
            let createStatus = membershipService.CreateUser(model.UserName, model.Password, model.Email)
            match createStatus with
            | MembershipCreateStatus.Success ->
                formsService.SignIn(model.UserName, false)
                x.RedirectToAction("Index", "Home") :> ActionResult
            | _ ->
                x.ModelState.AddModelError("", AccountValidation.ErrorCodeToString(createStatus))
                x.View(model) :> ActionResult
          | _ ->
              x.ViewData.["PasswordLength"] <- membershipService.MinPasswordLength
              x.View(model) :> ActionResult
    // **************************************
    // URL: /Account/ChangePassword
    // **************************************
    [<Authorize>]
    member x.ChangePassword () =
        x.ViewData.["PasswordLength"] <- membershipService.MinPasswordLength
        x.View() :> ActionResult
    [<Authorize>]
    [<HttpPost>]
    member x.ChangePassword(model:ChangePasswordModel) =
        match x.ModelState.IsValid with
        | true ->
            let changePasswordResult = 
                membershipService.ChangePassword(x.User.Identity.Name, model.OldPassword, model.NewPassword)
            match changePasswordResult with
            | true -> x.RedirectToAction("ChangePasswordSuccess") :> ActionResult
            | _ ->  
                x.ModelState.AddModelError("", 
                    "The current password is incorrect or the new password is invalid.")
                x.View(model) :> ActionResult
        | _ -> 
            x.ViewData.["PasswordLength"] <- membershipService.MinPasswordLength
            x.View(model) :> ActionResult
    // **************************************
    // URL: /Account/ChangePasswordSuccess
    // **************************************
    member x.ChangePasswordSuccess () = x.View()
