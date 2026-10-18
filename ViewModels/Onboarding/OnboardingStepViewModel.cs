using CommunityToolkit.Mvvm.ComponentModel;

namespace Mercury.Desktop.ViewModels.Onboarding;

/// <summary>
/// Base class for one step of onboarding. Steps validate with data annotations
/// (<c>[Required]</c>, <c>[EmailAddress]</c> and so on); WPF shows the messages next to the
/// fields because ObservableValidator implements INotifyDataErrorInfo.
/// </summary>
public abstract class OnboardingStepViewModel : ObservableValidator
{
    /// <summary>Text on the button that moves to the next step.</summary>
    public virtual string ContinueLabel => "Continue";

    /// <summary>Whether the dark stepper sidebar is shown beside this step.</summary>
    public virtual bool ShowsSidebar => true;

    /// <summary>
    /// Checks every field, which also reveals messages on fields the user has not touched.
    /// Returns true when the step can be left.
    /// </summary>
    public virtual bool TryComplete()
    {
        ValidateAllProperties();
        return !HasErrors;
    }
}
