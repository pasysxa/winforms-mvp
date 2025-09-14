namespace WinformsMVP.MVP.Views
{
    internal interface IValidationCommitable
    {
        bool HasChanges();

        bool Validate();

        void Commit();
    }
}
