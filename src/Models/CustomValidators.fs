namespace FSharpMVC3.Web.Models
open System
open System.ComponentModel.DataAnnotations
open System.ComponentModel
open System.Globalization
open System.Web.Security

[<AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = true)>]
[<Sealed>] 
type PropertiesMustMatchAttribute(originalProperty : string, confirmProperty : string) = 
    inherit ValidationAttribute("'{0}' and '{1}' do not match.")
    let typeId = new Object()
    let mutable originalProperty = originalProperty
    let mutable confirmProperty = confirmProperty
    member x.OriginalProperty with get() = originalProperty and set(value) = originalProperty <- value
    member x.ConfirmProperty with get() = confirmProperty and set(value) = confirmProperty <- value
    override x.TypeId = typeId
    override x.FormatErrorMessage name =
        String.Format(CultureInfo.CurrentUICulture, x.ErrorMessageString, x.OriginalProperty, x.ConfirmProperty)
    override x.IsValid value =
        let properties = TypeDescriptor.GetProperties value
        let originalValue = properties.Find(x.OriginalProperty, true).GetValue(value)
        let confirmValue = properties.Find(x.ConfirmProperty, true).GetValue(value)
        Object.Equals(originalValue, confirmValue)

[<AttributeUsage(AttributeTargets.Field ||| AttributeTargets.Property, AllowMultiple = false, Inherited = true)>]
[<Sealed>] 
type ValidatePasswordLengthAttribute() = 
    inherit ValidationAttribute("'{0}' must be at least {1} characters long.")
    let minCharacters = Membership.Provider.MinRequiredPasswordLength
    override x.FormatErrorMessage name =
        String.Format(CultureInfo.CurrentUICulture, x.ErrorMessageString, name, minCharacters)
    override x.IsValid value =
        let valueAsString = value :?> string
        (valueAsString <> null && valueAsString.Length >= minCharacters)
