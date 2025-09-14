namespace WinformsMVP.MVP.Views
{
    public interface IValidationCommitable
    {
        bool HasChanges();

        bool Validate();

        void Commit();
    }
}
