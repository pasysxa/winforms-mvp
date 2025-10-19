using System;
using System.Windows.Forms;

namespace WinformsMVP.Services
{
    public interface IViewMappingRegister
    {
        Type GetViewImplementationType(Type viewInterfaceType);

        void Register<TViewInterface, TViewImplementation>() where TViewImplementation : Form, TViewInterface;
    }
}
