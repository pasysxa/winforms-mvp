using System;
using System.Windows.Forms;
using WinformsMVP.Core.Presenters;
using WinformsMVP.Core.Views;
using WinformsMVP.Services.Implementations;

namespace MinformsMVP.Samples
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            var todoPresenter = new ToDoPresenter();
            todoPresenter.AttachView(null as IToDoView);

            var navigator = new WindowNavigator(null);
            navigator.ShowWindowAsModal(todoPresenter);

        }
    }

    public interface IToDoView : IWindowView
    {

    }

    public class ToDoPresenter : PresenterBase<IToDoView>
    {
        protected override void OnViewAttached()
        {
        }
    }
}
